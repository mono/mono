
// System.Runtime.InteropServices.UCOMITypeInfo.cs
//
// Paolo Molaro (lupus@ximian.com)
//
// (C) 2002 Ximian, Inc.

using System;

namespace System.Runtime.InteropServices
{

	[Guid("00020401-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface UCOMITypeInfo {
		void AddressOfMember (int memid, INVOKEKIND invKind, out IntPtr ppv); 
		void CreateInstance (object pUnkOuter, ref Guid riid, out object ppvObj);
		void GetContainingTypeLib (out UCOMITypeLib ppTLB, out int pIndex);
		void GetDllEntry (int memid, INVOKEKIND invKind, out string pBstrDllName, out string pBstrName, out short pwOrdinal);
		void GetDocumentation (int index, out string strName, out string strDocString, out int dwHelpContext, out string strHelpFile);
		void GetFuncDesc (int index, out IntPtr ppFuncDesc);
		void GetIDsOfNames ([In] string[] rgszNames, int cNames, [Out] int[] pMemId);
		void GetImplTypeFlags (int index, out int pImplTypeFlags);
		void GetMops (int memid, out string pBstrMops);
		void GetNames (int memid, [Out] string[] rgBstrNames, int cMaxNames, out int pcNames);
		void GetRefTypeInfo (int hRef, out UCOMITypeInfo ppTI);
		void GetRefTypeOfImplType (int index, out int href);
		void GetTypeAttr (out IntPtr ppTypeAttr);
		void GetTypeComp (out UCOMITypeComp ppTComp);
		void GetVarDesc (int index, out IntPtr ppVarDesc);
		void Invoke (object pvInstance, int memid, short wFlags, ref DISPPARAMS pDispParams, out object pVarResult, out EXCEPINFO pExcepInfo, out int puArgErr);
		void ReleaseFuncDesc (IntPtr pFuncDesc);
		void ReleaseTypeAttr (IntPtr pTypeAttr);
		void ReleaseVarDesc (IntPtr pVarDesc);
	}
}

