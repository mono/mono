// CS0458: The result of the expression is always `null' of type `E?'
// Line: 15
// Compiler options: -warnaserror -warn:2

enum E
{
	V
}

public class C
{
	public static void Main ()
	{
		E e = E.V;
		object o = e + null;
	}
}
