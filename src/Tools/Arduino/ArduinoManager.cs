using ArduinoClient.Tools.Arduino;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoClient.Tools
{
	public class ArduinoManager : IArduinoManager
	{
			
		private static ManualResetEvent suspendEvent = new ManualResetEvent(false);
		private static ManualResetEvent resumeEvent = new ManualResetEvent(false);
		private Queue<string> receivedDataQueue = new Queue<string>();
		public SerialPort _serialPort;
		public Thread _readThread;
		
		private object threadLock = new object();
		private object queueLock = new object();
		public bool reading = false;

		public ArduinoManager(SerialPort serialPort)
		{
			this._serialPort = serialPort;
			SystemEvents.PowerModeChanged += OnPowerModeChanged;
			InitArduino();
			InitThread();			
			StartReading();
		}
		public void WriteToSerialPort(string data)
		{
			try
			{
				if (_serialPort != null && _serialPort.IsOpen)
				{
					_serialPort.WriteLine(data);
				}
				else
				{
					Console.WriteLine("Serial port is not open.");
				}
			}
			catch (IOException ioex)
			{
				Console.WriteLine($"Error writing to serial port (IO exception): {ioex.Message}");
				// Intentar reabrir el puerto si ocurre una desconexión
				ReopenArduinoPort();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error writing to serial port: {ex.Message}");
			}
		}
		public void OpenPort()
		{
			if (_serialPort != null && !_serialPort.IsOpen)
			{
				_serialPort.Open();
				Console.WriteLine("Puerto Arduino abierto correctamente.");
			}
			else
			{
				Console.WriteLine("Puerto Arduino NO abierto correctamente.");
			}
		}

		public void ClosePort()
		{
			if (_serialPort != null && _serialPort.IsOpen)
			{
				_serialPort.Close();
				Console.WriteLine("Puerto Arduino cerrado correctamente.");
			}
			else
			{
				Console.WriteLine("Puerto Arduino NO cerrado correctamente.");
			}
		}

		public bool IsPortOpen()
		{
			return _serialPort != null && _serialPort.IsOpen;
		}

		public void DisposePort()
		{
			if (_serialPort != null)
			{
				_serialPort.Dispose();
				Console.WriteLine("Puerto Arduino liberado correctamente.");
			}
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
			_readThread = new Thread(ReadArduinoData)
			{
				Name = "ReaderArduinoProcess",
				IsBackground = true // Hilo en segundo plano
			};
		}
		private void InitArduino()
		{
			try
			{
				if (!_serialPort.IsOpen)
				{
					_serialPort.Open();
					Task.Delay(1000);
				}
			}
			catch (TimeoutException tex)
			{
				Console.WriteLine("Tiempo de espera excedido al comunicarse con el Arduino: " + tex.Message);
			}
			catch (UnauthorizedAccessException uex)
			{
				Console.WriteLine("Acceso denegado al puerto COM: " + uex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error al inicializar el puerto Arduino: " + ex.Message);
			}
		}
		public void StartReading()
		{
			lock (threadLock)
			{
				if (!reading)
				{
					reading = true;
					if (_readThread == null || !_readThread.IsAlive)
					{
						Console.WriteLine("StartReading");
						InitThread(); // Crea un nuevo hilo antes de iniciarlo
						_readThread.Start();
					}
				}
			}
		}
		private void CloseArduinoPort()
		{
			try
			{
				if (_serialPort != null && _serialPort.IsOpen)
				{
					_serialPort.Close();
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
					if (!_serialPort.IsOpen)
					{
						_serialPort.Open();
						Console.WriteLine("Puerto Arduino reabierto correctamente.");
						StartReading();
						break;
					}
				}
				catch (UnauthorizedAccessException)
				{
					Console.WriteLine("Acceso denegado al puerto COM. Reintentando...");
					Task.Delay(1000);
				}
				catch (IOException ioex)
				{
					Console.WriteLine("Error I/O al intentar reabrir el puerto: " + ioex.Message);
					break;
				}
			}
		}
		public void StopReading()
		{
			reading = false;
			if (_readThread != null && _readThread.IsAlive)
			{
				_readThread.Join();
				Console.WriteLine("Hilo de lectura detenido correctamente.");
				_readThread = null; // Asegúrate de que el hilo se puede recrear
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

					if (_serialPort != null && _serialPort.IsOpen && _serialPort.BytesToRead > 0)
					{						
						string data = _serialPort.ReadExisting();
						Task.Delay(100);

						lock (queueLock)
						{
							Console.WriteLine($"CODIGO-> {data}");

							receivedDataQueue.Enqueue(data.Trim());

							Task.Delay(100);
						}								
					}
					else
					{
						Task.Delay(100);
					}
				}
			}
			catch (IOException ioex)
			{
				Console.WriteLine("Excepción de IO en ReadArduinoData: " + ioex.Message);
				ReopenArduinoPort(); // Intentar reabrir el puerto
			}
			catch (TimeoutException tex)
			{
				Console.WriteLine("Tiempo de espera en la lectura de datos: " + tex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Excepción en ReadArduinoData: " + ex.Message);
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
