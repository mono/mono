using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Explicit)]
struct A {
	[FieldOffset (0)]
	public int a;
	[FieldOffset (4)]
	public int b;
}

class X {
	static void Main ()
	{
	}
}
	
