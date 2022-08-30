using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Bricsys
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;

// ODA
using _OdDb = Teigha.DatabaseServices;
using _OdGe = Teigha.Geometry;
using _OdRx = Teigha.Runtime;

// This line is not mandatory, but improves loading performances
[assembly: _OdRx.CommandClass(typeof(BSC.Commands))]

namespace BSC // BricsCAD Speckle Connector
{
    // This class is instantiated by BricsCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document
    public class Commands
    {
        [_OdRx.CommandMethod("SpeckleSend")]
        public static void SendDataToSpeckle()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\nSending data to speckle");
            //_OdDb.Database db = _OdDb.HostApplicationServices.WorkingDatabase;
        }

        [_OdRx.CommandMethod("SpeckleReceive")]
        public static void ReceiveSpeckleData()
        {
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\nCollecting data from speckle");
        }
    }
}
