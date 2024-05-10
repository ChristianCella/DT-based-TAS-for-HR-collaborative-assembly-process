/*
This snippet was provided to me by a technician in Siemens. 
It allows to modulate the speed of the robot when a certain condition is verified: to do this, at each time interval,
a check is performed on that specific condition and, if verified, the speed of the robot 
is reduced to a certain value (that represents the percentage with respect to 100).

The key in this snippet is the definition of the event TimeIntervalReached, which is triggered at each time interval:
at that point, it surely displays on the terminal the current time and, if the condition on the time instant is verified, 
it reduces the speed of the robot.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
    static StringWriter m_output;
    static int reduction_perc = 50;
    
    public static void MainWithOutput(ref StringWriter output)
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

            // Display the curent time
            m_output = output;

            // The event handler 'player_TimeIntervalReached' is subscribed to the event 'TimeIntervalReached'
            output.Write("The simulation is started" + output.NewLine);
            player.TimeIntervalReached += new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            
            // play
            player.Play();

            // The event handler 'player_TimeIntervalReached' is unsubscribed from the event 'TimeIntervalReached'
            player.TimeIntervalReached -= new TxSimulationPlayer_TimeIntervalReachedEventHandler(player_TimeIntervalReached);
            output.Write("The simulation is over" + output.NewLine);

            // Set the speed of the robot to 100% of the original one (so you do not keep a reduced speed for the next simulation)
            SetSpeed(100);

            // Rewind the simulation once it's over
            player.Rewind();
        }
      
    }

    // Define the event handler 'player_TimeIntervalReached'
    private static void player_TimeIntervalReached(object sender, TxSimulationPlayer_TimeIntervalReachedEventArgs args)
    {
        m_output.Write(args.CurrentTime.ToString() + m_output.NewLine);
        
        // Check if the current time is greater than or equal to 1.0
        if (args.CurrentTime >= 1.0)
        {
            // Reduce the speed of the following percentage (call the function 'SetSpeed' defined below)
            SetSpeed(reduction_perc);
        }

    }
    
    // custom method to scale the velocity
    private static void SetSpeed(int value)
    {
        // Specify the parameter on which the function acts
        TxRoboticIntParam intParam = new TxRoboticIntParam("REDUCE_SPEED", value);

        // Get the current operation (an cast the type 'ITxRoboticOrderedCompoundOperation')
        ITxOperation currentOp = TxApplication.ActiveDocument.CurrentOperation;
        ITxRoboticOrderedCompoundOperation roboticOperation = currentOp as ITxRoboticOrderedCompoundOperation;

        // Save the robot associated to the operation and change its speed (at the TCP)
        if (roboticOperation is ITxRoboticOrderedCompoundOperation)
        {
            TxRobot robot = roboticOperation.Robot as TxRobot;
            robot.SetParameter(intParam);
        }
    }
}