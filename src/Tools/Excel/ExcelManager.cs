using ArduinoClient.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArduinoClient.Tools
{
	public interface IExcelManager
	{
		void AddSheet(string sheetName);
		void AddItems(List<UsuarioDB> items);
		void PrintRowInRed(int row);
		void PrintRowInblue(int row);
		void Calculate(string cell, string formula);
		void SetStyleByRange(System.Drawing.Color bkgrColor, System.Drawing.Color fontColor, int fontSize, bool blod = true, bool italic = false, string range = "A1:D1");
		void SetStyleSheet(System.Drawing.Color bkgrColor, System.Drawing.Color fontColor, int fontSize, bool blod = true, bool italic = false);
		void SelectWorksheetByName(string sheetName);
		string ImportFromCsv(string csvFilePath, string csvFileName, char separator, bool header = true);
		void SaveAs(string path);
		void RemoveSheet(string sheetName);
		void Initialize();
	}

	public class ExcelManager : IExcelManager
	{
		XLWorkbook workbook;
		IXLWorksheet worksheet;
		public ExcelManager()
		{
			workbook = new XLWorkbook();
		}
		public void Initialize()
		{
			workbook = new XLWorkbook();
		}
		public void AddSheet(string sheetName)
		{
			worksheet = workbook.Worksheets.Add(sheetName);
			AddHeaders();
		}

		public void AddItems(List<UsuarioDB> items)
		{
			int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
			int row = lastRow + 1; 

			foreach (var item in items)
			{
				worksheet.Cell(row, 1).Value = item.Id;
				worksheet.Cell(row, 2).Value = item.Nombre;
				worksheet.Cell(row, 3).Value = item.Apellido;
				worksheet.Cell(row, 4).Value = item.Fecha;
				worksheet.Cell(row, 5).Value = item.Monto;
				worksheet.Cell(row, 6).Value = item.Documento;
				worksheet.Cell(row, 7).Value = item.Sexo;
				worksheet.Cell(row, 8).Value = item.Celular;
				worksheet.Cell(row, 9).Value = item.MedioPago;
				worksheet.Cell(row, 10).Value = item.Correo;
				worksheet.Cell(row, 11).Value = item.Codigo;
				//worksheet.Cell(row, 12).Value = item.Log;

				if (!item.isUpToDate())
				{
					PrintRowInRed(row);
				}

				row++;
			}
		}
		public void PrintRowInRed(int row)
		{
			var range = worksheet.Row(row).CellsUsed();
			range.Style.Fill.BackgroundColor = XLColor.Red;
			range.Style.Font.FontColor = XLColor.White;
		}
		public void PrintRowInblue(int row)
		{
			var range = worksheet.Row(row).CellsUsed();
			range.Style.Fill.BackgroundColor = XLColor.Blue;
			range.Style.Font.FontColor = XLColor.White;
		}
		public void Calculate(string cell, string formula)
			=> worksheet.Cell(cell).FormulaA1 = formula;
		private void AddHeaders()
		{
			worksheet.Cell(1, 1).Value = "ID";
			worksheet.Cell(1, 2).Value = "Nombre";
			worksheet.Cell(1, 3).Value = "Apellido";
			worksheet.Cell(1, 4).Value = "Fecha";
			worksheet.Cell(1, 5).Value = "Monto";
			worksheet.Cell(1, 6).Value = "Documento";
			worksheet.Cell(1, 7).Value = "Sexo";
			worksheet.Cell(1, 8).Value = "Celular";
			worksheet.Cell(1, 9).Value = "Medio";
			worksheet.Cell(1, 10).Value = "Correo";
			worksheet.Cell(1, 11).Value = "Codigo";
			worksheet.Cell(1, 11).Value = "Log";

			PrintRowInblue(1);
		}

		public void SetStyleByRange(System.Drawing.Color bkgrColor, System.Drawing.Color fontColor, int fontSize, bool blod = true, bool italic = false, string range = "A1:D1")
		{
			worksheet.Range(range).Style.Font.SetFontColor(ClosedXML.Excel.XLColor.FromColor(fontColor));
			worksheet.Range(range).Style.Font.Italic = italic;
			worksheet.Range(range).Style.Font.Bold = blod;
			worksheet.Range(range).Style.Font.SetFontSize(fontSize);
			worksheet.Range(range).Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.FromColor(bkgrColor));
		}
		public void SetStyleSheet(System.Drawing.Color bkgrColor, System.Drawing.Color fontColor, int fontSize, bool blod = true, bool italic = false)
		{
			worksheet.Style.Font.SetFontColor(ClosedXML.Excel.XLColor.FromColor(fontColor));
			worksheet.Style.Font.Italic = italic;
			worksheet.Style.Font.Bold = blod;
			worksheet.Style.Font.SetFontSize(fontSize);
			worksheet.Style.Fill.SetBackgroundColor(ClosedXML.Excel.XLColor.FromColor(bkgrColor));
		}
		public void SelectWorksheetByName(string sheetName)
			=> worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);

		//TODO: Options: Inside a new sheet, on a new file, with a custom style or new style
		public string ImportFromCsv(string csvFilePath, string csvFileName, char separator, bool header = true)
		{
			if (!File.Exists(csvFilePath + @"\" + csvFileName))
				return "Archivo no encontrado";

			int row = 1;
			foreach (string line in File.ReadLines(csvFilePath + @"\" + csvFileName).Skip(header == false ? 1 : 0))
			{
				string[] parts = line.Split(separator);
				for (int col = 1; col <= parts.Length; col++)
				{
					worksheet.Cell(row, col).Value = parts[col - 1];
				}
				row++;
			}

			return "OK";
		}
		public void RemoveSheet(string sheetName)
		{
			var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
			if (sheet != null)
			{
				workbook.Worksheets.Delete(sheetName);
			}
		}
		public void SaveAs(string path)
		{
			workbook.SaveAs(path);
		}
	}
}
