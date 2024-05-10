/*
This snippet allows to set specific angular values to each joint of the robot.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Plc;
using System.Collections;

public class MainScript
{
	public static void Main()
	{	
		
		// Activate the robot instance to allow not clicking on it everytime		
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		TxApplication.ActiveSelection.SetItems(selectedObjects);	
		
		// TxObjectList of logic resources ==> way to get the joints		
		TxObjectList logicResources = TxApplication.ActiveSelection.
		GetFilteredItems(new TxTypeFilter(typeof(ITxDevice)));	
		
		ITxPlcLogicResource logicResource = logicResources[0] as ITxPlcLogicResource;		
		TxPlcLogicBehavior logicBehavior = logicResource.LogicBehavior;
		
		// store a specific joint of the robot		
		TxObjectList drivingJoints = (logicResource as ITxDevice).DrivingJoints;
        TxJoint j1 = drivingJoints[0] as TxJoint;
        
        // print the name of the selected joint      
        TxMessageBox.Show(string.Format(j1.Name.ToString()), "Name", MessageBoxButtons.OK,
		MessageBoxIcon.Information);
        
        // print the current value (in radians)      
        TxMessageBox.Show(string.Format(j1.CurrentValue.ToString()), "Value", MessageBoxButtons.OK,
		MessageBoxIcon.Information);
		
		// Change the current value       
        double RotNow = -Math.PI / 6;
        j1.CurrentValue = RotNow;		
		TxApplication.RefreshDisplay();
		
		// Deselect the robot		
		TxApplication.ActiveSelection.Clear();
	}
}
