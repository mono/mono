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
using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewImageCell : DataGridViewCell {

		private object defaultNewRowValue;
		private string description;
		private DataGridViewImageCellLayout imageLayout;
		private bool valueIsIcon;

		private static Image missing_image;
		
		public DataGridViewImageCell (bool valueIsIcon) {
			this.valueIsIcon = valueIsIcon;
			this.imageLayout = DataGridViewImageCellLayout.NotSet;
		}

		public DataGridViewImageCell () : this(false) {
		}

		static DataGridViewImageCell ()
		{
			missing_image = ResourceImageLoader.Get ("image-missing.png");
		}
		
		public override object DefaultNewRowValue {
			get { return missing_image; }
		}

		[DefaultValue ("")]
		public string Description {
			get { return description; }
			set { description = value; }
		}

		public override Type EditType {
			get { return null; }
		}

		public override Type FormattedValueType {
			get { return (valueIsIcon)? typeof(Icon) : typeof(Image); }
		}

		[DefaultValue (DataGridViewImageCellLayout.NotSet)]
		public DataGridViewImageCellLayout ImageLayout {
			get { return imageLayout; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewImageCellLayout), value)) {
					throw new InvalidEnumArgumentException("Value is invalid image cell layout.");
				}
				imageLayout = value;
			}
		}

		[DefaultValue (false)]
		public bool ValueIsIcon {
			get { return valueIsIcon; }
			set { valueIsIcon = value; }
		}

		public override Type ValueType {
			get {
				if (base.ValueType != null) {
					return base.ValueType;
				}
				if (OwningColumn != null && OwningColumn.ValueType != null) {
					return OwningColumn.ValueType;
				}
				if (valueIsIcon) {
					return typeof(Icon);
				}
				else {
					return typeof(Image);
				}
			}
			set { base.ValueType = value; }
		}

		public override object Clone () {
			DataGridViewImageCell cell = (DataGridViewImageCell) base.Clone();
			cell.defaultNewRowValue = this.defaultNewRowValue;
			cell.description = this.description;
			cell.valueIsIcon = this.valueIsIcon;
			return cell;
		}

		public override string ToString () {
			return GetType().Name;
		}

		protected override AccessibleObject CreateAccessibilityInstance () {
			return new DataGridViewImageCellAccessibleObject(this);
		}

		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;
				
			Rectangle image_bounds = Rectangle.Empty;
			Image i = (Image)GetFormattedValue (Value, rowIndex, ref cellStyle, null, null, DataGridViewDataErrorContexts.PreferredSize);
			
			if (i == null)
				i = missing_image;

			switch (imageLayout) {
				case DataGridViewImageCellLayout.NotSet:
				case DataGridViewImageCellLayout.Normal:
					image_bounds = new Rectangle ((Size.Width - i.Width) / 2, (Size.Height - i.Height) / 2, i.Width, i.Height);
					break;
				case DataGridViewImageCellLayout.Stretch:
					image_bounds = new Rectangle (Point.Empty, Size);
					break;
				case DataGridViewImageCellLayout.Zoom:
					Size image_size;

					if (((float)i.Width / (float)i.Height) >= ((float)Size.Width / (float)Size.Height))
						image_size = new Size (Size.Width, (i.Height * Size.Width) / i.Width);
					else
						image_size = new Size ((i.Width * Size.Height) / i.Height, Size.Height);

					image_bounds = new Rectangle ((Size.Width - image_size.Width) / 2, (Size.Height - image_size.Height) / 2, image_size.Width, image_size.Height);
					break;
			}
			
			return image_bounds;
		}

		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null || string.IsNullOrEmpty (ErrorText))
				return Rectangle.Empty;

			Size error_icon = new Size (12, 11);
			return new Rectangle (new Point (Size.Width - error_icon.Width - 5, (Size.Height - error_icon.Height) / 2), error_icon);
		}

		protected override object GetFormattedValue (object value, int rowIndex, ref DataGridViewCellStyle cellStyle, TypeConverter valueTypeConverter, TypeConverter formattedValueTypeConverter, DataGridViewDataErrorContexts context)
		{
			return base.GetFormattedValue (value, rowIndex, ref cellStyle, valueTypeConverter, formattedValueTypeConverter, context);
		}

		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			Image i = (Image)FormattedValue;
			
			if (i == null)
				return new Size (21, 20);
				
			if (i != null)
				return new Size (i.Width + 1, i.Height + 1);

			return new Size (21, 20);
		}

		protected override object GetValue (int rowIndex)
		{
			return base.GetValue (rowIndex);
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates elementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint (graphics, clipBounds, cellBounds, rowIndex, elementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
		
		internal override void PaintPartContent (Graphics graphics, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, object formattedValue)
		{
			Image i;
			
			if (formattedValue == null)
				i = missing_image;
			else
				i = (Image)formattedValue;

			Rectangle image_bounds = Rectangle.Empty;

			switch (imageLayout) {
				case DataGridViewImageCellLayout.NotSet:
				case DataGridViewImageCellLayout.Normal:
					image_bounds = AlignInRectangle (new Rectangle (2, 2, cellBounds.Width - 4, cellBounds.Height - 4), i.Size, cellStyle.Alignment);
					break;
				case DataGridViewImageCellLayout.Stretch:
					image_bounds = new Rectangle (Point.Empty, cellBounds.Size);
					break;
				case DataGridViewImageCellLayout.Zoom:
					Size image_size;

					if (((float)i.Width / (float)i.Height) >= ((float)Size.Width / (float)Size.Height))
						image_size = new Size (Size.Width, (i.Height * Size.Width) / i.Width);
					else
						image_size = new Size ((i.Width * Size.Height) / i.Height, Size.Height);

					image_bounds = new Rectangle ((Size.Width - image_size.Width) / 2, (Size.Height - image_size.Height) / 2, image_size.Width, image_size.Height);
					break;
				default:
					break;
			}

			image_bounds.X += cellBounds.Left;
			image_bounds.Y += cellBounds.Top;
			
			graphics.DrawImage (i, image_bounds);
		}
		
		protected class DataGridViewImageCellAccessibleObject : DataGridViewCellAccessibleObject {

			public DataGridViewImageCellAccessibleObject (DataGridViewCell owner) : base(owner) {
			}

			public override string DefaultAction {
				get { return ""; }
			}

			public override string Description {
				get { return (Owner as DataGridViewImageCell).Description; }
			}

			public override string Value {
				get { return base.Value; }
				set { base.Value = value; }
			}
			
			public override void DoDefaultAction () {
				// The DataGridViewImageCell has no default action.
			}

			public override int GetChildCount () {
				return -1;
			}

		}

	}

}

