// CS0038: Cannot access a nonstatic member of outer type `A' via nested type `B.C'
// Line: 15

public class A {
	public int Foo { get { return 1; } }
}

public class B : A {
	public static void Main ()
	{
	}

	public class C {
		public void Baz ()
		{
			int x = Foo;
		}
	}
}
