/*
This snipppet is a simple example of how to flag the simulation result.
It ouptuts 0 if no error occured during the simualtion and 1 if there was an error.
One good way to apply this snippet is to create 2 robotic rograms:
    ° In the first the robot moves in a reasonable area;
    ° In the second the robot grasps a cube which is very close to its base: this will probably cause an error.
*/

using System;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using Tecnomatix.Engineering;

public class MainScript
{
    public static void Main()
    {
    
        // Access the simulation player and rewind the simulation
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();
        
        // Run the simulation in a way that you can control some parameters, if necessary
        if (!player.IsSimulationRunning())
        {
            player.Play();
        }
        
        // Check for errors after simulation completes (call the method 'CheckSimulationSuccess')
        int simulationSuccess = CheckSimulationSuccess();
        player.Rewind();

        // Display the results with the Tecnomatix MessageBox
        TxMessageBox.Show(string.Format(simulationSuccess.ToString()), "result", 
        MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    
    // Definition of the static method that returns an ineteger value
    static int CheckSimulationSuccess()
    {
        // Get errors and traces associated to the current simulation
        List<string> errors = TxApplication.ActiveDocument.SimulationPlayer.GetErrorsAndTraces(TxSimulationErrorType.Error);

        // Check if there are any errors and give a return value
        if (errors.Count > 0)
        {
            // Use any form to display the error messages
            foreach (var error in errors)
            {
                // Creates a MessageBox for all errors
                MessageBox.Show(error, "Caption");
            }

            return 1; // Simulation failed
        }

        else // No error
        {
            return 0; // Simulation succeeded
        }
    }
}
