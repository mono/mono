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
		void GetCompiledState (out byte [] pe, out byte [] debugInfo);

		
		//[Guid ("")]
		object GetEventSourceInstance (string itemName, string eventSourceName);


		//[Guid ("")]
		object GetGlobalInstance (string name);


		//[Guid ("")]
		void Notify (string notify, object info);


		//[Guid ("")]
		bool OnCompilerError (IVsaError error);
	}
}		
