// System.ComponentModel.Design.ActiveDesignerEventArgs.cs
//
// Author:
// 	Alejandro Sánchez Acosta  <raciel@es.gnu.org>
//
// (C) Alejandro Sánchez Acosta
// 

using System.Runtime.Serialization;

namespace System.ComponentModel.Design
{
	public class ActiveDesignerEventArgs : EventArgs
	{
		private IDesignerHost oldDesigner;
		private IDesignerHost newDesigner;
		
		public ActiveDesignerEventArgs (IDesignerHost oldDesigner, IDesignerHost newDesigner) {
			this.oldDesigner = oldDesigner;
			this.newDesigner = newDesigner;
		}

		public IDesignerHost NewDesigner {
			get {
				return newDesigner;
			}

			set {
				newDesigner = value;
			}
		}

		public IDesignerHost OldDesigner {
			get {
				return oldDesigner;
			}

			set {
				newDesigner = value;
			}			
		}
	}
}
