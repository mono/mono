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
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService
	{
		[ComVisible(true)]
		protected class DataGridColumnHeaderAccessibleObject : AccessibleObject
		{
			#region Local Variables
			private DataGridColumnStyle owner;
			#endregion

			#region Constructors
			public DataGridColumnHeaderAccessibleObject (DataGridColumnStyle columnstyle)
			{
				owner = columnstyle;
			}
			#endregion //Constructors

			#region Public Instance Properties
			[MonoTODO]
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
			[MonoTODO]
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
			[MonoTODO]
			public static TraceSwitch DGEditColumnEditing {
				get {
					throw new NotImplementedException ();
				}
			}
			#endregion Public Instance Methods
		}

		#region	Local Variables
		private HorizontalAlignment alignment;
		private int fontheight;
		private DataGridTableStyle table_style;
		private string header_text;
		private string mapping_name;
		private string null_text;
		private PropertyDescriptor property_descriptor;
		private bool read_only;
		private int width;
		private DataGridColumnHeaderAccessibleObject accesible_object;
		#endregion	// Local Variables

		#region Constructors
		public DataGridColumnStyle ()
		{
			CommmonConstructor ();
			property_descriptor = null;
		}

		public DataGridColumnStyle (PropertyDescriptor prop)
		{
			CommmonConstructor ();
			property_descriptor = prop;
		}

		private void CommmonConstructor ()
		{
			fontheight = -1;
			table_style = null;
			header_text = string.Empty;
			mapping_name  = "(null)";
			null_text = string.Empty;
			accesible_object = new DataGridColumnHeaderAccessibleObject (this);
			read_only = false;
			width = -1;
			alignment = HorizontalAlignment.Left;
		}

		#endregion

		#region Public Instance Properties
		public virtual HorizontalAlignment Alignment {
			get {
				return alignment;
			}
			set {
				if (value != alignment) {
					alignment = value;

					if (AlignmentChanged != null) {
						AlignmentChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public virtual DataGridTableStyle DataGridTableStyle {
			get {
				return table_style;
			}
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

		public AccessibleObject HeaderAccessibleObject {
			get {
				return accesible_object;
			}
		}

		public virtual string HeaderText {
			get {
				return header_text;
			}
			set {
				if (value != header_text) {
					header_text = value;

					if (HeaderTextChanged != null) {
						HeaderTextChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public string MappingName {
			get {
				return mapping_name;
			}
			set {
				if (value != mapping_name) {
					mapping_name = value;

					if (MappingNameChanged != null) {
						MappingNameChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public virtual string NullText {
			get {
				return null_text;
			}
			set {
				if (value != null_text) {
					null_text = value;

					if (NullTextChanged != null) {
						NullTextChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public virtual PropertyDescriptor PropertyDescriptor {
			get {
				return property_descriptor;
			}
			set {
				if (value != property_descriptor) {
					property_descriptor = value;

					if (PropertyDescriptorChanged != null) {
						PropertyDescriptorChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public virtual bool ReadOnly  {
			get {
				return read_only;
			}
			set {
				if (value != read_only) {
					read_only = value;

					if (ReadOnlyChanged != null) {
						ReadOnlyChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public virtual int Width {
			get {
				return width;
			}
			set {
				if (value != width) {
					width = value;

					if (WidthChanged != null) {
						WidthChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected internal abstract void Abort (int rowNum);

		[MonoTODO]
		protected void BeginUpdate ()
		{

		}

		[MonoTODO]
		protected void CheckValidDataSource (CurrencyManager value)
		{

		}

		[MonoTODO]
		protected internal virtual void ColumnStartedEditing (Control editingControl)
		{

		}

		protected internal abstract bool Commit (CurrencyManager dataSource, int rowNum);


		protected internal virtual void ConcedeFocus ()
		{

		}

		[MonoTODO]
		protected virtual AccessibleObject CreateHeaderAccessibleObject ()
		{
			throw new NotImplementedException ();
		}


		protected internal virtual void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool readOnly)
		{

		}

		[MonoTODO]
		protected internal virtual void Edit (CurrencyManager source, int rowNum, Rectangle bounds,  bool readOnly,   string instantText)
		{

		}

		protected internal abstract void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly,   string instantText,  bool cellIsVisible);


		[MonoTODO]
		protected void EndUpdate ()
		{

		}

		protected internal virtual void EnterNullValue ()
		{

		}

		[MonoTODO]
		protected internal virtual object GetColumnValueAtRow (CurrencyManager source, int rowNum)
		{
			throw new NotImplementedException ();
		}

		protected internal abstract int GetMinimumHeight ();

		protected internal abstract int GetPreferredHeight (Graphics g, object value);

		protected internal abstract Size GetPreferredSize (Graphics g,  object value);

		void  IDataGridColumnStyleEditingNotificationService.ColumnStartedEditing (Control editingControl)
		{

		}

		protected virtual void Invalidate ()
		{

		}

		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum);
		protected internal abstract void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight);

		[MonoTODO]
		protected internal virtual void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,
   			Brush backBrush,  Brush foreBrush, bool alignToRight)
   		{

		}

		protected internal virtual void ReleaseHostedControl ()
		{

		}

		public void ResetHeaderText ()
		{
			HeaderText = string.Empty;
		}


		protected internal virtual void SetColumnValueAtRow (CurrencyManager source, int rowNum,  object value)
		{

		}

		protected virtual void SetDataGrid (DataGrid value)
		{

		}

		protected virtual void SetDataGridInColumn (DataGrid value)
		{

		}

		protected internal virtual void UpdateUI (CurrencyManager source, int rowNum, string instantText)
		{

		}

		#endregion	// Public Instance Methods


		#region Events
		public event EventHandler AlignmentChanged;
		public event EventHandler FontChanged;
		public event EventHandler HeaderTextChanged;
		public event EventHandler MappingNameChanged;
		public event EventHandler NullTextChanged;
		public event EventHandler PropertyDescriptorChanged;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler WidthChanged;
		#endregion	// Events
	}
}
