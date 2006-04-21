// cs1612-5.cs: Cannot modify the return value of `bar.this[...]' because it is not a variable
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
	static void baz (out int x) { x = 5; }
	static void Main ()
	{
		bar b = new bar ();
		baz (out b [0].x);
	}
}
