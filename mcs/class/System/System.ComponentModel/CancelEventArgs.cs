//------------------------------------------------------------------------------
// 
// System.ComponentModel.CancelEventArgs.
//
// Author:  Asier Llano Palacios, asierllano@infonegocio.com
//
//------------------------------------------------------------------------------

using System;

namespace System.ComponentModel {

	public class CancelEventArgs : EventArgs
	{
		private bool cancel;
	
		public CancelEventArgs() { 
			cancel = false;
		}

		public CancelEventArgs( bool cancel )
		{
			this.cancel = cancel;
		}

		public bool Cancel {
			get {
				return cancel;
			}
			set {
				cancel = value;
			}
		}
	}

}


