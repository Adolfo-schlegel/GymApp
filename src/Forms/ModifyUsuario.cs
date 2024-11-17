using ArduinoClient.DB;
using ArduinoClient.Models;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Arduino;
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
		private IArduinoManager _arduinoManager;
		private ISqliteDataAccess _sqliteDataAccess;
		public ModifyUsuario(IArduinoManager arduinoManager, ISqliteDataAccess sqliteDataAccess)
		{
			InitializeComponent();
			_sqliteDataAccess = sqliteDataAccess;
			this._arduinoManager = arduinoManager;

			// Asignar evento KeyPress a campos numéricos
			txtDocumento.KeyPress += txtNumerico_KeyPress;
			txtCelular.KeyPress += txtNumerico_KeyPress;
			txtMonto.KeyPress += txtNumerico_KeyPress;

			hilo = new Thread(listenSerial);
			hilo.Start();			
		}
		
		private void btnGuardarUsuario_Click(object sender, EventArgs e)
		{
			try
			{
				_sqliteDataAccess.ModifyUser(getUserFromTextbox());				
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			hilo.Suspend();
			MessageBox.Show("Usuario modificado: " + txtNombre.Text);

			int currentCount = _sqliteDataAccess.GetActionCount("UsersModified");
			_sqliteDataAccess.UpdateActionCount("UsersModified", currentCount + 1);
		}
		//public UsuarioDB getUserFromTextbox()
		//{
		//	return new UsuarioDB()
		//	{
		//		Id = int.Parse(lblID.Text),
		//		Codigo = lblCodigo.Text,
		//		Nombre = txtNombre.Text,
		//		Apellido = txtApellido.Text,
		//		Documento = long.Parse(txtDocumento.Text),
		//		Sexo = comboBox1.Text,
		//		Celular = long.Parse(txtCelular.Text),
		//		MedioPago = cbMedio.Text,
		//		Fecha = dateTimePicker1.Text,
		//		Monto = long.Parse(txtMonto.Text),
		//		Correo = txtAddres.Text + "@" + txtCorreo.Text,
		//		Log = txtRegistro.Text
		//	};
		//}
		public UsuarioDB getUserFromTextbox()
		{
			// Variables para las conversiones
			int id;
			long documento;
			long celular;
			long monto;

			// Intentamos convertir los valores a sus tipos correspondientes
			bool idValido = int.TryParse(lblID.Text, out id);
			bool documentoValido = long.TryParse(txtDocumento.Text, out documento);
			bool celularValido = long.TryParse(txtCelular.Text, out celular);
			bool montoValido = long.TryParse(txtMonto.Text, out monto);

			// Si alguna conversión falla, lanzamos una excepción
			if (!idValido || !documentoValido || !celularValido || !montoValido)
			{
				throw new FormatException("Uno o más campos contienen datos no válidos.");
			}

			// Crear y devolver el objeto UsuarioDB si todas las conversiones son válidas
			return new UsuarioDB()
			{
				Id = id,
				Codigo = lblCodigo.Text,
				Nombre = txtNombre.Text,
				Apellido = txtApellido.Text,
				Documento = documento,
				Sexo = comboBox1.Text,
				Celular = celular,
				MedioPago = cbMedio.Text,
				Fecha = dateTimePicker1.Text,
				Monto = monto,
				Correo = txtAddres.Text + "@" + txtCorreo.Text,
				Log = txtRegistro.Text
			};
		}
		public void fillTextBoxUser(string numero, long id, string nombre, string apellido, string documento, string sexo, string celular, string medioPago, string fecha, string monto, string correo, string logginUserDay)
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
			labelNombre.Text = nombre;
			txtRegistro.Text = logginUserDay;

			if (correo.Contains("@"))
			{
				txtAddres.Text = correo.Split('@')[0];
				txtCorreo.Text = correo.Split('@')[1];
			}
			else
				txtAddres.Text = correo;

		}
		private void txtNumerico_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Permitir solo dígitos y teclas de control
			if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				e.Handled = true; // Cancela la tecla si no es válida
			}
		}

		private void listenSerial()
		{
			while (true)
			{
				var data = _arduinoManager.GetNextReceivedData();
		
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
