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
// Authors:
//		Jordi Mas i Hernandez, jordi@ximian.com
//
// TODO:
//
// Copyright (C) Novell Inc., 2004-2005
//
//

// COMPLETE

using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Windows.Forms
{
	[DefaultProperty("Text")]
	[DefaultEvent("Enter")]
	[Designer ("System.Windows.Forms.Design.GroupBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class GroupBox : Control
	{
		private FlatStyle flat_style;
		private Rectangle display_rectangle = new Rectangle ();

		#region Events
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler Click {
			add { base.Click += value; }
			remove { base.Click -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseDown {
			add { base.MouseDown += value; }
			remove { base.MouseDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler MouseEnter {
			add { base.MouseEnter += value; }
			remove { base.MouseEnter -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler MouseLeave {
			add { base.MouseLeave += value; }
			remove { base.MouseLeave -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseMove {
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseUp {
			add { base.MouseUp += value; }
			remove { base.MouseUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}
		#endregion Events

		public GroupBox ()
		{
			TabStop = false;
			flat_style = FlatStyle.Standard;

			SetStyle(ControlStyles.ContainerControl | ControlStyles.ResizeRedraw | ControlStyles.SupportsTransparentBackColor, true);
			SetStyle(ControlStyles.Selectable, false);
		}

		#region Public Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override bool AllowDrop {
			get { return base.AllowDrop;  }
			set { base.AllowDrop = value; }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.GroupBoxDefaultSize;}
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

		[DefaultValue(FlatStyle.Standard)]
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

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new bool TabStop {
			get { return base.TabStop;  }
			set { base.TabStop = value; }
		}

		[Localizable(true)]
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
			ThemeEngine.Current.DrawGroupBox (pevent.Graphics, ClientRectangle, this);
			base.OnPaint(pevent);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			if (IsMnemonic(charCode, Text) == true) {
				// Select item next in line in tab order
				if (this.parent != null) {
					parent.SelectNextControl(this, true, false, true, false);
				}
				return true;
			}
			
			return base.ProcessMnemonic (charCode);
		}

		public override string ToString()
		{
			return GetType ().FullName.ToString () + ", Text: " + Text;
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
				
		#endregion Public Methods
#if NET_2_0

		public bool UseCompatibleTextRendering {
			get {
				return use_compatible_text_rendering;
			}

			set {
				use_compatible_text_rendering = value;
			}
		}
#endif
	}
}
