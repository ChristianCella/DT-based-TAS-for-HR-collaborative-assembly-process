/*
This code allows to select a waypoint (of an existing robot program) and access the speed value of the waypoint.
In a certain way, this is a simplified version of the snippets "CreateARobotProgramFromScratch.cs" and 
"AddAWaypointInARoboticProgram.cs".;
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;
using System.Windows.Forms;

public class MainScript
{
	public static void Main()
	{
		// robot ("UR5e")
		TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
		selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		var robot = selectedObjects1[0] as TxRobot;

		// Get a point defined in a robot program
		TxRoboticViaLocationOperation pointPrePickPos = TxApplication.ActiveDocument.GetObjectsByName("PrePickPos")[0] as TxRoboticViaLocationOperation;
		var PointPrePickPos = new TxTransformation(pointPrePickPos.LocationRelativeToWorkingFrame);
		
		// Reference the wanted operation (type: TxContinuousRoboticOperation)
		TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;

		// Get all the available operations 
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);

		// Get the only operation present (if not, change the index accordingly or perform a name search)
		TxContinuousRoboticOperation lineSimOp = allOps[0] as TxContinuousRoboticOperation;

		// If necessary, display the name of the operation
		TxMessageBox.Show(string.Format(lineSimOp.Name.ToString()), "Name", MessageBoxButtons.OK,
		MessageBoxIcon.Information);

		// Implement the logic to access the parameters of the virtual controller (filter "ControllerName")
		TxOlpControllerUtilities ControllerUtils = new TxOlpControllerUtilities();
		ITxOlpRobotControllerParametersHandler paramHandler = (ITxOlpRobotControllerParametersHandler)
		ControllerUtils.GetInterfaceImplementationFromController(robot.Controller.Name,
		typeof(ITxOlpRobotControllerParametersHandler), typeof(TxRobotSimulationControllerAttribute),
		"ControllerName");
		
		// Get the current value of the speed of the waypoint
		string SpeedVal = paramHandler.GetComplexRepresentation("Speed",
		pointPrePickPos, TxOlpCommandLayerRepresentation.UI);
		
		// If necessary, display the value
		TxMessageBox.Show(string.Format(SpeedVal), "Name", MessageBoxButtons.OK,
		MessageBoxIcon.Information);
		
		// Set the new speed		
		string new_speed = "50 mm/s";
        paramHandler.OnComplexValueChanged("Speed", new_speed, pointPrePickPos);
		
		
	}


}

