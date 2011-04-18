// CS0038: Cannot access a nonstatic member of outer type `B' via nested type `B.C'
// Line: 14

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
