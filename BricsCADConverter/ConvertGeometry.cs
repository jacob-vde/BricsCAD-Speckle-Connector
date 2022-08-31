using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using Bricscad.ApplicationServices;

using _OdDb = Teigha.DatabaseServices;
using _OdGe = Teigha.Geometry;
using _OdCm = Teigha.Colors;
using _OdRx = Teigha.Runtime;

using Arc = Objects.Geometry.Arc;
using Box = Objects.Geometry.Box;
using Brep = Objects.Geometry.Brep;
using BrepEdge = Objects.Geometry.BrepEdge;
using BrepFace = Objects.Geometry.BrepFace;
using BrepLoop = Objects.Geometry.BrepLoop;
using BrepLoopType = Objects.Geometry.BrepLoopType;
using BrepTrim = Objects.Geometry.BrepTrim;
using Circle = Objects.Geometry.Circle;
using ControlPoint = Objects.Geometry.ControlPoint;
using Curve = Objects.Geometry.Curve;
using Ellipse = Objects.Geometry.Ellipse;
using Interval = Objects.Primitive.Interval;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using Plane = Objects.Geometry.Plane;
using Point = Objects.Geometry.Point;
using Polycurve = Objects.Geometry.Polycurve;
using Polyline = Objects.Geometry.Polyline;
using Spiral = Objects.Geometry.Spiral;
using Surface = Objects.Geometry.Surface;
using Vector = Objects.Geometry.Vector;

using Objects.Utils;

namespace BricsCADConverter
{
    class ConvertGeometry
    {
        public _OdDb.DBPoint PointToNativeDB(Point point)
        {
            return new _OdDb.DBPoint(PointToNative(point));
        }

        public _OdDb.Line LineToNativeDB(Line line)
        {
            return new _OdDb.Line(PointToNative(line.start), PointToNative(line.end));
        }

        public _OdDb.Arc ArcToNativeDB(Arc arc)
        {
            // because of different plane & start/end angle conventions, most reliable method to convert to autocad convention is to calculate from start, end, and midpoint
            var circularArc = ArcToNative(arc);

            // calculate adjusted start and end angles from circularArc reference
            double angle = circularArc.ReferenceVector.AngleOnPlane(PlaneToNative(arc.plane));
            double startAngle = circularArc.StartAngle + angle;
            double endAngle = circularArc.EndAngle + angle;

            var _arc = new _OdDb.Arc(
                  PointToNative(arc.plane.origin),
                  VectorToNative(arc.plane.normal),
                  (double)arc.radius,
                  startAngle,
                  endAngle);

            return _arc;
        }

        public _OdDb.Circle CircleToNativeDB(Circle circle)
        {
            var normal = VectorToNative(circle.plane.normal);
            return new _OdDb.Circle(PointToNative(circle.plane.origin), normal, (double) circle.radius);
        }

        public _OdDb.Ellipse EllipseToNativeDB(Ellipse ellipse)
        {
            var normal = VectorToNative(ellipse.plane.normal);
            var xAxisVector = VectorToNative(ellipse.plane.xdir);
            var majorAxis = (double)ellipse.firstRadius * xAxisVector.GetNormal();
            var radiusRatio = (double)ellipse.secondRadius / (double)ellipse.firstRadius;
            return new _OdDb.Ellipse(PointToNative(ellipse.plane.origin), normal, majorAxis, radiusRatio, 0, 2 * Math.PI);
        }

        public _OdDb.Polyline3d PolylineToNativeDB(Polyline polyline)
        {
            var vertices = new _OdGe.Point3dCollection();
            for (int i = 0; i < polyline.points.Count; i++)
                vertices.Add(PointToNative(polyline.points[i]));
            return new _OdDb.Polyline3d(_OdDb.Poly3dType.SimplePoly, vertices, polyline.closed);
        }

