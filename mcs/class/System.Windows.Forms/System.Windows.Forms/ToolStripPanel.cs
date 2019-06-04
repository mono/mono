//
// ToolStripPanel.cs
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


using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ToolboxBitmap ("")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[Designer ("System.Windows.Forms.Design.ToolStripPanelDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class ToolStripPanel : ContainerControl, IComponent, IDisposable, IBindableComponent, IDropTarget
	{
		private bool done_first_layout;
		private bool locked;
		private Orientation orientation;
		private ToolStripRenderer renderer;
		private ToolStripRenderMode render_mode;
		private Padding row_margin;
		private ToolStripPanelRowCollection rows;
		
		public ToolStripPanel () : base ()
		{
			base.AutoSize = true;
			this.locked = false;
			this.renderer = null;
			this.render_mode = ToolStripRenderMode.ManagerRenderMode;
			this.row_margin = new Padding (3, 0, 0, 0);
			this.rows = new ToolStripPanelRowCollection (this);
		}

		#region Public Properties
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AllowDrop {
			get { return base.AllowDrop; }
			set { base.AllowDrop = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override bool AutoScroll {
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMargin {
			get { return base.AutoScrollMargin; }
			set { base.AutoScrollMargin = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new Size AutoScrollMinSize {
			get { return base.AutoScrollMinSize; }
			set { base.AutoScrollMinSize = value; }
		}

		[DefaultValue (true)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		public override bool AutoSize {
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				base.Dock = value;

				switch (value) {
					case DockStyle.Top:
					case DockStyle.Bottom:
					case DockStyle.None:
						this.orientation = Orientation.Horizontal;
						break;
					case DockStyle.Left:
					case DockStyle.Right:
						this.orientation = Orientation.Vertical;
						break;
				}
			}
		}

		public override LayoutEngine LayoutEngine {
			get { 
				return System.Windows.Forms.Layout.FlowLayout.Instance;
			}
		}

		[Browsable (false)]
		[DefaultValue (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool Locked {
			get { return this.locked; }
			set { this.locked = value; }
		}

		public Orientation Orientation {
			get { return this.orientation; }
			set { this.orientation = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ToolStripRenderer Renderer {
			get {
				if (this.render_mode == ToolStripRenderMode.ManagerRenderMode)
					return ToolStripManager.Renderer;

				return this.renderer;
			}
			set {
				if (this.renderer != value) {
					this.renderer = value;
					this.render_mode = ToolStripRenderMode.Custom;
					this.OnRendererChanged (EventArgs.Empty);
				}
			}
		}

		public ToolStripRenderMode RenderMode {
			get { return this.render_mode; }
			set {
				if (!Enum.IsDefined (typeof (ToolStripRenderMode), value))
					throw new InvalidEnumArgumentException (string.Format ("Enum argument value '{0}' is not valid for ToolStripRenderMode", value));

				if (value == ToolStripRenderMode.Custom && this.renderer == null)
					throw new NotSupportedException ("Must set Renderer property before setting RenderMode to Custom");
				if (value == ToolStripRenderMode.Professional || value == ToolStripRenderMode.System)
					this.Renderer = new ToolStripProfessionalRenderer ();

				this.render_mode = value;
			}
		}
		
		public Padding RowMargin {
			get { return this.row_margin; }
			set { this.row_margin = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ToolStripPanelRow[] Rows {
			get { 
				ToolStripPanelRow[] retval = new ToolStripPanelRow [this.rows.Count];
				this.rows.CopyTo (retval, 0); 
				return retval;	
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new int TabIndex {
			get { return base.TabIndex; }
			set { base.TabIndex = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public new bool TabStop {
			get { return base.TabStop; }
			set { base.TabStop = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}
		#endregion

		#region Protected Properties
		protected override Padding DefaultMargin {
			get { return new Padding (0); }
		}

		protected override Padding DefaultPadding {
			get { return new Padding (0); }
		}
		#endregion

		#region Public Methods
		public void BeginInit ()
		{
		}
		
		public void EndInit ()
		{
		}

		[MonoTODO("Not implemented")]
		public void Join (ToolStrip toolStripToDrag)
		{
			if (!Contains (toolStripToDrag))
				Controls.Add (toolStripToDrag);
		}

		[MonoTODO("Not implemented")]
		public void Join (ToolStrip toolStripToDrag, int row)
		{
			Join (toolStripToDrag);
		}

		[MonoTODO("Not implemented")]
		public void Join (ToolStrip toolStripToDrag, Point location)
		{
			Join (toolStripToDrag);
		}

		[MonoTODO("Not implemented")]
		public void Join (ToolStrip toolStripToDrag, int x, int y)
		{
			Join (toolStripToDrag);
		}
		
		public ToolStripPanelRow PointToRow (Point clientLocation)
		{
			foreach (ToolStripPanelRow row in this.rows)
				if (row.Bounds.Contains (clientLocation))
					return row;
					
			return null;
		}
		#endregion

		#region Protected Methods
		protected override ControlCollection CreateControlsInstance ()
		{
			return new ToolStripPanelControlCollection (this);
		}
		
		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}

		protected override void OnControlAdded (ControlEventArgs e)
		{
			if (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right)
				(e.Control as ToolStrip).LayoutStyle = ToolStripLayoutStyle.VerticalStackWithOverflow;
			else
				(e.Control as ToolStrip).LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			
			if (done_first_layout && e.Control is ToolStrip)
				this.AddControlToRows (e.Control);
			
			base.OnControlAdded (e);
		}

		protected override void OnControlRemoved (ControlEventArgs e)
		{
			base.OnControlRemoved (e);
			
			foreach (ToolStripPanelRow row in this.rows)
				if (row.controls.Contains (e.Control))
					row.OnControlRemoved (e.Control, 0);
		}

		protected override void OnDockChanged (EventArgs e)
		{
			base.OnDockChanged (e);
		}
		
		protected override void OnLayout (LayoutEventArgs e)
		{
			// Don't see any reason to layout if we aren't created
			if (!this.Created)
				return;
				
			// The first time through, we have to layout everything where it belongs.
			// The key is that when we resize and stuff, we don't resize or move toolstrips.
			if (!done_first_layout) {
				ArrayList al = new ArrayList (this.Controls);
				al.Sort (new TabIndexComparer ());
				
				foreach (ToolStrip ts in al)
					this.AddControlToRows (ts);
					
				done_first_layout = true;
			}
			
			// Lay out all the rows
			Point position = this.DisplayRectangle.Location;

			if (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right) {
				foreach (ToolStripPanelRow row in this.rows) {
					row.SetBounds (new Rectangle (position, new Size (row.Bounds.Width, this.Height)));

					position.X += row.Bounds.Width;
				}

				// Find how big we are so we can autosize ourself
				if (this.rows.Count > 0) {
					int last_row_right = this.rows[this.rows.Count - 1].Bounds.Right;

					if (last_row_right != this.Width)
						this.SetBounds (bounds.X, bounds.Y, last_row_right, bounds.Bottom);
				}
			} else {
				foreach (ToolStripPanelRow row in this.rows) {
					row.SetBounds (new Rectangle (position, new Size (this.Width, row.Bounds.Height)));

					position.Y += row.Bounds.Height;
				}

				// Find how big we are so we can autosize ourself
				if (this.rows.Count > 0) {
					int last_row_bottom = this.rows[this.rows.Count - 1].Bounds.Bottom;
				
					if (last_row_bottom != this.Height)
						this.SetBounds (bounds.X, bounds.Y, bounds.Width, last_row_bottom);
				}
			}
			
			this.Invalidate ();
			
			return;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);

			this.Renderer.DrawToolStripPanelBackground (new ToolStripPanelRenderEventArgs (e.Graphics, this));
		}

		protected override void OnParentChanged (EventArgs e)
		{
			base.OnParentChanged (e);
		}
		
		protected virtual void OnRendererChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [RendererChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnRightToLeftChanged (EventArgs e)
		{
			base.OnRightToLeftChanged (e);
		}
		#endregion

		#region Public Events
		static object RendererChangedEvent = new object ();

		[Browsable (true)]
		[EditorBrowsable (EditorBrowsableState.Always)]
		public new event EventHandler AutoSizeChanged {
			add { base.AutoSizeChanged += value; }
			remove { base.AutoSizeChanged -= value; }
		}

		public event EventHandler RendererChanged {
			add { Events.AddHandler (RendererChangedEvent, value); }
			remove { Events.RemoveHandler (RendererChangedEvent, value); }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabIndexChanged {
			add { base.TabIndexChanged += value; }
			remove { base.TabIndexChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TabStopChanged {
			add { base.TabStopChanged += value; }
			remove { base.TabStopChanged -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion

		#region Private Methods

		private void AddControlToRows (Control control)
		{
			if (this.rows.Count > 0)
				if (this.rows[this.rows.Count - 1].CanMove ((ToolStrip)control)) {
					this.rows[this.rows.Count - 1].OnControlAdded (control, 0);
					return;
				}

			ToolStripPanelRow new_row = new ToolStripPanelRow (this);
			
			if (this.Dock == DockStyle.Left || this.Dock == DockStyle.Right)
				new_row.SetBounds (new Rectangle (0, 0, 25, this.Height));
			else
				new_row.SetBounds (new Rectangle (0, 0, this.Width, 25));
				
			this.rows.Add (new_row);
			new_row.OnControlAdded (control, 0);
		}
		/*
		private Region FindBackgroundRegion ()
		{
			Region r = new Region (this.Bounds);

			foreach (Control c in this.Controls)
				r.Exclude (c.Bounds);

			return r;
		}
		*/
		#endregion

		#region Nested Classes
		[ComVisible (false)]
		[ListBindable (false)]
		public class ToolStripPanelRowCollection : ArrangedElementCollection, IList, ICollection, IEnumerable
		{
			//private ToolStripPanel owner;
			
			public ToolStripPanelRowCollection (ToolStripPanel owner) : base ()
			{
				//this.owner = owner;
			}
			
			public ToolStripPanelRowCollection (ToolStripPanel owner, ToolStripPanelRow[] value) : this (owner)
			{
				if (value != null)
					foreach (ToolStripPanelRow tspr in value)
						this.Add (tspr);
			}
			
			public new virtual ToolStripPanelRow this [int index] {
				get { return (ToolStripPanelRow)base[index]; }
			}

			#region Public Methods
			public int Add (ToolStripPanelRow value)
			{
				return base.Add (value);
			}
			
			public void AddRange (ToolStripPanelRowCollection value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				foreach (ToolStripPanelRow tspr in value)
					this.Add (tspr);
			}
			
			public void AddRange (ToolStripPanelRow[] value)
			{
				if (value == null)
					throw new ArgumentNullException ("value");

				foreach (ToolStripPanelRow tspr in value)
					this.Add (tspr);
			}
			
			public new virtual void Clear ()
			{
				base.Clear ();
			}
			
			public bool Contains (ToolStripPanelRow value)
			{
				return base.Contains (value);
			}
			
			public void CopyTo (ToolStripPanelRow[] array, int index)
			{
				base.CopyTo (array, index);
			}
			
			public int IndexOf (ToolStripPanelRow value)
			{
				return base.IndexOf (value);
			}
			
			public void Insert (int index, ToolStripPanelRow value)
			{
				base.Insert (index, value);
			}
			
			public void Remove (ToolStripPanelRow value)
			{
				base.Remove (value);
			}
			
			public void RemoveAt (int index)
			{
				base.InternalRemoveAt (index);
			}
			#endregion

			#region IList Members
			object IList.this [int index] {
				get { return this [index]; }
				[MonoTODO ("Stub, does nothing")]
				set { }
			}

			bool IList.IsFixedSize {
				get { return IsFixedSize; }
			}

			bool IList.IsReadOnly {
				get { return IsReadOnly; }
			}

			int IList.Add (object value)
			{
				return Add (value as ToolStripPanelRow);
			}

			void IList.Clear ()
			{
				Clear ();
			}

			bool IList.Contains (object value)
			{
				return Contains (value as ToolStripPanelRow);
			}

			int IList.IndexOf (object value)
			{
				return IndexOf (value as ToolStripPanelRow);
			}

			void IList.Insert (int index, object value)
			{
				Insert (index, value as ToolStripPanelRow);
			}

			void IList.Remove (object value)
			{
				Remove (value as ToolStripPanelRow);
			}

			void IList.RemoveAt (int index)
			{
				base.InternalRemoveAt (index);
			}
			#endregion
		}
		
		private class ToolStripPanelControlCollection : ControlCollection
		{
			public ToolStripPanelControlCollection (Control owner) : base (owner)
			{
			}
		}

		private class TabIndexComparer : IComparer
		{
			#region IComparer Members
			public int Compare (object x, object y)
			{
				if (!(x is Control) || !(y is Control))
					throw new ArgumentException ();

				return (x as Control).TabIndex - (y as Control).TabIndex;
			}
			#endregion
		}
		#endregion
	}
}
