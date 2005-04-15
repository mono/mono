// cs0208: _Port is not an unmanaged type.
// Line: 16
// Compiler options: -unsafe

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct _Port {
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] char[] port;
}

unsafe class d {
	static void Main ()
	{
		_Port * port = null;
	}
}
