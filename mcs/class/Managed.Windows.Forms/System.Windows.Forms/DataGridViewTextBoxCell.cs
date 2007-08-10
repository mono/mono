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


#if NET_2_0

using System;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	public class DataGridViewTextBoxCell : DataGridViewCell {

		private int maxInputLength = 32767;
		private static DataGridViewTextBoxEditingControl editingControl;

		static DataGridViewTextBoxCell ()
		{
			editingControl = new DataGridViewTextBoxEditingControl();
			editingControl.Multiline = false;
			editingControl.BorderStyle = BorderStyle.None;
		}

		public DataGridViewTextBoxCell ()
		{
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
			get { return typeof(string); }
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
			
			Console.WriteLine("Detached: ({0}, {1});", RowIndex, ColumnIndex);
		}

		public override void InitializeEditingControl (int rowIndex, object initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			if (DataGridView == null) {
				throw new InvalidOperationException("There is no associated DataGridView.");
			}
			
			DataGridView.EditingControlInternal = editingControl;
			
			editingControl.EditingControlDataGridView = DataGridView;
			editingControl.MaxLength = maxInputLength;
			if (initialFormattedValue == null || (string) initialFormattedValue == "") {
				editingControl.Text = "";
			}
			else {
				editingControl.Text = (string) initialFormattedValue;
			}
			editingControl.ApplyCellStyleToEditingControl(dataGridViewCellStyle);
			editingControl.PrepareEditingControlForEdit(true);
		}

		public override bool KeyEntersEditMode (KeyEventArgs e)
		{
			throw new NotImplementedException();
		}

		public override void PositionEditingControl (bool setLocation, bool setSize, Rectangle cellBounds, Rectangle cellClip, DataGridViewCellStyle cellStyle, bool singleVerticalBorderAdded, bool singleHorizontalBorderAdded, bool isFirstDisplayedColumn, bool isFirstDisplayedRow)
		{
			if (setSize) {
				editingControl.Size = new Size(cellBounds.Width, cellBounds.Height + 2);
			}
			if (setLocation) {
				editingControl.Location = new Point(cellBounds.X, cellBounds.Y);
			}
			editingControl.Invalidate();
		}

		public override string ToString ()
		{
			return this.GetType().Name;
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			throw new NotImplementedException();
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			throw new NotImplementedException();
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			throw new NotImplementedException();
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
			//////////////////
			/*
			Size size = DataGridViewCell.MeasureTextSize(graphics, (string) formattedValue, cellStyle.Font, TextFormatFlags.Default);
			switch (cellStyle.Alignment) {
				case DataGridViewContentAlignment.TopLeft:
					break;
			}
			//cell.SetContentBounds(cellBounds);
			*/
			//////////////////
			StringFormat format;
			Brush forecolor_brush;
			Brush backcolor_brush;
			Rectangle text_rect = cellBounds;
			Rectangle borders = BorderWidths (advancedBorderStyle);
			
			text_rect.X += borders.X;
			text_rect.Y += borders.Y;
			text_rect.Height -= (borders.Y + borders.Height);
			text_rect.Width -= (borders.X + borders.Width);
			
			format = cellStyle.SetAlignment ((StringFormat) StringFormat.GenericTypographic.Clone ());
			if ((cellState & DataGridViewElementStates.Selected) != 0 && !IsInEditMode) {
				backcolor_brush =  ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.SelectionBackColor);
				forecolor_brush = ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.SelectionForeColor);
			} else {
				backcolor_brush =  ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.BackColor);
				forecolor_brush = ThemeEngine.Current.ResPool.GetSolidBrush (cellStyle.ForeColor);
			}

			graphics.FillRectangle (backcolor_brush, cellBounds);
			graphics.DrawString ((string) formattedValue, cellStyle.Font, forecolor_brush, text_rect, format);
			PaintBorder (graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
		}

	}

}

#endif
