import socket
import numpy as np
import Ergonomic_indicators as ei

# Define the host and port to receive data (both decided by me)
host = "127.0.0.1"
port = 12345

# Static variables
trigger_end = 0
Nsim = 2
additional_var = 1

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
        
        # a) receive the kpi(s) (parameters to be optimized) from the C# code (they are surely integers)

        owas_values = client.recv(1024).decode()
        owas_values = [int(num) for num in owas_values.split(',')] # list variable
                  
        # Transform the data into a numpy array
            
        owas_vec = np.array(owas_values)
        print(f"The OWAS indicators are: {owas_vec}")
        
        # Evaluate the OWAS
        owas_index = ei.owas_value(owas_vec[0], owas_vec[1], owas_vec[2], owas_vec[3], ei.owas_table)
       
        # resulting array conatining the OWAS score
        score = np.array([[int(owas_index)]])   
        shape_layout = np.array(score.shape, dtype = np.int32)  
        
        # Actual send of the data (in the future: try to remove the double send and try to send just one time)
         
        client.sendall(shape_layout.tobytes())
        client.sendall(score.tobytes())
        
        # c) Receive the variable 'trigger_end' from C# code (this time this is surely a single number)
        
        trigger_end = client.recv(1024).decode() 
        trigger_end = int(''.join(map(str, trigger_end))) # transform the string into an integer
        
    client.close() # close the socket once the condition for the while loop is not satisfied anymore
        
