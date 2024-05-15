# Socket-based communication

## C# side
The first thing to do is to copy and paste 'TPS_main.cs' in the '.NET Script' viewer in TPS, debug it and then run. This code allows to simulate some operations and get, for each of them, the 5 indices to calculate the OWAS ergonomic index. These values are passed to python.
## Python side
The python code takes the 5 values from C# and uses them as inputs to enter in the hard-coded table in 'OWAS_table.py': by crossing rows and columns, a sinlge value is obtained and it is fed back to C#.

## Snippets
Please do not change the snippets present in the folder 'C#_snippets'