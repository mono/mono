//
// System.Runtime.InteropServices.TYPELIBATTR.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct TYPELIBATTR
	{
		public Guid guid;
		public int lcid;
		public SYSKIND syskind;
		public LIBFLAGS wLibFlags;
		public short wMajorVerNum;
		public short wMinorVerNum;
	}
}
