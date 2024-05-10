/*
This snippet allows to get the x, y, z coordinates of the TCP of a robot.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Tecnomatix.Engineering.Olp;
using Tecnomatix.Engineering.Utilities;

public class MainScript
{
    
    public static void Main(ref StringWriter output)
    {
		
		// Reference the operation	
		TxTypeFilter opFilter = new TxTypeFilter(typeof(TxContinuousRoboticOperation));
		TxOperationRoot opRoot = TxApplication.ActiveDocument.OperationRoot;
		TxObjectList allOps = opRoot.GetAllDescendants(opFilter);
		TxContinuousRoboticOperation lineSimOp = allOps[0] as TxContinuousRoboticOperation;
    
    	// Save the robot instance (the index may change) 
    	TxObjectList selectedObjects = TxApplication.ActiveSelection.GetItems();
		selectedObjects = TxApplication.ActiveDocument.GetObjectsByName("UR5e");
    	TxRobot rob = selectedObjects[0] as TxRobot;
    			
    	// Actual TCP position and orientation (pose)   	
    	TxTransformation actTcp = rob.TCPF.AbsoluteLocation;

      	// x, y, z coordinates of the TCP      	
     	var x_rob = actTcp[0, 3];
      	var y_rob = actTcp[1, 3];
      	var z_rob = actTcp[2, 3];
      	    	
        
        // print the x, y, z coordinates of the TCP
		output.Write("The x position of the TCP is: " + x_rob.ToString() + ", the y position of the TCP is: " 
		+ y_rob.ToString() + ", the z position of the TCP is: " + z_rob.ToString() + output.NewLine);
        
    }

}

