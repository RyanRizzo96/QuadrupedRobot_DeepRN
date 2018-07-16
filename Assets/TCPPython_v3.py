#!/usr/bin/env python
# GIT test

import test
import socket 

host = '' 
port = 50000
backlog = 5
size = 1024

socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
socket.bind((host, port))
socket.listen(backlog)

test.test_func()  # prints "Hello!"
print("Server Started")

while True:
        connection = None
        try:
            connection, address = socket.accept()
            print("Unity Connected.")
            # connection.send("First call from Python\n")
           
            while True:
                    data = connection.recv(size).decode()
                    if data:
                        if data == "Disconnect":
                            print("Connection closed by Unity")
                            connection.close()
                            exit()
                            break
                        else:
                            valuesList = data.split(",")
                            xInput = valuesList[0]
                            yInput = valuesList[1]
                            zInput = valuesList[2]
                            floatInput = float(valuesList[3])     # used to send float to C#
                            intInput = int(valuesList[4])
                            msg = xInput + "," + yInput + "," + zInput + "," + str(floatInput) + "," + str(intInput) + "\r\n"
                            connection.send(msg.encode('ascii'))  # .encode returns a byte representation of the string
        except KeyboardInterrupt:
                if connection:
                    connection.close()
                    break
