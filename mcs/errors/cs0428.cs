// cs0428.cs: Can not convert method group to type X, since type is not a delegate
// Line: 9
class X {
	static void Method ()
	{
	}

	static void Main ()
	{
		object o = Method;
	}
}
