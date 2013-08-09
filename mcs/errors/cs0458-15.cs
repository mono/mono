// CS0458: The result of the expression is always `null' of type `short?'
// Line: 24
// Compiler options: -warnaserror -warn:2

struct S
{
	public static short operator + (S s, S i)
	{
		return 2;
	}

	public static int? operator + (S? s, int? i)
	{
		return 2;
	}

}

class C
{
	public static void Main ()
	{
		S? s = new S ();
		var x = s + (S?)null;
	}
}