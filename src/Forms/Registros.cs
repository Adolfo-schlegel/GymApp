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
using ArduinoClient.DB;
using ArduinoClient.Tools.Arduino;
using DocumentFormat.OpenXml.Office.CustomUI;
using ArduinoClient.Forms;
using Microsoft.Extensions.Logging;
using ArduinoClient.WorkingService;

namespace ArduinoClient
{
	public partial class Cliente : Form
	{
		private int ClickedId;

		private Thread Hilo;		
		private List<UsuarioDB> LstUsers;
		private UsuarioDB ScannedUser;

		private IArduinoManager _arduinoManager;
		private ISqliteDataAccess _sqliteDataAccess;
		private IReportSender _reportSender;
		private IExcelManager _excelManager;
		public Cliente(IArduinoManager arduinoManager, ISqliteDataAccess sqliteDataAccess, IReportSender reportSender, IExcelManager excelManager)
		{			
			Init();
			dataGridView2.CellFormatting += dataGridView2_CellFormatting;

			_arduinoManager = arduinoManager;
			_sqliteDataAccess = sqliteDataAccess;
			_reportSender = reportSender;
			_excelManager = excelManager;
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
			LstUsers = _sqliteDataAccess.LoadPeople();
			return LstUsers;
		}
		private void openDoor()
		{			
			string result = _arduinoManager.WriteToSerialPort("E");
			
			if(result != "OK")
				MessageBox.Show(result);			
		}
		private void closeDoor()
		{

			string result = _arduinoManager.WriteToSerialPort("X");
			if (result != "OK")
				MessageBox.Show(result);
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
		private void checkUserInfo(string code)
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

					var status = "OK";
					if (ScannedUser.isUpToDate())
					{
						lblState.ForeColor = System.Drawing.Color.Green;
						lblState.Text = "Usuario al dia";
						ledPanel.GradientBottomColor = System.Drawing.Color.Green;
						ledPanel.GradientTopColor = System.Drawing.Color.Green;
						openDoor();
					}
					else
					{
						status = "Error";
						btnAtualizarCuota.Visible = true;
						lblState.ForeColor = System.Drawing.Color.Red;
						lblState.Text = "Usuario adeuda";
						ledPanel.GradientBottomColor = System.Drawing.Color.Red;
						ledPanel.GradientTopColor = System.Drawing.Color.Red;						
						closeDoor();
					}

					logUser(status, ScannedUser.Id);
				}));		
			}

			refreshGrid();
		}
		private void logUser(string status, int userId)
		{
			var line = $"{DateTime.Now.ToString("dd/MM/yyyy - HH:mm")} - {status}";

			_sqliteDataAccess.LogHistoricalDateAccessUser(userId, line);
			_sqliteDataAccess.LogTodaysAccess(userId, line);
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
					var data = _arduinoManager.GetNextReceivedData();

					if (data != null)
					{
						var code = data.Replace("Card UID: ", "");

						checkUserInfo(code);
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

				ModifyUsuario userForm = new ModifyUsuario(_arduinoManager, _sqliteDataAccess);

				userForm.fillTextBoxUser(user.Codigo, user.Id, user.Nombre, user.Apellido, user.Documento.ToString(), user.Sexo,user.Celular.ToString(), user.MedioPago, user.Fecha.ToString(), user.Monto.ToString(), user.Correo, user.Log);

				userForm.ShowDialog();

				refreshGrid();

				Hilo.Resume();//reactivo el hilo
			}
		}
		private void EventCliente_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_arduinoManager.IsPortOpen())
			{
				_arduinoManager.ClosePort();
			}
			_arduinoManager.StopReading();
			_arduinoManager.DisposePort();

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
				_sqliteDataAccess.UpdateQuota(new UsuarioDB {Id = int.Parse(lblId.Text), Fecha = DateTime.Today.ToShortDateString() });							
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

			NewUsuario user = new NewUsuario(_arduinoManager, _sqliteDataAccess);

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
						_sqliteDataAccess.DeleteUser(ClickedId);

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
						var date = DateTime.Now.ToString("dd-MM-yyyy");

						_excelManager.AddSheet($"Gym - {date}");

						_excelManager.AddItems(LstUsers);

						_excelManager.SaveAs(directoryPath + @"\" + "RegistroGym - " + date);

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
			NewUsuario user = new NewUsuario(_arduinoManager, _sqliteDataAccess);

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
		private void ingresosDeHOYToolStripMenuItem_Click(object sender, EventArgs e)
		{

			//logUser("Error", 9);
			//
			//logUser("Error", 10);
			//logUser("OK", 10);
			//
			//logUser("Error", 12);
			//logUser("Error", 12);

			var todayAccess = new TodayAccess(_sqliteDataAccess, _reportSender);
			todayAccess.ShowDialog();
		}
	}
}
