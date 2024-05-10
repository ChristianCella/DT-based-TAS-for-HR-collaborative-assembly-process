/*
This snippet allows to delete an OLP command after a simulation is run.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using System.Collections;

public class Mainscript
{
	public static void DummyOLPCommand()
	{
	
		// Store the Waypoint P2		
		TxRoboticViaLocationOperation Waypoint = TxApplication.ActiveDocument.
		GetObjectsByName("p2")[0] as TxRoboticViaLocationOperation;
		
		// Create the variable MyCmd			
		var MyCmd = new TxRoboticCompositeCommandStringElement("MyCommandToBeErased");
		ArrayList element = new ArrayList();
		element.Add(MyCmd);
		
		TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData =
    	new TxRoboticCompositeCommandCreationData(element);	
    	
    	// Create The command and store it in a TxRoboticCommand variable
    	TxRoboticCommand command = Waypoint.
    	CreateCompositeCommand(txRoboticCompositeCommandCreationData);
    	
    	TxTypeFilter opFilter = new TxTypeFilter(typeof(ITxOperation));
    	TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
    	TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
    	ITxOperation lineSimOp = null;
    	
    	// Scan the list of operations
		foreach(ITxOperation op in allOps)
		{
		
			// If it's the one I want ==> print the name			
			if (op.Name.Equals("MOV_main"))
			{
				lineSimOp = op;
				break; // exit the loop if the condition is satisfied
			}
		}
		
		// If the variable is still null: don't do anything; otherwise:		
		TxSimulationPlayer simPlayer = TxApplication.ActiveDocument.
		SimulationPlayer;
		
		if (lineSimOp != null)
		{
			//TxSimulationPlayer simPlayer = TxApplication.ActiveDocument.
			//SimulationPlayer;
			simPlayer.SetOperation(lineSimOp); // start to simulate the wanted operation
			simPlayer.Play(); // start the simulation		
		}
    	
    	// After the simulation is over, erase all the commands    	
    	simPlayer.Rewind(); // get back to the original position
    	command.Delete();

    	
	}
} 