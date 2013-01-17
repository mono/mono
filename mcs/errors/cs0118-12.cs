// CS0118: `A.Test' is a `type' but a `variable' was expected
// Line: 10

class A
{
	delegate string Test (string t);

	public static void Main ()
	{
		Test ("t");
	}
}
