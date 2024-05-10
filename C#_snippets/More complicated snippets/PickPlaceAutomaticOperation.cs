/*
This code is not a snippet: it is composed of a void procedure (main) that creates a PickAndPlace operation for a specific robot.
All the required points are created by calling class-specific methods and the operation is saved in the document.
(This code relies a lot on the snippet 'CreateARobotProgramFromScratch.cs')
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
using System.Threading.Tasks;
using System.Threading;

public class MainScript
{

    // Define the parameters for the operation
    static string new_tcp = "tcp_1";
    static string new_motion_type = "MoveL";
    static string new_speed = "1000";
    static string new_accel = "1200";
    static string new_blend = "0"; // This parameter will be changed if the point is a waypoint
    static string new_coord = "Cartesian";

    static bool verbose = true;

    public static void MainWithOutput(ref StringWriter output)
    {
        // Number of points to be created (if you do not want to add waypoints, set n = 8)
        int n = 9;

        // Maximum number of points: 8 (standard) + 3 (possible collisions)
        int maxn = 11;

    	// Get the robot    	
    	TxObjectList objects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	var robot = objects[0] as TxRobot;

        // Store the gripper "Camozzi gripper UR5e" 	
  		ITxObject Gripper = TxApplication.ActiveDocument.
		GetObjectsByName("Camozzi Gripper UR5e")[0] as TxGripper;

        // Task in which the collision will be detected (To be defined by an external script)
        string collisiontask = "PrePlacePosYAOSC_cube";
        // string collisiontask = "RobotHomeFinishcube3";
        // string collisiontask = "PrePickPoscube3";

  		// Store the pose "Gripper Closed" 		
  		ITxObject PoseClose = TxApplication.ActiveDocument.
		GetObjectsByName("CLOSE")[0] as TxPose;

		// Store the pose "Gripper Open" 		
  		ITxObject PoseOpen = TxApplication.ActiveDocument.
		GetObjectsByName("OPEN")[0] as TxPose;
  		
  		// Store the reference frame "tgripper_tf" 		
  		ITxObject tGripper = TxApplication.ActiveDocument.
		GetObjectsByName("tf_tcp_1")[0] as TxFrame;

        // Object to be picked
        TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
        selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube");
        var Cube = selectedObjects[0] as ITxLocatableObject;
        var cube_name = Cube.Name; // Save the name of the object

        // Define the pick point and the place point
        var pick_point = new TxVector(Cube.LocationRelativeToWorkingFrame.Translation); 
        var place_point = new TxVector (600, 450, 25);

        // Offset for the pick and place
        var pnpoffset = new TxVector(0, 0, 100);

        //Define the home position 
        var home_point = new TxVector (301, -133, 290);
        
        //Create an array with the general name of the points
        string[] genPointNames = new string[maxn];

        // assign the name of the points
        genPointNames[0] = "RobotHomeStart";
        genPointNames[1] = "PrePickPos";
        genPointNames[2] = "PointPickPos";
        genPointNames[3] = "PostPickPos";
        genPointNames[4] = "PrePlacePos";
        genPointNames[5] = "PointPlacePos";
        genPointNames[6] = "PostPlacePos";
        genPointNames[7] = "RobotHomeFinish";
        genPointNames[8] = "PlaceWaypoint";
        genPointNames[9] = "HomeWaypoint";
        genPointNames[10] = "PickWaypoint";

        // Create the operation (call the method CreatePickPoseOperation)
        CreatePickPoseOperation(n, robot, pick_point, place_point, home_point, pnpoffset, collisiontask, PoseOpen, 
                                PoseClose, Gripper, tGripper, cube_name, genPointNames, ref output);

    }

