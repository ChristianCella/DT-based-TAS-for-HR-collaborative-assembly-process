using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;
using Tecnomatix.Engineering.Plc;
using Tecnomatix.Engineering.Utilities;
using Tecnomatix.Engineering.ModelObjects;
using System.Windows.Forms;

public class MainScript
{
    
    public static void MainWithOutput()
    {
    
    	// Variables defining the x, y, z translations of the frames   	
    	double x_point1 = 450;
    	double y_point1 = 300;
    	double z_point1 = 25;
    	
    	double x_point2 = 500;
    	double y_point2 = -300;
    	double z_point2 = 25;
    	
    	double x_point3 = (x_point1 + x_point2) / 2;
    	double y_point3 = (y_point1 + y_point2) / 2;
    	double z_point3 = z_point1 + 40;
    	
    	bool verbose = false;
    
    	// Get the frame of the human hand and the three points to be reached					
		TxObjectList first_point = TxApplication.ActiveDocument.GetObjectsByName("P1");
		var first_pos = first_point[0] as ITxLocatableObject;
		var first_position = new TxTransformation(first_pos.LocationRelativeToWorkingFrame);
		
		TxObjectList second_point = TxApplication.ActiveDocument.GetObjectsByName("P2");
		var second_pos = second_point[0] as ITxLocatableObject;
		var second_position = new TxTransformation(second_pos.LocationRelativeToWorkingFrame);
		
		TxObjectList third_point = TxApplication.ActiveDocument.GetObjectsByName("P3");
		var third_pos = third_point[0] as ITxLocatableObject;
		var third_position = new TxTransformation(third_pos.LocationRelativeToWorkingFrame);
		
		// Move objects on the table		
		TxObjectList selectedObjects1 = TxApplication.ActiveSelection.GetItems();
		selectedObjects1 = TxApplication.ActiveDocument.GetObjectsByName("YAOSC_cube1");
		var Cube = selectedObjects1[0] as ITxLocatableObject;
		
		var pickpos = new TxTransformation(Cube.LocationRelativeToWorkingFrame);
		pickpos.Translation = new TxVector(x_point1, y_point1, z_point1);
		Cube.LocationRelativeToWorkingFrame = pickpos;
		
		var placepos = new TxTransformation(third_pos.LocationRelativeToWorkingFrame);
		placepos.Translation = new TxVector(x_point2, y_point2, z_point2);
		third_pos.LocationRelativeToWorkingFrame = placepos;
		
		var interpos = new TxTransformation(second_pos.LocationRelativeToWorkingFrame);
		interpos.Translation = new TxVector(x_point3, y_point3, z_point3);
		second_pos.LocationRelativeToWorkingFrame = interpos;
		
		// calculate distances
		
			// human hand
		
		double x0 = 714.5;
		double y0 = 119.03;
		double z0 = 130.85;
		
			// grasp point
		
		double x1 = first_position[0, 3];
		double y1 = first_position[1, 3];
		double z1 = first_position[2, 3];
		
			// intermediate point
		
		double x2 = second_position[0, 3];
		double y2 = second_position[1, 3];
		double z2 = second_position[2, 3];
		
			// final point
		
		double x3 = third_position[0, 3];
		double y3 = third_position[1, 3];
		double z3 = third_position[2, 3];
		
		// Deltas
		
			// First dimension
		
		double dimx_1 = x0 - x1;
		double dimy_1 = y0 - y1;
		double dimz_1 = z0 - z1; 
		double dim1_norm = Math.Sqrt((dimx_1 * dimx_1) + (dimy_1 * dimy_1) + (dimz_1 * dimz_1));
		
			// Second dimension
		
		double dimx_2 = x1 - x2;
		double dimy_2 = y1 - y2;
		double dimz_2 = z1 - z2; 
		double dim2_norm = Math.Sqrt((dimx_2 * dimx_2) + (dimy_2 * dimy_2) + (dimz_2 * dimz_2));
		
			// Third dimension
		
		double dimx_3 = x2 - x3;
		double dimy_3 = y2 - y3;
		double dimz_3 = z2 - z3; 
		double dim3_norm = Math.Sqrt((dimx_3 * dimx_3) + (dimy_3 * dimy_3) + (dimz_3 * dimz_3));
		
		// Display useful information
		
		//TxMessageBox.Show(string.Format(hand_position.ToString()), "First distance", 
		//MessageBoxButtons.OK, MessageBoxIcon.Information);
		if (verbose)
		
		{
		
			TxMessageBox.Show(string.Format(dim1_norm.ToString()), "First distance", 
			MessageBoxButtons.OK, MessageBoxIcon.Information);
			
			TxMessageBox.Show(string.Format(dim2_norm.ToString()), "Second distance", 
			MessageBoxButtons.OK, MessageBoxIcon.Information);
			
			TxMessageBox.Show(string.Format(dim3_norm.ToString()), "Third distance", 
			MessageBoxButtons.OK, MessageBoxIcon.Information);
			
		}
    
    	// Set the 'reach' velocity
    	
    	double reach_speed = 750; // mm/s
    
    	// Get the compound operation
    	
        TxObjectList Operation = TxApplication.ActiveDocument.GetObjectsByName("Attempt operation");
        var op = Operation[0] as ITxCompoundOperation;
        
        TxCompoundOperation comp_op = Operation[0] as TxCompoundOperation;
        
        // Get the 3 operations by name (grasp, Intermediate and Final)
        
        TxObjectList CompOp1 = TxApplication.ActiveDocument.GetObjectsByName("New Grasp");
        var op1 = CompOp1[0] as ITxOperation;
        
        TxObjectList CompOp2 = TxApplication.ActiveDocument.GetObjectsByName("New intermediate");
        var op2 = CompOp2[0] as ITxOperation;
        
        TxObjectList CompOp3 = TxApplication.ActiveDocument.GetObjectsByName("New final");
        var op3 = CompOp3[0] as ITxOperation;
        
        // Set the time of the operations
        
        double time1 = dim1_norm / reach_speed;
        double time2 = dim2_norm / reach_speed;
        double time3 = dim3_norm / reach_speed;
        
        op1.Duration = time1;
        op2.Duration = time2;
        op3.Duration = time3;
        
        // Get the times of the operations
        
        double time_finish_grasp = op1.Duration;
        double time_intermediate = op2.Duration;
        double time_finish_intermediate = time_finish_grasp + time_intermediate;
        double time_final = op3.Duration;
        
        // Concatenate the operations correctly
        
        comp_op.SetChildOperationRelativeStartTime(op2, time_finish_grasp);
        comp_op.SetChildOperationRelativeStartTime(op3, time_finish_intermediate);
        
        //op.Duration = 0.28;
        
        // Get the informstion about time
        
        double Ttime = op.Duration;
        
        // Display the result
        
        if (verbose)
        
        {
        
        TxMessageBox.Show(string.Format(Ttime.ToString()), "Flag", 
		MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
    }
}



