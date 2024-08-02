using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public class DailyWorker : BackgroundService
	{
		private IReportSender _reportSender;

		public DailyWorker(IReportSender reportSender)
		{
			_reportSender = reportSender;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{



				Console.WriteLine("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
