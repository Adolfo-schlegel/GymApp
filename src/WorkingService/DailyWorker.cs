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
		private readonly Tools.Log.ILogger _logger;
		private readonly IReportSender _reportSender;
		private Timer _timer;
		private TimeSpan? _executionTime; // Hora específica de ejecución (opcional)
		private TimeSpan? _executionInterval; // Intervalo de ejecución (opcional)
		private DateTime _lastExecutionTime; // Última hora de ejecución

		public TimeSpan? ExecutionTime
		{
			get => _executionTime;
			set
			{
				_executionTime = value;
				ResetTimer();
			}
		}
		public TimeSpan? ExecutionInterval
		{
			get => _executionInterval;
			set
			{
				_executionInterval = value;
				ResetTimer();
			}
		}

		public DailyWorker(Tools.Log.ILogger logger, IReportSender reportSender)
		{
			_logger = logger;
			_reportSender = reportSender;
			_lastExecutionTime = DateTime.MinValue;
			StartBackgroundThread();
		}

		private void StartBackgroundThread()
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
		}

		private void CheckTimeAndExecute(object state)
		{
			try
			{
				var currentTime = DateTime.Now;

				//Ejecuta los trabajos por Hora indicada
				if (_executionTime.HasValue)
				{
					var executionDateTime = currentTime.Date + _executionTime.Value;
					if (currentTime >= executionDateTime && currentTime - _lastExecutionTime >= TimeSpan.FromDays(1))
					{
						ExecuteJobs();
						_lastExecutionTime = currentTime;
					}
				}

				//Ejecuta los trabajos por intervalo indicado
				if (_executionInterval.HasValue)
				{
					if (currentTime - _lastExecutionTime >= _executionInterval.Value)
					{
						ExecuteJobs();
						_lastExecutionTime = currentTime;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Log("Error in CheckTimeAndExecute: " + ex.Message);
			}
		}
		private void ExecuteJobs()
		{
			try
			{
				_logger.Log("Executing task at: " + DateTime.Now);

				var diskResult = _reportSender.SendToDisk();
				var emailResult = _reportSender.SendEmailReport();
				var googleDriveResult = _reportSender.SendGoogleDriveReport();

				_logger.Log("SendToDisk result -> " + diskResult);
				_logger.Log("SendEmailReport result -> " + emailResult);
				_logger.Log("SendGoogleDriveReport result -> " + googleDriveResult);

				_logger.Log("End task");
			}
			catch (Exception ex)
			{
				_logger.Log("Error in ExecuteJobs: " + ex.Message);
			}
		}
		private void ResetTimer()
		{
			try
			{
				_timer?.Change(TimeSpan.Zero, TimeSpan.FromMinutes(1)); // Reiniciar el temporizador
			}
			catch (Exception ex)
			{
				_logger.Log("Error in ResetTimer: " + ex.Message);
			}
		}
		public void Stop()
		{
			try
			{
				_timer?.Change(Timeout.Infinite, 0); // Detiene el temporizador
			}
			catch (Exception ex)
			{
				_logger.Log("Error in Stop: " + ex.Message);
			}
		}
	}
}
