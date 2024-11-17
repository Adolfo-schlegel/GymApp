using ArduinoClient.DB;
using ArduinoClient.Models;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Arduino;
using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
namespace ArduinoClient
{
	public partial class NewUsuario : Form
	{
		private Thread hilo;
		public string Code { get; set; }
		private IArduinoManager _arduinoManager;
		private ISqliteDataAccess _sqliteDataAccess;
		public NewUsuario(IArduinoManager arduinoManager, ISqliteDataAccess sqliteDataAccess)
		{			
			InitializeComponent();
			this._arduinoManager = arduinoManager;
			_sqliteDataAccess = sqliteDataAccess;
			hilo = new Thread(listenSerial);
			hilo.Name = "UserIdProcess";
			hilo.Start();
		}
		private void NewUsuario_Load(object sender, EventArgs e)
		{
			lblCodigo.Text = Code;

			dateTimePicker1.Text = DateTime.Now.ToShortDateString();

			txtDocumento.KeyPress += txtNumerico_KeyPress;
			txtCelular.KeyPress += txtNumerico_KeyPress;
			txtMonto.KeyPress += txtNumerico_KeyPress; 
		}
		private void btnGuardarUsuario_Click(object sender, EventArgs e)
		{
			try
			{
				_sqliteDataAccess.SaveUser(getUserFromTextbox());
				MessageBox.Show("Usuario agregado: " + txtNombre.Text);

				int currentCount = _sqliteDataAccess.GetActionCount("UsersCreated");
				_sqliteDataAccess.UpdateActionCount("UsersCreated", currentCount + 1);
			}
			catch(Exception ex)
			{
				MessageBox.Show(ex.Message);
			}

			hilo.Suspend();
		}		
		private void txtNumerico_KeyPress(object sender, KeyPressEventArgs e)
		{
			// Permitir solo dígitos y teclas de control (como retroceso)
			if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
			{
				e.Handled = true; // Cancela la tecla presionada
			}
		}
		public UsuarioDB getUserFromTextbox()
		{
			long documento = 0;
			long celular = 0;
			long monto = 0;

			// Intentamos convertir los valores numéricos de los campos
			bool documentoValido = long.TryParse(txtDocumento.Text, out documento);
			bool celularValido = long.TryParse(txtCelular.Text, out celular);
			bool montoValido = long.TryParse(txtMonto.Text, out monto);

			// Validamos que todos los campos numéricos críticos sean correctos
			if (!documentoValido || !celularValido || !montoValido)
			{
				throw new FormatException("Uno o más campos contienen datos no válidos.");
			}

			return new UsuarioDB()
			{
				Codigo = lblCodigo.Text,
				Nombre = txtNombre.Text,
				Apellido = txtApellido.Text,
				Documento = documento,
				Sexo = comboBox1.Text,
				Celular = celular,
				MedioPago = cbMedio.Text,
				Fecha = dateTimePicker1.Value.ToString("dd-MM-yyyy"),
				Monto = monto, 
				Correo = txtAddres.Text + "@" + txtCorreo.Text
			};
		}

		private void listenSerial()
		{
			while (true)
			{
				Thread.Sleep(100);

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

		private void NewUsuario_FormClosed(object sender, FormClosedEventArgs e)
		{
			hilo.Suspend();
		}
	}
}
