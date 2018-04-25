// CS0611: Array elements cannot be of type `S'
// Line: 13
// Compiler options: -langversion:latest

public ref struct S
{
}

class Test
{
	public static void Main ()
	{
		var x = new S[0];
	}
}