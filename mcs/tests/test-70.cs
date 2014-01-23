//
// Tests the right settings for overrides
//

class X {

	public virtual int A {
		get {
			return 1;
		}
	}

	public virtual int B ()
	{
		return 1;
	}
}

class Y : X {
	public override int A {
		get {
			return base.A + 2;
		}
	}

	public override int B ()
	{
		return base.B () + 1;
	}
}

class Z {
	public static int Main ()
	{
		Y y = new Y ();
		X x = new X ();
				       
		if (y.B () != 2)
			return 1;
		if (y.A != 3)
			return 2;
		if (x.A != 1)
			return 3;
		if (x.B () != 1)
			return 4;
		return 0;
	}
}