    // Method to create Pick&Place operations
    public static void CreatePickPoseOperation(int n, TxRobot robot, TxVector pick_point, TxVector place_point, TxVector home_point, 
                                                TxVector pnpoffset, string collisiontask, ITxObject PoseOpen, ITxObject PoseClose, 
                                                ITxObject Gripper, ITxObject tGripper, string cube_name, string[] genPointNames, ref StringWriter output)
    {

        /*Define an hypothetical waypoint
            * The x and y coordinate will be in the middle point between the pick and place point
            * The z coordinate will be 4 times the offset over the z coordinate of the pick point
        */ 
        var waypoint = new TxVector(((pick_point.X + place_point.X) / 2) - 50, (pick_point.Y + place_point.Y) / 2, pick_point.Z + 3 * pnpoffset.Z);
        var return_home_waypoint = new TxVector((home_point.X + place_point.X) / 2, (home_point.Y + place_point.Y) / 2, home_point.Z +  pnpoffset.Z);
        var home_waypoint = new TxVector((home_point.X + pick_point.X) / 2, (home_point.Y + pick_point.Y) / 2, home_point.Z + pnpoffset.Z);

		// Create the name for the operation: it will be PickPos + the name of the selected object
        string Opname = "PickPos" + cube_name;

        // Create the array of the points
        string[] pointNames = new string[n];     

        // Decide the number of points to be created

        if (n >= 8) {// In this part, you enter both if you need striclty 8 points and if you need more than 8 points
            pointNames[0] = genPointNames[0] + cube_name;
            pointNames[1] = genPointNames[1] + cube_name;
            pointNames[2] = genPointNames[2] + cube_name;
            pointNames[3] = genPointNames[3] + cube_name;
            pointNames[4] = genPointNames[4] + cube_name;
            pointNames[5] = genPointNames[5] + cube_name;
            pointNames[6] = genPointNames[6] + cube_name;
            pointNames[7] = genPointNames[7] + cube_name;
        }

        if (n > 8) { // In this part, you enter only if you need more than 8 points

            // Case in which the collision is between the pick and place point
            if ( collisiontask == genPointNames[4] + cube_name) 
            {
                pointNames[8] = genPointNames[7] + cube_name;
                pointNames[7] = genPointNames[6] + cube_name;
                pointNames[6] = genPointNames[5]+ cube_name;
                pointNames[5] = genPointNames[4] + cube_name;
                pointNames[4] = genPointNames[8] + cube_name;
            }
            // Case in which the collision is between the place point and the home point
            else if (collisiontask == genPointNames[7] + cube_name)
            {
                pointNames[8] = genPointNames[7] + cube_name;
                pointNames[7] = genPointNames[9] + cube_name;
            }
            // Case in which the collision is between the home point and the pick point
            else if (collisiontask == genPointNames[1] + cube_name)
            {
                pointNames[8] = genPointNames[7] + cube_name;
                pointNames[7] = genPointNames[6] + cube_name;
                pointNames[6] = genPointNames[5] + cube_name;
                pointNames[5] = genPointNames[4] + cube_name;
                pointNames[4] = genPointNames[3] + cube_name;
                pointNames[3] = genPointNames[2] + cube_name;
                pointNames[2] = genPointNames[1] + cube_name;
                pointNames[1] = genPointNames[10] + cube_name;
            }
        }

		// Create the new operation with the name "Opname"

		TxContinuousRoboticOperationCreationData data = new TxContinuousRoboticOperationCreationData(Opname);
		TxApplication.ActiveDocument.OperationRoot.CreateContinuousRoboticOperation(data);
    	
        // Save the created operation in a variable (TODO: save the complete robotic operation by addressing its name)
    	TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
        TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;

 		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
        TxContinuousRoboticOperation MyOp = allOps[0] as TxContinuousRoboticOperation;
	
		// Implement the logic to access the parameters of the virtual controller		
		TxOlpControllerUtilities ControllerUtils = new TxOlpControllerUtilities();

		MyOp.Robot = robot; // Associate the robot to the operation (fundamental for the simulation)
		TxRobot AssociatedRobot = ControllerUtils.GetRobot(MyOp); // Verify that the robot is associated
				
		ITxOlpRobotControllerParametersHandler paramHandler = (ITxOlpRobotControllerParametersHandler)
		ControllerUtils.GetInterfaceImplementationFromController(robot.Controller.Name,
		typeof(ITxOlpRobotControllerParametersHandler), typeof(TxRobotSimulationControllerAttribute),
		"ControllerName");

        // Display some information about the operation
        if (verbose)
        {
            output.Write("The name of the operation is: " + Opname + output.NewLine);
            output.Write("The name of the robot is: " + AssociatedRobot.Name.ToString() + output.NewLine);
            output.Write("The name of the controller is: " + robot.Controller.Name.ToString());
        }
        
		//Add the points in pointNames to the operation
        var addpoint = new TxVector(0, 0, 0);

        foreach (string pointName in pointNames) // Scan the vector of needed waypoints
        {
            // if the pointName-th element of pointNames is equal to the name of a specific point present in genPointNames ==> Add the point (call the custom method)
            if (pointName.Contains(genPointNames[0])) 
            {
                // The home position is fixed
                TxRoboticViaLocationOperationCreationData NewPoint1 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint1.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew1 = MyOp.CreateRoboticViaLocationOperation(NewPoint1);
                addpoint = home_point;
                PointNew1 = addPoint(PointNew1, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                    paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }

            // if you need to add a waypoint between the home and the pick point ==> Add the point
            else if (pointName.Contains(genPointNames[10]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint11 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint11.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew11 = MyOp.CreateRoboticViaLocationOperation(NewPoint11);
            	addpoint = home_waypoint;
                PointNew11 = addPoint(PointNew11, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[1]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint2 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint2.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew2 = MyOp.CreateRoboticViaLocationOperation(NewPoint2);
            	addpoint = pick_point + pnpoffset;
                PointNew2 = addPoint(PointNew2, addpoint,  new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                    paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[2]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint3 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint3.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew3 = MyOp.CreateRoboticViaLocationOperation(NewPoint3);
            	addpoint = pick_point ;
                PointNew3 = addPoint(PointNew3, addpoint,  new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler,  ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[3]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint4 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint4.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew4 = MyOp.CreateRoboticViaLocationOperation(NewPoint4);
            	addpoint= pick_point + pnpoffset;
                PointNew4 = addPoint(PointNew4, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[8]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint9 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint9.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew9 = MyOp.CreateRoboticViaLocationOperation(NewPoint9);
            	addpoint = waypoint;
                PointNew9 = addPoint(PointNew9, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen  , PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[4]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint5 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint5.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew5 = MyOp.CreateRoboticViaLocationOperation(NewPoint5);
            	addpoint = place_point + pnpoffset;
                PointNew5 = addPoint(PointNew5, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[5]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint6 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint6.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew6 = MyOp.CreateRoboticViaLocationOperation(NewPoint6);
            	addpoint = place_point;
                PointNew6 = addPoint(PointNew6, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[6]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint7 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint7.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew7 = MyOp.CreateRoboticViaLocationOperation(NewPoint7);
            	addpoint = place_point + pnpoffset;
                PointNew7 = addPoint(PointNew7, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            else if (pointName.Contains(genPointNames[9]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint10 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint10.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew10 = MyOp.CreateRoboticViaLocationOperation(NewPoint10);
            	addpoint = return_home_waypoint;
                PointNew10 = addPoint(PointNew10, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);  
            }
            
            else if (pointName.Contains(genPointNames[7]))
            {
                TxRoboticViaLocationOperationCreationData NewPoint8 = new TxRoboticViaLocationOperationCreationData();
        	    NewPoint8.Name = pointName;
        	    TxRoboticViaLocationOperation PointNew8 = MyOp.CreateRoboticViaLocationOperation(NewPoint8);
            	addpoint = home_point;
                PointNew8 = addPoint(PointNew8, addpoint, new_tcp, new_motion_type, new_speed, new_accel, new_blend, new_coord, 
                                        paramHandler, ref output, PoseOpen, PoseClose, Gripper, tGripper, cube_name, genPointNames);
            }
            
        }
    }


    // Method to add a point to the operation
    public static TxRoboticViaLocationOperation addPoint(TxRoboticViaLocationOperation PointNew, TxVector point, string new_tcp, string new_motion_type, 
                                                            string new_speed, string new_accel, string new_blend, string new_coord, 
                                                            ITxOlpRobotControllerParametersHandler paramHandler, ref StringWriter output, 
                                                            ITxObject PoseOpen, ITxObject PoseClose, ITxObject Gripper, ITxObject tGripper, 
                                                            string cube_name, string[] genPointNames)
    {
        // Rotate and translate the point
        TxTransformation rotX = new TxTransformation(new TxVector(Math.PI, 0, 0), TxTransformation.TxRotationType.RPY_XYZ);
        PointNew.AbsoluteLocation = rotX;

        var pointToAdd = new TxTransformation(PointNew.AbsoluteLocation);
        pointToAdd.Translation = point; 
        PointNew.AbsoluteLocation = pointToAdd;

        /*
        Checks for the need of OLP commands: 
            * if the point name contains 'pointpickpos' (genPointNames[2]) ==> close the gripper
            * if the point name contains 'PointPlacePos' (genPointNames[5]) ==> open the gripper
        In both of the cases, a custom method is called to add the commands to the operation      
        */
        bool close;

        if (PointNew.Name.Contains(genPointNames[2]))
        {
            close = true;
            gripperfunction (PointNew, close, PoseClose, PoseOpen, Gripper, tGripper, cube_name, genPointNames);
        }
        
        else if (PointNew.Name.Contains(genPointNames[5]))
        {
            close = false;
            gripperfunction (PointNew, close , PoseClose, PoseOpen, Gripper, tGripper, cube_name, genPointNames);
        }

        /*
        Check if the point is a waypoint
            * if it is, the blend is set to 100
        */
        if (PointNew.Name.Contains("Waypoint"))
        {
            new_blend = "100";
        }

        // Set the parameters of the point by accessing the virtual controller
        paramHandler.OnComplexValueChanged("Tool", new_tcp, PointNew);
        paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, PointNew);
        paramHandler.OnComplexValueChanged("Speed", new_speed, PointNew);
        paramHandler.OnComplexValueChanged("Accel", new_accel, PointNew);
        paramHandler.OnComplexValueChanged("Blend", new_blend, PointNew);
        paramHandler.OnComplexValueChanged("Coord Type", new_coord, PointNew);

        return PointNew;
    }
    
    // Custom method to associate OLP commands to a waypoint
    public static void gripperfunction (TxRoboticViaLocationOperation PointNew, bool close, ITxObject PoseClose, ITxObject PoseOpen, ITxObject Gripper, ITxObject tGripper, string cube_name, string[] genPointNames)
    {
        // Initialize (to null) some variables
        TxRoboticViaLocationOperation Waypoint;
        TxPose Pose;

        /*
        Add OLP commands:
            * if the point is a pick point, close the gripper
            * if the point is a place point, open the gripper
        */
		if (close == true)
        {
            Waypoint =  TxApplication.ActiveDocument.
            GetObjectsByName(genPointNames[2] + cube_name)[0] as TxRoboticViaLocationOperation;
            Pose = PoseClose as TxPose;
        }
        
        else
        {
            Waypoint =  TxApplication.ActiveDocument.
            GetObjectsByName(genPointNames[5] + cube_name)[0] as TxRoboticViaLocationOperation;
            Pose = PoseOpen as TxPose;
        }
		
		// Standard set of commands to create an OLP command
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

        // Watch out: the syntax changes a bit if the gripper is closed or opened
        if (close == false)
        {
            myCmd5 = new TxRoboticCompositeCommandStringElement("# Release");
            myCmd51 = new TxRoboticCompositeCommandTxObjectElement(tGripper);
        }

        // First sub-command
    	elements1.Add(myCmd1);
    	elements1.Add(myCmd11);
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1 =
    	new TxRoboticCompositeCommandCreationData(elements1);	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1);
    	
        // Second sub-command
    	elements2.Add(myCmd2);
    	elements2.Add(myCmd21);
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2 =
    	new TxRoboticCompositeCommandCreationData(elements2);	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2);
    	
        // Third sub-command
    	elements3.Add(myCmd3);
    	elements3.Add(myCmd31);
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData3 =
    	new TxRoboticCompositeCommandCreationData(elements3);	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData3);
    	
        // Fourth sub-command
    	elements4.Add(myCmd4);
    	elements4.Add(myCmd41);
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData4 =
    	new TxRoboticCompositeCommandCreationData(elements4);	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData4);
    	
        // Fifth sub-command
    	elements5.Add(myCmd5);
    	elements5.Add(myCmd51);    	
    	TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData5 =
    	new TxRoboticCompositeCommandCreationData(elements5);	
    	Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData5);

    }
    
}
