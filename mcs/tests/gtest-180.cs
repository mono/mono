using System;
using System.Runtime.InteropServices;

[module: DefaultCharSet (CharSet.Unicode)]

struct foo1
{
}

enum E
{
}

[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Auto)]
struct foo2
{
}

[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
delegate void D ();

class C
{
	public class CC
	{
	}
}

class Program
{

	[DllImport ("bah")]
	public static extern void test ();

	static int Main ()
	{
		DllImportAttribute dia = Attribute.GetCustomAttribute (typeof (Program).GetMethod ("test"), typeof (DllImportAttribute)) as DllImportAttribute;
		if (dia == null)
			return 1;

		if (dia.CharSet != CharSet.Unicode)
			return 2;

		if (!typeof (C).IsUnicodeClass)
			return 3;

		if (!typeof (C.CC).IsUnicodeClass)
			return 4;

		if (!typeof (D).IsUnicodeClass)
			return 5;

		var ufp = typeof (D).GetCustomAttributes (false)[0] as UnmanagedFunctionPointerAttribute;
		if (ufp.CharSet != CharSet.Unicode)
			return 51;

		if (!typeof (E).IsUnicodeClass)
			return 6;

		if (!typeof (foo1).IsUnicodeClass)
			return 7;

		if (!typeof (foo2).IsAutoClass)
			return 8;

		return 0;
	}
}
