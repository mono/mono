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

	static void Main() {}
}
