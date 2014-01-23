//
// This test exercises the various ways in which mutator operators can be
// used in C# and the various different scenarios that the compiler would
// have to deal with
//
// variables, linear arrays, multi-dimensional arrays, jagged arrays,
// properties, indexers and overloaded ++ and --
//

class X {

	public int v, p;
	public int idx;
	
	public int this [int n] {
		get {
			idx = n;
			return v;
		}
		set {
			idx = n;
			v = value;
		}
	}

	public int P {
		get {
			return p;
		}

		set {
			p = value;
		}
	}
	
}

class Z {
	int v;

	public Z P {
		get {
			return null;
		}

		set {
		}
	}
	
	static public Z operator ++ (Z v)
	{
		v.v++;
		return v;
	}
}

class Y {

	static int p_pre_increment (X x)
	{
		return ++x.P;
	}

	static int p_post_increment (X x)
	{
		return x.P++;
	}

	static int i_pre_increment (X x)
	{
		return ++x [100];
	}

	static int  i_post_increment (X x)
	{
		return x [14]++;
	}

	static Z overload_increment (Z z)
	{
		  return z++;
	}
	
	static Z overload_pre_increment (Z z)
	{
		  return ++z;
	}
	
	static Z ugly (Z z)
	{
		  return z.P++;
	}

	//
	// Tests the ++ and -- operators on integers
	//
	static int simple (int i)
	{
		if (++i != 11)
			return 1;
		if (--i != 10)
			return 2;
		if (i++ != 10)
			return 3;
		if (i-- != 11)
			return 4;
		return 0;
	}

	static int arrays ()
	{
		int [] a = new int [10];
		int i, j;
		
		for (i = 0; i < 10; i++)
			a [i]++;

		for (i = 0; i < 10; i++)
			if (a [i] != 1)
				return 100;

		int [,] b = new int [10,10];
		for (i = 0; i < 10; i++){
			for (j = 0; j < 10; j++){
				b [i,j] = i * 10 + j;
				if (i < 5)
					b [i,j]++;
				else
					++b [i,j];
			}
		}

		for (i = 0; i < 10; i++){
			for (j = 0; j < 10; j++){
				if (b [i,j] != i * 10 + (j + 1))
					return 101;
			}
		}
		
		return 0;
	}
	
	public static int Main ()
	{
		X x = new X ();
		int c;
		
		if ((c = simple (10)) != 0)
			return c;
		
		if (i_pre_increment (x) != 1)
			return 5;
		
		if (x.idx != 100)
			return 6;
		
		if (i_post_increment (x) != 1)
			return 7;

		if (x.idx != 14)
			return 8;
		
		if (p_pre_increment (x) != 1)
			return 9;

		if (x.p != 1)
			return 10;
		
		if (p_post_increment (x) != 1)
			return 10;

		if (x.p != 2)
			return 11;

		Z z = new Z();

		overload_increment (z);

		arrays ();
		
		return 0;
	}
	
}
