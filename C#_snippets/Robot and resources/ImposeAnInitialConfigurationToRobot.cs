/*
This snippet allows to impose a certain configuration to the robot, in the following way:
	- Get the position and the orientation of a point that represents the desired pose of the TCP
	- Solve the inverse kinematics to get the joint values that correspond to the desired pose
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
	public static void Main()
	{
	
		// Get the position and the orientation of a point	
		TxRoboticViaLocationOperation via1 = TxApplication.ActiveDocument.
        GetObjectsByName("point1")[0] as TxRoboticViaLocationOperation;
        
        // Define a variable for the inverse kinematics       
		TxRobotInverseData inv = new TxRobotInverseData
		(via1.AbsoluteLocation);
		
		// Save in a variable the instance of the robot	
		
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");		
		var Robot = selectedObjects[0] as TxRobot;
		
		// Evaluate ALL the solutions of the inverse kinematics (and save the first pose)		
		var poses = Robot.CalcInverseSolutions(inv);	
		var poseData = poses[0] as TxPoseData;
		
		// Impose a specific configuration to the robot		
		Robot.CurrentPose = poseData;
		
		// Refresh the display		
		TxApplication.RefreshDisplay();
		
	}
	
}
