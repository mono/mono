// CS0029: Cannot implicitly convert type `int' to `A'
// Line: 14

class A
{
	public static implicit operator int (A x)
	{
		return 1;
	}
	
	public static void Main ()
	{
		var a = new A ();
		a++;
	}
}
