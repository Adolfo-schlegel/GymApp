namespace ArduinoClient.Forms
{
	partial class TodayAccess
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.txtRegistro = new System.Windows.Forms.TextBox();
			this.lblTotal = new System.Windows.Forms.Label();
			this.lblPermitidos = new System.Windows.Forms.Label();
			this.lblDenegados = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.btnEnviarReporte = new ArduinoClient.Models.RJButton();
			this.SuspendLayout();
			// 
			// txtRegistro
			// 
			this.txtRegistro.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
			this.txtRegistro.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.txtRegistro.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
			this.txtRegistro.Location = new System.Drawing.Point(12, 9);
			this.txtRegistro.Multiline = true;
			this.txtRegistro.Name = "txtRegistro";
			this.txtRegistro.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtRegistro.Size = new System.Drawing.Size(608, 368);
			this.txtRegistro.TabIndex = 34;
			// 
			// lblTotal
			// 
			this.lblTotal.AutoSize = true;
			this.lblTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTotal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.lblTotal.Location = new System.Drawing.Point(690, 18);
			this.lblTotal.Name = "lblTotal";
			this.lblTotal.Size = new System.Drawing.Size(19, 20);
			this.lblTotal.TabIndex = 35;
			this.lblTotal.Text = "0";
			// 
			// lblPermitidos
			// 
			this.lblPermitidos.AutoSize = true;
			this.lblPermitidos.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblPermitidos.ForeColor = System.Drawing.Color.Lime;
			this.lblPermitidos.Location = new System.Drawing.Point(732, 50);
			this.lblPermitidos.Name = "lblPermitidos";
			this.lblPermitidos.Size = new System.Drawing.Size(19, 20);
			this.lblPermitidos.TabIndex = 36;
			this.lblPermitidos.Text = "0";
			// 
			// lblDenegados
			// 
			this.lblDenegados.AutoSize = true;
			this.lblDenegados.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDenegados.ForeColor = System.Drawing.Color.Red;
			this.lblDenegados.Location = new System.Drawing.Point(738, 82);
			this.lblDenegados.Name = "lblDenegados";
			this.lblDenegados.Size = new System.Drawing.Size(19, 20);
			this.lblDenegados.TabIndex = 37;
			this.lblDenegados.Text = "0";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.ForeColor = System.Drawing.Color.Red;
			this.label1.Location = new System.Drawing.Point(629, 81);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(111, 20);
			this.label1.TabIndex = 40;
			this.label1.Text = "Denegados :";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.ForeColor = System.Drawing.Color.Lime;
			this.label2.Location = new System.Drawing.Point(629, 49);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(103, 20);
			this.label2.TabIndex = 39;
			this.label2.Text = "Permitidos :";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.label3.Location = new System.Drawing.Point(629, 17);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 20);
			this.label3.TabIndex = 38;
			this.label3.Text = "Total :";
			// 
			// btnEnviarReporte
			// 
			this.btnEnviarReporte.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.btnEnviarReporte.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(128)))));
			this.btnEnviarReporte.BorderColor = System.Drawing.Color.PaleVioletRed;
			this.btnEnviarReporte.BorderRadius = 10;
			this.btnEnviarReporte.BorderSize = 0;
			this.btnEnviarReporte.FlatAppearance.BorderSize = 0;
			this.btnEnviarReporte.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnEnviarReporte.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold);
			this.btnEnviarReporte.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.btnEnviarReporte.Location = new System.Drawing.Point(642, 331);
			this.btnEnviarReporte.Name = "btnEnviarReporte";
			this.btnEnviarReporte.Size = new System.Drawing.Size(140, 46);
			this.btnEnviarReporte.TabIndex = 41;
			this.btnEnviarReporte.Text = "Enviar Reporte";
			this.btnEnviarReporte.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(70)))), ((int)(((byte)(70)))), ((int)(((byte)(70)))));
			this.btnEnviarReporte.UseVisualStyleBackColor = false;
			this.btnEnviarReporte.Click += new System.EventHandler(this.btnEnviarReporte_ClickAsync);
			// 
			// TodayAccess
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
			this.ClientSize = new System.Drawing.Size(796, 385);
			this.Controls.Add(this.btnEnviarReporte);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblDenegados);
			this.Controls.Add(this.lblPermitidos);
			this.Controls.Add(this.lblTotal);
			this.Controls.Add(this.txtRegistro);
			this.Name = "TodayAccess";
			this.Text = "TodayAccess";
			this.Load += new System.EventHandler(this.TodayAccess_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtRegistro;
		private System.Windows.Forms.Label lblTotal;
		private System.Windows.Forms.Label lblPermitidos;
		private System.Windows.Forms.Label lblDenegados;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private Models.RJButton btnEnviarReporte;
	}
}