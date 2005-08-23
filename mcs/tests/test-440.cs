public class A {
	public static implicit operator double (A a)
	{
		return 0.5;
	}

	// unlike CS0034 case, two or more implicit conversion on other 
	// than string is still valid.
	public static implicit operator int (A a)
	{
		return 0;
	}

	public static void Main ()
	{
		A a = new A ();
		object p = a + a;
	}
}
