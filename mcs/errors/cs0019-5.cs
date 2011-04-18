// CS0019: Operator `==' cannot be applied to operands of type `X' and `Y'
// Line : 13

class X {
}

class Y {
}

class T {
	static void Main ()
	{
		X x = new X ();
		Y y = new Y ();

		if (x == y){
		}
	}
}
