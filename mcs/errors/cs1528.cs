// cs1528.cs: cant specify constructor arguments in declaration
// Line:
class X {
	X (int a)
	{
	}
}

class Y {
	static void Main ()
	{
		X x (4);
	}
}
