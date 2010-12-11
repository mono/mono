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
using System.Globalization;
using System.Reflection;
using System.Text;

namespace System.Windows.Forms
{
	[DefaultProperty("Document")]
	public sealed class PageSetupDialog : CommonDialog
	{
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
#if NET_2_0
		private bool enable_metric;
#endif
		private GroupBox groupbox_paper;
		private Label label_source;
		private Label label_size;
		private GroupBox groupbox_orientation;
		private RadioButton radio_landscape;
		private RadioButton radio_portrait;
		private GroupBox groupbox_margin;
		private Label label_left;
		private Button button_help;
		private Button button_ok;
		private Button button_cancel;
		private Button button_printer;
		private Label label_top;
		private Label label_right;
		private Label label_bottom;
		private NumericTextBox textbox_left;
		private NumericTextBox textbox_top;
		private NumericTextBox textbox_right;
		private NumericTextBox textbox_bottom;
		private ComboBox combobox_source;
		private ComboBox combobox_size;
		private PagePreview pagePreview;
		#endregion // Local variables

		#region Public Constructors
		public PageSetupDialog ()
		{
			form = new DialogForm (this);
			InitializeComponent();
			Reset ();
		}
		#endregion // Public Constructors

		#region Public Instance Methods
		public override void Reset ()
		{
			AllowMargins = true;
			AllowOrientation = true;
			AllowPaper = true;
			AllowPrinter = true;
			ShowHelp = false;
			ShowNetwork = true;
			MinMargins = new Margins (0, 0, 0, 0);
			PrinterSettings = null;
			PageSettings = null;
			Document = null;
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
			set { 
				document = value;
				if (document != null) {
					printer_settings = document.PrinterSettings;
					page_settings = document.DefaultPageSettings;
				}
			}
		}

#if NET_2_0
		[Browsable (true)]
		[DefaultValue (false)]
		[MonoTODO ("Stubbed, not implemented")]
		[EditorBrowsableAttribute (EditorBrowsableState.Always)]
		public bool EnableMetric {
			get { return enable_metric; }
			set { enable_metric = value; }
		}
#endif

