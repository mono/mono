public class A
{
	private class C
	{
		protected class D : C
		{
		}
	}

	public static void Main ()
	{
	}
}

namespace N
{
	public class B
	{
		protected class C : A
		{
			public class E
			{
			}
		}

		protected internal class A : B
		{
			protected class D : C.E
			{
			}
		}
	}
}

namespace N2
{
	public class X<T>
	{
		private class A<T>
		{
			private class B<T>
			{
				public class C<T>
				{
				}
			
				internal C<T> foo;
			}
		}
	}
}
