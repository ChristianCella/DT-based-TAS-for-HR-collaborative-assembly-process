/*
This snippet adds a waypoint in a robotic program after a selected point and sets its parameters.
*/

using System.Collections;
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
    
    	// Variables defined to set the new parameters of the waypoint   	
    	string new_tcp = "tcp_1";
    	string new_motion_type = "MoveL";
		string new_speed = "1000";
		string new_accel = "1200";
		string new_blend = "0";
		string new_coord = "Cartesian";
		
		bool verbose = false; // Boolean controlling some display messages
    
		// Set (in the sequence editor) the desired operation by calling its name   	
        var op = TxApplication.ActiveDocument.OperationRoot.GetAllDescendants(new 
        TxTypeFilter(typeof(TxCompoundOperation))).FirstOrDefault(x => x.Name.Equals("TentativeTrajectory")) as 
        TxCompoundOperation;     
        TxApplication.ActiveDocument.CurrentOperation = op;

    	// Create a variable to run the simualtion    
    	TxSimulationPlayer simPlayer = TxApplication.ActiveDocument.
		SimulationPlayer;
		
		// Save the robot in a 'general' variable (the index may change)		
		TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
		selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		var robot = selectedObjects1[1] as TxRobot;
		
		// Search for the operation of type "TxContinuousRoboticOperation" and save it in a variable   	
     	TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;  
		
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
		TxContinuousRoboticOperation MyOp = allOps[1] as TxContinuousRoboticOperation;
		
		// Save the point after which you want to introduce the waypoint 		
		TxRoboticViaLocationOperation point1 = TxApplication.ActiveDocument.
        GetObjectsByName("Pick3")[0] as TxRoboticViaLocationOperation; 
        
		// Create a new waypoint after the selected point
        TxRoboticViaLocationOperationCreationData NewPoint = new TxRoboticViaLocationOperationCreationData();       
		NewPoint.Name = "NewPoint";
		
		TxRoboticViaLocationOperation PointNew = MyOp.CreateRoboticViaLocationOperationAfter(NewPoint, point1);
		
		// Impose a position and a rotation to the new waypoint		
		double rotVal = Math.PI;
		TxTransformation rotX = new TxTransformation(new TxVector(rotVal, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		PointNew.AbsoluteLocation = rotX;
		
		var pointA = new TxTransformation(PointNew.AbsoluteLocation);
		pointA.Translation = new TxVector(300, 0, 300);
		PointNew.AbsoluteLocation = pointA;
		
		// Implement the logic to access the parameters of the controller		
		TxOlpControllerUtilities ControllerUtils = new TxOlpControllerUtilities();

		ITxOlpRobotControllerParametersHandler paramHandler = (ITxOlpRobotControllerParametersHandler)
		ControllerUtils.GetInterfaceImplementationFromController(robot.Controller.Name,
		typeof(ITxOlpRobotControllerParametersHandler), typeof(TxRobotSimulationControllerAttribute),
		"ControllerName");
		
		// Set the new parameters for the waypoint (one forneach column in the table)				
		TxRoboticViaLocationOperation newpoint = TxApplication.ActiveDocument.GetObjectsByName("NewPoint")[0] as TxRoboticViaLocationOperation;	
		
		paramHandler.OnComplexValueChanged("Tool", new_tcp, newpoint);
		paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, newpoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, newpoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, newpoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, newpoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, newpoint);
		
		// Refresh the display and run the simulation		
		simPlayer.Rewind();
		TxApplication.RefreshDisplay();
		simPlayer.Play();
		
		// Delete the waypoint added (if needed)		
		if (verbose)
		{
			TxRoboticViaLocationOperation PointToDelete = TxApplication.ActiveDocument.
        	GetObjectsByName("NewPoint")[0] as TxRoboticViaLocationOperation;
        	PointToDelete.Delete();
		}	
        
		
    }

    
    
}
