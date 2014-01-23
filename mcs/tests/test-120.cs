//
// This tests checks that the compiler catches the special attributes
// for in a struct for CharSet, and turns the right bit on the TypeAttribute
//
using System;
using System.Reflection;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit, Size=32,CharSet=CharSet.Unicode)]
struct MyUnicode
{
	[FieldOffset(0)] public float fh_float;
	[FieldOffset(0)] public int fh_int;
}

[StructLayout(LayoutKind.Explicit, Size=32,CharSet=CharSet.Ansi)]
struct MyAnsi
{
	[FieldOffset(0)] public float fh_float;
	[FieldOffset(0)] public int fh_int;
}
[StructLayout(LayoutKind.Explicit, Size=32,CharSet=CharSet.Auto)]
struct MyAuto
{
	[FieldOffset(0)] public float fh_float;
	[FieldOffset(0)] public int fh_int;
}

class test
{
	
	public static int Main ()
	{
		int errors = 0;
		Type t = typeof (MyUnicode);

		if ((t.Attributes & TypeAttributes.StringFormatMask) != TypeAttributes.UnicodeClass){
			Console.WriteLine ("Class MyUnicode does not have Unicode bit set");
			errors += 1;
		}

		t = typeof (MyAuto);
		if ((t.Attributes & TypeAttributes.StringFormatMask) != TypeAttributes.AutoClass){
			Console.WriteLine ("Class MyAuto does not have Auto bit set");
			errors += 2;
		}

		t = typeof (MyAnsi);

		if ((t.Attributes & TypeAttributes.StringFormatMask) != TypeAttributes.AnsiClass){
			Console.WriteLine ("Class MyUnicode does not have Ansi bit set");
			errors += 4;
		}

		return errors;
	}
}
