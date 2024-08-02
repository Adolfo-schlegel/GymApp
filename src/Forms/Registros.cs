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
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel;
using ArduinoClient.Extensions;

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
			dataGridView2.CellFormatting += dataGridView2_CellFormatting;
			this.arduinoManager = arduinoManager;
		}
		private void dataGridView2_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e) => printUserNotUpdated();
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
			btnAtualizarCuota.Visible = false;
			btnAgregar.Visible = false;
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

			SortableBindingList<UsuarioDB> bindingList = new SortableBindingList<UsuarioDB>(LstUsers);

			bindingList.SetStatusFunc(item => item.isUpToDate());


			dataGridView2.Invoke(new MethodInvoker(
				delegate
				{
					dataGridView2.DataSource = null;
					dataGridView2.DataSource = bindingList;
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
					row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255,121,121);
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

			if (!isUserExist(code))
			{
				lblCodigo.Invoke(new MethodInvoker(
					delegate
					{
						btnAtualizarCuota.Visible = false;
						btnAgregar.Visible = true;

						ledPanel.GradientBottomColor = System.Drawing.Color.FromArgb(55, 125, 255);
						ledPanel.GradientTopColor = System.Drawing.Color.FromArgb(55, 125, 255);
						lblCodigo.Text = code;
					}));
			}

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
						ledPanel.GradientBottomColor = System.Drawing.Color.Green;
						ledPanel.GradientTopColor = System.Drawing.Color.Green;

						SqliteDataAccess.LogDateAccessUser(ScannedUser.Id, 
							$"{DateTime.Now.ToString("yy/MM/dd - HH:mm")} - OK");

						openDoor();
					}
					else
					{
						btnAtualizarCuota.Visible = true;
						lblState.ForeColor = System.Drawing.Color.Red;
						lblState.Text = "Usuario adeuda";
						ledPanel.GradientBottomColor = System.Drawing.Color.Red;
						ledPanel.GradientTopColor = System.Drawing.Color.Red;

						SqliteDataAccess.LogDateAccessUser(ScannedUser.Id,
							$"{DateTime.Now.ToString("yy/MM/dd - HH:mm")} - Error");

						closeDoor();
					}
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
				ledPanel.GradientBottomColor = System.Drawing.Color.White;
				ledPanel.GradientTopColor = System.Drawing.Color.White;
				btnAtualizarCuota.Visible = false;
			}));

		}
		private void controlDeAcceso()
		{
			try
			{
				while (true)
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
			catch(Exception ex)
			{
				MessageBox.Show("Registros control de acceso -> " +  ex.Message);
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

				userForm.fillTextBoxUser(user.Codigo, user.Id, user.Nombre, user.Apellido, user.Documento.ToString(), user.Sexo,user.Celular.ToString(), user.MedioPago, user.Fecha.ToString(), user.Monto.ToString(), user.Correo, user.Log);

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

					try
					{
						ExcelManager excelManager = new ExcelManager();

						excelManager.AddSheet($"Gym - {DateTime.Now.ToString("dd-MM-yyyy")}");

						excelManager.AddItems(LstUsers);

						excelManager.SaveAs(directoryPath);

						MessageBox.Show("Archivo guardado exitosamente en: " + directoryPath);
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
			refreshGrid();
			cleanLabels();
		}
		private void btnAgregar_Click(object sender, EventArgs e)
		{
			NewUsuario user = new NewUsuario(arduinoManager);

			user.Code = lblCodigo.Text;

			user.ShowDialog();

			refreshGrid();
		}
		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			var text = txtSearch.Text;
			List<UsuarioDB> searched = new List<UsuarioDB>();

			var regex = new Regex(Regex.Escape(text), RegexOptions.IgnoreCase);

			foreach (var user in LstUsers)
			{
				var token = user.Nombre.ToLower() +" "+ user.Apellido.ToLower() + " "+ user.Documento +" "+ user.Fecha;

				if (regex.IsMatch(token))
				{
					searched.Add(user);
				}
			}

			dataGridView2.DataSource = null;
			dataGridView2.DataSource = searched;

		}
		private void dataGridView2_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			var columnIndex = e.ColumnIndex;
			var column = dataGridView2.Columns[columnIndex];

			PropertyDescriptor propDesc = TypeDescriptor.GetProperties(typeof(UsuarioDB))[column.DataPropertyName];

			ListSortDirection direction = ListSortDirection.Descending;
			if (dataGridView2.SortOrder == SortOrder.Descending)
			{
				direction = ListSortDirection.Ascending;
			}


			((IBindingList)dataGridView2.DataSource).ApplySort(propDesc, direction);

		}
		private void btnGroupDeudores_Click(object sender, EventArgs e)
		{
			SortableBindingList<UsuarioDB> bindingList = (SortableBindingList<UsuarioDB>)dataGridView2.DataSource;

			// Sort by status first, then by the "Fecha" column (you can change this to any column you prefer)
			PropertyDescriptor propDesc = TypeDescriptor.GetProperties(typeof(UsuarioDB))["Fecha"];
			((IBindingList)bindingList).ApplySort(propDesc, ListSortDirection.Ascending);
		}
	}
}
