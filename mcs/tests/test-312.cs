using System;

struct PointF {
	public float fa, fb;
	
	public PointF (float a, float b)
	{
		fa = a;
		fb = b;
		Console.WriteLine ("PointF created {0} and {1}", fa, fb);
	}
}

struct Point {
	int ia, ib;
	
	public static implicit operator PointF (Point pt)
	{
		return new PointF (pt.ia, pt.ib);
	}

	public Point (int a, int b)
	{
		Console.WriteLine ("Initialized with {0} and {1}", a, b);
		ia = a;
		ib = b;
	}
}

class X {
	static bool ok = false;
	PointF field;
	
	static bool Method (PointF f)
	{
		Console.WriteLine ("Method with PointF arg: {0} {1}", f.fa, f.fb);
		if (f.fa != 100 || f.fb != 200)
			return false;
		return true;
	}
	
	static bool Call_constructor_and_implicit ()
	{
		ok = false;
		return Method (new Point (100, 200));
	}


	static bool Init_with_implicit_conv ()
	{
		PointF p = new Point (1, 100);
		if (p.fa == 1 && p.fb == 100)
			return true;
		return false;
	}

	static bool Init_ValueType ()
	{
		Point p = new Point (100, 200);
		return Method (p);
	}
	
	static bool InstanceAssignTest ()
	{
		X x = new X ();
		x.field = new Point (100, 200);
		if (x.field.fa != 100 || x.field.fb != 200)
			return false;
		return true;
	}
	
	static int T ()
	{
		
		if (!Init_with_implicit_conv ())
			return 100;
		if (!Call_constructor_and_implicit ())
			return 101;
		if (!Init_ValueType ())
			return 102;
		if (!InstanceAssignTest ())
			return 103;
		return 0;
	}

	public static int Main ()
	{
		int t = T ();
		if (t != 0)
			Console.WriteLine ("Failed on test: " + t);
		Console.WriteLine ("Succeed");
		return t;
	}
	
}
