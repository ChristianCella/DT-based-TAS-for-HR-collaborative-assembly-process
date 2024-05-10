/*
This code allows to insert an intermediate pose between the 'get' and the 'put' tasks.
Basically, it's very similar to 'HumanCreateOperationNewAPI'.
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
    	string selected_name = "Pick&Place";
    	
    	int posx_pick = 400;
    	int posy_pick = 300;
    	int posz_pick = 25;
    	
    	int posx_place = 400;
    	int posy_place = -100;
    	int posz_place = 100;
    	
    	
    	// Initialization variables for the pick and place   	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData1 = new TxHumanTSBTaskCreationDataEx();
    
    	// Get the human and their initial posture	
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		TxHumanPosture posture = human.GetPosture();
		
		// Get the cube for the pick and impose the initial location		
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube1 = cube_pick[0] as ITxLocatableObject;
		
		var position_pick = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_pick, posy_pick, posz_pick);
		cube1.AbsoluteLocation = position_pick;
		
		TxApplication.RefreshDisplay();
		
		// Choose the hand
		if (posy_pick >= 0)
    	{
    		taskCreationData.Effector = HumanTsbEffector.RIGHT_HAND;
    	}
    	else
    	{
    		taskCreationData.Effector = HumanTsbEffector.LEFT_HAND;
    	}  	
    		
    	// Create the simulation  		
    	op = TxHumanTSBSimulationUtilsEx.CreateSimulation(selected_name);
    	
    	// Create the 'get' task   		
		taskCreationData.Human = human;						
		taskCreationData.PrimaryObject = cube1;               			
		taskCreationData.TaskType = TsbTaskType.HUMAN_Get;			
		TxHumanTsbTaskOperation tsbGetTask = op.CreateTask(taskCreationData);
		
		// cache the current location of the object			
		TxTransformation curLoc = cube1.AbsoluteLocation;
		
		// Come back to the original pose (more 'natural' motion)		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTask = op.CreateTask(taskCreationData, tsbGetTask);
		
 		// Move the object to the 'place' desired location	
		var position_place = new TxTransformation(cube1.AbsoluteLocation);
		position_place.Translation = new TxVector(posx_place, posy_place, posz_place);
		cube1.AbsoluteLocation = position_place;		
		
		// Create the 'put' task			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube1;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbPoseTask);
   		
   		// Move the object back to it's cached location  			
   		cube1.AbsoluteLocation = curLoc;
   		
   		// Set the correct pose to be reached by the human (equal to the initial one)
		human.SetPosture(posture);
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPose1Task = op.CreateTask(taskCreationData, tsbPutTask);
 						 	  		    	
    	// Set the initial context (and force the resimulation)   	
    	op.SetInitialContext();
        op.ForceResimulation();
		
		// Set the desired operation as the current operation (the one just created)		
		var sim = TxApplication.ActiveDocument.OperationRoot.GetAllDescendants(new 
        TxTypeFilter(typeof(TxHumanTsbSimulationOperation))).FirstOrDefault(x => x.Name.Equals(selected_name)) as 
        TxHumanTsbSimulationOperation;     
        TxApplication.ActiveDocument.CurrentOperation = sim;
        
        // Access the simulation player		
		TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();
    	player.Play();
    	
    	// Refresh and Rewind (The rewind is not mandatory)   	
    	TxApplication.RefreshDisplay();  
    	player.Rewind();
    	
    	// Display a message    	
    	TxMessageBox.Show(string.Format("The operation is finihsed!"), "Final message", 
		MessageBoxButtons.OK, MessageBoxIcon.Information);
		
		// Delete the operation (if necessary)		
		//op.Delete();
    }
}
