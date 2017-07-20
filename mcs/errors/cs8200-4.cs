// CS8200: Out variable and pattern variable declarations are not allowed within constructor initializers, field initializers, or property initializers
// Line: 8

using System;

public class C
{
	event Action H = Foo (out var res);

	static Action Foo (out int arg)
	{
		arg = 2;
		return null;
	}
}
