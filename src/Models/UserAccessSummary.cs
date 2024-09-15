using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Models
{
	public class UserAccessSummary
	{
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public string Log { get; set; }
		public int IngresoCount { get; set; }
	}
}
