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
//	Jordi Mas i Hernandez	(jordi@ximian.com)
//	Benjamin Dasnois	(benjamin.dasnois@gmail.com)
//	Robert Thompson		(rmt@corporatism.org)
//	Peter Bartok		(pbartok@novell.com)
//
// TODO:
//	- Complete the implementation when Form.BorderStyle is available.
//	- Add support for MessageBoxOptions and MessageBoxDefaultButton.
//	- Button size calculations assume fixed height for buttons, that could be bad
//


// NOT COMPLETE

using System;
using System.Drawing;
using System.Globalization;
using System.Resources;

namespace System.Windows.Forms
{
	public class MessageBox
	{
		#region Private MessageBoxForm class
		private class MessageBoxForm : Form
		{
			#region MessageBoxFrom Local Variables
			internal static Image	error_icon	= (Image)Locale.GetResource("mbox_error.png");
			internal static Image	question_icon	= (Image)Locale.GetResource("mbox_question.png");
			internal static Image	warning_icon	= (Image)Locale.GetResource("mbox_warn.png");
			internal static Image	info_icon	= (Image)Locale.GetResource("mbox_info.png");
			internal static string	Yes;
			internal const int	space_border	= 10;
			string			msgbox_text;
			bool			size_known	= false;
			const int		space_image_text= 10;
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

					case MessageBoxIcon.Error: {		// Same as MessageBoxIcon.Hand and MessageBoxIcon.Stop
						icon_image = error_icon;
						break;
					}

					case MessageBoxIcon.Question: {
 						icon_image = question_icon;
						break;
					}

					case MessageBoxIcon.Asterisk: {		// Same as MessageBoxIcon.Information
						icon_image = info_icon;
						break;
					}

					case MessageBoxIcon.Warning: {		// Same as MessageBoxIcon.Exclamation:
						icon_image = warning_icon;
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

			#region Protected Instance Properties
			protected override CreateParams CreateParams {
				get {
					CreateParams	cp;

					ControlBox = true;
					MinimizeBox = false;
					MaximizeBox = false;

					cp = base.CreateParams;

					cp.Style = (int)(WindowStyles.WS_DLGFRAME | WindowStyles.WS_POPUP | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CAPTION);

					return cp;
				}
			}
			#endregion	// Protected Instance Properties

			#region MessageBoxForm Methods
			public DialogResult RunDialog ()
			{
				this.StartPosition = FormStartPosition.CenterScreen;

				if (size_known == false) {
					InitFormsSize ();
				}

				this.ShowDialog ();

				return this.DialogResult;

			}

			private void MessageBoxForm_Paint (object sender, PaintEventArgs e)
			{
				e.Graphics.DrawString (msgbox_text, this.Font, ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black), textleft_up);
				if (icon_image != null) {
					e.Graphics.DrawImage(icon_image, new Point(space_border, space_border));
				}
			}

			private void InitFormsSize ()
			{
				int tb_width = 0;
		
				// First we have to know the size of text + image
				Drawing.SizeF tsize = DeviceContext.MeasureString (msgbox_text, this.Font);

				if (icon_image != null) {
					tsize.Width += icon_image.Width + 10;
					if(icon_image.Height > tsize.Height) {
						// Place text middle-right
						textleft_up = new Point (icon_image.Width + space_image_text + space_border, (int)((icon_image.Height/2)-(tsize.Height/2)) + space_border);
					} else {
						textleft_up = new Point (icon_image.Width + space_image_text + space_border, 2 + space_border);
					}
					tsize.Height = icon_image.Height;
				} else {
					tsize.Width += space_border * 2;
					textleft_up = new Point (space_border + 12, space_border + 12);
					tsize.Height += space_border * 2;
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
					//this.Width = tsize.ToSize().Width + 10;
					this.ClientSize = new Size(tsize.ToSize().Width + 10 + (space_border * 2), Height = tsize.ToSize ().Height + 40 + (space_border * 2));
				} else {
					//this.Width = tb_width + 10;
					this.ClientSize = new Size(tb_width + 10 + (space_border * 2), Height = tsize.ToSize ().Height + 40 + (space_border * 2));
				}

				// Now we set the left of the buttons
				button_left = (this.ClientSize.Width / 2) - (tb_width / 2) + 5;
				AddButtons ();
				size_known = true;
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
				bok.Text = Locale.GetText("OK");
				bok.Width = 100;
				bok.Height = 30;
				bok.Top = this.ClientSize.Height - 35 - space_border;
				bok.Left = left;
				bok.Click += new EventHandler (OkClick);
				AcceptButton = bok;
				this.Controls.Add (bok);
			}

			private void AddCancelButton (int left)
			{
				Button bcan = new Button ();
				bcan.Text = Locale.GetText("Cancel");
				bcan.Width = 100;
				bcan.Height = 30;
				bcan.Top = this.ClientSize.Height - 35 - space_border;
				bcan.Left = left;
				bcan.Click += new EventHandler (CancelClick);
				CancelButton = bcan;
				this.Controls.Add (bcan);
			}

			private void AddAbortButton (int left)
			{
				Button babort = new Button ();
				babort.Text = Locale.GetText("Abort");
				babort.Width = 100;
				babort.Height = 30;
				babort.Top = this.ClientSize.Height - 35 - space_border;
				babort.Left = left;
				babort.Click += new EventHandler (AbortClick);
				CancelButton = babort;
				this.Controls.Add (babort);
			}

			private void AddRetryButton(int left)
			{
				Button bretry = new Button ();
				bretry.Text = Locale.GetText("Retry");
				bretry.Width = 100;
				bretry.Height = 30;
				bretry.Top = this.ClientSize.Height - 35 - space_border;
				bretry.Left = left;
				bretry.Click += new EventHandler (RetryClick);
				AcceptButton = bretry;
				this.Controls.Add (bretry);
			}

			private void AddIgnoreButton (int left)
			{
				Button bignore = new Button ();
				bignore.Text = Locale.GetText("Ignore");
				bignore.Width = 100;
				bignore.Height = 30;
				bignore.Top = this.ClientSize.Height - 35 - space_border;
				bignore.Left = left;
				bignore.Click += new EventHandler (IgnoreClick);
				this.Controls.Add (bignore);
			}

			private void AddYesButton (int left)
			{
				Button byes = new Button ();
				byes.Text = Locale.GetText("Yes");
				byes.Width = 100;
				byes.Height = 30;
				byes.Top = this.ClientSize.Height - 35 - space_border;
				byes.Left = left;
				byes.Click += new EventHandler (YesClick);
				AcceptButton = byes;
				this.Controls.Add (byes);
			}

			private void AddNoButton (int left)
			{
				Button bno = new Button ();
				bno.Text = Locale.GetText("No");
				bno.Width = 100;
				bno.Height = 30;
				bno.Top = this.ClientSize.Height - 35 - space_border;
				bno.Left = left;
				bno.Click += new EventHandler (NoClick);
				CancelButton = bno;
				this.Controls.Add (bno);
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
			MessageBoxForm form = new MessageBoxForm (null, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, string.Empty, MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);

			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons, icon);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (IWin32Window owner, string text, string caption)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, MessageBoxButtons.OK, MessageBoxIcon.None);
				
			return form.RunDialog ();
		}


		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
				MessageBoxIcon icon)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, icon);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons,
				MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
		{

			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();

		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
				MessageBoxButtons buttons, MessageBoxIcon icon,	 MessageBoxDefaultButton defaultButton)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons, icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption,
					buttons, icon, defaultButton, options);
				
			return form.RunDialog ();
		}
		#endregion	// Public Static Methods
	}
}

