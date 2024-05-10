/*
This is not a snippet, but a first example of a script that automatically calculates the (average) OWAS index.
The part related to the triggering of events is better explained in 'ScaleVelocityBasedOnDistance.cs'.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using System.Collections.Generic;
using System.Linq;

public class MainScript
{
    // Static variables to calculate the average OWAS of the human 
    static bool verbose = true;
    
    static List<int> back_vec = null;
    static List<int> arm_vec = null;
    static List<int> leg_vec = null;
    static List<int> head_vec = null;
    static List<int> load_vec = null;
    static List<double> avg_owas = null;
    
    
    public static void Main(ref StringWriter output)
    {
        // Part of code that generates the operations for the human

    	// Specify the names of the operations to be opened in the sequence editor (automatically)
    	string op_name = "Pick&Place2";
    	
    	// Set (in the sequence editor) the desired operation by calling its name   	
        var op = TxApplication.ActiveDocument.OperationRoot.GetAllDescendants(new 
        TxTypeFilter(typeof(TxCompoundOperation))).FirstOrDefault(x => x.Name.Equals(op_name)) as 
        TxCompoundOperation;     
        TxApplication.ActiveDocument.CurrentOperation = op;

        // Create a new simulation player
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        
        // Get the result by calling the method'CalculateOWAS'
        List<double> owas_op1 = CalculateOWAS(player, output);

        // Display the results (Call the method 'DisplayResults')
        DisplayResults(owas_op1, output);
    }
    
    // Custom method for the evalaution of the average OWAS score
    public static List<double> CalculateOWAS(TxSimulationPlayer player, StringWriter m_output)
    {   			
		// Initialize new lists (same name as the class-specific static variables)
		back_vec = new List<int>();
        arm_vec = new List<int>();
        leg_vec = new List<int>();
        head_vec = new List<int>();
        load_vec = new List<int>();
        avg_owas = new List<double>();

        // Trigger the events
        player.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
        player.Play(); // If no graphical update is needed, write player.PlayWithoutRefresh();
        player.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);

        // Compute the average OWAS (all the 5 indices stored in the lists thanks to the event handler)
        double avg_back_owas = back_vec.Average();
        double avg_arm_owas = arm_vec.Average();
        double avg_leg_owas = leg_vec.Average();
        double avg_head_owas = head_vec.Average();
        double avg_load_owas = load_vec.Average();

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
    private static void DisplayResults(List<double> owas, StringWriter m_output)
    {
        m_output.Write("average back OWAS: " + owas[0].ToString() + m_output.NewLine);
        m_output.Write("average arm OWAS: " + owas[1].ToString() + m_output.NewLine);
        m_output.Write("average leg OWAS: " + owas[2].ToString() + m_output.NewLine);
        m_output.Write("average head OWAS: " + owas[3].ToString() + m_output.NewLine);
        m_output.Write("average load OWAS: " + owas[4].ToString() + m_output.NewLine);
    } 
}
