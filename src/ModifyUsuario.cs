using ArduinoClient.Models;
using ArduinoClient.Tools;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ArduinoClient
{
	public partial class ModifyUsuario : Form
	{
		private Thread hilo;
		private ArduinoManager arduinoManager;
		public ModifyUsuario(ArduinoManager arduinoManager)
		{
			InitializeComponent();
			this.arduinoManager = arduinoManager;

			hilo = new Thread(listenSerial);
			hilo.Start();
		}
		
		private void btnGuardarUsuario_Click(object sender, EventArgs e)
		{
			try
			{
				SqliteDataAccess.ModifyUser(getUserFromTextbox());				
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			hilo.Suspend();
			MessageBox.Show("Usuario agregado: " + txtNombre.Text);			
		}
		public UsuarioDB getUserFromTextbox()
		{
			return new UsuarioDB()
			{
				Id = int.Parse(lblID.Text),
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
		public void fillTextBoxUser(string numero, long id, string nombre, string apellido, string documento, string sexo, string celular, string medioPago, string fecha, string monto, string correo)
		{
			lblID.Text = id.ToString();
			lblCodigo.Text = numero;
			txtNombre.Text = nombre;
			txtApellido.Text = apellido;
			txtDocumento.Text = documento;
			comboBox1.Text = sexo;
			txtCelular.Text = celular;
			cbMedio.Text = medioPago;
			dateTimePicker1.Text = fecha;
			txtMonto.Text = monto;

			if(correo.Contains("@"))
			{
				txtAddres.Text = correo.Split('@')[0];
				txtCorreo.Text = correo.Split('@')[1];
			}
			else
				txtAddres.Text = correo;

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

		private void ModifyUsuario_FormClosed(object sender, FormClosedEventArgs e)
		{
			hilo.Suspend();
		}
	}
}
