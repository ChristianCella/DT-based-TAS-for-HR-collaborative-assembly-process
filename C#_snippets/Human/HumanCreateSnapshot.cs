/*
This snippet allows to cerate a snapshot of the scene: basically, it's very similar to what 'refresh display' does.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
    {
    
    	string snapName = "MySnap";
    
    	// Save the human   	
    	TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman; 
		
		// Capture the current positions of the instances		
		TxSnapshotCreationData creationData = new TxSnapshotCreationData();
		creationData.Name = snapName;
		TxPhysicalRoot cell = TxApplication.ActiveDocument.PhysicalRoot;
		TxSnapshot txSnapshot = cell.CreateSnapshot(creationData);

		// Here write a code that does something
		
		// Return everything to the original positions		
		TxApplySnapshotParams snapParam = new TxApplySnapshotParams();
        snapParam.ObjectsLocation = true;
        snapParam.ObjectsAttachments = true;
        snapParam.DevicePoses = true; // Also applies to Human postures               
        txSnapshot.Apply(snapParam);

    }
}
