using System;
using System.Data;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Npgsql.Design {
	public class ConnectionStringEditorForm : System.Windows.Forms.Form {
		private System.Windows.Forms.TabControl tc_main;
		private System.Windows.Forms.TabPage tp_connection;
		private System.Windows.Forms.Label lab_advise;
		private System.Windows.Forms.Label lab_login;
		private System.Windows.Forms.Label lab_username;
		private System.Windows.Forms.TextBox tb_username;
		private System.Windows.Forms.Label lab_select_db;
		private System.Windows.Forms.ComboBox cb_select_db;
		private System.Windows.Forms.GroupBox gb_add_parms;
		private System.Windows.Forms.Label lab_timeout;
		private System.Windows.Forms.TextBox tb_timeout;
		private System.Windows.Forms.Button btn_check_connection;
		private System.Windows.Forms.Button btn_ok;
		private System.Windows.Forms.Button btn_cancel;
		private System.Windows.Forms.Button btn_help;
		private System.Windows.Forms.Label lab_port;
		private System.Windows.Forms.TextBox tb_port;
		private System.Windows.Forms.Label lab_server;
		private System.Windows.Forms.TextBox tb_server;
		private System.Windows.Forms.TextBox tb_password;
		private System.Windows.Forms.Button btn_refresh;
		private System.Windows.Forms.Label lab_password;
		private System.Resources.ResourceManager resman;
		private Npgsql.NpgsqlConnection pgconn;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ConnectionStringEditorForm()
    : this("")
    {}

		public ConnectionStringEditorForm(String ConnectionString) {
			InitializeComponent();
			// Attention: The localization-issues don't only affect the surface but also affect some
			// MessageBoxes which have to be localized too - look for resman!
			resman = new System.Resources.ResourceManager(typeof(ConnectionStringEditorForm));

			this.pgconn.ConnectionString = ConnectionString;
			this.tb_server.Text = this.pgconn.Host;
			this.tb_port.Text = this.pgconn.Port.ToString();
			this.tb_timeout.Text = this.pgconn.ConnectionTimeout.ToString();
			if (this.pgconn.Database != "") {
				this.cb_select_db.Items.Add(this.pgconn.Database);
				this.cb_select_db.SelectedIndex = 0;
			}
			this.tb_username.Text = this.pgconn.UserName;
			this.tb_password.Text = this.pgconn.Password;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing ) {
			if( disposing ) {
				if(components != null) {
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
		private void InitializeComponent() {
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ConnectionStringEditorForm));
			this.tc_main = new System.Windows.Forms.TabControl();
			this.tp_connection = new System.Windows.Forms.TabPage();
			this.btn_refresh = new System.Windows.Forms.Button();
			this.tb_server = new System.Windows.Forms.TextBox();
			this.btn_check_connection = new System.Windows.Forms.Button();
			this.gb_add_parms = new System.Windows.Forms.GroupBox();
			this.tb_port = new System.Windows.Forms.TextBox();
			this.lab_port = new System.Windows.Forms.Label();
			this.tb_timeout = new System.Windows.Forms.TextBox();
			this.lab_timeout = new System.Windows.Forms.Label();
			this.cb_select_db = new System.Windows.Forms.ComboBox();
			this.lab_select_db = new System.Windows.Forms.Label();
			this.tb_password = new System.Windows.Forms.TextBox();
			this.lab_password = new System.Windows.Forms.Label();
			this.tb_username = new System.Windows.Forms.TextBox();
			this.lab_username = new System.Windows.Forms.Label();
			this.lab_login = new System.Windows.Forms.Label();
			this.lab_server = new System.Windows.Forms.Label();
			this.lab_advise = new System.Windows.Forms.Label();
			this.btn_ok = new System.Windows.Forms.Button();
			this.btn_cancel = new System.Windows.Forms.Button();
			this.btn_help = new System.Windows.Forms.Button();
			this.pgconn = new Npgsql.NpgsqlConnection();
			this.tc_main.SuspendLayout();
			this.tp_connection.SuspendLayout();
			this.gb_add_parms.SuspendLayout();
			this.SuspendLayout();
			// 
			// tc_main
			// 
			this.tc_main.AccessibleDescription = ((string)(resources.GetObject("tc_main.AccessibleDescription")));
			this.tc_main.AccessibleName = ((string)(resources.GetObject("tc_main.AccessibleName")));
			this.tc_main.Alignment = ((System.Windows.Forms.TabAlignment)(resources.GetObject("tc_main.Alignment")));
			this.tc_main.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tc_main.Anchor")));
			this.tc_main.Appearance = ((System.Windows.Forms.TabAppearance)(resources.GetObject("tc_main.Appearance")));
			this.tc_main.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tc_main.BackgroundImage")));
			this.tc_main.Controls.AddRange(new System.Windows.Forms.Control[] {
																																					this.tp_connection});
			this.tc_main.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tc_main.Dock")));
			this.tc_main.Enabled = ((bool)(resources.GetObject("tc_main.Enabled")));
			this.tc_main.Font = ((System.Drawing.Font)(resources.GetObject("tc_main.Font")));
			this.tc_main.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tc_main.ImeMode")));
			this.tc_main.ItemSize = ((System.Drawing.Size)(resources.GetObject("tc_main.ItemSize")));
			this.tc_main.Location = ((System.Drawing.Point)(resources.GetObject("tc_main.Location")));
			this.tc_main.Name = "tc_main";
			this.tc_main.Padding = ((System.Drawing.Point)(resources.GetObject("tc_main.Padding")));
			this.tc_main.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tc_main.RightToLeft")));
			this.tc_main.SelectedIndex = 0;
			this.tc_main.ShowToolTips = ((bool)(resources.GetObject("tc_main.ShowToolTips")));
			this.tc_main.Size = ((System.Drawing.Size)(resources.GetObject("tc_main.Size")));
			this.tc_main.TabIndex = ((int)(resources.GetObject("tc_main.TabIndex")));
			this.tc_main.Text = resources.GetString("tc_main.Text");
			this.tc_main.Visible = ((bool)(resources.GetObject("tc_main.Visible")));
			// 
			// tp_connection
			// 
			this.tp_connection.AccessibleDescription = ((string)(resources.GetObject("tp_connection.AccessibleDescription")));
			this.tp_connection.AccessibleName = ((string)(resources.GetObject("tp_connection.AccessibleName")));
			this.tp_connection.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tp_connection.Anchor")));
			this.tp_connection.AutoScroll = ((bool)(resources.GetObject("tp_connection.AutoScroll")));
			this.tp_connection.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("tp_connection.AutoScrollMargin")));
			this.tp_connection.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("tp_connection.AutoScrollMinSize")));
			this.tp_connection.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tp_connection.BackgroundImage")));
			this.tp_connection.Controls.AddRange(new System.Windows.Forms.Control[] {
																																								this.btn_refresh,
																																								this.tb_server,
																																								this.btn_check_connection,
																																								this.gb_add_parms,
																																								this.cb_select_db,
																																								this.lab_select_db,
																																								this.tb_password,
																																								this.lab_password,
																																								this.tb_username,
																																								this.lab_username,
																																								this.lab_login,
																																								this.lab_server,
																																								this.lab_advise});
			this.tp_connection.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tp_connection.Dock")));
			this.tp_connection.Enabled = ((bool)(resources.GetObject("tp_connection.Enabled")));
			this.tp_connection.Font = ((System.Drawing.Font)(resources.GetObject("tp_connection.Font")));
			this.tp_connection.ImageIndex = ((int)(resources.GetObject("tp_connection.ImageIndex")));
			this.tp_connection.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tp_connection.ImeMode")));
			this.tp_connection.Location = ((System.Drawing.Point)(resources.GetObject("tp_connection.Location")));
			this.tp_connection.Name = "tp_connection";
			this.tp_connection.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tp_connection.RightToLeft")));
			this.tp_connection.Size = ((System.Drawing.Size)(resources.GetObject("tp_connection.Size")));
			this.tp_connection.TabIndex = ((int)(resources.GetObject("tp_connection.TabIndex")));
			this.tp_connection.Text = resources.GetString("tp_connection.Text");
			this.tp_connection.ToolTipText = resources.GetString("tp_connection.ToolTipText");
			this.tp_connection.Visible = ((bool)(resources.GetObject("tp_connection.Visible")));
			// 
			// btn_refresh
			// 
			this.btn_refresh.AccessibleDescription = ((string)(resources.GetObject("btn_refresh.AccessibleDescription")));
			this.btn_refresh.AccessibleName = ((string)(resources.GetObject("btn_refresh.AccessibleName")));
			this.btn_refresh.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btn_refresh.Anchor")));
			this.btn_refresh.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_refresh.BackgroundImage")));
			this.btn_refresh.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btn_refresh.Dock")));
			this.btn_refresh.Enabled = ((bool)(resources.GetObject("btn_refresh.Enabled")));
			this.btn_refresh.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btn_refresh.FlatStyle")));
			this.btn_refresh.Font = ((System.Drawing.Font)(resources.GetObject("btn_refresh.Font")));
			this.btn_refresh.Image = ((System.Drawing.Image)(resources.GetObject("btn_refresh.Image")));
			this.btn_refresh.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_refresh.ImageAlign")));
			this.btn_refresh.ImageIndex = ((int)(resources.GetObject("btn_refresh.ImageIndex")));
			this.btn_refresh.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btn_refresh.ImeMode")));
			this.btn_refresh.Location = ((System.Drawing.Point)(resources.GetObject("btn_refresh.Location")));
			this.btn_refresh.Name = "btn_refresh";
			this.btn_refresh.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btn_refresh.RightToLeft")));
			this.btn_refresh.Size = ((System.Drawing.Size)(resources.GetObject("btn_refresh.Size")));
			this.btn_refresh.TabIndex = ((int)(resources.GetObject("btn_refresh.TabIndex")));
			this.btn_refresh.Text = resources.GetString("btn_refresh.Text");
			this.btn_refresh.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_refresh.TextAlign")));
			this.btn_refresh.Visible = ((bool)(resources.GetObject("btn_refresh.Visible")));
			this.btn_refresh.Click += new System.EventHandler(this.btn_refresh_Click);
			// 
			// tb_server
			// 
			this.tb_server.AccessibleDescription = ((string)(resources.GetObject("tb_server.AccessibleDescription")));
			this.tb_server.AccessibleName = ((string)(resources.GetObject("tb_server.AccessibleName")));
			this.tb_server.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tb_server.Anchor")));
			this.tb_server.AutoSize = ((bool)(resources.GetObject("tb_server.AutoSize")));
			this.tb_server.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tb_server.BackgroundImage")));
			this.tb_server.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tb_server.Dock")));
			this.tb_server.Enabled = ((bool)(resources.GetObject("tb_server.Enabled")));
			this.tb_server.Font = ((System.Drawing.Font)(resources.GetObject("tb_server.Font")));
			this.tb_server.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tb_server.ImeMode")));
			this.tb_server.Location = ((System.Drawing.Point)(resources.GetObject("tb_server.Location")));
			this.tb_server.MaxLength = ((int)(resources.GetObject("tb_server.MaxLength")));
			this.tb_server.Multiline = ((bool)(resources.GetObject("tb_server.Multiline")));
			this.tb_server.Name = "tb_server";
			this.tb_server.PasswordChar = ((char)(resources.GetObject("tb_server.PasswordChar")));
			this.tb_server.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tb_server.RightToLeft")));
			this.tb_server.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tb_server.ScrollBars")));
			this.tb_server.Size = ((System.Drawing.Size)(resources.GetObject("tb_server.Size")));
			this.tb_server.TabIndex = ((int)(resources.GetObject("tb_server.TabIndex")));
			this.tb_server.Text = resources.GetString("tb_server.Text");
			this.tb_server.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tb_server.TextAlign")));
			this.tb_server.Visible = ((bool)(resources.GetObject("tb_server.Visible")));
			this.tb_server.WordWrap = ((bool)(resources.GetObject("tb_server.WordWrap")));
			// 
			// btn_check_connection
			// 
			this.btn_check_connection.AccessibleDescription = ((string)(resources.GetObject("btn_check_connection.AccessibleDescription")));
			this.btn_check_connection.AccessibleName = ((string)(resources.GetObject("btn_check_connection.AccessibleName")));
			this.btn_check_connection.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btn_check_connection.Anchor")));
			this.btn_check_connection.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_check_connection.BackgroundImage")));
			this.btn_check_connection.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btn_check_connection.Dock")));
			this.btn_check_connection.Enabled = ((bool)(resources.GetObject("btn_check_connection.Enabled")));
			this.btn_check_connection.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btn_check_connection.FlatStyle")));
			this.btn_check_connection.Font = ((System.Drawing.Font)(resources.GetObject("btn_check_connection.Font")));
			this.btn_check_connection.Image = ((System.Drawing.Image)(resources.GetObject("btn_check_connection.Image")));
			this.btn_check_connection.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_check_connection.ImageAlign")));
			this.btn_check_connection.ImageIndex = ((int)(resources.GetObject("btn_check_connection.ImageIndex")));
			this.btn_check_connection.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btn_check_connection.ImeMode")));
			this.btn_check_connection.Location = ((System.Drawing.Point)(resources.GetObject("btn_check_connection.Location")));
			this.btn_check_connection.Name = "btn_check_connection";
			this.btn_check_connection.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btn_check_connection.RightToLeft")));
			this.btn_check_connection.Size = ((System.Drawing.Size)(resources.GetObject("btn_check_connection.Size")));
			this.btn_check_connection.TabIndex = ((int)(resources.GetObject("btn_check_connection.TabIndex")));
			this.btn_check_connection.Text = resources.GetString("btn_check_connection.Text");
			this.btn_check_connection.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_check_connection.TextAlign")));
			this.btn_check_connection.Visible = ((bool)(resources.GetObject("btn_check_connection.Visible")));
			this.btn_check_connection.Click += new System.EventHandler(this.btn_check_connection_Click);
			// 
			// gb_add_parms
			// 
			this.gb_add_parms.AccessibleDescription = ((string)(resources.GetObject("gb_add_parms.AccessibleDescription")));
			this.gb_add_parms.AccessibleName = ((string)(resources.GetObject("gb_add_parms.AccessibleName")));
			this.gb_add_parms.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("gb_add_parms.Anchor")));
			this.gb_add_parms.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("gb_add_parms.BackgroundImage")));
			this.gb_add_parms.Controls.AddRange(new System.Windows.Forms.Control[] {
																																							 this.tb_port,
																																							 this.lab_port,
																																							 this.tb_timeout,
																																							 this.lab_timeout});
			this.gb_add_parms.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("gb_add_parms.Dock")));
			this.gb_add_parms.Enabled = ((bool)(resources.GetObject("gb_add_parms.Enabled")));
			this.gb_add_parms.Font = ((System.Drawing.Font)(resources.GetObject("gb_add_parms.Font")));
			this.gb_add_parms.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("gb_add_parms.ImeMode")));
			this.gb_add_parms.Location = ((System.Drawing.Point)(resources.GetObject("gb_add_parms.Location")));
			this.gb_add_parms.Name = "gb_add_parms";
			this.gb_add_parms.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("gb_add_parms.RightToLeft")));
			this.gb_add_parms.Size = ((System.Drawing.Size)(resources.GetObject("gb_add_parms.Size")));
			this.gb_add_parms.TabIndex = ((int)(resources.GetObject("gb_add_parms.TabIndex")));
			this.gb_add_parms.TabStop = false;
			this.gb_add_parms.Text = resources.GetString("gb_add_parms.Text");
			this.gb_add_parms.Visible = ((bool)(resources.GetObject("gb_add_parms.Visible")));
			// 
			// tb_port
			// 
			this.tb_port.AccessibleDescription = ((string)(resources.GetObject("tb_port.AccessibleDescription")));
			this.tb_port.AccessibleName = ((string)(resources.GetObject("tb_port.AccessibleName")));
			this.tb_port.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tb_port.Anchor")));
			this.tb_port.AutoSize = ((bool)(resources.GetObject("tb_port.AutoSize")));
			this.tb_port.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tb_port.BackgroundImage")));
			this.tb_port.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tb_port.Dock")));
			this.tb_port.Enabled = ((bool)(resources.GetObject("tb_port.Enabled")));
			this.tb_port.Font = ((System.Drawing.Font)(resources.GetObject("tb_port.Font")));
			this.tb_port.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tb_port.ImeMode")));
			this.tb_port.Location = ((System.Drawing.Point)(resources.GetObject("tb_port.Location")));
			this.tb_port.MaxLength = ((int)(resources.GetObject("tb_port.MaxLength")));
			this.tb_port.Multiline = ((bool)(resources.GetObject("tb_port.Multiline")));
			this.tb_port.Name = "tb_port";
			this.tb_port.PasswordChar = ((char)(resources.GetObject("tb_port.PasswordChar")));
			this.tb_port.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tb_port.RightToLeft")));
			this.tb_port.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tb_port.ScrollBars")));
			this.tb_port.Size = ((System.Drawing.Size)(resources.GetObject("tb_port.Size")));
			this.tb_port.TabIndex = ((int)(resources.GetObject("tb_port.TabIndex")));
			this.tb_port.Text = resources.GetString("tb_port.Text");
			this.tb_port.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tb_port.TextAlign")));
			this.tb_port.Visible = ((bool)(resources.GetObject("tb_port.Visible")));
			this.tb_port.WordWrap = ((bool)(resources.GetObject("tb_port.WordWrap")));
			// 
			// lab_port
			// 
			this.lab_port.AccessibleDescription = ((string)(resources.GetObject("lab_port.AccessibleDescription")));
			this.lab_port.AccessibleName = ((string)(resources.GetObject("lab_port.AccessibleName")));
			this.lab_port.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_port.Anchor")));
			this.lab_port.AutoSize = ((bool)(resources.GetObject("lab_port.AutoSize")));
			this.lab_port.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_port.Dock")));
			this.lab_port.Enabled = ((bool)(resources.GetObject("lab_port.Enabled")));
			this.lab_port.Font = ((System.Drawing.Font)(resources.GetObject("lab_port.Font")));
			this.lab_port.Image = ((System.Drawing.Image)(resources.GetObject("lab_port.Image")));
			this.lab_port.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_port.ImageAlign")));
			this.lab_port.ImageIndex = ((int)(resources.GetObject("lab_port.ImageIndex")));
			this.lab_port.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_port.ImeMode")));
			this.lab_port.Location = ((System.Drawing.Point)(resources.GetObject("lab_port.Location")));
			this.lab_port.Name = "lab_port";
			this.lab_port.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_port.RightToLeft")));
			this.lab_port.Size = ((System.Drawing.Size)(resources.GetObject("lab_port.Size")));
			this.lab_port.TabIndex = ((int)(resources.GetObject("lab_port.TabIndex")));
			this.lab_port.Text = resources.GetString("lab_port.Text");
			this.lab_port.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_port.TextAlign")));
			this.lab_port.Visible = ((bool)(resources.GetObject("lab_port.Visible")));
			// 
			// tb_timeout
			// 
			this.tb_timeout.AccessibleDescription = ((string)(resources.GetObject("tb_timeout.AccessibleDescription")));
			this.tb_timeout.AccessibleName = ((string)(resources.GetObject("tb_timeout.AccessibleName")));
			this.tb_timeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tb_timeout.Anchor")));
			this.tb_timeout.AutoSize = ((bool)(resources.GetObject("tb_timeout.AutoSize")));
			this.tb_timeout.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tb_timeout.BackgroundImage")));
			this.tb_timeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tb_timeout.Dock")));
			this.tb_timeout.Enabled = ((bool)(resources.GetObject("tb_timeout.Enabled")));
			this.tb_timeout.Font = ((System.Drawing.Font)(resources.GetObject("tb_timeout.Font")));
			this.tb_timeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tb_timeout.ImeMode")));
			this.tb_timeout.Location = ((System.Drawing.Point)(resources.GetObject("tb_timeout.Location")));
			this.tb_timeout.MaxLength = ((int)(resources.GetObject("tb_timeout.MaxLength")));
			this.tb_timeout.Multiline = ((bool)(resources.GetObject("tb_timeout.Multiline")));
			this.tb_timeout.Name = "tb_timeout";
			this.tb_timeout.PasswordChar = ((char)(resources.GetObject("tb_timeout.PasswordChar")));
			this.tb_timeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tb_timeout.RightToLeft")));
			this.tb_timeout.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tb_timeout.ScrollBars")));
			this.tb_timeout.Size = ((System.Drawing.Size)(resources.GetObject("tb_timeout.Size")));
			this.tb_timeout.TabIndex = ((int)(resources.GetObject("tb_timeout.TabIndex")));
			this.tb_timeout.Text = resources.GetString("tb_timeout.Text");
			this.tb_timeout.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tb_timeout.TextAlign")));
			this.tb_timeout.Visible = ((bool)(resources.GetObject("tb_timeout.Visible")));
			this.tb_timeout.WordWrap = ((bool)(resources.GetObject("tb_timeout.WordWrap")));
			// 
			// lab_timeout
			// 
			this.lab_timeout.AccessibleDescription = ((string)(resources.GetObject("lab_timeout.AccessibleDescription")));
			this.lab_timeout.AccessibleName = ((string)(resources.GetObject("lab_timeout.AccessibleName")));
			this.lab_timeout.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_timeout.Anchor")));
			this.lab_timeout.AutoSize = ((bool)(resources.GetObject("lab_timeout.AutoSize")));
			this.lab_timeout.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_timeout.Dock")));
			this.lab_timeout.Enabled = ((bool)(resources.GetObject("lab_timeout.Enabled")));
			this.lab_timeout.Font = ((System.Drawing.Font)(resources.GetObject("lab_timeout.Font")));
			this.lab_timeout.Image = ((System.Drawing.Image)(resources.GetObject("lab_timeout.Image")));
			this.lab_timeout.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_timeout.ImageAlign")));
			this.lab_timeout.ImageIndex = ((int)(resources.GetObject("lab_timeout.ImageIndex")));
			this.lab_timeout.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_timeout.ImeMode")));
			this.lab_timeout.Location = ((System.Drawing.Point)(resources.GetObject("lab_timeout.Location")));
			this.lab_timeout.Name = "lab_timeout";
			this.lab_timeout.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_timeout.RightToLeft")));
			this.lab_timeout.Size = ((System.Drawing.Size)(resources.GetObject("lab_timeout.Size")));
			this.lab_timeout.TabIndex = ((int)(resources.GetObject("lab_timeout.TabIndex")));
			this.lab_timeout.Text = resources.GetString("lab_timeout.Text");
			this.lab_timeout.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_timeout.TextAlign")));
			this.lab_timeout.Visible = ((bool)(resources.GetObject("lab_timeout.Visible")));
			// 
			// cb_select_db
			// 
			this.cb_select_db.AccessibleDescription = ((string)(resources.GetObject("cb_select_db.AccessibleDescription")));
			this.cb_select_db.AccessibleName = ((string)(resources.GetObject("cb_select_db.AccessibleName")));
			this.cb_select_db.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cb_select_db.Anchor")));
			this.cb_select_db.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cb_select_db.BackgroundImage")));
			this.cb_select_db.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cb_select_db.Dock")));
			this.cb_select_db.Enabled = ((bool)(resources.GetObject("cb_select_db.Enabled")));
			this.cb_select_db.Font = ((System.Drawing.Font)(resources.GetObject("cb_select_db.Font")));
			this.cb_select_db.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cb_select_db.ImeMode")));
			this.cb_select_db.IntegralHeight = ((bool)(resources.GetObject("cb_select_db.IntegralHeight")));
			this.cb_select_db.ItemHeight = ((int)(resources.GetObject("cb_select_db.ItemHeight")));
			this.cb_select_db.Location = ((System.Drawing.Point)(resources.GetObject("cb_select_db.Location")));
			this.cb_select_db.MaxDropDownItems = ((int)(resources.GetObject("cb_select_db.MaxDropDownItems")));
			this.cb_select_db.MaxLength = ((int)(resources.GetObject("cb_select_db.MaxLength")));
			this.cb_select_db.Name = "cb_select_db";
			this.cb_select_db.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cb_select_db.RightToLeft")));
			this.cb_select_db.Size = ((System.Drawing.Size)(resources.GetObject("cb_select_db.Size")));
			this.cb_select_db.TabIndex = ((int)(resources.GetObject("cb_select_db.TabIndex")));
			this.cb_select_db.Text = resources.GetString("cb_select_db.Text");
			this.cb_select_db.Visible = ((bool)(resources.GetObject("cb_select_db.Visible")));
			this.cb_select_db.DropDown += new System.EventHandler(this.cb_select_db_DropDown);
			// 
			// lab_select_db
			// 
			this.lab_select_db.AccessibleDescription = ((string)(resources.GetObject("lab_select_db.AccessibleDescription")));
			this.lab_select_db.AccessibleName = ((string)(resources.GetObject("lab_select_db.AccessibleName")));
			this.lab_select_db.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_select_db.Anchor")));
			this.lab_select_db.AutoSize = ((bool)(resources.GetObject("lab_select_db.AutoSize")));
			this.lab_select_db.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_select_db.Dock")));
			this.lab_select_db.Enabled = ((bool)(resources.GetObject("lab_select_db.Enabled")));
			this.lab_select_db.Font = ((System.Drawing.Font)(resources.GetObject("lab_select_db.Font")));
			this.lab_select_db.Image = ((System.Drawing.Image)(resources.GetObject("lab_select_db.Image")));
			this.lab_select_db.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_select_db.ImageAlign")));
			this.lab_select_db.ImageIndex = ((int)(resources.GetObject("lab_select_db.ImageIndex")));
			this.lab_select_db.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_select_db.ImeMode")));
			this.lab_select_db.Location = ((System.Drawing.Point)(resources.GetObject("lab_select_db.Location")));
			this.lab_select_db.Name = "lab_select_db";
			this.lab_select_db.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_select_db.RightToLeft")));
			this.lab_select_db.Size = ((System.Drawing.Size)(resources.GetObject("lab_select_db.Size")));
			this.lab_select_db.TabIndex = ((int)(resources.GetObject("lab_select_db.TabIndex")));
			this.lab_select_db.Text = resources.GetString("lab_select_db.Text");
			this.lab_select_db.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_select_db.TextAlign")));
			this.lab_select_db.Visible = ((bool)(resources.GetObject("lab_select_db.Visible")));
			// 
			// tb_password
			// 
			this.tb_password.AccessibleDescription = ((string)(resources.GetObject("tb_password.AccessibleDescription")));
			this.tb_password.AccessibleName = ((string)(resources.GetObject("tb_password.AccessibleName")));
			this.tb_password.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tb_password.Anchor")));
			this.tb_password.AutoSize = ((bool)(resources.GetObject("tb_password.AutoSize")));
			this.tb_password.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tb_password.BackgroundImage")));
			this.tb_password.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tb_password.Dock")));
			this.tb_password.Enabled = ((bool)(resources.GetObject("tb_password.Enabled")));
			this.tb_password.Font = ((System.Drawing.Font)(resources.GetObject("tb_password.Font")));
			this.tb_password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tb_password.ImeMode")));
			this.tb_password.Location = ((System.Drawing.Point)(resources.GetObject("tb_password.Location")));
			this.tb_password.MaxLength = ((int)(resources.GetObject("tb_password.MaxLength")));
			this.tb_password.Multiline = ((bool)(resources.GetObject("tb_password.Multiline")));
			this.tb_password.Name = "tb_password";
			this.tb_password.PasswordChar = ((char)(resources.GetObject("tb_password.PasswordChar")));
			this.tb_password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tb_password.RightToLeft")));
			this.tb_password.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tb_password.ScrollBars")));
			this.tb_password.Size = ((System.Drawing.Size)(resources.GetObject("tb_password.Size")));
			this.tb_password.TabIndex = ((int)(resources.GetObject("tb_password.TabIndex")));
			this.tb_password.Text = resources.GetString("tb_password.Text");
			this.tb_password.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tb_password.TextAlign")));
			this.tb_password.Visible = ((bool)(resources.GetObject("tb_password.Visible")));
			this.tb_password.WordWrap = ((bool)(resources.GetObject("tb_password.WordWrap")));
			// 
			// lab_password
			// 
			this.lab_password.AccessibleDescription = ((string)(resources.GetObject("lab_password.AccessibleDescription")));
			this.lab_password.AccessibleName = ((string)(resources.GetObject("lab_password.AccessibleName")));
			this.lab_password.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_password.Anchor")));
			this.lab_password.AutoSize = ((bool)(resources.GetObject("lab_password.AutoSize")));
			this.lab_password.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_password.Dock")));
			this.lab_password.Enabled = ((bool)(resources.GetObject("lab_password.Enabled")));
			this.lab_password.Font = ((System.Drawing.Font)(resources.GetObject("lab_password.Font")));
			this.lab_password.Image = ((System.Drawing.Image)(resources.GetObject("lab_password.Image")));
			this.lab_password.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_password.ImageAlign")));
			this.lab_password.ImageIndex = ((int)(resources.GetObject("lab_password.ImageIndex")));
			this.lab_password.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_password.ImeMode")));
			this.lab_password.Location = ((System.Drawing.Point)(resources.GetObject("lab_password.Location")));
			this.lab_password.Name = "lab_password";
			this.lab_password.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_password.RightToLeft")));
			this.lab_password.Size = ((System.Drawing.Size)(resources.GetObject("lab_password.Size")));
			this.lab_password.TabIndex = ((int)(resources.GetObject("lab_password.TabIndex")));
			this.lab_password.Text = resources.GetString("lab_password.Text");
			this.lab_password.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_password.TextAlign")));
			this.lab_password.Visible = ((bool)(resources.GetObject("lab_password.Visible")));
			// 
			// tb_username
			// 
			this.tb_username.AccessibleDescription = ((string)(resources.GetObject("tb_username.AccessibleDescription")));
			this.tb_username.AccessibleName = ((string)(resources.GetObject("tb_username.AccessibleName")));
			this.tb_username.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("tb_username.Anchor")));
			this.tb_username.AutoSize = ((bool)(resources.GetObject("tb_username.AutoSize")));
			this.tb_username.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("tb_username.BackgroundImage")));
			this.tb_username.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("tb_username.Dock")));
			this.tb_username.Enabled = ((bool)(resources.GetObject("tb_username.Enabled")));
			this.tb_username.Font = ((System.Drawing.Font)(resources.GetObject("tb_username.Font")));
			this.tb_username.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("tb_username.ImeMode")));
			this.tb_username.Location = ((System.Drawing.Point)(resources.GetObject("tb_username.Location")));
			this.tb_username.MaxLength = ((int)(resources.GetObject("tb_username.MaxLength")));
			this.tb_username.Multiline = ((bool)(resources.GetObject("tb_username.Multiline")));
			this.tb_username.Name = "tb_username";
			this.tb_username.PasswordChar = ((char)(resources.GetObject("tb_username.PasswordChar")));
			this.tb_username.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("tb_username.RightToLeft")));
			this.tb_username.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("tb_username.ScrollBars")));
			this.tb_username.Size = ((System.Drawing.Size)(resources.GetObject("tb_username.Size")));
			this.tb_username.TabIndex = ((int)(resources.GetObject("tb_username.TabIndex")));
			this.tb_username.Text = resources.GetString("tb_username.Text");
			this.tb_username.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("tb_username.TextAlign")));
			this.tb_username.Visible = ((bool)(resources.GetObject("tb_username.Visible")));
			this.tb_username.WordWrap = ((bool)(resources.GetObject("tb_username.WordWrap")));
			// 
			// lab_username
			// 
			this.lab_username.AccessibleDescription = ((string)(resources.GetObject("lab_username.AccessibleDescription")));
			this.lab_username.AccessibleName = ((string)(resources.GetObject("lab_username.AccessibleName")));
			this.lab_username.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_username.Anchor")));
			this.lab_username.AutoSize = ((bool)(resources.GetObject("lab_username.AutoSize")));
			this.lab_username.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_username.Dock")));
			this.lab_username.Enabled = ((bool)(resources.GetObject("lab_username.Enabled")));
			this.lab_username.Font = ((System.Drawing.Font)(resources.GetObject("lab_username.Font")));
			this.lab_username.Image = ((System.Drawing.Image)(resources.GetObject("lab_username.Image")));
			this.lab_username.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_username.ImageAlign")));
			this.lab_username.ImageIndex = ((int)(resources.GetObject("lab_username.ImageIndex")));
			this.lab_username.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_username.ImeMode")));
			this.lab_username.Location = ((System.Drawing.Point)(resources.GetObject("lab_username.Location")));
			this.lab_username.Name = "lab_username";
			this.lab_username.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_username.RightToLeft")));
			this.lab_username.Size = ((System.Drawing.Size)(resources.GetObject("lab_username.Size")));
			this.lab_username.TabIndex = ((int)(resources.GetObject("lab_username.TabIndex")));
			this.lab_username.Text = resources.GetString("lab_username.Text");
			this.lab_username.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_username.TextAlign")));
			this.lab_username.Visible = ((bool)(resources.GetObject("lab_username.Visible")));
			// 
			// lab_login
			// 
			this.lab_login.AccessibleDescription = ((string)(resources.GetObject("lab_login.AccessibleDescription")));
			this.lab_login.AccessibleName = ((string)(resources.GetObject("lab_login.AccessibleName")));
			this.lab_login.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_login.Anchor")));
			this.lab_login.AutoSize = ((bool)(resources.GetObject("lab_login.AutoSize")));
			this.lab_login.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_login.Dock")));
			this.lab_login.Enabled = ((bool)(resources.GetObject("lab_login.Enabled")));
			this.lab_login.Font = ((System.Drawing.Font)(resources.GetObject("lab_login.Font")));
			this.lab_login.Image = ((System.Drawing.Image)(resources.GetObject("lab_login.Image")));
			this.lab_login.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_login.ImageAlign")));
			this.lab_login.ImageIndex = ((int)(resources.GetObject("lab_login.ImageIndex")));
			this.lab_login.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_login.ImeMode")));
			this.lab_login.Location = ((System.Drawing.Point)(resources.GetObject("lab_login.Location")));
			this.lab_login.Name = "lab_login";
			this.lab_login.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_login.RightToLeft")));
			this.lab_login.Size = ((System.Drawing.Size)(resources.GetObject("lab_login.Size")));
			this.lab_login.TabIndex = ((int)(resources.GetObject("lab_login.TabIndex")));
			this.lab_login.Text = resources.GetString("lab_login.Text");
			this.lab_login.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_login.TextAlign")));
			this.lab_login.Visible = ((bool)(resources.GetObject("lab_login.Visible")));
			// 
			// lab_server
			// 
			this.lab_server.AccessibleDescription = ((string)(resources.GetObject("lab_server.AccessibleDescription")));
			this.lab_server.AccessibleName = ((string)(resources.GetObject("lab_server.AccessibleName")));
			this.lab_server.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_server.Anchor")));
			this.lab_server.AutoSize = ((bool)(resources.GetObject("lab_server.AutoSize")));
			this.lab_server.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_server.Dock")));
			this.lab_server.Enabled = ((bool)(resources.GetObject("lab_server.Enabled")));
			this.lab_server.Font = ((System.Drawing.Font)(resources.GetObject("lab_server.Font")));
			this.lab_server.Image = ((System.Drawing.Image)(resources.GetObject("lab_server.Image")));
			this.lab_server.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_server.ImageAlign")));
			this.lab_server.ImageIndex = ((int)(resources.GetObject("lab_server.ImageIndex")));
			this.lab_server.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_server.ImeMode")));
			this.lab_server.Location = ((System.Drawing.Point)(resources.GetObject("lab_server.Location")));
			this.lab_server.Name = "lab_server";
			this.lab_server.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_server.RightToLeft")));
			this.lab_server.Size = ((System.Drawing.Size)(resources.GetObject("lab_server.Size")));
			this.lab_server.TabIndex = ((int)(resources.GetObject("lab_server.TabIndex")));
			this.lab_server.Text = resources.GetString("lab_server.Text");
			this.lab_server.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_server.TextAlign")));
			this.lab_server.Visible = ((bool)(resources.GetObject("lab_server.Visible")));
			// 
			// lab_advise
			// 
			this.lab_advise.AccessibleDescription = ((string)(resources.GetObject("lab_advise.AccessibleDescription")));
			this.lab_advise.AccessibleName = ((string)(resources.GetObject("lab_advise.AccessibleName")));
			this.lab_advise.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("lab_advise.Anchor")));
			this.lab_advise.AutoSize = ((bool)(resources.GetObject("lab_advise.AutoSize")));
			this.lab_advise.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("lab_advise.Dock")));
			this.lab_advise.Enabled = ((bool)(resources.GetObject("lab_advise.Enabled")));
			this.lab_advise.Font = ((System.Drawing.Font)(resources.GetObject("lab_advise.Font")));
			this.lab_advise.Image = ((System.Drawing.Image)(resources.GetObject("lab_advise.Image")));
			this.lab_advise.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_advise.ImageAlign")));
			this.lab_advise.ImageIndex = ((int)(resources.GetObject("lab_advise.ImageIndex")));
			this.lab_advise.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("lab_advise.ImeMode")));
			this.lab_advise.Location = ((System.Drawing.Point)(resources.GetObject("lab_advise.Location")));
			this.lab_advise.Name = "lab_advise";
			this.lab_advise.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("lab_advise.RightToLeft")));
			this.lab_advise.Size = ((System.Drawing.Size)(resources.GetObject("lab_advise.Size")));
			this.lab_advise.TabIndex = ((int)(resources.GetObject("lab_advise.TabIndex")));
			this.lab_advise.Text = resources.GetString("lab_advise.Text");
			this.lab_advise.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("lab_advise.TextAlign")));
			this.lab_advise.Visible = ((bool)(resources.GetObject("lab_advise.Visible")));
			// 
			// btn_ok
			// 
			this.btn_ok.AccessibleDescription = ((string)(resources.GetObject("btn_ok.AccessibleDescription")));
			this.btn_ok.AccessibleName = ((string)(resources.GetObject("btn_ok.AccessibleName")));
			this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btn_ok.Anchor")));
			this.btn_ok.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_ok.BackgroundImage")));
			this.btn_ok.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btn_ok.Dock")));
			this.btn_ok.Enabled = ((bool)(resources.GetObject("btn_ok.Enabled")));
			this.btn_ok.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btn_ok.FlatStyle")));
			this.btn_ok.Font = ((System.Drawing.Font)(resources.GetObject("btn_ok.Font")));
			this.btn_ok.Image = ((System.Drawing.Image)(resources.GetObject("btn_ok.Image")));
			this.btn_ok.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_ok.ImageAlign")));
			this.btn_ok.ImageIndex = ((int)(resources.GetObject("btn_ok.ImageIndex")));
			this.btn_ok.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btn_ok.ImeMode")));
			this.btn_ok.Location = ((System.Drawing.Point)(resources.GetObject("btn_ok.Location")));
			this.btn_ok.Name = "btn_ok";
			this.btn_ok.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btn_ok.RightToLeft")));
			this.btn_ok.Size = ((System.Drawing.Size)(resources.GetObject("btn_ok.Size")));
			this.btn_ok.TabIndex = ((int)(resources.GetObject("btn_ok.TabIndex")));
			this.btn_ok.Text = resources.GetString("btn_ok.Text");
			this.btn_ok.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_ok.TextAlign")));
			this.btn_ok.Visible = ((bool)(resources.GetObject("btn_ok.Visible")));
			this.btn_ok.Click += new System.EventHandler(this.btn_ok_Click);
			// 
			// btn_cancel
			// 
			this.btn_cancel.AccessibleDescription = ((string)(resources.GetObject("btn_cancel.AccessibleDescription")));
			this.btn_cancel.AccessibleName = ((string)(resources.GetObject("btn_cancel.AccessibleName")));
			this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btn_cancel.Anchor")));
			this.btn_cancel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_cancel.BackgroundImage")));
			this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btn_cancel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btn_cancel.Dock")));
			this.btn_cancel.Enabled = ((bool)(resources.GetObject("btn_cancel.Enabled")));
			this.btn_cancel.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btn_cancel.FlatStyle")));
			this.btn_cancel.Font = ((System.Drawing.Font)(resources.GetObject("btn_cancel.Font")));
			this.btn_cancel.Image = ((System.Drawing.Image)(resources.GetObject("btn_cancel.Image")));
			this.btn_cancel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_cancel.ImageAlign")));
			this.btn_cancel.ImageIndex = ((int)(resources.GetObject("btn_cancel.ImageIndex")));
			this.btn_cancel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btn_cancel.ImeMode")));
			this.btn_cancel.Location = ((System.Drawing.Point)(resources.GetObject("btn_cancel.Location")));
			this.btn_cancel.Name = "btn_cancel";
			this.btn_cancel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btn_cancel.RightToLeft")));
			this.btn_cancel.Size = ((System.Drawing.Size)(resources.GetObject("btn_cancel.Size")));
			this.btn_cancel.TabIndex = ((int)(resources.GetObject("btn_cancel.TabIndex")));
			this.btn_cancel.Text = resources.GetString("btn_cancel.Text");
			this.btn_cancel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_cancel.TextAlign")));
			this.btn_cancel.Visible = ((bool)(resources.GetObject("btn_cancel.Visible")));
			this.btn_cancel.Click += new System.EventHandler(this.btn_cancel_Click);
			// 
			// btn_help
			// 
			this.btn_help.AccessibleDescription = ((string)(resources.GetObject("btn_help.AccessibleDescription")));
			this.btn_help.AccessibleName = ((string)(resources.GetObject("btn_help.AccessibleName")));
			this.btn_help.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("btn_help.Anchor")));
			this.btn_help.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn_help.BackgroundImage")));
			this.btn_help.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("btn_help.Dock")));
			this.btn_help.Enabled = ((bool)(resources.GetObject("btn_help.Enabled")));
			this.btn_help.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("btn_help.FlatStyle")));
			this.btn_help.Font = ((System.Drawing.Font)(resources.GetObject("btn_help.Font")));
			this.btn_help.Image = ((System.Drawing.Image)(resources.GetObject("btn_help.Image")));
			this.btn_help.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_help.ImageAlign")));
			this.btn_help.ImageIndex = ((int)(resources.GetObject("btn_help.ImageIndex")));
			this.btn_help.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("btn_help.ImeMode")));
			this.btn_help.Location = ((System.Drawing.Point)(resources.GetObject("btn_help.Location")));
			this.btn_help.Name = "btn_help";
			this.btn_help.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("btn_help.RightToLeft")));
			this.btn_help.Size = ((System.Drawing.Size)(resources.GetObject("btn_help.Size")));
			this.btn_help.TabIndex = ((int)(resources.GetObject("btn_help.TabIndex")));
			this.btn_help.Text = resources.GetString("btn_help.Text");
			this.btn_help.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("btn_help.TextAlign")));
			this.btn_help.Visible = ((bool)(resources.GetObject("btn_help.Visible")));
			this.btn_help.Click += new System.EventHandler(this.btn_help_Click);
			// 
			// ConnectionStringEditorForm
			// 
			this.AcceptButton = this.btn_ok;
			this.AccessibleDescription = ((string)(resources.GetObject("$this.AccessibleDescription")));
			this.AccessibleName = ((string)(resources.GetObject("$this.AccessibleName")));
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.btn_cancel;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																																	this.btn_help,
																																	this.btn_cancel,
																																	this.btn_ok,
																																	this.tc_main});
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximizeBox = false;
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimizeBox = false;
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "ConnectionStringEditorForm";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.ShowInTaskbar = false;
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.tc_main.ResumeLayout(false);
			this.tp_connection.ResumeLayout(false);
			this.gb_add_parms.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void btn_cancel_Click(object sender, System.EventArgs e) {
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btn_ok_Click(object sender, System.EventArgs e) {
			if(connect(false) == true){
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
		}

		private void btn_help_Click(object sender, System.EventArgs e) {
		
		}

		private void btn_check_connection_Click(object sender, System.EventArgs e) {
			if(connect(false) == true){
				MessageBox.Show(this, resman.GetString("MsgboxText_Success"), resman.GetString("MsgboxTitle_Success"), MessageBoxButtons.OK, MessageBoxIcon.None);
			}
		}

		/// <summary>
		/// Returns the generated ConnectionString
		/// </summary>
		public string ConnectionString {
			get {
				return this.pgconn.ConnectionString;
			}
		}

		private bool connect(bool fillComboBox) {
			try{
				StringWriter sw = new StringWriter();
				if(this.tb_server.Text == String.Empty){
					MessageBox.Show(this, resman.GetString("MsgboxText_NoServer"), resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				if(this.tb_username.Text == String.Empty){
					MessageBox.Show(this, resman.GetString("MsgboxText_NoUser"), resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				sw.Write("Server={0};", this.tb_server.Text);
				if(this.tb_port.Text != String.Empty && Convert.ToInt32(this.tb_port.Text) != ConnectionStringDefaults.Port){
					sw.Write("{0}={1};", ConnectionStringKeys.Port, tb_port.Text);
				}
				// this happens if the user clicks Ok or Check Connection
				// before selecting a database
				if(fillComboBox == false && (String)this.cb_select_db.Text == String.Empty){
					MessageBox.Show(this, resman.GetString("MsgboxText_NoDb"), resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
					// this happens if the user clicks the database-combobox
					// in order to select a database
				else if(fillComboBox == true && (String)this.cb_select_db.Text == String.Empty){
					sw.Write("{0}=template1;", ConnectionStringKeys.Database);
				}
				else{
					sw.Write("{0}={1};", ConnectionStringKeys.Database, this.cb_select_db.Text);
				}
				try{
					if(this.tb_timeout.Text != String.Empty && Convert.ToInt32(this.tb_timeout.Text) != ConnectionStringDefaults.Timeout){
						sw.Write("{0}={1};", ConnectionStringKeys.Timeout, this.tb_timeout.Text);
					}
				}
					// don't mind if the value is nonsense - just don't put it into the string
				catch(FormatException){
					MessageBox.Show(this, resman.GetString("MsgboxText_TimeoutNaN"), resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}
				catch(OverflowException){
					MessageBox.Show(this, resman.GetString("MsgboxText_TimeoutOverflow"), resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
					return false;
				}

				sw.Write("{0}={1};", ConnectionStringKeys.UserName, this.tb_username.Text);
				sw.Write("{0}={1};", ConnectionStringKeys.Password, this.tb_password.Text);
				this.pgconn.ConnectionString = sw.ToString();
				this.pgconn.Open();
				if(fillComboBox == true){
					cb_select_db.Items.Clear();
					NpgsqlCommand com = new NpgsqlCommand("SELECT datname FROM pg_database WHERE datallowconn = 't'", this.pgconn);
					NpgsqlDataReader dr = com.ExecuteReader();
					while(dr.Read()){
						cb_select_db.Items.Add(dr["datname"]);
						if(cb_select_db.Items.Count > 0){
							cb_select_db.SelectedIndex = 0;
						}
					}
				}
				this.pgconn.Close();
			}catch(Exception ex){
				MessageBox.Show(this, ex.Message, resman.GetString("MsgboxTitle_Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
			return true;
		}

		private void btn_refresh_Click(object sender, System.EventArgs e) {
			connect(true);
		}



		private void cb_select_db_DropDown(object sender, System.EventArgs e) {

			if(cb_select_db.Items.Count < 1){
				connect(true);
			}
		}
	}
}
