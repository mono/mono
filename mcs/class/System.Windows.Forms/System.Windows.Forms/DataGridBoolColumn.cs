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
// Copyright (c) 2005-2007 Novell, Inc. (http://www.novell.com)
//
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//	Chris toshok <toshok@ximian.com>
//
//

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections;

namespace System.Windows.Forms
{
	public class DataGridBoolColumn : DataGridColumnStyle
	{
		[Flags]
		private enum CheckState {
			Checked		= 0x00000001,
			UnChecked	= 0x00000002,
			Null		= 0x00000004,
			Selected	= 0x00000008
		}

		#region	Local Variables
		private bool allow_null;
		private object false_value;
		private object null_value;
		private object true_value;
		int editing_row;
		CheckState editing_state;
		CheckState model_state;
		Size checkbox_size;

		#endregion	// Local Variables

		#region Constructors
		public DataGridBoolColumn () : this (null, false)
		{
		}

		public DataGridBoolColumn (PropertyDescriptor prop) : this (prop, false)
		{
		}

		public DataGridBoolColumn (PropertyDescriptor prop, bool isDefault)  : base (prop)
		{
			false_value = false;
			null_value = null;
			true_value = true;
			allow_null = true;
			is_default = isDefault;
			checkbox_size = new Size (ThemeEngine.Current.DataGridMinimumColumnCheckBoxWidth, ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight);
		}
		#endregion

