// System.Drawing.Design.ToolboxComponentsCreatedEventArgs.cs
// 
// Author:
//      Alejandro Sánchez Acosta  <raciel@es.gnu.org>
// 
// (C) Alejandro Sánchez Acosta
// 
//

using System.ComponentModel;

namespace System.Drawing.Design
{
	public class ToolboxComponentsCreatedEventArgs : EventArgs
	{
		private IComponent[] components;
		
		public ToolboxComponentsCreatedEventArgs (IComponent[] components) {
			this.components = components;
		}

		public IComponent[] Components {
			get {
				return components;
			}
		}	
	}
}
