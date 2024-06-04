"""
This code is the Python side of the communication between Python and C#.
It works according to the scheme proposed on OneNote in the section "Tecnomatix - Architecture of the TCP socket communication".
First, the C# is launched (Fn + F5) and then this Python code is executed.
a) from C#, the python code receives the time and the RULA score of the simulation.
b) on the basis of these values, the python code performs the Bayesian Optimization and sends the command to C#.
Of course, for this example, the command is simply the sum of 1 to each element of the array containing the time and the RULA score
(additional_var is added in order to increase the number of elements to be packed and sent to C#).
c) from C#, the python code receives the variable 'trigger_end' that is used to stop the communication when the maximum 
number of simulations is reached.
"""

# Import libraries (.dll files)

import socket
import numpy as np

# Define the host and port to receive data (both decided by me)

host = "127.0.0.1"
port = 12345

# Initialize the trigger to stop the socket communication and the number of simulations to be performed

trigger_end = 0
Nsim = 4

# Define a varibale in order to increase the number of elements to be packed and sent to C#

additional_var = 1

x_place_1 = 475
y_place_1 = -200
z_place_1 = 200

x_place_2 = 300
y_place_2 = -100
z_place_2 = 200

x_place_3 = 400
y_place_3 = 100
z_place_3 = 200


obj_index = np.array([1, 2, 3])

# Start the real communication

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as client : # creation of the socket

    client.connect((host, port)) # connect to the server
    print("The connection has happened successfully!")
    
    """
    Fundamental part of the code:
    Until the trigger reaches the maximum number of simulations, keep looping
    """
    
    while trigger_end < Nsim - 1 : # keep looping until you receive a variable that is smaller than the maximum number of iterations
        
        print("\n")
        print(f" -*-*-*-*-*-*-*- The current simulation on TPS is the number: {trigger_end + 1} -*-*-*-*-*-*-*- ")
        print("\n")
        
        # a receive the kpi(s) (parameters to be optimized) from the C# code (they are surely integers)

        kpi = client.recv(1024).decode()
        kpi = [int(num) for num in kpi.split(',')] # list variable
                  
        # Transform the data into a numpy array
            
        kpi_vec = np.array(kpi)
        print(f"The time and RULA score for ergonomic assessment received from C# are: {kpi_vec}")
      
        # Isolate time (first element) and RULA score (second element) (rememebr to divide by a certain number 
        # in order to restore the original value)

        SimTime = kpi_vec[0] /1000
        print(f"The time taken by the simulation number {trigger_end + 1} is : {SimTime} second(s)")

        # resulting array conatining the 'tentative' layout to be sent to C# code

        layout = np.array([[int((x_place_1) * 1000), int((y_place_1) * 1000), int((z_place_1) * 1000), int((obj_index[0]) * 1000),
                            int((x_place_3) * 1000), int((y_place_3) * 1000), int((z_place_3) * 1000), int((obj_index[2]) * 1000)]])
        
        print(f"The tentative layout is : {layout}")
    
        shape_layout = np.array(layout.shape, dtype = np.int32)  
        print(f"The shape of the command is : {shape_layout}")
        
        # Actual send of the data (in the future: try to remove the double send and try to send just one time)
         
        client.sendall(shape_layout.tobytes())
        client.sendall(layout.tobytes())
        
        # c Receive the variable 'trigger_end' from C# code (this time this is surely a single number)
        
        trigger_end = client.recv(1024).decode() 
        trigger_end = int(''.join(map(str, trigger_end))) # transform the string into an integer
        
    client.close() # close the socket once the condition for the while loop is not satisfied anymore
        
