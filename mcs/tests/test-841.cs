struct S
{
	public R a, b;
}

struct R
{
	public double v;
	
	public static implicit operator R (int v)
	{
		return new R ();
	}
	
	public static implicit operator double (R r)
	{
		return r.v;
	}
}

class C
{
	public static int Main ()
	{
		S r1, r2;
		r1.a = 1;
		r1.b = 2;
		
		r2.a = 1;
		r2.b = 2;
		
		bool b = r1.a == r2.a && r1.b == r2.b;
		if (!b)
			return 1;
		return 0;
	}
}