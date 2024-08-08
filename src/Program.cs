using ArduinoClient.DB;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Log;
using ArduinoClient.WorkingService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Threading.Tasks;
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

			var sqliteDataAccess = host.Services.GetRequiredService<ISqliteDataAccess>();

			var arduinoManager = host.Services.GetRequiredService<ArduinoManager>();
			arduinoManager.StartReading();

			var dailyWorker = host.Services.GetRequiredService<DailyWorker>();		
			//dailyWorker.ExecutionTime = TimeSpan.FromHours(23);
			dailyWorker.ExecutionInterval = TimeSpan.FromMinutes(1); 
			dailyWorker.StartWorking();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Cliente(arduinoManager, sqliteDataAccess));			
		}

		private static IHostBuilder CreateHostBuilder() =>
		Host.CreateDefaultBuilder()
		.ConfigureServices((hostContext, services) =>
		{
			services.AddSingleton<ISqliteDataAccess, SqliteDataAccess>();

			services.AddSingleton<IReportSender, ReportSender>();

			services.AddSingleton<IFileLogger, FileLogger>(provider => 
			new FileLogger(@"C:\TEMP\logs", "DailyWorkerLogFile") { TypeLogger = TypeLogger.DailyWorker });

			services.AddSingleton<IFileLogger, FileLogger>(provider =>
			new FileLogger(@"C:\TEMP\logs", "ReportSenderLogFile") { TypeLogger = TypeLogger.ReportSender });

			services.AddSingleton(provider => new SerialPort
			{
				PortName = ConfigurationManager.AppSettings["PuertoCOM"],
				BaudRate = 9600
			});
			
			services.AddSingleton<ArduinoManager>();

			services.AddSingleton<DailyWorker>();
		});

	}
}

