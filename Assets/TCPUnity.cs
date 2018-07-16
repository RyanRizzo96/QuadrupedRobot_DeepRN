using UnityEngine;
using UnityEngine.UI;

using System;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Text.RegularExpressions;

public class TCPUnity : MonoBehaviour
{
	TcpClient tcp_socket;
	NetworkStream net_stream;
	StreamWriter socket_writer;
	StreamReader socket_reader;

	public String host = "localhost";
	public Int32 port = 50000;

	internal Boolean connected = false;
	internal Boolean canSend = true;

	internal String sendData = "";
	internal String receivedData = "";

	public InputField xInput;
	public InputField yInput;
	public InputField zInput;

	public float floatInput;
	public int intInput;

	public Text receivedTxt;
	public Text statusTxt;

	public void Connect()
	{
		if (!connected)
		{
			try
			{
				tcp_socket = new TcpClient(host, port);

				net_stream = tcp_socket.GetStream();
				socket_writer = new StreamWriter(net_stream);
				socket_reader = new StreamReader(net_stream);

				connected = true;
			}
			catch (Exception e)
			{
				statusTxt.text = "Socket error: " + e;
			}
		}
	}

	void Update()
	{
		if (connected)
		{
			if (!canSend)
				return;

			sendData = xInput.text + "," + yInput.text + "," + zInput.text + "," + floatInput + "," + intInput;
			writeSocket(sendData);

			string receivedData = readSocket();
			if (receivedData != "")
			{
				string[] splitString = Regex.Split(receivedData, ",");

				string r_xInput = splitString[0];
				string r_yInput = splitString[1];
				string r_zInput = splitString[2];

				float r_floatInput = float.Parse(splitString[3]);
				int r_intInput = int.Parse(splitString[4]);

				receivedTxt.text = r_xInput + "\n" + r_yInput + "\n" + r_zInput + "\n" + r_floatInput + "\n" + r_intInput;
				statusTxt.text = "Received";
				canSend = true;
			}
			else
			{
				statusTxt.text = "!Waiting for python data";
			}
		}
	}

	public void writeSocket(string line)
	{
		socket_writer.Write(line);
		socket_writer.Flush();
	}

	public String readSocket()
	{
		if (net_stream.DataAvailable)
			return socket_reader.ReadLine();

		return "";
	}

	public void Disconnect()
	{
		if (connected)
		{
			writeSocket("Disconnect");
			socket_writer.Close();
			socket_reader.Close();
			tcp_socket.Close();
			connected = false;
		}
	}

	void OnApplicationQuit()
	{
		Disconnect();
	}
}