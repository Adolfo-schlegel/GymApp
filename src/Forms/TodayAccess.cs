using ArduinoClient.DB;
using ArduinoClient.WorkingService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoClient.Forms
{
	public partial class TodayAccess : Form
	{
		private ISqliteDataAccess _sqliteDataAccess;
		private IReportSender _reportSender;
		public TodayAccess(ISqliteDataAccess sqliteDataAccess, IReportSender reportSender)
		{
			_sqliteDataAccess = sqliteDataAccess;
			_reportSender = reportSender;
			InitializeComponent();
		}

		private async void btnEnviarReporte_ClickAsync(object sender, EventArgs e)
		{
			string result = await _reportSender.SendEmailReportAsync();

			if (result != "OK")
				MessageBox.Show("Error al enviar reporte, consulte con el tecnico: " + result);
			else
				MessageBox.Show($"Reporte Enviado a {ConfigurationManager.AppSettings["Email.To"]}");
		}

		private void TodayAccess_Load(object sender, EventArgs e)
		{
			int okCount = 0;
			int errorCount = 0;
			int totalCount = 0;

			var entries = _sqliteDataAccess.GetTodaysAccessSummary();

			// Limpiar el contenido inicial del TextBox
			txtRegistro.Text = "";

			entries.ForEach(usuario =>
			{
				var content = $"{usuario.Nombre} {usuario.Apellido} {usuario.Log}";

				// Añadir cada entrada en una nueva línea
				txtRegistro.Text += content + Environment.NewLine;

				// Contar los tipos de entradas
				if (usuario.Log.Contains("OK"))
				{
					okCount++;
				}
				else if (usuario.Log.Contains("Error"))
				{
					errorCount++;
				}

				// Acumular el total de ingresos
				totalCount += usuario.IngresoCount;
			});

			lblDenegados.Text = errorCount.ToString();
			lblPermitidos.Text = okCount.ToString();
			lblTotal.Text = totalCount.ToString();
		}
	}
}
