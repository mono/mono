//
// System.Runtime.InteropServices.ComTypes.ITypeInfo2.cs
//
// Author:
//   Kazuki Oikawa (kazuki@panicode.com)
//

#if NET_2_0
using System;

namespace System.Runtime.InteropServices.ComTypes
{
	[ComImport]
	[Guid("00020412-0000-0000-C000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITypeInfo2 : ITypeInfo
	{
		void GetTypeKind (out TYPEKIND pTypeKind);
		void GetTypeFlags (out int pTypeFlags);
		void GetFuncIndexOfMemId (int memid, INVOKEKIND invKind, out int pFuncIndex);
		void GetVarIndexOfMemId (int memid, out int pVarIndex);
		void GetCustData (ref Guid guid, out object pVarVal);
		void GetFuncCustData(int index, ref Guid guid, out object pVarVal);
		void GetParamCustData(int indexFunc, int indexParam, ref Guid guid, out object pVarVal);
		void GetVarCustData(int index, ref Guid guid, out object pVarVal);
		void GetImplTypeCustData(int index, ref Guid guid, out object pVarVal);
		[LCIDConversion (1)]
		void GetDocumentation2(int memid, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll);
		void GetAllCustData(IntPtr pCustData);
		void GetAllFuncCustData(int index, IntPtr pCustData);
		void GetAllParamCustData(int indexFunc, int indexParam, IntPtr pCustData);
		void GetAllVarCustData(int index, IntPtr pCustData);
		void GetAllImplTypeCustData(int index, IntPtr pCustData);
	}
}
#endif