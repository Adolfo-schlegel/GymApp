using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Models
{
	public class UsuarioDB
	{
		public int Id { get; set; }
		public string Codigo { get; set; }
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public long Documento { get; set; }
		public string Sexo { get; set; }
		public long Celular { get; set; }
		public string MedioPago { get; set; }
		public string Fecha { get; set; }
		public long Monto { get; set; }
		public string Correo { get; set; }
		public int EstadoCorreo { get; set; }
		public string Log { get; set; }

		public bool isUpToDate()
		{
			// Normalizamos las fechas
			DateTime fechaUsuario = DateTime.Parse(Fecha).Date;
			DateTime fechaActual = DateTime.Today;

			// Comprobamos si la diferencia es de 30 días o menos
			return fechaActual.Subtract(fechaUsuario).TotalDays <= 30;
		}

		public int daysLeft()
		{
			// Normalizamos ambas fechas para ignorar la hora
			DateTime fechaUsuario = DateTime.Parse(Fecha).Date; // Solo la fecha
			DateTime fechaActual = DateTime.Today; // Ya es solo la fecha

			// Calcula los días restantes hasta que la fecha esté desactualizada
			TimeSpan diferencia = fechaUsuario.AddDays(30) - fechaActual;
			return (int)diferencia.TotalDays;
		}
	}
}
