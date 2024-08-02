using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Services
{
	public interface IReportSender
	{
		string SendEmailReport();
		string SendGoogleDriveReport();
		string SendToDisk();

		//void SendSmsReport();
	}
}
