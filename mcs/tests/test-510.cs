class Foo {
	static void test39 (ref int a)
	{
		int x_0 = 0;
		int ll_1 = 0;
        
		switch (0) {
		default:
			switch (x_0) {
			default:
				if (ll_1 == 0)
					break;
				else
					goto k_1;
			}
			a = 5;
			break;
		k_1:
			break;
		}
	}

	public static void Main ()
	{
		int a = 0;
		test39 (ref a);
		if (a != 5)
			throw new System.Exception ("reachable code got marked as unreachable");
	}
}
