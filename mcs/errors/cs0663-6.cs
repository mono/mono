// CS0633: Overloaded method `C.Foo(string)' cannot differ on use of parameter modifiers only
// Line: 11


public static class C
{
	static  void Foo (this string eType)
	{
	}
	
	static  void Foo (string value)
	{
	}
}
