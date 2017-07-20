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

		Out (out var o3);
		if (o3 != 3)
			return 4;

		Out2 (str: "b", v: out var o5);
		if (o5 != 9)
			return 7;

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
}