using System;
using System.IO.Ports;
using System.Threading;

public class SerialPortManager
{
	private SerialPort serialPort;
	private bool isConnected;

	public SerialPortManager(string portName, int baudRate)
	{
		serialPort = new SerialPort(portName, baudRate);
		serialPort.DataReceived += SerialPort_DataReceived;
	}

	public void Connect()
	{
		if (!serialPort.IsOpen)
		{
			try
			{
				serialPort.Open();
				isConnected = true;
				Console.WriteLine("Serial port connected.");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error connecting to serial port: " + ex.Message);
				isConnected = false;
			}
		}
	}

	public void Disconnect()
	{
		if (serialPort.IsOpen)
		{
			serialPort.Close();
			isConnected = false;
			Console.WriteLine("Serial port disconnected.");
		}
	}

	private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		// Handle data received from the serial port
	}

	public bool IsConnected()
	{
		return isConnected;
	}

	public void Reconnect()
	{
		Disconnect();
		Thread.Sleep(1000); // Espera 1 segundo antes de intentar reconectar
		Connect();
	}
}
