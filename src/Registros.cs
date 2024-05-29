using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;
using ArduinoClient.Models;
using ArduinoClient.Tools;
using DocumentFormat.OpenXml.Office2013.Drawing.Chart;
using DocumentFormat.OpenXml.EMMA;
using System.IO;

namespace ArduinoClient
{
	public partial class Cliente : Form
	{
		private int ClickedId;

		private Thread Hilo;		
		private List<UsuarioDB> LstUsers;
		private UsuarioDB ScannedUser;
		private ArduinoManager arduinoManager;

		public Cliente(ArduinoManager arduinoManager)
		{			
			Init();
			this.arduinoManager = arduinoManager;
		}
		private void Init()
		{
			InitializeComponent();

			LstUsers = new List<UsuarioDB>();

			Hilo = new Thread(controlDeAcceso);
			Hilo.Name = "AccessDoorProcess";
			Hilo.Start();
		}


		private void Cliente_Load(object sender, EventArgs e)
		{
			refreshGrid();
		}
		public List<UsuarioDB> getUsers()
		{
			LstUsers = SqliteDataAccess.LoadPeople();
			return LstUsers;
		}
		private void openDoor()
		{
			try
			{
				arduinoManager.serialPort.Write("E");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message);
			}
		}
		private void closeDoor()
		{
			try
			{
				arduinoManager.serialPort.Write("X");
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error: " + ex.Message);
			}
		}
		private void refreshGrid()
		{
			LstUsers = getUsers();

			dataGridView2.Invoke(new MethodInvoker(
				delegate
				{
					dataGridView2.DataSource = null;
					dataGridView2.DataSource = LstUsers;
				}));

			printUserNotUpdated();
		}
		private void printUserNotUpdated()
		{
			foreach (DataGridViewRow row in dataGridView2.Rows)
			{
				var user = (UsuarioDB)row.DataBoundItem;

				if (!user.isUpToDate())
				{
					row.DefaultCellStyle.BackColor = System.Drawing.Color.Red;
					row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
				}
			}
		}
		private void setUserInfo()
		{
			btnAtualizarCuota.Visible = false;
			btnAgregar.Visible = false;
			lblId.Text = ScannedUser.Id.ToString();
			lblNombre.Text = ScannedUser.Nombre;
			lblApellido.Text = ScannedUser.Apellido;
			lblCelular.Text = ScannedUser.Celular.ToString();
			lblDocumento.Text = ScannedUser.Documento.ToString();
			lblMonto.Text = ScannedUser.Monto.ToString();
			lblMedio.Text = ScannedUser.MedioPago;
			lblSexo.Text = ScannedUser.Sexo;
			lblFecha.Text = ScannedUser.Fecha.ToString();
			lblDaysLeft.Text = ScannedUser.daysLeft().ToString();
			lblCodigo.Text = ScannedUser.Codigo;
		}
		private void showUserInfo(string code)
		{
			cleanLabels();
			
			if (isUserExist(code))
				{
					ScannedUser = userByCode(code);

					lblId.Invoke(new MethodInvoker(
					delegate
					{
						setUserInfo();

						if (ScannedUser.isUpToDate())
						{
							lblState.ForeColor = System.Drawing.Color.Green;
							lblState.Text = "Usuario al dia";
							label15.BackColor = System.Drawing.Color.Green;

							openDoor();
						}
						else
						{
							btnAtualizarCuota.Visible = true;
							lblState.ForeColor = System.Drawing.Color.Red;
							lblState.Text = "Usuario adeuda";
							label15.BackColor = System.Drawing.Color.Red;

							closeDoor();
						}
					}));		
				}
			else
				{
					lblCodigo.Invoke(new MethodInvoker(
							delegate
							{
								btnAtualizarCuota.Visible = false;
								btnAgregar.Visible = true;
								label15.BackColor = System.Drawing.Color.Blue;
								lblCodigo.Text = code;
							}));
				}

			refreshGrid();
		}
		private void cleanLabels()
		{		
			lblId.Invoke(new MethodInvoker(
			delegate
			{
				lblId.Text = "";
				lblCodigo.Text = "";
				lblNombre.Text = "";
				lblApellido.Text = "";
				lblCelular.Text = "";
				lblDocumento.Text = "";
				lblFecha.Text = "";
				lblMonto.Text = "";
				lblMedio.Text = "";
				lblSexo.Text = "";
				lblDaysLeft.Text = "";
				lblState.Text = "";
				label15.BackColor = System.Drawing.Color.FromArgb(113, 92, 232);
				btnAtualizarCuota.Visible = false;
			}));

		}
		private void controlDeAcceso()
		{
			while(true)
			{
				Thread.Sleep(100);
				var data = arduinoManager.GetNextReceivedData();
				
				if (data != null)
				{
					var code = data.Replace("Card UID: ", "");

					showUserInfo(code);					
				}					
			}		
		}
		public bool isUserExist(string idCard) => LstUsers.Exists(u => u.Codigo ==idCard);
		private void EvenModifyUser_DoubleClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
			{
				Hilo.Suspend();//tengo que suspender el hilo que esta actualizando los valores de este formulario, sino cuando entre al otroformulario, este hilo mas el otro hilo van a funcionar turnandose y por ende se van a actualizar los valores en este forms

				var row = dataGridView2.Rows[e.RowIndex];

				UsuarioDB user = row.DataBoundItem as UsuarioDB;

				ModifyUsuario userForm = new ModifyUsuario(arduinoManager);

				userForm.fillTextBoxUser(user.Codigo, user.Id, user.Nombre, user.Apellido, user.Documento.ToString(), user.Sexo,user.Celular.ToString(), user.MedioPago, user.Fecha.ToString(), user.Monto.ToString(), user.Correo);

				userForm.ShowDialog();

				refreshGrid();

				Hilo.Resume();//reactivo el hilo
			}
		}
		private void EventCliente_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (arduinoManager.serialPort.IsOpen) arduinoManager.serialPort.Close();
			arduinoManager.StopReading();
			arduinoManager.serialPort.Dispose();			
			Environment.Exit(0);

		}
		private void btnAgregar_Click_1(object sender, EventArgs e)
		{
			NewUsuario user = new NewUsuario(arduinoManager);

			user.Code = lblCodigo.Text;

			user.ShowDialog();

			refreshGrid();
		}
		private void EvenDataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0)
			{				
				DataGridViewRow filaSeleccionada = dataGridView2.Rows[e.RowIndex];

				UsuarioDB usuarioSeleccionado = filaSeleccionada.DataBoundItem as UsuarioDB;

				ClickedId = usuarioSeleccionado.Id;

				dataGridView2.Rows[e.RowIndex].Selected = true;
			}
		}
		public UsuarioDB userByCode(string code) => LstUsers.FirstOrDefault(u => u.Codigo == code);
		private void btnAtualizarCuota_Click(object sender, EventArgs e)
		{
			try
			{ 				
				SqliteDataAccess.UpDateUser(new UsuarioDB {Id = int.Parse(lblId.Text), Fecha = DateTime.Today.ToShortDateString() });							
				refreshGrid();

				MessageBox.Show("Usuario al dia");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			
		}
		private void agregarUsuarioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Hilo.Suspend();

			NewUsuario user = new NewUsuario(arduinoManager);

			user.Code = lblCodigo.Text;

			user.ShowDialog();			

			refreshGrid();

			Hilo.Resume();
		}
		private void abrirPuertaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			openDoor();
		}
		private void eliminarUsuarioToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (dataGridView2.SelectedRows.Count > 0)
			{
				try
				{
					if (ClickedId >= 0)
					{
						SqliteDataAccess.DeleteUser(ClickedId);

						refreshGrid();

						MessageBox.Show("Usuario eliminado");
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error al eliminar el usuario: " + ex.Message);
				}
			}
			else
			{
				MessageBox.Show("Seleccione un registro para eliminar");
			}
		}
		private void exportarPlanillaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			using (FolderBrowserDialog folderBrowser = new FolderBrowserDialog())
			{
				DialogResult result = folderBrowser.ShowDialog();

				if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
				{
					string directoryPath = folderBrowser.SelectedPath;
					string fileName = "RegistroUsuariosGYM.xlsx";

					try
					{
						ExcelManager excelManager = new ExcelManager();

						excelManager.AddSheet($"Gym - {DateTime.Now.ToString("dd-MM-yyyy")}");

						excelManager.AddItems(LstUsers);

						excelManager.SaveAs(directoryPath, fileName);

						MessageBox.Show("Archivo guardado exitosamente en: " + directoryPath + fileName);
					}
					catch (Exception ex)
					{
						MessageBox.Show("Error al guardar el archivo: " + ex.Message);
					}
				}
			}
		
		}
		private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
		{
			cleanLabels();
		}
	}
}
