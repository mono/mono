// CS8200: Out variable and pattern variable declarations are not allowed within constructor initializers, field initializers, or property initializers
// Line: 11

public class C
{
	public C (bool value)
	{		
	}

	public C ()
		: this (Foo (out int arg))
	{	
	}

	static bool Foo (out int arg)
	{
		arg = 2;
		return false;
	}
}