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

using System.Drawing;

namespace System.Windows.Forms
{
	public class DataGridViewTopLeftHeaderCell : DataGridViewColumnHeaderCell
	{
		public DataGridViewTopLeftHeaderCell ()
		{
		}

		public override string ToString ()
		{
			return GetType ().Name;
		}

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewTopLeftHeaderCellAccessibleObject (this);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;

			Size s = new Size (36, 13);
			return new Rectangle (2, (DataGridView.ColumnHeadersHeight - s.Height) / 2, s.Width, s.Height);
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
			object o = Value;

			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 17);
				s.Width += 29;
				return s;
			} else
				return new Size (39, 17);
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates  cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint (graphics, clipBounds, cellBounds, rowIndex,  cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
	
		protected override void PaintBorder (Graphics graphics, Rectangle clipBounds, Rectangle bounds, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle)
		{
			base.PaintBorder (graphics, clipBounds, bounds, cellStyle, advancedBorderStyle);
		}

		protected class DataGridViewTopLeftHeaderCellAccessibleObject : DataGridViewColumnHeaderCellAccessibleObject
		{
			public DataGridViewTopLeftHeaderCellAccessibleObject (DataGridViewTopLeftHeaderCell owner) : base (owner)
			{
			}

			public override Rectangle Bounds {
				get { throw new NotImplementedException (); }
			}

			public override string DefaultAction {
				get {
					if (Owner.DataGridView != null && Owner.DataGridView.MultiSelect) {
						return "Press to Select All";
					}
					return string.Empty;
				}
			}

			public override string Name {
				get { return base.Name; }
			}

			public override AccessibleStates State {
				get { return base.State; }
			}

			public override string Value {
				get { return base.Value; }
			}
			
			public override void DoDefaultAction ()
			{
				if (Owner.DataGridView != null)
					Owner.DataGridView.SelectAll();
			}

			public override AccessibleObject Navigate (AccessibleNavigation navigationDirection)
			{
				throw new NotImplementedException ();
			}

			public override void Select (AccessibleSelection flags)
			{
				base.Select (flags);
			}
		}
	}
}

