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
		private IFormatProvider format_provider;		
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBoxColumn ()
		{
			
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop) : base (prop)
		{
			format = string.Empty;
		}
		
		// TODO: What is isDefault for?
		public DataGridTextBoxColumn (PropertyDescriptor prop,  bool isDefault) : base (prop)
		{
			
		}
		
		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format) : base (prop)
		{
			this.format = format;			
		}
		
		// TODO: What is isDefault for?
		public DataGridTextBoxColumn (PropertyDescriptor prop,  string format, bool isDefault) : base (prop)
		{
			this.format = format;
		}
		
		#endregion

		#region Public Instance Properties
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
		
		public PropertyDescriptor PropertyDescriptor {
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
		
		[MonoTODO]
		public virtual TextBox TextBox {
			get {
				return null;
			}			
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		
		[MonoTODO]
		protected internal override void Abort (int rowNum)
		{
			
		}
		
		[MonoTODO]
		protected internal override bool Commit (CurrencyManager dataSource, int rowNum)
		{
			throw new NotImplementedException ();	
		}
		
		[MonoTODO]
		protected internal override void ConcedeFocus ()
		{
			
		}
		
		[MonoTODO]
		protected internal override void Edit (CurrencyManager source, int rowNum,  Rectangle bounds,  bool _readonly, string instantText, bool cellIsVisible)
		{
			
		}
		
		[MonoTODO]
		protected void EndEdit ()
		{
			
		}
		
		[MonoTODO]
		protected internal override void EnterNullValue ()
		{
			
		}
		
		[MonoTODO]
		protected internal override int GetMinimumHeight ()
		{
			throw new NotImplementedException ();
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
		
		[MonoTODO]
		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			
		}
		
		[MonoTODO]
		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			
		}
		
		[MonoTODO]
		protected internal override void Paint (Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,   Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			
		}
		
		[MonoTODO]
		protected void PaintText (Graphics g, Rectangle bounds, string text,  bool alignToRight)
		{
			
		}
		
		[MonoTODO]
		protected void PaintText (Graphics g, Rectangle textBounds, string text, Brush backBrush,  Brush foreBrush, bool alignToRight)
		{
			
		}
		
		[MonoTODO]
		protected internal override void ReleaseHostedControl ()
		{
			
		}
		
		[MonoTODO]
		protected override void SetDataGridInColumn (DataGrid value)
		{
			
		}

		[MonoTODO]
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
		#endregion Private Instance Methods	


		#region Events

		#endregion	// Events
	}
}
