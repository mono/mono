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

namespace System.Windows.Forms
{
	public class DataGridTextBoxColumn : DataGridColumnStyle
	{
		#region	Local Variables
		private string format;
		private IFormatProvider format_provider = null;
		private StringFormat string_format =  new StringFormat ();
		private DataGridTextBox textbox = null;
		private static readonly int offset_x = 2;
		private static readonly int offset_y = 2;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBoxColumn ()
		{
			format = string.Empty;
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop) : base (prop)
		{
			format = string.Empty;
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  bool isDefault) : base (prop)
		{
			format = string.Empty;
			is_default = isDefault;
		}

		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format) : base (prop)
		{
			this.format = format;
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format, bool isDefault) : base (prop)
		{
			this.format = format;
			is_default = isDefault;
		}

		#endregion

		#region Public Instance Properties
		[Editor("System.Windows.Forms.Design.DataGridColumnStyleFormatEditor, " + Consts.AssemblySystem_Design, typeof(System.Drawing.Design.UITypeEditor))]
		public string Format {
			get {
				return format;
			}
			set {
				if (value != format) {
					format = value;
				}
			}
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public IFormatProvider FormatInfo {
			get {
				return format_provider;
			}
			set {
				if (value != format_provider) {
					format_provider = value;
				}
			}
		}

		[DefaultValue(null)]
		public override PropertyDescriptor PropertyDescriptor {
			set {
				base.PropertyDescriptor = value;
			}
		}

		public override bool ReadOnly {
			get {
				return base.ReadOnly;
			}
			set {
				base.ReadOnly = value;
			}
		}
		
		[Browsable(false)]
		public virtual TextBox TextBox {
			get {
				EnsureTextBox();
				return textbox;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods


		protected internal override void Abort (int rowNum)
		{
			EndEdit ();			
		}
		
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			string text;
			
			if (textbox.Text == NullText) {
				text = string.Empty;
			} else {
				text = textbox.Text;
			}
			
			try {
				SetColumnValueAtRow (dataSource, rowNum, text);
			}
			
			catch (Exception e) {
				string message = "The data entered in column ["+ MappingName +"] has an invalid format.";
				MessageBox.Show( message);
			}
			
			
			EndEdit ();			
			return true;
		}

		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{

		}

		protected internal override void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool _ro, string instantText, bool cellIsVisible)
		{
			object obj;
			
			EnsureTextBox();

			textbox.TextAlign = alignment;
			
			if ((ParentReadOnly == true)  || 
				(ParentReadOnly == false && ReadOnly == true) || 
				(ParentReadOnly == false && _ro == true)) {
				textbox.ReadOnly = true;
			} else {
				textbox.ReadOnly = false;
			}			
			
			textbox.Location = new Point (bounds.X + offset_x, bounds.Y + offset_y);
			textbox.Size = new Size (bounds.Width - offset_x, bounds.Height - offset_y);

			obj = GetColumnValueAtRow (source, rowNum);
			textbox.Text = GetFormattedString (obj);

			textbox.Visible = cellIsVisible;
			textbox.Focus ();
			textbox.SelectAll ();			
		}

		protected void EndEdit ()
		{
			ReleaseHostedControl ();
			grid.Invalidate (grid.GetCurrentCellBounds ());
		}

		protected internal override void EnterNullValue ()
		{
			if (textbox != null) {
				textbox.Text = NullText;
			}
		}

		protected internal override int GetMinimumHeight ()
		{
			return FontHeight + 3;
		}

		[MonoTODO]
		protected internal override int GetPreferredHeight (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override Size GetPreferredSize (Graphics g, object value)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected void HideEditBox ()
		{

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
			object obj;
			obj = GetColumnValueAtRow (source, rowNum);

			PaintText (g, bounds, GetFormattedString (obj),  backBrush, foreBrush, alignToRight);
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
			
			textBounds.Y += offset_y;
			textBounds.Height -= offset_y;
			
			string_format.FormatFlags |= StringFormatFlags.NoWrap;
			g.DrawString (text, DataGridTableStyle.DataGrid.Font, foreBrush, textBounds, string_format);
			
		}
		
		protected internal override void ReleaseHostedControl ()
		{			
			if (textbox != null) {
				DataGridTableStyle.DataGrid.Controls.Remove (textbox);
				textbox.Dispose ();
				textbox = null;
			}
		}

		protected override void SetDataGridInColumn (DataGrid value)
		{
			base.SetDataGridInColumn (value);
		}
		
		protected internal override void UpdateUI (CurrencyManager source, int rowNum, string instantText)
		{

		}

		#endregion	// Public Instance Methods


		#region Private Instance Methods

		// We use DataGridTextBox to render everything that DataGridBoolColumn does not
		internal static bool CanRenderType (Type type)
		{
			return (type != typeof (Boolean));
		}

		private string GetFormattedString (object obj)
		{
			if (obj == DBNull.Value) {
				return NullText;
			}
			
			if (format != null && obj as IFormattable != null) {
				return ((IFormattable)obj).ToString (format, format_provider);
			}

			return obj.ToString ();

		}

		private void EnsureTextBox() {
			if (textbox == null) {
				textbox = new DataGridTextBox ();
				textbox.SetDataGrid (DataGridTableStyle.DataGrid);
				textbox.Multiline = true;
				textbox.BorderStyle = BorderStyle.None;
				textbox.Visible = false;

				DataGridTableStyle.DataGrid.Controls.Add (textbox);
			}			
		}
		#endregion Private Instance Methods
	}
}
