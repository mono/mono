
// System.Runtime.InteropServices.UCOMITypeLib.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;
using System.Runtime.InteropServices;

namespace System.Runtime.InteropServices
{

	[Guid("00020402-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMITypeLib { 
		void FindName( string szNameBuf, int lHashVal, [Out] UCOMITypeInfo[] ppTInfo, [Out] int[] rgMemId, ref short pcFound);
		void GetDocumentation( int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
		void GetLibAttr( out IntPtr ppTLibAttr);
		void GetTypeComp( out UCOMITypeComp ppTComp); 
		void GetTypeInfo( int index, out UCOMITypeInfo ppTI);
		int GetTypeInfoCount();
		void GetTypeInfoOfGuid( ref Guid guid, out UCOMITypeInfo ppTInfo);
		void GetTypeInfoType( int index, out TYPEKIND pTKind);
		bool IsName( string szNameBuf, int lHashVal);
		void ReleaseTLibAttr( IntPtr pTLibAttr);
	}
}

