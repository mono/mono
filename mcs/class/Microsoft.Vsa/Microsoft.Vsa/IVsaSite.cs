//
// IVsaSite.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;
	using System.Runtime.InteropServices;


	//[Guid ("")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface IVsaSite
	{
		// public methods

		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void GetCompiledState (out byte [] pe, out byte [] debugInfo);

		
		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		object GetEventSourceInstance (string itemName, string eventSourceName);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		object GetGlobalInstance (string name);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		void Notify (string notify, object info);


		//[Guid ("")]
		[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
		bool OnCompilerError (IVsaError error);
	}
}		
