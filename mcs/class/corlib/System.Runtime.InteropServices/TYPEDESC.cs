//
// System.Runtime.InteropServices.TYPEDESC.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct TYPEDESC
	{
		public IntPtr lpValue;
		public short vt;
	}
}
