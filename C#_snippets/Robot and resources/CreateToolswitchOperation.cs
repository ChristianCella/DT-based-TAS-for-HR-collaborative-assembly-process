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
    
		// Save the robot (the index may change)
    	TxObjectList objects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	var robot = objects[0] as TxRobot;
    	
    	// Get the tool that is currently mounted on the robot
    	TxObjectList tools_list = robot.MountedTools;
    	var tool = tools_list[0];
    	string tool_name = tool.Name;
    	output.Write("The name of the mounted tool is : " + tool_name + output.NewLine);
		
		// Define some variables
		string operation_name = "SwitchTool";
		
		string flange = "TOOLFRAME";
		string new_tcp = "tcp_1";
    	string motion_type_L = "MoveL";
    	string motion_type_J = "MoveJ";
		string new_speed = "1000";
		string new_accel = "1200";
		string new_blend = "0";
		string new_coord = "Cartesian";
		
		bool verbose = false; // Controls some display options
      	
    	// Save the currently mounted gripper 	
		ITxObject Gripper_mounted = TxApplication.ActiveDocument.
		GetObjectsByName(tool_name)[0] as TxGripper;
		
		// Save the gripper to be mounted
    	TxTypeFilter opFilter = new TxTypeFilter(typeof(TxGripper));
        TxPhysicalRoot opRoot = TxApplication.ActiveDocument.PhysicalRoot;                
 		TxObjectList allTools = opRoot.GetAllDescendants(opFilter);
 		
 		// Conversion to list of strings (from TxObjectList type)
 		List<string> all_available_tools = null;
 		all_available_tools = new List<string>();
 		
 		foreach(ITxObject ob in allTools)
 		{
 			all_available_tools.Add(ob.Name.ToString());
 		}
 		int index = all_available_tools.FindIndex(name => name != tool_name); //Check when the name differs		
 		output.Write("The index of the tool to be mounted is : " + index.ToString() + output.NewLine);
		
		// Save the instance of the gripper to be mounted in a variable	
		ITxObject Gripper_tobe_mounted = allTools[index] as TxGripper;
		output.Write("The name of the tool to be mounted is : " + Gripper_tobe_mounted.Name + output.NewLine);
		
		// Save the reference frame of the flange	
  		ITxObject toolframe = TxApplication.ActiveDocument.
		GetObjectsByName("TOOLFRAME")[0] as TxFrame;
    	   	
    	// Create the new operation    	
    	TxContinuousRoboticOperationCreationData data = new TxContinuousRoboticOperationCreationData(operation_name);
    	TxApplication.ActiveDocument.OperationRoot.CreateContinuousRoboticOperation(data);
        
        // Save the created operartion in a variable
        TxContinuousRoboticOperation MyOp = TxApplication.ActiveDocument.GetObjectsByName(operation_name)[0] as TxContinuousRoboticOperation;

		// Create all the necessary points (9 in total)      
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
        
        TxRoboticViaLocationOperationCreationData Point7 = new TxRoboticViaLocationOperationCreationData();
        Point7.Name = "point7"; // Seventh point
        
        TxRoboticViaLocationOperationCreationData Point8 = new TxRoboticViaLocationOperationCreationData();
        Point8.Name = "point8"; // Eighth point
        
        TxRoboticViaLocationOperationCreationData Point9 = new TxRoboticViaLocationOperationCreationData();
        Point9.Name = "point9"; // Ninth point
        
        TxRoboticViaLocationOperation FirstPoint = MyOp.CreateRoboticViaLocationOperation(Point1);
        TxRoboticViaLocationOperation SecondPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point2, FirstPoint);
        TxRoboticViaLocationOperation ThirdPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point3, SecondPoint);
        TxRoboticViaLocationOperation FourthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point4, ThirdPoint);
        TxRoboticViaLocationOperation FifthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point5, FourthPoint);
        TxRoboticViaLocationOperation SixthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point6, FifthPoint);
        TxRoboticViaLocationOperation SeventhPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point7, SixthPoint);
        TxRoboticViaLocationOperation EighthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point8, SeventhPoint);
        TxRoboticViaLocationOperation NinthPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point9, EighthPoint);
        
        // Impose a position to the first waypoint		
		TxTransformation rotX = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FirstPoint.AbsoluteLocation = rotX;
		
		var pointA = new TxTransformation(FirstPoint.AbsoluteLocation);
		pointA.Translation = new TxVector(300, -200, 300);
		FirstPoint.AbsoluteLocation = pointA;
		
		// Impose a position to the second waypoint		
		TxTransformation rotX2 = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		SecondPoint.AbsoluteLocation = rotX2;
		
		var pointB = new TxTransformation(SecondPoint.AbsoluteLocation);
		pointB.Translation = new TxVector(325, 0, 450);
		SecondPoint.AbsoluteLocation = pointB;
		
		// Impose a position to the third waypoint			
		if (tool_name == "Gripper 1")
		{
			TxTransformation rotX3 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			ThirdPoint.AbsoluteLocation = rotX3;
			
			var pointC = new TxTransformation(ThirdPoint.AbsoluteLocation);
			pointC.Translation = new TxVector(350, 330, 300);
			ThirdPoint.AbsoluteLocation = pointC;
		}
		else if (tool_name == "Gripper 2")
		{
			TxTransformation rotX3 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			ThirdPoint.AbsoluteLocation = rotX3;
			
			var pointC = new TxTransformation(ThirdPoint.AbsoluteLocation);
			pointC.Translation = new TxVector(500, 330, 300);
			ThirdPoint.AbsoluteLocation = pointC;
		}	
		
		// Impose a position to the fourth waypoint
		if (tool_name == "Gripper 1")
		{
			TxTransformation rotX4 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			FourthPoint.AbsoluteLocation = rotX4;
			
			var pointD = new TxTransformation(FourthPoint.AbsoluteLocation);
			pointD.Translation = new TxVector(350, 330, 180);
			FourthPoint.AbsoluteLocation = pointD;
		}	
		else if (tool_name == "Gripper 2")
		{
			TxTransformation rotX4 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			FourthPoint.AbsoluteLocation = rotX4;
			
			var pointD = new TxTransformation(FourthPoint.AbsoluteLocation);
			pointD.Translation = new TxVector(500, 330, 180);
			FourthPoint.AbsoluteLocation = pointD;
		}	
				
		// Impose a position to the fifth waypoint		
		TxTransformation rotX5 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		FifthPoint.AbsoluteLocation = rotX5;
		
		var pointE = new TxTransformation(FifthPoint.AbsoluteLocation);
		pointE.Translation = new TxVector(420, 125, 180);
		FifthPoint.AbsoluteLocation = pointE;
		
		// Impose a position to the sixth waypoint
		if (tool_name == "Gripper 1")
		{
			TxTransformation rotX6 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			SixthPoint.AbsoluteLocation = rotX6;
			
			var pointF = new TxTransformation(SixthPoint.AbsoluteLocation);
			pointF.Translation = new TxVector(500, 330, 180);
			SixthPoint.AbsoluteLocation = pointF;
		}	
		else if (tool_name == "Gripper 2")
		{
			TxTransformation rotX6 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			SixthPoint.AbsoluteLocation = rotX6;
			
			var pointF = new TxTransformation(SixthPoint.AbsoluteLocation);
			pointF.Translation = new TxVector(350, 330, 180);
			SixthPoint.AbsoluteLocation = pointF;
		}	
		
		// Impose a position to the seventh waypoint
		if (tool_name == "Gripper 1")
		{
			TxTransformation rotX7 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			SeventhPoint.AbsoluteLocation = rotX7;
			
			var pointG = new TxTransformation(SeventhPoint.AbsoluteLocation);
			pointG.Translation = new TxVector(500, 330, 330);
			SeventhPoint.AbsoluteLocation = pointG;
		}	
		else if (tool_name == "Gripper 2")
		{
			TxTransformation rotX7 = new TxTransformation(new TxVector(-Math.PI/2, 0, 0), 
			TxTransformation.TxRotationType.RPY_XYZ);
			SeventhPoint.AbsoluteLocation = rotX7;
			
			var pointG = new TxTransformation(SeventhPoint.AbsoluteLocation);
			pointG.Translation = new TxVector(350, 330, 300);
			SeventhPoint.AbsoluteLocation = pointG;
		}
		
		// Impose a position to the eighth waypoint		
		TxTransformation rotX8 = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		EighthPoint.AbsoluteLocation = rotX8;
		
		var pointH = new TxTransformation(EighthPoint.AbsoluteLocation);
		pointH.Translation = new TxVector(325, 0, 450);
		EighthPoint.AbsoluteLocation = pointH;
		
		// Impose a position to the ninth waypoint		
		TxTransformation rotX9 = new TxTransformation(new TxVector(Math.PI, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		NinthPoint.AbsoluteLocation = rotX9;
		
		var pointI = new TxTransformation(NinthPoint.AbsoluteLocation);
		pointI.Translation = new TxVector(300, -200, 300);
		NinthPoint.AbsoluteLocation = pointI;

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
		
		paramHandler.OnComplexValueChanged("Tool", flange, SixthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, SixthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SixthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SixthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, SixthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, SixthPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, SeventhPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, SeventhPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SeventhPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SeventhPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, SeventhPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, SeventhPoint);
		
		paramHandler.OnComplexValueChanged("Tool", flange, EighthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, EighthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, EighthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, EighthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, EighthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, EighthPoint);
		
		paramHandler.OnComplexValueChanged("Tool", new_tcp, NinthPoint);
		paramHandler.OnComplexValueChanged("Motion Type", motion_type_L, NinthPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, NinthPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, NinthPoint);
		paramHandler.OnComplexValueChanged("Blend", new_blend, NinthPoint);
		paramHandler.OnComplexValueChanged("Coord Type", new_coord, NinthPoint);
		
		// UnMount the current tool in point 4
		ArrayList elements1_unmount = new ArrayList();
		ArrayList elements2_unmount = new ArrayList();
		
		var myCmd1_unmount = new TxRoboticCompositeCommandStringElement("# UnMount");
    	var myCmd11_unmount = new TxRoboticCompositeCommandTxObjectElement(Gripper_mounted);
    	var myCmd12_unmount = new TxRoboticCompositeCommandTxObjectElement(toolframe);		
		var myCmd2_unmount = new TxRoboticCompositeCommandStringElement("# WaitTime 1");
		
		elements1_unmount.Add(myCmd1_unmount);
    	elements1_unmount.Add(myCmd11_unmount);
    	elements1_unmount.Add(myCmd12_unmount);
		elements2_unmount.Add(myCmd2_unmount);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1_unmount =
    	new TxRoboticCompositeCommandCreationData(elements1_unmount);
    	FourthPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1_unmount);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2_unmount =
    	new TxRoboticCompositeCommandCreationData(elements2_unmount);
    	FourthPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2_unmount);
		
		// Add the mounting command and the waittime to point 6
		ArrayList elements1 = new ArrayList();
		ArrayList elements2 = new ArrayList();
		
		var myCmd1 = new TxRoboticCompositeCommandStringElement("# Mount");
    	var myCmd11 = new TxRoboticCompositeCommandTxObjectElement(Gripper_tobe_mounted);
    	var myCmd12 = new TxRoboticCompositeCommandTxObjectElement(toolframe);		
		var myCmd2 = new TxRoboticCompositeCommandStringElement("# WaitTime 1");
		
		elements1.Add(myCmd1);
    	elements1.Add(myCmd11);
    	elements1.Add(myCmd12);
		elements2.Add(myCmd2);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1 =
    	new TxRoboticCompositeCommandCreationData(elements1);
    	SixthPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2 =
    	new TxRoboticCompositeCommandCreationData(elements2);
    	SixthPoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2);
       
        // Some display information
		if (verbose)
		{
			output.Write("The name of the operation is: " + MyOp.Name.ToString() + output.NewLine);
			output.Write("The name of the robot is: " + AssociatedRobot.Name.ToString() + output.NewLine);
			output.Write("The name of the controller is: " + robot.Controller.Name.ToString());
		}
           
    }
}

















