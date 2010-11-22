// Compiler options: -unsafe

using System;
using System.Runtime.InteropServices;

[StructLayout (LayoutKind.Auto, CharSet = CharSet.Auto)]
struct S
{
	public unsafe fixed byte o[6];
}

class A
{
	public static int Main ()
	{
		Type t = typeof (S);
		var sa = t.StructLayoutAttribute;
		if (sa.Value != LayoutKind.Auto)
			return 1;

		if (sa.CharSet != CharSet.Auto)
			return 2;

		if (sa.Pack != 8)
			return 3;

		if (sa.Size != 0)
			return 4;

		t = t.GetNestedTypes ()[0];
		sa = t.StructLayoutAttribute;
		if (sa.Value != LayoutKind.Sequential)
			return 11;

		if (sa.CharSet != CharSet.Auto)
			return 12;

		if (sa.Pack != 8)
			return 13;

		if (sa.Size != 6)
			return 14;

		return 0;
	}
}
