using System;

class TestAttribute : Attribute
{
	public TestAttribute (dynamic[] arga)
	{
	}
	
	public dynamic[] a;
}

[Test (null, a = null)]
class C
{
	public static void Main ()
	{
		var a = typeof (C).GetCustomAttributes (true)[0];
	}
}