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
using System.Collections;
using System.ComponentModel;
#if WINDOWS
using System.Drawing;
using System.Windows.Forms;
#endif

namespace ByteFX.Data.MySqlClient.Designers
{
#if WINDOWS
	/// <summary>
	/// Summary description for EditConnectionString.
	/// </summary>
	public class EditConnectionString : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox textServer;
		private System.Windows.Forms.TextBox textUser;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox textPassword;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox comboDatabase;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox textPort;
		private System.Windows.Forms.CheckBox useCompression;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EditConnectionString()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public string ConnectionString;

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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.textServer = new System.Windows.Forms.TextBox();
			this.textUser = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.textPassword = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.comboDatabase = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.textPort = new System.Windows.Forms.TextBox();
			this.useCompression = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Location = new System.Drawing.Point(67, 200);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 0;
			this.buttonOK.Text = "&OK";
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(187, 200);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 0;
			this.buttonCancel.Text = "&Cancel";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(12, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(104, 16);
			this.label1.TabIndex = 1;
			this.label1.Text = "Mysql server name:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textServer
			// 
			this.textServer.Location = new System.Drawing.Point(112, 20);
			this.textServer.Name = "textServer";
			this.textServer.Size = new System.Drawing.Size(168, 20);
			this.textServer.TabIndex = 2;
			this.textServer.Text = "";
			// 
			// textUser
			// 
			this.textUser.Location = new System.Drawing.Point(112, 76);
			this.textUser.Name = "textUser";
			this.textUser.Size = new System.Drawing.Size(168, 20);
			this.textUser.TabIndex = 2;
			this.textUser.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(68, 76);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(40, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "User:";
			this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPassword
			// 
			this.textPassword.Location = new System.Drawing.Point(112, 108);
			this.textPassword.Name = "textPassword";
			this.textPassword.PasswordChar = '*';
			this.textPassword.Size = new System.Drawing.Size(168, 20);
			this.textPassword.TabIndex = 2;
			this.textPassword.Text = "";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(44, 108);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 16);
			this.label3.TabIndex = 1;
			this.label3.Text = "Password:";
			this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// comboDatabase
			// 
			this.comboDatabase.Location = new System.Drawing.Point(112, 160);
			this.comboDatabase.Name = "comboDatabase";
			this.comboDatabase.Size = new System.Drawing.Size(168, 21);
			this.comboDatabase.TabIndex = 3;
			this.comboDatabase.DropDown += new System.EventHandler(this.comboDatabase_DropDown);
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(4, 164);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 16);
			this.label4.TabIndex = 1;
			this.label4.Text = "Select a database:";
			this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(72, 52);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(36, 16);
			this.label5.TabIndex = 1;
			this.label5.Text = "Port:";
			this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// textPort
			// 
			this.textPort.Location = new System.Drawing.Point(112, 48);
			this.textPort.Name = "textPort";
			this.textPort.Size = new System.Drawing.Size(168, 20);
			this.textPort.TabIndex = 2;
			this.textPort.Text = "";
			// 
			// useCompression
			// 
			this.useCompression.Location = new System.Drawing.Point(10, 132);
			this.useCompression.Name = "useCompression";
			this.useCompression.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
			this.useCompression.Size = new System.Drawing.Size(116, 24);
			this.useCompression.TabIndex = 4;
			this.useCompression.Text = "Use compression";
			this.useCompression.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// EditConnectionString
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(298, 239);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.useCompression,
																		  this.comboDatabase,
																		  this.textServer,
																		  this.label1,
																		  this.buttonOK,
																		  this.buttonCancel,
																		  this.textUser,
																		  this.label2,
																		  this.textPassword,
																		  this.label3,
																		  this.label4,
																		  this.label5,
																		  this.textPort});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EditConnectionString";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Connection String";
			this.Load += new System.EventHandler(this.EditConnectionString_Load);
			this.ResumeLayout(false);

		}
		#endregion

		MySqlConnection conn;

		private void EditConnectionString_Load(object sender, System.EventArgs e)
		{
			conn = new MySqlConnection(ConnectionString);
			textServer.Text = conn.DataSource;
			textUser.Text = conn.User;
			textPassword.Text = conn.Password;
			textPort.Text = conn.Port.ToString();
			conn = null;
		}

		string GetString()
		{
			System.Text.StringBuilder s;
			s = new System.Text.StringBuilder();
			s.Append("server=" + textServer.Text);
			s.Append(";port=" + textPort.Text);
			s.Append(";uid=" + textUser.Text);
			s.Append(";pwd=" + textPassword.Text);
			if (useCompression.Checked) 
			{
				s.Append(";Use compression=true");
			}
			if (comboDatabase.SelectedIndex != -1)
				s.Append(";Initial Catalog=" + comboDatabase.SelectedItem.ToString());
			else
				s.Append(";Initial Catalog=" + comboDatabase.Text);
			return s.ToString();
		}

		private void comboDatabase_DropDown(object sender, System.EventArgs e)
		{
			comboDatabase.Items.Clear();
			comboDatabase.SelectedIndex = -1;
		
			try
			{
				conn = new MySqlConnection(GetString());
				conn.Open();
				MySqlCommand comm = new MySqlCommand("show databases",conn);
				MySqlDataReader r = (MySqlDataReader)comm.ExecuteReader();
				while(r.Read())
				{
					comboDatabase.Items.Add(r[0]);
				}
				r.Close();
				conn.Close();
			}
			catch(MySqlException)
			{
			}
		}

		private void buttonOK_Click(object sender, System.EventArgs e)
		{
			ConnectionString = GetString();
			DialogResult = DialogResult.OK;
			Close();
		}
	}
#endif
}
