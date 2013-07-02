class X {
	public static void Main ()
	{
		int i = 0;
		goto a;
	b:
		if (++ i > 1)
			throw new System.Exception ("infloop!!!");
		return;
	a:
		goto b;
	}
}
