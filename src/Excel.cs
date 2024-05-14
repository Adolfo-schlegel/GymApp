using SpreadsheetLight;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ArduinoClient
{
	public class Excel
	{
		private static SLDocument file;
		private static SLWorksheetStatistics stats;
		private static string FilePath;

		public Excel()
		{
			LoadExcel();
		}
		private static void deleteUserFromExcel(int row)
		{
			file.DeleteRow(row, 1);

			file.SaveAs(FilePath);
		}
		private void AgregarEncabezados()
		{
			file.SetCellValue(1, 1, "ID");
			file.SetCellValue(1, 2, "Nombre");
			file.SetCellValue(1, 3, "Apellido");
			file.SetCellValue(1, 4, "Documento");
			file.SetCellValue(1, 5, "Sexo");
			file.SetCellValue(1, 6, "Celular");
			file.SetCellValue(1, 7, "Medio de Pago");
			file.SetCellValue(1, 8, "Fecha");
			file.SetCellValue(1, 9, "Monto");
			file.SetCellValue(1, 10, "Correo");

			file.SaveAs(FilePath);
		}
		private void LoadExcel()
		{
			if (ConfigurationManager.AppSettings["FilePath"] != "")
				FilePath = ConfigurationManager.AppSettings["FilePath"];
			else
				FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "archivo.xlsm");


			file = new SLDocument(FilePath);
			stats = file.GetWorksheetStatistics();
		}
		private static void AgregarFila(string id, string nombre, string apellido, string documento, string sexo, string celular, string medioPago, string fecha, string monto, string correo)
		{
			try
			{
				//// Busca la última fila con datos
				int lastRowWithData = stats.EndRowIndex;
				while (lastRowWithData > 1 && file.GetCellValueAsString(lastRowWithData, 1) == "")
				{
					lastRowWithData--;
				}

				int newRow = lastRowWithData + 1;

				file.SetCellValue(newRow, 1, id);
				file.SetCellValue(newRow, 2, nombre);
				file.SetCellValue(newRow, 3, apellido);
				file.SetCellValue(newRow, 4, documento);
				file.SetCellValue(newRow, 5, sexo);
				file.SetCellValue(newRow, 6, celular);
				file.SetCellValue(newRow, 7, medioPago);
				file.SetCellValue(newRow, 8, fecha);
				file.SetCellValue(newRow, 9, monto); 
				file.SetCellValue(newRow, 10, correo);

				file.SaveAs(FilePath);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Caracter no valido \n" + ex.Message);
			}
		}
		public static void ModificarUsuario(string numero, string id, string nombre, string apellido, string documento, string sexo, string celular, string medioPago, string fecha, string monto, string correo)
		{
			var row = Convert.ToInt32(numero);
			
			try
			{
				file.SetCellValue(row, 1, id);
				file.SetCellValue(row, 2, nombre);
				file.SetCellValue(row, 3, apellido);
				file.SetCellValue(row, 4, documento);
				file.SetCellValue(row, 5, sexo);
				file.SetCellValue(row, 6, celular);
				file.SetCellValue(row, 7, medioPago);
				file.SetCellValue(row, 8, DateTime.Parse(fecha));
				file.SetCellValue(row, 9, decimal.Parse(monto)); 
				file.SetCellValue(row, 10, correo);

				file.SaveAs(FilePath);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Caracter no valido \n" + ex.Message);
			}
		}
	}
}
