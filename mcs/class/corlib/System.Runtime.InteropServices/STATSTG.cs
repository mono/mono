//
// System.Runtime.InteropServices.STATSTG.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct STATSTG
	{
		public FILETIME atime;
		public long cbSize;
		public Guid clsid;
		public FILETIME ctime;
		public int grfLocksSupported;
		public int grfMode;
		public int grfStateBits;
		public FILETIME mtime;
		public string pwcsName;
		public int reserved;
		public int type;
	}
}
