// Copyright 2019 Siemens Industry Software Ltd.
using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using Jack.Toolkit;
using System.Collections;
using System.Collections.Generic;
using Tecnomatix.Engineering.DataTypes.Graphics;
using Tecnomatix.Planning;

public class MainScript
{
    
    public static void MainWithOutput(ref StringWriter output)
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
		
		// DIsplay the results
        output.Write("Back : " + back_code.ToString() + output.NewLine);
        output.Write("Arm : " + arm_code.ToString() + output.NewLine);
        output.Write("Leg : " + leg_code.ToString() + output.NewLine);
        output.Write("Head : " + head_code.ToString() + output.NewLine);
        output.Write("Load : " + load_code.ToString() + output.NewLine);
    }
}
