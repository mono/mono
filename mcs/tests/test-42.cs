//
// This has a bunch of different ways of using the mutator
// operators in C#.  Not all of them are yet supported by the
// compiler.  This is here just for references purposes until
// I am finised and make this into a real test.
//
class X {

	int v, p;
	
	public int this [int n] {
		get {
			return v;
		}
		set {
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
		return x [100]++;
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
	
	static void Main ()
	{
		X x = new X ();
		
		i_pre_increment (x);
		i_post_increment (x);
		p_pre_increment (x);
		p_post_increment (x);

		Z z = new Z();

		overload_increment (z);
	}
	
}
