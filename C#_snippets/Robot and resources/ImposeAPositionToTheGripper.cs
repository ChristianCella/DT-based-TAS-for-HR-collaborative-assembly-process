/*
This snippet allows to impose a certain pose of the gripper, after specifying it by name.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
	public static void Main()
	{
	
		// Save the 3 poses of the gripper by specifying the name of the poses		
		TxPose ClosePose = TxApplication.ActiveDocument.
		GetObjectsByName("CLOSE")[0] as TxPose;
		
		TxPose OpenPose = TxApplication.ActiveDocument.
		GetObjectsByName("OPEN")[0] as TxPose;  
		
		TxPose HomePose = TxApplication.ActiveDocument.
		GetObjectsByName("HOME")[0] as TxPose; 
		
		// Save the gripper		
		ITxDevice Gripper = TxApplication.ActiveDocument.
		GetObjectsByName("Camozzi Gripper UR5e")[0] as TxGripper;
		
		// Impose a specific pose to the gripper		
		Gripper.CurrentPose = HomePose.PoseData;
		
		// Refresh the display		
		TxApplication.RefreshDisplay();
	}
}
