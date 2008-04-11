// CS0428: Cannot convert method group `a' to non-delegate type `int'. Consider using parentheses to invoke the method
// Line: 12

class X {
	int a (int a)
	{
		return 0;
	}

	void b ()
	{
		int x = a;
	}
}
