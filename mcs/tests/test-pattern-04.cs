// Compiler options: -langversion:experimental

using System;

class RecursivePattern
{
	static int Main ()
	{
		object o = null;
		bool b = o is C1 (8);
		if (b)
			return 1;

		o = new C1 ();
		b = o is C1 (-4);
		if (b)
			return 2;

		b = o is C1 (8);
		if (!b)
			return 3;

		b = o is C1 (C1 (9), C1 (8));
		if (b)
			return 4;

		b = o is C1 (C1 (*), C1 (8));
		if (!b)
			return 41;

		b = o is C1 (0);
		if (b)
			return 5;

		ValueType vt = new S ();
		b = vt is S (null, 0);
		if (b)
			return 6;

		b = vt is S (8, 0);
		if (b)
			return 7;

		b = vt is S (8, 2);
		if (!b)
			return 8;

		Console.WriteLine ("ok");
		return 0;
	}
}

class C1
{
	public static bool operator is (C1 c, out int x)
	{
		x = 8;
		return true;
	}

	public static bool operator is (C1 c, out C1 c2, out C1 c3)
	{
		c2 = null;
		c3 = null;
		return true;
	}
}

struct S
{
	public static bool operator is (S s, out int? x, out decimal d)
	{
		x = 8;
		d = 2;
		return true;
	}
}