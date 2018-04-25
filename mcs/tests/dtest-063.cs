using System;

class InvalidILWhenInterpolatingDynamicObjectTest
{
	static int Main ()
	{
		dynamic d = 1;
		var str = $"{d + 3}";
		if (str != "4")
			return 1;

		Console.WriteLine (str);
		return 0;
	}
}