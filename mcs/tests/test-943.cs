using System;
using System.Runtime.InteropServices;

public struct MyStruct
{
	public int X { get; set; }
}

[StructLayout (LayoutKind.Sequential, Pack = 1)]
public struct MyStruct2
{
    public IntPtr handle;
    public uint type_reference;
}

class X
{
	public static int Main ()
	{
		var s = typeof (MyStruct);

		if (s.StructLayoutAttribute.Size != 0)
			return 1;

		var s2 = typeof (MyStruct2);

		if (s2.StructLayoutAttribute.Size != 0)
			return 2;

		if (s2.StructLayoutAttribute.Pack != 1)
			return 3;

		return 0;
	}
}