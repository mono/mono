// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Windows.Forms;
using System.Data;
using ByteFX.Data.MySqlClient;

namespace ByteFX.Data.Common
{
	/// <summary>
	/// Summary description for SqlCommandEditorDlg.
	/// </summary>
	internal class SqlCommandEditorDlg : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TextBox		sqlText;
		private System.Windows.Forms.Button			CancelBtn;
		private System.Windows.Forms.Button			OKBtn;
		private System.Windows.Forms.Panel			panel1;
		private System.Windows.Forms.Splitter		splitter1;
		private System.Windows.Forms.DataGrid		dataGrid;
		private System.Windows.Forms.ContextMenu	sqlMenu;
		private System.Windows.Forms.MenuItem		runMenuItem;
		private	IDbCommand							command;
		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SqlCommandEditorDlg(object o)
		{
			command = (IDbCommand)o;

			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		public string SQL 
		{
			get { return sqlText.Text; }
			set { sqlText.Text = value; }
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.sqlText = new System.Windows.Forms.TextBox();
			this.sqlMenu = new System.Windows.Forms.ContextMenu();
			this.runMenuItem = new System.Windows.Forms.MenuItem();
			this.CancelBtn = new System.Windows.Forms.Button();
			this.OKBtn = new System.Windows.Forms.Button();
			this.panel1 = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.dataGrid = new System.Windows.Forms.DataGrid();
			this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// sqlText
			// 
			this.sqlText.ContextMenu = this.sqlMenu;
			this.sqlText.Dock = System.Windows.Forms.DockStyle.Top;
			this.sqlText.Location = new System.Drawing.Point(10, 10);
			this.sqlText.Multiline = true;
			this.sqlText.Name = "sqlText";
			this.sqlText.Size = new System.Drawing.Size(462, 144);
			this.sqlText.TabIndex = 0;
			this.sqlText.Text = "";
			// 
			// sqlMenu
			// 
			this.sqlMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.runMenuItem});
			// 
			// runMenuItem
			// 
			this.runMenuItem.Index = 0;
			this.runMenuItem.Text = "Run";
			this.runMenuItem.Click += new System.EventHandler(this.runMenuItem_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = new System.Drawing.Point(400, 350);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.TabIndex = 3;
			this.CancelBtn.Text = "Cancel";
			this.CancelBtn.Click += new System.EventHandler(this.CancelBtn_Click);
			// 
			// OKBtn
			// 
			this.OKBtn.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.OKBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKBtn.Location = new System.Drawing.Point(316, 350);
			this.OKBtn.Name = "OKBtn";
			this.OKBtn.TabIndex = 4;
			this.OKBtn.Text = "OK";
			this.OKBtn.Click += new System.EventHandler(this.OKBtn_Click);
			// 
			// panel1
			// 
			this.panel1.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.panel1.Controls.AddRange(new System.Windows.Forms.Control[] {
																				 this.splitter1,
																				 this.dataGrid,
																				 this.sqlText});
			this.panel1.DockPadding.Bottom = 10;
			this.panel1.DockPadding.Left = 10;
			this.panel1.DockPadding.Right = 14;
			this.panel1.DockPadding.Top = 10;
			this.panel1.Location = new System.Drawing.Point(2, 2);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(486, 344);
			this.panel1.TabIndex = 5;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(10, 154);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(462, 3);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// dataGrid
			// 
			this.dataGrid.CaptionVisible = false;
			this.dataGrid.DataMember = "";
			this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid.Location = new System.Drawing.Point(10, 154);
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.Size = new System.Drawing.Size(462, 180);
			this.dataGrid.TabIndex = 2;
			this.dataGrid.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																								 this.dataGridTableStyle1});
			// 
			// dataGridTableStyle1
			// 
			this.dataGridTableStyle1.AllowSorting = false;
			this.dataGridTableStyle1.DataGrid = this.dataGrid;
			this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle1.MappingName = "";
			// 
			// SqlCommandEditorDlg
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(486, 384);
			this.ControlBox = false;
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.panel1,
																		  this.OKBtn,
																		  this.CancelBtn});
			this.DockPadding.Bottom = 10;
			this.DockPadding.Left = 10;
			this.DockPadding.Right = 12;
			this.DockPadding.Top = 10;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SqlCommandEditorDlg";
			this.Text = "Query Builder";
			this.panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void CancelBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void OKBtn_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void runMenuItem_Click(object sender, System.EventArgs e)
		{
			if (command is MySqlCommand)
			{
				RunMySql(); 
			}
		}

		private void RunMySql() 
		{
			try 
			{
				MySqlDataAdapter da = new MySqlDataAdapter((MySqlCommand)command);
				command.CommandText = sqlText.Text;
				command.Connection.Open();
				DataTable dt = new DataTable();
				da.Fill(dt);
				dataGrid.DataSource = dt;
				command.Connection.Close();
				dataGrid.Expand(-1);
			}
			catch (Exception ex) 
			{
				MessageBox.Show(ex.Message);
			}
		}

	}
}
