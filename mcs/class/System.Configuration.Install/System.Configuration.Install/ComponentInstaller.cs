// System.Configuration.Install.ComponentInstaller.cs
//
// Author:
// 	Alejandro Sánchez Acosta
//
// (C) Alejandro Sánchez Acosta
//

using System.ComponentModel;

namespace System.Configuration.Install
{
	public abstract class ComponentInstaller : Installer
	{
		[MonoTODO]
		protected ComponentInstaller () {
			throw new NotImplementedException ();
		}

		public abstract void CopyFromComponent (IComponent component);
		
		[MonoTODO]
		public virtual bool IsEquivalentInstaller (ComponentInstaller otherInstaller)
		{
			throw new NotImplementedException ();	
		}
	}
}
