class A
{
	public static int Main ()
	{
		dynamic d = 'a';
		object o = null;
		
		char ch = o ?? d;
		if (ch != 'a')
			return 1;
		
		const A a = null;
		ch = a ?? d;
		if (ch != 'a')
			return 2;
		
		ch = d ?? 'b';
		if (ch != 'a')
			return 3;
		
		return 0;
	}
}