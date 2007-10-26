public struct C
{
	struct S
	{
		public int i;
		public S (int i)
		{
			this.i = i;
		}
	}

	static S[] s;
	
	int Test ()
	{
		int i = 0;
		s = new S [1];
		if (s.Length > 0)
			s [i++] = new S (10);
		
		if (s [0].i != 10)
			return 1;
		
		return 0;
	}
	
	public static int Main ()
	{
		return new C().Test ();
	}
}