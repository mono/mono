class X {
	int v1, v2;
	
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

	static int Main ()
	{
		X x = new X ();
		int b;

		x [1] = x [0] = 1;
		if (x.v1 != 1)
			return 1;

		if (x [0] != 1)
			return 2;

		return 0;
		
	}
}
