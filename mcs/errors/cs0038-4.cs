public class B {
	public static void Main ()
	{
	}

	public int Foo { get { return 1; } }

	public class C {
		public void Baz ()
		{
			int x = Foo;
		}
	}
}
