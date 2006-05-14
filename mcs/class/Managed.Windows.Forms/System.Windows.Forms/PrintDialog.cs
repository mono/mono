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
// Copyright (c) 2005-2006 Novell, Inc.
//
// Authors:
//	Someone
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//	Jordi Mas i Hernandez (jordimash@gmail.com)
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

namespace System.Windows.Forms
{
	[DefaultProperty("Document")]
	public sealed class PrintDialog : CommonDialog {
		PrintDocument document;
		PrinterSettings printer_settings;
		bool allow_current_page;
		bool allow_print_to_file;
		bool allow_selection;
		bool allow_some_pages;
		bool show_help;
		bool show_network;
		bool print_to_file;
		PrinterSettings current_settings;

		private Button cancel_button;
		private Button accept_button;
		private Button help_button;
		private ComboBox printer_combo;
		private RadioButton radio_all;
		private RadioButton radio_pages;
		private RadioButton radio_sel;
		private PrinterSettings.StringCollection installed_printers;
		private PrinterSettings default_printer_settings;
		private TextBox txtFrom;
		private TextBox txtTo;
		private Label labelTo;
		private Label labelFrom;
		private CheckBox chkbox_print;
		private NumericUpDown updown_copies;
		private CheckBox chkbox_collate;

		public PrintDialog ()
		{
			help_button = null;
			installed_printers = System.Drawing.Printing.PrinterSettings.InstalledPrinters;

			form.Text = "Print";
			CreateFormControls ();
			Reset ();
		}

		public override void Reset ()
		{
			current_settings = null;
			AllowPrintToFile = true;
			AllowSelection = false;
			AllowSomePages = false;
			PrintToFile = false;
			ShowHelp = false;
			ShowNetwork = true;
		}

#if NET_2_0
		public bool AllowCurrentPage {
			get {
				return allow_current_page;
			}

			set {
				allow_current_page = value;
				radio_pages.Enabled = value;
			}
		}
#endif

