// CS0728: Possibly incorrect assignment to `token' which is the argument to a using or lock statement
// Line: 11
// Compiler options: -warnaserror

public class Foo
{
	public static void Main ()
	{
		object token = new object ();
		lock (token)
		{
			Foo2 (ref token);
		}
	}
	
	static void Foo2 (ref object o)
	{
	}
}
