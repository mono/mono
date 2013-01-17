// Compiler options: -warnaserror -warn:2

// Same as test-515, but we're checking that there's no "unreachable code" warning either

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
