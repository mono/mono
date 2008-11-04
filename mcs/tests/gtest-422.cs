class A
{
	public static bool operator > (A a, object b)
	{
		return false;
	}
	
	public static bool operator < (A a, object b)
	{
		return true;
	}
}

class C
{
	public static int Main ()
	{
		return new C ().Test () ? 1 : 0;
	}
	
	int? Id {
		get { return 1; }
	}
	
	bool Test ()
	{
		A a = new A ();
		bool b = a > Id && a < Id;
        return b;
	}
}
