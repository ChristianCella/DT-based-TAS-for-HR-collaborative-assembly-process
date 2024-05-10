/*
This snippet allows to create a robot program from zero (and it's the equivalent of importing a program from the
simulator). 
It also allows to add waypoints in specific positions and set their parameters.
Finally, it allows to add OLP commands to close the gripper in a specific waypoint.
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
    public static void MainWithOutput(ref StringWriter output)
    {

		// Define some variables
		string operation_name = "RoboticProgram";

		string new_tcp = "tcp_1";
    	string new_motion_type = "MoveL";
		string new_speed = "1000";
		string new_accel = "1200";
		string new_blend = "0";
		string new_coord = "Cartesian";
		
		bool verbose = false; // Controls some display options
    
    	// Save the robot (the index may change)  	
    	TxObjectList objects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	var robot = objects[1] as TxRobot;
    	   	
    	// Create the new operation    	
    	TxContinuousRoboticOperationCreationData data = new TxContinuousRoboticOperationCreationData(operation_name);
    	TxApplication.ActiveDocument.OperationRoot.CreateContinuousRoboticOperation(data);
    	
		// Get the created operation
    	TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
        TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
                
 		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
        TxContinuousRoboticOperation MyOp = allOps[0] as TxContinuousRoboticOperation; // The index may change

		// Create all the necessary points       
        TxRoboticViaLocationOperationCreationData Point1 = new TxRoboticViaLocationOperationCreationData();
        Point1.Name = "point1"; // First point
        
        TxRoboticViaLocationOperationCreationData Point2 = new TxRoboticViaLocationOperationCreationData();
        Point2.Name = "point2"; // Second point
        
        TxRoboticViaLocationOperationCreationData Point3 = new TxRoboticViaLocationOperationCreationData();
        Point3.Name = "point3"; // Third point
        
        TxRoboticViaLocationOperation FirstPoint = MyOp.CreateRoboticViaLocationOperation(Point1);
        TxRoboticViaLocationOperation SecondPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point2, FirstPoint);
        TxRoboticViaLocationOperation ThirdPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point3, SecondPoint);
        
        // Impose a position to the new waypoint		
		double rotVal = Math.PI;
		TxTransformation rotX = new TxTransformation(new TxVector(rotVal, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FirstPoint.AbsoluteLocation = rotX;
		
		var pointA = new TxTransformation(FirstPoint.AbsoluteLocation);
		pointA.Translation = new TxVector(300, 0, 300);
		FirstPoint.AbsoluteLocation = pointA;
		
		// Impose a position to the second waypoint		
		double rotVal2 = Math.PI;
		TxTransformation rotX2 = new TxTransformation(new TxVector(rotVal2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		SecondPoint.AbsoluteLocation = rotX2;
		
		var pointB = new TxTransformation(SecondPoint.AbsoluteLocation);
		pointB.Translation = new TxVector(300, 0, 25);
		SecondPoint.AbsoluteLocation = pointB;
		
		// Impose a position to the third waypoint		
		double rotVal3 = Math.PI;
		TxTransformation rotX3 = new TxTransformation(new TxVector(rotVal3, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		ThirdPoint.AbsoluteLocation = rotX3;
		
		var pointC = new TxTransformation(ThirdPoint.AbsoluteLocation);
		pointC.Translation = new TxVector(300, 0, 300);
		ThirdPoint.AbsoluteLocation = pointC;

		// NOTE: you must associate the robot to the operation!
		MyOp.Robot = robot; 

		// Implement the logic to access the parameters of the controller		
		TxOlpControllerUtilities ControllerUtils = new TxOlpControllerUtilities();		
		TxRobot AssociatedRobot = ControllerUtils.GetRobot(MyOp); // Verify the correct robot is associated 
				
		ITxOlpRobotControllerParametersHandler paramHandler = (ITxOlpRobotControllerParametersHandler)
		ControllerUtils.GetInterfaceImplementationFromController(robot.Controller.Name,
		typeof(ITxOlpRobotControllerParametersHandler), typeof(TxRobotSimulationControllerAttribute),
		"ControllerName");
		
		// Set the new parameters for the waypoint					
		paramHandler.OnComplexValueChanged("Tool", new_tcp, FirstPoint);
		paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, FirstPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, FirstPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, FirstPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, FirstPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, FirstPoint);
		
		paramHandler.OnComplexValueChanged("Tool", new_tcp, SecondPoint);
		paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, SecondPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SecondPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SecondPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, SecondPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, SecondPoint);
		
		paramHandler.OnComplexValueChanged("Tool", new_tcp, ThirdPoint);
		paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, ThirdPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, ThirdPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, ThirdPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, ThirdPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, ThirdPoint);
		
		// Save the second point to close the gripper		
		TxRoboticViaLocationOperation Waypoint =  TxApplication.ActiveDocument.
  		GetObjectsByName("point2")[0] as TxRoboticViaLocationOperation;
  		
  		// Save the gripper "Camozzi gripper" 	
  		ITxObject Gripper = TxApplication.ActiveDocument.
		GetObjectsByName("Camozzi Gripper UR5e")[0] as TxGripper;
  		
  		// Save the pose "Gripper Closed"  		
  		ITxObject Pose = TxApplication.ActiveDocument.
		GetObjectsByName("CLOSE")[0] as TxPose;
  		
  		// Save the reference frame of the gripper 		
  		ITxObject tGripper = TxApplication.ActiveDocument.
		GetObjectsByName("tf_tcp_2")[0] as TxFrame;
		
		// Create an array called "elements" and the command to be written in it
    	ArrayList elements1 = new ArrayList();
    	ArrayList elements2 = new ArrayList();
    	ArrayList elements3 = new ArrayList();
    	ArrayList elements4 = new ArrayList();
    	ArrayList elements5 = new ArrayList();
	
    	var myCmd1 = new TxRoboticCompositeCommandStringElement("# Destination");
    	var myCmd11 = new TxRoboticCompositeCommandTxObjectElement(Gripper);

    	var myCmd2 = new TxRoboticCompositeCommandStringElement("# Drive");
    	var myCmd21 = new TxRoboticCompositeCommandTxObjectElement(Pose);

    	var myCmd3 = new TxRoboticCompositeCommandStringElement("# Destination");
    	var myCmd31 = new TxRoboticCompositeCommandTxObjectElement(Gripper);

    	var myCmd4 = new TxRoboticCompositeCommandStringElement("# WaitDevice");
    	var myCmd41 = new TxRoboticCompositeCommandTxObjectElement(Pose);

    	var myCmd5 = new TxRoboticCompositeCommandStringElement("# Grip");
    	var myCmd51 = new TxRoboticCompositeCommandTxObjectElement(tGripper);
	
 		// First line of command	
    	elements1.Add(myCmd1);
    	elements1.Add(myCmd11);
	  	
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1 =
    	new TxRoboticCompositeCommandCreationData(elements1);
	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1);
    	
		// Second line of command
    	elements2.Add(myCmd2);
    	elements2.Add(myCmd21);

    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2 =
    	new TxRoboticCompositeCommandCreationData(elements2);
	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2);
    	
		// Third line of command
    	elements3.Add(myCmd3);
    	elements3.Add(myCmd31);

    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData3 =
    	new TxRoboticCompositeCommandCreationData(elements3);
	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData3);
    	
		// Fourth line of command
    	elements4.Add(myCmd4);
    	elements4.Add(myCmd41);

    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData4 =
    	new TxRoboticCompositeCommandCreationData(elements4);
	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData4);
    	
		// Fifth line of command	
    	elements5.Add(myCmd5);
    	elements5.Add(myCmd51);

    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData5 =
    	new TxRoboticCompositeCommandCreationData(elements5);
	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData5);
       
        // Some display information
		if (verbose)
		{
			output.Write("The name of the operation is: " + MyOp.Name.ToString() + output.NewLine);
			output.Write("The name of the robot is: " + AssociatedRobot.Name.ToString() + output.NewLine);
			output.Write("The name of the controller is: " + robot.Controller.Name.ToString());
		}
               
    }
}















