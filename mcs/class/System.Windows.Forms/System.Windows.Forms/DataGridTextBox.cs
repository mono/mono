//
// System.Windows.Forms.DataGridTextBox
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

//
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
