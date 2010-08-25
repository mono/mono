class C
{
	public static int Main ()
	{
		int? x = null;
		dynamic y = 50;
		int v =  x.GetValueOrDefault(y);
		if (v != 50)
			return 1;
		
		return 0;
	}
}
