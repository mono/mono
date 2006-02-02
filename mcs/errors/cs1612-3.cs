// cs1612-3.cs: Cannot modify the return value of `bar.this[...]' because it is not a variable
// Line: 19

struct foo {
	public int x;
}

class bar {
	public foo this [int x] {
		get { return new foo (); }
		set { }
	}
}

class main {
	static void Main ()
	{
		bar b = new bar ();
		b [0].x = 5;
	}
}
