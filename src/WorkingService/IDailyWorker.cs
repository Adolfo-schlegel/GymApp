using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.WorkingService
{
	public interface IDailyWorker
	{
		TimeSpan? ExecutionTime { get; set; }
		TimeSpan? ExecutionInterval { get; set; }

		void StartWorking();
		void Stop();
		void ExecuteJobs();
	}
}
