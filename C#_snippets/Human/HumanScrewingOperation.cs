/*
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
    	string selected_name = "ScrewingOperation";
    	
    	int posx_pick_cube = 400;
    	int posy_pick_cube = 300;
    	int posz_pick_cube = 25;
    	
    	int posx_place_cube = 300;
    	int posy_place_cube = 0;
    	int posz_place_cube = 120;
    	
    	int posx_place_cube_screwing = 250;
    	int posy_place_cube_screwing = 0;
    	int posz_place_cube_screwing = 120;
    	
    	int posx_pick_drill = 550;
    	int posy_pick_drill = -350;
    	int posz_pick_drill = 0;
    	
    	int posx_place_drill = 380;
    	int posy_place_drill = 0;
    	int posz_place_drill = 20;
    	    	
    	// Initialization variables for the pick and place 	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData1 = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData2 = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData3 = new TxHumanTSBTaskCreationDataEx();
    	    	
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Apply a certain position to the human and save it in a variable
		human.ApplyPosture("Leaned");
		TxHumanPosture posture_lean = human.GetPosture();
		TxApplication.RefreshDisplay();
		
		human.ApplyPosture("UserHome"); // Re-initialize the human in tne home position
		TxHumanPosture posture_home = human.GetPosture(); 
		TxApplication.RefreshDisplay();
		
		// Get the object for the pick	(Also, refresh the display)	
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube1 = cube_pick[0] as ITxLocatableObject;
		
		var position_pick = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_pick_cube, posy_pick_cube, posz_pick_cube);
		position_pick.RotationRPY_ZYX = new TxVector(0, 0, 0);
		
		// Get the reference frame of the cube		
		TxObjectList ref_frame_cube = TxApplication.ActiveSelection.GetItems();
		ref_frame_cube = TxApplication.ActiveDocument.GetObjectsByName("fr_cube");
		TxFrame frame_cube = ref_frame_cube[0] as TxFrame;
		
		// Get the drill	
		TxObjectList drill_pick = TxApplication.ActiveSelection.GetItems();
		drill_pick = TxApplication.ActiveDocument.GetObjectsByName("Drill");
		var drill = drill_pick[0] as ITxLocatableObject;
		
		var position_pick_drill = new TxTransformation(drill.AbsoluteLocation);
		position_pick_drill.Translation = new TxVector(posx_pick_drill, posy_pick_drill, posz_pick_drill);
		position_pick_drill.RotationRPY_ZYX = new TxVector(0, 0, Math.PI/2);
		
		// Get the reference frame of the cube		
		TxObjectList ref_frame_drill = TxApplication.ActiveSelection.GetItems();
		ref_frame_drill = TxApplication.ActiveDocument.GetObjectsByName("fr_drill");
		TxFrame frame_drill = ref_frame_drill[0] as TxFrame;
									
		TxApplication.RefreshDisplay();
		
		// Decide which hand should grasp the cube as a function of the position of the cube		
		if (posy_pick_cube >= 0) // grasp with right hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.RIGHT_HAND;
    		TxTransformation rightHandTarget = null;
        	taskCreationData.RightHandAutoGrasp = true;
        	rightHandTarget = new TxTransformation();
        	rightHandTarget = (frame_cube as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.RightHandAutoGraspTargetLocation =  rightHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
			TxTransformation leftHandTarget = null;
        	taskCreationData.LeftHandAutoGrasp = true;
        	leftHandTarget = new TxTransformation();
        	leftHandTarget = (frame_cube as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.LeftHandAutoGraspTargetLocation =  leftHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}  
    	
    	// Create the simulation and set the initial context 		
    	op = TxHumanTSBSimulationUtilsEx.CreateSimulation(selected_name);
    	op.SetInitialContext();
        op.ForceResimulation();
        
    	// Create the 'get' task 		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube1;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData.TargetLocation = position_pick;	
		taskCreationData.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask = op.CreateTask(taskCreationData);		
		
		// cache the current location of the object			
		TxTransformation curLoc = cube1.AbsoluteLocation;
		
		// Set the intermediate pose to be reached by the human
		human.SetPosture(posture_lean);		
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTaskInt = op.CreateTask(taskCreationData, tsbGetTask);  		
   		
   		// Set the place position (if you need, also rotate the object)		
   		var position_place = new TxTransformation(cube1.AbsoluteLocation);
		position_place.Translation = new TxVector(posx_place_cube, posy_place_cube, posz_place_cube);
		position_place.RotationRPY_ZYX = new TxVector(-Math.PI/2, 0, 0);
				
		// Create the 'put' task			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube1;
   		taskCreationData.TargetLocation = position_place;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbPoseTaskInt);
   		
   		// Move the object back to it's cached location  			
   		cube1.AbsoluteLocation = curLoc;
   		
   		// Decide which hand should grasp the drill as a function of its position	
   			
		if (posy_pick_drill >= 0) // grasp with right hand
    	{
    		taskCreationData1.Effector = HumanTsbEffector.RIGHT_HAND;
    		TxTransformation rightHandTarget = null;
        	taskCreationData1.RightHandAutoGrasp = true;
        	rightHandTarget = new TxTransformation();
        	rightHandTarget = (frame_drill as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData1.RightHandAutoGraspTargetLocation =  rightHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData1.Effector = HumanTsbEffector.LEFT_HAND;
			TxTransformation leftHandTarget = null;
        	taskCreationData1.LeftHandAutoGrasp = true;
        	leftHandTarget = new TxTransformation();
        	leftHandTarget = (frame_drill as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData1.LeftHandAutoGraspTargetLocation =  leftHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	
    	// Create the 'get' task (for the drill, the first time)	
		taskCreationData1.Human = human;						
		taskCreationData1.PrimaryObject = drill;               			
		taskCreationData1.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData1.TargetLocation = position_pick_drill;	
		taskCreationData1.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask1 = op.CreateTask(taskCreationData1, tsbPutTask);  
		
		// Set the place position for the drill		
   		var position_place_drill = new TxTransformation(drill.AbsoluteLocation);
		position_place_drill.Translation = new TxVector(posx_place_drill, posy_place_drill, posz_place_drill);
		position_place_drill.RotationRPY_ZYX = new TxVector(0, 0, Math.PI/2);
				
		// Create the 'put' task for the drill			
		taskCreationData1.Human = human;
   		taskCreationData1.PrimaryObject = drill;
   		taskCreationData1.TargetLocation = position_place_drill;
   		taskCreationData.KeepUninvolvedHandStill = true;						
   		taskCreationData1.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask1 = op.CreateTask(taskCreationData1, tsbGetTask1);
   		
   		var position_place1 = new TxTransformation(drill.AbsoluteLocation);
		position_place1.Translation = new TxVector(posx_place_drill, posy_place_drill, 100);
		position_place1.RotationRPY_ZYX = new TxVector(Math.PI/2, -Math.PI/2, Math.PI/2);
   		
   		// Create the 'get' task to get the drill once more, before moving the cube		
		taskCreationData3.Human = human;						
		taskCreationData3.PrimaryObject = drill; 
		taskCreationData3.KeepUninvolvedHandStill = true;
		taskCreationData3.Effector = HumanTsbEffector.LEFT_HAND;
		TxTransformation leftHandTarget1 = null;
        taskCreationData3.LeftHandAutoGrasp = true;
        leftHandTarget1 = new TxTransformation();
        leftHandTarget1 = position_place1;
        taskCreationData3.LeftHandAutoGraspTargetLocation =  leftHandTarget1 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);      
        taskCreationData3.TaskType = TsbTaskType.HUMAN_Get;
		//taskCreationData3.TargetLocation = position_place_drill;					
		TxHumanTsbTaskOperation tsbGetTask3 = op.CreateTask(taskCreationData3, tsbPutTask1); 
   		
   		// Set the position of the cube after screwing it	
   		var position_place_screw = new TxTransformation(cube1.AbsoluteLocation);
		position_place_screw.Translation = new TxVector(posx_place_cube_screwing, posy_place_cube_screwing, posz_place_cube_screwing);
		position_place_screw.RotationRPY_ZYX = new TxVector(-Math.PI/2, 0, 0);
   		
   		// Move the cube	
   		taskCreationData2.PrimaryObject = cube1;               
		taskCreationData2.TaskType = TsbTaskType.OBJECT_Move;
		taskCreationData2.TargetLocation = position_place_screw;
		taskCreationData2.KeepUninvolvedHandStill = true;
		taskCreationData2.TaskDuration= 2;
		TxHumanTsbTaskOperation tsbMoveTask = op.CreateTask(taskCreationData2, tsbGetTask3);
		
		     		
		
		
		// Put the drill back to the original position			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = drill;
   		taskCreationData.TargetLocation = position_pick_drill;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask2 = op.CreateTask(taskCreationData, tsbMoveTask);	
   		
   		// Set the correct pose to be reached by the human
		human.SetPosture(posture_home);
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;			
   		TxHumanTsbTaskOperation tsbPoseTask = op.CreateTask(taskCreationData, tsbPutTask2);

    }
}
