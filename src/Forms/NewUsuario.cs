using ArduinoClient.DB;
using ArduinoClient.Models;
using ArduinoClient.Tools;
using ArduinoClient.Tools.Arduino;
using System;
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
		}
		private void btnGuardarUsuario_Click(object sender, EventArgs e)
		{
			try
			{
				_sqliteDataAccess.SaveUser(getUserFromTextbox());
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

		//private string lastCode = "";  // Almacenar el último código recibido

		//private void listenSerial()
		//{
		//	while (true)
		//	{
		//		var data = _arduinoManager.GetNextReceivedData();

		//		if (data != null)
		//		{
		//			var code = data.Replace("Card UID: ", "").Trim();

		//			if (code != lastCode)  // Solo actualiza si el código es diferente
		//			{
		//				lastCode = code;

		//				lblCodigo.Invoke(new MethodInvoker(
		//					delegate
		//					{
		//						lblCodigo.Text = code;
		//					}));
		//			}
		//		}
		//	}
		//}
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

		private void NewUsuario_FormClosed(object sender, FormClosedEventArgs e)
		{
			hilo.Suspend();
		}
	}
}
