//
// System.Windows.Forms.DataGridBoolColumn
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

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Specifies a column in which each cell contains a check box for representing a Boolean value.
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
			
		}
		
		[MonoTODO]
		public DataGridBoolColumn(PropertyDescriptor prop) 
		{
			
		}
		
		// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		[MonoTODO]
		public DataGridBoolColumn(PropertyDescriptor prop,bool isDefault) 
		{
		}

		#endregion
		
		#region Properties

		[MonoTODO]
		public AccessibleObject HeaderAccessibleObject {
			get {
				throw new NotImplementedException ();
			}
		}
		
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
			//FIXME:
			base.ConcedeFocus();
		}
		
		[MonoTODO]
		protected internal override void Edit(CurrencyManager source, int rowNum,
			Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible) {
			//FIXME:
		}

		[MonoTODO]
		protected internal override void EnterNullValue() 
		{
			//FIXME:
			base.EnterNullValue();
		}
		
		[MonoTODO]
		protected internal override object GetColumnValueAtRow(CurrencyManager source,int rowNum) 
		{
			//FIXME:
			return base.GetColumnValueAtRow(source, rowNum);
		}
		
		[MonoTODO]
		protected internal override int GetMinimumHeight() 
		{
			//FIXME:made up number
			return 20;
		}
		
		[MonoTODO]
		protected internal override int GetPreferredHeight(Graphics g, object value) 
		{
			//FIXME:made up number
			return 300;
		}
		
		[MonoTODO]
		protected internal override Size GetPreferredSize(Graphics g, object value) 
		{
			//FIXME:made up number
			return new Size(300,300);
		}
		
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds,
			CurrencyManager source, int rowNum) {
			//FIXME:made up number
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds,
			CurrencyManager source, int rowNum, bool alignToRight) {

			//return base.Paint(g, bounds, source, rowNum, 
		}

		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source,
			int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight) {
			//FIXME:
		}

		[MonoTODO]
		protected internal override void SetColumnValueAtRow(CurrencyManager source,int rowNum, object value) 
		{
			//FIXME:
			base.SetColumnValueAtRow(source, rowNum, value);
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler AllowNullChanged;
		
		[MonoTODO]
		public event EventHandler FalseValueChanged;

		[MonoTODO]
		public event EventHandler FontChanged;
		
		[MonoTODO]
		public event EventHandler TrueValueChanged;
		#endregion
	}
}
