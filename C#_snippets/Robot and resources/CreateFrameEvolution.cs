/*
This snippet is an evolution of 'CreateFrame.cs'. It allows to create a frame and associate it to a selected object without the need
for it to be opened in the 'setModellingScope' interface of the simulator. It also allows to specify the color of the frame and its position.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
    {
    	// Initialize a new data
    	TxFrameCreationData frameData = new TxFrameCreationData();
		frameData.Name = "NewFrame";
		
		// Create the frame (in the 'Frames' folder)
    	TxFrame frame = TxApplication.ActiveDocument.PhysicalRoot.CreateFrame(frameData);
        frame.Color = TxColor.TxColorOrange; // Give an orange color to the frame
        
        // Save the newly created frame in a variable
        var fr = frame as ITxLocatableObject;

        // Set the new position of the frame (usually, it's the same position as the object)
        var position = new TxTransformation(fr.LocationRelativeToWorkingFrame);
        position.Translation = new TxVector(300, 500, 30);
        fr.LocationRelativeToWorkingFrame = position;
        
        // Get the object for the pick	(Also, refresh the display)	
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube3");
		var cube = cube_pick[0] as ITxLocatableObject;
		
		// Attach the frame to the cube
		fr.AttachTo(cube);        
    }
}
