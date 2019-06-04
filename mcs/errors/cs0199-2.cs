// CS0199: A static readonly field `X.f' cannot be passed ref or out (except in a static constructor)
// Line: 10

class X
{
	static readonly int f = 0;

	public static void Main ()
	{
		ref int j = ref f;
	}
}