class T {
	static int fib (int n) {
		int f0 = 0, f1 = 1, f2 = 0, i;

		if (n <= 1) goto L3;
		i = 2;
	L1:
		if (i <= n) goto L2;
		return f2;
	L2:
		f2 = f0 + f1;
		f0 = f1;
		f1 = f2;
		i++;
		goto L1;
	L3: 
		return n;
	}

	static int xx (int n) {
		if (n <= 1) goto L3;
	L1:
		if (1 <= n) goto L2;
		return n;
	L2:
		goto L1;
	L3:               
		return n;
	}

	// This is from System.Text.RegularExpressions.Syntax.Parser::ParseGroup.
	void foo (int a)
	{
		bool b = false;

		while (true) {
			switch (a) {
			case 3:
				break;
			}

			if (b)
				goto EndOfGroup;
		}

	EndOfGroup:
		;
	}

	// Another case of goto that we did not handle properly
	static void XXXA () {
		goto each_logic_expr;

		int j;
		bool x = true;
		try {
		}
		catch {}
		int dd;
		each_logic_expr:
		;
	}

	public static void Main() {}
}
