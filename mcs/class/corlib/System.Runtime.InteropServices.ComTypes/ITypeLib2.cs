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
		void GetCustData(ref Guid guid, out object pVarVal);
		void GetLibStatistics(IntPtr pcUniqueNames, out int pcchUniqueNames);
		[LCIDConversion(1)]
		void GetDocumentation2(int index, out string pbstrHelpString, out int pdwHelpStringContext, out string pbstrHelpStringDll);
		void GetAllCustData(IntPtr pCustData);
	}
}
#endif