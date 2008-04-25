using System;

enum MyEnum : byte
{
	A = 1,
	B = 2,
	Z = 255
}

class C
{
	public static int Main ()
	{
		MyEnum? e = MyEnum.A;
		byte? b = 255;
		MyEnum? res = e + b;
		if (res != 0)
			return 1;

		e = null;
		b = 255;
		res = e + b;
		if (res != null)
			return 2;
			
		MyEnum e2 = MyEnum.A;
		byte b2 = 1;
		MyEnum res2 = e2 + b2;
		if (res2 != MyEnum.B)
			return 3;
			
		Console.WriteLine ("OK");
		return 0;
	}
}

