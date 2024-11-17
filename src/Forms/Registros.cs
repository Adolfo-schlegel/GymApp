using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ArduinoClient.Models;
using ArduinoClient.Tools;
using System.Text.RegularExpressions;
using System.ComponentModel;
using ArduinoClient.Extensions;
using ArduinoClient.DB;
using ArduinoClient.Tools.Arduino;
using ArduinoClient.Forms;
using ArduinoClient.WorkingService;
using DocumentFormat.OpenXml.EMMA;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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
		private void openDoor()=>_arduinoManager.WriteToSerialPort("E");								
		private void closeDoor() => _arduinoManager.WriteToSerialPort("X");
		
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
					printUserNotUpdated();
				}));						
		}
		private void printUserNotUpdated()
		{
			try
			{
				foreach (DataGridViewRow row in dataGridView2.Rows)
				{
					var user = (UsuarioDB)row.DataBoundItem;
					if (!user.isUpToDate())
					{
						row.DefaultCellStyle.BackColor = System.Drawing.Color.FromArgb(255, 121, 121);
						row.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
					}
				}
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show("Error al actualizar el estilo de la celda: " + ex.Message);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error al actualizar el estilo de la celda: " + ex.Message);
			}
		}
		private void setUserInfo()
		{
			try
			{
				lblId.Invoke(new MethodInvoker(delegate {
					if (ScannedUser != null)
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
				}));
			}
			catch (ArgumentException ex)
			{
				MessageBox.Show("Error en setUserInfo: " + ex.Message);
			}			
		}
		private void checkUserInfo(string code)
		{
			cleanLabels();

			//Blanco
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
						this.Invoke(new MethodInvoker(() => checkUserInfo(code)));
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
				MessageBox.Show("Error al actualizar cuota consulte al desarrollador" + ex.Message);
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
		// Define una variable DateTime para registrar la última vez que se hizo clic en el botón.
		private DateTime lastClickTime = DateTime.MinValue;

		private void abrirPuertaToolStripMenuItem_Click(object sender, EventArgs e)
		{
			// Compara el tiempo actual con el último clic.
			if ((DateTime.Now - lastClickTime).TotalSeconds >= 2)
			{
				// Actualiza el tiempo del último clic y ejecuta la acción.
				lastClickTime = DateTime.Now;
				openDoor();

				int currentCount = _sqliteDataAccess.GetActionCount("Door");
				_sqliteDataAccess.UpdateActionCount("Door", currentCount + 1);
			}
			else
			{
				// Opción: puedes mostrar un mensaje indicando que debe esperar.
				MessageBox.Show("Espere 2 segundos antes de volver a abrir la puerta.");
			}
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

						int currentCount = _sqliteDataAccess.GetActionCount("UsersDeleted");
						_sqliteDataAccess.UpdateActionCount("UsersDeleted", currentCount + 1);
					}
				}
				catch (Exception ex)
				{
					MessageBox.Show("Error al eliminar el usuario: " + ex.Message);
				}
			}
			else
				MessageBox.Show("Seleccione un registro para eliminar");
			
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

						_excelManager.Initialize();

						_excelManager.AddSheet($"Gym - {date}");

						_excelManager.AddItems(LstUsers);

						_excelManager.SaveAs(directoryPath + @"\RegistroGym.xlsm");

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

			// Convertir la lista a BindingList para aplicar la ordenación
			var dataSource = dataGridView2.DataSource as List<UsuarioDB>;
			if (dataSource != null)
			{
				var bindingList = new BindingList<UsuarioDB>(dataSource);
				var sortedList = new BindingList<UsuarioDB>(bindingList.ToList());
				((IBindingList)sortedList).ApplySort(propDesc, direction);
				dataGridView2.DataSource = sortedList;
			}
		}
		private void btnGroupDeudores_Click(object sender, EventArgs e)
		{
			try
			{
				// Ensure the DataSource is a SortableBindingList
				if (dataGridView2.DataSource is SortableBindingList<UsuarioDB> bindingList)
				{
					// Get the property descriptor for "Fecha"
					PropertyDescriptor propDesc = TypeDescriptor.GetProperties(typeof(UsuarioDB))["Fecha"];

					// Check if propDesc is null (i.e., if "Fecha" property doesn't exist)
					if (propDesc == null)
					{
						MessageBox.Show("The 'Fecha' property was not found in the data model.");
						return; // Exit the method to prevent further errors
					}

					// Apply sort based on the "Fecha" property
					((IBindingList)bindingList).ApplySort(propDesc, ListSortDirection.Ascending);
				}
				else
				{
					// Handle the case where the DataSource is not a SortableBindingList
					MessageBox.Show("The data source is not a sortable list.");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error in btnGroupDeudores_Click: " + ex.Message);
			}
		}

		private void txtSearch_TextChanged(object sender, EventArgs e)
		{
			var text = txtSearch.Text;
			List<UsuarioDB> searched = new List<UsuarioDB>();
			var regex = new Regex(Regex.Escape(text), RegexOptions.IgnoreCase);

			// Search logic with null checks
			foreach (var user in LstUsers)
			{
				if (user != null) // Ensure user is not null
				{
					var token = user.Nombre.ToLower() + " " + user.Apellido.ToLower() + " " + user.Documento + " " + user.Fecha;
					if (regex.IsMatch(token))
					{
						searched.Add(user);
					}
				}
			}

			// Convert List<UsuarioDB> to SortableBindingList<UsuarioDB> after the search
			SortableBindingList<UsuarioDB> sortableSearchedList = new SortableBindingList<UsuarioDB>(searched);

			// Reapply the status function after filtering
			sortableSearchedList.SetStatusFunc(item => item.isUpToDate());

			// Update the DataSource with the sortable list
			dataGridView2.DataSource = null; // Clear existing binding
			dataGridView2.DataSource = sortableSearchedList; // Set new binding list
		}

		private void ingresosDeHOYToolStripMenuItem_Click(object sender, EventArgs e)
		{
			var todayAccess = new TodayAccess(_sqliteDataAccess, _reportSender);
			todayAccess.ShowDialog();
		}
	}
}
