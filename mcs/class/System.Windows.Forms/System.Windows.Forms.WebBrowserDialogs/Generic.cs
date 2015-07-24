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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita	<avidigal@novell.com>


using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

namespace System.Windows.Forms.WebBrowserDialogs
{
	internal class Generic : Form
	{
		TableLayoutPanel table;

		public Generic (string title)
		{
			this.SuspendLayout ();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.ControlBox = true;
			this.MinimizeBox = false;
			this.MaximizeBox = false;
			this.ShowInTaskbar = (Owner == null);
			this.FormBorderStyle = FormBorderStyle.FixedDialog;

			table = new TableLayoutPanel ();
			table.SuspendLayout ();
			table.AutoSize = true;
			this.Controls.Add (table);

			this.Text = title;
		}

		public new DialogResult Show ()
		{
			return this.RunDialog ();
		}

		private void InitSize ()
		{
		}

		protected void InitTable (int rows, int cols)
		{
			table.ColumnCount = cols;
			for (int i = 0; i < cols; i++)
				table.ColumnStyles.Add (new ColumnStyle ());
			table.RowCount = rows;
			for (int i = 0; i < rows; i++)
				table.RowStyles.Add (new RowStyle ());
		}

		protected void AddLabel (int row, int col, int colspan, string text, int width, int height)
		{
			Label ctl = new Label ();
			ctl.Text = text;
			if (width == -1 && height == -1)
				ctl.AutoSize = true;
			else {
				ctl.Width = width;
				ctl.Height = height;
			}
			table.Controls.Add (ctl, col, row);
			if (colspan > 1)
				table.SetColumnSpan (ctl, colspan);
		}

		protected void AddButton (int row, int col, int colspan, string text, int width, int height, bool isAccept, bool isCancel, EventHandler onClick)
		{
			Button ctl = new Button ();
			ctl.Text = text;
			if (width == -1 && height == -1) {
				//SizeF s = TextRenderer.MeasureString (text, ctl.Font);
				//ctl.Width = (int) ((float) s.Width / 62f);
				//ctl.Height = (int)s.Height;
			} else {
				ctl.Width = width;
				ctl.Height = height;
			}

			if (onClick != null)
				ctl.Click += onClick;
			if (isAccept)
				AcceptButton = ctl;
			if (isCancel)
				CancelButton = ctl;
			table.Controls.Add (ctl, col, row);
			if (colspan > 1)
				table.SetColumnSpan (ctl, colspan);
		}

		protected void AddCheck (int row, int col, int colspan, string text, bool check, int width, int height, EventHandler onCheck)
		{
			CheckBox ctl = new CheckBox ();
			ctl.Text = text;
			ctl.Checked = check;

			if (width == -1 && height == -1) {
				SizeF s = TextRenderer.MeasureString (text, ctl.Font);
				ctl.Width += (int) ((float) s.Width / 62f);
				if (s.Height > ctl.Height)
					ctl.Height = (int) s.Height;
			} else {
				ctl.Width = width;
				ctl.Height = height;
			}

			if (onCheck != null)
				ctl.CheckedChanged += onCheck;

			table.Controls.Add (ctl, col, row);
			if (colspan > 1)
				table.SetColumnSpan (ctl, colspan);
		}

		protected void AddText (int row, int col, int colspan, string text, int width, int height, EventHandler onText)
		{
			TextBox ctl = new TextBox ();
			ctl.Text = text;

			if (width > -1)
				ctl.Width = width;
			if (height > -1)
				ctl.Height = height;

			if (onText != null)
				ctl.TextChanged += onText;

			table.Controls.Add (ctl, col, row);
			if (colspan > 1)
				table.SetColumnSpan (ctl, colspan);
		}

		protected void AddPassword (int row, int col, int colspan, string text, int width, int height, EventHandler onText)
		{
			TextBox ctl = new TextBox ();
			ctl.PasswordChar = '*';
			ctl.Text = text;

			if (width > -1)
				ctl.Width = width;
			if (height > -1)
				ctl.Height = height;

			if (onText != null)
				ctl.TextChanged += onText;

			table.Controls.Add (ctl, col, row);
			if (colspan > 1)
				table.SetColumnSpan (ctl, colspan);
		}

		protected DialogResult RunDialog ()
		{
			this.StartPosition = FormStartPosition.CenterScreen;

			InitSize ();

			table.ResumeLayout (false);
			table.PerformLayout ();
			this.ResumeLayout (false);
			this.PerformLayout ();

			this.ShowDialog ();

			return this.DialogResult;
		}
	}
}
