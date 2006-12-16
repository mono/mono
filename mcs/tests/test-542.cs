class C
{
	static void Test (C arg)
	{
	}
	
	public static void Main ()
	{
		object value = null;
		C.Test(
#if true
		(C)
#else
		no error here
#endif
		value);
	}
}