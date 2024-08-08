using ArduinoClient.DB;
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
		public ReportSender(IEnumerable<IFileLogger> logger, ISqliteDataAccess sqliteDataAccess) 
		{
			_sqliteDataAccess = sqliteDataAccess;
			_logger = logger.Where(x => x.TypeLogger == TypeLogger.ReportSender).First();
		}
		public string SendEmailReport()
		{
			_logger.Log("");
			
			var entries = _sqliteDataAccess.GetLogEntries();
			
			entries.ForEach(x=> _logger.Log(x));

			//Email.Send(pablo.gym@gmail.com, entries)
			return "OK";
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
