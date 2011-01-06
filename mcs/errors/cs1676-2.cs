// CS1676: Parameter `1' must be declared with the `out' keyword
// Line: 10

class C
{
	delegate int D (out int i);

	public static void Main ()
	{
		 D d = a => 1;
	}
}
