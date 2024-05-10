using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class ParallelWorkstation
{
	public static void Main()
	{
	
		// Access the simulation player
		
		TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();
        
        // Obtain all the operations in the scene of type 'TxCompoundOperation'
        
        TxTypeFilter opFilter = new TxTypeFilter(typeof(TxCompoundOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
		TxCompoundOperation firstSimulation = allOps[1] as TxCompoundOperation;
		
		// Display the name of the simulation
		
		TxMessageBox.Show(string.Format(firstSimulation.Name), "Name of the operation", MessageBoxButtons.OK, MessageBoxIcon.Information);
		
		// Set the position of the robot bases
		
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e1");
		var robot1 = selectedObjects[0] as ITxLocatableObject;
		
		TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
		selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("UR5e2");
		var robot2 = selectedObjects1[0] as ITxLocatableObject;
		
		TxMessageBox.Show(string.Format(robot1.Name.ToString()), "First Robot", MessageBoxButtons.OK, MessageBoxIcon.Information);
		TxMessageBox.Show(string.Format(robot2.Name.ToString()), "Second Robot", MessageBoxButtons.OK, MessageBoxIcon.Information);
		
		double rotZrobInit = 0;		
		TxTransformation rotRobInit = new TxTransformation(new TxVector(0, 0, rotZrobInit), 
		TxTransformation.TxRotationType.RPY_XYZ);
		robot1.AbsoluteLocation = rotRobInit;
		
		var positionInit = new TxTransformation(robot1.LocationRelativeToWorkingFrame);
		positionInit.Translation = new TxVector(1000, 0, 0);
		robot1.LocationRelativeToWorkingFrame = positionInit;
		
		// Get and change the positions of the cubes
		
		TxObjectList selectedObjects2 = TxApplication.ActiveSelection.GetItems();
		selectedObjects2 = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var cube1 = selectedObjects2[0] as ITxLocatableObject;
		
		TxObjectList selectedObjects3 = TxApplication.ActiveSelection.GetItems();
		selectedObjects3 = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube2");
		var cube2 = selectedObjects3[0] as ITxLocatableObject;
		
		double rotZcubeInit1 = 0;		
		TxTransformation rotCubeInit1 = new TxTransformation(new TxVector(rotZcubeInit1, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		cube1.AbsoluteLocation = rotCubeInit1;
		
		var positionCubInit1 = new TxTransformation(cube1.LocationRelativeToWorkingFrame);
		positionCubInit1.Translation = new TxVector(1300, -250, 25);
		cube1.LocationRelativeToWorkingFrame = positionCubInit1;
		
		double rotZcubeInit2 = 0;		
		TxTransformation rotCubeInit2 = new TxTransformation(new TxVector(rotZcubeInit2, 0, 0), 
		TxTransformation.TxRotationType.RPY_XYZ);
		cube2.AbsoluteLocation = rotCubeInit2;
		
		var positionCubInit2 = new TxTransformation(cube2.LocationRelativeToWorkingFrame);
		positionCubInit2.Translation = new TxVector(-700, -250, 25);
		cube2.LocationRelativeToWorkingFrame = positionCubInit2;
		
		// Get the points for the pick of the first cube
			
		TxRoboticViaLocationOperation PrePick1 = TxApplication.ActiveDocument.
        GetObjectsByName("PrePickPos1")[1] as TxRoboticViaLocationOperation;
        var prePick1 = new TxTransformation(PrePick1.LocationRelativeToWorkingFrame);
        prePick1.Translation = new TxVector(-700, -250, 100);
        PrePick1.LocationRelativeToWorkingFrame = prePick1;
	
		// Refresh and rewind
		
		player.Rewind();
		TxApplication.RefreshDisplay();
		
		// Play the simulation
		
		player.Play();
		
		// Display the time taken by the simulation
		
		double TaskDuration = firstSimulation.Duration;
		TxMessageBox.Show(string.Format(TaskDuration.ToString()), "Duration", MessageBoxButtons.OK, MessageBoxIcon.Information);
		
		player.Rewind();
	}
}
    
