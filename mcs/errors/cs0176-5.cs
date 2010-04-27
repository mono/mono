// CS0176: Static member `MyEnum.Foo' cannot be accessed with an instance reference, qualify it with a type name instead
// Line: 14

public enum MyEnum
{
	Foo = 1
}

public class Test
{
	static void Main ()
	{
		MyEnum theEnum = MyEnum.Foo;
		if (theEnum == theEnum.Foo)
		{
		}
	}
}
