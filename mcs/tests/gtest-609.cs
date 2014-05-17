class X<T, TBase> where T : TBase
{
	static TBase tb;

	public static void X2<T2> (T2 arg) where T2 : T
	{
		tb = arg;
		tb = (T2)tb;
	}
}

class Z
{
	public static void Main ()
	{
		X<string, object>.X2 ("");
	}
}