// CS0464: The result of comparing type `int?' with null is always `false'
// Line: 10
// Compiler options: -warnaserror -warn:2

class C
{
	public static void Main ()
	{
		int? k = 1;
		var x = k > null;
	}
}