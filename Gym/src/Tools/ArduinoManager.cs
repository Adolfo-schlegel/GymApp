using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace ArduinoClient.Tools
{
	public class ArduinoManager
	{
		private static ArduinoManager instance;
		public SerialPort serialPort;
		private Thread readThread;
		private bool reading = false;
		private Queue<string> receivedDataQueue = new Queue<string>();
		private object queueLock = new object();

		private ArduinoManager()
		{
			InitThread();
			InitArduino();
		}
		private void InitThread()
		{
			readThread = new Thread(ReadArduinoData);
			readThread.Name = "ReaderArduinoProcess";
		}
		private void InitArduino()
		{
			try
			{
				serialPort = new SerialPort();
				serialPort.PortName = ConfigurationManager.AppSettings["PuertoCOM"];
				serialPort.BaudRate = 9600;

				if (!serialPort.IsOpen)
				{										
					serialPort.Open();

					Thread.Sleep(1000);
				}
			}
			catch (TimeoutException tex)
			{
				MessageBox.Show("Tiempo de espera excedido al comunicarse con el Arduino: " + tex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Se desconecto el dispositivo del puerto especificado" + ex.Message);
			}
		}
		public static ArduinoManager GetInstance()
		{
			if (instance == null)
			{
				instance = new ArduinoManager();
			}
			return instance;
		}
		public SerialPort GetArduino()
		{
			return serialPort;
		}
		public void StartReading()
		{
			if (!reading)
			{
				reading = true;
				readThread.Start();
			}
		}
		public void StopReading()
		{
			reading = false;
			if (readThread != null && readThread.IsAlive)
			{
				readThread.Join(); 
			}
		}
		private void ReadArduinoData()
		{
			try
			{
				while (reading)
				{
					if (serialPort.BytesToRead > 0)
					{
						string data = serialPort.ReadLine();
						var line = data.Contains("Card UID:") ? data.Trim() : null;

						lock (queueLock)
						{
							receivedDataQueue.Enqueue(line);
						}
					}
					else
					{
						Thread.Sleep(100); 
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error en ReadArduinoData " + ex.Message);
			}
		}
		public string GetNextReceivedData()
		{
			string content = "";
			try
			{
				lock (queueLock)
				{
					if (receivedDataQueue.Count > 0)
					{
						content = receivedDataQueue.Dequeue();
					}
					else
					{
						return null; 
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error en GetNextReceivedData " + ex.Message);
			}
			return content;
		}
		
	}
}
