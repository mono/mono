// System.ComponentModel.Design.Serialization.IDesignerLoaderHost.cs
//
// Author:
// 	Alejandro Sánchez Acosta   <raciel@gnome.org>
//
// (C) Alejandro Sánchez Acosta
//

using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public interface IDesignerLoaderHost : IDesignerHost, IServiceContainer, IServiceProvider
	{
		void EndLoad (string baseClassName, bool successful, ICollection errorCollection);

		void Reload();
	}
}
