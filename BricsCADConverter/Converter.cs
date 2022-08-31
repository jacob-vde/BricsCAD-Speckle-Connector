using System.Collections.Generic;
using System.Linq;

using Bricscad.ApplicationServices;

using _OdDb = Teigha.DatabaseServices;
using _OdGe = Teigha.Geometry;
using _OdRx = Teigha.Runtime;

using Speckle.Core.Kits;
using Speckle.Core.Models;

using Alignment = Objects.BuiltElements.Alignment;
using Arc = Objects.Geometry.Arc;
using BlockDefinition = Objects.Other.BlockDefinition;
using BlockInstance = Objects.Other.BlockInstance;
using Circle = Objects.Geometry.Circle;
using Curve = Objects.Geometry.Curve;
using Dimension = Objects.Other.Dimension;
using Ellipse = Objects.Geometry.Ellipse;
using Hatch = Objects.Other.Hatch;
using Line = Objects.Geometry.Line;
using Mesh = Objects.Geometry.Mesh;
using ModelCurve = Objects.BuiltElements.Revit.Curve.ModelCurve;
using Point = Objects.Geometry.Point;
using Polycurve = Objects.Geometry.Polycurve;
using Polyline = Objects.Geometry.Polyline;
using Spiral = Objects.Geometry.Spiral;
using Text = Objects.Other.Text;
using Brep = Objects.Geometry.Brep;

namespace BricsCADConverter
{
  public class Converter: ISpeckleConverter
  {
    public string Description => "BricsCAD Converter";
    public string Name => "BricsCAD Converter";
    public string Author => "The BricsCAD Peeps)";
    public string WebsiteOrEmail => "https://github.com/jacob-vde/BricsCAD-Speckle-Connector";
    public ProgressReport Report { get; private set; } = new ProgressReport();
    public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.Create;
    
    public Document Doc { get; private set; }
    private ConvertGeometry GeomConverter = new ConvertGeometry();
    public Base ConvertToSpeckle(object @object)
    {
      throw new System.NotImplementedException();
    }

    public List<Base> ConvertToSpeckle(List<object> objects) => objects.Select(ConvertToSpeckle).ToList();

    public bool CanConvertToSpeckle(object @object)
    {
      //TODO: Update as you add conversions
      Doc.Editor.WriteMessage("Checking if we can convert to Speckle: " + @object);
      return false;
    }

    public object ConvertToNative(Base @object)
    {
            object bcadObj = null;
            switch (@object)
            {
                case Point o:
                    bcadObj = GeomConverter.PointToNativeDB(o);
                    Report.Log($"Created Point {o.id}");
                    break;

                case Line o:
                    bcadObj = GeomConverter.LineToNativeDB(o);
                    Report.Log($"Created Line {o.id}");
                    break;

                case Arc o:
                    bcadObj = GeomConverter.ArcToNativeDB(o);
                    Report.Log($"Created Arc {o.id}");
                    break;

                case Circle o:
                    bcadObj = GeomConverter.CircleToNativeDB(o);
                    Report.Log($"Created Circle {o.id}");
                    break;

                case Ellipse o:
                    bcadObj = GeomConverter.EllipseToNativeDB(o);
                    Report.Log($"Created Ellipse {o.id}");
                    break;

                /*                case Spiral o:
                                    bcadObj = GeomConverter.PolylineToNativeDB(o.displayValue);
                                    Report.Log($"Created Spiral {o.id} as Polyline");
                                    break;*/

                /*                case Hatch o:
                                    bcadObj = GeomConverter.HatchToNativeDB(o);
                                    Report.Log($"Created Hatch {o.id}");
                                    break;*/

                case Polyline o:
                    bcadObj = GeomConverter.PolylineToNativeDB(o);
                    Report.Log($"Created Polyline {o.id}");
                    break;

                case Polycurve o:
                    bcadObj = GeomConverter.PolycurveToNativeDB(o);
                    Report.Log($"Created Polycurve {o.id} as Polyline");
                    break;

                case Curve o:
                    bcadObj = GeomConverter.CurveToNativeDB(o);
                    Report.Log($"Created Curve {o.id}");
                    break;


                /*                case Brep o:
                                    bcadObj = (o.displayMesh != null) ? MeshToNativeDB(o.displayMesh) : null;
                                    Report.Log($"Created Brep {o.id} as Mesh");
                                    break;*/


                case Mesh o:
                    bcadObj = GeomConverter.MeshToNativeDB(o);
                    Report.Log($"Created Mesh {o.id}");
                    break;

                /*                case Dimension o:
                                    bcadObj = isFromAutoCAD ? AcadDimensionToNative(o) : DimensionToNative(o);
                                    Report.Log($"Created Dimension {o.id}");
                                    break;*/

                /*                case BlockInstance o:
                                    bcadObj = BlockInstanceToNativeDB(o, out BlockReference reference);
                                    Report.Log($"Created Block Instance {o.id}");
                                    break;*/

                /*                case BlockDefinition o:
                                    bcadObj = BlockDefinitionToNativeDB(o);
                                    Report.Log($"Created Block Definition {o.id}");
                                    break;*/

                /*                case Text o:
                                    bcadObj = isFromAutoCAD ? AcadTextToNative(o) : TextToNative(o);
                                    Report.Log($"Created Text {o.id}");
                                    break;*/

                /*case Alignment o:
                    string fallback = " as Polyline";
                    if (o.curves is null) // TODO: remove after a few releases, this is for backwards compatibility
                    {
                        bcadObj = CurveToNativeDB(o.baseCurve);
                        Report.Log($"Created Alignment {o.id} as Curve");
                        break;
                    }
                    if (bcadObj == null)
                        bcadObj = PolylineToNativeDB(o.displayValue);
                    Report.Log($"Created Alignment {o.id}{fallback}");
                    break;*/

                /*case ModelCurve o:
                    bcadObj = CurveToNativeDB(o.baseCurve);
                    Report.Log($"Created ModelCurve {o.id} as Curve");
                    break;*/
                default:
                    Report.Log($"Skipped not supported type: {@object.GetType()} {@object.id}");
                    throw new System.NotSupportedException();
            }
            return bcadObj;
    }

    public List<object> ConvertToNative(List<Base> objects) => objects.Select(ConvertToNative).ToList();

    public bool CanConvertToNative(Base @object)
    {
      switch (@object)
      {
        case Point _:
        case Line _:
        case Arc _:
        case Circle _:
        case Ellipse _:
        // case Spiral _:
        // case Hatch _:
        case Polyline _:
        case Polycurve _:
        case Curve _:
        // case Brep _:
        // case Mesh _:
        // case Dimension _:
        // case BlockDefinition _:
        // case BlockInstance _:
        // case Text _:
        // case Alignment _:
        // case ModelCurve _:
          return true;

        default:
          return false;
      }
    }

    public IEnumerable<string> GetServicedApplications() => new[] { "BricsCAD" };
    

    public void SetContextDocument(object doc)
    {
      // TODO: Here's were you pass in the BricsCAD document.
      if (doc is not Document document)
        throw new System.Exception("Input 'Doc'  must be a BricsCAD document");
      
      Doc = document;
    }

    public void SetContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      // TODO: This is to enable updating behaviour
    }

    public void SetPreviousContextObjects(List<ApplicationPlaceholderObject> objects)
    {
      // TODO: This is to enable updating behaviour
    }

    public void SetConverterSettings(object settings)
    {
      // TODO: Not mandatory.
    }
  }
}