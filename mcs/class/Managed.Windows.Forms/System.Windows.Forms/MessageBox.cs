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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez, jordi@ximian.com
//	Benjamin Dasnois, benjamin.dasnois@gmail.com
//
// TODO:
//	- Complete the implementation when icons are available, Form.BorderStyle is available.
//	- Add support for MessageBoxOptions and MessageBoxDefaultButton.
//
//
// $Log: MessageBox.cs,v $
// Revision 1.2  2004/10/05 17:10:57  pbartok
// - Partial implementation by Benjamin Dasnois
//
// Revision 1.1	 2004/07/26 11:41:35  jordi
// initial messagebox implementation
//
//

// INCOMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms
{
	public class MessageBox
	{
		#region Private MessageBoxForm class
		private class MessageBoxForm : Form
		{
			#region MessageBoxFrom Local Variables
			string			msgbox_text;
			bool			size_known	=false;
			const int		space_image_text= 20;
			Image			icon_image;
			Point			textleft_up;
			MessageBoxButtons	msgbox_buttons;
			bool			buttons_placed	= false;
			int			button_left;
			#endregion	// MessageBoxFrom Local Variables

			#region MessageBoxForm Constructors
			public MessageBoxForm (IWin32Window owner, string text, string caption,
					MessageBoxButtons buttons, MessageBoxIcon icon) 
			{
				switch (icon) {
					case MessageBoxIcon.None: {
						icon_image = null;
						break;
					}

					case MessageBoxIcon.Error: {
						break;
					}

					case MessageBoxIcon.Question: {
						break;
					}

					case MessageBoxIcon.Exclamation: {
						break;
					}

					case MessageBoxIcon.Asterisk: {
						break;
					}
				}

				msgbox_text = text;
				msgbox_buttons = buttons;
				this.Text = caption;
				this.Paint += new PaintEventHandler (MessageBoxForm_Paint);
			}

			[MonoTODO]
			public MessageBoxForm (IWin32Window owner, string text, string caption,
					MessageBoxButtons buttons, MessageBoxIcon icon,
					MessageBoxDefaultButton defaultButton, MessageBoxOptions options) : this (owner, text, caption, buttons, icon)
			{
				// Still needs to implement defaultButton and options

			}
			#endregion	// MessageBoxForm Constructors

			
			#region MessageBoxForm Methods
			public DialogResult RunDialog ()
			{
				this.StartPosition = FormStartPosition.CenterScreen;
				this.ShowDialog ();

				return this.DialogResult;

			}

			private void MessageBoxForm_Paint(object sender, PaintEventArgs e)
			{
				if (size_known == false) {
					InitFormsSize (e);
				}
			       
				e.Graphics.DrawString (msgbox_text, this.Font, new SolidBrush(Color.Black), textleft_up);
			}

			private void InitFormsSize(PaintEventArgs e)
			{
				int tb_width = 0;
		
				// First we have to know the size of text + image
				Drawing.SizeF tsize = e.Graphics.MeasureString (msgbox_text, this.Font);

				if (!(icon_image == null)) {
					tsize.Width += icon_image.Width;
					textleft_up = new Point (icon_image.Width + space_image_text, 0);
				} else {
					textleft_up = new Point (0, 0);
				}

				// Now we want to know the width of buttons
		
				switch (msgbox_buttons) {
					case MessageBoxButtons.OK: {
						tb_width = 110 * 1;
						break;
					}

					case MessageBoxButtons.OKCancel: {
						tb_width = 110 * 2;
						break;
					}

					case MessageBoxButtons.AbortRetryIgnore: {
						tb_width = 110 * 3;
						break;
					}

					case MessageBoxButtons.YesNoCancel: {
						tb_width = 110 * 3;
						break;
					}

					case MessageBoxButtons.YesNo: {
						tb_width = 110 * 2;
						break;
					}

					case MessageBoxButtons.RetryCancel: {
						tb_width = 110 * 2;
						break;
					}
				}

				// Now we choose the good size for the form
				if (tsize.ToSize ().Width > tb_width) {
					this.Width = tsize.ToSize().Width + 10;
				} else {
					this.Width = tb_width;
				}
				this.Height = tsize.ToSize ().Height + 80;

				// Now we set the left of the buttons
				button_left = (this.Width / 2) - (tb_width / 2);
				AddButtons ();
				size_known = true;
				this.Refresh ();
			}
			#endregion	// MessageBoxForm Methods

			#region Functions for Adding buttons
			private void AddButtons()
			{
				if (!buttons_placed) {
					switch (msgbox_buttons) {
						case MessageBoxButtons.OK: {
							AddOkButton (0 + button_left);
							break;
						}

						case MessageBoxButtons.OKCancel: {
							AddOkButton (0 + button_left);
							AddCancelButton (110 + button_left);
							break;
						}

						case MessageBoxButtons.AbortRetryIgnore: {
							AddAbortButton (0 + button_left);
							AddRetryButton (110 + button_left);
							AddIgnoreButton (220 + button_left);
							break;
						}

						case MessageBoxButtons.YesNoCancel: {
							AddYesButton (0 + button_left);
							AddNoButton (110 + button_left);
							AddCancelButton (220 + button_left);
							break;
						}

						case MessageBoxButtons.YesNo: {
							AddYesButton (0 + button_left);
							AddNoButton (110 + button_left);
							break;
						}

						case MessageBoxButtons.RetryCancel: {
							AddRetryButton (0 + button_left);
							AddCancelButton (110 + button_left);
							break;
						}
					}
					buttons_placed = true;
				}
			}

			private void AddOkButton (int left)
			{
				Button bok = new Button ();
				bok.Text = "OK";
				this.Controls.Add (bok);
				bok.Width = 100;
				bok.Height = 30;
				bok.Top = this.Height - 70;
				bok.Left = left;
				bok.Show ();
				bok.Click += new EventHandler (OkClick);

			}

			private void AddCancelButton (int left)
			{
				Button bcan = new Button ();
				bcan.Text = "Cancel";
				this.Controls.Add (bcan);
				bcan.Width = 100;
				bcan.Height = 30;
				bcan.Top = this.Height - 70;
				bcan.Left = left;
				bcan.Show ();
				bcan.Click += new EventHandler (CancelClick);
			}

			private void AddAbortButton (int left)
			{
				Button babort = new Button ();
				babort.Text = "Abort";
				this.Controls.Add (babort);
				babort.Width = 100;
				babort.Height = 30;
				babort.Top = this.Height - 70;
				babort.Left = left;
				babort.Show ();
				babort.Click += new EventHandler (AbortClick);

			}

			private void AddRetryButton(int left)
			{
				Button bretry = new Button ();
				bretry.Text = "Retry";
				this.Controls.Add (bretry);
				bretry.Width = 100;
				bretry.Height = 30;
				bretry.Top = this.Height - 70;
				bretry.Left = left;
				bretry.Show ();
				bretry.Click += new EventHandler (RetryClick);

			}

			private void AddIgnoreButton (int left)
			{
				Button bignore = new Button ();
				bignore.Text = "Ignore";
				this.Controls.Add (bignore);
				bignore.Width = 100;
				bignore.Height = 30;
				bignore.Top = this.Height - 70;
				bignore.Left = left;
				bignore.Show ();
				bignore.Click += new EventHandler (IgnoreClick);

			}

			private void AddYesButton (int left)
			{
				Button byes = new Button ();
				byes.Text = "Yes";
				this.Controls.Add (byes);
				byes.Width = 100;
				byes.Height = 30;
				byes.Top = this.Height - 70;
				byes.Left = left;
				byes.Show ();
				byes.Click += new EventHandler (YesClick);

			}

			private void AddNoButton (int left)
			{
				Button bno = new Button ();
				bno.Text = "No";
				this.Controls.Add (bno);
				bno.Width = 100;
				bno.Height = 30;
				bno.Top = this.Height - 70;
				bno.Left = left;
				bno.Show ();
				bno.Click += new EventHandler (NoClick);

			}
			#endregion

			#region Button click handlers
			private void OkClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.OK;
				this.Close ();
			}

			private void CancelClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close ();
			}

			private void AbortClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.Abort;
				this.Close ();
			}

			private void RetryClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.Retry;
				this.Close ();
			}

			private void IgnoreClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.Ignore;
				this.Close ();
			}

			private void YesClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.Yes;
				this.Close ();
			}

			private void NoClick (object sender, EventArgs e)
			{
				this.DialogResult = DialogResult.No;
				this.Close ();
			}
			#endregion
		}
		#endregion	// Private MessageBoxForm class


		#region	Constructors
		private MessageBox ()
		{

		}
		#endregion	// Constructors

		#region Public Static Methods
		public static DialogResult Show (string text)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, string.Empty,
					MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, string.Empty,
					MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
					MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
					buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text,  string caption,
				MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					buttons, icon);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
				MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
					buttons, icon);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
				MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{

			MessageBoxForm form = new MessageBoxForm (null, text, caption,
					buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon,	 MessageBoxDefaultButton defaultButton)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();
		}

		public static DialogResult
				Show (string text, string caption,
						MessageBoxButtons buttons, MessageBoxIcon icon,
						MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption,
					buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon,
				MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}
		#endregion	// Public Static Methods
	}
}

