using System;

static class X
{
	static (int, string) Test1 ()
	{
		return ValueTuple.Create (1, "2");
	}

	static void Test2 ((int Item1, int Item2) arg)
	{
	}

	static void Test3 ((int a, string b) arg)
	{
	}

	static (int a, string b) Test4 ()
	{
		return ValueTuple.Create (1, "x");
	}

	static int Main ()
	{
		var res = Test1 ();
		if (res.Item1 != 1) {
			return 1;
		}

		if (res.Item2 != "2") {
			return 2;
		}

		ValueTuple<int, string> res2 = res;

		Test3 (ValueTuple.Create (1, "2"));

		var res3 = Test4 ();
		if (res3.Item1 != 1)
			return 3;

		if (res3.a != 1)
			return 4;

		if (res3.Item2 != "x")
			return 5;

		if (res3.b != "x")
			return 6;

		return 0;
	}
}