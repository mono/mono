// CS0308: The non-generic type `Test.NonGeneric' cannot be used with the type arguments
// Line: 8

public class Test
{
	public static void Main (string[] args)
	{
		NonGeneric dummy = new NonGeneric<string> ();
	}

	internal class NonGeneric
	{
	}
}
