/*
This snippet allows to retrieve some information about the joints of a robot.
Look at 'ImposeEachJointValue.cs' to see how to change the value of a joint.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Plc;

public class MainScript
{
	public static void Main()
	{	
		
		// Save and activate automatically the robot 		
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		TxApplication.ActiveSelection.SetItems(selectedObjects);	
		
		// TxObjectList of logic resources ==> way to get the joints of the robot		
		TxObjectList logicResources = TxApplication.ActiveSelection.
		GetFilteredItems(new TxTypeFilter(typeof(ITxDevice)));	
		
		ITxPlcLogicResource logicResource = logicResources[0] as ITxPlcLogicResource;		
		TxPlcLogicBehavior logicBehavior = logicResource.LogicBehavior;
		
		// Save all the joints of the robot		
		TxObjectList drivingJoints = (logicResource as ITxDevice).DrivingJoints;
        TxJoint j1 = drivingJoints[0] as TxJoint;
        
        // Display the name       
        TxMessageBox.Show(string.Format(j1.Name.ToString()), "Name", MessageBoxButtons.OK,
		MessageBoxIcon.Information);
        
        // Display the current value (in radians)       
        TxMessageBox.Show(string.Format(j1.CurrentValue.ToString()), "Value", MessageBoxButtons.OK,
		MessageBoxIcon.Information);
        
	}
}

