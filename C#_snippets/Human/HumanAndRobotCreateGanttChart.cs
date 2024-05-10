/*
This snippet allows to re-arrange the operations that are inside a compound operation, so that they start at different times.
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
    {
    
    	// Define some variables
    	string comp_op_name = "CompOp";
    
    	// Get the compound operation    	
        TxObjectList Operation = TxApplication.ActiveDocument.GetObjectsByName(comp_op_name);
        var op = Operation[0] as ITxCompoundOperation;
        
        TxCompoundOperation comp_op = Operation[0] as TxCompoundOperation;
    	
    	// Save all the needed operations in variables
    	TxObjectList Task1 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place1");
        var task1 = Task1[0] as ITxOperation;
        
        TxObjectList Task2 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place2");
        var task2 = Task2[0] as ITxOperation;
        
        TxObjectList Task3 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place3");
        var task3 = Task3[0] as ITxOperation;
        
        TxObjectList Task4 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place4");
        var task4 = Task4[0] as ITxOperation;
        
        // Calculate the starting times
        double time1 = 0.0;
        double time2 = task1.Duration;
        double time3 = task2.Duration + time2;
		double time4 = time3;   
		
		// Create the Gantt chart
		comp_op.SetChildOperationRelativeStartTime(task1, time1); 
		comp_op.SetChildOperationRelativeStartTime(task2, time2); 
		comp_op.SetChildOperationRelativeStartTime(task3, time3); 
		comp_op.SetChildOperationRelativeStartTime(task4, time4); 
    	
        output.Write("Hello World!");
    }
}
