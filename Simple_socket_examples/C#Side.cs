// Import libraries (.dll files)

using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

class Program
{
    static public void Main(ref StringWriter output)
    {
        TcpListener server = null;

        /* 
         * Definition of the 'try - catch' architecture:
         * First, the code inside 'try' is executed
         * If no exception was found, the 'catch' block is never executed
         * If an exception was found, the control is passed to that block
            a) This catch block can handle exceptions of type 'Exception' (so it can catch any type of them)
            b) The caught exception is represented by the variable 'e'
            c) The line 'e.Message' reterives the error message
        */

        try
        {
            // Here, you are writing the code to 'try' to execute, that might cause an exception

            // Initialize the variables for time and RULA ergonomic score (this should not be done, because they are the result of the
            // first simulation before enterinf the 'for' loop)

            int time = 1;
            int RULA = 4;
            int Nsim = 3;

            // Start listening for possible connections

            var ipAddress = IPAddress.Parse("127.0.0.1");
            int port = 12345;
            server = new TcpListener(ipAddress, port);
            server.Start();

            // Display the message saying that the server is waiting for the client
			
			output.Write("The C# client is waiting the Python server to connect ...\n");
            TcpClient client = server.AcceptTcpClient();

            // If the client successfully connected, print a message

			output.Write("Connection successfully established with the Python server!.\n");
            /*
             * In this part, the first API-guided simulation is performed: time and RULA are the results
            */

            /*
             * This part is the core of the process and it allows to run automatic simulations until an upper bound (Nsim) is reached
            */

            for (int ii = 1; ii <= Nsim; ii++)
            {

                // a) Send the time and RULA kpi(s) of the previous simulation

                int[] kpis = { time + ii, RULA + ii }; // pack the KPIs
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
				
                // Run the ii-th simulation

                // ... the result of thi phase is the KPI(s) that will be sent to Python at the next iteration

                // c) Send the varible trigger_end to python

                string trigger_end = ii.ToString(); // convert the current iteration index to string
                NetworkStream stream2 = client.GetStream(); // open the second stream
                byte[] byte_trigger_end = Encoding.ASCII.GetBytes(trigger_end); // ASCII encoding           
                stream2.Write(byte_trigger_end, 0, byte_trigger_end.Length); // Write on the stream
                output.Write("The current iteration number is sent to Python and it is equal to:\n");
                output.Write(trigger_end.ToString());
                output.Write("\n");
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

    // ............................................ Static Methods ......................................... //

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
}
