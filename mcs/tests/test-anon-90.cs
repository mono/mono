class C
{
	public static int Main ()
	{
		new C ().Foo ();
		return 0;
	}
	
	delegate void D ();
	
	void Foo ()
	{
		int x = 0;
		D d1 = delegate () {
			int y = 1;
			if (y == 1) {
				int z = 2;
				D d2 = delegate () {
					int a = x + y + z;
				};
			}
		};
	}
}