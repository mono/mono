
// System.Runtime.InteropServices/UCOMITypeLib.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	//[Guid("")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMITypeLib { 
		void FindName( string szNameBuf, int lHashVal, UCOMITypeInfo[] ppTInfo, int[] rgMemId, ref short pcFound);
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

