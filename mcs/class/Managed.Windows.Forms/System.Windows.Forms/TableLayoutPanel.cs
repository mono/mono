//
// TableLayoutPanel.cs
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

#if NET_2_0
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms
{
	[ComVisible (true)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ProvideProperty ("CellPosition", typeof (Control))]
	[ProvideProperty ("Column", typeof (Control))]
	[ProvideProperty ("ColumnSpan", typeof (Control))]
	[ProvideProperty ("Row", typeof (Control))]
	[ProvideProperty ("RowSpan", typeof (Control))]
	[DefaultProperty ("ColumnCount")]
	[Docking (DockingBehavior.Never)]
	public class TableLayoutPanel : Panel, IExtenderProvider
	{
		private TableLayoutSettings settings;
		private static TableLayout layout_engine = new TableLayout ();
		private TableLayoutPanelCellBorderStyle cell_border_style;

		// This is the row/column the Control actually got placed
		internal Control[,] actual_positions;
		
		// Widths and heights of each column/row
		internal int[] column_widths;
		internal int[] row_heights;

		#region Public Constructor
		public TableLayoutPanel ()
		{
			settings = new TableLayoutSettings(this);
			cell_border_style = TableLayoutPanelCellBorderStyle.None;
		}
		#endregion

		#region Public Properties
		[Localizable (true)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		new public BorderStyle BorderStyle {
			get { return base.BorderStyle; }
			set { base.BorderStyle = value; }
		}

		[Localizable (true)]
		[DefaultValue (TableLayoutPanelCellBorderStyle.None)]
		public TableLayoutPanelCellBorderStyle CellBorderStyle {
			get { return this.cell_border_style; }
			set { this.cell_border_style = value; }
		}

		[Localizable (true)]
		[DefaultValue (0)]
		public int ColumnCount {
			get { return settings.ColumnCount; }
			set { settings.ColumnCount = value; }
		}

		[Browsable (false)]
		[DisplayName ("Columns")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public TableLayoutColumnStyleCollection ColumnStyles {
			get { return settings.ColumnStyles; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		new public TableLayoutControlCollection Controls {
			get { return (TableLayoutControlCollection) base.Controls; }
		}

		[DefaultValue (TableLayoutPanelGrowStyle.AddRows)]
		public TableLayoutPanelGrowStyle GrowStyle {
			get { return settings.GrowStyle; }
			set { settings.GrowStyle = value; }
		}

		public override System.Windows.Forms.Layout.LayoutEngine LayoutEngine {
			get { return TableLayoutPanel.layout_engine; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TableLayoutSettings LayoutSettings {
			get { return this.settings; }
			set {
				if (value.isSerialized) {
					this.settings = value;
					value.isSerialized = false;
				} else
					throw new NotSupportedException ("LayoutSettings value cannot be set directly.");
			}
		}

		[Localizable (true)]
		[DefaultValue (0)]
		public int RowCount {
			get { return settings.RowCount; }
			set { this.settings.RowCount = value; }
		}

		[Browsable (false)]
		[DisplayName ("Rows")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableLayoutRowStyleCollection RowStyles {
			get { return settings.RowStyles; }
		}
		#endregion

		#region Public Methods
		[DefaultValue (-1)]
		[DisplayName ("Cell")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public TableLayoutPanelCellPosition GetCellPosition (Control control)
		{
			return settings.GetCellPosition (control);
		}

		[DisplayName ("Column")]
		[DefaultValue (-1)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int GetColumn (Control control)
		{
			return settings.GetColumn (control);
		}

		[DisplayName ("ColumnSpan")]
		[DefaultValue (1)]
		public int GetColumnSpan (Control control)
		{
			return settings.GetColumnSpan (control);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int[] GetColumnWidths ()
		{
			return this.column_widths;
		}

		public Control GetControlFromPosition (int column, int row)
		{
			if (column < 0 || row < 0)
				throw new ArgumentException ();

			TableLayoutPanelCellPosition pos = new TableLayoutPanelCellPosition (column, row);

			foreach (Control c in this.Controls)
				if (settings.GetCellPosition (c) == pos)
					return c;

			return null;
		}

		public TableLayoutPanelCellPosition GetPositionFromControl (Control control)
		{
			for (int x = 0; x < this.actual_positions.GetLength (0); x++)
				for (int y = 0; y < this.actual_positions.GetLength (1); y++)
					if (this.actual_positions[x, y] == control)
						return new TableLayoutPanelCellPosition (x, y);

			return new TableLayoutPanelCellPosition (-1, -1);
		}

		[DisplayName ("Row")]
		[DefaultValue ("-1")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int GetRow (Control control)
		{
			return settings.GetRow (control);
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public int[] GetRowHeights ()
		{
			return this.row_heights;
		}

		[DisplayName ("RowSpan")]
		[DefaultValue (1)]
		public int GetRowSpan (Control control)
		{
			return settings.GetRowSpan (control);
		}

		public void SetCellPosition (Control control, TableLayoutPanelCellPosition position)
		{
			settings.SetCellPosition (control, position);
			this.PerformLayout ();
		}

		public void SetColumn (Control control, int column)
		{
			settings.SetColumn (control, column);
			this.PerformLayout ();
		}

		public void SetColumnSpan (Control control, int value)
		{
			settings.SetColumnSpan (control, value);
			this.PerformLayout ();
		}

		public void SetRow (Control control, int row)
		{
			settings.SetRow (control, row);
			this.PerformLayout ();
		}

		public void SetRowSpan (Control control, int value)
		{
			settings.SetRowSpan (control, value);
			this.PerformLayout ();
		}
		#endregion

		#region Protected Methods
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override ControlCollection CreateControlsInstance ()
		{
			return new TableLayoutControlCollection (this);
		}

		protected virtual void OnCellPaint (TableLayoutCellPaintEventArgs e)
		{
			TableLayoutCellPaintEventHandler eh = (TableLayoutCellPaintEventHandler)(Events [CellPaintEvent]);
			if (eh != null)
				eh (this, e);
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		protected override void OnLayout (LayoutEventArgs levent)
		{
			base.OnLayout (levent);
		}

		protected override void OnPaintBackground (PaintEventArgs e)
		{
			base.OnPaintBackground (e);
		}
		#endregion

		#region Public Events
		static object CellPaintEvent = new object ();

		public event TableLayoutCellPaintEventHandler CellPaint {
			add { Events.AddHandler (CellPaintEvent, value); }
			remove { Events.RemoveHandler (CellPaintEvent, value); }
		}
		#endregion
		
		#region IExtenderProvider
		bool IExtenderProvider.CanExtend (object extendee)
		{
			if (extendee is Control)
				if ((extendee as Control).Parent == this)
					return true;

			return false;
		}
		#endregion
		
	}
}
#endif
