//
// System.Windows.Forms.DataGridTextBox
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a TextBox control that is hosted in a DataGridTextBoxColumn.
	/// </summary>

	[MonoTODO]
	public class DataGridTextBox : TextBox {

		#region Fields
		bool isInEditOrNavigateMode;
		#endregion
		
		#region Constructor
		[MonoTODO]
		public DataGridTextBox() 
		{
			isInEditOrNavigateMode=true;
		}
		#endregion
		
		#region Properties
		public bool IsInEditOrNavigateMode {
			get { return isInEditOrNavigateMode; }
			set { isInEditOrNavigateMode=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		protected override void OnKeyPress(KeyPressEventArgs e) 
		{
			//FIXME:
			base.OnKeyPress(e);
		}
		
		[MonoTODO]
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			//FIXME:
			base.OnMouseWheel(e);
		}
		
		[MonoTODO]
		protected internal override bool ProcessKeyMessage(ref Message m) 
		{
			//FIXME:
			return base.ProcessKeyMessage(ref m);
		}
		
		[MonoTODO]
		public void SetDataGrid(DataGrid parentGrid) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
			base.WndProc(ref m);
		}
		#endregion
	}
}
