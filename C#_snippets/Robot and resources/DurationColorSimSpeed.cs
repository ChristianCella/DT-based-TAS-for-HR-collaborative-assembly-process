/*
This snippet allows mainly to set the percentage of the real time at which the simulation is run.
Also, it allows to save a specific point present in the robot program and display its duration and color.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Utilities;

public class MainScript
{
    public static void Main(ref StringWriter output)
    {
    
    	// Variables for the automatic simulation
		
		TxSimulationPlayer simPlayer = TxApplication.ActiveDocument.
		SimulationPlayer;
		
		// Reference the operation (type: "TxContinuousRoboticOperation")		
		TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
		TxContinuousRoboticOperation lineSimOp = allOps[0] as TxContinuousRoboticOperation;
		
		// Rewind the simulation		
		simPlayer.Rewind();
		TxApplication.RefreshDisplay();
		
    	// Save a specific point present in the robot program		
		TxRoboticViaLocationOperation selected_point = TxApplication.ActiveDocument.
        GetObjectsByName("point2")[0] as TxRoboticViaLocationOperation;
        
        // Display the duration of the selected waypoint      
        string Time = selected_point.Duration.ToString();
		output.Write("The time associated to the selected waypoint is: " + Time + output.NewLine);
           
        // Display the color of the first waypoint        
        HashSet<TxColor> colors = (selected_point as ITxDisplayableObject).GetColors();       
        foreach (var color in colors)
   		{
			// Display each channel separately
    		output.Write("The RGB colors are --- Red: " + color.Red.ToString() + " Green: " + color.Green.ToString() + "Blue: " +
			color.Blue.ToString() + output.NewLine);
		}
				
		// Run the simulation by controlling its speed (in terms of percentage of the real time)		
		TxApplication.RefreshDisplay();
		TxApplication.Options.Simulation.SimulationSpeed = 50; // velocity at 50% (Real time)
		simPlayer.SetOperation(lineSimOp);
		simPlayer.Play();
		  		
    }

    
}


