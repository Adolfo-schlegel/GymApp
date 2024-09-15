using ArduinoClient.Tools.Log;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.IO;
using System.IO.Packaging;

namespace ArduinoClient.Tools
{
	public class FileLogger : IFileLogger
	{
		private readonly string _logFilePath;

		public FileLogger(string logDirectory, string logFileName)
		{
			if (!Directory.Exists(logDirectory))
				Directory.CreateDirectory(logDirectory);
			
			_logFilePath = Path.Combine(logDirectory, logFileName + "-" + DateTime.Now.ToString("yyyy") + ".log");
		}
		public TypeLogger TypeLogger { get; set; }
		public void Log(string text)
		{
			try
			{
				var isEmpty = !File.Exists(_logFilePath) || new FileInfo(_logFilePath).Length == 0;

				using (var writer = new StreamWriter(_logFilePath,true))
				{
					if (isEmpty)
					{
						writer.WriteLine($"{DateTime.Now} -INIT-");
					}

					writer.WriteLine($"{text}");
				}
			}
			catch (Exception ex	)
			{				
				throw new Exception("Failed to write log", ex);
			}
			
		}
	}
}
