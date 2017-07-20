using System;

class X
{
	int v;

	public int Prop {
		get => 1;
		set => v = value;
	}

	public event Action A {
		add => v = 1;
		remove => v = 2;
	}

	public static void Main ()
	{		
	}
}