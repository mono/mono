struct R
{
	public static bool operator < (R left, R right)
	{
		return true;
	}

	public static bool operator > (R left, R right)
	{
		return false;
	}
	
	public static implicit operator float(R r)
	{
		return 5;
	}
	
	public static implicit operator R(float f)
	{
		return new R ();
	}
}

class C
{
	public static int Main ()
	{
		var r = new R ();
		float f = 999f;
		
		bool b = f < r;
		if (!b)
			return 1;
		
		return 0;
	}
}