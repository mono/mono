// cs0428.cs: Cannot convert method group 'a' to non-delegate type 'int'. Did you intend to invoke the method?
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
