

using System;
using System.Collections;

class Data
{
	public int Value;
}

public class Test
{
	static Data Prop {
		set {
		}
	}
	
	public object Foo ()
	{
		return new Data () { Value = 3 };
	}
	
	public static void Main ()
	{
		Prop = new Data () { Value = 3 };
		Data data = new Data () { Value = 6 };
		Data a, b;
		a = b = new Data () { Value = 3 };
	}
}
