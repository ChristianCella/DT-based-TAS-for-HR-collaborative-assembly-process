/*
This snippet allows to expand the code 'ExecuteCommandAutomaticallyRobotHomePosition.cs' since it also allows to
open the joint jog window. 
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Plc;

public class MainScript
{
	public static void Main(ref StringWriter output)
	{	
		
		// Get all the robots present in the document		
		TxTypeFilter opFilter = new TxTypeFilter(typeof(TxRobot));
		TxPhysicalRoot opRoot = TxApplication.ActiveDocument.PhysicalRoot;
		
		TxObjectList allRobots = opRoot.GetAllDescendants(opFilter);
		TxRobot rob = null;
			
		// Scan the list of robots
		foreach(TxRobot r in allRobots)
		{
		
			// If it's the one I want ==> update the variable lineSimOp
			
			if (r.Name.Equals("UR5e"))
			{
				rob = r;
				output.Write("The name of the robot is: " + r.Name.ToString() + output.NewLine);
				
				// Select the command to activate
				TxApplication.ActiveSelection.SetItems(allRobots);
				TxApplication.CommandsManager.ExecuteCommand("RobotAndDevice.Home"); // Home the robot
				// TxApplication.CommandsManager.ExecuteCommand("RobotAndDevice.JointJog"); // Open the joint jog window
				TxApplication.ActiveSelection.Clear();
				
				break; // exit the loop if the condition is satisfied
			}
		}
		
		
	}
}
