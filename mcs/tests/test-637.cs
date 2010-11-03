using System;

struct S {}

class A : Attribute
{
	public A ()
	{
	}
	
	public A (object value)
	{
		Value = (Type) value;
	}
	
	public Type Value { get; set; }
}

[A (Value = typeof (S*))]
class TestProp
{
}

[A (typeof (ushort**))]
public class Test
{
	public static int Main ()
	{
		A a = (A)typeof (Test).GetCustomAttributes (false)[0];
		if (a.Value != typeof (ushort**))
			return 1;

		a = (A)typeof (TestProp).GetCustomAttributes (false)[0];
		if (a.Value != typeof (S*))
			return 2;
		
		return 0;
	}
}
