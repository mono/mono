public class A
{
	protected internal struct C { }
}

class B
{
	class D : A
	{
		public A.C Property { get; set; }
	}

	public static void Main ()
	{
	}
}