using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoClient.Tools.Email
{
	public interface IEmailSender
	{
		string SendEmail(string fromAddress, string toAddress, string subject, string body);
	}
	public class EmailSender : IEmailSender
	{
		// Propiedades para configuración SMTP
		public string SmtpHost { get; set; }
		public int SmtpPort { get; set; }
		public string SmtpUser { get; set; }
		public string SmtpPass { get; set; }
		public bool EnableSsl { get; set; }

		// Constructor
		public EmailSender(string smtpHost, int smtpPort, string smtpUser, string smtpPass, bool enableSsl)
		{
			SmtpHost = smtpHost;
			SmtpPort = smtpPort;
			SmtpUser = smtpUser;
			SmtpPass = smtpPass;
			EnableSsl = enableSsl;
		}

		// Método para enviar el correo
		public string SendEmail(string fromAddress, string toAddress, string subject, string body)
		{
			var res = "OK";
			try
			{
				MailMessage mail = new MailMessage();
				mail.From = new MailAddress(fromAddress);
				mail.To.Add(toAddress);
				mail.Subject = subject;
				mail.Body = body;

				using (SmtpClient smtpClient = new SmtpClient(SmtpHost, SmtpPort))
				{
					smtpClient.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
					smtpClient.EnableSsl = EnableSsl;
					smtpClient.Send(mail);
				}
			}
			catch (Exception ex)
			{
				res = "Error al enviar el correo: " + ex.Message;
			}

			return res;
		}
	}
}
