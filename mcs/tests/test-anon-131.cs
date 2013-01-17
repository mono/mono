using System;
public static class A
{
	public static void Fail<X> ()
	{
		EventHandler t = delegate {
			t = delegate { X foo; };
		};
	}

	public static void Main ()
	{
	}
} 
