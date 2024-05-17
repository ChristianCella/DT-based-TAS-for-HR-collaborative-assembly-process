/*
This snippet allows to create from scratch the program with which the robot will pick up the tool at the beginning of the simulation.
It is suppoosed that, in the very first operation, the flange of the robot is empty and the tool is placed in a specific position (usually fixed).
The mounting (and, if necessary, the unmounting) of the tool is done in the third waypoint of the operation by implementing a OLP command.
To be mor realistic, a wait time is added after the mounting command.
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
		string operation_name = "PickTool1";
		
		string flange = "TOOLFRAME";
		string new_tcp = "tcp_1";
    	string motion_type_L = "MoveL";
    	string motion_type_J = "MoveJ";
		string new_speed = "1000";
		string new_accel = "1200";
		string new_blend = "0";
		string new_coord = "Cartesian";
		
		bool verbose = false; // Controls some display options
    
    	// Save the robot (the index may change)  	
    	TxObjectList objects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	var robot = objects[0] as TxRobot;
    	
    	// Store the gripper "Camozzi gripper"  	
		ITxObject Gripper_1 = TxApplication.ActiveDocument.
		GetObjectsByName("Gripper 1")[0] as TxGripper;
		
		// Store the reference frame "tgripper_tf" 	
  		ITxObject toolframe = TxApplication.ActiveDocument.
		GetObjectsByName("TOOLFRAME")[0] as TxFrame;
    	   	
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
        
        TxRoboticViaLocationOperationCreationData Point4 = new TxRoboticViaLocationOperationCreationData();
        Point4.Name = "point4"; // Fourth point
        
        TxRoboticViaLocationOperationCreationData Point5 = new TxRoboticViaLocationOperationCreationData();
        Point5.Name = "point5"; // Fifth point
        
        TxRoboticViaLocationOperationCreationData Point6 = new TxRoboticViaLocationOperationCreationData();
        Point6.Name = "point6"; // Sixth point
        
        TxRoboticViaLocationOperation FirstPoint = MyOp.CreateRoboticViaLocationOperation(Point1);
        TxRoboticViaLocationOperation SecondPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point2, FirstPoint);
        TxRoboticViaLocationOperation ThirdPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point3, SecondPoint);
        TxRoboticViaLocationOperation FourthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point4, ThirdPoint);
        TxRoboticViaLocationOperation FifthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point5, FourthPoint);
        TxRoboticViaLocationOperation SixthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point6, FifthPoint);
        
        // Impose a position to the first waypoint		
		TxTransformation rotX = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FirstPoint.AbsoluteLocation = rotX;
		
		var pointA = new TxTransformation(FirstPoint.AbsoluteLocation);
		pointA.Translation = new TxVector(300, -200, 300);
		FirstPoint.AbsoluteLocation = pointA;
		
		// Impose a position to the second waypoint		
		TxTransformation rotX2 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		SecondPoint.AbsoluteLocation = rotX2;
		
		var pointB = new TxTransformation(SecondPoint.AbsoluteLocation);
		pointB.Translation = new TxVector(420, 125, 180);
		SecondPoint.AbsoluteLocation = pointB;
		
		// Impose a position to the third waypoint		
		TxTransformation rotX3 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		ThirdPoint.AbsoluteLocation = rotX3;
		
		var pointC = new TxTransformation(ThirdPoint.AbsoluteLocation);
		pointC.Translation = new TxVector(350, 330, 180);
		ThirdPoint.AbsoluteLocation = pointC;
		
		// Impose a position to the fourth waypoint		
		TxTransformation rotX4 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FourthPoint.AbsoluteLocation = rotX4;
		
		var pointD = new TxTransformation(FourthPoint.AbsoluteLocation);
		pointD.Translation = new TxVector(350, 330, 300);
		FourthPoint.AbsoluteLocation = pointD;
		
		// Impose a position to the fifth waypoint		
		TxTransformation rotX5 = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FifthPoint.AbsoluteLocation = rotX5;
		
		var pointE = new TxTransformation(FifthPoint.AbsoluteLocation);
		pointE.Translation = new TxVector(325, 0, 450);
		FifthPoint.AbsoluteLocation = pointE;
		
		// Impose a position to the sixth waypoint		
		TxTransformation rotX6 = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		SixthPoint.AbsoluteLocation = rotX6;
		
		var pointF = new TxTransformation(SixthPoint.AbsoluteLocation);
		pointF.Translation = new TxVector(300, -200, 300);
		SixthPoint.AbsoluteLocation = pointF;

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
		paramHandler.OnComplexValueChanged("Tool", flange, FirstPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, FirstPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, FirstPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, FirstPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, FirstPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, FirstPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, SecondPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, SecondPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SecondPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SecondPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, SecondPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, SecondPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, ThirdPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, ThirdPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, ThirdPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, ThirdPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, ThirdPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, ThirdPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, FourthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, FourthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, FourthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, FourthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, FourthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, FourthPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, FifthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, FifthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, FifthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, FifthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, FifthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, FifthPoint);
		
		paramHandler.OnComplexValueChanged("Tool", new_tcp, SixthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, SixthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SixthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SixthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, SixthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, SixthPoint);
		
		// Add the mounting command and the waittime to point 3
		ArrayList elements1 = new ArrayList();
		ArrayList elements2 = new ArrayList();
		
		var myCmd1 = new TxRoboticCompositeCommandStringElement("# Mount");
    	var myCmd11 = new TxRoboticCompositeCommandTxObjectElement(Gripper_1);
    	var myCmd12 = new TxRoboticCompositeCommandTxObjectElement(toolframe);		
		var myCmd2 = new TxRoboticCompositeCommandStringElement("# WaitTime 2");
		
		elements1.Add(myCmd1);
    	elements1.Add(myCmd11);
    	elements1.Add(myCmd12);
		elements2.Add(myCmd2);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1 =
    	new TxRoboticCompositeCommandCreationData(elements1);
    	ThirdPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2 =
    	new TxRoboticCompositeCommandCreationData(elements2);
    	ThirdPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2);
       
        // Some display information
		if (verbose)
		{
			output.Write("The name of the operation is: " + MyOp.Name.ToString() + output.NewLine);
			output.Write("The name of the robot is: " + AssociatedRobot.Name.ToString() + output.NewLine);
			output.Write("The name of the controller is: " + robot.Controller.Name.ToString());
		}
               
    }
}
















