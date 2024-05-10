/*
This snippet allows to execute a command present in the simulation program by calling its name.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
	public static void Main()
	{
	
		// Save the name of the robot in a variable		
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		
		// Watch out: unlike the majority of other cases, robot should not be cast
		// as a "ITxLocatableObject", but as a "TxCompoundResource"		
		var robot = selectedObjects[0] as TxCompoundResource;
		
		// Now, on the basis of the robot stored in "robot", activate the instance		
		TxApplication.ActiveSelection.SetItems(selectedObjects);
		
		// Execute the command "bring the robot to the home position" by passing the Id of the command		
		TxApplication.CommandsManager.ExecuteCommand("RobotAndDevice.Home");
		
		// Refresh the display
		TxApplication.RefreshDisplay();
		
		// Deselect the robot		
		TxApplication.ActiveSelection.Clear();
	}
	
}
