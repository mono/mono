class A
{
	public int[] BB;
}

class X
{
	public static int Main ()
	{
		A a = null;
		var m = a?.BB?[3];
		return 0;
	}
}