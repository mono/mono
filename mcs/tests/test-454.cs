// string array initializer is allowed as a parameter type.
using System;

class FooAttribute : Attribute
{
	public string [] StringValues;
	public object [] ObjectValues;
	public Type [] Types;

	public FooAttribute ()
	{
	}
}

[Foo (StringValues = new string [] {"foo", "bar", "baz"},
	ObjectValues = new object [] {1, 'A', "B"},
	Types = new Type [] {typeof (int), typeof (Type)}
	)]
class Test
{
	public static void Main () 
	{
		FooAttribute foo = (FooAttribute) typeof (Test)
			.GetCustomAttributes (false) [0];
		if (foo.StringValues [0] != "foo"
			|| foo.StringValues [1] != "bar"
			|| foo.StringValues [2] != "baz"
			|| 1 != (int) foo.ObjectValues [0]
			|| 'A' != (char) foo.ObjectValues [1]
			|| "B" != (string) foo.ObjectValues [2]
			|| foo.Types [0] != typeof (int)
			|| foo.Types [1] != typeof (Type)
			)
			throw new ApplicationException ();
	}
}
