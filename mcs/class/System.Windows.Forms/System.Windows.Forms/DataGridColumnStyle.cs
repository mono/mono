//
// System.Windows.Forms.DataGridColumnStyle
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

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

using System.ComponentModel;
using System.Drawing;
using System.Collections;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies the appearance and text formatting and behavior of a System.Windows.Forms.DataGrid control column. This class is abstract.
	/// </summary>
	
	[MonoTODO]
	public abstract class DataGridColumnStyle : Component, IDataGridColumnStyleEditingNotificationService {

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

		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		[MonoTODO]		public AccessibleObject HeaderAccessibleObject {
			get{
				throw new NotImplementedException ();
			}
		}
		
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
			//FIXME:
		}
		
		[MonoTODO]
		protected void CheckValidDataSource(CurrencyManager value) {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void ColumnStartedEditing(Control editingControl) {
			//FIXME:
		}
		
		protected internal abstract bool Commit(CurrencyManager dataSource,int rowNum);

		
		[MonoTODO]
		protected internal virtual void ConcedeFocus() {
			//FIXME:
		}
		
		//This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		[MonoTODO]
		protected virtual AccessibleObject CreateHeaderAccessibleObject(){
			throw new NotImplementedException ();
		}
		 
		 
		[MonoTODO]
		protected internal virtual void Edit(CurrencyManager source) {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void Edit(CurrencyManager source, int rowNum) {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal abstract void Edit(CurrencyManager source, int rowNum, Rectangle bounds,
			bool readOnly, string instantText, bool cellIsVisible);
		
		[MonoTODO]
		protected void EndUpdate() {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void EnterNullValue() {
			//FIXME:
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
			//FIXME:
		}

		[MonoTODO]
		protected virtual void Invalidate() {
			//FIXME:
		}
		
		protected internal abstract void Paint(Graphics g,Rectangle bounds,CurrencyManager source,int rowNum);
		
		protected internal abstract void Paint(Graphics g,Rectangle bounds,CurrencyManager source,int rowNum,bool alignToRight);
		
		[MonoTODO]
		protected internal virtual void Paint(Graphics g, Rectangle bounds, CurrencyManager source,
			int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight) {
			//FIXME:
		}
		
		[MonoTODO]
		public void ResetHeaderText() {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void SetColumnValueAtRow(CurrencyManager source,int rowNum,object value) {
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void SetDataGrid(DataGrid value) {
			//FIXME:
		}
		
		[MonoTODO]
		protected virtual void SetDataGridInColumn(DataGrid value) {
			//FIXME:
		}
		
		[MonoTODO]
		protected internal virtual void UpdateUI(CurrencyManager source,int rowNum,string instantText) {
			//FIXME:
		}
		#endregion
		
		#region Events

		public event EventHandler AlignmentChanged;
		
		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code
		public event EventHandler FontChanged;
		
		
		public event EventHandler HeaderTextChanged;
		public event EventHandler MappingNameChanged;
		public event EventHandler NullTextChanged;
		public event EventHandler PropertyDescriptorChanged;
		public event EventHandler ReadOnlyChanged;
		public event EventHandler WidthChanged;
		#endregion

		/// sub-classes:
		/// This type supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		///
		protected class CompModSwitches{
		}
		protected class DataGridColumnHeaderAccessibleObject : AccessibleObject{
		}
		
	}
}

