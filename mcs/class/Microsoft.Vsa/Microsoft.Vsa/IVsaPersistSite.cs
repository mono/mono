//
// IVsaPersistSite.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaPersistSite
	{
		// public methods

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		string LoadElement (string name);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void SaveElement (string name, string source);
	}
}
