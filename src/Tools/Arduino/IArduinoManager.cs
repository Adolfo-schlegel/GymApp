using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Tools.Arduino
{
	public interface IArduinoManager
	{
		void StartReading();
		void StopReading();
		string GetNextReceivedData();
		string WriteToSerialPort(string data);
		void OpenPort();
		void ClosePort();
		bool IsPortOpen();
		void DisposePort();
	}
}
