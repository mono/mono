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
using System.ComponentModel;

namespace System.Windows.Forms.PropertyGridInternal 
{
	internal class PGTextBox : TextBox 
	{
		private bool _focusing = false;

		public void FocusAt (Point location)
		{
			_focusing = true;
			Point pnt = PointToClient (location);
			XplatUI.SendMessage (Handle, Msg.WM_LBUTTONDOWN, new IntPtr ((int)MsgButtons.MK_LBUTTON), Control.MakeParam (pnt.X, pnt.Y));
		}

		protected override bool IsInputKey (Keys keyData)
		{
			// To be handled by the PropertyGridView
			if ((keyData & Keys.Alt) != 0 && 
			    (keyData & Keys.KeyCode) == Keys.Down)
				return true;
			return base.IsInputKey (keyData);
		}

		protected override void WndProc (ref Message m)
		{
			// Swallow the first MOUSEMOVE after the focusing WM_LBUTTONDOWN
			if (_focusing && m.Msg == (int)Msg.WM_MOUSEMOVE) {
				_focusing = false;
				return;
			}
			base.WndProc (ref m);
		}
	}
	
	internal class PropertyGridTextBox : System.Windows.Forms.UserControl, IMessageFilter
	{
		#region Private Members

		private PGTextBox textbox;
		private Button dialog_button;
		private Button dropdown_button;
		private bool validating = false;
		private bool filtering = false;

		#endregion Private Members

		#region Contructors
		public PropertyGridTextBox() {
			dialog_button = new Button();
			dropdown_button = new Button();
			textbox = new PGTextBox ();

			SuspendLayout();

			dialog_button.Dock = DockStyle.Right;
			dialog_button.BackColor = SystemColors.Control;
			dialog_button.Size = new Size(16, 16);
			dialog_button.TabIndex = 1;
			dialog_button.Visible = false;
			dialog_button.Click += new System.EventHandler(dialog_button_Click);

			dropdown_button.Dock = DockStyle.Right;
			dropdown_button.BackColor = SystemColors.Control;
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

			SetStyle (ControlStyles.Selectable, true);

			ResumeLayout(false);

			dropdown_button.Paint+=new PaintEventHandler(dropdown_button_Paint);
			dialog_button.Paint+=new PaintEventHandler(dialog_button_Paint);
			textbox.DoubleClick+=new EventHandler(textbox_DoubleClick);
			textbox.KeyDown+=new KeyEventHandler(textbox_KeyDown);
			textbox.GotFocus+=new EventHandler(textbox_GotFocus);
		}

		
		#endregion Contructors

		#region Protected Instance Properties

		protected override void OnGotFocus (EventArgs args)
		{
			base.OnGotFocus (args);
			// force-disable selection
			textbox.has_been_focused = true;
			textbox.Focus ();
			textbox.SelectionLength = 0;
		}

		#endregion

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

		public new Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				textbox.ForeColor = value;
				dropdown_button.ForeColor = value;
				dialog_button.ForeColor = value;
				base.ForeColor = value;
			}
		}

		public new Color BackColor {
			get {
				return base.BackColor;
			}
			set {
				textbox.BackColor = value;
				base.BackColor = value;
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

		public char PasswordChar {
			set { textbox.PasswordChar = value; }
		}

		#endregion Public Instance Properties
		
		#region Events
		static object DropDownButtonClickedEvent = new object ();
		static object DialogButtonClickedEvent = new object ();
		static object ToggleValueEvent = new object ();
		static object KeyDownEvent = new object ();
		static object ValidateEvent = new object ();

		public event EventHandler DropDownButtonClicked {
			add { Events.AddHandler (DropDownButtonClickedEvent, value); }
			remove { Events.RemoveHandler (DropDownButtonClickedEvent, value); }
		}

		public event EventHandler DialogButtonClicked {
			add { Events.AddHandler (DialogButtonClickedEvent, value); }
			remove { Events.RemoveHandler (DialogButtonClickedEvent, value); }
		}

		public event EventHandler ToggleValue {
			add { Events.AddHandler (ToggleValueEvent, value); }
			remove { Events.RemoveHandler (ToggleValueEvent, value); }
		}

		public new event KeyEventHandler KeyDown {
			add { Events.AddHandler (KeyDownEvent, value); }
			remove { Events.RemoveHandler (KeyDownEvent, value); }
		}
		
		public new event CancelEventHandler Validate {
			add { Events.AddHandler (ValidateEvent, value); }
			remove { Events.RemoveHandler (ValidateEvent, value); }
		}
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

		private void dropdown_button_Click(object sender, EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DropDownButtonClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void dialog_button_Click(object sender, EventArgs e) {
			EventHandler eh = (EventHandler)(Events [DialogButtonClickedEvent]);
			if (eh != null)
				eh (this, e);
		}

		#endregion Private Helper Methods

		internal void SendMouseDown (Point screenLocation)
		{
			Point clientLocation = PointToClient (screenLocation);
			XplatUI.SendMessage (Handle, Msg.WM_LBUTTONDOWN, new IntPtr ((int) MsgButtons.MK_LBUTTON), Control.MakeParam (clientLocation.X, clientLocation.Y));
			textbox.FocusAt (screenLocation);
		}	

		private void textbox_DoubleClick(object sender, EventArgs e) {
			EventHandler eh = (EventHandler)(Events [ToggleValueEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void textbox_KeyDown(object sender, KeyEventArgs e) {
			KeyEventHandler eh = (KeyEventHandler)(Events [KeyDownEvent]);
			if (eh != null)
				eh (this, e);
		}

		private void textbox_GotFocus(object sender, EventArgs e) {
			if (!filtering) {
				filtering = true;
				Application.AddMessageFilter ((IMessageFilter)this);
			}
		}


		protected override void DestroyHandle ()
		{
			Application.RemoveMessageFilter ((IMessageFilter)this);
			filtering = false;
			base.DestroyHandle ();
		}

		bool IMessageFilter.PreFilterMessage(ref Message m)
		{
			// validating check is to allow whatever UI code to execute
			// without filtering messages (i.e. error dialogs, etc)
			//
			if (!validating && m.HWnd != textbox.Handle && textbox.Focused &&
			    (m.Msg == (int)Msg.WM_LBUTTONDOWN || 
			     m.Msg == (int)Msg.WM_MBUTTONDOWN ||
			     m.Msg == (int)Msg.WM_RBUTTONDOWN ||
			     m.Msg == (int)Msg.WM_NCLBUTTONDOWN ||
			     m.Msg == (int)Msg.WM_NCMBUTTONDOWN ||
			     m.Msg == (int)Msg.WM_NCRBUTTONDOWN)) {
				CancelEventHandler validateHandler = (CancelEventHandler)(Events [ValidateEvent]);
				if (validateHandler != null) {
					CancelEventArgs args = new CancelEventArgs ();
					validating = true;
					validateHandler (this, args);
					validating = false;
					if (!args.Cancel) {
						Application.RemoveMessageFilter ((IMessageFilter)this);
						filtering = false;
					}
					return args.Cancel;
				}
			}
			return false;
		}
	}
}
