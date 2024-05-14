using ArduinoClient.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace ArduinoClient
{
	internal static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			// Obtener la instancia de ArduinoManager
			ArduinoManager arduinoManager = ArduinoManager.GetInstance();

			//Empieza a leer datos del puerto Serial del arduino
			arduinoManager.StartReading();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Cliente(arduinoManager));

			
		}
	}
}
