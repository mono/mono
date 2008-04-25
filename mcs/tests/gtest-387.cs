class C
{
	public static int Main ()
	{
		sbyte? s = null;
		long? tt = +s;
		if (tt != null)
			return 1;
			
		long? l = null;
		l = +l;
		if (l != null)
			return 2;
			
		return 0;
	}
}
