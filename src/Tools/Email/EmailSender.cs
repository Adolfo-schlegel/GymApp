using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ArduinoClient.Tools.Email
{
	public interface IEmailSender
	{
		Task<string> SendEmailAsync(string fromAddress, string toAddress, string subject, string body);
		Task<string> SendEmailWithAttachmentAsync(string fromAddress, string toAddress, string subject, string body, string file);
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
		public async Task<string> SendEmailAsync(string fromAddress, string toAddress, string subject, string body)
		{
			return await SendEmailWithAttachmentAsync(fromAddress, toAddress, subject, body, null);
		}

		// Método para enviar el correo con adjunto
		public async Task<string> SendEmailWithAttachmentAsync(string fromAddress, string toAddress, string subject, string body, string file)
		{
			var res = "OK";
			try
			{
				MailMessage mail = new MailMessage();
				mail.From = new MailAddress(fromAddress);
				mail.To.Add(toAddress);
				mail.Subject = subject;

				// Allow HTML content in the email body
				mail.IsBodyHtml = true;
				mail.Body = body;

				// Verifica si se debe adjuntar un archivo
				if (!string.IsNullOrEmpty(file) && File.Exists(file))
				{
					Attachment attachment = new Attachment(file);
					mail.Attachments.Add(attachment);
				}

				using (SmtpClient smtpClient = new SmtpClient(SmtpHost, SmtpPort))
				{
					smtpClient.Credentials = new NetworkCredential(SmtpUser, SmtpPass);
					smtpClient.EnableSsl = EnableSsl;
					await smtpClient.SendMailAsync(mail);
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
