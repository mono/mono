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
// Copyright (c) 2004-2006 Novell, Inc.
//
// Authors:
//	Jordi Mas i Hernandez	(jordi@ximian.com)
//	Benjamin Dasnois	(benjamin.dasnois@gmail.com)
//	Robert Thompson		(rmt@corporatism.org)
//	Peter Bartok		(pbartok@novell.com)
//
// TODO:
//	- Add support for MessageBoxOptions and MessageBoxDefaultButton.
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
		internal class MessageBoxForm : Form
		{
			#region MessageBoxFrom Local Variables
			const int space_border = 10;
			const int button_width = 86;
			const int button_height = 23;
			const int button_space = 5;
			const int space_image_text= 10;

			string			msgbox_text;
			bool			size_known	= false;
			Icon			icon_image;
			RectangleF		text_rect;
			MessageBoxButtons	msgbox_buttons;
			MessageBoxDefaultButton	msgbox_default;
			bool			buttons_placed	= false;
			int			button_left;
			Button[]		buttons = new Button[4];
			bool                    show_help;
			string help_file_path;
			string help_keyword;
			HelpNavigator help_navigator;
			object help_param;
			AlertType		alert_type;
			#endregion	// MessageBoxFrom Local Variables
			
			#region MessageBoxForm Constructors
			public MessageBoxForm (IWin32Window owner, string text, string caption,
					       MessageBoxButtons buttons, MessageBoxIcon icon,
					       bool displayHelpButton)
			{
				show_help = displayHelpButton;

				switch (icon) {
					case MessageBoxIcon.None: {
						icon_image = null;
						alert_type = AlertType.Default;
						break;
					}

					case MessageBoxIcon.Error: {		// Same as MessageBoxIcon.Hand and MessageBoxIcon.Stop
						icon_image = SystemIcons.Error;
						alert_type = AlertType.Error;
						break;
					}

					case MessageBoxIcon.Question: {
 						icon_image = SystemIcons.Question;
						alert_type = AlertType.Question;
						break;
					}

					case MessageBoxIcon.Asterisk: {		// Same as MessageBoxIcon.Information
						icon_image = SystemIcons.Information;
						alert_type = AlertType.Information;
						break;
					}

					case MessageBoxIcon.Warning: {		// Same as MessageBoxIcon.Exclamation:
						icon_image = SystemIcons.Warning;
						alert_type = AlertType.Warning;
						break;
					}
				}

				msgbox_text = text;
				msgbox_buttons = buttons;
				msgbox_default = MessageBoxDefaultButton.Button1;

				if (owner != null) {
					Owner = Control.FromHandle(owner.Handle).FindForm();
				} else {
					if (Application.MWFThread.Current.Context != null) {
						Owner = Application.MWFThread.Current.Context.MainForm;
					}
				}
				this.Text = caption;
				this.ControlBox = true;
				this.MinimizeBox = false;
				this.MaximizeBox = false;
				this.ShowInTaskbar = (Owner == null);
				this.FormBorderStyle = FormBorderStyle.FixedDialog;
			}

			public MessageBoxForm (IWin32Window owner, string text, string caption,
					MessageBoxButtons buttons, MessageBoxIcon icon,
					MessageBoxDefaultButton defaultButton, MessageBoxOptions options, bool displayHelpButton)
				: this (owner, text, caption, buttons, icon, displayHelpButton)
			{
				msgbox_default = defaultButton;
			}

			public MessageBoxForm (IWin32Window owner, string text, string caption,
					       MessageBoxButtons buttons, MessageBoxIcon icon)
				: this (owner, text, caption, buttons, icon, false)
			{
			}
			#endregion	// MessageBoxForm Constructors

			#region Protected Instance Properties
			protected override CreateParams CreateParams {
				get {
					CreateParams cp = base.CreateParams;;

					cp.Style |= (int)(WindowStyles.WS_DLGFRAME | WindowStyles.WS_CLIPCHILDREN | WindowStyles.WS_CLIPSIBLINGS | WindowStyles.WS_CAPTION);
					
					if (!is_enabled)
						cp.Style |= (int)(WindowStyles.WS_DISABLED);

					return cp;
				}
			}
			#endregion	// Protected Instance Properties

			#region MessageBoxForm Methods
			public void SetHelpData (string file_path, string keyword, HelpNavigator navigator, object param)
			{
				help_file_path = file_path;
				help_keyword = keyword;
				help_navigator = navigator;
				help_param = param;
			}
			
			internal string HelpFilePath {
				get { return help_file_path; }
			}
			
			internal string HelpKeyword {
				get { return help_keyword; }
			}
			
			internal HelpNavigator HelpNavigator {
				get { return help_navigator; }
			}
			
			internal object HelpParam {
				get { return help_param; }
			}
			
			public DialogResult RunDialog ()
			{
				this.StartPosition = FormStartPosition.CenterScreen;

				if (size_known == false) {
					InitFormsSize ();
				}

				if (Owner != null)
					TopMost = Owner.TopMost;
					
				XplatUI.AudibleAlert (alert_type);
				this.ShowDialog ();

				return this.DialogResult;
			}

			internal override void OnPaintInternal (PaintEventArgs e)
			{
				e.Graphics.DrawString (msgbox_text, this.Font, ThemeEngine.Current.ResPool.GetSolidBrush (Color.Black), text_rect);
				if (icon_image != null) {
					e.Graphics.DrawIcon(icon_image, space_border, space_border);
				}
			}

			private void InitFormsSize ()
			{
				int tb_width = 0;

				// Max width of messagebox must be 60% of screen width
				int max_width = (int) (Screen.GetWorkingArea (this).Width * 0.6);

				// First we have to know the size of text + image
				Drawing.SizeF tsize = TextRenderer.MeasureString (msgbox_text, this.Font, max_width);
				text_rect = new RectangleF ();
				text_rect.Size = tsize;
				
				if (icon_image != null) {
					tsize.Width += icon_image.Width + 10;
					if(icon_image.Height > tsize.Height) {
						// Place text middle-right
						text_rect.Location = new Point (icon_image.Width + space_image_text + space_border, (int)((icon_image.Height/2)-(tsize.Height/2)) + space_border);
					} else {
						text_rect.Location = new Point (icon_image.Width + space_image_text + space_border, 2 + space_border);
					}
					if (tsize.Height < icon_image.Height)
						tsize.Height = icon_image.Height;
				} else {
					text_rect.Location = new Point (space_border + button_space, space_border);
				}
				tsize.Height += space_border * 2;

				// Now we want to know the amount of buttons
				int buttoncount;
				switch (msgbox_buttons) {
					case MessageBoxButtons.OK:
						buttoncount = 1;
						break;

					case MessageBoxButtons.OKCancel:
						buttoncount = 2;
						break;

					case MessageBoxButtons.AbortRetryIgnore:
						buttoncount = 3;
						break;

					case MessageBoxButtons.YesNoCancel:
						buttoncount = 3;
						break;

					case MessageBoxButtons.YesNo:
						buttoncount = 2;
						break;

					case MessageBoxButtons.RetryCancel:
						buttoncount = 2;
						break;
					
					default:
						buttoncount = 0;
						break;
				
				}
				if (show_help)
					buttoncount ++;
				
				// Calculate the width based on amount of buttons 
				tb_width = (button_width + button_space) * buttoncount;  

				// The form caption can also make us bigger
				SizeF caption = TextRenderer.MeasureString (Text, new Font (DefaultFont, FontStyle.Bold));
				
				// Use the bigger of the caption size (plus some arbitrary borders/close button)
				// or the text size, up to 60% of the screen (max_size)
				Size new_size = new SizeF (Math.Min (Math.Max (caption.Width + 40, tsize.Width), max_width), tsize.Height).ToSize ();
				
				// Now we choose the good size for the form
				if (new_size.Width > tb_width)
					this.ClientSize = new Size (new_size.Width + (space_border * 2), Height = new_size.Height + (space_border * 4));
				else
					this.ClientSize = new Size (tb_width + (space_border * 2), Height = new_size.Height + (space_border * 4));

				// Now we set the left of the buttons
				button_left = (this.ClientSize.Width / 2) - (tb_width / 2) + 5;
				AddButtons ();
				size_known = true;

				// Still needs to implement defaultButton and options
				switch(msgbox_default) {
					case MessageBoxDefaultButton.Button2: {
						if (this.buttons[1] != null) {
							ActiveControl = this.buttons[1];
						}
						break;
					}

					case MessageBoxDefaultButton.Button3: {
						if (this.buttons[2] != null) {
							ActiveControl = this.buttons[2];
						}
						break;
					}
				}
			}

			protected override bool ProcessDialogKey(Keys keyData) {
				if (keyData == Keys.Escape) {
					this.CancelClick(this, null);
					return true;
				}

				if (((keyData & Keys.Modifiers) == Keys.Control) &&
					(((keyData & Keys.KeyCode) == Keys.C) ||
					 ((keyData & Keys.KeyCode) == Keys.Insert))) {  
					Copy();
				}

				return base.ProcessDialogKey (keyData);
			}

			protected override bool ProcessDialogChar (char charCode)
			{
				// Shortcut keys, kinda like mnemonics, except you don't have to press Alt
				if ((charCode == 'N' || charCode == 'n') && (CancelButton != null && (CancelButton as Button).Text == "No"))
					CancelButton.PerformClick ();
				else if ((charCode == 'Y' || charCode == 'y') && (AcceptButton as Button).Text == "Yes")
					AcceptButton.PerformClick ();
				else if ((charCode == 'A' || charCode == 'a') && (CancelButton != null && (CancelButton as Button).Text == "Abort"))
					CancelButton.PerformClick ();
				else if ((charCode == 'R' || charCode == 'r') && (AcceptButton as Button).Text == "Retry")
					AcceptButton.PerformClick ();
				else if ((charCode == 'I' || charCode == 'i') && buttons.Length >= 3 && buttons[2].Text == "Ignore")
					buttons[2].PerformClick ();
				
				return base.ProcessDialogChar (charCode);
			}
			
			private void Copy ()
			{
				string separator = "---------------------------" + Environment.NewLine;

				System.Text.StringBuilder contents = new System.Text.StringBuilder ();

				contents.Append (separator);
				contents.Append (this.Text).Append (Environment.NewLine);
				contents.Append (separator);
				contents.Append (msgbox_text).Append (Environment.NewLine);
				contents.Append (separator);

				foreach (Button btn in buttons) {
					if (btn == null)
						break;
					contents.Append (btn.Text).Append ("   ");;
				}

				contents.Append (Environment.NewLine);
				contents.Append (separator);

				DataObject obj = new DataObject(DataFormats.Text, contents.ToString());
				Clipboard.SetDataObject (obj);
			}

			#endregion	// MessageBoxForm Methods

			#region Functions for Adding buttons
			private void AddButtons()
			{
				if (!buttons_placed) {
					switch (msgbox_buttons) {
						case MessageBoxButtons.OK: {
							buttons[0] = AddOkButton (0);
							break;
						}

						case MessageBoxButtons.OKCancel: {
							buttons[0] = AddOkButton (0);
							buttons[1] = AddCancelButton (1);
							break;
						}

						case MessageBoxButtons.AbortRetryIgnore: {
							buttons[0] = AddAbortButton (0);
							buttons[1] = AddRetryButton (1);
							buttons[2] = AddIgnoreButton (2);
							break;
						}

						case MessageBoxButtons.YesNoCancel: {
							buttons[0] = AddYesButton (0);
							buttons[1] = AddNoButton (1);
							buttons[2] = AddCancelButton (2);
							break;
						}

						case MessageBoxButtons.YesNo: {
							buttons[0] = AddYesButton (0);
							buttons[1] = AddNoButton (1);
							break;
						}

						case MessageBoxButtons.RetryCancel: {
							buttons[0] = AddRetryButton (0);
							buttons[1] = AddCancelButton (1);
							break;
						}
					}

					if (show_help) {
						for (int i = 0; i <= 3; i++) {
							if (buttons [i] == null) {
								AddHelpButton (i);
								break;
							}
						}
					}

					buttons_placed = true;
				}
			}

			private Button AddButton (string text, int left, EventHandler click_event)
			{
				Button button = new Button ();
				button.Text = Locale.GetText(text);
				button.Width = button_width;
				button.Height = button_height;
				button.Top = this.ClientSize.Height - button.Height - space_border;
				button.Left =  ((button_width + button_space) * left) + button_left;
				
				if (click_event != null)
					button.Click += click_event;
				
				if ((text == "OK") || (text == "Retry") || (text == "Yes")) 
					AcceptButton = button;
				else if ((text == "Cancel") || (text == "Abort") || (text == "No"))
					CancelButton = button;
				
				this.Controls.Add (button);

				return button;
			}

			private Button AddOkButton (int left)
			{
				return AddButton ("OK", left, new EventHandler (OkClick));
			}

			private Button AddCancelButton (int left)
			{
				return AddButton ("Cancel", left, new EventHandler (CancelClick));
			}

			private Button AddAbortButton (int left)
			{
				return AddButton ("Abort", left, new EventHandler (AbortClick));
			}

			private Button AddRetryButton(int left)
			{
				return AddButton ("Retry", left, new EventHandler (RetryClick));
			}

			private Button AddIgnoreButton (int left)
			{
				return AddButton ("Ignore", left, new EventHandler (IgnoreClick));
			}

			private Button AddYesButton (int left)
			{
				return AddButton ("Yes", left, new EventHandler (YesClick));
			}

			private Button AddNoButton (int left)
			{
				return AddButton ("No", left, new EventHandler (NoClick));
			}

			private Button AddHelpButton (int left)
			{
				Button button = AddButton ("Help", left, null);
				button.Click += delegate { Owner.RaiseHelpRequested (new HelpEventArgs (Owner.Location)); };
				return button;
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

			#region UIA Framework: Methods, Properties and Events

			internal string UIAMessage {
				get { return msgbox_text; }
			}

			internal Rectangle UIAMessageRectangle {
				get { 
					return new Rectangle ((int) text_rect.X,
					                      (int) text_rect.Y, 
					                      (int) text_rect.Width, 
					                      (int) text_rect.Height); 
				}
			}

			internal Rectangle UIAIconRectangle {
				get { 
					return new Rectangle (space_border, 
					                      space_border, 
							      icon_image == null ? -1 : icon_image.Width, 
							      icon_image == null ? -1 : icon_image.Height);
				}
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

			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption,
						 MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, MessageBoxOptions.DefaultDesktopOnly, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, false);
				
			return form.RunDialog ();
		}

		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, false);
				
			return form.RunDialog ();
		}
		#endregion	// Public Static Methods

		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 bool displayHelpButton)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, displayHelpButton);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, string keyword)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, keyword, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator, object param)
		{
			MessageBoxForm form = new MessageBoxForm (null, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, param);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, string keyword)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, keyword, HelpNavigator.TableOfContents, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, null);
			return form.RunDialog ();
		}
		
		[MonoTODO ("Help is not implemented")]
		public static DialogResult Show (IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon,
						 MessageBoxDefaultButton defaultButton, MessageBoxOptions options,
						 string helpFilePath, HelpNavigator navigator, object param)
		{
			MessageBoxForm form = new MessageBoxForm (owner, text, caption, buttons,
								  icon, defaultButton, options, true);
			form.SetHelpData (helpFilePath, null, navigator, param);
			return form.RunDialog ();
		}
	}
}

