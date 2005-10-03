// cs0029.cs: Cannot implicitly convert type `X' to `bool'
// Line : 11

class X {
}

class T {
	static void Main ()
	{
		X x = new X ();
		if (x){
		}
	}
}
