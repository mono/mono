// System.ComponentModel.Design.DesignerTransactionCloseEventArgs.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.InteropServices;

namespace System.ComponentModel.Design
{
	[ComVisible(true)]
	public class DesignerTransactionCloseEventArgs : EventArgs
	{
		private bool commit;
		public DesignerTransactionCloseEventArgs (bool commit) {
			this.commit = commit;
		}

		public bool TransactionCommitted 
		{
			get {
				return commit;
			}
		}
	}
}
