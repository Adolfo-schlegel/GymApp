using WorkerService.Services;

namespace WorkerService
{
	public class Worker : BackgroundService
	{
		private readonly ILogger<Worker> _logger;
		private IReportSender _reportSender;

		public Worker(ILogger<Worker> logger, IReportSender reportSender)
		{
			_logger = logger;
			_reportSender = reportSender;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{





				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await Task.Delay(1000, stoppingToken);
			}
		}
	}
}
