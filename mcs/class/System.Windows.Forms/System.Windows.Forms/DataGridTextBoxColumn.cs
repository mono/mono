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
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop) : base(prop)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, bool isDefault) : base(prop)
		{
			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, string format) : base(prop)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public DataGridTextBoxColumn(PropertyDescriptor prop, string format, bool isDefault) : base(prop)
		{
			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void EndEdit()
		{
			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void EnterNullValue()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override int GetMinimumHeight()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override int GetPreferredHeight(Graphics g, object value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override Size GetPreferredSize(Graphics g, object value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void HideEditBox()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum)
		{
			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight)
		{
			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum,
		                                        Brush backBrush, Brush foreBrush, bool alignToRight) {

			// This method is internal to the .NET framework.
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void PaintText(Graphics g, Rectangle bounds, string text, bool alignToRight)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected void PaintText(Graphics g, Rectangle textBounds, string text, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetDataGridInColumn(DataGrid value)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected internal override void UpdateUI(CurrencyManager source, int rowNum, string instantText)
		{
			throw new NotImplementedException ();
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

			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public override bool ReadOnly {

			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		[MonoTODO]
		public virtual TextBox TextBox {

			get { throw new NotImplementedException (); }
		}
	}
}
