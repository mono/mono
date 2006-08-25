//
// EventEntryProperties.cs
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
// 
//
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

using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace Mono.Tools.EventViewer
{
	public class EventEntryProperties : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage Event;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button applyButton;
		private System.Windows.Forms.Label dateGeneratedLabel;
		private System.Windows.Forms.Label timeGeneratedLabel;
		private System.Windows.Forms.Label entryTypeLabel;
		private System.Windows.Forms.Label userNameLabel;
		private System.Windows.Forms.Label machineNameLabel;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label sourceLabel;
		private System.Windows.Forms.Label categoryLabel;
		private System.Windows.Forms.Label instanceIdLabel;
		private System.Windows.Forms.Label descriptionLabel;
		private System.Windows.Forms.TextBox dateGeneratedText;
		private System.Windows.Forms.TextBox timeGeneratedText;
		private System.Windows.Forms.TextBox entryTypeText;
		private System.Windows.Forms.TextBox userNameText;
		private System.Windows.Forms.TextBox machineNameText;
		private System.Windows.Forms.TextBox sourceText;
		private System.Windows.Forms.TextBox categoryText;
		private System.Windows.Forms.TextBox instanceIdText;
		private System.Windows.Forms.Label dataLabel;
		private System.Windows.Forms.RadioButton bytesRadioButton;
		private System.Windows.Forms.RadioButton wordsRadioButton;

		private EventEntryView _entry;
		private System.Windows.Forms.RichTextBox dataText;
		private System.Windows.Forms.RichTextBox descriptionText;
		private System.Windows.Forms.Button cancelButton;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public EventEntryProperties ()
		{
			InitializeComponent ();
		}

		public void DisplayEventEntry (EventEntryView entry)
		{
			_entry = entry;

			dateGeneratedText.Text = entry.DateGenerated;
			timeGeneratedText.Text = entry.TimeGenerated;
			entryTypeText.Text = entry.EntryType;
			userNameText.Text = entry.UserName;
			machineNameText.Text = entry.MachineName;
			sourceText.Text = entry.Source;
			categoryText.Text = entry.Category;
			instanceIdText.Text = entry.InstanceId;
			descriptionText.Text = entry.Message;

			bool hasData = (entry.Data != null && entry.Data.Length > 0);
			dataLabel.Enabled = hasData;
			bytesRadioButton.Enabled = hasData;
			wordsRadioButton.Enabled = hasData;
			dataText.Enabled = hasData;

			if (hasData) {
				if (bytesRadioButton.Checked)
					DisplayDataBytes ();
				else
					DisplayDataWords ();
			} else {
				dataText.Text = string.Empty;
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose ();
				}
			}
			base.Dispose (disposing);
		}

		private void InitializeComponent ()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl ();
			this.Event = new System.Windows.Forms.TabPage ();
			this.descriptionText = new System.Windows.Forms.RichTextBox ();
			this.dataText = new System.Windows.Forms.RichTextBox ();
			this.wordsRadioButton = new System.Windows.Forms.RadioButton ();
			this.bytesRadioButton = new System.Windows.Forms.RadioButton ();
			this.dataLabel = new System.Windows.Forms.Label ();
			this.instanceIdText = new System.Windows.Forms.TextBox ();
			this.categoryText = new System.Windows.Forms.TextBox ();
			this.sourceText = new System.Windows.Forms.TextBox ();
			this.machineNameText = new System.Windows.Forms.TextBox ();
			this.userNameText = new System.Windows.Forms.TextBox ();
			this.entryTypeText = new System.Windows.Forms.TextBox ();
			this.timeGeneratedText = new System.Windows.Forms.TextBox ();
			this.dateGeneratedText = new System.Windows.Forms.TextBox ();
			this.descriptionLabel = new System.Windows.Forms.Label ();
			this.instanceIdLabel = new System.Windows.Forms.Label ();
			this.categoryLabel = new System.Windows.Forms.Label ();
			this.sourceLabel = new System.Windows.Forms.Label ();
			this.button4 = new System.Windows.Forms.Button ();
			this.button3 = new System.Windows.Forms.Button ();
			this.button2 = new System.Windows.Forms.Button ();
			this.machineNameLabel = new System.Windows.Forms.Label ();
			this.userNameLabel = new System.Windows.Forms.Label ();
			this.entryTypeLabel = new System.Windows.Forms.Label ();
			this.timeGeneratedLabel = new System.Windows.Forms.Label ();
			this.dateGeneratedLabel = new System.Windows.Forms.Label ();
			this.okButton = new System.Windows.Forms.Button ();
			this.cancelButton = new System.Windows.Forms.Button ();
			this.applyButton = new System.Windows.Forms.Button ();
			this.tabControl1.SuspendLayout ();
			this.Event.SuspendLayout ();
			this.SuspendLayout ();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add (this.Event);
			this.tabControl1.HotTrack = true;
			this.tabControl1.Location = new System.Drawing.Point (4, 8);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size (384, 368);
			this.tabControl1.TabIndex = 0;
			// 
			// Event
			// 
			this.Event.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.Event.Controls.Add (this.descriptionText);
			this.Event.Controls.Add (this.dataText);
			this.Event.Controls.Add (this.wordsRadioButton);
			this.Event.Controls.Add (this.bytesRadioButton);
			this.Event.Controls.Add (this.dataLabel);
			this.Event.Controls.Add (this.instanceIdText);
			this.Event.Controls.Add (this.categoryText);
			this.Event.Controls.Add (this.sourceText);
			this.Event.Controls.Add (this.machineNameText);
			this.Event.Controls.Add (this.userNameText);
			this.Event.Controls.Add (this.entryTypeText);
			this.Event.Controls.Add (this.timeGeneratedText);
			this.Event.Controls.Add (this.dateGeneratedText);
			this.Event.Controls.Add (this.descriptionLabel);
			this.Event.Controls.Add (this.instanceIdLabel);
			this.Event.Controls.Add (this.categoryLabel);
			this.Event.Controls.Add (this.sourceLabel);
			this.Event.Controls.Add (this.button4);
			this.Event.Controls.Add (this.button3);
			this.Event.Controls.Add (this.button2);
			this.Event.Controls.Add (this.machineNameLabel);
			this.Event.Controls.Add (this.userNameLabel);
			this.Event.Controls.Add (this.entryTypeLabel);
			this.Event.Controls.Add (this.timeGeneratedLabel);
			this.Event.Controls.Add (this.dateGeneratedLabel);
			this.Event.ForeColor = System.Drawing.SystemColors.ControlText;
			this.Event.Location = new System.Drawing.Point (4, 22);
			this.Event.Name = "Event";
			this.Event.Size = new System.Drawing.Size (376, 342);
			this.Event.TabIndex = 0;
			this.Event.Text = "Event";
			// 
			// descriptionText
			// 
			this.descriptionText.BackColor = System.Drawing.SystemColors.Control;
			this.descriptionText.Location = new System.Drawing.Point (8, 128);
			this.descriptionText.Name = "descriptionText";
			this.descriptionText.ReadOnly = true;
			this.descriptionText.Size = new System.Drawing.Size (360, 120);
			this.descriptionText.TabIndex = 34;
			this.descriptionText.Text = "<Description>";
			// 
			// dataText
			// 
			this.dataText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.dataText.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dataText.DetectUrls = false;
			this.dataText.Font = new System.Drawing.Font ("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte) (0)));
			this.dataText.Location = new System.Drawing.Point (8, 272);
			this.dataText.Name = "dataText";
			this.dataText.ReadOnly = true;
			this.dataText.Size = new System.Drawing.Size (360, 56);
			this.dataText.TabIndex = 33;
			this.dataText.Text = "<Data>";
			// 
			// wordsRadioButton
			// 
			this.wordsRadioButton.Location = new System.Drawing.Point (104, 250);
			this.wordsRadioButton.Name = "wordsRadioButton";
			this.wordsRadioButton.Size = new System.Drawing.Size (64, 24);
			this.wordsRadioButton.TabIndex = 31;
			this.wordsRadioButton.Text = "Words";
			this.wordsRadioButton.CheckedChanged += new System.EventHandler (this.wordsRadioButton_CheckedChanged);
			// 
			// bytesRadioButton
			// 
			this.bytesRadioButton.Checked = true;
			this.bytesRadioButton.Location = new System.Drawing.Point (48, 250);
			this.bytesRadioButton.Name = "bytesRadioButton";
			this.bytesRadioButton.Size = new System.Drawing.Size (56, 24);
			this.bytesRadioButton.TabIndex = 30;
			this.bytesRadioButton.TabStop = true;
			this.bytesRadioButton.Text = "Bytes";
			this.bytesRadioButton.CheckedChanged += new System.EventHandler (this.bytesRadioButton_CheckedChanged);
			// 
			// dataLabel
			// 
			this.dataLabel.AutoSize = true;
			this.dataLabel.Location = new System.Drawing.Point (8, 256);
			this.dataLabel.Name = "dataLabel";
			this.dataLabel.Size = new System.Drawing.Size (31, 16);
			this.dataLabel.TabIndex = 29;
			this.dataLabel.Text = "Data:";
			// 
			// instanceIdText
			// 
			this.instanceIdText.AutoSize = false;
			this.instanceIdText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.instanceIdText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.instanceIdText.Location = new System.Drawing.Point (208, 48);
			this.instanceIdText.Name = "instanceIdText";
			this.instanceIdText.ReadOnly = true;
			this.instanceIdText.Size = new System.Drawing.Size (72, 20);
			this.instanceIdText.TabIndex = 28;
			this.instanceIdText.Text = "<Event ID>";
			this.instanceIdText.WordWrap = false;
			// 
			// categoryText
			// 
			this.categoryText.AutoSize = false;
			this.categoryText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.categoryText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.categoryText.Location = new System.Drawing.Point (208, 28);
			this.categoryText.Name = "categoryText";
			this.categoryText.ReadOnly = true;
			this.categoryText.Size = new System.Drawing.Size (72, 20);
			this.categoryText.TabIndex = 27;
			this.categoryText.Text = "<Category>";
			this.categoryText.WordWrap = false;
			// 
			// sourceText
			// 
			this.sourceText.AutoSize = false;
			this.sourceText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.sourceText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.sourceText.Location = new System.Drawing.Point (208, 8);
			this.sourceText.Name = "sourceText";
			this.sourceText.ReadOnly = true;
			this.sourceText.Size = new System.Drawing.Size (72, 20);
			this.sourceText.TabIndex = 26;
			this.sourceText.Text = "<Source>";
			this.sourceText.WordWrap = false;
			// 
			// machineNameText
			// 
			this.machineNameText.AutoSize = false;
			this.machineNameText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.machineNameText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.machineNameText.Location = new System.Drawing.Point (72, 88);
			this.machineNameText.Name = "machineNameText";
			this.machineNameText.ReadOnly = true;
			this.machineNameText.Size = new System.Drawing.Size (240, 20);
			this.machineNameText.TabIndex = 25;
			this.machineNameText.Text = "<Computer>";
			this.machineNameText.WordWrap = false;
			// 
			// userNameText
			// 
			this.userNameText.AutoSize = false;
			this.userNameText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.userNameText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.userNameText.Location = new System.Drawing.Point (72, 68);
			this.userNameText.Name = "userNameText";
			this.userNameText.ReadOnly = true;
			this.userNameText.Size = new System.Drawing.Size (240, 20);
			this.userNameText.TabIndex = 24;
			this.userNameText.Text = "<User>";
			this.userNameText.WordWrap = false;
			// 
			// entryTypeText
			// 
			this.entryTypeText.AutoSize = false;
			this.entryTypeText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.entryTypeText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.entryTypeText.Location = new System.Drawing.Point (72, 48);
			this.entryTypeText.Name = "entryTypeText";
			this.entryTypeText.ReadOnly = true;
			this.entryTypeText.Size = new System.Drawing.Size (72, 20);
			this.entryTypeText.TabIndex = 23;
			this.entryTypeText.Text = "<Type>";
			this.entryTypeText.WordWrap = false;
			// 
			// timeGeneratedText
			// 
			this.timeGeneratedText.AutoSize = false;
			this.timeGeneratedText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.timeGeneratedText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.timeGeneratedText.Location = new System.Drawing.Point (72, 28);
			this.timeGeneratedText.Name = "timeGeneratedText";
			this.timeGeneratedText.ReadOnly = true;
			this.timeGeneratedText.Size = new System.Drawing.Size (72, 20);
			this.timeGeneratedText.TabIndex = 22;
			this.timeGeneratedText.Text = "<Time>";
			this.timeGeneratedText.WordWrap = false;
			// 
			// dateGeneratedText
			// 
			this.dateGeneratedText.AutoSize = false;
			this.dateGeneratedText.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.dateGeneratedText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.dateGeneratedText.Location = new System.Drawing.Point (72, 8);
			this.dateGeneratedText.Name = "dateGeneratedText";
			this.dateGeneratedText.ReadOnly = true;
			this.dateGeneratedText.Size = new System.Drawing.Size (72, 20);
			this.dateGeneratedText.TabIndex = 21;
			this.dateGeneratedText.Text = "<Date>";
			this.dateGeneratedText.WordWrap = false;
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.AutoSize = true;
			this.descriptionLabel.Location = new System.Drawing.Point (8, 112);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = new System.Drawing.Size (64, 16);
			this.descriptionLabel.TabIndex = 19;
			this.descriptionLabel.Text = "Description:";
			// 
			// instanceIdLabel
			// 
			this.instanceIdLabel.AutoSize = true;
			this.instanceIdLabel.Location = new System.Drawing.Point (152, 48);
			this.instanceIdLabel.Name = "instanceIdLabel";
			this.instanceIdLabel.Size = new System.Drawing.Size (51, 16);
			this.instanceIdLabel.TabIndex = 15;
			this.instanceIdLabel.Text = "Event ID:";
			// 
			// categoryLabel
			// 
			this.categoryLabel.AutoSize = true;
			this.categoryLabel.Location = new System.Drawing.Point (152, 28);
			this.categoryLabel.Name = "categoryLabel";
			this.categoryLabel.Size = new System.Drawing.Size (53, 16);
			this.categoryLabel.TabIndex = 14;
			this.categoryLabel.Text = "Category:";
			// 
			// sourceLabel
			// 
			this.sourceLabel.AutoSize = true;
			this.sourceLabel.Location = new System.Drawing.Point (152, 8);
			this.sourceLabel.Name = "sourceLabel";
			this.sourceLabel.Size = new System.Drawing.Size (43, 16);
			this.sourceLabel.TabIndex = 13;
			this.sourceLabel.Text = "Source:";
			// 
			// button4
			// 
			this.button4.BackColor = System.Drawing.SystemColors.Control;
			this.button4.Location = new System.Drawing.Point (320, 72);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size (48, 23);
			this.button4.TabIndex = 7;
			this.button4.Text = "Copy";
			// 
			// button3
			// 
			this.button3.BackColor = System.Drawing.SystemColors.Control;
			this.button3.Location = new System.Drawing.Point (320, 40);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size (48, 23);
			this.button3.TabIndex = 6;
			this.button3.Text = "Down";
			// 
			// button2
			// 
			this.button2.BackColor = System.Drawing.SystemColors.Control;
			this.button2.Location = new System.Drawing.Point (320, 8);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size (48, 23);
			this.button2.TabIndex = 5;
			this.button2.Text = "Up";
			// 
			// machineNameLabel
			// 
			this.machineNameLabel.AutoSize = true;
			this.machineNameLabel.Location = new System.Drawing.Point (8, 88);
			this.machineNameLabel.Name = "machineNameLabel";
			this.machineNameLabel.Size = new System.Drawing.Size (57, 16);
			this.machineNameLabel.TabIndex = 4;
			this.machineNameLabel.Text = "Computer:";
			// 
			// userNameLabel
			// 
			this.userNameLabel.AutoSize = true;
			this.userNameLabel.Location = new System.Drawing.Point (8, 68);
			this.userNameLabel.Name = "userNameLabel";
			this.userNameLabel.Size = new System.Drawing.Size (31, 16);
			this.userNameLabel.TabIndex = 3;
			this.userNameLabel.Text = "User:";
			// 
			// entryTypeLabel
			// 
			this.entryTypeLabel.AutoSize = true;
			this.entryTypeLabel.Location = new System.Drawing.Point (8, 48);
			this.entryTypeLabel.Name = "entryTypeLabel";
			this.entryTypeLabel.Size = new System.Drawing.Size (33, 16);
			this.entryTypeLabel.TabIndex = 2;
			this.entryTypeLabel.Text = "Type:";
			// 
			// timeGeneratedLabel
			// 
			this.timeGeneratedLabel.AutoSize = true;
			this.timeGeneratedLabel.Location = new System.Drawing.Point (8, 28);
			this.timeGeneratedLabel.Name = "timeGeneratedLabel";
			this.timeGeneratedLabel.Size = new System.Drawing.Size (33, 16);
			this.timeGeneratedLabel.TabIndex = 1;
			this.timeGeneratedLabel.Text = "Time:";
			// 
			// dateGeneratedLabel
			// 
			this.dateGeneratedLabel.AutoSize = true;
			this.dateGeneratedLabel.Location = new System.Drawing.Point (8, 8);
			this.dateGeneratedLabel.Name = "dateGeneratedLabel";
			this.dateGeneratedLabel.Size = new System.Drawing.Size (31, 16);
			this.dateGeneratedLabel.TabIndex = 0;
			this.dateGeneratedLabel.Text = "Date:";
			// 
			// okButton
			// 
			this.okButton.Location = new System.Drawing.Point (128, 384);
			this.okButton.Name = "okButton";
			this.okButton.TabIndex = 1;
			this.okButton.Text = "OK";
			this.okButton.Click += new System.EventHandler (this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Location = new System.Drawing.Point (216, 384);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler (this.cancelButton_Click);
			// 
			// applyButton
			// 
			this.applyButton.Enabled = false;
			this.applyButton.Location = new System.Drawing.Point (304, 384);
			this.applyButton.Name = "applyButton";
			this.applyButton.TabIndex = 3;
			this.applyButton.Text = "Apply";
			// 
			// EventEntryProperties
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size (5, 13);
			this.ClientSize = new System.Drawing.Size (392, 414);
			this.Controls.Add (this.applyButton);
			this.Controls.Add (this.cancelButton);
			this.Controls.Add (this.okButton);
			this.Controls.Add (this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EventEntryProperties";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Event Properties";
			this.tabControl1.ResumeLayout (false);
			this.Event.ResumeLayout (false);
			this.ResumeLayout (false);

		}

		private void wordsRadioButton_CheckedChanged (object sender, System.EventArgs e)
		{
			if (!wordsRadioButton.Checked)
				return;
			DisplayDataWords ();
		}

		private void bytesRadioButton_CheckedChanged (object sender, System.EventArgs e)
		{
			if (!bytesRadioButton.Checked)
				return;
			DisplayDataBytes ();
		}

		private void DisplayDataBytes ()
		{
			byte [] data = _entry.Data;
			dataText.Text = string.Empty;

			StringBuilder lineBuffer = new StringBuilder ();
			StringBuilder wordBuffer = new StringBuilder ();
			for (int i = 0; i < data.Length; i++) {
				if ((i % 8) == 0) {
					if (i > 0) {
						lineBuffer.Append ("  ");
						lineBuffer.Append (wordBuffer.ToString ());
						lineBuffer.Append (Environment.NewLine);
						wordBuffer.Length = 0;
						dataText.Text += lineBuffer.ToString ();
						lineBuffer.Length = 0;
					}
					lineBuffer.Append (i.ToString ("x").PadLeft (4, '0'));
					lineBuffer.Append (": ");
				}
				byte b = data [i];
				string hexValue = b.ToString ("x");
				lineBuffer.Append (hexValue.PadLeft (2, '0'));
				lineBuffer.Append (' ');

				char c = (char) b;
				if (char.IsControl (c))
					wordBuffer.Append ('.');
				else
					wordBuffer.Append (c);
			}
			if (lineBuffer.Length > 0) {
				lineBuffer.Append (new string (' ', 32 - lineBuffer.Length));
				lineBuffer.Append (wordBuffer.ToString ());
				dataText.Text += lineBuffer.ToString ();
			}
		}

		private void DisplayDataWords ()
		{
			dataText.Text = string.Empty;

			StringBuilder lineBuffer = new StringBuilder ();
			StringBuilder wordBuffer = new StringBuilder ();
			byte [] data = _entry.Data;
			for (int i = 0; i < data.Length; i++) {
				if (i > 0 && (i % 4) == 0) {
					lineBuffer.Append (' ');
					lineBuffer.Append (wordBuffer.ToString ());
					wordBuffer.Length = 0;
				}
				if ((i % 16) == 0) {
					if (i > 0) {
						lineBuffer.Append (Environment.NewLine);
						dataText.Text += lineBuffer.ToString ();
						lineBuffer.Length = 0;
					}
					lineBuffer.Append (i.ToString ("x").PadLeft (4, '0'));
					lineBuffer.Append (":");
				}
				byte b = data [i];
				string hexValue = b.ToString ("x");
				wordBuffer.Insert (0, hexValue.PadLeft (2, '0'));
			}
			if (lineBuffer.Length > 0) {
				lineBuffer.Append (' ');
				lineBuffer.Append (wordBuffer.ToString ());
				dataText.Text += lineBuffer.ToString ();
			}
		}

		private void okButton_Click (object sender, System.EventArgs e)
		{
			Close ();
		}

		private void cancelButton_Click (object sender, System.EventArgs e)
		{
			Close ();
		}
	}
}
