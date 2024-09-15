using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Services
{
	public class ReportSender : IReportSender
	{
		public string SendEmailReport()
		{

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
