// Compiler options: -langversion:experimental

using System;

class RecursiveNamedPattern
{
	public static int Main ()
	{
		object o = new C ();
		bool b = o is C (name2: "", name1: -2);
		if (b)
			return 1;

		b = o is C (name2: "n2", name1: -2);
		if (!b)
			return 2;

		b = o is C ();
		if (b)
			return 3;

		return 0;
	}
}

class C
{
	public static bool operator is (C c, out long name1, out string name2)
	{
		name1 = -2;
		name2 = "n2";
		return true;
	}

	public static bool operator is (C c)
	{
		return false;
	}
}