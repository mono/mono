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
using System.Globalization;

namespace System.Windows.Forms {

	[Editor ("System.Windows.Forms.Design.DataGridViewCellStyleEditor, " + Consts.AssemblySystem_Design,
		 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	[TypeConverter (typeof (DataGridViewCellStyleConverter))]
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

		public DataGridViewCellStyle ()
		{
			alignment = DataGridViewContentAlignment.NotSet;
			backColor = Color.Empty;
			dataSourceNullValue = DBNull.Value;
			font = null;
			foreColor = Color.Empty;
			format = String.Empty;
			nullValue = string.Empty;
			padding = Padding.Empty;
			selectionBackColor = Color.Empty;
			selectionForeColor = Color.Empty;
			tag = null;
			wrapMode = DataGridViewTriState.NotSet;
		}

		public DataGridViewCellStyle (DataGridViewCellStyle dataGridViewCellStyle)
		{
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

		[DefaultValue (DataGridViewContentAlignment.NotSet)]
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

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.FormatStringEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public string Format {
			get { return format; }
			set {
				if (format != value) {
					format = value;
					OnStyleChanged();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public IFormatProvider FormatProvider {
			get {
				if (formatProvider == null)
					return CultureInfo.CurrentCulture;
				return formatProvider;
			}
			set {
				if (formatProvider != value) {
					formatProvider = value;
					OnStyleChanged();
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool IsDataSourceNullValueDefault {
			get { return dataSourceNullValue != null; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool IsFormatProviderDefault {
			get { return formatProvider == null; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public bool IsNullValueDefault {
			get {
				if (nullValue is string) {
					return (string) nullValue == string.Empty;
				}
				return false;
			}
		}

		[DefaultValue ("")]
		[TypeConverter (typeof (StringConverter))]
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

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public object Tag {
			get { return tag; }
			set {
				if (tag != value) {
					tag = value;
					OnStyleChanged();
				}
			}
		}

		[DefaultValue (DataGridViewTriState.NotSet)]
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

		public virtual void ApplyStyle (DataGridViewCellStyle dataGridViewCellStyle)
		{
			// We should only apply the new style if it is 'set'.
			if (dataGridViewCellStyle.alignment != DataGridViewContentAlignment.NotSet)
				this.alignment = dataGridViewCellStyle.alignment;
			if (dataGridViewCellStyle.backColor != Color.Empty)
				this.backColor = dataGridViewCellStyle.backColor;
			if (dataGridViewCellStyle.dataSourceNullValue != DBNull.Value)
				this.dataSourceNullValue = dataGridViewCellStyle.dataSourceNullValue;
			if (dataGridViewCellStyle.font != null)
				this.font = dataGridViewCellStyle.font;
			if (dataGridViewCellStyle.foreColor != Color.Empty)
				this.foreColor = dataGridViewCellStyle.foreColor;
			if (dataGridViewCellStyle.format != string.Empty)
				this.format = dataGridViewCellStyle.format;
			if (dataGridViewCellStyle.formatProvider != null)
				this.formatProvider = dataGridViewCellStyle.formatProvider;
			if (dataGridViewCellStyle.nullValue != null)
				this.nullValue = dataGridViewCellStyle.nullValue;
			if (dataGridViewCellStyle.padding != Padding.Empty)
				this.padding = dataGridViewCellStyle.padding;
			if (dataGridViewCellStyle.selectionBackColor != Color.Empty)
				this.selectionBackColor = dataGridViewCellStyle.selectionBackColor;
			if (dataGridViewCellStyle.selectionForeColor != Color.Empty)
				this.selectionForeColor = dataGridViewCellStyle.selectionForeColor;
			if (dataGridViewCellStyle.tag != null)
				this.tag = dataGridViewCellStyle.tag;
			if (dataGridViewCellStyle.wrapMode != DataGridViewTriState.NotSet)
				this.wrapMode = dataGridViewCellStyle.wrapMode;
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		public virtual DataGridViewCellStyle Clone ()
		{
			return new DataGridViewCellStyle(this);
		}

		public override bool Equals (object o)
		{
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

		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}

		public override string ToString ()
		{
			/////////////////////////////////////// COMPROBAR EN Windows ////////////////////////////////
			return "";
		}

		internal event EventHandler StyleChanged;

		internal void OnStyleChanged ()
		{
			if (StyleChanged != null) {
				StyleChanged(this, EventArgs.Empty);
			}
		}
			
		internal StringFormat SetAlignment (StringFormat format)
		{
			switch (Alignment) {
			case DataGridViewContentAlignment.BottomCenter:
			case DataGridViewContentAlignment.BottomLeft:
			case DataGridViewContentAlignment.BottomRight:
				format.LineAlignment = StringAlignment.Near;
				break;
			case DataGridViewContentAlignment.MiddleCenter:
			case DataGridViewContentAlignment.MiddleLeft:
			case DataGridViewContentAlignment.MiddleRight:
				format.LineAlignment = StringAlignment.Center;
				break;
			case DataGridViewContentAlignment.TopCenter:
			case DataGridViewContentAlignment.TopLeft:
			case DataGridViewContentAlignment.TopRight:
				format.LineAlignment = StringAlignment.Far;
				break;
			}

			switch (Alignment) {
			case DataGridViewContentAlignment.BottomCenter:
			case DataGridViewContentAlignment.MiddleCenter:
			case DataGridViewContentAlignment.TopCenter:
				format.Alignment = StringAlignment.Center;
				break;
			case DataGridViewContentAlignment.BottomLeft:
			case DataGridViewContentAlignment.MiddleLeft:
			case DataGridViewContentAlignment.TopLeft:
				format.Alignment = StringAlignment.Near;
				break;

			case DataGridViewContentAlignment.BottomRight:
			case DataGridViewContentAlignment.MiddleRight:
			case DataGridViewContentAlignment.TopRight:
				format.Alignment = StringAlignment.Far;
				break;
			}
			return format;
		}
	}

}