		public Margins MinMargins {
			get { return min_margins; }
			set { min_margins = value; }
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PageSettings PageSettings {
			get { return page_settings; }
			set {
				page_settings = value;
				document = null;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings {
			get { return printer_settings; }
			set {
				printer_settings = value;
				document = null;
			}
		}

		[DefaultValue(false)]
		public bool ShowHelp {
			get { return show_help; }
			set {
				if (value != show_help) {
					show_help = value;
					ShowHelpButton ();
				}
			}
		}

		[DefaultValue(true)]
		public bool ShowNetwork {
			get { return show_network; }
			set { show_network = value; }
		}

		#endregion // Public Instance Properties

		#region Protected Instance Methods
		protected override bool RunDialog (IntPtr hwndOwner)
		{
			try {
				SetPrinterDetails ();
				return true;
			} catch (Exception e) {
				MessageBox.Show (e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return false;
			}
		}
		#endregion // Protected Instance Methods

		#region Private Helper
		private void InitializeComponent()
		{
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
			this.textbox_left = new System.Windows.Forms.NumericTextBox();
			this.textbox_top = new System.Windows.Forms.NumericTextBox();
			this.textbox_right = new System.Windows.Forms.NumericTextBox();
			this.textbox_bottom = new System.Windows.Forms.NumericTextBox();
			this.pagePreview = new PagePreview ();
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
			// 
			// combobox_size
			// 
			this.combobox_size.ItemHeight = 13;
			this.combobox_size.Location = new System.Drawing.Point(84, 22);
			this.combobox_size.Name = "combobox_size";
			this.combobox_size.Size = new System.Drawing.Size(240, 21);
			this.combobox_size.TabIndex = 2;
			this.combobox_size.SelectedIndexChanged += new EventHandler(this.OnPaperSizeChange);
			// 
			// label_source
			// 
			this.label_source.Location = new System.Drawing.Point(13, 58);
			this.label_source.Name = "label_source";
			this.label_source.Size = new System.Drawing.Size(48, 16);
			this.label_source.TabIndex = 1;
			this.label_source.Text = "&Source:";
			// 
			// label_size
			// 
			this.label_size.Location = new System.Drawing.Point(13, 25);
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
			this.radio_landscape.CheckedChanged += new EventHandler(this.OnLandscapeChange);
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
			this.groupbox_margin.Text = LocalizedLengthUnit ();
			// 
			// label_left
			// 
			this.label_left.Location = new System.Drawing.Point(11, 25);
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
			this.button_ok.Click += new EventHandler (OnClickOkButton);
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
			this.button_printer.Click += new EventHandler (OnClickPrinterButton);
			// 
			// label_top
			// 
			this.label_top.Location = new System.Drawing.Point(11, 57);
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
			this.textbox_left.TextChanged +=new EventHandler(OnMarginChange);
			// 
			// textbox_top
			//
			this.textbox_top.Location = new System.Drawing.Point(57, 54);
			this.textbox_top.Name = "textbox_top";
			this.textbox_top.Size = new System.Drawing.Size(48, 20);
			this.textbox_top.TabIndex = 5;
			this.textbox_top.TextChanged +=new EventHandler(OnMarginChange);
			// 
			// textbox_right
			// 
			this.textbox_right.Location = new System.Drawing.Point(171, 21);
			this.textbox_right.Name = "textbox_right";
			this.textbox_right.Size = new System.Drawing.Size(48, 20);
			this.textbox_right.TabIndex = 6;
			this.textbox_right.TextChanged +=new EventHandler(OnMarginChange);
			// 
			// textbox_bottom
			// 
			this.textbox_bottom.Location = new System.Drawing.Point(171, 54);
			this.textbox_bottom.Name = "textbox_bottom";
			this.textbox_bottom.Size = new System.Drawing.Size(48, 20);
			this.textbox_bottom.TabIndex = 7;
			this.textbox_bottom.TextChanged +=new EventHandler(OnMarginChange);
			// 
			// pagePreview
			// 
			this.pagePreview.Location = new System.Drawing.Point (130, 10);
			this.pagePreview.Name = "pagePreview";
			this.pagePreview.Size = new System.Drawing.Size (150, 150);
			this.pagePreview.TabIndex = 6;
			// 
			// Form3
			// 
			form.AcceptButton = this.button_ok;
			form.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			form.CancelButton = this.button_cancel;
			form.ClientSize = new System.Drawing.Size(360, 390);
			form.Controls.Add (this.pagePreview);
			form.Controls.Add (this.button_printer);
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

		static bool UseYardPound {
			get {
				var current = RegionInfo.CurrentRegion;
				if (current == null)
					return true;
				return !current.IsMetric;
			}
		}

		// .Net uses PrinterSettings property if it is not null.
		// Otherwise, it uses PageSettings.PrinterSettings to set values.
		// We use this property internally to automatically select the available one.
		PrinterSettings InternalPrinterSettings {
			get {
				return (printer_settings == null ? page_settings.PrinterSettings :
						printer_settings);
			}
		}

		private double ToLocalizedLength (int marginsUnit)
		{
			return UseYardPound ?
				PrinterUnitConvert.Convert (marginsUnit, PrinterUnit.ThousandthsOfAnInch, PrinterUnit.Display) :
				PrinterUnitConvert.Convert (marginsUnit, PrinterUnit.ThousandthsOfAnInch, PrinterUnit.TenthsOfAMillimeter);
		}

		private int FromLocalizedLength (double marginsUnit)
		{
			return  (int)(UseYardPound ?
				PrinterUnitConvert.Convert (marginsUnit, PrinterUnit.Display, PrinterUnit.ThousandthsOfAnInch) :
				PrinterUnitConvert.Convert (marginsUnit, PrinterUnit.TenthsOfAMillimeter, PrinterUnit.ThousandthsOfAnInch));
		}

		private string LocalizedLengthUnit ()
		{
			return UseYardPound ? "Margins (inches)" : "Margins (millimeters)";
		}
		
		private void SetPrinterDetails ()
		{
			if (PageSettings == null)
				throw new ArgumentException ("PageSettings");

			combobox_size.Items.Clear ();
			foreach (PaperSize paper_size in InternalPrinterSettings.PaperSizes)
				combobox_size.Items.Add (paper_size.PaperName);
			combobox_size.SelectedItem = page_settings.PaperSize.PaperName;
			
			combobox_source.Items.Clear ();
			foreach (PaperSource paper_source in InternalPrinterSettings.PaperSources)
				combobox_source.Items.Add (paper_source.SourceName);
			combobox_source.SelectedItem = page_settings.PaperSource.SourceName;
			
			if (PageSettings.Landscape)
				radio_landscape.Checked = true;
			else
				radio_portrait.Checked = true;

			if (ShowHelp)
				ShowHelpButton ();

			Margins page_margins = PageSettings.Margins;
			Margins min_margins = MinMargins;

			// Update margin data
			textbox_top.Text = ToLocalizedLength (page_margins.Top).ToString ();
			textbox_bottom.Text = ToLocalizedLength (page_margins.Bottom).ToString ();
			textbox_left.Text = ToLocalizedLength (page_margins.Left).ToString ();
			textbox_right.Text = ToLocalizedLength (page_margins.Right).ToString ();
			textbox_top.Min = ToLocalizedLength (min_margins.Top);
			textbox_bottom.Min = ToLocalizedLength (min_margins.Bottom);
			textbox_left.Min = ToLocalizedLength (min_margins.Left);
			textbox_right.Min = ToLocalizedLength (min_margins.Right);

			button_printer.Enabled = AllowPrinter && PrinterSettings != null;
			groupbox_orientation.Enabled = AllowOrientation;
			groupbox_paper.Enabled = AllowPaper;
			groupbox_margin.Enabled = AllowMargins;

			pagePreview.Setup (PageSettings);
		}

		private void OnClickOkButton (object sender, EventArgs e)
		{
			if (combobox_size.SelectedItem != null) {
				foreach (PaperSize paper_size in InternalPrinterSettings.PaperSizes) {
					if (paper_size.PaperName == (string) combobox_size.SelectedItem) {
						PageSettings.PaperSize = paper_size;
						break;
					}
				}
			}
			
			if (combobox_source.SelectedItem != null) {
				foreach (PaperSource paper_source in InternalPrinterSettings.PaperSources) {
					if (paper_source.SourceName == (string) combobox_source.SelectedItem) {
						PageSettings.PaperSource = paper_source;
						break;
					}
				}
			}

			Margins margins = new Margins ();
			margins.Top = FromLocalizedLength (textbox_top.Value);
			margins.Bottom = FromLocalizedLength (textbox_bottom.Value);
			margins.Left = FromLocalizedLength (textbox_left.Value);
			margins.Right = FromLocalizedLength (textbox_right.Value);
			PageSettings.Margins = margins;

			PageSettings.Landscape = radio_landscape.Checked;
			form.DialogResult = DialogResult.OK;
		}

		void ShowHelpButton ()
		{
			if (button_help == null) {
				button_help = new Button ();
				button_help.Location = new System.Drawing.Point (12, 358);
				button_help.Name = "button_help";
				button_help.Size = new System.Drawing.Size (72, 23);
				button_help.Text = "&Help";
				form.Controls.Add (button_help);
			}

			button_help.Visible = show_help;
		}

		void OnClickPrinterButton (object sender, EventArgs args)
		{
			PrinterForm printer_helper_form = new PrinterForm (this);

			printer_helper_form.UpdateValues ();

			// Here update values for PrinterSettings
			if (printer_helper_form.ShowDialog () == DialogResult.OK)
				if (printer_helper_form.SelectedPrinter != PrinterSettings.PrinterName)
					PrinterSettings.PrinterName = printer_helper_form.SelectedPrinter;

			PageSettings = PrinterSettings.DefaultPageSettings;
			SetPrinterDetails ();
			button_ok.Select ();
			printer_helper_form.Dispose ();
		}

		void OnPaperSizeChange (object sender, EventArgs e)
		{
			if (combobox_size.SelectedItem != null) {
				foreach (PaperSize paper_size in InternalPrinterSettings.PaperSizes) {
					if (paper_size.PaperName == (string) combobox_size.SelectedItem) {
						pagePreview.SetSize (paper_size.Width, paper_size.Height);
						break;
					}
				}
			}
		}

		void OnMarginChange (object sender, EventArgs e)
		{
			pagePreview.SetMargins (
				FromLocalizedLength (textbox_left.Value),
				FromLocalizedLength (textbox_right.Value),
				FromLocalizedLength (textbox_top.Value),
				FromLocalizedLength (textbox_bottom.Value)
			);
		}

		void OnLandscapeChange (object sender, EventArgs e)
		{
			pagePreview.Landscape = radio_landscape.Checked;
		}
		#endregion // Private Helper

		class PrinterForm : Form
		{
			private System.Windows.Forms.GroupBox groupbox_printer;
			private System.Windows.Forms.ComboBox combobox_printers;
			private System.Windows.Forms.Label label_name;
			private System.Windows.Forms.Label label_status;
			private System.Windows.Forms.Button button_properties;
			private System.Windows.Forms.Button button_network;
			private System.Windows.Forms.Button button_cancel;
			private System.Windows.Forms.Button button_ok;
			private System.Windows.Forms.Label label_status_text;
			private System.Windows.Forms.Label label_type;
			private System.Windows.Forms.Label label_where;
			private System.Windows.Forms.Label label_where_text;
			private System.Windows.Forms.Label label_type_text;
			private System.Windows.Forms.Label label_comment;
			private System.Windows.Forms.Label label_comment_text;
			PageSetupDialog page_setup_dialog;

			public PrinterForm (PageSetupDialog page_setup_dialog)
			{
				InitializeComponent();
				this.page_setup_dialog = page_setup_dialog;
			}

			public string SelectedPrinter {
				get {
					return (string) combobox_printers.SelectedItem;
				}
				set {
					combobox_printers.SelectedItem = value;
					label_type_text.Text = value;
				}
			}

			public void UpdateValues ()
			{
				combobox_printers.Items.Clear ();
				foreach (string printer_name in PrinterSettings.InstalledPrinters)
					combobox_printers.Items.Add (printer_name);

				// Select the printer indicated by PageSetupDialog.PrinterSettings
				SelectedPrinter = page_setup_dialog.PrinterSettings.PrinterName;

				button_network.Enabled = page_setup_dialog.ShowNetwork;
			}

#region Windows Form Designer generated code
			private void InitializeComponent()
			{
				this.groupbox_printer = new System.Windows.Forms.GroupBox();
				this.combobox_printers = new System.Windows.Forms.ComboBox();
				this.button_network = new System.Windows.Forms.Button();
				this.button_cancel = new System.Windows.Forms.Button();
				this.button_ok = new System.Windows.Forms.Button();
				this.label_name = new System.Windows.Forms.Label();
				this.label_status = new System.Windows.Forms.Label();
				this.label_status_text = new System.Windows.Forms.Label();
				this.label_type = new System.Windows.Forms.Label();
				this.label_type_text = new System.Windows.Forms.Label();
				this.label_where = new System.Windows.Forms.Label();
				this.label_comment = new System.Windows.Forms.Label();
				this.label_where_text = new System.Windows.Forms.Label();
				this.label_comment_text = new System.Windows.Forms.Label();
				this.button_properties = new System.Windows.Forms.Button();
				this.groupbox_printer.SuspendLayout();
				this.SuspendLayout();
				// 
				// groupbox_printer
				// 
				this.groupbox_printer.Controls.AddRange(new System.Windows.Forms.Control[] {
						this.button_properties,
						this.label_comment_text,
						this.label_where_text,
						this.label_comment,
						this.label_where,
						this.label_type_text,
						this.label_type,
						this.label_status_text,
						this.label_status,
						this.label_name,
						this.combobox_printers});
				this.groupbox_printer.Location = new System.Drawing.Point(12, 8);
				this.groupbox_printer.Name = "groupbox_printer";
				this.groupbox_printer.Size = new System.Drawing.Size(438, 136);
				this.groupbox_printer.Text = "Printer";
				// 
				// combobox_printers
				// 
				this.combobox_printers.Location = new System.Drawing.Point(64, 24);
				this.combobox_printers.Name = "combobox_printers";
				this.combobox_printers.SelectedValueChanged += new EventHandler (OnSelectedValueChangedPrinters);
				this.combobox_printers.Size = new System.Drawing.Size(232, 21);
				this.combobox_printers.TabIndex = 1;
				// 
				// button_network
				// 
				this.button_network.Location = new System.Drawing.Point(16, 160);
				this.button_network.Name = "button_network";
				this.button_network.Size = new System.Drawing.Size(68, 22);
				this.button_network.TabIndex = 5;
				this.button_network.Text = "Network...";
				// 
				// button_cancel
				// 
				this.button_cancel.DialogResult = DialogResult.Cancel;
				this.button_cancel.Location = new System.Drawing.Point(376, 160);
				this.button_cancel.Name = "button_cancel";
				this.button_cancel.Size = new System.Drawing.Size(68, 22);
				this.button_cancel.TabIndex = 4;
				this.button_cancel.Text = "Cancel";
				// 
				// button_ok
				// 
				this.button_ok.DialogResult = DialogResult.OK;
				this.button_ok.Location = new System.Drawing.Point(300, 160);
				this.button_ok.Name = "button_ok";
				this.button_ok.Size = new System.Drawing.Size(68, 22);
				this.button_ok.TabIndex = 3;
				this.button_ok.Text = "OK";
				// 
				// label_name
				// 
				this.label_name.Location = new System.Drawing.Point(12, 28);
				this.label_name.Name = "label_name";
				this.label_name.Size = new System.Drawing.Size(48, 20);
				this.label_name.Text = "Name:";
				// 
				// label_status
				// 
				this.label_status.Location = new System.Drawing.Point(6, 52);
				this.label_status.Name = "label_status";
				this.label_status.Size = new System.Drawing.Size(58, 14);
				this.label_status.Text = "Status:";
				// 
				// label_status_text
				// 
				this.label_status_text.Location = new System.Drawing.Point(64, 52);
				this.label_status_text.Name = "label_status_text";
				this.label_status_text.Size = new System.Drawing.Size(64, 14);
				this.label_status_text.Text = String.Empty;
				// 
				// label_type
				// 
				this.label_type.Location = new System.Drawing.Point(6, 72);
				this.label_type.Name = "label_type";
				this.label_type.Size = new System.Drawing.Size(58, 14);
				this.label_type.Text = "Type:";
				// 
				// label_type_text
				// 
				this.label_type_text.Location = new System.Drawing.Point(64, 72);
				this.label_type_text.Name = "label_type_text";
				this.label_type_text.Size = new System.Drawing.Size(232, 14);
				this.label_type_text.TabIndex = 5;
				this.label_type_text.Text = String.Empty;
				// 
				// label_where
				// 
				this.label_where.Location = new System.Drawing.Point(6, 92);
				this.label_where.Name = "label_where";
				this.label_where.Size = new System.Drawing.Size(58, 16);
				this.label_where.TabIndex = 6;
				this.label_where.Text = "Where:";
				// 
				// label_comment
				// 
				this.label_comment.Location = new System.Drawing.Point(6, 112);
				this.label_comment.Name = "label_comment";
				this.label_comment.Size = new System.Drawing.Size(56, 16);
				this.label_comment.Text = "Comment:";
				// 
				// label_where_text
				// 
				this.label_where_text.Location = new System.Drawing.Point(64, 92);
				this.label_where_text.Name = "label_where_text";
				this.label_where_text.Size = new System.Drawing.Size(232, 16);
				this.label_where_text.Text = String.Empty;
				// 
				// label_comment_text
				// 
				this.label_comment_text.Location = new System.Drawing.Point(64, 112);
				this.label_comment_text.Name = "label_comment_text";
				this.label_comment_text.Size = new System.Drawing.Size(232, 16);
				this.label_comment_text.Text = String.Empty;
				// 
				// button_properties
				// 
				this.button_properties.Location = new System.Drawing.Point(308, 22);
				this.button_properties.Name = "button_properties";
				this.button_properties.Size = new System.Drawing.Size(92, 22);
				this.button_properties.TabIndex = 2;
				this.button_properties.Text = "Properties...";
				// 
				// PrinterForm
				// 
				this.AllowDrop = true;
				this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
				this.AcceptButton = button_ok;
				this.CancelButton = button_cancel;
				this.ClientSize = new System.Drawing.Size(456, 194);
				this.Controls.AddRange(new System.Windows.Forms.Control[] {
						this.button_ok,
						this.button_cancel,
						this.button_network,
						this.groupbox_printer});
				this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
				this.HelpButton = true;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.Name = "PrinterForm";
				this.ShowInTaskbar = false;
				this.Text = "Configure page";
				this.groupbox_printer.ResumeLayout(false);
				this.ResumeLayout(false);
			}
#endregion
			void OnSelectedValueChangedPrinters (object sender, EventArgs args)
			{
				SelectedPrinter = (string) combobox_printers.SelectedItem;
			}
		}

		private class PagePreview : UserControl
		{
			int width;
			int height;

			int marginBottom;
			int marginTop;
			int marginLeft;
			int marginRight;
			bool landscape;

			bool loaded = false;

			StringBuilder sb;
			float displayHeight;
			new Font font;

			public bool Landscape {
				get { return landscape; }
				set {
					if (landscape != value) {
						landscape = value;
						Invalidate ();
					}
				}
			}

			public new float Height {
				get { return displayHeight; }
				set {
					if (displayHeight != value) {
						displayHeight = value; 
						Invalidate ();
					}
				}
			}

			public PagePreview ()
			{
				sb = new StringBuilder ();
				for (int i = 0; i < 4; i++) {
					sb.Append ("blabla piu piublapiu haha lai dlais dhlçai shd ");
					sb.Append ("çoasd çlaj sdç\r\n lajsd lçaisdj lçillaisd lahs dli");
					sb.Append ("laksjd liasjdliasdj blabla piu piublapiu haha ");
					sb.Append ("lai dlais dhlçai shd çoasd çlaj sdç lajsd lçaisdj");
					sb.Append (" lçillaisd lahs dli laksjd liasjdliasdj\r\n\r\n");
				}
				
				font = new Font (FontFamily.GenericSansSerif, 4);
				this.displayHeight = 130;
			}

			public void SetSize (int width, int height)
			{
				this.width = width;
				this.height = height;
				this.Invalidate ();
			}

			public void SetMargins (int left, int right, int top, int bottom)
			{
				this.marginBottom = bottom;
				this.marginTop = top;
				this.marginLeft = left;
				this.marginRight = right;
				this.Invalidate ();
			}


			public void Setup (PageSettings pageSettings)
			{
				this.width = pageSettings.PaperSize.Width;
				this.height = pageSettings.PaperSize.Height;

				Margins margins = pageSettings.Margins;
				this.marginBottom = margins.Bottom;
				this.marginTop = margins.Top;
				this.marginLeft = margins.Left;
				this.marginRight = margins.Right;
				this.landscape = pageSettings.Landscape;
				this.loaded = true;
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				if (!loaded) {
					base.OnPaint (e);
					return;
				}

				Graphics g = e.Graphics;

				float h = displayHeight;
				float w = (width * displayHeight) / height;
				float top = (marginTop * displayHeight) / height;
				float left = (marginLeft * displayHeight) / height;
				float bottom = (marginBottom * displayHeight) / height;
				float right = (marginRight * displayHeight) / height;

				if (landscape) {
					float a = w;
					w = h;
					h = a;
					a = right;
					right = top;
					top = left;
					left = bottom;
					bottom = a;
				}
				
				g.FillRectangle (SystemBrushes.ControlDark, 4, 4, w + 4, h + 4);
				g.FillRectangle (Brushes.White, 0, 0, w, h);

				RectangleF outerrect = new RectangleF (0, 0, w, h);
				RectangleF innerrect = new RectangleF (left, top,
														w - left - right,
														h - top - bottom);

				ControlPaint.DrawBorder (g, outerrect, Color.Black, ButtonBorderStyle.Solid);
				ControlPaint.DrawBorder (g, innerrect, SystemColors.ControlDark, ButtonBorderStyle.Dashed);

				g.DrawString (sb.ToString (), font, Brushes.Black,
								new RectangleF (innerrect.X + 2,
											innerrect.Y + 2,
											innerrect.Width - 4,
											innerrect.Height - 4));


				base.OnPaint (e);
			}
		}
	}

}
