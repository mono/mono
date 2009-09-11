class C
{
	public static dynamic Create ()
	{
		return 1;
	}
	
	public static void Main ()
	{
		var d = Create ();
		d.Foo ();
	}
}