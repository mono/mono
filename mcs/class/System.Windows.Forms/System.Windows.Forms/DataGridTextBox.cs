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
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected internal override bool ProcessKeyMessage(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void SetDataGrid(DataGrid parentGrid) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			throw new NotImplementedException ();
		}
		#endregion
	}
}
