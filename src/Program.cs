using ArduinoClient.DB;
using ArduinoClient.Forms;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Arduino;
using ArduinoClient.Tools.Email;
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

			//Background Services
			var _dailyWorker = host.Services.GetRequiredService<IDailyWorker>();
			var _arduinoManager = host.Services.GetRequiredService<IArduinoManager>();

			//Run App
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(host.Services.GetRequiredService<Cliente>());			
		}

		private static IHostBuilder CreateHostBuilder() =>
		Host.CreateDefaultBuilder()
		.ConfigureServices((hostContext, services) =>
		{
			services.AddSingleton<ISqliteDataAccess, SqliteDataAccess>();

			services.AddSingleton<IReportSender, ReportSender>();
		
			services.AddTransient<IEmailSender>(provider =>
				new EmailSender(
					smtpHost: "smtp.gmail.com",
					smtpPort: 587,
					smtpUser: ConfigurationManager.AppSettings["Email.From"], 
					smtpPass: "fghmesdmyzgpqefv", 
					enableSsl: true
				)
			);
			services.AddSingleton<IFileLogger, FileLogger>(provider => 
			new FileLogger(ConfigurationManager.AppSettings["Log.Folder"], "DailyWorkerLogFile") { TypeLogger = TypeLogger.DailyWorker });

			services.AddSingleton<IFileLogger, FileLogger>(provider =>
			new FileLogger(ConfigurationManager.AppSettings["Log.Folder"], "ReportSenderLogFile") { TypeLogger = TypeLogger.ReportSender });

			services.AddSingleton(provider => new SerialPort
			{
				PortName = ConfigurationManager.AppSettings["PuertoCOM"],
				BaudRate = 9600
			});
			
			services.AddSingleton<IArduinoManager, ArduinoManager>();
			services.AddSingleton<IDailyWorker,DailyWorker>();
			services.AddTransient<Cliente>();
		});
	}
}

