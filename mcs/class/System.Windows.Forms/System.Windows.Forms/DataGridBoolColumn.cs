//
// System.Windows.Forms.DataGridBoolColumn
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies a column in which each cell contains a check box for representing a Boolean value.
	///
	/// ToDo note:
	///  - nothing is implemented
	/// </summary>

	[MonoTODO]
	public class DataGridBoolColumn : DataGridColumnStyle {

		#region Fields
		bool allowNull;
		object falseValue;
		object nullValue;
		object trueValue;
		#endregion
		
		#region Constructor
		[MonoTODO]
		public DataGridBoolColumn() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public DataGridBoolColumn(PropertyDescriptor prop) 
		{
			throw new NotImplementedException ();
		}
		
		/*
		 * This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		[MonoTODO]
		public DataGridBoolColumn(PropertyDescriptor prop,bool isDefault) 
		{
		}
		*/
		#endregion
		
		#region Properties
		
		public bool AllowNull {
			get { return allowNull; }
			set { allowNull=value; }
		}
		
		public object FalseValue {
			get { return falseValue; }
			set { falseValue=value; }
		}
		
		public object NullValue {
			get { return nullValue; }
			set { nullValue=value; }
		}
		
		public object TrueValue {
			get { return trueValue; }
			set { trueValue=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		protected internal override void Abort(int rowNum) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override bool Commit(CurrencyManager dataSource,int rowNum) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void ConcedeFocus() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void Edit(
			CurrencyManager source,
			int rowNum,
			Rectangle bounds,
			bool readOnly,
			string instantText,
			bool cellIsVisible) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void EnterNullValue() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override object GetColumnValueAtRow(CurrencyManager lm,int row) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override int GetMinimumHeight() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override int GetPreferredHeight(Graphics g,object value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override Size GetPreferredSize(Graphics g,object value) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void Paint(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source,
			int rowNum) {

			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void Paint(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source,
			int rowNum,
			bool alignToRight) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Paint(
			Graphics g,
			Rectangle bounds,
			CurrencyManager source,
			int rowNum,
			Brush backBrush,
			Brush foreBrush,
			bool alignToRight) {

			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void SetColumnValueAtRow(CurrencyManager lm,int row,object value) 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler AllowNullChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public event EventHandler FalseValueChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		protected event EventHandler TrueValueChanged {
			add {
				throw new NotImplementedException ();
			}
			remove {
				throw new NotImplementedException ();
			}
		}
		#endregion
	}
}
