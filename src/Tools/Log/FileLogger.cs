using ArduinoClient.Tools.Log;
using DocumentFormat.OpenXml.Bibliography;
using System;
using System.IO;

namespace ArduinoClient.Tools
{
	public class FileLogger : ILogger
	{
		private string _logFilePath;
		private FileStream _fileStream;

		public FileLogger(string logDirectory, string logFileName)
		{
			if(!Directory.Exists(logDirectory))
				Directory.CreateDirectory(logDirectory);

			_logFilePath = logDirectory +@"\"+ logFileName +"-"+ DateTime.Now.ToString("yyyy") + ".log";

			_fileStream = new FileStream(_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
		}

		public void Log(string text)
		{
			var isEmpty = !File.Exists(_logFilePath) || new FileInfo(_logFilePath).Length == 0;		

			using (var writer = new StreamWriter(_fileStream))
			{
				// If the file is empty, write the headers first
				if (isEmpty)
				{
					writer.WriteLine(DateTime.Today);
				}

				writer.WriteLine(text);
			}
		}
	}
}
