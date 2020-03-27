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

using System.Drawing;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;

namespace System.Windows.Forms
{
	[DefaultProperty("Text")]
	[DefaultEvent("Enter")]
	[Designer ("System.Windows.Forms.Design.GroupBoxDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class GroupBox : Control
	{
		private FlatStyle flat_style;
		private Rectangle display_rectangle = new Rectangle ();

		#region Events
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseClick {
			add { base.MouseClick += value; }
			remove { base.MouseClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
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

			can_cache_preferred_size = true;
		}

		#region Public Properties
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public override bool AllowDrop {
			get { return base.AllowDrop;  }
			set { base.AllowDrop = value; }
		}

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (true)]
		[DefaultValue (AutoSizeMode.GrowOnly)]
		[Localizable (true)]
		public AutoSizeMode AutoSizeMode {
			get { return base.GetAutoSizeMode (); }
			set { base.SetAutoSizeMode (value); }
		}

		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.GroupBoxDefaultSize;}
		}

		public override Rectangle DisplayRectangle {
			get {
				display_rectangle.X = Padding.Left;
				display_rectangle.Y = Font.Height + Padding.Top;
				display_rectangle.Width = Width - Padding.Horizontal;
				display_rectangle.Height = Height - Font.Height - Padding.Vertical;
				return display_rectangle;
			}
		}

		[DefaultValue(FlatStyle.Standard)]
		public FlatStyle FlatStyle {
			get { return flat_style; }
			set {
				if (!Enum.IsDefined (typeof (FlatStyle), value))
					 throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for FlatStyle", value));

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
		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new GroupBoxAccessibleObject (this);
		}
		
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
			Refresh ();
		}

		protected override void OnPaint (PaintEventArgs e)
		{
			ThemeEngine.Current.DrawGroupBox (e.Graphics, ClientRectangle, this);
			base.OnPaint(e);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			if (IsMnemonic(charCode, Text) == true) {
				// Select item next in line in tab order
				if (this.Parent != null) {
					Parent.SelectNextControl(this, true, false, true, false);
				}
				return true;
			}
			
			return base.ProcessMnemonic (charCode);
		}

		protected override void ScaleControl (SizeF factor, BoundsSpecified specified)
		{
			base.ScaleControl (factor, specified);
		}

		public override string ToString()
		{
			return GetType ().FullName + ", Text: " + Text;
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
				
		#endregion Public Methods

		[DefaultValue (false)]
		public bool UseCompatibleTextRendering {
			get {
				return use_compatible_text_rendering;
			}

			set {
				if (use_compatible_text_rendering != value) {
					use_compatible_text_rendering = value;
					if (Parent != null)
						Parent.PerformLayout (this, "UseCompatibleTextRendering");
					Invalidate ();
				}
			}
		}

		#region Protected Properties
		protected override Padding DefaultPadding {
			get { return new Padding (3); }
		}
		#endregion

		#region Internal Methods
		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			// (Copied from Panel)
			// Translating 0, 0 from ClientSize to actual Size tells us how much space
			// is required for the borders.
			Size borderSize = SizeFromClientSize(Size.Empty);
			Size totalPadding = borderSize + Padding.Size;
			return LayoutEngine.GetPreferredSize(this, proposedSize - totalPadding) + totalPadding;
		}
		#endregion

		#region Private Classes
		private class GroupBoxAccessibleObject : Control.ControlAccessibleObject
		{
			public GroupBoxAccessibleObject (Control owner) : base (owner)
			{
			}
		}
		#endregion
	}
}
