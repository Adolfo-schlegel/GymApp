using ArduinoClient.Tools;
using ArduinoClient.Tools.Log;
using ArduinoClient.WorkingService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Configuration;
using System.IO;
using System.IO.Ports;
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
			var host = CreateHostBuilder().Build();

			var arduinoManager = host.Services.GetRequiredService<ArduinoManager>();
			arduinoManager.StartReading();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Cliente(arduinoManager));			
		}

		private static IHostBuilder CreateHostBuilder() =>
		Host.CreateDefaultBuilder()
		.ConfigureServices((hostContext, services) =>
		{
			services.AddSingleton<ILogger, FileLogger>(provider =>
			{
				return new FileLogger(@"C:\TEMP\logs", "LogFile");
			});
			
			services.AddSingleton(provider => new SerialPort
			{
				PortName = ConfigurationManager.AppSettings["PuertoCOM"],
				BaudRate = 9600
			});
			
			services.AddSingleton<ArduinoManager>();
			
			services.AddSingleton<IReportSender, ReportSender>();
			
			services.AddSingleton<DailyWorker>(provider =>
			{
				var logger = provider.GetRequiredService<ILogger>();
				var reportSender = provider.GetRequiredService<IReportSender>();

				var worker = new DailyWorker(logger, reportSender)
				{
					//ExecutionTime = TimeSpan.FromHours(23), // Ejemplo de configuración
					ExecutionInterval = TimeSpan.FromSeconds(2) // Ejemplo de configuración
				};

				return worker;
			});
		});

	}
}

