//
// System.Runtime.InteropServices.UCOMIConnectionPoint.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Guid ("b196b286-bab4-101a-b69c-00aa00341d07")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMIConnectionPoint
	{
		void Advise (object pUnkSink, ref int pdwCookie);
		void EnumConnections (ref UCOMIEnumConnections ppEnum);
		void GetConnectionInterface (ref Guid pIID);
		void GetConnectionPointContainer (ref UCOMIConnectionPointContainer ppCPC);
		void Unadvise (int dwCookie);
	}
}
