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
using Speckle.Core.Models;
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
            //_OdDb.Database db = _OdDb.HostApplicationServices.WorkingDatabase;

            Task.Run(() =>
            {
                try
                {
                    // The stream you want to send to
                    var streamId = "5a16dde665";
                    // The name of the branch we'll send data to.
                    var branchName = "main";

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

                    // TODO: We use a custom object for now
                    var baseObject = new Base();
                    baseObject["fromBRICS"] = "This comes from BRICSCAD!!!";
                    // Send them to speckle!! TBD

                    var defaultAccount = AccountManager.GetDefaultAccount();
                    // Or get all the accounts and manually choose the one you want
                    // var accounts = AccountManager.GetAccounts();
                    // var defaultAccount = accounts.ToList().FirstOrDefault();

                    // Authenticate using the account
                    var client = new Client(defaultAccount);

                    // Create the server transport for the specified stream.
                    var transport = new ServerTransport(defaultAccount, streamId);

                    editor.WriteMessage($"\nStarting to send data to stream {streamId} and branch {branchName}");

                    // Sending the object will return it's unique identifier.
                    var newHash = Operations.Send(baseObject, new List<ITransport> { transport }).Result;

                    // Create a commit in `processed` branch (it must previously exist)
                    var commitId = client.CommitCreate(new CommitCreateInput()
                    {
                        branchName = branchName,
                        message = "Sent a Custom Object from BricsCAD",
                        objectId = newHash,
                        streamId = streamId,
                        sourceApplication = "BricsCAD"
                    }).Result;

                    editor.WriteMessage(
                        $"\nSuccessfully created commit {commitId} in branch {branchName}. Go see it at: {defaultAccount.serverInfo.url}/streams/{streamId}/commits/{commitId}");

                    // Remember to dispose of the client once you've finished with it.
                    client.Dispose();
                }
                catch (Exception e)
                {
                    editor.WriteMessage($"\n{e.ToFormattedString()}");
                }
                editor.WriteMessage($"\n");
            });
        }
        
        
        [_OdRx.CommandMethod("SpeckleReceive")]
        public static void ReceiveSpeckleData()
        {
            // TODO: These may be inputs of your commands, most of these things are handled by our UI (which we're skipping for now)
            // We're also assuming you'll be sending/receiving data from "https://speckle.xyz" for now
            
            // The stream you want to receive from
            var streamId = "5a16dde665";
            // The name of the branch we'll receive data from.
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
                    var convertibleObjects = receivedBase.Flatten(converter.CanConvertToNative).Where(converter.CanConvertToNative);

                    editor.WriteMessage($"\nConvertible objects: {convertibleObjects.Count}");
                }
                catch (Exception e)
                {
                    editor.WriteMessage($"\n{e.ToFormattedString()}");
                }
                editor.WriteMessage($"\n");
            });
        }
    }
}
