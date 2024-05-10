/*
This snippet allows to move and rotate the robot in the cartesian space. 
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
	public static void Main()
	{
	
		// Get the position of the robot (the index may change)	
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		var robot = selectedObjects[0] as ITxLocatableObject;
		
		// Rotate of an angle around the z axis		
		double rotVal = 0;
		TxTransformation rotZ = new TxTransformation(new TxVector(0, 0, rotVal), 
		TxTransformation.TxRotationType.RPY_XYZ);
		robot.AbsoluteLocation = rotZ;
		
		// Move the base of a certain quantity		
		var position = new TxTransformation(robot.LocationRelativeToWorkingFrame);
		position.Translation = new TxVector(0, 0, 0);
		robot.LocationRelativeToWorkingFrame = position;
		
		TxApplication.RefreshDisplay();
	}
}
