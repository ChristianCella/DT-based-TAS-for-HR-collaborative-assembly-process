/*
This snippet allows, at every temporal discretization, to:
    * Calculate the jacobian and its determinant, based on the configuration;
    * Check if a collision happens between the robot and the human: in that case, also the time instant is given;
    * Set the speed of the robot, based on the distance between the robot and the human;
The simulation must have:
    * A robot ("UR5e", for example);
    * A human;
    * A dimension between two reference frames to calculate the distance between the robot and the human.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{

    // Class-specific static variables   
    static StringWriter m_output;
    static int flag = 0;
    static int max_perc = 100;
    static int scale_vel_perc = 30;

    static int distnace_thresh = 210; 

    public static void Main(ref StringWriter output)
    {
    
        // Save the complete operation (human + robot) in a variable
        TxObjectList Operation = TxApplication.ActiveDocument.GetObjectsByName("Total operation");
        var op = Operation[0] as ITxCompoundOperation;

        m_output = output; // Display the output in the console

        // Create a simualtion player and start increasing the simulation time
        TxSimulationPlayer simulationPlayer = new TxSimulationPlayer(false, false);

        simulationPlayer.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(SimulationTimeIntervalReached);
        simulationPlayer.SetOperation(op); // Set the operation
        simulationPlayer.TimeInterval = 0.01; // Set the discretization step

        // Check the collisions
        TxApplication.ActiveDocument.CollisionRoot.CheckCollisions = true;

        // Play the simulation
        simulationPlayer.Play();
        simulationPlayer.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(SimulationTimeIntervalReached);

		SetSpeed(max_perc); // This command allows to have the correct speed when manually playing the simulation

        // Rewind the simulation at the very end and display a message if no collision happened
        
        if (flag == 0)
        {
        	m_output.Write("No collision happened!" + m_output.NewLine);
        }

        simulationPlayer.Rewind();
    }

    /* Definition of the method to:
        * scale the velocity;
        * calculate the Jacobian matrix;
        * check the collisions.
    */
    private static void SimulationTimeIntervalReached(object sender, TxSimulationPlayer_TimeIntervalReachedEventArgs args)
    {
    
    	/*
    	Implement the speed reduction
    	*/
    	
    	// Store the instance of a dimension (homogeneous matrix)
		TxLinearDimension dim = TxApplication.ActiveDocument.GetObjectsByName("Dim")[0] as TxLinearDimension;
    	
    	
    	// Get the first object 		
		ITxLocatableObject Obj1 = dim.FirstObject;
		TxTransformation HomMat1 = Obj1.LocationRelativeToWorkingFrame;
		
		// Get the second object 		
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

        // Verify the relative distance and scale the speed (call the method 'SetSpeed' defined below)

    	if (DimNorm <= distnace_thresh)
        {
        
        	// Speed set at the 30% of the maximum        	
            SetSpeed(scale_vel_perc);
        }
        else
        {
        
        	// Speed at the 100%    	
        	SetSpeed(max_perc);
        }
        
    	
    	/*
    	Implement the jacobian
    	*/
    	
    	// Save the robot instance (the index may change) 
    	TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	TxRobot rob = selectedObjects[0] as TxRobot;
    	
    	// Ground
		
		TxFrame DH0 = TxApplication.ActiveDocument.
        GetObjectsByName("BASEFRAME")[0] as TxFrame;
        var Frame0 = new TxTransformation(DH0.LocationRelativeToWorkingFrame);
        
        // Joint 1 (Base)
		
		TxFrame DH1 = TxApplication.ActiveDocument.
        GetObjectsByName("fr1")[0] as TxFrame;
        var Frame1 = new TxTransformation(DH1.LocationRelativeToWorkingFrame);
        
        // Joint 2 (Shoulder)
		
		TxFrame DH2 = TxApplication.ActiveDocument.
        GetObjectsByName("fr2")[0] as TxFrame;
        var Frame2 = new TxTransformation(DH2.LocationRelativeToWorkingFrame);
        
        // Joint 3 (Elbow)
		
		TxFrame DH3 = TxApplication.ActiveDocument.
        GetObjectsByName("fr3")[0] as TxFrame;
        var Frame3 = new TxTransformation(DH3.LocationRelativeToWorkingFrame);
        
        // Joint 4 (Wrist 1)
		
		TxFrame DH4 = TxApplication.ActiveDocument.
        GetObjectsByName("fr4")[0] as TxFrame;
        var Frame4 = new TxTransformation(DH4.LocationRelativeToWorkingFrame);
        
        // Joint 5 (Wrist 2)
		
		TxFrame DH5 = TxApplication.ActiveDocument.
        GetObjectsByName("fr5")[0] as TxFrame;
        var Frame5 = new TxTransformation(DH5.LocationRelativeToWorkingFrame);
        
        // Joint 6 (Wrist 3)
		
		TxFrame DH6 = TxApplication.ActiveDocument.
        GetObjectsByName("TOOLFRAME")[0] as TxFrame;
        var Frame6 = new TxTransformation(DH6.LocationRelativeToWorkingFrame);
        
        // Store the x, y, z coordinates to create vectors (They need to be in meters and they are the fourth columns)
        
        var x1 = Frame1[0, 3] / 1000;
        var y1 = Frame1[1, 3] / 1000;
        var z1 = Frame1[2, 3] / 1000;
        
        var x2 = Frame2[0, 3] / 1000;
        var y2 = Frame2[1, 3] / 1000;
        var z2 = Frame2[2, 3] / 1000;
        
        var x3 = Frame3[0, 3] / 1000;
        var y3 = Frame3[1, 3] / 1000;
        var z3 = Frame3[2, 3] / 1000;
        
        var x4 = Frame4[0, 3] / 1000;
        var y4 = Frame4[1, 3] / 1000;
        var z4 = Frame4[2, 3] / 1000;
        
        var x5 = Frame5[0, 3] / 1000;
        var y5 = Frame5[1, 3] / 1000;
        var z5 = Frame5[2, 3] / 1000;
        
        var x6 = Frame6[0, 3] / 1000;
        var y6 = Frame6[1, 3] / 1000;
        var z6 = Frame6[2, 3] / 1000;
        
        // Define the "Z" vectors (they are the third columns of the homogeneous matrices)
        
        double[] Z0 = { Frame1[0, 2], Frame1[1, 2], Frame1[2, 2] };
        double[] Z1 = { Frame2[0, 2], Frame2[1, 2], Frame2[2, 2] };
        double[] Z2 = { Frame3[0, 2], Frame3[1, 2], Frame3[2, 2] };
        double[] Z3 = { Frame4[0, 2], Frame4[1, 2], Frame4[2, 2] };
        double[] Z4 = { Frame5[0, 2], Frame5[1, 2], Frame5[2, 2] };
        double[] Z5 = { Frame6[0, 2], Frame6[1, 2], Frame6[2, 2] };
        
        // Define the position vectors (p and p6 are the same)
        
        double[] p0 = { 0.0, 0.0, 0.0 };
        double[] p1 = { x1, y1, z1 };
        double[] p2 = { x2, y2, z2 };
        double[] p3 = { x3, y3, z3 };
        double[] p4 = { x4, y4, z4 };
        double[] p5 = { x5, y5, z5 };
        double[] p6 = { x6, y6, z6 };
        double[] p = { x6, y6, z6 };
        
        // Subtract the vectors
        
        double[] Pp0 = { p[0] - p1[0], p[1] - p1[1], p[2] - p1[2] };
        double[] Pp1 = { p[0] - p2[0], p[1] - p2[1], p[2] - p2[2] };
        double[] Pp2 = { p[0] - p3[0], p[1] - p3[1], p[2] - p3[2] };
        double[] Pp3 = { p[0] - p4[0], p[1] - p4[1], p[2] - p4[2] };
        double[] Pp4 = { p[0] - p5[0], p[1] - p5[1], p[2] - p5[2] };
        double[] Pp5 = { p[0] - p6[0], p[1] - p6[1], p[2] - p6[2] };
        
        // Compute the cross product (the method "CrossProduct" is defined below)
        
        double[] result0 = CrossProduct(Z0, Pp0);
        double[] result1 = CrossProduct(Z1, Pp1);
        double[] result2 = CrossProduct(Z2, Pp2);
        double[] result3 = CrossProduct(Z3, Pp3);
        double[] result4 = CrossProduct(Z4, Pp4);
        double[] result5 = CrossProduct(Z5, Pp5);
        
        // Create a matrix of "double" numbers
        
        double[,] matrixData = {
            { result0[0], result1[0], result2[0], result3[0], result4[0], result5[0] },
            { result0[1], result1[1], result2[1], result3[1], result4[1], result5[1] },
            { result0[2], result1[2], result2[2], result3[2], result4[2], result5[2] },
            { Z0[0], Z1[0], Z2[0], Z3[0], Z4[0], Z5[0] },
            { Z0[1], Z1[1], Z2[1], Z3[1], Z4[1], Z5[1] },
            { Z0[2], Z1[2], Z2[2], Z3[2], Z4[2], Z5[2] }
        };
        
        // Get the determinant (the function "CalculateDeterminant" is defined below)
        
        double determinant = CalculateDeterminant(matrixData);      
        //m_output.Write("The determinant of the Jacobian is: " + determinant.ToString() + m_output.NewLine);

		/*
		Implement the collision check
		*/

        // Set some parameters for the collision check
        TxCollisionQueryParams collisionQueryParams = new TxCollisionQueryParams
        {
            UseNearMiss = false,
            UseAllowedPenetration = false
        };

        // Create a new context
        TxCollisionQueryResults results =
        TxApplication.ActiveDocument.CollisionRoot.GetCollidingObjects(collisionQueryParams, new
        TxCollisionQueryContext());

        // Analyze all the states
        foreach (TxCollisionState state in results.States)
        {
            switch (state.Type)
            {
                case TxCollisionState.TxCollisionStateType.Collision:

                    // Display a message on the screen: the time instant at which a possible collision happens
                    if (flag < 1) // 'flag' has not been incremented yet
                    {
                    	// Actual TCP position and orientation (pose)   	
    					TxTransformation actTcp = rob.TCPF.AbsoluteLocation;

      					// x, y, z coordinates of the TCP      	
     					var x_rob = actTcp[0, 3];
      					var y_rob = actTcp[1, 3];
      					var z_rob = actTcp[2, 3];
      					
      					// display the time instant
                        m_output.Write("A collision happened at: " + args.CurrentTime.ToString() + " seconds" + m_output.NewLine);
                        
                        // display the x, y, z coordinates of the TCP
						m_output.Write("The x position of the TCP is: " + x_rob.ToString() + ", the y position of the TCP is: " 
						+ y_rob.ToString() + ", the z position of the TCP is: " + z_rob.ToString() + m_output.NewLine);
                        
                        // Increase teh counter to avoid displaying all the collisions
                        flag++;
                    }
                    

                    break;

            }
        }
    }
    
    // Method for the cross product needed in the Jacobian
    static double[] CrossProduct(double[] vectorA, double[] vectorB)
    {
        // Check that both vectors are 3x1
        if (vectorA.Length != 3 || vectorB.Length != 3)
        {
            throw new ArgumentException("Vectors must be of length 3.");
        }

        // Compute the cross product
        double[] result = new double[3];
        result[0] = vectorA[1] * vectorB[2] - vectorA[2] * vectorB[1];
        result[1] = vectorA[2] * vectorB[0] - vectorA[0] * vectorB[2];
        result[2] = vectorA[0] * vectorB[1] - vectorA[1] * vectorB[0];

        return result;
    }
    
    // Method to calculate the determinant
    
    static double CalculateDeterminant(double[,] matrix)
    {
        // Check if the matrix is square
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        if (rows != cols || rows != 6)
        {
            throw new ArgumentException("Matrix must be a square 6x6 matrix to calculate determinant.");
        }

        // Calculate the determinant
        double determinant = RecursiveDeterminant(matrix);

        return determinant;
    }

    // Another static method to recursively calculate the determinant
    
    static double RecursiveDeterminant(double[,] matrix)
    {
        int size = matrix.GetLength(0);

        // Base case: for a 1x1 matrix, the determinant is the single element
        if (size == 1)
        {
            return matrix[0, 0];
        }

        double result = 0.0;

        // Expand along the first row
        for (int j = 0; j < size; j++)
        {
            // Calculate the minor matrix
            double[,] minorMatrix = new double[size - 1, size - 1];
            for (int k = 1; k < size; k++)
            {
                for (int l = 0, m = 0; l < size; l++)
                {
                    if (l != j)
                    {
                        minorMatrix[k - 1, m++] = matrix[k, l];
                    }
                }
            }

            // Calculate the cofactor and recursively compute the determinant
            double cofactor = matrix[0, j] * RecursiveDeterminant(minorMatrix);

            // Alternate signs for each element in the row
            result += (j % 2 == 0 ? 1 : -1) * cofactor;
        }

        return result;
    }
    
    
    // Definition of a method to set the speed
    private static void SetSpeed(int val)
    {
    	
        TxRoboticIntParam intParam = new TxRoboticIntParam("REDUCE_SPEED", val);
        
        // robot ("UR5e")
		TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
		var robot = selectedObjects[0] as TxRobot;
		
		robot.SetParameter(intParam);
    }



}




