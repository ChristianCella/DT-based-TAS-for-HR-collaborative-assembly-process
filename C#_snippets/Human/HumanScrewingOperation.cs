/*
This snippet implements a complete operation of screwing. The new APIs allow to grasp objects in a more robust way and also it is possible to specify
the target location for the grasp by passing a reference frame that mut be attached to the object. The sequence of operations is the following:
	- 'get' task to grasp the object;
	- 'pose' task to reach a certain pose (intermediate);
	- 'put' task to place the object in a certain position (maybe also specifying the orientation);
	- 'get' task to grasp the drill;
	- 'put' task to place the drill in a certain position;
	- 'get' task to grasp the drill again (regrasp);
	- 'move' task to move the object in a certain position (screwing-like operation);
	- 'put' task to place the drill back in the original position;
	- 'pose' task to reach the initial pose.
The script is a bit more complex than the previous one, and what is left to do is to see how specify 2 different frames for the two hands foir each object.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using EngineeringInternalExtension;
using Tecnomatix.Engineering.Olp;
using Tecnomatix.Engineering.Plc;
using Tecnomatix.Engineering.Utilities;
using Tecnomatix.Engineering.ModelObjects;
using Jack.Toolkit;
using Jack.Toolkit.TSB;
using scaleParam = Jack.Toolkit.jcAdvancedAnthroScale.input;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
    {   
    	// Set some control variables   	
    	string selected_name = "Attempt";
    	
    	int posx_pick_cube = 400;
    	int posy_pick_cube = -300;
    	int posz_pick_cube = 25;
    	
    	int posx_place_cube = 300;
    	int posy_place_cube = 0;
    	int posz_place_cube = 180;
    	
    	int posx_place_cube_screwing = 250;
    	int posy_place_cube_screwing = 0;
    	int posz_place_cube_screwing = 180;
    	
    	int posx_pick_drill = 550;
    	int posy_pick_drill = 350;
    	int posz_pick_drill = 0;
    	
    	int posx_place_drill = 450;
    	int posy_place_drill = 0;
    	int posz_place_drill = 20;
    	    	
    	// Initialization variables for the pick and place 	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx(); // For the cube
    	TxHumanTSBTaskCreationDataEx taskCreationData1 = new TxHumanTSBTaskCreationDataEx(); // For the pose
    	TxHumanTSBTaskCreationDataEx taskCreationData2 = new TxHumanTSBTaskCreationDataEx(); // For the drill
    	TxHumanTSBTaskCreationDataEx taskCreationData3 = new TxHumanTSBTaskCreationDataEx();
    	
    	// Initialize the target locations
    	TxTransformation rightHandTarget = null;
    	TxTransformation leftHandTarget = null;
    	TxTransformation rightHandTarget2 = null;
    	TxTransformation leftHandTarget2 = null;
    	    	
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Apply a certain position to the human and save it in some variables
		human.ApplyPosture("Leaned");
		TxHumanPosture posture_lean = human.GetPosture();
		TxApplication.RefreshDisplay();
		
		human.ApplyPosture("UserHome"); // Re-initialize the human in tne home position
		TxHumanPosture posture_home = human.GetPosture(); 
		TxApplication.RefreshDisplay();
		
		// Get the object for the pick and specify the initial pick position	
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube = cube_pick[0] as ITxLocatableObject;
		
		var position_pick_cube = new TxTransformation(cube.AbsoluteLocation);
		position_pick_cube.Translation = new TxVector(posx_pick_cube, posy_pick_cube, posz_pick_cube);
		position_pick_cube.RotationRPY_ZYX = new TxVector(0, 0, 0);
		
		// Get the reference frame of the cube		
		TxObjectList ref_frame_cube = TxApplication.ActiveSelection.GetItems();
		ref_frame_cube = TxApplication.ActiveDocument.GetObjectsByName("fr_cube");
		TxFrame frame_cube = ref_frame_cube[0] as TxFrame;
		
		// Get the drill and specify the initial pick psoition	
		TxObjectList drill_pick = TxApplication.ActiveSelection.GetItems();
		drill_pick = TxApplication.ActiveDocument.GetObjectsByName("Drill");
		var drill = drill_pick[0] as ITxLocatableObject;
		
		var position_pick_drill = new TxTransformation(drill.AbsoluteLocation);
		position_pick_drill.Translation = new TxVector(posx_pick_drill, posy_pick_drill, posz_pick_drill);
		position_pick_drill.RotationRPY_ZYX = new TxVector(0, 0, Math.PI/2);
		
		// Get the reference frame of the cube		
		TxObjectList ref_frame_drill = TxApplication.ActiveSelection.GetItems();
		ref_frame_drill = TxApplication.ActiveDocument.GetObjectsByName("fr_drill_right");
		TxFrame frame_drill = ref_frame_drill[0] as TxFrame;
		
		// Refresh the display							
		TxApplication.RefreshDisplay();
		
		// Decide which hand should grasp the object as a function of its position	
		if (posy_pick_cube >= 0) // grasp with right hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.RIGHT_HAND;    		
        	taskCreationData.RightHandAutoGrasp = true;
        	rightHandTarget = new TxTransformation();
        	rightHandTarget = (frame_cube as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.RightHandAutoGraspTargetLocation =  rightHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
        	taskCreationData.LeftHandAutoGrasp = true;
        	leftHandTarget = new TxTransformation();
        	leftHandTarget = (frame_cube as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.LeftHandAutoGraspTargetLocation =  leftHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}  
    	
    	// Create the simulation and set the initial context 		
    	op = TxHumanTSBSimulationUtilsEx.CreateSimulation(selected_name);
    	op.SetInitialContext();
        op.ForceResimulation();
        
    	// Create the first 'get' task 		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData.TargetLocation = position_pick_cube;	
		taskCreationData.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask = op.CreateTask(taskCreationData);
		op.ApplyTask(tsbGetTask, 1);
   		TxApplication.RefreshDisplay();		
		
		// Set the intermediate pose to be reached by the human and create the 'pose' task
		human.SetPosture(posture_lean);			
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTaskInt = op.CreateTask(taskCreationData, tsbGetTask); 
   		op.ApplyTask(tsbPoseTaskInt, 1);
   		TxApplication.RefreshDisplay(); 		
   		
   		// Set the place position associated to the first 'get' task		
   		var position_place_cube = new TxTransformation(cube.AbsoluteLocation);
		position_place_cube.Translation = new TxVector(posx_place_cube, posy_place_cube, posz_place_cube);
		position_place_cube.RotationRPY_ZYX = new TxVector(Math.PI/2, 0, 0);
				
		// Create the first 'put' task (prepare the object for a screwing operation)		
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube;
   		taskCreationData.TargetLocation = position_place_cube;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbPoseTaskInt);
   		op.ApplyTask(tsbPutTask, 1);
   		TxApplication.RefreshDisplay();
   		
   		// Decide which hand should grasp the drill as a function of its position	  			
		if (posy_pick_drill >= 0) // grasp with right hand
    	{
    		taskCreationData1.Effector = HumanTsbEffector.RIGHT_HAND;
        	taskCreationData1.RightHandAutoGrasp = true;
        	rightHandTarget2 = new TxTransformation();
        	rightHandTarget2 = (frame_drill as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData1.RightHandAutoGraspTargetLocation =  rightHandTarget2 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData1.Effector = HumanTsbEffector.LEFT_HAND;
        	taskCreationData1.LeftHandAutoGrasp = true;
        	leftHandTarget2 = new TxTransformation();
        	leftHandTarget2 = (frame_drill as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData1.LeftHandAutoGraspTargetLocation =  leftHandTarget2 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	
    	// Create the 'get' task for the drill	
		taskCreationData1.Human = human;						
		taskCreationData1.PrimaryObject = drill;               			
		taskCreationData1.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData1.TargetLocation = position_pick_drill;	
		taskCreationData1.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask1 = op.CreateTask(taskCreationData1, tsbPutTask); 
		op.ApplyTask(tsbGetTask1, 1);
   		TxApplication.RefreshDisplay(); 
		
		// Set the position to place the drill	
   		var position_place_drill = new TxTransformation(drill.AbsoluteLocation);
		position_place_drill.Translation = new TxVector(posx_place_drill, posy_place_drill, posz_place_drill);
		position_place_drill.RotationRPY_ZYX = new TxVector(0, 0, Math.PI/2);
				
		// Create the 'put' task for the drill			
   		taskCreationData1.PrimaryObject = drill;
   		taskCreationData1.TargetLocation = position_place_drill;
   		taskCreationData1.KeepUninvolvedHandStill = true;						
   		taskCreationData1.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask1 = op.CreateTask(taskCreationData1, tsbGetTask1);
   		op.ApplyTask(tsbPutTask1, 1);
   		TxApplication.RefreshDisplay();
   		
   		
   		// Create the new 'get' task to get the drill once more ('Immediate' regrasp), before moving the cube			
		if (leftHandTarget2 != null & rightHandTarget2 == null)
		{
			leftHandTarget2 = (frame_drill as ITxLocatableObject).AbsoluteLocation;
			taskCreationData1.LeftHandAutoGraspTargetLocation =  leftHandTarget2 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
		}
		else if (leftHandTarget2 == null & rightHandTarget2 != null)
		{
			rightHandTarget2 = (frame_drill as ITxLocatableObject).AbsoluteLocation;
			taskCreationData1.RightHandAutoGraspTargetLocation =  rightHandTarget2 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
		}			
		taskCreationData1.PrimaryObject = drill; 
        taskCreationData1.TaskType = TsbTaskType.HUMAN_Get;				
		TxHumanTsbTaskOperation tsbGetTask2 = op.CreateTask(taskCreationData1, tsbPutTask1);
		op.ApplyTask(tsbGetTask2, 1);
   		TxApplication.RefreshDisplay(); 
   		
   		// Set the position of the cube after screwing it (target location for the 'move' task)	
   		var position_place_screw = new TxTransformation(cube.AbsoluteLocation);
		position_place_screw.Translation = new TxVector(posx_place_cube_screwing, posy_place_cube_screwing, posz_place_cube_screwing);
		position_place_screw.RotationRPY_ZYX = new TxVector(Math.PI/2, 0, 0);
   		
   		// Move the cube	
   		taskCreationData2.PrimaryObject = cube;               
		taskCreationData2.TaskType = TsbTaskType.OBJECT_Move;
		taskCreationData2.TargetLocation = position_place_screw;
		taskCreationData2.KeepUninvolvedHandStill = true;
		taskCreationData2.TaskDuration= 1;
		TxHumanTsbTaskOperation tsbMoveTask = op.CreateTask(taskCreationData2, tsbGetTask2);
		op.ApplyTask(tsbMoveTask, 1);
   		TxApplication.RefreshDisplay();
		     				
		// Put the drill back to the original position (the drill has already been re-grasped)		
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = drill;
   		taskCreationData.TargetLocation = position_pick_drill;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask2 = op.CreateTask(taskCreationData, tsbMoveTask);
   		op.ApplyTask(tsbPutTask2, 1);
   		TxApplication.RefreshDisplay();	
   		
   		// Set the correct pose to be reached by the human and create the final 'pose' task
		human.SetPosture(posture_home);	
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;			
   		TxHumanTsbTaskOperation tsbPoseTask = op.CreateTask(taskCreationData, tsbPutTask2);
    }
}

