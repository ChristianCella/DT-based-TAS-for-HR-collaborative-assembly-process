/*
This snippet allows to get the 3 rotational coordinates and the 3 translational coordinates of a joint of a human.
In particular, the method 'GetJointAngles' returns a list of 6 values:
	* the first 3 values are the rotational coordinates (x, y, z);
	* the last 3 values are the translational coordinates (x, y, z).

For a possible evaluation of the RULA score, the main names to be remembered are:
	* "spine_couple" ==> Torso (Rotation around y)
	* "right/left_shoulder_couple" ==> Right/Left shoulder (Rotation around y)
	* "right/left_elbow" ==> Right/Left elbow (Rotation around y)
	* "right/left wrist" ==> Right/Left wrist (Rotation around y)
*/

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Tecnomatix.Engineering;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
    {
    	// Define some variables
    	bool verbose = true;
    	
    	// Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Get the names of all the joints
		List<string> joint_names = human.GetJointNames();

		// Display the joint names (useful to see how they are called, that is different than their name in the simulator)
		if(verbose){

			output.Write("The available joints are: " + output.NewLine);
        	foreach (string name in joint_names)
        	{
        		output.Write(name + output.NewLine);
        	}
        }
				
		// Set the joint angles (in case you need to change them)
		List<double> new_values = new List<double>();
		new_values.Add(0);
		new_values.Add(Math.PI/4);
		new_values.Add(0);
		new_values.Add(0.0);
		new_values.Add(0.0);
		new_values.Add(0.0);
		//human.SetJointAngles("right_shoulder_couple", new_values);
        
		// Refresh the display
        TxApplication.RefreshDisplay();
        
        // Get the joint angles
		List<double> joint_angles = human.GetJointAngles("spine_couple");
               
        // Display the joint angles for a specific joint
		output.Write("The 6 values are: " + output.NewLine);
        foreach (double angle in joint_angles)
        	{
        		output.Write(angle.ToString() + output.NewLine);
        	}
    }
}
