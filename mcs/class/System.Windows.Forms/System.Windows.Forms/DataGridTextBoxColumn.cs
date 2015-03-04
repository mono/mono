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

using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	public class DataGridTextBoxColumn : DataGridColumnStyle
	{
		#region	Local Variables
		private string format;
		private IFormatProvider format_provider = null;
		private StringFormat string_format =  new StringFormat ();
		private DataGridTextBox textbox;
		private static readonly int offset_x = 2;
		private static readonly int offset_y = 2;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBoxColumn () : this (null, String.Empty, false)
		{
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop) : this (prop, String.Empty, false)
		{
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  bool isDefault) : this (prop, String.Empty, isDefault)
		{
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format) : this (prop, format, false)
		{
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format, bool isDefault) : base (prop)
		{
			Format = format;
			is_default = isDefault;

			textbox = new DataGridTextBox ();
			textbox.Multiline = true;
			textbox.WordWrap = false;
			textbox.BorderStyle = BorderStyle.None;
			textbox.Visible = false;
		}

		#endregion

		#region Public Instance Properties
		[Editor("System.Windows.Forms.Design.DataGridColumnStyleFormatEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		[DefaultValue (null)]
		public string Format {
			get { return format; }
			set {
				if (value != format) {
					format = value;
					Invalidate ();
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IFormatProvider FormatInfo {
			get { return format_provider; }
			set {
				if (value != format_provider) {
					format_provider = value;
				}
			}
		}

		[DefaultValue(null)]
		public override PropertyDescriptor PropertyDescriptor {
			set { base.PropertyDescriptor = value; }
		}

		public override bool ReadOnly {
			get { return base.ReadOnly; }
			set { base.ReadOnly = value; }
		}
		
		[Browsable(false)]
		public virtual TextBox TextBox {
			get { return textbox; }
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods

		protected internal override void Abort (int rowNum)
		{
			EndEdit ();
		}
		
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			textbox.Bounds = Rectangle.Empty;

			/* Do not write data if not editing. */
			if (textbox.IsInEditOrNavigateMode)
				return true;

			try {
				string existing_text = GetFormattedValue (dataSource, rowNum);

				if (existing_text != textbox.Text) {
					if (textbox.Text == NullText) {
						SetColumnValueAtRow (dataSource, rowNum, DBNull.Value);
					} else {
						object newValue = textbox.Text;

						TypeConverter converter = TypeDescriptor.GetConverter (
							PropertyDescriptor.PropertyType);
						if (converter != null && converter.CanConvertFrom (typeof (string))) {
							newValue = converter.ConvertFrom (null, CultureInfo.CurrentCulture,
								textbox.Text);
							if (converter.CanConvertTo (typeof (string)))
								textbox.Text = (string) converter.ConvertTo (null, 
									CultureInfo.CurrentCulture, newValue, typeof (string));
						}

						SetColumnValueAtRow (dataSource, rowNum, newValue);
					}
				}
			} catch {
				return false;
			}
			
			EndEdit ();
			return true;
		}

		protected internal override void ConcedeFocus ()
		{
			HideEditBox ();
		}

		protected internal override void Edit (CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string displayText, bool cellIsVisible)
		{
			string instantText = displayText;
			grid.SuspendLayout ();

			textbox.TextChanged -= new EventHandler (textbox_TextChanged);

			textbox.TextAlign = alignment;
			
			bool ro = false;

			ro = (TableStyleReadOnly || ReadOnly || readOnly);

			if (!ro && instantText != null) {
				textbox.Text = instantText;
				textbox.IsInEditOrNavigateMode = false;
			} else {
				textbox.Text = GetFormattedValue (source, rowNum);
			}

			textbox.TextChanged += new EventHandler (textbox_TextChanged);

			textbox.ReadOnly = ro;
			textbox.Bounds = new Rectangle (new Point (bounds.X + offset_x, bounds.Y + offset_y),
							new Size (bounds.Width - offset_x - 1, bounds.Height - offset_y - 1));

			textbox.Visible = cellIsVisible;
			textbox.SelectAll ();
			textbox.Focus ();
			grid.ResumeLayout (false);

		}

		void textbox_TextChanged (object o, EventArgs e)
		{
			textbox.IsInEditOrNavigateMode = false;
			grid.EditRowChanged (this);
		}

		protected void EndEdit ()
		{
			textbox.TextChanged -= new EventHandler (textbox_TextChanged);
			HideEditBox ();
		}

		protected internal override void EnterNullValue ()
		{
			textbox.Text = NullText;
		}

		protected internal override int GetMinimumHeight ()
		{
			return FontHeight + 3;
		}

		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			string text = GetFormattedValue (value);
			System.Text.RegularExpressions.Regex r = new System.Text.RegularExpressions.Regex("/\r\n/");
			int lines = r.Matches (text).Count;
			return this.DataGridTableStyle.DataGrid.Font.Height * (lines+1) + 1;
		}

		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			string text = GetFormattedValue (value);
			Size s = Size.Ceiling (g.MeasureString (text, this.DataGridTableStyle.DataGrid.Font));
			s.Width += 4;
			return s;
		}

		protected void HideEditBox ()
		{
			if (!textbox.Visible)
				return;

			grid.SuspendLayout ();
			textbox.Bounds = Rectangle.Empty;
			textbox.Visible = false;
			textbox.IsInEditOrNavigateMode = true;
			grid.ResumeLayout (false);
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
			PaintText (g, bounds, GetFormattedValue (source, rowNum),  backBrush, foreBrush, alignToRight);
		}

		protected void PaintText (Graphics g, Rectangle bounds, string text, bool alignToRight)
		{
			PaintText (g, bounds, text,  ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.BackColor),
				ThemeEngine.Current.ResPool.GetSolidBrush (DataGridTableStyle.ForeColor), alignToRight);
		}

		protected void PaintText (Graphics g, Rectangle textBounds, string text, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			if (alignToRight == true) {
				string_format.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			} else {
				string_format.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			}
			
			switch (alignment) {
			case HorizontalAlignment.Center:
				string_format.Alignment = StringAlignment.Center;
				break;
			case HorizontalAlignment.Right:
				string_format.Alignment = StringAlignment.Far;
				break;
			default:
				string_format.Alignment = StringAlignment.Near;
				break;
			}
			
			g.FillRectangle (backBrush, textBounds);
			PaintGridLine (g, textBounds);

			textBounds.X += offset_x;
			textBounds.Width -= offset_x;

			textBounds.Y += offset_y;
			textBounds.Height -= offset_y;

			string_format.FormatFlags |= StringFormatFlags.NoWrap;
			g.DrawString (text, DataGridTableStyle.DataGrid.Font, foreBrush, textBounds, string_format);
			
		}
		
		protected internal override void ReleaseHostedControl ()
		{
			if (textbox == null)
				return;

			grid.SuspendLayout ();
			grid.Controls.Remove (textbox);
			grid.Invalidate (new Rectangle (textbox.Location, textbox.Size));
			textbox.Dispose ();
			textbox = null;
			grid.ResumeLayout (false);
		}

		protected override void SetDataGridInColumn (DataGrid value)
		{
			base.SetDataGridInColumn (value);

			if (value == null)
				return;

			textbox.SetDataGrid (grid);
			grid.SuspendLayout ();
			grid.Controls.Add (textbox);
			grid.ResumeLayout (false);
		}

		protected internal override void UpdateUI (CurrencyManager source, int rowNum, string displayText)
		{
			string instantText = displayText;
			if (textbox.Visible // I don't really like this, but it gets DataGridTextBoxColumnTest.TestUpdateUI passing
			    && textbox.IsInEditOrNavigateMode) {
				textbox.Text = GetFormattedValue (source, rowNum);
			} else {
				textbox.Text = instantText;
			}
		}

		#endregion	// Public Instance Methods

		#region Private Instance Methods

		private string GetFormattedValue (CurrencyManager source, int rowNum)
		{
			object obj = GetColumnValueAtRow (source, rowNum);
			return GetFormattedValue (obj);
		}

		private string GetFormattedValue (object obj)
		{
			if (DBNull.Value.Equals(obj) || obj == null)
				return NullText;

			if (format != null && format != String.Empty && obj as IFormattable != null)
				return ((IFormattable) obj).ToString (format, format_provider);

			TypeConverter converter = TypeDescriptor.GetConverter (
				PropertyDescriptor.PropertyType);
			if (converter != null && converter.CanConvertTo (typeof (string)))
				return (string) converter.ConvertTo (null, CultureInfo.CurrentCulture,
					obj, typeof (string));

			return obj.ToString ();

		}
		#endregion Private Instance Methods
	}
}
