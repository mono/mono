// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Reflection;

namespace System.Windows.Forms {

	[DefaultProperty("Document")]
	public sealed class PageSetupDialog : CommonDialog {
		#region Local variables
		private PrintDocument document;
		private PageSettings page_settings;
		private PrinterSettings printer_settings;
		private Margins	min_margins;
		private bool allow_margins;
		private bool allow_orientation;
		private bool allow_paper;
		private bool allow_printer;
		private bool show_help;
		private bool show_network;

		private GroupBox groupbox_paper;
		private Label label_source;
		private Label label_size;
		private GroupBox groupbox_orientation;
		private RadioButton radio_landscape;
		private RadioButton radio_portrait;
		private GroupBox groupbox_margin;
		private Label label_left;
		private Button button_ok;
		private Button button_cancel;
		private Button button_printer;
		private Label label_top;
		private Label label_right;
		private Label label_bottom;
		private TextBox textbox_left;
		private TextBox textbox_top;
		private TextBox textbox_right;
		private TextBox textbox_bottom;
		private ComboBox combobox_source;
		private ComboBox combobox_size;
		#endregion // Local variables

		#region Public Constructors
		public PageSetupDialog () {
			InitializeComponent();
		}
		#endregion // Public Constructors


		#region Public Instance Methods
		public override void Reset () {
		}
		#endregion // Public Instance Methods

		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AllowMargins {
			get { return allow_margins; }
			set { allow_margins = value; }
		}

		[DefaultValue(true)]
		public bool AllowOrientation {
			get { return allow_orientation; }
			set { allow_orientation = value; }
		}

		[DefaultValue(true)]
		public bool AllowPaper {
			get { return allow_paper; }
			set { allow_paper = value; }
		}

		[DefaultValue(true)]
		public bool AllowPrinter {
			get { return allow_printer; }
			set { allow_printer = value; }
		}

		[DefaultValue(null)]
		public PrintDocument Document {
			get { return document; }
			set { document = value; }
		}

