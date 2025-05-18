using System;
using System.Collections.Generic;
using System.Globalization;
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
			DateTime fechaUsuario = DateTime.ParseExact(Fecha, "dd/M/yyyy", CultureInfo.InvariantCulture).Date;
			DateTime fechaActual = DateTime.Today;

			// Comprobamos si la diferencia es de 30 días o menos
			return fechaActual.Subtract(fechaUsuario).TotalDays <= 30;
		}

		public int daysLeft()
		{
			string[] formatosAceptados = {
		"dd/MM/yyyy", "d/M/yyyy",  // Con y sin ceros
		"dd-MM-yyyy", "d-M-yyyy",  // También con guiones
	};

			DateTime fechaUsuario;
			if (!DateTime.TryParseExact(Fecha, formatosAceptados, CultureInfo.InvariantCulture,
										DateTimeStyles.None, out fechaUsuario))
			{
				throw new FormatException("Fecha inválida: " + Fecha);
			}

			fechaUsuario = fechaUsuario.Date;
			DateTime fechaActual = DateTime.Today;

			// Calcula los días restantes hasta que la fecha esté desactualizada
			TimeSpan diferencia = fechaUsuario.AddDays(30) - fechaActual;
			return (int)diferencia.TotalDays;
		}

	}
}
