// CS1978: An expression of type `int*' cannot be used as an argument of dynamic operation
// Line: 9
// Compiler options: -unsafe

unsafe class C
{
	public static void Main ()
	{
		dynamic d = null;
		d ((int*)0);
	}
}
