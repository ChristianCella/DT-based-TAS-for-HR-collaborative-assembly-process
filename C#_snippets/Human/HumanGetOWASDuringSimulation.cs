/*
This snippet allows to obtain the average OWAS quintet of a human during a simulation.
At every temporal discretization, an event is triggered and the 5 parameters are obtained. In the end,
the average of each parameter is calculated and displayed.
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
    static StringWriter m_output;
    static int reduction_perc = 50;
    static List<int> back_vec = null;
    static List<int> arm_vec = null;
    static List<int> leg_vec = null;
    static List<int> head_vec = null;
    static List<int> load_vec = null;
    static List<double> avg_owas = null;
    
    public static void Main(ref StringWriter output)
    {

        // Create a new simulation player
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        
        // Check if the simulation is NOT ('!') running: if it is not running, the simulation is started
        if (!player.IsSimulationRunning())
        {
            // Check if the current operation of the active document is 'null'
            if (TxApplication.ActiveDocument.CurrentOperation == null)
            {
                // Get all the operations in the active document
                TxObjectList ops = TxApplication.ActiveDocument.OperationRoot.
                GetAllDescendants(new TxTypeFilter(typeof(ITxOperation)));

                // Simulate the first opeartion
                if (ops.Count > 0)
                {
                    ITxOperation op = ops[0] as ITxOperation;
                    if (op != TxApplication.ActiveDocument.CurrentOperation)
                    {
                        TxApplication.ActiveDocument.CurrentOperation = op;   
                    }
                }
                else
                {
                    return;
                }
            }
			
			// Initialize new lists
			back_vec = new List<int>();
            arm_vec = new List<int>();
            leg_vec = new List<int>();
            head_vec = new List<int>();
            load_vec = new List<int>();
            avg_owas = new List<double>();
			
            // Possibly display what is defined inside 'player_TimeIntervalReached'
            m_output = output;

            // The event handler 'player_TimeIntervalReached' is subscribed to the event 'TimeIntervalReached'
            output.Write("The simulation is started" + output.NewLine);
            player.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            
            // play (use PlayWithoutRefresh)
            player.PlayWithoutRefresh();

            // The event handler 'player_TimeIntervalReached' is unsubscribed from the event 'TimeIntervalReached'
            player.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            output.Write("The simulation is over" + output.NewLine);

            // Compute the average OWAS
            double avg_back_owas = back_vec.Average();
            double avg_arm_owas = arm_vec.Average();
            double avg_leg_owas = leg_vec.Average();
            double avg_head_owas = head_vec.Average();
            double avg_load_owas = load_vec.Average();

            avg_owas.Add(avg_back_owas);
            avg_owas.Add(avg_arm_owas);
            avg_owas.Add(avg_leg_owas);
            avg_owas.Add(avg_head_owas);
            avg_owas.Add(avg_load_owas);

            // Display some messages
			output.Write("average back OWAS: " + avg_owas[0].ToString() + output.NewLine);
			output.Write("average arm OWAS: " + avg_owas[1].ToString() + output.NewLine);
			output.Write("average leg OWAS: " + avg_owas[2].ToString() + output.NewLine);
			output.Write("average head OWAS: " + avg_owas[3].ToString() + output.NewLine);
			output.Write("average load OWAS: " + avg_owas[4].ToString() + output.NewLine);

            // Rewind the simulation once it's over
            player.Rewind();
        }
      
    }

    // Define the event handler 'player_TimeIntervalReached'
    private static void player_TimeIntervalReached(object sender, TxSimulationPlayer_TimeIntervalReachedEventArgs args)
    {       
        // Get the human		
		TxObjectList humans = TxApplication.ActiveSelection.GetItems();
		humans = TxApplication.ActiveDocument.GetObjectsByName("Jack");
		TxHuman human = humans[0] as TxHuman;
		
		// Save the OWAS code in a struct (called owas_code)
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
}
