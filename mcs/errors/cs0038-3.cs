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
