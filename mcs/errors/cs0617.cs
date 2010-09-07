// CS0617: `MyNamedArg' is not a valid named attribute argument. Named attribute arguments must be fields which are not readonly, static, const or read-write properties which are public and not static
// Line : 19

using System;

[AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
public class SimpleAttribute : Attribute
{
	string name;

	public readonly string MyNamedArg;

	public SimpleAttribute (string name)
	{
		this.name = name;
	}
}

[Simple ("Dummy", MyNamedArg = "Dude!")]
public class Blah {

	public static void Main ()
	{
	}
}
