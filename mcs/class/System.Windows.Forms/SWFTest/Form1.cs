using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

namespace SWFTest
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.MonthCalendar monthCalendar1;
		private System.Windows.Forms.DateTimePicker dateTimePicker1;
		private System.Windows.Forms.HScrollBar hScrollBar1;
		private System.Windows.Forms.VScrollBar vScrollBar1;
		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.DomainUpDown domainUpDown1;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.TrackBar trackBar1;
		private System.Windows.Forms.ProgressBar progressBar1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.HelpProvider helpProvider1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ContextMenu contextMenu2;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.FontDialog fontDialog1;
		private System.Windows.Forms.ColorDialog colorDialog1;
		private System.Windows.Forms.PrintDialog printDialog1;
		private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
		private System.Windows.Forms.ErrorProvider errorProvider1;
		private System.Drawing.Printing.PrintDocument printDocument1;
		private System.Windows.Forms.PageSetupDialog pageSetupDialog1;
		private System.ComponentModel.IContainer components;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.button1 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.toolBar1 = new System.Windows.Forms.ToolBar();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.panel1 = new System.Windows.Forms.Panel();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.monthCalendar1 = new System.Windows.Forms.MonthCalendar();
			this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
			this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
			this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.domainUpDown1 = new System.Windows.Forms.DomainUpDown();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.progressBar1 = new System.Windows.Forms.ProgressBar();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			//this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.helpProvider1 = new System.Windows.Forms.HelpProvider();
			//this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.contextMenu2 = new System.Windows.Forms.ContextMenu();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.fontDialog1 = new System.Windows.Forms.FontDialog();
			this.colorDialog1 = new System.Windows.Forms.ColorDialog();
			this.printDialog1 = new System.Windows.Forms.PrintDialog();
			this.printPreviewControl1 = new System.Windows.Forms.PrintPreviewControl();
			this.errorProvider1 = new System.Windows.Forms.ErrorProvider();
			this.printDocument1 = new System.Drawing.Printing.PrintDocument();
			this.pageSetupDialog1 = new System.Windows.Forms.PageSetupDialog();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(111, 252);
			this.label1.Name = "label1";
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			// 
			// linkLabel1
			// 
			this.linkLabel1.Location = new System.Drawing.Point(26, 74);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.TabIndex = 1;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "linkLabel1";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(26, 117);
			this.button1.Name = "button1";
			this.button1.TabIndex = 2;
			this.button1.Text = "button1";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(17, 157);
			this.textBox1.Name = "textBox1";
			this.textBox1.TabIndex = 3;
			this.textBox1.Text = "textBox1";
			// 
			// checkBox1
			// 
			this.checkBox1.Location = new System.Drawing.Point(16, 211);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.TabIndex = 4;
			this.checkBox1.Text = "checkBox1";
			// 
			// radioButton1
			// 
			this.radioButton1.Location = new System.Drawing.Point(10, 257);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.TabIndex = 5;
			this.radioButton1.Text = "radioButton1";
			// 
			// groupBox1
			// 
			this.groupBox1.Location = new System.Drawing.Point(14, 293);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.TabIndex = 6;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "groupBox1";
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 548);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(813, 22);
			this.statusBar1.TabIndex = 7;
			this.statusBar1.Text = "statusBar1";
			// 
			// toolBar1
			// 
			this.toolBar1.DropDownArrows = true;
			this.toolBar1.Location = new System.Drawing.Point(0, 0);
			this.toolBar1.Name = "toolBar1";
			this.toolBar1.ShowToolTips = true;
			this.toolBar1.Size = new System.Drawing.Size(813, 42);
			this.toolBar1.TabIndex = 8;
			// 
			// pictureBox1
			// 
			this.pictureBox1.BackColor = System.Drawing.Color.Red;
			this.pictureBox1.Location = new System.Drawing.Point(22, 415);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.TabIndex = 9;
			this.pictureBox1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Location = new System.Drawing.Point(164, 421);
			this.panel1.Name = "panel1";
			this.panel1.TabIndex = 10;
			// 
			// dataGrid1
			// 
			this.dataGrid1.DataMember = "";
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(147, 78);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.TabIndex = 11;
			// 
			// monthCalendar1
			// 
			this.monthCalendar1.Location = new System.Drawing.Point(318, 72);
			this.monthCalendar1.Name = "monthCalendar1";
			this.monthCalendar1.TabIndex = 12;
			// 
			// dateTimePicker1
			// 
			this.dateTimePicker1.Location = new System.Drawing.Point(109, 188);
			this.dateTimePicker1.Name = "dateTimePicker1";
			this.dateTimePicker1.TabIndex = 13;
			// 
			// hScrollBar1
			// 
			this.hScrollBar1.Location = new System.Drawing.Point(189, 242);
			this.hScrollBar1.Name = "hScrollBar1";
			this.hScrollBar1.TabIndex = 14;
			// 
			// vScrollBar1
			// 
			this.vScrollBar1.Location = new System.Drawing.Point(241, 318);
			this.vScrollBar1.Name = "vScrollBar1";
			this.vScrollBar1.TabIndex = 15;
			// 
			// splitter1
			// 
			this.splitter1.Location = new System.Drawing.Point(0, 42);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 506);
			this.splitter1.TabIndex = 16;
			this.splitter1.TabStop = false;
			// 
			// domainUpDown1
			// 
			this.domainUpDown1.Location = new System.Drawing.Point(315, 257);
			this.domainUpDown1.Name = "domainUpDown1";
			this.domainUpDown1.TabIndex = 17;
			this.domainUpDown1.Text = "domainUpDown1";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.Location = new System.Drawing.Point(319, 314);
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.TabIndex = 18;
			// 
			// trackBar1
			// 
			this.trackBar1.Location = new System.Drawing.Point(392, 374);
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.TabIndex = 19;
			// 
			// progressBar1
			// 
			this.progressBar1.Location = new System.Drawing.Point(381, 442);
			this.progressBar1.Name = "progressBar1";
			this.progressBar1.TabIndex = 20;
			// 
			// richTextBox1
			// 
			this.richTextBox1.Location = new System.Drawing.Point(527, 79);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.TabIndex = 21;
			this.richTextBox1.Text = "richTextBox1";
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.Text = "notifyIcon1";
			this.notifyIcon1.Visible = true;

			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.FileName = "doc1";
			// 
			// printPreviewControl1
			// 
			this.printPreviewControl1.AutoZoom = false;
			this.printPreviewControl1.Location = new System.Drawing.Point(456, 260);
			this.printPreviewControl1.Name = "printPreviewControl1";
			this.printPreviewControl1.Size = new System.Drawing.Size(100, 100);
			this.printPreviewControl1.TabIndex = 22;
			this.printPreviewControl1.Zoom = 0.3;
			// 
			// errorProvider1
			// 
			this.errorProvider1.ContainerControl = this;
			this.errorProvider1.DataMember = "";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(813, 570);
			this.Controls.Add(this.printPreviewControl1);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.progressBar1);
			this.Controls.Add(this.trackBar1);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.domainUpDown1);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.vScrollBar1);
			this.Controls.Add(this.hScrollBar1);
			this.Controls.Add(this.dateTimePicker1);
			this.Controls.Add(this.monthCalendar1);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.toolBar1);
			this.Controls.Add(this.statusBar1);
			this.Controls.Add(this.groupBox1);
			this.Controls.Add(this.radioButton1);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.label1);
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.Text = "Form1";
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}
	}
}
