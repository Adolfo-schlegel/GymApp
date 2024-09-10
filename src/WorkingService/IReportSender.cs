using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public interface IReportSender
	{
		Task<string> SendEmailReportAsync();
		string SendGoogleDriveReport();
		Task<string> SendToDiskAsync();

		//void SendSmsReport();
	}
}
