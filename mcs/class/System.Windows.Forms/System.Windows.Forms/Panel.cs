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
//	Jackson Harper (jackson@ximian.com)
//

// COMPLETE

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	[DefaultProperty("BorderStyle")]
	[DefaultEvent("Paint")]
	[Designer ("System.Windows.Forms.Design.PanelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[Docking (DockingBehavior.Ask)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class Panel : ScrollableControl {
		#region	Constructors & Destructors
		public Panel () {
			base.TabStop = false;
			SetStyle(ControlStyles.Selectable, false);
			SetStyle (ControlStyles.SupportsTransparentBackColor, true);
			can_cache_preferred_size = true;
		}
		#endregion	// Constructors & Destructors

		#region Public Instance Properties
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
		public virtual AutoSizeMode AutoSizeMode {
			get { return base.GetAutoSizeMode (); }
			set { base.SetAutoSizeMode (value); }
		}

		[DefaultValue(BorderStyle.None)]
		[DispId(-504)]
		public BorderStyle BorderStyle {
			get { return InternalBorderStyle; }
			set { InternalBorderStyle = value; }
		}

		[DefaultValue(false)]
		public new bool TabStop {
			get { return base.TabStop; }
			set {
				if (value == TabStop)
					return;
				base.TabStop = value;
			}
		}

		[Bindable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override string Text {
			get { return base.Text; }
			set {
				if (value == Text)
					return;
				base.Text = value;
				Refresh ();
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get { return ThemeEngine.Current.PanelDefaultSize; }
		}
		#endregion	// Proteced Instance Properties

		#region Public Instance Methods
		public override string ToString ()
		{
			return base.ToString () + ", BorderStyle: " + BorderStyle;
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void OnResize(EventArgs eventargs) {
			base.OnResize (eventargs);
			Invalidate(true);
		}

		#endregion	// Protected Instance Methods

		#region Events
		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion

		#region Internal Methods
		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			// Translating 0, 0 from ClientSize to actual Size tells us how much space
			// is required for the borders.
			Size borderSize = SizeFromClientSize(Size.Empty);
			Size totalPadding = borderSize + Padding.Size;
			return LayoutEngine.GetPreferredSize(this, proposedSize - totalPadding) + totalPadding;
		}
		#endregion
	}
}

