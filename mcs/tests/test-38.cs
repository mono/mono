class X {
	int v1, v2;
	int y;
	
	int this [int a] {
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

	int Foo () {
		return 8;
	}

	int Y {
		get {
			return y;
		}

		set {
			y = value;
		}
	}

	static int Main ()
	{
		X x = new X ();
		int b;

		x [0] = x [1] = 1;
		x [0] = 1;
		if (x.v1 != 1)
			return 1;

		if (x [0] != 1)
			return 2;

		double d;
		long l;

		d = l = b = x [0] = x [1] = x.Y = x [2] = x [3] = x [4] = x.Foo ();

		if (x.Y != 8)
			return 3;

		if (l != 8)
			return 4;

		return 0;
		
	}
}
