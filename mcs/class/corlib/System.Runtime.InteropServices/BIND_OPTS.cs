//
// System.Runtime.InteropServices.BIND_OPTS.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public struct BIND_OPTS
	{
		public int cbStruct;
		public int dwTickCountDeadline;
		public int grfFlags;
		public int grfMode;
	}
}
