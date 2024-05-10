/*
This snippet allows to concatenate operations in a compound operation. The fundamental part is that the operations
must be present in the document (possibly created by some previous scripts). They must be targeted as objects:
thanks to the AddObject method, they will be grouped inside a compound operation.
The problem that this script does NOT tackle is that they start from the same time instsnt: another script
must be created to create the real Gantt chart.
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
    public static void MainWithOutput()
    {

        // Define some variables
        string comp_op_name = "CompOp";

        // Create the compound operation and save it in a variable
        TxCompoundOperationCreationData dat = new TxCompoundOperationCreationData(comp_op_name);
        TxApplication.ActiveDocument.OperationRoot.CreateCompoundOperation(dat);
        
        TxObjectList operations = TxApplication.ActiveDocument.GetObjectsByName(comp_op_name);
    	var comp_op = operations[0] as TxCompoundOperation;
        
        // Save in specific variables all the operations to be grouped together
        TxObjectList CompOp1 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place1");
        var op_ob1 = CompOp1[0] as ITxObject;
        var op1 = CompOp1[0] as ITxOperation;

        TxObjectList CompOp2 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place2");
        var op_ob2 = CompOp2[0] as ITxObject;
        var op2 = CompOp2[0] as ITxOperation;
        
        TxObjectList CompOp3 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place3");
        var op_ob3 = CompOp3[0] as ITxObject;
        var op3 = CompOp3[0] as ITxOperation;

        TxObjectList CompOp4 = TxApplication.ActiveDocument.GetObjectsByName("Pick&Place4");
        var op_ob4 = CompOp4[0] as ITxObject;
        var op4 = CompOp4[0] as ITxOperation;
        
        // Add all the targeted operations as objects to the compound operation
        comp_op.AddObject(op_ob1);
        comp_op.AddObject(op_ob2);
        comp_op.AddObject(op_ob3);
        comp_op.AddObject(op_ob4);

    }
}