//
// System.Runtime.InteropServices.PARAMDESC.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct PARAMDESC
	{
		public IntPtr lpVarValue;
		public PARAMFLAG wParamFlags;
	}
}
