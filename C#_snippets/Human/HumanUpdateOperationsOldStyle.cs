/*
The script is used to manipulate operations for the humans with the old format (manual concatenation of tasks).
You need to have an operation manually initialiezed in the root of the simulator, and then you can use this 
script to move the waypoints to the desired positions:
	* first you get the intermediate point and the final one (the first one is moved by moving the cube);
	* you update the simulation and you run the it.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using System.Collections.Generic;
using Tecnomatix.Engineering.DataTypes;

public class MainScript
{
    public static void Main(ref StringWriter output)
    {
    
    	// Get the position of the cube    	
    	TxObjectList CubeYaosc = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube = CubeYaosc[0] as ITxLocatableObject;
		
		// Move the cube of a certain quantity		
		var position = new TxTransformation(cube.LocationRelativeToWorkingFrame);
		position.Translation = new TxVector(250, 250, 25);
		cube.LocationRelativeToWorkingFrame = position;
		
		// Get the 'TxHumanReachLocationOperation' frames		
		TxHumanReachLocationOperation FrameInt = TxApplication.ActiveDocument.
        GetObjectsByName("ReachLoc5")[0] as TxHumanReachLocationOperation;
        
        var positionInt = new TxTransformation(FrameInt.LocationRelativeToWorkingFrame);
        positionInt.Translation = new TxVector(350, 0, 40);
        FrameInt.LocationRelativeToWorkingFrame = positionInt;
        
        TxHumanReachLocationOperation FrameFin = TxApplication.ActiveDocument.
        GetObjectsByName("ReachLoc6")[0] as TxHumanReachLocationOperation;
        
        var positionFin = new TxTransformation(FrameFin.LocationRelativeToWorkingFrame);
        positionFin.Translation = new TxVector(600, -250, 25);
        FrameFin.LocationRelativeToWorkingFrame = positionFin;
		
		// Refresh the display
		
		TxApplication.RefreshDisplay();
		
		// Get the task

		TxObjectList selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("Seq3");	
		TxCompoundOperation PickAndPlace = selectedObjects[0] as TxCompoundOperation;
		TxApplication.ViewersManager.PathEditorViewer.AddOperation(PickAndPlace);
		
		// Access the simulation player
		
		TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();
		
		// Run the simulation
		
		TxApplication.RefreshDisplay();
		player.Play();
		
		// Rewind
		
		player.Rewind();
		

    } 
}


