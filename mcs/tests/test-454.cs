// Test various kinds of arrays as custom attribute parameters
using System;

enum EnumType {
	X,
	Y
};

class FooAttribute : Attribute
{
	public string [] StringValues;
	public object [] ObjectValues;
	public EnumType [] EnumValues;
	public Type [] Types;

	public FooAttribute ()
	{
	}
}

[Foo (StringValues = new string [] {"foo", "bar", "baz"},
	ObjectValues = new object [] {1, 'A', "B"},
	EnumValues = new EnumType [] { EnumType.X, EnumType.Y },
	Types = new Type [] {typeof (int), typeof (Type)}
	)]
class Test
{
	public static int Main () 
	{
		FooAttribute foo = (FooAttribute) typeof (Test)
			.GetCustomAttributes (false) [0];
		if (foo.StringValues [0] != "foo"
			|| foo.StringValues [1] != "bar"
			|| foo.StringValues [2] != "baz"
			|| 1 != (int) foo.ObjectValues [0]
			|| 'A' != (char) foo.ObjectValues [1]
			|| "B" != (string) foo.ObjectValues [2]
			|| EnumType.X != foo.EnumValues [0]
			|| EnumType.Y != foo.EnumValues [1]
			|| foo.Types [0] != typeof (int)
			|| foo.Types [1] != typeof (Type)
			)
			return 1;
		
		return 0;
	}
}