using ArduinoClient.Tools.Log;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public class DailyWorker
	{
		private Tools.Log.ILogger _logger;
		private IReportSender _reportSender;
		private Thread _thread;
		private readonly CancellationTokenSource _cancellationTokenSource;
		public DailyWorker(Tools.Log.ILogger logger, IReportSender reportSender)
		{
			_cancellationTokenSource = new CancellationTokenSource();
			_logger = logger;
			_reportSender = reportSender;
			InitThread();
		}
		public void InitThread()
		{
			_thread = new Thread(() => SendingProcess(_cancellationTokenSource.Token))
			{
				Name = "SendingProcess",
				IsBackground = true
			};
			_thread.Start();
		}
		private void SendingProcess(CancellationToken cancellationToken)
		{
			_logger.Log("Worker", "Worker starting at: " + DateTimeOffset.Now);
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					//var result = _reportSender.SendToDisk();
					_logger.Log("Resultado", "Hola");
					Thread.Sleep(1000); // Sleep for 1 second before the next iteration
				}
			}
			catch (Exception ex)
			{
				_logger.Log("Worker", "Worker encountered an error: " + ex.Message);
			}
			finally
			{
				_logger.Log("Worker", "Worker stopping at: " + DateTimeOffset.Now);
			}
		}
		public void Stop()
		{
			_cancellationTokenSource.Cancel();
			_thread.Join(); 
		}
	}
}
