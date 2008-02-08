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

