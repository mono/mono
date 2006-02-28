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

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewCellStyle : ICloneable {

		private DataGridViewContentAlignment alignment;
		private Color backColor;
		private object dataSourceNullValue;
		private Font font;
		private Color foreColor;
		private string format;
		private IFormatProvider formatProvider;
		private object nullValue;
		private Padding padding;
		private Color selectionBackColor;
		private Color selectionForeColor;
		private object tag;
		private DataGridViewTriState wrapMode;

		public DataGridViewCellStyle () {
			alignment = DataGridViewContentAlignment.NotSet;
			backColor = Color.Empty;
			font = null;
			foreColor = Color.Empty;
			format = String.Empty;
			formatProvider =  System.Globalization.CultureInfo.CurrentUICulture;
			nullValue = "(null)";
			padding = Padding.Empty;
			selectionBackColor = Color.Empty;
			selectionForeColor = Color.Empty;
			tag = null;
			wrapMode = DataGridViewTriState.NotSet;
		}

		public DataGridViewCellStyle (DataGridViewCellStyle dataGridViewCellStyle) {
			ApplyStyle(dataGridViewCellStyle);
		}

		public DataGridViewContentAlignment Alignment {
			get { return alignment; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewContentAlignment), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewContentAlignment.");
				}
				if (alignment != value) {
					alignment = value;
					OnStyleChanged();
				}
			}
		}

		public Color BackColor {
			get { return backColor; }
			set {
				if (backColor != value) {
					backColor = value;
					OnStyleChanged();
				}
			}
		}

		public object DataSourceNullValue {
			get { return dataSourceNullValue; }
			set {
				if (dataSourceNullValue != value) {
					dataSourceNullValue = value;
					OnStyleChanged();
				}
			}
		}

		public Font Font {
			get { return font; }
			set {
				if (font != value) {
					font = value;
					OnStyleChanged();
				}
			}
		}

		public Color ForeColor {
			get { return foreColor; }
			set {
				if (foreColor != value) {
					foreColor = value;
					OnStyleChanged();
				}
			}
		}

		public string Format {
			get { return format; }
			set {
				if (format != value) {
					format = value;
					OnStyleChanged();
				}
			}
		}

		public IFormatProvider FormatProvider {
			get { return formatProvider; }
			set {
				if (formatProvider != value) {
					formatProvider = value;
					OnStyleChanged();
				}
			}
		}

		public bool IsDataSourceNullValueDefault {
			get { return dataSourceNullValue != null; }
		}

		public bool IsFormatProviderDefault {
			get { return formatProvider ==  System.Globalization.CultureInfo.CurrentUICulture; }
		}

		public bool IsNullValueDefault {
			get {
				if (nullValue is string) {
					return (string) nullValue == "(null)";
				}
				return false;
			}
		}

		public object NullValue {
			get { return nullValue; }
			set {
				if (nullValue != value) {
					nullValue = value;
					OnStyleChanged();
				}
			}
		}

		public Padding Padding {
			get { return padding; }
			set {
				if (padding != value) {
					padding = value;
					OnStyleChanged();
				}
			}
		}

		public Color SelectionBackColor {
			get { return selectionBackColor; }
			set {
				if (value != Color.Empty && (int) value.A != 255) {
					throw new ArgumentException("BackColor can't have alpha transparency component.");
				}
				if (selectionBackColor != value) {
					selectionBackColor = value;
					OnStyleChanged();
				}
			}
		}

		public Color SelectionForeColor {
			get { return selectionForeColor; }
			set {
				if (selectionForeColor != value) {
					selectionForeColor = value;
					OnStyleChanged();
				}
			}
		}

		public object Tag {
			get { return tag; }
			set {
				if (tag != value) {
					tag = value;
					OnStyleChanged();
				}
			}
		}

		public DataGridViewTriState WrapMode {
			get { return wrapMode; }
			set {
				if (!Enum.IsDefined(typeof(DataGridViewTriState), value)) {
					throw new InvalidEnumArgumentException("Value is not valid DataGridViewTriState.");
				}
				if (wrapMode != value) {
					wrapMode = value;
					OnStyleChanged();
				}
			}
		}

		public virtual void ApplyStyle (DataGridViewCellStyle dataGridViewCellStyle) {
			this.alignment = dataGridViewCellStyle.alignment;
			this.backColor = dataGridViewCellStyle.backColor;
			this.dataSourceNullValue = dataGridViewCellStyle.dataSourceNullValue;
			this.font = dataGridViewCellStyle.font;
			this.foreColor = dataGridViewCellStyle.foreColor;
			this.format = dataGridViewCellStyle.format;
			this.formatProvider = dataGridViewCellStyle.formatProvider;
			this.nullValue = dataGridViewCellStyle.nullValue;
			this.padding = dataGridViewCellStyle.padding;
			this.selectionBackColor = dataGridViewCellStyle.selectionBackColor;
			this.selectionForeColor = dataGridViewCellStyle.selectionForeColor;
			this.tag = dataGridViewCellStyle.tag;
			this.wrapMode = dataGridViewCellStyle.wrapMode;
		}

		public virtual object Clone () {
			return new DataGridViewCellStyle(this);
		}

		public override bool Equals (object o) {
			if (o is DataGridViewCellStyle) {
				DataGridViewCellStyle o_aux = (DataGridViewCellStyle) o;
				return this.alignment == o_aux.alignment &&
					this.backColor == o_aux.backColor &&
					this.dataSourceNullValue == o_aux.dataSourceNullValue &&
					this.font == o_aux.font &&
					this.foreColor == o_aux.foreColor &&
					this.format == o_aux.format &&
					this.formatProvider == o_aux.formatProvider &&
					this.nullValue == o_aux.nullValue &&
					this.padding == o_aux.padding &&
					this.selectionBackColor == o_aux.selectionBackColor &&
					this.selectionForeColor == o_aux.selectionForeColor &&
					this.tag == o_aux.tag &&
					this.wrapMode == o_aux.wrapMode;
			}
			return false;
		}

		public override int GetHashCode () {
			return base.GetHashCode();
		}

		public override string ToString () {
			/////////////////////////////////////// COMPROBAR EN Windows ////////////////////////////////
			return "";
		}

		internal event EventHandler StyleChanged;

		internal void OnStyleChanged () {
			if (StyleChanged != null) {
				StyleChanged(this, EventArgs.Empty);
			}
		}

	}

}

#endif
