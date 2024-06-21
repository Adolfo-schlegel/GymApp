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

		public bool isUpToDate() => DateTime.Today.Subtract(DateTime.Parse(Fecha)).TotalDays <= 30;

		public int daysLeft()
		{
			// Calcula los días restantes hasta que la fecha esté desactualizada
			TimeSpan diferencia = DateTime.Parse(Fecha).AddDays(30) - DateTime.Today;
			return (int)diferencia.TotalDays;
		}
	}
}
