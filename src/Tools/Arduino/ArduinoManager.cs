using Microsoft.Win32;
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
		public Thread readThread;
		public bool reading = false;
		private Queue<string> receivedDataQueue = new Queue<string>();
		private object queueLock = new object();
		private static ManualResetEvent suspendEvent = new ManualResetEvent(false);
		private static ManualResetEvent resumeEvent = new ManualResetEvent(false);
		private object threadLock = new object();

		public ArduinoManager(SerialPort serialPort)
		{
			this.serialPort = serialPort;
			SystemEvents.PowerModeChanged += OnPowerModeChanged;
			InitArduino();
			InitThread();			
		}
		
		private void OnPowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Suspend)
			{
				Console.WriteLine("Sistema suspendido. Deteniendo thread...");
				suspendEvent.Set(); // Detiene el hilo estableciendo el evento
				StopReading();
				CloseArduinoPort();  // Cierra el puerto serie
			}
			else if (e.Mode == PowerModes.Resume)
			{
				Console.WriteLine("Sistema reanudado. Reiniciando thread...");
				ReopenArduinoPort(); // Reabre el puerto serie después de la reanudación
			}
		}

		private void InitThread()
		{
			readThread = new Thread(ReadArduinoData)
			{
				Name = "ReaderArduinoProcess",
				IsBackground = true // Hilo en segundo plano
			};
		}
		private void InitArduino()
		{
			try
			{
				//Se movio esta dependencia al contructor para ser inyectada desde el contenedor de dependencias en program.cs
				//serialPort = new SerialPort
				//{
				//	PortName = ConfigurationManager.AppSettings["PuertoCOM"],
				//	BaudRate = 9600
				//};

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
			catch (UnauthorizedAccessException uex)
			{
				MessageBox.Show("Acceso denegado al puerto COM: " + uex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error al inicializar el puerto Arduino: " + ex.Message);
			}
		}
		//public static ArduinoManager GetInstance()
		//{
		//	if (instance == null)
		//	{
		//		instance = new ArduinoManager();
		//	}
		//	return instance;
		//}
		//public SerialPort GetArduino()
		//{
		//	return serialPort;
		//}
		//public void StartReading()
		//{
		//	if (!reading)
		//	{
		//		reading = true;
		//		readThread.Start();
		//
		//		if (readThread == null || !readThread.IsAlive)
		//		{
		//			InitThread();
		//			readThread.Start();
		//		}
		//	}
		//}
		public void StartReading()
		{
			lock (threadLock)
			{
				if (!reading)
				{
					reading = true;
					if (readThread == null || !readThread.IsAlive)
					{
						InitThread(); // Crea un nuevo hilo antes de iniciarlo
						readThread.Start();
					}
				}
			}
		}
		private void CloseArduinoPort()
		{
			try
			{
				if (serialPort != null && serialPort.IsOpen)
				{
					serialPort.Close();
					Console.WriteLine("Puerto Arduino cerrado correctamente.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error cerrando el puerto Arduino: " + ex.Message);
			}
		}
		private void ReopenArduinoPort()
		{
			CloseArduinoPort();  // Asegura que el puerto esté cerrado antes de intentar reabrirlo

			for (int attempt = 0; attempt < 5; attempt++)
			{
				try
				{
					serialPort.Open();
					Console.WriteLine("Puerto Arduino reabierto correctamente.");
					StartReading();
					suspendEvent.Reset(); // Reanuda el hilo reiniciando el evento
					resumeEvent.Set(); // Señala que el sistema ha reanudado
					break;
				}
				catch (UnauthorizedAccessException)
				{
					Console.WriteLine("Acceso denegado al puerto COM. Reintentando...");
					Thread.Sleep(1000); // Espera antes de reintentar
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error reabriendo el puerto Arduino: " + ex.Message);
					break;
				}
			}
		}
		public void StopReading()
		{
			reading = false;
			if (readThread != null && readThread.IsAlive)
			{
				readThread.Join();
				Console.WriteLine("Hilo de lectura detenido correctamente.");
				readThread = null; // Asegúrate de que el hilo se puede recrear
			}
		}
		private void ReadArduinoData()
		{
			try
			{
				while (reading)
				{
					// Verifica si el hilo debe esperar debido a la suspensión
					if (suspendEvent.WaitOne(0))
					{
						Console.WriteLine("El hilo se quedo en espera...");
						suspendEvent.WaitOne(); // Espera hasta que se limpie el evento de suspensión
					}

					if (!reading) break;//Si no esta leyendo rompe el bucle

					if (serialPort != null && serialPort.IsOpen && serialPort.BytesToRead > 0)
					{
						Console.WriteLine("Leyendo datos desde el Arduino...");
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
			catch (TimeoutException ex)
			{
				MessageBox.Show("Error en ReadArduinoData TIMEOUT: " + ex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error en ReadArduinoData: " + ex.Message);
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
