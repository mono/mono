using System;
using System.Reflection;
using System.Runtime.InteropServices;

[Test2]
public partial class Test
{ }


[AttributeUsage(AttributeTargets.Struct)]
public partial class TestAttribute: Attribute
{
}

[AttributeUsage(AttributeTargets.All)]
public partial class Test2Attribute: Attribute
{
}

[TestAttribute]
public struct Test_2 {
}

class X
{
	public static int Main ()
	{
		if (Attribute.GetCustomAttributes (typeof (Test)).Length != 1)
			return 1;

		if (Attribute.GetCustomAttributes (typeof (Test_2)).Length != 1)
			return 1;
	
		Console.WriteLine ("OK");
		return 0;
	}
}
