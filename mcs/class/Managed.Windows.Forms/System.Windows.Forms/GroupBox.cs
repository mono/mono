//
// System.Windows.Forms.GroupBox.cs
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
// Autors:
//		Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//
// Copyright (C) Novell Inc., 2004
//
// $CVS$
//
//

// COMPLETE

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class GroupBox : Control
	{
		private FlatStyle flat_style;
		private Bitmap bmp_mem = null;
		private Graphics dc_mem = null;
		private Rectangle display_rectangle = new Rectangle ();

		#region Events
		public new event EventHandler Click;
		public new event EventHandler DoubleClick;
		public new event KeyEventHandler KeyDown;
		public new event KeyEventHandler KeyPress;
		public new event KeyEventHandler KeyUp;
		public new event MouseEventHandler MouseDown;
		public new event MouseEventHandler MouseEnter;
		public new event MouseEventHandler MouseLeave;
		public new event MouseEventHandler MouseMove;
		public new event MouseEventHandler MouseUp;
		public new event EventHandler TabStopChanged;
		#endregion Events

		public GroupBox ()
		{
			TabStop = false;
			flat_style = FlatStyle.Standard;
		}

		#region Public Properties
		public override bool AllowDrop {
			get { return base.AllowDrop;  }
			set { base.AllowDrop = value; }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return new Size (200,100); }
		}

		public override Rectangle DisplayRectangle {
			get {
				display_rectangle.X = 3;
				display_rectangle.Y = Font.Height + 3;
				display_rectangle.Width = Width - 6;
				display_rectangle.Height = Height - Font.Height - 6;
				return display_rectangle;
			}
		}

		public FlatStyle FlatStyle {
			get { return flat_style; }
			set {
				if (!Enum.IsDefined (typeof (FlatStyle), value))
					 new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for FlatStyle", value));

				if (flat_style == value)
					return;
					
				flat_style = value;
				Refresh ();
			}
		}

		public new bool TabStop {
			get { return base.TabStop;  }
			set { base.TabStop = value; }
		}

		public override string Text {
			get { return base.Text; }
			set {
				if (base.Text == value)
					return;

				base.Text = value;
				Refresh ();
			}
		}

		#endregion //Public Properties

		#region Public Methods
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Refresh ();
		}

		protected override void OnPaint (PaintEventArgs pevent)
		{
			Draw ();
			pevent.Graphics.DrawImage (ImageBuffer, 0, 0);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			return base.ProcessMnemonic(charCode);
		}

		public override string ToString()
		{
			return GetType ().FullName.ToString () + ", Text: " + Text;
		}

		protected override void WndProc(ref Message m) {
			switch ((Msg) m.Msg) {
				case Msg.WM_ERASEBKGND:
					m.Result = (IntPtr)1;
					break;
				default:
					base.WndProc (ref m);
					break;
			}
		}
				
		#endregion Public Methods
		
		#region Private Methods

		private void Draw ()
		{			
			ThemeEngine.Current.DrawGroupBox (DeviceContext, ClientRectangle, this);			
		}
		
		#endregion // Private Methods
	}
}
