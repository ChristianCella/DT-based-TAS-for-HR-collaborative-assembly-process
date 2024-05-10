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
using System.Linq;

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
            int Nsim = 2;

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
			
            // Define the names of the operations to be run
            string[] op_names = { "Pick&Place2", "Pick&Place4" };

			// Run both the simulations of the human
            for (int ii = 0; ii <= Nsim - 1; ii++)
            {
    			
    			// Set (in the sequence editor) the desired operation by calling its name   	
        		var op = TxApplication.ActiveDocument.OperationRoot.GetAllDescendants(new 
        		TxTypeFilter(typeof(TxCompoundOperation))).FirstOrDefault(x => x.Name.Equals(op_names[ii])) as 
        		TxCompoundOperation;     
        		TxApplication.ActiveDocument.CurrentOperation = op;
		
        		// Create a new simulation player
        		TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        		
        		// Get the result by calling the method'CalculateOWAS'
        		List<int> owas_op1 = CalculateOWAS(player, output);

                if (verbose)
                {
                    // Display the results (Call the method 'DisplayResults')
        		    DisplayResults(owas_op1, output);                     
                }
        		
                // a) Send the 5 indices of the OWAS score
                int[] kpis = owas_op1.ToArray(); // pack the KPIs
                string data = string.Join(",", kpis); // convert tthe array into a string
                NetworkStream stream1 = client.GetStream(); // open the first stream 
                byte[] kpi_vec = Encoding.ASCII.GetBytes(data); // ASCII encoding              
                stream1.Write(kpi_vec, 0, kpi_vec.Length); // Write on the stream
                output.Write("The Key Performance Indicator(s) sent to Python are: " + data.ToString() + output.NewLine);

                // b) Get the resulting score
                var receivedArray = ReceiveNumpyArray(client); // static method defined below
                output.Write("The resulting OWAS from python is: " + ArrayToString(receivedArray) + output.NewLine);

                // c) Send the varible trigger_end to python
                string trigger_end = ii.ToString(); // convert the current iteration index to string
                NetworkStream stream2 = client.GetStream(); // open the second stream
                byte[] byte_trigger_end = Encoding.ASCII.GetBytes(trigger_end); // ASCII encoding           
                stream2.Write(byte_trigger_end, 0, byte_trigger_end.Length); // Write on the stream
                output.Write("The current iteration number is sent to Python and it is equal to: " 
                + trigger_end.ToString() + output.NewLine);
            }

            // Close the connection after the 'Nsim' simulations
            client.Close();
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

    public static List<int> CalculateOWAS(TxSimulationPlayer player, StringWriter m_output)
    {   			
		// Initialize new lists (same name as the class-specific static variables)
		back_vec = new List<int>();
        arm_vec = new List<int>();
        leg_vec = new List<int>();
        head_vec = new List<int>();
        load_vec = new List<int>();
        avg_owas = new List<int>();

        // Trigger the events
        player.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
        player.Play(); // If no graphical update is needed, write player.PlayWithoutRefresh();
        player.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);

        // Compute the average OWAS (all the 5 indices stored in the lists thanks to the event handler)
        double avg_back_owas_d = back_vec.Average();
        int avg_back_owas = (int)Math.Round(avg_back_owas_d);
        double avg_arm_owas_d = arm_vec.Average();
        int avg_arm_owas = (int)Math.Round(avg_arm_owas_d);
        double avg_leg_owas_d = leg_vec.Average();
        int avg_leg_owas = (int)Math.Round(avg_leg_owas_d);
        double avg_head_owas_d = head_vec.Average();
        int avg_head_owas = (int)Math.Round(avg_head_owas_d);
        double avg_load_owas_d = load_vec.Average();
        int avg_load_owas = (int)Math.Round(avg_load_owas_d);

        // Append the new values
        avg_owas.Add(avg_back_owas);
        avg_owas.Add(avg_arm_owas);
        avg_owas.Add(avg_leg_owas);
        avg_owas.Add(avg_head_owas);
        avg_owas.Add(avg_load_owas);
		
        // Rewind the simulation once it's over
        player.Rewind();
        
        // Possible display
        if (verbose)
        {
            m_output.Write("The simulation is over" + m_output.NewLine);
        }

        // Return the average owas
        return avg_owas;     
    }

    // Custom method implementing the event handler 'player_TimeIntervalReached'
    private static void player_TimeIntervalReached(object sender, TxSimulationPlayer_TimeIntervalReachedEventArgs args)
    {       
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Save the OWAS code in a struct (called owas_code and obtained by calling the method 'GetOWASCodes')
		var owas_code = human.GetOWASCodes();
		
		// save the 5 single values
		int back_code = owas_code.BackCode;
        int arm_code = owas_code.ArmCode;
        int leg_code = owas_code.LegCode;
        int head_code = owas_code.HeadCode;
        int load_code = owas_code.LoadCode;
		
        // Append the new values
		back_vec.Add(back_code);
        arm_vec.Add(arm_code);
        leg_vec.Add(leg_code);
        head_vec.Add(head_code);
        load_vec.Add(load_code);
		
        // Possible display
		//m_output.Write("Back : " + back_code.ToString() + m_output.NewLine);       
    } 

    // Custom method to display the results
    private static void DisplayResults(List<int> owas, StringWriter m_output)
    {
        m_output.Write("average back OWAS: " + owas[0].ToString() + m_output.NewLine);
        m_output.Write("average arm OWAS: " + owas[1].ToString() + m_output.NewLine);
        m_output.Write("average leg OWAS: " + owas[2].ToString() + m_output.NewLine);
        m_output.Write("average head OWAS: " + owas[3].ToString() + m_output.NewLine);
        m_output.Write("average load OWAS: " + owas[4].ToString() + m_output.NewLine);
    }
}

