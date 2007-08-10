//
// System.Runtime.InteropServices.ComTypes.ITypeLib2.cs
//
// Author:
//   Kazuki Oikawa (kazuki@panicode.com)
//

#if NET_2_0
using System;

namespace System.Runtime.InteropServices.ComTypes
{
	[ComImport]
	[Guid ("00020411-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeLib2 : ITypeLib
	{
		new void FindName ([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal, [Out, MarshalAs (UnmanagedType.LPArray)] ITypeInfo[] ppTInfo, [Out, MarshalAs (UnmanagedType.LPArray)] int[] rgMemId, ref short pcFound);
		void GetCustData(ref Guid guid, out object pVarVal);
		new void GetDocumentation (int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
		new void GetLibAttr (out IntPtr ppTLibAttr);
		void GetLibStatistics(IntPtr pcUniqueNames, out int pcchUniqueNames);
		[LCIDConversion(1)]
		void GetDocumentation2(int index, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll);
		void GetAllCustData(IntPtr pCustData);
		new void GetTypeComp (out ITypeComp ppTComp);
		new void GetTypeInfo (int index, out ITypeInfo ppTI);
		new void GetTypeInfoOfGuid (ref Guid guid, out ITypeInfo ppTInfo);
		new void GetTypeInfoType (int index, out TYPEKIND pTKind);
		[return: MarshalAs (UnmanagedType.Bool)]
		new bool IsName ([MarshalAs(UnmanagedType.LPWStr)] string szNameBuf, int lHashVal);
		[PreserveSig]
		new void ReleaseTLibAttr (IntPtr pTLibAttr);
		// undocumented
		[PreserveSig]
		new int GetTypeInfoCount ();
	}
}
#endif