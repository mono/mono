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
//	Jordi Mas i Hernadez <jordi@ximian.com>
//
//

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	public class DataGridBoolColumn : DataGridColumnStyle
	{
		#region	Local Variables
		private bool allownull;
		private object falsevalue;
		private object nullvalue;
		private object truevalue;
		#endregion	// Local Variables

		#region Constructors
		public DataGridBoolColumn ()
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop)
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop, bool isDefault)
		{
			CommonConstructor ();
		}

		private void CommonConstructor ()
		{
			allownull = true;
			falsevalue = false;
			nullvalue = null;
			truevalue = true;
		}

		#endregion

		#region Public Instance Properties
		public bool AllowNull {
			get {
				return allownull;
			}
			set {
				if (value != allownull) {
					allownull = value;

					if (AllowNullChanged != null) {
						AllowNullChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public object FalseValue {
			get {
				return falsevalue;
			}
			set {
				if (value != falsevalue) {
					falsevalue = value;

					if (FalseValueChanged != null) {
						FalseValueChanged (this, EventArgs.Empty);
					}
				}
			}
		}

		public object NullValue {
			get {
				return nullvalue;
			}
			set {
				if (value != nullvalue) {
					nullvalue = value;
				}
			}
		}

		public object TrueValue {
			get {
				return truevalue;
			}
			set {
				if (value != truevalue) {
					truevalue = value;

					if (TrueValueChanged != null) {
						TrueValueChanged (this, EventArgs.Empty);
					}
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		protected internal override void Abort (int rowNum)
		{

		}

		[MonoTODO]
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{

		}

		[MonoTODO]
		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText,  bool cellIsVisible)
		{

		}

		[MonoTODO]
		protected internal override void EnterNullValue ()
		{

		}

		[MonoTODO]
		protected internal override object GetColumnValueAtRow (CurrencyManager lm, int row)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override int GetMinimumHeight ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{

		}

		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,   bool alignToRight)
		{

		}

		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{

		}

		[MonoTODO]
		protected internal override void SetColumnValueAtRow (CurrencyManager lm, int row, object value)
		{

		}
		#endregion	// Public Instance Methods


		#region Events
		public event EventHandler AllowNullChanged;
		public event EventHandler FalseValueChanged;
		public event EventHandler TrueValueChanged;
		#endregion	// Events
	}
}
