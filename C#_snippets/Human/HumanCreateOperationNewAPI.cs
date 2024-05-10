/*
This snippet allows to create an operation of the type 'Pick-and-place' for the human:
	* the user specifies the 'pick' and 'place' locations;
	* the simulation is cancelled (unless the last line ios commented out) in order to avoid to keep creating operations
	* instead of updating an old simulation, every time a change in position has to be made, the old
	simulation is cancelled and a new one is created;
	* the complete 'Pick&Place' task is composed of three phases:
		° 'Get' the object;
		° 'Put' the object;
		° 'Pose' the human back to the original posture;
This script requires the use of the new APIs.
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
    
    public static void Main(ref StringWriter output)
    {
    
    	// Set some control variables   	
    	string selected_name = "Simulation1";
    	
    	int posx_pick = 400;
    	int posy_pick = 300;
    	int posz_pick = 25;
    	
    	int posx_place = 400;
    	int posy_place = -100;
    	int posz_place = 100;
    	
    	double rot_y = Math.PI/2;
    	
    	// Initialization variables for the pick and place   	
    	TxHumanTsbSimulationOperation op = null; 
    	TxHumanTSBTaskCreationDataEx taskCreationData = new TxHumanTSBTaskCreationDataEx();
    	TxHumanTSBTaskCreationDataEx taskCreationData1 = new TxHumanTSBTaskCreationDataEx();

		// Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;

		// Get the human posture
		TxHumanPosture posture = human.GetPosture(); 
		
		// Get the cube for the pick		
		TxObjectList cube_pick = TxApplication.ActiveSelection.GetItems();
		cube_pick = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube1 = cube_pick[0] as ITxLocatableObject;
		
		var position_pick = new TxTransformation(cube1.AbsoluteLocation);
		position_pick.Translation = new TxVector(posx_pick, posy_pick, posz_pick);
		cube1.AbsoluteLocation = position_pick;
		
		TxApplication.RefreshDisplay();
		
		// Decide which hand should grasp the cube as a function of the position of the cube
		
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
		
		// Move (and rotate around y) the object to the 'place' desired location	
		
		TxTransformation rotY = new TxTransformation(new TxVector(0, rot_y, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		cube1.AbsoluteLocation = rotY;
				
		var position_place = new TxTransformation(cube1.AbsoluteLocation);
		position_place.Translation = new TxVector(posx_place, posy_place, posz_place);
		cube1.AbsoluteLocation = position_place;		
		
		// Create the 'put' task			
		taskCreationData.Human = human;
   		taskCreationData.PrimaryObject = cube1;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Put;			
   		TxHumanTsbTaskOperation tsbPutTask = op.CreateTask(taskCreationData, tsbGetTask);
   		
   		// Move the object back to it's cached location  			
   		cube1.AbsoluteLocation = curLoc;

		// Set the correct pose to be reached by the human
		human.SetPosture(posture);
		
		// Create the 'pose' task		
		taskCreationData.Human = human;					
   		taskCreationData.TaskType = TsbTaskType.HUMAN_Pose;	
		taskCreationData.TaskDuration = 0.7;		
   		TxHumanTsbTaskOperation tsbPoseTask = op.CreateTask(taskCreationData, tsbPutTask);
 		
 		var pose_op = tsbPoseTask as ITxOperation;
 		bool answer = pose_op.CanChangeDuration();
 				 	  		    	
    	// Set the initial context (and force the resimulation)   	
    	op.SetInitialContext();
        op.ForceResimulation();
        
        // Display some data       
		output.Write("The name of the operation is: " + op.Name.ToString() + output.NewLine); 
		output.Write("The name of the pose task is: " + tsbPoseTask.Name.ToString() + output.NewLine);
		output.Write("The possibility to chnage the duration is: " + answer.ToString() + output.NewLine);
		
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
		
		// Delete the operation (not necessary)		
		//op.Delete();
    	
    }

}












