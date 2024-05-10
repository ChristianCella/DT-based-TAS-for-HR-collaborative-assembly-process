/*
This snippet allows to calculate the total duration of a task and to display the homogeneous matrix of a waypoint.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{

    public static void Main()
    {
    	
    	// Store in Waypoint the coordinates of p1        
        TxRoboticViaLocationOperation Waypoint = TxApplication.ActiveDocument.
        GetObjectsByName("p1")[0] as TxRoboticViaLocationOperation;
        
        // Store in positionP1 the homogeneous matrix (4x4)      
        var positionP1 = new TxTransformation(Waypoint.LocationRelativeToWorkingFrame);
        positionP1.Translation = new TxVector(300, 300, 300);
        Waypoint.LocationRelativeToWorkingFrame = positionP1;
        TxApplication.RefreshDisplay();
        string PosP1 = positionP1.ToString();
        
        // Display the homogeneous matrix       
        TxMessageBox.Show(string.Format(PosP1), "p1", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
        TxRoboticViaLocationOperationCreationData newPoint =
        new TxRoboticViaLocationOperationCreationData("Via1");
        
        // In Waypoints store all the via points imported in the code      
        TxObjectList Waypoints = TxApplication.ActiveDocument.OperationRoot.
        GetAllDescendants(new TxTypeFilter(typeof(TxRoboticViaLocationOperation)));
        
        // Initialize the variable for the time       
        double dDuration = 0.0;
        
        // Calculate the total duration of the task       
        foreach(TxRoboticViaLocationOperation point in Waypoints)
        {
        	dDuration += point.Duration;
        	String Name = point.Name;
        	 TxMessageBox.Show(string.Format(Name), "Waypoint Name", MessageBoxButtons.OK, MessageBoxIcon.Information);
        	
        }
        
        string durationTime = dDuration.ToString();    
        TxMessageBox.Show(string.Format(durationTime), "Time", MessageBoxButtons.OK, MessageBoxIcon.Information);
        
    }

}
