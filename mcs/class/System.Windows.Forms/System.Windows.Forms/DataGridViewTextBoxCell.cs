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
//	Pedro Martínez Juliá <pedromj@gmail.com>
//


using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	public class DataGridViewTextBoxCell : DataGridViewCell {

		private int maxInputLength = 32767;
		private DataGridViewTextBoxEditingControl editingControl;

		void CreateEditingControl ()
		{
			editingControl = new DataGridViewTextBoxEditingControl() {
				Multiline = false,
				BorderStyle = BorderStyle.None
			};
		}

		public DataGridViewTextBoxCell ()
		{
			base.ValueType = typeof (object);
		}

		public override Type FormattedValueType {
			get { return typeof(string); }
		}

		[DefaultValue (32767)]
		public virtual int MaxInputLength {
			get { return maxInputLength; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException("MaxInputLength coudn't be less than 0.");
				}
				maxInputLength = value;
			}
		}

		public override Type ValueType {
			get { return base.ValueType; }
		}

		public override object Clone ()
		{
			DataGridViewTextBoxCell result = (DataGridViewTextBoxCell) base.Clone();
			result.maxInputLength = maxInputLength;
			return result;
		}

		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public override void DetachEditingControl ()
		{
			if (DataGridView == null) {
				throw new InvalidOperationException("There is no associated DataGridView.");
			}
			
			DataGridView.EditingControlInternal = null;
		}

		public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			if (DataGridView == null) {
				throw new InvalidOperationException("There is no associated DataGridView.");
			}

			if (editingControl == null || editingControl.IsDisposed)
				CreateEditingControl ();

			DataGridView.EditingControlInternal = editingControl;

			editingControl.EditingControlDataGridView = DataGridView;
			editingControl.MaxLength = maxInputLength;
			
			if (initialFormattedValue == null || initialFormattedValue.ToString () == string.Empty)
				editingControl.Text = string.Empty;
			else
				editingControl.Text = initialFormattedValue.ToString ();

			editingControl.ApplyCellStyleToEditingControl(dataGridViewCellStyle);
			editingControl.PrepareEditingControlForEdit(true);
		}

		public override bool KeyEntersEditMode (KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
				return true;
			if ((int)e.KeyCode >= 48 && (int)e.KeyCode <= 90)
				return true;
			if ((int)e.KeyCode >= 96 && (int)e.KeyCode <= 111)
				return true;
			if (e.KeyCode == Keys.BrowserSearch || e.KeyCode == Keys.SelectMedia)
				return true;
			if ((int)e.KeyCode >= 186 && (int)e.KeyCode <= 229)
				return true;
			if (e.KeyCode == Keys.Attn || e.KeyCode == Keys.Packet)
				return true;
			if ((int)e.KeyCode >= 248 && (int)e.KeyCode <= 254)
				return true;
				
			return false;
		}

		public override void PositionEditingControl (bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
		{
			if (editingControl == null)
				CreateEditingControl ();

			cellBounds.Size = new Size (cellBounds.Width - 5, cellBounds.Height + 2);
			cellBounds.Location = new Point (cellBounds.X + 3, ((cellBounds.Height - editingControl.Height) / 2) + cellBounds.Y - 1);

			base.PositionEditingControl (setLocation, setSize, cellBounds, cellClip, cellStyle, singleVerticalBorderAdded, singleHorizontalBorderAdded, isFirstDisplayedColumn, isFirstDisplayedRow);
			
			editingControl.Invalidate();
		}

		public override string ToString ()
		{
			return string.Format ("DataGridViewTextBoxCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex, RowIndex);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;
				
			object o = FormattedValue;
			Size s = Size.Empty;
			
			if (o != null) {
				s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height += 2;
			}
			
			return new Rectangle (0, (OwningRow.Height - s.Height) / 2, s.Width, s.Height);
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null || string.IsNullOrEmpty (ErrorText))
				return Rectangle.Empty;
				
			Size error_icon = new Size (12, 11);
			return new Rectangle (new Point (Size.Width - error_icon.Width - 5, (Size.Height - error_icon.Height) / 2), error_icon);
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			object o = FormattedValue;
			
			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 20);
				s.Width += 2;
				return s;
			} else
				return new Size (21, 20);
		}

		protected override void OnEnter (int rowIndex, bool throughMouseClick)
		{
		}

		protected override void OnLeave (int rowIndex, bool throughMouseClick)
		{
		}

		protected override void OnMouseClick (DataGridViewCellMouseEventArgs e)
		{
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			// Prepaint
			DataGridViewPaintParts pre = DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground;
			pre = pre & paintParts;
			
			base.Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, pre);

			// Paint content
			if (!IsInEditMode && (paintParts & DataGridViewPaintParts.ContentForeground) == DataGridViewPaintParts.ContentForeground) {
				Color color = Selected ? cellStyle.SelectionForeColor : cellStyle.ForeColor;

				TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.TextBoxControl;
				flags |= AlignmentToFlags (cellStyle.Alignment);

				Rectangle contentbounds = cellBounds;

				//Border widths
				Rectangle borderWidths = BorderWidths(advancedBorderStyle);
				contentbounds.Offset(borderWidths.X, borderWidths.Y);
				contentbounds.Width -= borderWidths.Right;
				contentbounds.Height -= borderWidths.Bottom;

				//Padding
				if (cellStyle.Padding != Padding.Empty)
				{
					contentbounds.Offset(cellStyle.Padding.Left, cellStyle.Padding.Top);
					contentbounds.Width -= cellStyle.Padding.Horizontal;
					contentbounds.Height -= cellStyle.Padding.Vertical;
				}

				const int textTopAdditionalPadding = 1;
				const int textBottomAdditionalPadding = 2;
				const int textLeftAdditionalPadding = 0;
				const int textRightAdditionalPadding = 2;
				contentbounds.Offset (textLeftAdditionalPadding, textTopAdditionalPadding);
				contentbounds.Width -= textLeftAdditionalPadding + textRightAdditionalPadding;
				contentbounds.Height -= textTopAdditionalPadding + textBottomAdditionalPadding;

				if (formattedValue != null && contentbounds.Width > 0 && contentbounds.Height > 0)
					TextRenderer.DrawText (graphics, formattedValue.ToString (), cellStyle.Font, contentbounds, color, flags);
			}

			// Postpaint
			DataGridViewPaintParts post = DataGridViewPaintParts.Border | DataGridViewPaintParts.Focus | DataGridViewPaintParts.ErrorIcon;
			post = post & paintParts;

			base.Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, post);
		}

	}

}
