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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//      Chris Toshok <toshok@ximian.com>
//

using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	[DesignTimeVisible(false)]
	[DefaultProperty("Header")]
	[ToolboxItem(false)]
	public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService
	{
		[ComVisible(true)]
		protected class DataGridColumnHeaderAccessibleObject : AccessibleObject
		{
			#region Local Variables
			private new DataGridColumnStyle owner;
			#endregion

			#region Constructors
			public DataGridColumnHeaderAccessibleObject ()
			{
			}
			public DataGridColumnHeaderAccessibleObject (DataGridColumnStyle owner)
			{
				this.owner = owner;
			}
			#endregion //Constructors

			#region Public Instance Properties
			[MonoTODO ("Not implemented, will throw NotImplementedException")]
			public override Rectangle Bounds {
				get {
					throw new NotImplementedException ();
				}
			}

			public override string Name {
				get {
					throw new NotImplementedException ();
				}
			}

			protected DataGridColumnStyle Owner {
				get { return owner; }
			}

			public override AccessibleObject Parent {
				get {
					throw new NotImplementedException ();
				}
			}

			public override AccessibleRole Role {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion

			#region Public Instance Methods
			[MonoTODO ("Not implemented, will throw NotImplementedException")]
			public override AccessibleObject Navigate (AccessibleNavigation navdir)
			{
				throw new NotImplementedException ();
			}
			#endregion Public Instance Methods
		}

		protected class CompModSwitches
		{
			public CompModSwitches ()
			{
			}

			#region Public Instance Methods
			[MonoTODO ("Not implemented, will throw NotImplementedException")]
			public static TraceSwitch DGEditColumnEditing {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion Public Instance Methods
		}
		
		internal enum ArrowDrawing
		{
			No = 0,
			Ascending = 1,
			Descending = 2
		}
		
		#region	Local Variables
		internal HorizontalAlignment alignment;
		private int fontheight;
		internal DataGridTableStyle table_style;
		private string header_text;
		private string mapping_name;
		private string null_text;
		private PropertyDescriptor property_descriptor;
		private bool _readonly;
		private int width;
		internal bool is_default;
		internal DataGrid grid;
		private DataGridColumnHeaderAccessibleObject accesible_object;
		static string def_null_text = "(null)";
		private ArrowDrawing arrow_drawing = ArrowDrawing.No;
		internal bool bound;
		#endregion	// Local Variables

		#region Constructors
		public DataGridColumnStyle () : this (null)
		{
		}

		public DataGridColumnStyle (PropertyDescriptor prop)
		{
			property_descriptor = prop;

			fontheight = -1;
			table_style = null;
			header_text = string.Empty;
			mapping_name  = string.Empty;
			null_text = def_null_text;
			accesible_object = new DataGridColumnHeaderAccessibleObject (this);
			_readonly = prop == null ? false : prop.IsReadOnly;
			width = -1;
			grid = null;
			is_default = false;
			alignment = HorizontalAlignment.Left;
		}

		#endregion

		#region Public Instance Properties
		[Localizable(true)]
		[DefaultValue(HorizontalAlignment.Left)]
		public virtual HorizontalAlignment Alignment {
			get {
				return alignment;
			}
			set {
				if (value != alignment) {
					alignment = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.Invalidate ();
					}
					
					EventHandler eh = (EventHandler)(Events [AlignmentChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		public virtual DataGridTableStyle DataGridTableStyle {
			get { return table_style; }
		}
		
		protected int FontHeight {
			get {
				if (fontheight != -1) {
					return fontheight;
				}

				if (table_style != null) {
					//return table_style.DataGrid.FontHeight
					return -1;
				}

				// TODO: Default Datagrid font height
				return -1;
			}
		}

		[Browsable(false)]
		public AccessibleObject HeaderAccessibleObject {
			get {
				return accesible_object;
			}
		}

		[Localizable(true)]
		public virtual string HeaderText {
			get {
				return header_text;
			}
			set {
				if (value != header_text) {
					header_text = value;
					
					Invalidate ();

					EventHandler eh = (EventHandler)(Events [HeaderTextChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}


		[Editor("System.Windows.Forms.Design.DataGridColumnStyleMappingNameEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[Localizable(true)]
		[DefaultValue ("")]
		public string MappingName {
			get {
				return mapping_name;
			}
			set {
				if (value != mapping_name) {
					mapping_name = value;

					EventHandler eh = (EventHandler)(Events [MappingNameChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[Localizable(true)]
		public virtual string NullText {
			get {
				return null_text;
			}
			set {
				if (value != null_text) {
					null_text = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.Invalidate ();
					}

					EventHandler eh = (EventHandler)(Events [NullTextChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public virtual PropertyDescriptor PropertyDescriptor {
			get {
				return property_descriptor;
			}
			set {
				if (value != property_descriptor) {
					property_descriptor = value;

					EventHandler eh = (EventHandler)(Events [PropertyDescriptorChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[DefaultValue(false)]
		public virtual bool ReadOnly {
			get {
				return _readonly;
			}
			set {
				if (value != _readonly) {
					_readonly = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.CalcAreasAndInvalidate ();
					}
					
					EventHandler eh = (EventHandler)(Events [ReadOnlyChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[DefaultValue(100)]
		[Localizable(true)]
		public virtual int Width {
			get {
				return width;
			}
			set {
				if (value != width) {
					width = value;
					
					if (table_style != null && table_style.DataGrid != null) {
						table_style.DataGrid.CalcAreasAndInvalidate ();
					}

					EventHandler eh = (EventHandler)(Events [WidthChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		#endregion	// Public Instance Properties
		
		#region Private Instance Properties

		internal ArrowDrawing ArrowDrawingMode {
			get { return arrow_drawing; }
			set { arrow_drawing = value; }
		}
		
		internal bool TableStyleReadOnly {
			get {
				return table_style != null && table_style.ReadOnly; 
			}
		}
		
		internal DataGridTableStyle TableStyle {
			set { table_style = value; }
		}
		
		internal bool IsDefault {
			get { return is_default; }
		}
		#endregion Private Instance Properties

		#region Public Instance Methods
		protected internal abstract void Abort (int rowNum);

		[MonoTODO ("Will not suspend updates")]
		protected void BeginUpdate ()
		{
		}
		
		protected void CheckValidDataSource (CurrencyManager value)
		{
			if (value == null) {
				throw new ArgumentNullException ("CurrencyManager cannot be null");
			}
			
			if (property_descriptor == null) {
				property_descriptor = value.GetItemProperties ()[mapping_name];

// 				Console.WriteLine ("mapping name = {0}", mapping_name);
// 				foreach (PropertyDescriptor prop in value.GetItemProperties ()) {
// 					Console.WriteLine (" + prop = {0}", prop.Name);
// 				}

				if (property_descriptor == null)
					throw new InvalidOperationException ("The PropertyDescriptor for this column is a null reference");

				 /*MonoTests.System.Windows.Forms.DataGridColumnStyleTest.GetColumnValueAtRow : System.InvalidOperationException : The 'foo' DataGridColumnStyle cannot be used because it is not associated with a Property or Column in the DataSource.*/

				
			}
		}

		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{
		}

		protected internal abstract bool Commit (CurrencyManager dataSource, int rowNum);


		protected internal virtual void ConcedeFocus ()
		{
		}
		
		protected virtual AccessibleObject CreateHeaderAccessibleObject ()
		{
			return new DataGridColumnHeaderAccessibleObject (this);
		}

		protected internal virtual void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool readOnly)
		{
			Edit (source, rowNum, bounds, readOnly, string.Empty);
		}

		protected internal virtual void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText)
		{
			string instantText = displayText;
			Edit (source, rowNum, bounds, readOnly, instantText, true);
		}

		protected internal abstract void Edit (CurrencyManager source,
			int rowNum, Rectangle bounds, bool readOnly,
			string displayText,
			bool cellIsVisible);


		protected void EndUpdate ()
		{
		}

		protected internal virtual void EnterNullValue () {}
		
		protected internal virtual object GetColumnValueAtRow (CurrencyManager source, int rowNum)
		{
			CheckValidDataSource (source);
			if (rowNum >= source.Count)
				return DBNull.Value;
			return property_descriptor.GetValue (source [rowNum]);
		}

		protected internal abstract int GetMinimumHeight ();

		protected internal abstract int GetPreferredHeight (Graphics g, object value);

		protected internal abstract Size GetPreferredSize (Graphics g,  object value);

		void IDataGridColumnStyleEditingNotificationService.ColumnStartedEditing (Control editingControl)
		{
			ColumnStartedEditing (editingControl);
		}

		protected virtual void Invalidate ()
		{
			if (grid != null)
				grid.InvalidateColumn (this);
		}

		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum);
		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight);
		
		protected internal virtual void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,
			Brush backBrush,  Brush foreBrush, bool alignToRight) {}

		protected internal virtual void ReleaseHostedControl () {}

		public void ResetHeaderText ()
		{
			HeaderText = string.Empty;
		}

		protected internal virtual void SetColumnValueAtRow (CurrencyManager source, int rowNum,  object value)
		{
			CheckValidDataSource (source);

			IEditableObject editable = source [rowNum] as IEditableObject;

			if (editable != null)
				editable.BeginEdit ();

			property_descriptor.SetValue (source [rowNum], value);
		}

		protected virtual void SetDataGrid (DataGrid value)
		{
			grid = value;

			property_descriptor = null;

			// we don't check whether the DataGrid.ListManager is valid or not.
			// This is done by .net later as requiered, but not at this point.
		}

		protected virtual void SetDataGridInColumn (DataGrid value)
		{
			SetDataGrid (value);
		}
		
		internal void SetDataGridInternal (DataGrid value)
		{
			SetDataGridInColumn (value);
		}

		protected internal virtual void UpdateUI (CurrencyManager source, int rowNum, string displayText)
		{
		}

		#endregion	// Public Instance Methods
		
		#region Private Instance Methods
		virtual internal void OnMouseDown (MouseEventArgs e, int row, int column) {}
		virtual internal void OnKeyDown (KeyEventArgs ke, int row, int column) {}
		
		internal void PaintHeader (Graphics g, Rectangle bounds, int colNum)
		{
			ThemeEngine.Current.DataGridPaintColumnHeader (g, bounds, grid, colNum);
		}
		
		internal void PaintNewRow (Graphics g, Rectangle bounds, Brush backBrush, Brush foreBrush)
		{
			g.FillRectangle (backBrush, bounds);
			PaintGridLine (g, bounds);
		}
		
		internal void PaintGridLine (Graphics g, Rectangle bounds)
		{
			if (table_style.CurrentGridLineStyle != DataGridLineStyle.Solid) {
				return;
			}
			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (table_style.CurrentGridLineColor),
				bounds.X, bounds.Y + bounds.Height - 1, bounds.X + bounds.Width - 1, bounds.Y + bounds.Height - 1);
			
			g.DrawLine (ThemeEngine.Current.ResPool.GetPen (table_style.CurrentGridLineColor),
				bounds.X + bounds.Width - 1, bounds.Y , bounds.X + bounds.Width - 1, bounds.Y + bounds.Height);
		}
		
		#endregion Private Instance Methods

		#region Events
		static object AlignmentChangedEvent = new object ();
		static object FontChangedEvent = new object ();
		static object HeaderTextChangedEvent = new object ();
		static object MappingNameChangedEvent = new object ();
		static object NullTextChangedEvent = new object ();
		static object PropertyDescriptorChangedEvent = new object ();
		static object ReadOnlyChangedEvent = new object ();
		static object WidthChangedEvent = new object ();

		public event EventHandler AlignmentChanged {
			add { Events.AddHandler (AlignmentChangedEvent, value); }
			remove { Events.RemoveHandler (AlignmentChangedEvent, value); }
		}

		public event EventHandler FontChanged {
			add { Events.AddHandler (FontChangedEvent, value); }
			remove { Events.RemoveHandler (FontChangedEvent, value); }
		}

		public event EventHandler HeaderTextChanged {
			add { Events.AddHandler (HeaderTextChangedEvent, value); }
			remove { Events.RemoveHandler (HeaderTextChangedEvent, value); }
		}

		public event EventHandler MappingNameChanged {
			add { Events.AddHandler (MappingNameChangedEvent, value); }
			remove { Events.RemoveHandler (MappingNameChangedEvent, value); }
		}

		public event EventHandler NullTextChanged {
			add { Events.AddHandler (NullTextChangedEvent, value); }
			remove { Events.RemoveHandler (NullTextChangedEvent, value); }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public event EventHandler PropertyDescriptorChanged {
			add { Events.AddHandler (PropertyDescriptorChangedEvent, value); }
			remove { Events.RemoveHandler (PropertyDescriptorChangedEvent, value); }
		}

		public event EventHandler ReadOnlyChanged {
			add { Events.AddHandler (ReadOnlyChangedEvent, value); }
			remove { Events.RemoveHandler (ReadOnlyChangedEvent, value); }
		}

		public event EventHandler WidthChanged {
			add { Events.AddHandler (WidthChangedEvent, value); }
			remove { Events.RemoveHandler (WidthChangedEvent, value); }
		}
		#endregion	// Events
	}
}
