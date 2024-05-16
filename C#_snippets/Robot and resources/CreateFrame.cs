/*
This snippet allows to create a Frame and associate it to as selected object (of class TxComponent). The problem is that the object msut be opened in 
'SetModellingScope' in the simulator, otherwise an error is thrown.
Look at 'CreateFrameEvolution.cs' for a more complete example.
*/
 
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;
 
public class MainScript
{
    public static void Main()
    {
        // Initialize a new data
    	TxFrameCreationData variabile = new TxFrameCreationData();
		variabile.Name = "Variabile";

        // Save the object in a variable
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
        selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("Cube");
        TxComponent cube = selectedObjects[0] as TxComponent;

        // Create a frame and associate it to the object
        TxFrame frame = cube.CreateFrame(variabile);

        // Save the newly created frame in a variable
        var fr = frame as ITxLocatableObject;

        // Set the new position of the frame (usually, it's the same position as the object)
        var position = new TxTransformation(fr.LocationRelativeToWorkingFrame);
        position.Translation = new TxVector(300, 500, 30);
        fr.LocationRelativeToWorkingFrame = position;
    }	
}
