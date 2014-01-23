// CS0458: The result of the expression is always `null' of type `int?'
// Line: 14
// Compiler options: -warnaserror -warn:2

public enum E
{
}

class C
{
	public static void Main ()
	{
		E? e = null;
		var res = e - null;
	}
}