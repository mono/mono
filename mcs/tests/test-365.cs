// Compiler options: -unsafe
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct _Port {
	[MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] char[] port;
	int a;
}

[StructLayout(LayoutKind.Sequential)]
internal unsafe struct _Camera
{
	_Port           *port;
}

class d {
	static void Main ()
	{
	}
}
