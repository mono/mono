// CS0119: Expression denotes a `type', where a `variable', `value' or `method group' was expected
// Line: 10

class A
{
	delegate string Test (string t);

	public static void Main ()
	{
		Test ("t");
	}
}
