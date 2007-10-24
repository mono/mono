// CS0633: An overloaded method `C.Foo(string)' cannot differ on use of parameter modifiers only
// Line: 11
// Compiler options: -langversion:linq

public static class C
{
	static  void Foo (this string eType)
	{
	}
	
	static  void Foo (string value)
	{
	}
}