		public Margins MinMargins {
			get { return min_margins; }
			set { min_margins = value; }
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PageSettings PageSettings {
			get { return page_settings; }
			set { page_settings = value; }
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings {
			get { return printer_settings; }
			set { printer_settings = value; }
		}

		[DefaultValue(false)]
		public bool ShowHelp {
			get { return show_help; }
			set { show_help = value; }
		}

		[DefaultValue(true)]
		public bool ShowNetwork {
			get { return show_network; }
			set { show_network = value; }
		}

		#endregion // Public Instance Properties

		#region Protected Instance Methods
		protected override bool RunDialog (IntPtr hwnd) {
			return true;
		}
		#endregion // Protected Instance Methods

		#region Private Helper
		private void InitializeComponent() {
			this.groupbox_paper = new System.Windows.Forms.GroupBox();
			this.combobox_source = new System.Windows.Forms.ComboBox();
			this.combobox_size = new System.Windows.Forms.ComboBox();
			this.label_source = new System.Windows.Forms.Label();
			this.label_size = new System.Windows.Forms.Label();
			this.groupbox_orientation = new System.Windows.Forms.GroupBox();
			this.radio_landscape = new System.Windows.Forms.RadioButton();
			this.radio_portrait = new System.Windows.Forms.RadioButton();
			this.groupbox_margin = new System.Windows.Forms.GroupBox();
			this.label_left = new System.Windows.Forms.Label();
			this.button_ok = new System.Windows.Forms.Button();
			this.button_cancel = new System.Windows.Forms.Button();
			this.button_printer = new System.Windows.Forms.Button();
			this.label_top = new System.Windows.Forms.Label();
			this.label_right = new System.Windows.Forms.Label();
			this.label_bottom = new System.Windows.Forms.Label();
			this.textbox_left = new System.Windows.Forms.TextBox();
			this.textbox_top = new System.Windows.Forms.TextBox();
			this.textbox_right = new System.Windows.Forms.TextBox();
			this.textbox_bottom = new System.Windows.Forms.TextBox();
			this.groupbox_paper.SuspendLayout();
			this.groupbox_orientation.SuspendLayout();
			this.groupbox_margin.SuspendLayout();
			form.SuspendLayout();
			// 
			// groupbox_paper
			// 
			this.groupbox_paper.Controls.Add(this.combobox_source);
			this.groupbox_paper.Controls.Add(this.combobox_size);
			this.groupbox_paper.Controls.Add(this.label_source);
			this.groupbox_paper.Controls.Add(this.label_size);
			this.groupbox_paper.Location = new System.Drawing.Point(12, 157);
			this.groupbox_paper.Name = "groupbox_paper";
			this.groupbox_paper.Size = new System.Drawing.Size(336, 90);
			this.groupbox_paper.TabIndex = 0;
			this.groupbox_paper.TabStop = false;
			this.groupbox_paper.Text = "Paper";
			// 
			// combobox_source
			// 
			this.combobox_source.Location = new System.Drawing.Point(84, 54);
			this.combobox_source.Name = "combobox_source";
			this.combobox_source.Size = new System.Drawing.Size(240, 21);
			this.combobox_source.TabIndex = 3;
			this.combobox_source.Text = "Default";
			// 
			// combobox_size
			// 
			this.combobox_size.ItemHeight = 13;
			this.combobox_size.Location = new System.Drawing.Point(84, 22);
			this.combobox_size.Name = "combobox_size";
			this.combobox_size.Size = new System.Drawing.Size(240, 21);
			this.combobox_size.TabIndex = 2;
			this.combobox_size.Text = "A4";
			// 
			// label_source
			// 
			this.label_source.Location = new System.Drawing.Point(10, 58);
			this.label_source.Name = "label_source";
			this.label_source.Size = new System.Drawing.Size(48, 16);
			this.label_source.TabIndex = 1;
			this.label_source.Text = "&Source:";
			// 
			// label_size
			// 
			this.label_size.Location = new System.Drawing.Point(10, 25);
			this.label_size.Name = "label_size";
			this.label_size.Size = new System.Drawing.Size(52, 16);
			this.label_size.TabIndex = 0;
			this.label_size.Text = "Si&ze:";
			// 
			// groupbox_orientation
			// 
			this.groupbox_orientation.Controls.Add(this.radio_landscape);
			this.groupbox_orientation.Controls.Add(this.radio_portrait);
			this.groupbox_orientation.Location = new System.Drawing.Point(12, 255);
			this.groupbox_orientation.Name = "groupbox_orientation";
			this.groupbox_orientation.Size = new System.Drawing.Size(96, 90);
			this.groupbox_orientation.TabIndex = 1;
			this.groupbox_orientation.TabStop = false;
			this.groupbox_orientation.Text = "Orientation";
			// 
			// radio_landscape
			// 
			this.radio_landscape.Location = new System.Drawing.Point(13, 52);
			this.radio_landscape.Name = "radio_landscape";
			this.radio_landscape.Size = new System.Drawing.Size(80, 24);
			this.radio_landscape.TabIndex = 7;
			this.radio_landscape.Text = "L&andscape";
			// 
			// radio_portrait
			// 
			this.radio_portrait.Location = new System.Drawing.Point(13, 19);
			this.radio_portrait.Name = "radio_portrait";
			this.radio_portrait.Size = new System.Drawing.Size(72, 24);
			this.radio_portrait.TabIndex = 6;
			this.radio_portrait.Text = "P&ortrait";
			// 
			// groupbox_margin
			// 
			this.groupbox_margin.Controls.Add(this.textbox_bottom);
			this.groupbox_margin.Controls.Add(this.textbox_right);
			this.groupbox_margin.Controls.Add(this.textbox_top);
			this.groupbox_margin.Controls.Add(this.textbox_left);
			this.groupbox_margin.Controls.Add(this.label_bottom);
			this.groupbox_margin.Controls.Add(this.label_right);
			this.groupbox_margin.Controls.Add(this.label_top);
			this.groupbox_margin.Controls.Add(this.label_left);
			this.groupbox_margin.Location = new System.Drawing.Point(120, 255);
			this.groupbox_margin.Name = "groupbox_margin";
			this.groupbox_margin.Size = new System.Drawing.Size(228, 90);
			this.groupbox_margin.TabIndex = 2;
			this.groupbox_margin.TabStop = false;
			this.groupbox_margin.Text = "Margins (inches)";
			// 
			// label_left
			// 
			this.label_left.Location = new System.Drawing.Point(10, 25);
			this.label_left.Name = "label_left";
			this.label_left.Size = new System.Drawing.Size(40, 23);
			this.label_left.TabIndex = 0;
			this.label_left.Text = "&Left:";
			// 
			// button_ok
			// 
			this.button_ok.Location = new System.Drawing.Point(120, 358);
			this.button_ok.Name = "button_ok";
			this.button_ok.Size = new System.Drawing.Size(72, 23);
			this.button_ok.TabIndex = 3;
			this.button_ok.Text = "OK";
			// 
			// button_cancel
			// 
			this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button_cancel.Location = new System.Drawing.Point(198, 358);
			this.button_cancel.Name = "button_cancel";
			this.button_cancel.Size = new System.Drawing.Size(72, 23);
			this.button_cancel.TabIndex = 4;
			this.button_cancel.Text = "Cancel";
			// 
			// button_printer
			// 
			this.button_printer.Location = new System.Drawing.Point(276, 358);
			this.button_printer.Name = "button_printer";
			this.button_printer.Size = new System.Drawing.Size(72, 23);
			this.button_printer.TabIndex = 5;
			this.button_printer.Text = "&Printer...";
			// 
			// label_top
			// 
			this.label_top.Location = new System.Drawing.Point(10, 57);
			this.label_top.Name = "label_top";
			this.label_top.Size = new System.Drawing.Size(40, 23);
			this.label_top.TabIndex = 1;
			this.label_top.Text = "&Top:";
			// 
			// label_right
			// 
			this.label_right.Location = new System.Drawing.Point(124, 25);
			this.label_right.Name = "label_right";
			this.label_right.Size = new System.Drawing.Size(40, 23);
			this.label_right.TabIndex = 2;
			this.label_right.Text = "&Right:";
			// 
			// label_bottom
			// 
			this.label_bottom.Location = new System.Drawing.Point(124, 57);
			this.label_bottom.Name = "label_bottom";
			this.label_bottom.Size = new System.Drawing.Size(40, 23);
			this.label_bottom.TabIndex = 3;
			this.label_bottom.Text = "&Bottom:";
			// 
			// textbox_left
			// 
			this.textbox_left.Location = new System.Drawing.Point(57, 21);
			this.textbox_left.Name = "textbox_left";
			this.textbox_left.Size = new System.Drawing.Size(48, 20);
			this.textbox_left.TabIndex = 4;
			this.textbox_left.Text = "1";
			// 
			// textbox_top
			// 
			this.textbox_top.Location = new System.Drawing.Point(57, 54);
			this.textbox_top.Name = "textbox_top";
			this.textbox_top.Size = new System.Drawing.Size(48, 20);
			this.textbox_top.TabIndex = 5;
			this.textbox_top.Text = "1";
			// 
			// textbox_right
			// 
			this.textbox_right.Location = new System.Drawing.Point(171, 21);
			this.textbox_right.Name = "textbox_right";
			this.textbox_right.Size = new System.Drawing.Size(48, 20);
			this.textbox_right.TabIndex = 6;
			this.textbox_right.Text = "1";
			// 
			// textbox_bottom
			// 
			this.textbox_bottom.Location = new System.Drawing.Point(171, 54);
			this.textbox_bottom.Name = "textbox_bottom";
			this.textbox_bottom.Size = new System.Drawing.Size(48, 20);
			this.textbox_bottom.TabIndex = 7;
			this.textbox_bottom.Text = "1";
			// 
			// Form3
			// 
			form.AcceptButton = this.button_ok;
			form.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			form.CancelButton = this.button_cancel;
			form.ClientSize = new System.Drawing.Size(360, 390);
			form.Controls.Add(this.button_printer);
			form.Controls.Add(this.button_cancel);
			form.Controls.Add(this.button_ok);
			form.Controls.Add(this.groupbox_margin);
			form.Controls.Add(this.groupbox_orientation);
			form.Controls.Add(this.groupbox_paper);
			form.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			form.HelpButton = true;
			form.MaximizeBox = false;
			form.MinimizeBox = false;
			form.Name = "Form3";
			form.ShowInTaskbar = false;
			form.Text = "Page Setup";
			this.groupbox_paper.ResumeLayout(false);
			this.groupbox_orientation.ResumeLayout(false);
			this.groupbox_margin.ResumeLayout(false);
			form.ResumeLayout(false);

		}
		#endregion // Private Helper
	}
}
