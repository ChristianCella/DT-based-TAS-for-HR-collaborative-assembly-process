// Copyright 2019 Siemens Industry Software Ltd.
using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;

public class MainScript
{

    static StringWriter m_output;
    static int reduction_perc = 50;

    public static void MainWithOutput(ref StringWriter output)
    {

        // Access the simulation player
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();

        // Access the robot item           
        TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
        selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
        var robot = selectedObjects1[0] as TxRobot;
		
        // Check if the simulation is running and access each time step (look at 'ScaleVelocityBasedOnTime.cs' 
        // for more details)
        if (!player.IsSimulationRunning())
        {

            // Specify the variable for the display
            m_output = output;

            // Subscribe to the event 'TimeIntervalReached'
            player.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            
            // Perform the simulation at the current time step           
            player.Play();

            // Un-subscribe to the event 'TimeIntervalReached'
            player.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            
            // Reset the speed at 100% of the initial value set in the robot program          
            SetSpeed(100);

            // Rewind the simulation
            player.Rewind();
           
        }

        // Access the current operation ('TxContinuousRoboticOperation'')       
        TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
		TxContinuousRoboticOperation lineSimOp = allOps[0] as TxContinuousRoboticOperation;
        
        // Evaluate the time of the simulation and display it
        double dDuration = 0.0;
        dDuration = lineSimOp.Duration;     
        string durationTime = dDuration.ToString();        
        TxMessageBox.Show(string.Format(durationTime), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
           
    }

    // Static method to stop at the desired time
    private static void player_TimeIntervalReached(object sender, TxSimulationPlayer_TimeIntervalReachedEventArgs args)
    {
    
    	// Store the instance of a dimension (homogeneous matrix)
		TxLinearDimension dim = TxApplication.ActiveDocument.GetObjectsByName("Dim")[0] as TxLinearDimension;
		
		// Get the first object (TCP frame, in this case)		
		ITxLocatableObject Obj1 = dim.FirstObject;
		TxTransformation HomMat1 = Obj1.LocationRelativeToWorkingFrame;
		
		// Get the second object (robot base frame, in this case)		
		ITxLocatableObject Obj2 = dim.SecondObject;
		TxTransformation HomMat2 = Obj2.LocationRelativeToWorkingFrame;
		
		// Calculate the norm of the distance between the two objects		
		double x_dim1 = HomMat1[0, 3];
		double y_dim1 = HomMat1[1, 3];
		double z_dim1 = HomMat1[2, 3];
		
		double x_dim2 = HomMat2[0, 3];
		double y_dim2 = HomMat2[1, 3];
		double z_dim2 = HomMat2[2, 3];
		
		double x_dim = x_dim2 - x_dim1;
		double y_dim = y_dim2 - y_dim1;
		double z_dim = z_dim2 - z_dim1;
		
		double DimNorm = Math.Sqrt((x_dim * x_dim) + (y_dim * y_dim) + (z_dim * z_dim));
		
        // Display the current value of the norm
        m_output.Write(DimNorm.ToString() + m_output.NewLine);
        
        // Scale the speed of the robot based on the distance
        if (DimNorm <= 410)
        {
        	
            SetSpeed(reduction_perc);
        }
        else // For all other distances
        {

        	SetSpeed(100);
        }

    }
    
    // Static method to set the speed (look at 'ScaleVelocityBasedOnTime.cs' for more details)  
    private static void SetSpeed(int value)
    {
    	
        TxRoboticIntParam intParam = new TxRoboticIntParam("REDUCE_SPEED", value);
		TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
		selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		var robot = selectedObjects1[0] as TxRobot;
		robot.SetParameter(intParam);
    }
}





