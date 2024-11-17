using ArduinoClient.DB;
using ArduinoClient.Models;
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
using System.Windows.Forms.DataVisualization.Charting;

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
			int totalPuertaAbierta = _sqliteDataAccess.GetActionCount("Door");
			int totalCreaciones = _sqliteDataAccess.GetActionCount("UsersCreated");
			int totalEliminaciones = _sqliteDataAccess.GetActionCount("UsersDeleted");
			int totalModificados = _sqliteDataAccess.GetActionCount("UsersModified");
			int totalIngresos = 0;
			

			if (entries.Count > 0)
			{
				// Initialize HTML message
				var message = $@"
<html>
<head>
    <style>
        body {{ font-family: 'Arial', sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
        .container {{ width: 80%; margin: 0 auto; background-color: white; padding: 20px; border-radius: 10px; box-shadow: 0px 0px 15px rgba(0, 0, 0, 0.1); }}
        .logo {{ text-align: center; }}
        .logo img {{ max-width: 150px; }}
        h2 {{ color: #2C3E50; text-align: center; font-size: 24px; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 20px; }}
        th, td {{ padding: 10px; border: 1px solid #ddd; text-align: left; }}
        th {{ background-color: #007BFF; color: white; }}
        .error-row {{ background-color: #8B0000; color: white; }} /* Red row with white text */
        .total {{ margin-top: 20px; font-size: 18px; text-align: right; }}
		.total-ingresos {{color: green; }}
		.total-puerta {{color: blue; }}
		.total-creaciones {{color: orange; }}
		.total-eliminaciones {{color: red; }}
		.total-modificaciones {{color: purple; }}
        footer {{ text-align: center; margin-top: 40px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>
            <img src='https://www.shutterstock.com/image-illustration/wolf-animal-body-builder-sports-260nw-1529580707.jpg' alt='SPORTLIFE Logo' />
        </div>
        <h2>Reporte de Ingresos - {date}</h2>
        <p>Resumen de los ingresos de hoy:</p>
        <table>
            <thead>
                <tr>
                    <th>Nombre</th>
                    <th>Apellido</th>
                    <th>Log</th>
                    <th>Celular</th>
                </tr>
            </thead>
            <tbody>";

				HashSet<int> uniqueUserIds = new HashSet<int>();

				entries.ForEach(usuario =>
				{
					var rowClass = usuario.Log.Contains("Error") ? "error-row" : ""; 

					var row = $@"
            <tr class='{rowClass}'>
                <td>{usuario.Nombre}</td>
                <td>{usuario.Apellido}</td>
                <td>{usuario.Log}</td>
                <td>{usuario.Celular}</td>
            </tr>";
					message += row;

					// Only increase the total if this Usuario_id hasn't been processed yet
					if (!uniqueUserIds.Contains(usuario.Usuario_id))
					{
						totalIngresos += usuario.IngresoCount;
						uniqueUserIds.Add(usuario.Usuario_id); // Mark this user as processed
					}
				});

				message += $@"
            </tbody>
        </table>
		<p class='total' style='color: blue;'>Total de ingresos con llavero: <strong>{totalIngresos}</strong></p>
        <p class='total' style='color: purple;'>Total de ingresos con sistema: <strong>{totalPuertaAbierta}</strong></p>
        <p class='total' style='color: orange;'>Total de nuevos usuarios: <strong>{totalCreaciones}</strong></p>
        <p class='total' style='color: red;'>Total de eliminaciones: <strong>{totalEliminaciones}</strong></p>
        <p class='total' style='color: green;'>Total de actualizaciones de cuota: <strong>{totalModificados}</strong></p>
    </div>
    <footer>
        <p>SPORTLIFE - Su aliado en el deporte y bienestar.</p>
        <p>Contacto Desarrollador: adolfo.77@outlook.es | Tel: +54 3446574460</p>
    </footer>
</body>
</html>";

				_sqliteDataAccess.ClearTodaysAccess();
				_sqliteDataAccess.DeleteAllActions();

				result = await _emailSender.SendEmailWithAttachmentAsync(emailFrom, emailTo, $"{totalIngresos} Ingresos de hoy {date}", message, excelFile);
			}
			else
			{
				result = "Las entidades fueron 0 al enviar el mail";
			}

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
