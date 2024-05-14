using ArduinoClient.Models;
using ArduinoClient.Tools;
using SpreadsheetLight;
using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Windows.Forms;
namespace ArduinoClient
{
	public partial class NewUsuario : Form
	{
		private Thread hilo;
		public string Code { get; set; }
		private ArduinoManager arduinoManager;
		public NewUsuario(ArduinoManager arduinoManager)
		{			
			InitializeComponent();
			this.arduinoManager = arduinoManager;

			hilo = new Thread(listenSerial);
			hilo.Name = "UserIdProcess";
			hilo.Start();
		}
		private void NewUsuario_Load(object sender, EventArgs e)
		{
			lblCodigo.Text = Code;

			dateTimePicker1.Text = DateTime.Now.ToShortDateString();
		}
		private void btnGuardarUsuario_Click(object sender, EventArgs e)
		{
			try
			{
				SqliteDataAccess.SaveUser(getUserFromTextbox());
				MessageBox.Show("Usuario agregado: " + txtNombre.Text);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			hilo.Suspend();
		}
		public UsuarioDB getUserFromTextbox()
		{
			return new UsuarioDB()
			{
				Codigo = lblCodigo.Text,
				Nombre = txtNombre.Text,
				Apellido = txtApellido.Text,
				Documento = long.Parse(txtDocumento.Text),
				Sexo = comboBox1.Text,
				Celular = long.Parse(txtCelular.Text),
				MedioPago = cbMedio.Text,
				Fecha = dateTimePicker1.Text,
				Monto = long.Parse(txtMonto.Text),
				Correo = txtAddres.Text + "@" + txtCorreo.Text
			};
		}

		private void listenSerial()
		{
			while (true)
			{
				var data = arduinoManager.GetNextReceivedData();

				if (data != null)
				{
					var code = data.Replace("Card UID: ", "");

					lblCodigo.Invoke(new MethodInvoker(
							delegate
							{
								lblCodigo.Text = code;
							}));

				}
			}
		}

		private void NewUsuario_FormClosed(object sender, FormClosedEventArgs e)
		{
			hilo.Suspend();
		}
	}
}
