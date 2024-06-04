/*
This code is supposed to be copied in the '.NET Script' viewer present in the Tecnomatix Process Simulate environment.
*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using System.Collections.Generic;
using Tecnomatix.Engineering.Olp;
//using System.Linq;

class Program
{
	// Static variables to calculate the average OWAS of the human 
    static bool verbose = true;   
    static List<int> back_vec = null;
    static List<int> arm_vec = null;
    static List<int> leg_vec = null;
    static List<int> head_vec = null;
    static List<int> load_vec = null;
    static List<int> avg_owas = null;

	
    static public void Main(ref StringWriter output)
    {
        TcpListener server = null;
        try
        {
            int base_val = 4;
            int Nsim = 2;
            int time = 1;
            // Start listening for possible connections
            var ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 12345;
            server = new TcpListener(ipAddress, port);
            server.Start();
            TcpClient client = server.AcceptTcpClient();

            // If the client successfully connected, print a message
			if (verbose)
			{
				output.Write("Connection successfully established with the Python server!.\n");
			}

            // Get the robot    	
            TxObjectList objects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
            var robot = objects[0] as TxRobot;

            //Define the home position for the robot
            var home_point = new TxVector (301, -133, 290);

			
            for (int ii = 1; ii <= Nsim; ii++)
            {

                // a) Send the time and RULA kpi(s) of the previous simulation

                int[] kpis = { time + ii}; // pack the KPIs
                string data = string.Join(",", kpis); // convert tthe array into a string
                NetworkStream stream1 = client.GetStream(); // open the first stream 
                byte[] kpi_vec = Encoding.ASCII.GetBytes(data); // ASCII encoding              
                stream1.Write(kpi_vec, 0, kpi_vec.Length); // Write on the stream
                output.Write("The Key Performance Indicator(s) sent to Python are:\n");
                output.Write(data.ToString());
                output.Write("\n");

                // b) Get the new 'tentative' layout to run the new simulation

                var receivedArray = ReceiveNumpyArray(client); // static method defined below
                output.Write("The tentative layout received by the BO is the following vector: \n");
				output.Write(ArrayToString(receivedArray));
				output.Write("\n");
			
                // conta quante righe ha receivedArray

                int len = receivedArray.GetLength(1);

                output.Write("The number of rows of the received array is: \n");
                output.Write(len.ToString());
                output.Write("\n");

                int num_op = ((len - 1 - (receivedArray[0, len - 1] / 1000))/ base_val);

                output.Write("The number of operations is: \n");
                output.Write(num_op.ToString());
                output.Write("\n");

                int num_robot_op = (receivedArray[0, len - 1]) / 1000;

                output.Write("The number of robot operations is: \n");
                output.Write(num_robot_op.ToString());
                output.Write("\n");

                for (int jj = 0; jj < num_robot_op; jj++)
                {

                    int rob_id = (receivedArray[0, len - 2 - jj])/1000;
                    output.Write("The robot id is: \n");
                    output.Write(rob_id.ToString());
                    output.Write("\n");
                    output.Write("Primo for \n");

                    for (int ff = 0; ff < num_op; ff++)

                    {
                    
                        output.Write("Secondo for \n");
                        
                        output.Write((ff*base_val+3).ToString());
                        output.Write(((receivedArray[0, ff * base_val + 3]) / 1000).ToString() + output.NewLine);
                        int curr_id = (receivedArray[0, ff * base_val + 3]) / 1000;
                        if (curr_id == rob_id)

                        {
                            CreateRobotOperation(receivedArray[0, ff * base_val]/1000, receivedArray[0, ff * base_val + 1]/1000, receivedArray[0, ff * base_val + 2]/1000, receivedArray[0, ff * base_val + 3], robot);
                        }
                    }

                }

                // c) Send the varible trigger_end to python

                string trigger_end = ii.ToString(); // convert the current iteration index to string
                NetworkStream stream2 = client.GetStream(); // open the second stream
                byte[] byte_trigger_end = Encoding.ASCII.GetBytes(trigger_end); // ASCII encoding           
                stream2.Write(byte_trigger_end, 0, byte_trigger_end.Length); // Write on the stream
                output.Write("The current iteration number is sent to Python and it is equal to:\n");
                output.Write(trigger_end.ToString());
                output.Write("\n");
            
                client.Close();
            
            }
        }
        catch (Exception e)
        {
            // If necessary, write the type of exception found
            output.Write("Error: {e.Message}");
        }
        
    }

    // Static method to convert from bytes to array (this method is used inside 'ReceiveNumpyArray')
    static int[] ConvertBytesToIntArray(byte[] bytes, int startIndex)
    {
        // Create an integer array, called 'result', by dividing the length of the vector 'bytes' by 4

        int[] result = new int[bytes.Length / 4];

        for (int i = 0; i < result.Length; i++) // Loop over all the elements of 'result'
        {
            result[i] = BitConverter.ToInt32(bytes, startIndex + i * 4); // convert a segment of 4 bytes inside 'bytes' into an integer
        }
        return result;
    }
    // Static method to receive a NumPy array from a Python server over a TCP connection
    static int[,] ReceiveNumpyArray(TcpClient client)
    {
        // Obtain the stream to read and write data over the network

        NetworkStream stream = client.GetStream();
        
        /* Receive the shape and data type of the array
         * It's assumed that the shape is represented by two integers, each of 4 bytes (N° rows, N°columns)
         * It's assumed that the data type information is represented by a 4-byte value
        */

        
        byte[] shapeBytes = new byte[8]; // create a variable for the two integers defining the shape
        stream.Read(shapeBytes, 0, shapeBytes.Length); // read the shape
        int[] shape = ConvertBytesToIntArray(shapeBytes, 0); // Convert the received shape bytes into an integer array
        
        // Receive the actual array data. It's important that 'SizeOf' contains the same type (int, in my case) defined besides 'static'

        byte[] arrayBytes = new byte[Marshal.SizeOf(typeof(int)) * shape[0] * shape[1]]; // Create a byte array to receive data
        stream.Read(arrayBytes, 0, arrayBytes.Length); // Read data from the network stream

        // Convert the received bytes back to a NumPy array. Again, the type (int) must be the same as above

        int[,] receivedArray = new int[shape[0], shape[1]]; // Create a 2D array with the received shape
        Buffer.BlockCopy(arrayBytes, 0, receivedArray, 0, arrayBytes.Length); // Copy the received data to 'receivedArray'

        // Return the array

        return receivedArray;
    }
    // Static method to convert an array into a string
    static string ArrayToString<T>(T[,] array)
    {

        // Define number of rows and columns

        int rows = array.GetLength(0);
        int cols = array.GetLength(1);

        // Loop to transform each element into a string

        string result = "";
        for (int i = 0; i < rows; i++) // Scan all the rows
        {
            for (int j = 0; j < cols; j++) // Scan all the columns
            {
                result += array[i, j].ToString() + "\t"; // separate each element with a tab ('\t') with respect to the previous
            }
            result += Environment.NewLine; // Aftre scanning all the elements in the columns, start displaying in the row below
        }
        return result;
    }

    public static void CreateRobotOperation (double xpos, double ypos, double zpos, int object_index, TxRobot robot)
    {
    // Define some variables
        string operation_name = "Pick&Place_" + object_index.ToString();

        string new_tcp = "tcp_1";
        string new_motion_type = "MoveL";
        string new_speed = "1000";
        string new_accel = "1200";
        string new_blend = "0";
        string new_coord = "Cartesian";
        
        bool verbose = false; // Controls some display options

        // search the cube that has name cube_(object_index)

        string pick_object = "cube_" + object_index.ToString();


        // Object to be picked
        TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
        selectedObjects = TxApplication.ActiveDocument.GetObjectsByName(pick_object);
        var Cube = selectedObjects[0] as ITxLocatableObject;
 

        // Create the new operation    	
        TxContinuousRoboticOperationCreationData data = new TxContinuousRoboticOperationCreationData(operation_name);
        TxApplication.ActiveDocument.OperationRoot.CreateContinuousRoboticOperation(data);
         

        // Save the created operartion in a variable
        TxContinuousRoboticOperation MyOp = TxApplication.ActiveDocument.GetObjectsByName(operation_name)[0] as TxContinuousRoboticOperation;

        // Create all the necessary points       
        TxRoboticViaLocationOperationCreationData Point1 = new TxRoboticViaLocationOperationCreationData();
        Point1.Name = "point1"; // First point
        
        TxRoboticViaLocationOperationCreationData Point2 = new TxRoboticViaLocationOperationCreationData();
        Point2.Name = "point2"; // Second point
        
        TxRoboticViaLocationOperationCreationData Point3 = new TxRoboticViaLocationOperationCreationData();
        Point3.Name = "point3"; // Third point
        
        TxRoboticViaLocationOperation FirstPoint = MyOp.CreateRoboticViaLocationOperation(Point1);
        TxRoboticViaLocationOperation SecondPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point2, FirstPoint);
        TxRoboticViaLocationOperation ThirdPoint = MyOp.CreateRoboticViaLocationOperationAfter(Point3, SecondPoint);
        
        // Impose a position to the new waypoint

        var cube_pos = new TxTransformation(Cube.LocationRelativeToWorkingFrame);

        double rotVal1 = Math.PI;
        TxTransformation rotX1 = new TxTransformation(new TxVector(rotVal1, 0, 0), 
        TxTransformation.TxRotationType.RPY_XYZ);
        FirstPoint.AbsoluteLocation = rotX1;
        
        var pointA = new TxTransformation(FirstPoint.AbsoluteLocation);
        pointA.Translation = new TxVector(cube_pos[0, 3], cube_pos[1, 3], cube_pos[2, 3]);
        FirstPoint.AbsoluteLocation = pointA;
        
        // Impose a position to the second waypoint		
        double rotVal2 = Math.PI;
        TxTransformation rotX2 = new TxTransformation(new TxVector(rotVal2, 0, 0), 
        TxTransformation.TxRotationType.RPY_XYZ);
        SecondPoint.AbsoluteLocation = rotX2;
        
        var pointB = new TxTransformation(SecondPoint.AbsoluteLocation);
        pointB.Translation = new TxVector(xpos, ypos, zpos);
        SecondPoint.AbsoluteLocation = pointB;
        
        // Impose a position to the third waypoint		
        double rotVal3 = Math.PI;
        TxTransformation rotX3 = new TxTransformation(new TxVector(rotVal3, 0, 0), 
        TxTransformation.TxRotationType.RPY_XYZ);
        ThirdPoint.AbsoluteLocation = rotX3;
        
        var pointC = new TxTransformation(ThirdPoint.AbsoluteLocation);
        pointC.Translation = new TxVector(300, 0, 300);
        ThirdPoint.AbsoluteLocation = pointC;

        // NOTE: you must associate the robot to the operation!
        MyOp.Robot = robot; 

        // Implement the logic to access the parameters of the controller		
        TxOlpControllerUtilities ControllerUtils = new TxOlpControllerUtilities();		
        TxRobot AssociatedRobot = ControllerUtils.GetRobot(MyOp); // Verify the correct robot is associated 
                
        ITxOlpRobotControllerParametersHandler paramHandler = (ITxOlpRobotControllerParametersHandler)
        ControllerUtils.GetInterfaceImplementationFromController(robot.Controller.Name,
        typeof(ITxOlpRobotControllerParametersHandler), typeof(TxRobotSimulationControllerAttribute),
        "ControllerName");

                // Set the new parameters for the waypoint					
        paramHandler.OnComplexValueChanged("Tool", new_tcp, FirstPoint);
        paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, FirstPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, FirstPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, FirstPoint);
        paramHandler.OnComplexValueChanged("Blend", new_blend, FirstPoint);
        paramHandler.OnComplexValueChanged("Coord Type", new_coord, FirstPoint);
        
        paramHandler.OnComplexValueChanged("Tool", new_tcp, SecondPoint);
        paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, SecondPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, SecondPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, SecondPoint);
        paramHandler.OnComplexValueChanged("Blend", new_blend, SecondPoint);
        paramHandler.OnComplexValueChanged("Coord Type", new_coord, SecondPoint);
        
        paramHandler.OnComplexValueChanged("Tool", new_tcp, ThirdPoint);
        paramHandler.OnComplexValueChanged("Motion Type", new_motion_type, ThirdPoint);
        paramHandler.OnComplexValueChanged("Speed", new_speed, ThirdPoint);
        paramHandler.OnComplexValueChanged("Accel", new_accel, ThirdPoint);
        paramHandler.OnComplexValueChanged("Blend", new_blend, ThirdPoint);
        paramHandler.OnComplexValueChanged("Coord Type", new_coord, ThirdPoint);
    }
}

