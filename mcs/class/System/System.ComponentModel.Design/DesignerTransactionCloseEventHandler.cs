// System.ComponentModel.Design.DesignerTransactionCloseEventHandler.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[Serializable]
	[ComVisible(true)]
	public delegate void DesignerTransactionCloseEventHandler (object sender, DesignerTransactionCloseEventArgs e);
}
