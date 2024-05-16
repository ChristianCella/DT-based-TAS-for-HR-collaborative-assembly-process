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
    	string selected_name = "Pick&PlaceObject";
    	
    	int posx_pick = 550;
    	int posy_pick = -350;
    	int posz_pick = 0;
    	
    	int posx_place = 400;
    	int posy_place = -200;
    	int posz_place = 0;
    	    	
    	// Initialization variables for the pick and place 	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx();
    	    	
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Get the reference frame of the object		
		TxObjectList refs = TxApplication.ActiveSelection.GetItems();
		refs = TxApplication.ActiveDocument.GetObjectsByName("fr_drill");
		TxFrame fram = refs[0] as TxFrame;
		
		// Apply a certain position to the human and save it in a variable
		human.ApplyPosture("Leaned");
		TxHumanPosture posture_lean = human.GetPosture();
		TxApplication.RefreshDisplay();
		
		human.ApplyPosture("UserHome"); // Re-initialize the human in tne home position
		TxHumanPosture posture_home = human.GetPosture(); 
		TxApplication.RefreshDisplay();
		
		// Get the object for the pick	(Also, refresh the display)	
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("Drill");
		var cube1 = cube_pick[0] as ITxLocatableObject;
		
		var position_pick = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_pick, posy_pick, posz_pick);
		position_pick.RotationRPY_ZYX = new TxVector(0, 0, 0);
									
		TxApplication.RefreshDisplay();
		
		// Decide which hand should grasp the cube as a function of the position of the cube		
		if (posy_pick >= 0) // grasp with right hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.RIGHT_HAND;
    		TxTransformation rightHandTarget = null;
        	taskCreationData.RightHandAutoGrasp = true;
        	rightHandTarget = new TxTransformation();
        	rightHandTarget = (fram as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.RightHandAutoGraspTargetLocation =  rightHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
			TxTransformation leftHandTarget = null;
        	taskCreationData.LeftHandAutoGrasp = true;
        	leftHandTarget = new TxTransformation();
        	leftHandTarget = (fram as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.LeftHandAutoGraspTargetLocation =  leftHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    	}  
    	
    	// Create the simulation  		
    	op = TxHumanTSBSimulationUtilsEx.CreateSimulation(selected_name);
    	
    	// Set the initial context (and force the resimulation)   	
    	op.SetInitialContext();
        op.ForceResimulation();
    	
    	// Create the 'get' task 		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube1;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData.TargetLocation = position_pick;	
		taskCreationData.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask = op.CreateTask(taskCreationData);
		
		// Set the intermediate pose to be reached by the human
		human.SetPosture(posture_lean);		
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTaskInt = op.CreateTask(taskCreationData, tsbGetTask);  		
   		
   		// Set the place position	
   		var position_place = new TxTransformation(cube1.AbsoluteLocation);
		position_place.Translation = new TxVector(posx_place, posy_place, posz_place);
		position_place.RotationRPY_ZYX = new TxVector(0, 0, Math.PI/2);
				
		// Create the 'put' task			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube1;
   		taskCreationData.TargetLocation = position_place;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbPoseTaskInt);
   		
   		// Try to see why the hand goes in the wrong position  		
   		output.Write("x: " + fram.AbsoluteLocation[1, 3] + output.NewLine);
   		output.Write("y: " + fram.AbsoluteLocation[1, 3] + output.NewLine);
   		output.Write("z: " + fram.AbsoluteLocation[2, 3] + output.NewLine);
   		
   		// Create the new target to perform the second 'get' operation
   		var position_place1 = new TxTransformation(cube1.AbsoluteLocation);
		position_place1.Translation = new TxVector(posx_place, posy_place, 100);
		position_place1.RotationRPY_ZYX = new TxVector(Math.PI/2, -Math.PI/2, Math.PI/2);
   		
   		// Create the second 'get' task 		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube1;
		taskCreationData.TargetLocation = position_place;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;
		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
		
		TxTransformation leftHandTarget1 = null;
        taskCreationData.LeftHandAutoGrasp = true;
        leftHandTarget1 = new TxTransformation();
        leftHandTarget1 = position_place1;
        // leftHandTarget1 = (fram as ITxLocatableObject).AbsoluteLocation;
        taskCreationData.LeftHandAutoGraspTargetLocation =  leftHandTarget1 *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
		taskCreationData.KeepUninvolvedHandStill = true;				
		TxHumanTsbTaskOperation tsbGetTask1 = op.CreateTask(taskCreationData, tsbPutTask);
   		
    }
}