		[DefaultValue(true)]
		public bool AllowPrintToFile {
			get {
				return allow_print_to_file;
			}

			set {
				allow_print_to_file = value;
				chkbox_print.Enabled = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSelection {
			get {
				return allow_selection;
			}

			set {
				allow_selection = value;
				radio_sel.Enabled = value;
			}
		}

		[DefaultValue(false)]
		public bool AllowSomePages {
			get {
				return allow_some_pages;
			}

			set {
				allow_some_pages = value;
				radio_pages.Enabled = value;
				txtFrom.Enabled = value;
				txtTo.Enabled = value;
				labelTo.Enabled = value;
				labelFrom.Enabled = value;

				if (current_settings != null) {
					txtFrom.Text = current_settings.FromPage.ToString ();
					txtTo.Text = current_settings.ToPage.ToString ();
				}
			}
		}

		[DefaultValue(null)]
		public PrintDocument Document {
			get {
				return document;
			}

			set {
				if (value == null)
					return;

				document = value;
				current_settings = value.PrinterSettings;
				printer_settings  = null;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PrinterSettings PrinterSettings {
			get {
				return printer_settings;
			}

			set {
				current_settings = printer_settings = value;
				document = null;
			}
		}

		[DefaultValue(false)]
		public bool PrintToFile {
			get {
				return print_to_file;
			}

			set {
				print_to_file = value;
			}
		}

		[DefaultValue(true)]
		public bool ShowNetwork {
			get {
				return show_network;
			}

			set {
				show_network = value;
			}
		}

		[DefaultValue(false)]
		public bool ShowHelp {
			get {
				return show_help;
			}

			set {
				show_help = value;
				ShowHelpButton ();
			}
		}

		protected override bool RunDialog (IntPtr hwnd)
		{
			if (Document == null && PrinterSettings == null)
				throw new ArgumentException ("PrintDialog needs a Docouement or PrinterSettings object to display");

			if (allow_some_pages && current_settings.FromPage > current_settings.ToPage)
				throw new ArgumentException ("FromPage out of range");

			if (allow_some_pages && current_settings != null) {
				txtFrom.Text = current_settings.FromPage.ToString ();
				txtTo.Text = current_settings.ToPage.ToString ();
			}
			
			updown_copies.Value = current_settings.Copies;
			chkbox_collate.Enabled = (updown_copies.Value > 0) ? true : false;

			if (show_help) {
				ShowHelpButton ();
			}

			return true;
		}

		private void OnClickCancelButton (object sender, EventArgs e)
		{
			form.DialogResult = DialogResult.Cancel;
		}

		private void OnClickOkButton (object sender, EventArgs e)
		{
			int from, to;

			try {
				from = Int32.Parse (txtFrom.Text);
				to = Int32.Parse (txtTo.Text);
			}
	
			catch {
				MessageBox.Show ("From/To values should be numeric", "Print",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
				return;
			}

			if (allow_some_pages) {
				if (from > to) {
					MessageBox.Show ("From value cannot be greater than To value", "Print",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (to < current_settings.MinimumPage || to > current_settings.MaximumPage) {
					MessageBox.Show ("To value is not within the page range\n" +
						"Enter a number between " + current_settings.MinimumPage +
						" and " + current_settings.MaximumPage, "Print",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}

				if (from < current_settings.MinimumPage || from > current_settings.MaximumPage) {
					txtTo.Focus ();
					MessageBox.Show ("From value is not within the page range\n" +
						"Enter a number between " + current_settings.MinimumPage +
						" and " + current_settings.MaximumPage, "Print",
						MessageBoxButtons.OK, MessageBoxIcon.Warning);
					return;
				}
			}

			if (radio_all.Checked == true) {
				current_settings.PrintRange = PrintRange.AllPages;
			} else {
				if (radio_pages.Checked == true) {
					current_settings.PrintRange = PrintRange.SomePages;
				} else {
					if (radio_sel.Checked == true) {
						current_settings.PrintRange = PrintRange.Selection;
					}
				}
			}
			
			current_settings.Copies = (short) updown_copies.Value;
			current_settings.FromPage = from;
			current_settings.ToPage = to;
			current_settings.Collate = chkbox_collate.Checked;

			if (allow_print_to_file) {
				current_settings.PrintToFile = chkbox_print.Checked;
			}

			form.DialogResult = DialogResult.OK;

			if (printer_combo.SelectedItem != null)
				current_settings.PrinterName = (string) printer_combo.SelectedItem;
		}

		private void ShowHelpButton ()
		{
			if (help_button == null) {
				help_button = new Button ();

				help_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
				help_button.FlatStyle = FlatStyle.System;
				help_button.Location = new Point (20, 270);
				help_button.Text = "&Help";
				help_button.FlatStyle = FlatStyle.System;
				form.Controls.Add (help_button);
			}

			help_button.Visible = show_help;
		}		
		
		private void OnUpDownValueChanged (object sender, System.EventArgs e)
		{
			chkbox_collate.Enabled = (updown_copies.Value > 0) ? true : false;
		}

		private void CreateFormControls ()
		{
			form.SuspendLayout ();

			// Accept button
			accept_button = new Button ();
			form.AcceptButton = accept_button;
			accept_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			accept_button.FlatStyle = FlatStyle.System;
			accept_button.Location = new Point (265, 270);
			accept_button.Text = "OK";
			accept_button.FlatStyle = FlatStyle.System;
			accept_button.Click += new EventHandler (OnClickOkButton);

			// Cancel button
			cancel_button = new Button ();
			form.CancelButton = cancel_button;
			cancel_button.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Right)));
			cancel_button.FlatStyle = FlatStyle.System;
			cancel_button.Location = new Point (350, 270);
			cancel_button.Text = "Cancel";
			cancel_button.FlatStyle = FlatStyle.System;
			cancel_button.Click += new EventHandler (OnClickCancelButton);

			// Static controls
			Label label = new Label ();
			label.AutoSize = true;
			label.Text = "&Name:";
			label.Location = new Point (20, 43);
			form.Controls.Add (label);

			label = new Label ();
			label.Text = "&Status:";
			label.AutoSize = true;
			label.Location = new Point (20, 70);
			form.Controls.Add (label);

			GroupBox group_box_prn = new GroupBox ();
			group_box_prn.Location = new Point (10, 8);
			group_box_prn.Text = "Printer";
			group_box_prn.Size = new Size (400, 140);

			GroupBox group_box_range = new GroupBox ();
			group_box_range.Location = new Point (10, 155);
			group_box_range.Text = "Print range";
			group_box_range.Size = new Size (220, 100);

			radio_all = new RadioButton ();
			radio_all.Location = new Point (20, 170);
			radio_all.Text = "&All";
			radio_all.Checked = true;
			form.Controls.Add (radio_all);

			radio_pages = new RadioButton ();
			radio_pages.Location = new Point (20, 192);
			radio_pages.Text = "Pa&ges";
			radio_pages.Width = 60;
			form.Controls.Add (radio_pages);

			radio_sel = new RadioButton ();
			radio_sel.Location = new Point (20, 215);
			radio_sel.Text = "&Selection";
			form.Controls.Add (radio_sel);

			labelFrom = new Label ();
			labelFrom.Text = "&from:";
			labelFrom.AutoSize = true;
			labelFrom.Location = new Point (80, 198);
			form.Controls.Add (labelFrom);

			txtFrom = new TextBox ();
			txtFrom.Location = new Point (115, 196);
			txtFrom.Width = 40;
			form.Controls.Add (txtFrom);

			labelTo = new Label ();
			labelTo.Text = "&to:";
			labelTo.AutoSize = true;
			labelTo.Location = new Point (160, 198);
			form.Controls.Add (labelTo);

			txtTo = new TextBox ();
			txtTo.Location = new Point (180, 196);
			txtTo.Width = 40;
			form.Controls.Add (txtTo);

			chkbox_print = new CheckBox ();
			chkbox_print.Location = new Point (305, 115);
			chkbox_print.Text = "Print to fil&e";
			form.Controls.Add (chkbox_print);

			label = new Label ();
			label.Text = "Number of &copies:";
			label.AutoSize = true;
			label.Location = new Point (255, 177);
			form.Controls.Add (label);

			updown_copies = new NumericUpDown ();
			updown_copies.Location = new Point (360, 175);
			form.Controls.Add (updown_copies);
			updown_copies.ValueChanged += new System.EventHandler (OnUpDownValueChanged);
			updown_copies.Size = new System.Drawing.Size (40, 20);
			
			chkbox_collate = new CheckBox ();
			chkbox_collate.Location = new Point (320, 210);
			chkbox_collate.Text = "C&ollate";
			chkbox_collate.Width = 80;
			form.Controls.Add (chkbox_collate);

			GroupBox group_box_copies = new GroupBox ();
			group_box_copies.Location = new Point (245, 155);
			group_box_copies.Text = "Copies";
			group_box_copies.Size = new Size (165, 100);
			form.Controls.Add (group_box_copies);



			// Printer combo
			printer_combo = new ComboBox ();
			printer_combo.DropDownStyle = ComboBoxStyle.DropDownList;
			printer_combo.Location = new Point (80, 42);
			printer_combo.Width = 220;

			default_printer_settings = new PrinterSettings ();
			for (int i = 0; i < installed_printers.Count; i++) {
				printer_combo.Items.Add (installed_printers[i]);
				if (installed_printers[i] == default_printer_settings.PrinterName)
					printer_combo.SelectedItem = installed_printers[i];
			}

			form.Size =  new Size (438, 327); // 384
			form.FormBorderStyle = FormBorderStyle.FixedDialog;
			form.MaximizeBox = false;
			form.Controls.Add (accept_button);
			form.Controls.Add (cancel_button);
			form.Controls.Add (printer_combo);
			form.Controls.Add (group_box_prn);
			form.Controls.Add (group_box_range);
			form.ResumeLayout (false);
		}		
	}
}