        public _OdDb.Curve CurveToNativeDB(Objects.ICurve icurve)
        {
            switch (icurve)
            {
                case Line line:
                    return LineToNativeDB(line);

                case Polyline polyline:
                    return PolylineToNativeDB(polyline);

                case Arc arc:
                    return ArcToNativeDB(arc);

                case Circle circle:
                    return CircleToNativeDB(circle);

                case Ellipse ellipse:
                    return EllipseToNativeDB(ellipse);

/*                case Polycurve polycurve:
                    if (polycurve.segments.Where(o => o is Curve).Count() > 0)
                        return PolycurveSplineToNativeDB(polycurve);
                    return PolycurveToNativeDB(polycurve);*/

                case Curve curve:
                    return NurbsToNativeDB(curve);

                default:
                    return null;
            }
        }
        public _OdDb.Curve NurbsToNativeDB(Curve curve)
        {
            var _curve = _OdDb.Curve.CreateFromGeCurve(NurbcurveToNative(curve));
            return _curve;
        }

        public _OdDb.Polyline PolycurveToNativeDB(Polycurve polycurve)
        {
            _OdDb.Polyline polyline = new _OdDb.Polyline() { Closed = polycurve.closed };
            var plane = new _OdGe.Plane(_OdGe.Point3d.Origin, _OdGe.Vector3d.ZAxis.TransformBy(Application.DocumentManager.MdiActiveDocument.Editor.CurrentUserCoordinateSystem)); // TODO: check this 

            // add all vertices
            int count = 0;
            foreach (var segment in polycurve.segments)
            {
                switch (segment)
                {
                    case Line o:
                        polyline.AddVertexAt(count, PointToNative(o.start).Convert2d(plane), 0, 0, 0);
                        if (!polycurve.closed && count == polycurve.segments.Count - 1)
                            polyline.AddVertexAt(count + 1, PointToNative(o.end).Convert2d(plane), 0, 0, 0);
                        count++;
                        break;
                    case Arc o:
                        var angle = o.endAngle - o.startAngle;
                        angle = angle < 0 ? angle + 2 * Math.PI : angle;
                        var bulge = Math.Tan((double)angle / 4) * BulgeDirection(o.startPoint, o.midPoint, o.endPoint); // bulge
                        polyline.AddVertexAt(count, PointToNative(o.startPoint).Convert2d(plane), bulge, 0, 0);
                        if (!polycurve.closed && count == polycurve.segments.Count - 1)
                            polyline.AddVertexAt(count + 1, PointToNative(o.endPoint).Convert2d(plane), 0, 0, 0);
                        count++;
                        break;
                    case Spiral o:
                        var vertices = o.displayValue.GetPoints().Select(p => PointToNative(p)).ToList();
                        foreach (var vertex in vertices)
                        {
                            polyline.AddVertexAt(count, vertex.Convert2d(plane), 0, 0, 0);
                            count++;
                        }
                        break;
                    default:
                        return null;
                }
            }

            return polyline;
        }

        public _OdDb.SubDMesh MeshToNativeDB(Mesh mesh)
        {
            mesh.TriangulateMesh(true);

            // get vertex points
            var vertices = new _OdGe.Point3dCollection();
            var points = mesh.GetPoints().Select(o => PointToNative(o)).ToList();
            foreach (var point in points)
                vertices.Add(point);

            var faceArray = new _OdGe.Int32Collection(mesh.faces.ToArray());

            _OdDb.SubDMesh _mesh = new _OdDb.SubDMesh();
            _mesh.SetSubDMesh(vertices, faceArray, 0);
            return _mesh;
        }



        public _OdGe.Point3d PointToNative(Point point)
        {
            var _point = new _OdGe.Point3d(point.x, point.y, point.z);
            return _point;
        }

        public _OdGe.CircularArc3d ArcToNative(Arc arc)
        {
            var _arc = new _OdGe.CircularArc3d(PointToNative(arc.startPoint), PointToNative(arc.midPoint), PointToNative(arc.endPoint));

            _arc.SetAxes(VectorToNative(arc.plane.normal), VectorToNative(arc.plane.xdir));
            _arc.SetAngles((double)arc.startAngle, (double)arc.endAngle);

            return _arc;
        }

        public _OdGe.Vector3d VectorToNative(Vector vector)
        {
            return new _OdGe.Vector3d(vector.x, vector.y, vector.z);
        }

