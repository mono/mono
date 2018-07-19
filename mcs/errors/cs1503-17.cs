// CS1503: Argument `#1' cannot convert `ref long' expression to type `ref int'
// Line: 18

using System;

class X
{
	long field;

	static void Main ()
	{
		var x = new X ();
		x.Run ();
	}

	void Run ()
	{
		Test (ref Prop);
	}

	static int Test (ref int y)
	{
		return y;
	}

	ref long Prop {
		get {
			return ref field;
		}
	}
}
