// CS0208: Cannot take the address of, get the size of, or declare a pointer to a managed type `_Port'
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
