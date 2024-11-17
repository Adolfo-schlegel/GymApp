using ArduinoClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.DB
{
	public interface ISqliteDataAccess
	{
		void DeleteAllActions();
		void UpdateActionCount(string actionName, int newCount);
		int GetActionCount(string actionName);
		List<UsuarioDB> LoadPeople();
		void LogHistoricalDateAccessUser(int userId, string logLine);
		void SaveUser(UsuarioDB user);
		void ModifyUser(UsuarioDB user);
		void UpdateQuota(UsuarioDB user);
		void DeleteUser(int id);
		List<string> GetHistoricalLogsEntries();
		List<UserAccessSummary> GetTodaysAccessSummary();
		void ClearHistoricalLogs();
		void ClearTodaysAccess();
		void LogTodaysAccess(int userId, string logLine);
	}
}
