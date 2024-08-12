using ArduinoClient.Tools.Log;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public class DailyWorker : IDailyWorker
	{
		private readonly IFileLogger _logger;
		private readonly IReportSender _reportSender;
		private Timer _timer;
		private DateTime _lastExecutionTime; 
		public TimeSpan? ExecutionTime { get; set; }
		public TimeSpan? ExecutionInterval { get; set; } 

		public DailyWorker(IEnumerable<IFileLogger> logger, IReportSender reportSender)
		{
			SetExecutionTime();

			_logger = logger.Where(x => x.TypeLogger == TypeLogger.DailyWorker).First();
			_reportSender = reportSender;
			_lastExecutionTime = DateTime.MinValue;
			_logger.Log($"{DateTime.Now} DailyWorker initialized.");

			StartWorking();
		}
		private void SetExecutionTime()
		{
			try
			{
				var executionTimeInHours = ConfigurationManager.AppSettings["DailyWorker.ExecutionTimeInHours"];
				var executionInterval = ConfigurationManager.AppSettings["DailyWorker.ExecutionIntervalMinutes"];

				ExecutionTime = string.IsNullOrEmpty(executionTimeInHours)
					? (TimeSpan?)null
					: TimeSpan.FromHours(Convert.ToDouble(executionTimeInHours));

				ExecutionInterval = string.IsNullOrEmpty(executionInterval)
					? (TimeSpan?)null
					: TimeSpan.FromHours(Convert.ToDouble(executionInterval));
			}
			catch(Exception ex)
			{
				_logger.Log($"{ex.Message}");
			}

		}
		public void StartWorking()
		{
			var thread = new Thread(() =>
			{
				_timer = new Timer(CheckTimeAndExecute, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
			})
			{
				Name = "DailyWorkerThread",
				IsBackground = true
			};
			thread.Start();

			_logger.Log($"{DateTime.Now} DailyWorker starting.\n");
		}
		
		private void CheckTimeAndExecute(object state)
		{
			try
			{
				var currentTime = DateTime.Now;

				//Ejecuta los trabajos por Hora indicada
				if (ExecutionTime.HasValue)
				{
					var executionDateTime = currentTime.Date + ExecutionTime.Value;
					if (currentTime >= executionDateTime && currentTime - _lastExecutionTime >= TimeSpan.FromDays(1))
					{
						ExecuteJobs();
						_lastExecutionTime = currentTime;
					}
				}

				//Ejecuta los trabajos por intervalo indicado
				if (ExecutionInterval.HasValue)
				{
					if (currentTime - _lastExecutionTime >= ExecutionInterval.Value)
					{
						ExecuteJobs();
						_lastExecutionTime = currentTime;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Log($"{DateTime.Now} Error in CheckTimeAndExecute: " + ex.Message);
			}
		}
		public void ExecuteJobs()
		{
			try
			{
				_logger.Log($"{DateTime.Now} Executing task at: " + DateTime.Now);

				var diskResult = _reportSender.SendToDisk();
				var emailResult = _reportSender.SendEmailReport();
				var googleDriveResult = _reportSender.SendGoogleDriveReport();

				_logger.Log($"{DateTime.Now} SendToDisk result -> " + diskResult);
				_logger.Log($"{DateTime.Now} SendEmailReport result -> " + emailResult);
				_logger.Log($"{DateTime.Now} SendGoogleDriveReport result -> " + googleDriveResult);

				_logger.Log($"{DateTime.Now} <<-End task\n");
			}
			catch (Exception ex)
			{
				_logger.Log($"{DateTime.Now} Error in ExecuteJobs: " + ex.Message);
			}
		}
		//private void ResetTimer()
		//{
		//	try
		//	{
		//		_timer?.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Reiniciar el temporizador
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger.Log("Error in ResetTimer: " + ex.Message);
		//	}
		//}
		public void Stop()
		{
			try
			{
				_timer?.Change(Timeout.Infinite, 0); // Detiene el temporizador
			}
			catch (Exception ex)
			{
				_logger.Log($"{DateTime.Now} Error in Stop: " + ex.Message);
			}
		}

		public void Dispose()
		{
			_timer?.Dispose();
		}
	}
}
