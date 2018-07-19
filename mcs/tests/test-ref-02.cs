using System;

class X
{
	int field;

	static void Main ()
	{
		var x = new X ();
		x.Run ();
	}

	void Run ()
	{
		Test (ref this[0]);
		Test (ref Prop);
	}

	static int Test (ref int y)
	{
		return y;
	}

	ref int this [int y] {
		get {
			return ref field;
		}
	}

	ref int Prop {
		get {
			return ref field;
		}
	}
}