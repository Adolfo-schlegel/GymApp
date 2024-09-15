using ArduinoClient.Tools.Arduino;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Linq;
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
		private object queueLock = new object();
		public bool reading = false;
		private Thread monitoringThread;
		private bool keepMonitoring = true;

		public ArduinoManager(SerialPort serialPort)
		{
			this._serialPort = serialPort;
			SystemEvents.PowerModeChanged += OnPowerModeChanged;
			InitArduino();
			StartMonitoring();
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
				suspendEvent.Set();
				StopReading();
				CloseArduinoPort();
			}
			else if (e.Mode == PowerModes.Resume)
			{
				Console.WriteLine("Sistema reanudado. Reiniciando thread...");
				ReopenArduinoPort();
			}
		}

		private void InitArduino()
		{
			try
			{
				if (!_serialPort.IsOpen)
				{
					_serialPort.Open();
					_serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
					_serialPort.PinChanged += new SerialPinChangedEventHandler(PinChangedHandler);
					Thread.Sleep(1000); // Use Thread.Sleep in synchronous context
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
			try
			{
				if (_serialPort != null)
				{
					if (_serialPort.IsOpen)
					{
						_serialPort.Close();
						Console.WriteLine("Puerto Arduino cerrado para reabrir.");
					}

					_serialPort.Open();
					_serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
					_serialPort.PinChanged += new SerialPinChangedEventHandler(PinChangedHandler);
					_serialPort.DiscardInBuffer(); // Flush the input buffer
					Console.WriteLine("Puerto Arduino reabierto correctamente.");
				}
			}
			catch (UnauthorizedAccessException uex)
			{
				Console.WriteLine("Acceso denegado al puerto COM: " + uex.Message);
			}
			catch (IOException ioex)
			{
				Console.WriteLine("Error de IO al reabrir el puerto Arduino: " + ioex.Message);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error al reabrir el puerto Arduino: " + ex.Message);
			}
		}

		public void StopReading()
		{
			reading = false;
			Console.WriteLine("Lectura detenida correctamente.");
		}

		private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				if (_serialPort != null && _serialPort.IsOpen)
				{
					SerialPort sp = (SerialPort)sender;
					string data = sp.ReadExisting();

					if (!string.IsNullOrWhiteSpace(data) && data.All(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || char.IsPunctuation(c)))
					{
						lock (queueLock)
						{
							Console.WriteLine($"CODIGO-> {data}");
							receivedDataQueue.Enqueue(data.Trim());
						}
					}		
				}
			}
			catch (IOException ioex)
			{
				Console.WriteLine("Error de IO en DataReceivedHandler: " + ioex.Message);
				ReopenArduinoPort();
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error en DataReceivedHandler: " + ex.Message);
			}
		}

		private void PinChangedHandler(object sender, SerialPinChangedEventArgs e)
		{
			if (e.EventType == SerialPinChange.CDChanged || e.EventType == SerialPinChange.DsrChanged)
			{
				Console.WriteLine("Cambio detectado en el estado del pin. Verificando conexión...");
				if (!_serialPort.IsOpen)
				{
					Console.WriteLine("El puerto parece estar desconectado. Intentando reabrir...");
					ReopenArduinoPort();
				}
			}
		}

		private void StartMonitoring()
		{
			monitoringThread = new Thread(MonitorPort);
			monitoringThread.IsBackground = true;
			monitoringThread.Start();
		}

		private void MonitorPort()
		{
			while (keepMonitoring)
			{
				if (_serialPort != null && !_serialPort.IsOpen)
				{
					Console.WriteLine("El puerto está desconectado. Intentando reabrir...");
					ReopenArduinoPort();
				}
				Thread.Sleep(1000); // Check every second
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
