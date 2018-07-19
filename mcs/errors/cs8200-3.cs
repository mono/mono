// CS8200: Out variable and pattern variable declarations are not allowed within constructor initializers, field initializers, or property initializers
// Line: 11

public class C
{
	bool Prop { get; } = Foo (out int arg);

	static bool Foo (out int arg)
	{
		arg = 2;
		return false;
	}
}