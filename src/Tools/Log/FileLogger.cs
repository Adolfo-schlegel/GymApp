using ArduinoClient.Tools.Log;
using System;
using System.IO;

namespace ArduinoClient.Tools
{
	public class FileLogger : ILogger
	{
		private readonly string _logDirectory;
		private readonly string _logFileName;

		public FileLogger(string logDirectory, string logFileName)
		{
			logFileName += GetLogFileName();

			_logDirectory = logDirectory;
			_logFileName = logFileName;

			Directory.CreateDirectory(logDirectory); 
		}
		private string GetLogFileName()
		{
			return DateTime.Now.ToString("yyyy") + ".log";
		}
		public void Log(string headers ,string bodyContent)
		{
			
			var logFilePath = _logDirectory + _logFileName;

			// Check if the file is empty
			var isEmpty = !File.Exists(logFilePath) || new FileInfo(logFilePath).Length == 0;

			FileStream fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

			// Write the log message to the log file
			using (var writer = new StreamWriter(fileStream))
			{
				// If the file is empty, write the headers first
				if (isEmpty)
				{
					writer.WriteLine(headers);
				}

				writer.WriteLine(bodyContent);
			}
		}
	}
}
