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
		private bool allownull;
		private object falsevalue;
		private object nullvalue;
		private object truevalue;
		private Hashtable checkboxes_state;
		#endregion	// Local Variables

		#region Constructors
		public DataGridBoolColumn () : base ()
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop) : base (prop)
		{
			CommonConstructor ();
		}

		public DataGridBoolColumn (PropertyDescriptor prop, bool isDefault)  : base (prop)
		{
			CommonConstructor ();
			is_default = isDefault;
		}

		private void CommonConstructor ()
		{
			allownull = true;
			falsevalue = false;
			nullvalue = null;
			truevalue = true;
			checkboxes_state = new Hashtable ();			
		}

		#endregion

		#region Public Instance Properties
		[DefaultValue(true)]
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

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
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

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
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

		[TypeConverter(typeof(System.ComponentModel.StringConverter))]
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
		protected internal override void Abort (int rowNum)
		{
			SetState (rowNum, GetState (null, rowNum) & ~CheckState.Selected);			
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		protected internal override bool Commit (CurrencyManager source, int rowNum)
		{
			SetColumnValueAtRow (source, rowNum, FromStateToValue (GetState (source, rowNum)));
			SetState (rowNum, GetState (source, rowNum) & ~CheckState.Selected);
			grid.Invalidate (grid.GetCurrentCellBounds ());
			return true;
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{

		}

		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText,  bool cellIsVisible)
		{
			SetState (rowNum, GetState (source, rowNum) | CheckState.Selected);
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		[MonoTODO]
		protected internal override void EnterNullValue ()
		{

		}

		protected internal override object GetColumnValueAtRow (CurrencyManager lm, int row)
		{
			object obj = base.GetColumnValueAtRow (lm, row);

			if (obj.Equals (nullvalue)) {
				return Convert.DBNull;
			}

			if (obj.Equals (truevalue)) {
				return true;
			}

			return false;
		}

		protected internal override int GetMinimumHeight ()
		{
			return ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight;
		}

		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			return ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight;
		}

		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			return new Size (ThemeEngine.Current.DataGridMinimumColumnCheckBoxWidth, ThemeEngine.Current.DataGridMinimumColumnCheckBoxHeight);
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
			Size chkbox_size = GetPreferredSize (g, null);
			Rectangle rect = new Rectangle ();			
			ButtonState state;
			chkbox_size.Width -= 2;
			chkbox_size.Height -= 2;
			rect.X = bounds.X + ((bounds.Width - chkbox_size.Width) / 2);
			rect.Y = bounds.Y + ((bounds.Height - chkbox_size.Height) / 2);
			rect.Width = chkbox_size.Width;
			rect.Height = chkbox_size.Height;			
			
			// If the cell is selected
			if ((GetState (source, rowNum) & CheckState.Selected) == CheckState.Selected) { 
				backBrush = ThemeEngine.Current.ResPool.GetSolidBrush (grid.SelectionBackColor);
			}
						
			g.FillRectangle (backBrush, bounds);			
			
			switch (GetState (source, rowNum) & ~CheckState.Selected) {
			case CheckState.Checked:
				state = ButtonState.Checked;
				break;
			case CheckState.Null:
				state = ButtonState.Inactive;
				break;
			case CheckState.UnChecked:
			default:
				state = ButtonState.Normal;
				break;
			}

			ThemeEngine.Current.CPDrawCheckBox (g, rect, state);
			PaintGridLine (g, bounds);
		}

		protected internal override void SetColumnValueAtRow (CurrencyManager lm, int row, object obj)
		{
			object value = null;

			if (obj.Equals (nullvalue)) {
				value = Convert.DBNull;
			} else {
				if (obj.Equals (truevalue)) {
					value = true;
				}
			}

			base.SetColumnValueAtRow (lm, row, value);
		}
		#endregion	// Public Instance Methods

		#region Private Instance Methods
		internal static bool CanRenderType (Type type)
		{
			return (type == typeof (Boolean));
		}

		private object FromStateToValue (CheckState state)
		{
			state = state & ~CheckState.Selected;	
			
			if ((state & CheckState.Checked) == CheckState.Checked) {
				return truevalue;
			}

			if ((state & CheckState.Null) == CheckState.Null) {
				return nullvalue;
			}

			return falsevalue;
		}

		private CheckState FromValueToState (object obj)
		{
			if (obj.Equals (truevalue)) {
				return CheckState.Checked;
			}

			if (obj.Equals (nullvalue)) {
				return CheckState.Null;
			}

			return CheckState.UnChecked;
		}

		private CheckState GetState (CurrencyManager source, int row)
		{
			CheckState state;

			if (checkboxes_state[row] == null) {
				object value = GetColumnValueAtRow (source, row);
				state =	FromValueToState (value);
				checkboxes_state.Add (row, state);
			} else {
				state = (CheckState) checkboxes_state[row];
			}

			return state;
		}

		private CheckState GetNextState (CheckState state)
		{
			CheckState new_state;
			bool selected = ((state & CheckState.Selected) == CheckState.Selected);

			switch (state & ~CheckState.Selected) {
			case CheckState.Checked:
				new_state = CheckState.Null;
				break;
			case CheckState.Null:
				new_state = CheckState.UnChecked;
				break;
			case CheckState.UnChecked:
			default:
				new_state = CheckState.Checked;
				break;
			}
			
			if (selected) {
				new_state = new_state | CheckState.Selected;
			}

			return new_state;
		}

		internal override void OnKeyDown (KeyEventArgs ke, int row, int column)
		{
			CheckState state = GetNextState (GetState (null, row));

			if (ke.KeyCode == Keys.Space) {
				grid.is_changing = true;
				grid.InvalidateCurrentRowHeader ();
				checkboxes_state[row] = state;
				grid.Invalidate (grid.GetCellBounds (row, column));
			}
		}

		internal override void OnMouseDown (MouseEventArgs e, int row, int column)
		{
			CheckState state = GetNextState (GetState (null, row));

			grid.is_changing = true;
			grid.InvalidateCurrentRowHeader ();			
			SetState (row, state);
			grid.Invalidate (grid.GetCellBounds (row, column));
		}

		private void SetState (int row, CheckState state)
		{			
			if (checkboxes_state[row] == null) {
				checkboxes_state.Add (row, state);
			} else {
				checkboxes_state[row] = state;
			}
		}

		#endregion Private Instance Methods

		#region Events
		public event EventHandler AllowNullChanged;
		public event EventHandler FalseValueChanged;
		public event EventHandler TrueValueChanged;
		#endregion	// Events
	}
}
