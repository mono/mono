// Using constructed types in a namespace alias.

namespace N1
{
	class A<T>
	{
		public class B { }

		public class C<U> { }
	}

	class C { }
}

namespace N2
{
	using Y = N1.A<int>;

	class X
	{
		public static void Main ()
		{
			Y y = new Y ();
			Y.B b = new Y.B ();
			Y.C<long> c = new Y.C<long> ();
		}
	}
}
