using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Bricsys
using Bricscad.ApplicationServices;
using Bricscad.EditorInput;
using Speckle.Core.Api;
using Speckle.Core.Credentials;
using Speckle.Core.Kits;
using Speckle.Core.Models.Extensions;
using Speckle.Core.Transports;

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
            //_OdDb.Database db = _OdDb.HostApplicationServices.WorkingDatabase;Z
            
            // Get the default speckle kit
            var kit = KitManager.GetDefaultKit();
            editor.WriteMessage("\nFound kit: " + kit.Name);

            // There should only be one available converter for bricscad right now
            var converter = kit.LoadConverter("BricsCAD");
            // Pass the current document to the converter
            converter.SetContextDocument(Application.DocumentManager.MdiActiveDocument);
            editor.WriteMessage("\nLoaded converter: " + converter);
            
            // Convert some objects, maybe the current selection?
            // var baseObject = converter.ConvertToNative(someObjectFromBRICS)
            
            // Send them to speckle!! TBD
        }
        
        [_OdRx.CommandMethod("SpeckleReceive")]
        public static void ReceiveSpeckleData()
        {
            // TODO: These may be inputs of your commands, most of these things are handled by our UI (which we're skipping for now)
            // We're also assuming you'll be sending/receiving data from "https://speckle.xyz" for now
            
            var streamId = "51d8c73c9d";
            // The name of the branch we'll send data to.
            var branchName = "main";
            
            var editor = Application.DocumentManager.MdiActiveDocument.Editor;
            editor.WriteMessage("\nCollecting data from speckle");
            
            // Get the default speckle kit
            var kit = KitManager.GetDefaultKit();
            editor.WriteMessage("\nFound kit: " + kit.Name);

            // There should only be one available converter for bricscad right now
            var converter = kit.LoadConverter("BricsCAD");
            // Pass the current document to the converter
            converter.SetContextDocument(Application.DocumentManager.MdiActiveDocument);
            editor.WriteMessage("\nLoaded converter: " + converter);
            
            // Wrapped this in a task bc it was freezing the main thread
            Task.Run(() =>
            {
                try
                {
                    // Get default account on this machine
                    // If you don't have Speckle Manager installed download it from https://speckle-releases.netlify.app
                    var defaultAccount = AccountManager.GetDefaultAccount();
                    // Or get all the accounts and manually choose the one you want
                    // var accounts = AccountManager.GetAccounts();
                    // var defaultAccount = accounts.ToList().FirstOrDefault();

                    // Authenticate using the account
                    var client = new Client(defaultAccount);


                    // Now we can start using the client
                    
                    // Get the main branch with it's latest commit reference
                    var branch = client.BranchGet(streamId, branchName, 1).Result;
                    // Get the id of the object referenced in the commit
                    var hash = branch.commits.items[0].referencedObject;


                    // Create the server transport for the specified stream.
                    var transport = new ServerTransport(defaultAccount, streamId);
                    // Receive the object
                    var receivedBase = Operations.Receive(hash, transport).Result;


                    editor.WriteMessage("\nFinished receiving data:");

                    // You can flatten the object you received and only get the inner children that are convertible
                    var convertibleObjects = receivedBase.Flatten(converter.CanConvertToNative);

                    editor.WriteMessage("\nConvertible objects: " + convertibleObjects.Count);

                }
                catch (Exception e)
                {
                    editor.WriteMessage("\n" + e.ToFormattedString());
                }
            });
        }
    }
}
