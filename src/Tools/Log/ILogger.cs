using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Tools.Log
{
	public interface ILogger
	{
		void Log(string headers, string bodyContent);
	}
}
