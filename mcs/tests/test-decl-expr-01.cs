// Compiler options: -langversion:experimental
using System;

class DeclarationExpression
{
	public static int Main ()
	{
		Out (out int o);
		if (o != 3)
			return 1;

		if (Out (out int o1)) {
			if (o1 != 3)
				return 2;
		}

		Out (out int o2 = 2);
		if (o2 != 3)
			return 3;

		Out (out var o3);
		if (o3 != 3)
			return 4;

		Ref (ref int r = 2);
		if (r != 7)
			return 5;

		Ref (ref ((var r2 = 3)));
		if (r2 != 8)
			return 6;

//		Out2 (str: "b", v: out var o5);
//		if (o5 != 9)
//			return 7;

		Out3 (out var o6 = 9m);
		if (o6.GetType () != typeof (decimal))
			return 8;

		Console.WriteLine ("ok");
		return 0;
	}

	static bool Out (out int value)
	{
		value = 3;
		return true;
	}

	static bool Out2 (out int v, string str)
	{
		v = 9;
		return true;
	}

	static void Out3<T> (out T t)
	{
		t = default (T);
	}

	static void Ref (ref int arg)
	{
		arg += 5;
	}
}