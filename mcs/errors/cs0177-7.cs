// CS0177: The out parameter `a' must be assigned to before control leaves the current method
// Line: 6

class Foo {
	static void test39 (out int a)
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

	static void Main () { int x; test39 (out x); }
}

