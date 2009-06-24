// CS1644: Feature `named argument' cannot be used because it is not part of the C# 3.0 language specification
// Line: 13
// Compiler options: -langversion:3

public class C
{
	static void Foo (int i)
	{
	}
	
	public static void Main ()
	{
		Foo (i : 3);
	}
}
