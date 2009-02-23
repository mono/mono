// CS0029: Cannot implicitly convert type `A.D' to `A'
// Line: 11

class A
{
	delegate void D ();

	public static void Main ()
	{
		const D d = null;
		A a = d;
	}
}
