// CS8200: Out variable and pattern variable declarations are not allowed within constructor initializers, field initializers, or property initializers
// Line: 6

public class C
{
	bool res = Foo () is string s;

	static object Foo ()
	{
		return null;
	}
}