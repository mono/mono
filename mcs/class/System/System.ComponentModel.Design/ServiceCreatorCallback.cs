// System.ComponentModel.Design.ServiceCreatorCallback.cs
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
	public delegate object ServiceCreatorCallback (IServiceContainer container, Type serviceType);
}
