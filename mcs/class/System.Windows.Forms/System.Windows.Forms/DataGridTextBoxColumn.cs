//
// System.Windows.Forms.DataGridTextBoxColumn
//
// Author:
//   stubbed out by Richard Baumann (biochem333@nyc.rr.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	//	Hosts a TextBox control in a cell of a DataGridColumnStyle for editing strings.
	// </summary>
	public class DataGridTextBoxColumn : DataGridColumnStyle {

		//
		//  --- Constructors/Destructors
		//
		[MonoTODO]
		public DataGridTextBoxColumn() : base()
		{
			
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop) : base(prop)
		{
			
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, bool isDefault) : base(prop)
		{
			// This method is internal to the .NET framework.
			
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, string format) : base(prop)
		{
			
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, string format, bool isDefault) : base(prop)
		{
			// This method is internal to the .NET framework.
		}
		
		//  --- Protected Methods
		
		[MonoTODO]
		protected internal override void Abort(int rowNum)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override bool Commit(CurrencyManager dataSource, int rowNum)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void ConcedeFocus()
		{
			//FIXME:
			base.ConcedeFocus();
		}
		[MonoTODO]
		protected internal override void Edit(CurrencyManager source, int rowNum, Rectangle bounds,
		                                       bool readOnly, string instantText, bool cellIsVisible) {

			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
//		[MonoTODO]
//		//FIXME
//		protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly)
//		{
//			// This method is internal to the .NET framework.
//			throw new NotImplementedException ();
//		}
		[MonoTODO]
		protected internal virtual void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText)
		{
			// This method is internal to the .NET framework.
			//FIXME:
		}
		[MonoTODO]
		protected void EndEdit()
		{
			// This method is internal to the .NET framework.
			//FIXME:
		}
		[MonoTODO]
		protected internal override void EnterNullValue()
		{
			//FIXME:
			base.EnterNullValue();
		}
		[MonoTODO]
		protected internal override int GetMinimumHeight()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override int GetPreferredHeight(Graphics g, object value)
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override Size GetPreferredSize(Graphics g, object value)
		{
			//FIXME:
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void HideEditBox()
		{
			//FIXME:
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			// This method is internal to the .NET framework.
			//FIXME:
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			// This method is internal to the .NET framework.
			//FIXME:
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,
		                                        Brush backBrush, Brush foreBrush, bool alignToRight) {

			// This method is internal to the .NET framework.
			//FIXME:
		}
		[MonoTODO]
		protected void PaintText(Graphics g, Rectangle bounds, string text, bool alignToRight)
		{
			//FIXME:
		}
		[MonoTODO]
		protected void PaintText(Graphics g, Rectangle textBounds, string text, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void SetDataGridInColumn(DataGrid value)
		{
			//FIXME:
		}
		[MonoTODO]
		protected internal override void UpdateUI(CurrencyManager source, int rowNum, string instantText)
		{
			//FIXME:
		}

		
		//  --- Public Properties
		
		[MonoTODO]
		public string Format {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public IFormatProvider FormatInfo {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override PropertyDescriptor PropertyDescriptor {

			set { 
				//FIXME:
				base.PropertyDescriptor = value;
			}
		}
		[MonoTODO]
		public override bool ReadOnly {

			get {
				//FIXME:
				return base.ReadOnly;
			}
			set {
				//FIXME:
				base.ReadOnly = value; 
			}
		}
		[MonoTODO]
		public virtual TextBox TextBox {

			get { throw new NotImplementedException (); }
		}
	}
}
