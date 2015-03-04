//
// SplitterPanel.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace System.Windows.Forms
{
	[ToolboxItem (false)]
	[ComVisibleAttribute (true)]
	[ClassInterfaceAttribute (ClassInterfaceType.AutoDispatch)]
	[Docking (DockingBehavior.Never)]
	[Designer ("System.Windows.Forms.Design.SplitterPanelDesigner, " + Consts.AssemblySystem_Design)]
	public sealed class SplitterPanel : Panel
	{
		//private SplitContainer owner;

		public SplitterPanel (SplitContainer owner)
		{
			//this.owner = owner;
		}

		#region Public Properties
		// All of these are overriden just to hide them from the IDE  :/
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public AnchorStyles Anchor {
			get { return base.Anchor; }
			set { base.Anchor = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Localizable(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override AutoSizeMode AutoSizeMode {
			get { return base.AutoSizeMode; }
			set { } // Does not change, always GrowOnly
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public DockStyle Dock {
			get { return base.Dock; }
			set { base.Dock = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public DockPaddingEdges DockPadding {
			get { return base.DockPadding; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public int Height {
			get { return this.Visible ? base.Height : 0; }
			set { throw new NotSupportedException ("The height cannot be set"); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Point Location {
			get { return base.Location; }
			set { base.Location = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Size MaximumSize {
			get { return base.MaximumSize; }
			set { base.MaximumSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Size MinimumSize {
			get { return base.MinimumSize; }
			set { base.MinimumSize = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public string Name {
			get { return base.Name; }
			set { base.Name = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Control Parent {
			get { return base.Parent; }
			set { throw new NotSupportedException ("The parent cannot be set"); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public Size Size {
			get { return base.Size; }
			set { base.Size = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public bool Visible {
			get { return base.Visible; }
			set { base.Visible = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		new public int Width {
			get { return this.Visible ? base.Width : 0; }
			set { throw new NotSupportedException ("The width cannot be set"); }
		}
		#endregion

		#region Public Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler DockChanged {
			add { base.DockChanged += value; }
			remove { base.DockChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler LocationChanged {
			add { base.LocationChanged += value; }
			remove { base.LocationChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new event EventHandler VisibleChanged {
			add { base.VisibleChanged += value; }
			remove { base.VisibleChanged -= value; }
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultMargin { get { return new Padding (0); } }
		#endregion
		
		#region Internal Properties
		internal int InternalHeight { set { base.Height = value; } }
		internal int InternalWidth { set { base.Width = value; } }
		#endregion
	}
}
