/*
This script takes an already existing operation (called "Atomic Pick and Place") and changes the position of the 
cube; after that, a new simulation is forced and the times update automatically.
This can only be done with the patch containing the new '.dll' files.
*/

using System;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using EngineeringInternalExtension;

public class MainScript
{
	
    public static void Main(ref StringWriter output)
    {
         
        // Get the operation
        TxObjectList Operation = TxApplication.ActiveDocument.GetObjectsByName("Atomic Pick and Place");
        TxHumanTsbSimulationOperation op = Operation[0] as TxHumanTsbSimulationOperation;

        // Display the name       
		output.Write("The name of the operation is : " + op.Name.ToString() + output.NewLine);
		
		// Get the cube   	
    	TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube = selectedObjects[0] as ITxLocatableObject;
		
		var position = new TxTransformation(cube.LocationRelativeToWorkingFrame);
		position.Translation = new TxVector(450, 0, 25);
		cube.LocationRelativeToWorkingFrame = position;
		
		TxApplication.RefreshDisplay();
		
        // Get the simulation and force the possible re-simulation        
        op.SetInitialContext();
        op.ForceResimulation();

        // Play the simulation
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Play();

        // Display the duration (if needed)
        double duration = op.Duration;
        output.Write("The duration is : " + duration);
        
        // Rewind for completeness (if needed)       
        player.Rewind();
    }
}