namespace N
{
	public partial class A<T1, T2>
	{
		public partial class D
		{
			public class C : D
			{
				public void Test ()
				{
					Foo ();
				}
			}
		}

		public partial class D2<U>
		{
			public class C2 : D2<int>
			{
				public void Test ()
				{
					Foo (2);
				}
			}
		}
	}

	public partial class A<T1, T2>
	{
		public partial class D : X
		{
		}

		public partial class D2<U> : X2<U>
		{
		}
	}

	public class X2<W>
	{
		public void Foo (W arg)
		{
		}
	}

	public class X
	{
		public void Foo ()
		{
		}

		public static void Main ()
		{
			new A<int, long>.D.C ().Test ();
			new A<int, long>.D2<string>.C2 ().Test ();
		}
	}
}
