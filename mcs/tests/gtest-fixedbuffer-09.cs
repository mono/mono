// Compiler options: -unsafe

using System;
using System.Runtime.CompilerServices;

unsafe struct Foo
{
	public fixed long FieldName[32];
}

class Test
{
	public static int Main ()
	{
		var t = typeof (Foo);
		var f = t.GetField ("FieldName");
		var fbas = f.GetCustomAttributes (typeof (FixedBufferAttribute), true)[0] as FixedBufferAttribute;
		if (fbas.Length != 32)
			return 1;

		return 0;
	}
}
