class X {
	delegate void B (int a, int b);
	static void A (int a, int b) {}

	delegate void D (out int a);
	static void C (out int a) { a = 5; }
	
	public static void Main()
	{
		(new B (A)) (1, 2);

		int x = 0;
		(new D (C)) (out x);
		if (x != 5)
			throw new System.Exception ("The value of x is " + x);
	}
}
