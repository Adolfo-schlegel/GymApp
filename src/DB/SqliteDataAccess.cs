using ArduinoClient.DB;
using ArduinoClient.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient
{
	public class SqliteDataAccess : ISqliteDataAccess
	{
		public List<UsuarioDB> LoadPeople()
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				var output = cnn.Query<UsuarioDB>("select * from Usuario", new DynamicParameters());
				return output.ToList();
			}
		}
		public void LogDateAccessUser(int userId, string logLine)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				string query = "UPDATE Usuario SET Log = IFNULL(Log, '') || @LogLine || char(10) WHERE Id = @UserId";
				cnn.Execute(query, new { LogLine = logLine, UserId = userId });
			}
		}
		public void SaveUser(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				cnn.Execute("insert into Usuario (Codigo, Nombre, Apellido, Documento,Sexo, Celular, MedioPago, Fecha, Monto, Correo, EstadoCorreo) values (@Codigo, @Nombre, @Apellido, @Documento, @Sexo, @Celular, @MedioPago, @Fecha, @Monto, @Correo, @EstadoCorreo)", user);
			}
		}
		public void ModifyUser(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				//cnn.Execute("update Usuario set Codigo = @Codigo, Nombre = @Nombre, Apellido = @Apellido, Documento = @Documento, Sexo = @Sexo, Celular = @Celular, MedioPago = @MedioPago, Fecha = @Fecha, Monto = @Monto, Correo = @Correo, EstadoCorreo = @EstadoCorreo where Id = @Id", user);

				//Comentar esta linea para que no se guarde el LOG
				cnn.Execute("update Usuario set Codigo = @Codigo, Nombre = @Nombre, Apellido = @Apellido, Documento = @Documento, Sexo = @Sexo, Celular = @Celular, MedioPago = @MedioPago, Fecha = @Fecha, Monto = @Monto, Correo = @Correo, EstadoCorreo = @EstadoCorreo, Log = @Log where Id = @Id", user);
			}
		}
		public void UpdateQuota(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				cnn.Execute("update Usuario set  Fecha = @Fecha where Id = @Id", user);
			}
		}

		public void DeleteUser(int id)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				cnn.Execute("delete from Usuario where Id = @Id", new { Id = id });
			}
		}
		private string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}
		public List<string> GetLogEntries()
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				var entries = cnn.Query<(string Nombre, string Log)>("SELECT Nombre, Log FROM Usuario", new DynamicParameters());

				var result = new List<string>();

				foreach (var entry in entries)
				{
					if (!string.IsNullOrEmpty(entry.Log))
					{
						var logLines = entry.Log.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
						foreach (var line in logLines)
						{
							result.Add($"{entry.Nombre} {line}");
						}
					}
				}

				return result;
			}
		}
	}
}
