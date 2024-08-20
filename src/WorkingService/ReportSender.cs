using ArduinoClient.DB;
using ArduinoClient.Tools.Email;
using ArduinoClient.Tools.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public class ReportSender : IReportSender
	{
		IFileLogger _logger;
		ISqliteDataAccess _sqliteDataAccess;
		IEmailSender _emailSender;
		public ReportSender(IEnumerable<IFileLogger> logger, ISqliteDataAccess sqliteDataAccess, IEmailSender emailSender) 
		{
			_emailSender = emailSender;
			_sqliteDataAccess = sqliteDataAccess;
			_logger = logger.Where(x => x.TypeLogger == TypeLogger.ReportSender).First();
		}
		public string SendEmailReport()
		{
			string result = "OK";

			var entries = _sqliteDataAccess.GetTodaysAccessSummary();
			
			if (entries.Count > 0)
			{
				var message = $"";

				entries.ForEach(usuario =>
				{
					var content = $"{usuario.Nombre} {usuario.Apellido} {usuario.Log}";

					_logger.Log(content);
					message += "\n" + content;
				});

				_sqliteDataAccess.ClearTodaysAccess();

				result = _emailSender.SendEmail("crosspablo23@gmail.com", "adolfo.77@outlook.es", $"{entries.FirstOrDefault().IngresoCount} Ingresos de hoy {DateTime.Today}", message);
			}

			return result;
		}

		public string SendGoogleDriveReport()
		{

			return "OK";
		}

		public string SendToDisk()
		{

			return "OK";
		}
	}
}