		#region Public Instance Properties
		[DefaultValue(true)]
		public bool AllowNull {
			get {
				return allow_null;
			}
			set {
				if (value != allow_null) {
					allow_null = value;

					EventHandler eh = (EventHandler)(Events [AllowNullChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		[DefaultValue (false)]
		public object FalseValue {
			get {
				return false_value;
			}
			set {
				if (value != false_value) {
					false_value = value;

					EventHandler eh = (EventHandler)(Events [FalseValueChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		public object NullValue {
			get {
				return null_value;
			}
			set {
				if (value != null_value) {
					null_value = value;

					// XXX no NullValueChangedEvent?  lame.
				}
			}
		}

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
		[DefaultValue (true)]
		public object TrueValue {
			get {
				return true_value;
			}
			set {
				if (value != true_value) {
					true_value = value;

					EventHandler eh = (EventHandler)(Events [TrueValueChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected internal override void Abort (int rowNum)
		{
			if (rowNum == editing_row) {
				// XXX 
				// this needs to not use the current cell
				// bounds, but the bounds of the cell for this
				// column/rowNum.
				grid.Invalidate (grid.GetCurrentCellBounds ());
				editing_row = -1;
			}
		}

		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			if (rowNum == editing_row) {
				SetColumnValueAtRow (dataSource, rowNum, FromStateToValue (editing_state));
				// XXX 
				// this needs to not use the current cell
				// bounds, but the bounds of the cell for this
				// column/rowNum.
				grid.Invalidate (grid.GetCurrentCellBounds ());
				editing_row = -1;
			}
			return true;
		}

		[MonoTODO ("Stub, does nothing")]
		protected internal override void ConcedeFocus ()
		{
			base.ConcedeFocus ();
		}

		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText,  bool cellIsVisible)
		{
			editing_row = rowNum;
			model_state = FromValueToState (GetColumnValueAtRow (source, rowNum));
			editing_state = model_state | CheckState.Selected;
			// XXX 
			// this needs to not use the current cell
			// bounds, but the bounds of the cell for this
			// column/rowNum.
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		[MonoTODO ("Stub, does nothing")]
		protected internal override void EnterNullValue ()
		{
			base.EnterNullValue ();
		}

		private bool ValueEquals (object value, object obj)
		{
			return value == null ? obj == null : value.Equals (obj);
		}

		protected internal override object GetColumnValueAtRow (CurrencyManager lm, int row)
		{
			object obj = base.GetColumnValueAtRow (lm, row);

			if (ValueEquals (DBNull.Value, obj))
				return null_value;

			if (ValueEquals (true, obj))
				return true_value;

			return false_value;
		}

		protected internal override int GetMinimumHeight ()
		{
			return checkbox_size.Height;
		}

		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			return checkbox_size.Height;
		}

		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			return checkbox_size;
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			Paint (g, bounds, source, rowNum, false);
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			Paint (g, bounds, source, rowNum, ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.BackColor),
				ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.ForeColor), alignToRight);
		}

		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			Rectangle rect = new Rectangle ();			
			ButtonState state;
			CheckState check_state;

			if (rowNum == editing_row)
				check_state = editing_state;
			else
				check_state = FromValueToState (GetColumnValueAtRow (source, rowNum));

			rect.X = bounds.X + ((bounds.Width - checkbox_size.Width - 2) / 2);
			rect.Y = bounds.Y + ((bounds.Height - checkbox_size.Height - 2) / 2);
			rect.Width = checkbox_size.Width - 2;
			rect.Height = checkbox_size.Height - 2;
			
			// If the cell is selected
			if ((check_state & CheckState.Selected) == CheckState.Selected) { 
				backBrush = ThemeEngine.Current.ResPool.GetSolidBrush (grid.SelectionBackColor);
				check_state &= ~CheckState.Selected;
			}
						
			g.FillRectangle (backBrush, bounds);			
			
			switch (check_state) {
			case CheckState.Checked:
				state = ButtonState.Checked;
				break;
			case CheckState.Null:
				state = ButtonState.Checked | ButtonState.Inactive;
				break;
			case CheckState.UnChecked:
			default:
				state = ButtonState.Normal;
				break;
			}

			ThemeEngine.Current.CPDrawCheckBox (g, rect, state);
			PaintGridLine (g, bounds);
		}

		protected internal override void SetColumnValueAtRow (CurrencyManager lm, int row, object value)
		{
			object final_value = null;

			if (ValueEquals (null_value, value))
				final_value = DBNull.Value;
			else if (ValueEquals (true_value, value))
				final_value = true;
			else if (ValueEquals (false_value, value))
				final_value = false;
			/* else error? */

			base.SetColumnValueAtRow (lm, row, final_value);
		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods
		private object FromStateToValue (CheckState state)
		{
			if ((state & CheckState.Checked) == CheckState.Checked)
				return true_value;
			else if ((state & CheckState.Null) == CheckState.Null)
				return null_value;
			else
				return false_value;
		}

		private CheckState FromValueToState (object obj)
		{
			if (ValueEquals (true_value, obj))
				return CheckState.Checked;
			else if (ValueEquals (null_value, obj))
				return CheckState.Null;
			else
				return CheckState.UnChecked;
		}

		private CheckState GetNextState (CheckState state)
		{
			CheckState new_state;

			switch (state & ~CheckState.Selected) {
			case CheckState.Checked:
				if (AllowNull)
					new_state = CheckState.Null;
				else
					new_state = CheckState.UnChecked;
				break;
			case CheckState.Null:
				new_state = CheckState.UnChecked;
				break;
			case CheckState.UnChecked:
			default:
				new_state = CheckState.Checked;
				break;
			}
			
			new_state |= (state & CheckState.Selected);

			return new_state;
		}

		internal override void OnKeyDown (KeyEventArgs ke, int row, int column)
		{
			switch (ke.KeyCode) {
			case Keys.Space:
				NextState (row, column);
				break;
			}
		}

		internal override void OnMouseDown (MouseEventArgs e, int row, int column)
		{
			NextState (row, column);
		}

		private void NextState (int row, int column)
		{
			grid.ColumnStartedEditing (new Rectangle());

			editing_state = GetNextState (editing_state);

			grid.Invalidate (grid.GetCellBounds (row, column));
		}

		#endregion Private Instance Methods

		#region Events
		static object AllowNullChangedEvent = new object ();
		static object FalseValueChangedEvent = new object ();
		static object TrueValueChangedEvent = new object ();

		public event EventHandler AllowNullChanged {
			add { Events.AddHandler (AllowNullChangedEvent, value); }
			remove { Events.RemoveHandler (AllowNullChangedEvent, value); }
		}

		public event EventHandler FalseValueChanged {
			add { Events.AddHandler (FalseValueChangedEvent, value); }
			remove { Events.RemoveHandler (FalseValueChangedEvent, value); }
		}

		public event EventHandler TrueValueChanged {
			add { Events.AddHandler (TrueValueChangedEvent, value); }
			remove { Events.RemoveHandler (TrueValueChangedEvent, value); }
		}
		#endregion	// Events
	}
}
