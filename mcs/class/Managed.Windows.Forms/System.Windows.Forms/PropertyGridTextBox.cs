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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

// COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms.PropertyGridInternal {
	internal class PropertyGridTextBox : System.Windows.Forms.UserControl {
		#region Private Members

		private TextBox textbox;
		private Button dialog_button;
		private Button dropdown_button;

		#endregion Private Members

		#region Contructors
		public PropertyGridTextBox() {
			dialog_button = new Button();
			dropdown_button = new Button();
			textbox = new TextBox();

			SuspendLayout();

			dialog_button.Dock = DockStyle.Right;
			dialog_button.Size = new Size(16, 16);
			dialog_button.TabIndex = 1;
			dialog_button.Visible = false;
			dialog_button.Click += new System.EventHandler(dialog_button_Click);

			dropdown_button.Dock = DockStyle.Right;
			dropdown_button.Size = new Size(16, 16);
			dropdown_button.TabIndex = 2;
			dropdown_button.Visible = false;
			dropdown_button.Click += new System.EventHandler(dropdown_button_Click);

			textbox.AutoSize = false;
			textbox.BorderStyle = BorderStyle.None;
			textbox.Dock = DockStyle.Fill;
			textbox.TabIndex = 3;

			Controls.Add(textbox);
			Controls.Add(dropdown_button);
			Controls.Add(dialog_button);

			ResumeLayout(false);

			dropdown_button.Paint+=new PaintEventHandler(dropdown_button_Paint);
			dialog_button.Paint+=new PaintEventHandler(dialog_button_Paint);
			textbox.DoubleClick+=new EventHandler(textbox_DoubleClick);
		}

		
		#endregion Contructors

		#region Public Instance Properties

		public bool DialogButtonVisible {
			get{
				return dialog_button.Visible;
			}
			set {
				dialog_button.Visible = value;
			}
		}
		public bool DropDownButtonVisible {
			get{
				return dropdown_button.Visible;
			}
			set {
				dropdown_button.Visible = value;
			}
		}

		public bool ReadOnly {
			get {
				return textbox.ReadOnly;
			}
			set {
				textbox.ReadOnly = value;
			}
		}

		public new string Text {
			get {
				return textbox.Text;
			}
			set {
				textbox.Text = value;
			}
		}

		#endregion Public Instance Properties
		
		#region Events

		public event EventHandler DropDownButtonClicked;
		public event EventHandler DialogButtonClicked;
		public event EventHandler ToggleValue;
		
		#endregion Events
		
		#region Private Helper Methods

		private void dropdown_button_Paint(object sender, PaintEventArgs e)
		{
			ThemeEngine.Current.CPDrawComboButton(e.Graphics, dropdown_button.ClientRectangle, dropdown_button.ButtonState);
		}

		private void dialog_button_Paint(object sender, PaintEventArgs e) {
			// best way to draw the ellipse?
			e.Graphics.DrawString("...", new Font(Font,FontStyle.Bold), Brushes.Black, 0,0);
		}

		private void dropdown_button_Click(object sender, System.EventArgs e) {
			if (DropDownButtonClicked != null)
				DropDownButtonClicked(this, EventArgs.Empty);
		}

		private void dialog_button_Click(object sender, System.EventArgs e) {
			if (DialogButtonClicked != null)
				DialogButtonClicked(this, EventArgs.Empty);
		}

		#endregion Private Helper Methods

		private void textbox_DoubleClick(object sender, EventArgs e) {
			if (ToggleValue != null)
				ToggleValue(this, EventArgs.Empty);
		}
	}
}
