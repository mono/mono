using System;

class C
{
	int[,] a = new int [2, 3];

	static int Main ()
	{
		var res = new C { a = { [1, 1] = 11, [0, 2] = 2} };
		if (res.a [1, 1] != 11)
			return 1;

		if (res.a [1, 2] != 0)
			return 2;

		if (res.a [0, 2] != 2)
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}
}