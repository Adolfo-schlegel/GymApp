using ArduinoClient.DB;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Email;
using ArduinoClient.Tools.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoClient.WorkingService
{
	public class ReportSender : IReportSender
	{
		IFileLogger _logger;
		ISqliteDataAccess _sqliteDataAccess;
		IEmailSender _emailSender;
		IExcelManager _excelManager;
		public ReportSender(IEnumerable<IFileLogger> logger, ISqliteDataAccess sqliteDataAccess, IEmailSender emailSender, IExcelManager excelManager) 
		{
			_excelManager = excelManager;
			_emailSender = emailSender;
			_sqliteDataAccess = sqliteDataAccess;
			_logger = logger.Where(x => x.TypeLogger == TypeLogger.ReportSender).First();
		}
		public string SendEmailReport()
		{
			string result = "OK";
			string emailFrom = ConfigurationManager.AppSettings["Email.From"];
			string emailTo = ConfigurationManager.AppSettings["Email.To"];
			string date = DateTime.Now.ToString("dd/MM/yyyy - HH:mm");
			var entries = _sqliteDataAccess.GetTodaysAccessSummary();
			
			if (entries.Count > 0)
			{
				var message = $"";
				int totalIngresos = 0;

				entries.ForEach(usuario =>
				{
					var content = $"{usuario.Nombre} {usuario.Apellido} {usuario.Log}";
					_logger.Log(content);
					message += "\n" + content;
					totalIngresos += usuario.IngresoCount; // Acumula el total de ingresos
				});

				_sqliteDataAccess.ClearTodaysAccess();

				result = _emailSender.SendEmail(emailFrom, emailTo, $"{totalIngresos} Ingresos de hoy {date}", message);
			}
			else			
				result = "Las entidades fueron 0 al enviar el mail";
			
			return result;
		}

		public string SendGoogleDriveReport()
		{

			return "OK";
		}

		public string SendToDisk()
		{
			string res = "OK";
			string path = ConfigurationManager.AppSettings["Log.Folder"];

			var entries = _sqliteDataAccess.LoadPeople();

			if (entries.Count < 0) { return "Las entidades fueron 0 al guardar el archivo"; }

			try
			{
				var date = DateTime.Now.ToString("dd-MM-yyyy");

				_excelManager.AddSheet($"Gym - {date}");

				_excelManager.AddItems(entries);

				_excelManager.SaveAs(path + @"\" + "RegistroGym - " + date);

				res = "Archivo guardado exitosamente en: " + path;
			}
			catch (Exception ex)
			{
				res = "Error al guardar el archivo: " + ex.Message;
			}
			return res;
		}
	}
}
