//
// System.Windows.Forms.DataGridColumnStyle
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms
{
	/// <summary>
	/// Specifies the appearance and text formatting and behavior of a System.Windows.Forms.DataGrid control column. This class is abstract.
	///
	/// ToDo note:
	///  - no methods are implemented
	/// </summary>
	
	[MonoTODO]
	public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService
	{
		#region Fields
		HorizontalAlignment alignment;
		DataGridTableStyle dataGridTableStyle;
		int fontHeight;
		string headerText;
		string mappingName;
		string nullText;
		PropertyDescriptor propertyDescriptor;
		bool readOnly;
		int width;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public DataGridColumnStyle() {
			alignment=HorizontalAlignment.Left;
			dataGridTableStyle=null;
			fontHeight=-1;
			headerText="";
			readOnly=false;
		}
		
		[MonoTODO]
		public DataGridColumnStyle(PropertyDescriptor prop) : this() {
			propertyDescriptor=prop;
		}
		#endregion
		
		#region Properties
		public virtual HorizontalAlignment Alignment {
			get { return alignment; }
			set { alignment=value; }
		}
		
		public virtual DataGridTableStyle DataGridTableStyle {
			get { return dataGridTableStyle; }
		}
		
		protected int FontHeight {
			get { return fontHeight; }
		}
		/*
		 * This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		public AccessibleObject HeaderAccessibleObject {get;}
		*/
		
		public virtual string HeaderText {
			get { return headerText; }
			set { headerText=value; }
		}
		
		public string MappingName {
			get { return mappingName; }
			set { mappingName=value; }
		}
		
		public virtual string NullText {
			get { return nullText; }
			set { nullText=value; }
		}
		
		public virtual PropertyDescriptor PropertyDescriptor {
			get { return propertyDescriptor; }
			set { propertyDescriptor=value; }
		}
		
		public virtual bool ReadOnly {
			get { return readOnly; }
			set { readOnly=value; }
		}
		
		public virtual int Width {
			get { return width; }
			set { width=value; }
		}
		#endregion
		
		#region Methods
		protected internal abstract void Abort(int rowNum);
		
		[MonoTODO]
		protected void BeginUpdate() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected void CheckValidDataSource(CurrencyManager value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Control editingControl) {
			throw new NotImplementedException ();
		}
		
		protected internal abstract bool Commit(CurrencyManager dataSource,int rowNum);
		
		[MonoTODO]
		protected internal virtual void ConcedeFocus() {
			throw new NotImplementedException ();
		}
		
		/*
		 * This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		protected virtual AccessibleObject CreateHeaderAccessibleObject()
		 */
		 
		[MonoTODO]
		protected internal virtual void Edit(
			CurrencyManager source,
			int rowNum,
			Rectangle bounds,
			bool readOnly)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void Edit(
			CurrencyManager source,
			int rowNum,
			Rectangle bounds,
			bool readOnly,
			string instantText)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal abstract void Edit(
			CurrencyManager source,
			int rowNum,
			Rectangle bounds,
			bool readOnly,
			string instantText,
			bool cellIsVisible);
		
		[MonoTODO]
		protected void EndUpdate() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void EnterNullValue() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual object GetColumnValueAtRow(CurrencyManager source,int rowNum) {
			throw new NotImplementedException ();
		}
		
		protected internal abstract int GetMinimumHeight();
		
		protected internal abstract int GetPreferredHeight(Graphics g,object value);
		
		protected internal abstract Size GetPreferredSize(Graphics g,object value);
		
		/*
		 * This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		 */
		[MonoTODO]
		void IDataGridColumnStyleEditingNotificationService.ColumnStartedEditing(Control editingControl) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void Invalidate() {
			throw new NotImplementedException ();
		}
		
		protected internal abstract void Paint(Graphics g,Rectangle bounds,CurrencyManager source,int rowNum);
		
		protected internal abstract void Paint(Graphics g,Rectangle bounds,CurrencyManager source,int rowNum,bool alignToRight);
		
		[MonoTODO]
		protected internal virtual void Paint(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source,
			int rowNum,
			Brush backBrush,
			Brush foreBrush,
			bool alignToRight)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void ResetHeaderText() {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void SetColumnValueAtRow(CurrencyManager source,int rowNum,object value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void SetDataGrid(DataGrid value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected virtual void SetDataGridInColumn(DataGrid value) {
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal virtual void UpdateUI(CurrencyManager source,int rowNum,string instantText) {
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler AlignmentChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		/* This member supports the .NET Framework infrastructure and is not intended to be used directly from your code
		public event EventHandler FontChanged;
		*/
		
		[MonoTODO]
		public event EventHandler HeaderTextChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler MappingNameChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler NullTextChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler PropertyDescriptorChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler ReadOnlyChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler WidthChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		#endregion

		/// sub-classes:
		/// This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		///
		/// protected class DataGridColumnStyle.CompModSwitches;
		/// protected class DataGridColumnStyle.DataGridColumnHeaderAccessibleObject : AccessibleObject;
		
	}
}

