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
		private string excelFile = ConfigurationManager.AppSettings["Excel.File"];
		public ReportSender(IEnumerable<IFileLogger> logger, ISqliteDataAccess sqliteDataAccess, IEmailSender emailSender, IExcelManager excelManager) 
		{
			_excelManager = excelManager;
			_emailSender = emailSender;
			_sqliteDataAccess = sqliteDataAccess;
			_logger = logger.Where(x => x.TypeLogger == TypeLogger.ReportSender).First();
		}
		public async Task<string> SendEmailReportAsync()
		{
			string result = "OK";
			string emailFrom = ConfigurationManager.AppSettings["Email.From"];
			string emailTo = ConfigurationManager.AppSettings["Email.To"];
			var date = DateTime.Now.ToString("dd-MM-yyyy");
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

				result = await _emailSender.SendEmailWithAttachmentAsync(emailFrom, emailTo, $"{totalIngresos} Ingresos de hoy {date}", message, (excelFile));
			}
			else			
				result = "Las entidades fueron 0 al enviar el mail";
			
			return result;
		}

		public string SendGoogleDriveReport()
		{

			return "OK";
		}

		public async Task<string> SendToDiskAsync()
		{
			string res = "OK";

			// Asumimos que LoadPeople devuelve una lista de manera síncrona.
			var entries = await Task.Run(() => _sqliteDataAccess.LoadPeople());

			if (entries.Count == 0)
			{
				return "Las entidades fueron 0 al guardar el archivo";
			}

			try
			{
				var date = DateTime.Now.ToString("dd-MM-yyyy");

				await Task.Run(() =>
				{
					_excelManager.Initialize();
					_excelManager.AddSheet($"Gym - {date}");
					_excelManager.AddItems(entries);
					_excelManager.SaveAs(excelFile);
					//_excelManager.RemoveSheet($"Gym - {date}");
				});

				res = "Archivo guardado exitosamente en: " + excelFile + date;
			}
			catch (Exception ex)
			{
				res = "Error al guardar el archivo: " + ex.Message;
			}

			return res;
		}

	}
}
