/*
This snippet is very useful to set a specific operation just by specifying its name.
*/

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
    public static void Main()
    {
    
        // Set the control variable
    	bool verbose = false;
    
    	// Set the desired operation (These lines are very important)   	
        var op = TxApplication.ActiveDocument.OperationRoot.GetAllDescendants(new 
        TxTypeFilter(typeof(TxCompoundOperation))).FirstOrDefault(x => x.Name.Equals("TrajectoryFile")) as 
        TxCompoundOperation;     
        TxApplication.ActiveDocument.CurrentOperation = op;
        
        // Now that the simulation is set, create the 'player' object and play the simulation       
        TxSimulationPlayer player = TxApplication.ActiveDocument.SimulationPlayer;
        player.Rewind();
        player.Play();
        
        if (verbose)
        {
        	TxMessageBox.Show(string.Format("Finished!"), "Flag", 
			MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        // Refresh and rewind (if needed)       
        TxApplication.RefreshDisplay();
        player.Rewind();
        
        // Reset the sequence editor (if needed)
        //sequenceEditor.Reset();
        
    }

}
