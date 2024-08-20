using ArduinoClient.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
		ISqliteDataAccess _sqliteDataAccess;
		public TodayAccess(ISqliteDataAccess sqliteDataAccess)
		{
			_sqliteDataAccess = sqliteDataAccess;
			InitializeComponent();
		}

		private void btnEnviarReporte_Click(object sender, EventArgs e)
		{

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