        public _OdGe.Plane PlaneToNative(Plane plane)
        {
            return new _OdGe.Plane(PointToNative(plane.origin), VectorToNative(plane.normal));
        }

        public _OdGe.NurbCurve3d NurbcurveToNative(Curve curve)
        {
            // process control points
            // NOTE: for **closed periodic** curves that have "n" control pts, curves sent from rhino will have n+degree points. Remove extra pts for autocad.
            var _points = curve.GetPoints().Select(o => PointToNative(o)).ToList();
            if (curve.closed && curve.periodic)
                _points = _points.GetRange(0, _points.Count - curve.degree);
            var points = new _OdGe.Point3dCollection(_points.ToArray());

            // process knots
            // NOTE: Autocad defines spline knots  as a vector of size # control points + degree + 1. (# at start and end should match degree)
            // Conversions for autocad need to make sure this is satisfied, otherwise will cause protected mem crash.
            // NOTE: for **closed periodic** curves that have "n" control pts, # of knots should be n + 1. Remove degree = 3 knots from start and end.
            var _knots = curve.knots;
            if (curve.knots.Count == _points.Count + curve.degree - 1) // handles rhino format curves
            {
                _knots.Insert(0, _knots[0]);
                _knots.Insert(_knots.Count - 1, _knots[_knots.Count - 1]);
            }
            if (curve.closed && curve.periodic) // handles closed periodic curves
                _knots = _knots.GetRange(curve.degree, _knots.Count - curve.degree * 2);
            var knots = new _OdGe.KnotCollection();
            foreach (var _knot in _knots)
                knots.Add(_knot);

            // process weights
            // NOTE: if all weights are the same, autocad convention is to pass an empty list (this will assign them a value of -1)
            var _weights = curve.weights;
            if (curve.closed && curve.periodic) // handles closed periodic curves
                _weights = curve.weights.GetRange(0, _points.Count);
            _OdGe.DoubleCollection weights;
            weights = (_weights.Distinct().Count() == 1) ? new _OdGe.DoubleCollection() : new _OdGe.DoubleCollection(_weights.ToArray());

            _OdGe.NurbCurve3d _curve = new _OdGe.NurbCurve3d(curve.degree, knots, points, weights, curve.periodic);
            if (curve.closed)
                _curve.MakeClosed();
            _curve.SetInterval(IntervalToNative(curve.domain));

            return _curve;
        }

        public _OdGe.Interval IntervalToNative(Interval interval)
        {
            return new _OdGe.Interval((double)interval.start, (double)interval.end, 0.000001);
        }

        public bool IsPolycurvePlanar(Polycurve polycurve)
        {
            double? z = null;
            foreach (var segment in polycurve.segments)
            {
                switch (segment)
                {
                    case Line o:
                        if (z == null) z = o.start.z;
                        if (o.start.z != z || o.end.z != z) return false;
                        break;
                    case Arc o:
                        if (z == null) z = o.startPoint.z;
                        if (o.startPoint.z != z || o.midPoint.z != z || o.endPoint.z != z) return false;
                        break;
                    case Curve o:
                        if (z == null) z = o.points[2];
                        for (int i = 2; i < o.points.Count; i += 3)
                            if (o.points[i] != z) return false;
                        break;
                    case Spiral o:
                        if (z == null) z = o.startPoint.z;
                        if (o.startPoint.z != z || o.endPoint.z != z) return false;
                        break;
                }
            }
            return true;
        }

        // calculates bulge direction: (-) clockwise, (+) counterclockwise
        private int BulgeDirection(Point start, Point mid, Point end)
        {
            // get vectors from points
            double[] v1 = new double[] { end.x - start.x, end.y - start.y, end.z - start.z }; // vector from start to end point
            double[] v2 = new double[] { mid.x - start.x, mid.y - start.y, mid.z - start.z }; // vector from start to mid point

            // calculate cross product z direction
            double z = v1[0] * v2[1] - v2[0] * v1[1];

            if (z > 0)
                return -1;
            else
                return 1;
        }

    }
}
