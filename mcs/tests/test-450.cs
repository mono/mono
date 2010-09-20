using System;

enum E : byte
{
	V
}

class A
{
	int value;
	
	private A (int value)
	{
		this.value = value;
	}
	
	public static implicit operator byte (A a)
	{
		return 6;
	}

	public static implicit operator A (int a)
	{
		return new A (a);
	}
	
	public static int Main ()
	{
		var a = new A (0);
		a++;
		if (a.value != 7)
			return 1;
		
		var e = E.V;
		e++;
		
		if ((int) e != 1)
			return 2;
		
		return 0;
	}
}