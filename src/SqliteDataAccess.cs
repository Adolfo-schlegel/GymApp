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
	public class SqliteDataAccess
	{
		public static List<UsuarioDB> LoadPeople()
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Open();
				var output = cnn.Query<UsuarioDB>("select * from Usuario", new DynamicParameters());
				return output.ToList();
			}
		}
		public static void SaveUser(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Execute("insert into Usuario (Codigo, Nombre, Apellido, Documento,Sexo, Celular, MedioPago, Fecha, Monto, Correo, EstadoCorreo) values (@Codigo, @Nombre, @Apellido, @Documento, @Sexo, @Celular, @MedioPago, @Fecha, @Monto, @Correo, @EstadoCorreo)", user);
			}
		}
		public static void ModifyUser(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Execute("update Usuario set Codigo = @Codigo, Nombre = @Nombre, Apellido = @Apellido, Documento = @Documento, Sexo = @Sexo, Celular = @Celular, MedioPago = @MedioPago, Fecha = @Fecha, Monto = @Monto, Correo = @Correo, EstadoCorreo = @EstadoCorreo where Id = @Id", user);
			}
		}
		public static void UpDateUser(UsuarioDB user)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Execute("update Usuario set  Fecha = @Fecha where Id = @Id", user);
			}
		}

		public static void DeleteUser(int id)
		{
			using (IDbConnection cnn = new SQLiteConnection(LoadConnectionString()))
			{
				cnn.Execute("delete from Usuario where Id = @Id", new { Id = id });
			}
		}
		private static string LoadConnectionString(string id = "Default")
		{
			return ConfigurationManager.ConnectionStrings[id].ConnectionString;
		}
	}
}
