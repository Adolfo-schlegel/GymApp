using System;
using System.IO.Ports;
using System.Threading;

public delegate void DataReceivedEventHandler(string data);

public class SerialPortManager
{

	// Define el evento que se disparará cuando se reciban datos
	public event DataReceivedEventHandler DataReceived;

	private SerialPort serialPort;
	private bool isConnected;

	public SerialPortManager(string portName = "COM13", int baudRate = 9600)
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
		string data = serialPort.ReadExisting(); 
		data = data.Contains("Card UID:") ? data.Trim() : null;

		if (data != null) DataReceived?.Invoke(data);
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
