// CS0029: Cannot implicitly convert type `S' to `object'
// Line: 13
// Compiler options: -langversion:latest

public ref struct S
{
}

class Test
{
	public static void Main ()
	{
		object o = new S ();
	}
}