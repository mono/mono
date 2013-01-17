class Test {
	static int count;

	static public bool operator == (Test x, Test y)
	{
		++count;
		return false;
	}

	static public bool operator != (Test x, Test y)	{ return true; }
	
	public override bool Equals (object o) { return false; }

	public override int GetHashCode () { return 0; }

	public static void Main ()
	{
		Test y = new Test ();
		if (y == null)
			throw new System.Exception ();
		if (count != 1)
			throw new System.Exception ("Operator == was not called");
	}
}
