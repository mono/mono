// CS0755: A partial method declaration and partial method implementation must be both an extension method or neither
// Line: 11


public static partial class C
{
	static partial void Foo (this string eType)
	{
	}
	
	static partial void Foo (string value);
}
