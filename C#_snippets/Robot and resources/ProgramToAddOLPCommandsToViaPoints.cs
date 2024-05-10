/*
This snippet allows to add OLP commands to a specific waypoint, that is specified by its name. The sequence of
commands is always the same and it is exactly the one that can be generated manually in the simualtion environment.
(Path Editor -> OLP Commands -> Add -> Standard Commands -> Part Handling -> Grip/Release)
*/

using System;
using System.IO;
using System.Windows.Forms;
using Tecnomatix.Engineering;
using System.Collections;

 
public class MainScript
{
  public static void DummyOLPCommand()
  {
  
  	// Store p2 in a variable called Waypoint 	
  	TxRoboticViaLocationOperation Waypoint =  TxApplication.ActiveDocument.
  	GetObjectsByName("p2")[0] as TxRoboticViaLocationOperation;
  	
  	// Store the gripper "Camozzi gripper"  	
  	ITxObject Gripper = TxApplication.ActiveDocument.
	GetObjectsByName("Camozzi gripper")[0] as TxGripper;
  	
  	// Store the pose "Gripper Closed"	
  	ITxObject Pose = TxApplication.ActiveDocument.
	GetObjectsByName("Gripper Closed")[0] as TxPose;
  	
  	// Store the reference frame "tgripper_tf" 	
  	ITxObject tGripper = TxApplication.ActiveDocument.
	GetObjectsByName("tgripper_tf")[0] as TxFrame;

	// Create an array called "elements" and the command to be written in it
    ArrayList elements1 = new ArrayList();
    ArrayList elements2 = new ArrayList();
    ArrayList elements3 = new ArrayList();
    ArrayList elements4 = new ArrayList();
    ArrayList elements5 = new ArrayList();

    var myCmd1 = new TxRoboticCompositeCommandStringElement("# Destination");
    var myCmd11 = new TxRoboticCompositeCommandTxObjectElement(Gripper);
    var myCmd2 = new TxRoboticCompositeCommandStringElement("# Drive");
    var myCmd21 = new TxRoboticCompositeCommandTxObjectElement(Pose);
    var myCmd3 = new TxRoboticCompositeCommandStringElement("# Destination");
    var myCmd31 = new TxRoboticCompositeCommandTxObjectElement(Gripper);
    var myCmd4 = new TxRoboticCompositeCommandStringElement("# WaitDevice");
    var myCmd41 = new TxRoboticCompositeCommandTxObjectElement(Pose);
    var myCmd5 = new TxRoboticCompositeCommandStringElement("# Grip");
    var myCmd51 = new TxRoboticCompositeCommandTxObjectElement(tGripper);

 	// Add the command to elements 
    elements1.Add(myCmd1);
    elements1.Add(myCmd11);

	// Create the real command      
    TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData1 =
    new TxRoboticCompositeCommandCreationData(elements1);

    Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData1);
    
    elements2.Add(myCmd2);
    elements2.Add(myCmd21);

	// Create the real command      
    TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData2 =
    new TxRoboticCompositeCommandCreationData(elements2);

    Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData2);
    
    elements3.Add(myCmd3);
    elements3.Add(myCmd31);

	// Create the real command     
    TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData3 =
    new TxRoboticCompositeCommandCreationData(elements3);

    Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData3);
    
    elements4.Add(myCmd4);
    elements4.Add(myCmd41);

	// Create the real command     
    TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData4 =
    new TxRoboticCompositeCommandCreationData(elements4);

    Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData4);
    
    elements5.Add(myCmd5);
    elements5.Add(myCmd51);

	// Create the real command     
    TxRoboticCompositeCommandCreationData txRoboticCompositeCommandCreationData5 =
    new TxRoboticCompositeCommandCreationData(elements5);

    Waypoint.CreateCompositeCommand(txRoboticCompositeCommandCreationData5);


  }

}