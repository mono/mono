// CS0472: The result of comparing value type `long' with null is always `false'
// Line: 9
// Compiler options: -warnaserror -warn:2

class C
{
	public static void Main ()
	{
		System.Console.WriteLine(5 == (long?)null);
	}
}
