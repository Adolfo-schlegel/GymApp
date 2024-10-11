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
				// Initialize HTML message
				var message = $@"
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; }}
                h2 {{ color: #2C3E50; }}
                table {{ width: 100%; border-collapse: collapse; }}
                th, td {{ padding: 8px 12px; border: 1px solid #ddd; text-align: left; }}
                th {{ background-color: #f4f4f4; }}
                .error-row {{ background-color: #8B0000; color: white; }} /* Red row with white text */
            </style>
        </head>
        <body>
            <h2>Reporte de Ingresos - {date}</h2>
            <p>Resumen de los ingresos de hoy:</p>
            <table>
                <thead>
                    <tr>
                        <th>Nombre</th>
                        <th>Apellido</th>
                        <th>Log</th>
                    </tr>
                </thead>
                <tbody>";

				int totalIngresos = 0;

				// Build table rows, checking for "Error" in usuario.Log
				entries.ForEach(usuario =>
				{
					var rowClass = usuario.Log.Contains("Error") ? "error-row" : ""; // Apply 'error-row' class if 'Error' is in the log
					var row = $@"
            <tr class='{rowClass}'>
                <td>{usuario.Nombre}</td>
                <td>{usuario.Apellido}</td>
                <td>{usuario.Log}</td>
            </tr>";
					message += row;

					totalIngresos += usuario.IngresoCount; // Acumula el total de ingresos
				});

				message += @"
                </tbody>
            </table>
            <p>Total de ingresos: <strong>" + totalIngresos + @"</strong></p>
        </body>
        </html>";

				_sqliteDataAccess.ClearTodaysAccess();

				// Send email with the HTML-formatted body
				result = await _emailSender.SendEmailWithAttachmentAsync(emailFrom, emailTo, $"{totalIngresos} Ingresos de hoy {date}", message, excelFile);
			}
			else
			{
				result = "Las entidades fueron 0 al enviar el mail";
			}

			return result;
		}


		//public async Task<string> SendEmailReportAsync()
		//{			 
		//	string result = "OK";
		//	string emailFrom = ConfigurationManager.AppSettings["Email.From"];
		//	string emailTo = ConfigurationManager.AppSettings["Email.To"];
		//	var date = DateTime.Now.ToString("dd-MM-yyyy");
		//	var entries = _sqliteDataAccess.GetTodaysAccessSummary();

		//	if (entries.Count > 0)
		//	{
		//		// Build an HTML-formatted message
		//		var message = $@"
		//          <h2>Reporte de Accesos - {date}</h2>
		//          <p>Total de ingresos: {entries.Count}</p>
		//          <ul>";

		//		int totalIngresos = 0;

		//		// Create an HTML list of users' access summary
		//		entries.ForEach(usuario =>
		//		{
		//			var content = $"<li>{usuario.Nombre} {usuario.Apellido}: {usuario.Log}</li>";
		//			_logger.Log($"{usuario.Nombre} {usuario.Apellido} {usuario.Log}");
		//			message += content;
		//			totalIngresos += usuario.IngresoCount; // Accumulate the total number of entries
		//		});

		//		message += "</ul>"; // Close the HTML list

		//		_sqliteDataAccess.ClearTodaysAccess();

		//		// Set the total ingresos in the subject and send the email
		//		result = await _emailSender.SendEmailWithAttachmentAsync(
		//			emailFrom,
		//			emailTo,
		//			$"{totalIngresos} Ingresos de hoy {date}",
		//			message,
		//			(excelFile) // Pass the attachment if available
		//		);
		//	}
		//	else
		//	{
		//		result = "Las entidades fueron 0 al enviar el mail";
		//	}

		//	return result;
		//}

		//public async Task<string> SendEmailReportAsync()
		//{
		//	string result = "OK";
		//	string emailFrom = ConfigurationManager.AppSettings["Email.From"];
		//	string emailTo = ConfigurationManager.AppSettings["Email.To"];
		//	var date = DateTime.Now.ToString("dd-MM-yyyy");
		//	var entries = _sqliteDataAccess.GetTodaysAccessSummary();

		//	if (entries.Count > 0)
		//	{
		//		var message = $"";
		//		int totalIngresos = 0;

		//		entries.ForEach(usuario =>
		//		{
		//			var content = $"{usuario.Nombre} {usuario.Apellido} {usuario.Log}";
		//			_logger.Log(content);
		//			message += "\n" + content;
		//			totalIngresos += usuario.IngresoCount; // Acumula el total de ingresos
		//		});

		//		_sqliteDataAccess.ClearTodaysAccess();

		//		result = await _emailSender.SendEmailWithAttachmentAsync(emailFrom, emailTo, $"{totalIngresos} Ingresos de hoy {date}", message, (excelFile));
		//	}
		//	else			
		//		result = "Las entidades fueron 0 al enviar el mail";

		//	return result;
		//}

		public string SendGoogleDriveReport()
		{

			return "OK";
		}

		public async Task<string> SendToDiskAsync()
		{
			string res = "OK";

			// Asumimos que LoadPeople devuelve una lista de manera síncrona.
			var entries = await Task.Run(() => _sqliteDataAccess.LoadPeople());

			if (entries.Count != 0)
			{
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
			}
			else
				return "Las entidades fueron 0 al guardar el archivo";
						
			return res;
		}

	}
}
