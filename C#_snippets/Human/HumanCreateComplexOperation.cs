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
    	string selected_name = "GetDrill";
    	
    	int posx_pick = 400;
    	int posy_pick = 300;
    	int posz_pick = 25;
    	
    	int posx_place = 400;
    	int posy_place = 100;
    	int posz_place = 25;
    	
    	double rot_y = 0;
    	
    	// Initialization variables for the pick and place   	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData1 = new TxHumanTSBTaskCreationDataEx();
    	
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Get the reference frame of the cube		
		TxObjectList refs = TxApplication.ActiveSelection.GetItems();
		refs = TxApplication.ActiveDocument.GetObjectsByName("fr_cube");
		TxFrame fram = refs[0] as TxFrame;
		
		// Apply a certain position to the human and save it in a variable
		human.ApplyPosture("Leaned");
		TxHumanPosture posture_lean = human.GetPosture();
		TxApplication.RefreshDisplay();
		
		human.ApplyPosture("UserHome"); // Re-initialize the human in tne home position
		TxHumanPosture posture_home = human.GetPosture(); 
		TxApplication.RefreshDisplay();
		
		// Get the cube for the pick		
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube1 = cube_pick[0] as ITxLocatableObject;
		
		var position_pick = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_pick, posy_pick, posz_pick);
		cube1.AbsoluteLocation = position_pick;
		
		// Move (and rotate around y) the object to the 'place' desired location							
		
		
		TxApplication.RefreshDisplay();
		
		// Decide which hand should grasp the cube as a function of the position of the cube		
		if (posy_pick >= 0) // grasp with right hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.RIGHT_HAND;
    		/*
    		TxTransformation rightHandTarget = null;
        	taskCreationData.RightHandAutoGrasp = true;
        	rightHandTarget = new TxTransformation();
        	rightHandTarget = (fram as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.RightHandAutoGraspTargetLocation =  rightHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
			*/
    	}
    	else // Grasp with left hand
    	{
    		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
    		/*
			TxTransformation leftHandTarget = null;
        	taskCreationData.LeftHandAutoGrasp = true;
        	leftHandTarget = new TxTransformation();
        	leftHandTarget = (fram as ITxLocatableObject).AbsoluteLocation;
        	taskCreationData.LeftHandAutoGraspTargetLocation =  leftHandTarget *= new TxTransformation(new TxVector(0, 0, 30), TxTransformation.TxTransformationType.Translate);
    		*/
    	}  
    	
    	// Create the simulation  		
    	op = TxHumanTSBSimulationUtilsEx.CreateSimulation(selected_name);
    	
    	// Create the 'get' task 		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube1;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;	
		//taskCreationData.KeepUninvolvedHandStill = true;				
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
   		
   		// Set the place position
   		//var position_place = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_place, posy_place, posz_place);
   		cube1.AbsoluteLocation = position_pick;
		
		output.Write("x: " + position_pick[0, 3].ToString() + output.NewLine);
		output.Write("y: " + position_pick[1, 3].ToString() + output.NewLine);
		output.Write("z: " + position_pick[2, 3].ToString() + output.NewLine);
				
		// Create the 'put' task			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube1;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbPoseTaskInt);
   		
   		// Move the object back to it's cached location  			
   		cube1.AbsoluteLocation = curLoc;
   		
   		// Set the correct pose to be reached by the human
		human.SetPosture(posture_home);
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTask = op.CreateTask(taskCreationData, tsbPutTask);
   		
   		// Set the initial context (and force the resimulation)   	
    	op.SetInitialContext();
        op.ForceResimulation();
    }
}
