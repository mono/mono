// System.Drawing.Design.ToolboxComponentsCreatingEventArgs.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
// 

namespace System.Drawing.Design
{
	public class ToolboxComponentsCreatingEventArgs : EventArgs
	{
		private IDesignerHost host;
		
		public ToolboxComponentsCreatingEventArgs (IDesignerHost host)
		{
			this.host = host;
		}

		public IDesignerHost DesignerHost {
			get {
				return host;				
			}
		}
	}	
}


