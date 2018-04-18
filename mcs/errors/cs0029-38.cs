// CS0029: Cannot implicitly convert type `int' to `(long, bool)'
// Line: 8

class C
{
	static void Test ()
	{
		System.ValueTuple<long, bool> arg = 1;
	}
}
