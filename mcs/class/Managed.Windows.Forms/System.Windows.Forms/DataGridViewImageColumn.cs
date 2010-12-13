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

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	[ToolboxBitmap ("")]
	public class DataGridViewImageColumn : DataGridViewColumn {

		private Icon icon;
		private Image image;
		private bool valuesAreIcons;

		public DataGridViewImageColumn () : this(false)
		{
		}

		public DataGridViewImageColumn (bool valuesAreIcons)
		{
			this.valuesAreIcons = valuesAreIcons;
			base.CellTemplate = new DataGridViewImageCell(valuesAreIcons);
			(base.CellTemplate as DataGridViewImageCell).ImageLayout = DataGridViewImageCellLayout.Normal;
			DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
			icon = null;
			image = null;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewImageCell; }
		}

		[Browsable (true)]
		public override DataGridViewCellStyle DefaultCellStyle {
			get { return base.DefaultCellStyle; }
			set { base.DefaultCellStyle = value; }
		}

		[Browsable (true)]
		[DefaultValue ("")]
		public string Description {
			get { return (base.CellTemplate as DataGridViewImageCell).Description; }
			set { (base.CellTemplate as DataGridViewImageCell).Description = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Icon Icon {
			get { return icon; }
			set { icon = value; }
		}

		[DefaultValue (null)]
		public Image Image {
			get { return image; }
			set { image = value; }
		}

		[DefaultValue (DataGridViewImageCellLayout.Normal)]
		public DataGridViewImageCellLayout ImageLayout {
			get { return (base.CellTemplate as DataGridViewImageCell).ImageLayout; }
			set { (base.CellTemplate as DataGridViewImageCell).ImageLayout = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool ValuesAreIcons {
			get { return valuesAreIcons; }
			set { valuesAreIcons = value; }
		}

		public override object Clone ()
		{
			DataGridViewImageColumn col = (DataGridViewImageColumn) base.Clone();
			col.icon = this.icon;
			col.image = this.image;
			return col;
		}

		public override string ToString ()
		{
			return GetType().Name;
		}

	}

}
