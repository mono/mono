class X {
	public int v1, v2;
	int y;
	
	public int this [int a] {
		get {
			if (a == 0)
				return v1;
			else
				return v2;
		}

		set {
			if (a == 0)
				v1 = value;
			else
				v2 = value;
		}
	}

	public int Foo () {
		return 8;
	}

	public int Bar {
		get {
			return y;
		}

		set {
			y = value;
		}
	}
}

class Y {
	public uint v1, v2;
	uint y;
	
	public uint this [uint a] {
		get {
			if (a == 0)
				return v1;
			else
				return v2;
		}

		set {
			if (a == 0)
				v1 = value;
			else
				v2 = value;
		}
	}

	public uint Foo () {
		return 8;
	}

	public uint Bar {
		get {
			return y;
		}

		set {
			y = value;
		}
	}
}

class Test {

	public static int Main ()
	{
		X x = new X ();
		Y y = new Y ();
		int b;

		x [0] = x [1] = 1;
		x [0] = 1;
		if (x.v1 != 1)
			return 1;

		if (x [0] != 1)
			return 2;

		double d;
		long l;

		d = l = b = x [0] = x [1] = x.Bar = x [2] = x [3] = x [4] = x.Foo ();

		if (x.Bar != 8)
			return 3;

		if (l != 8)
			return 4;

		uint e, f;
		e = 5;
		e = f = 8;

		if (e != 8)
			return 5;

		y [0] = y [1] = 9;
		y [0] = y.Bar = 12;

		if (y.Bar != 12)
			return 6;

		y.Bar = 15;
		if (y.Bar != 15)
			return 7;

		return 0;
		
	}
}
