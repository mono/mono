using System;
using System.Linq.Expressions;

class A<TA>
{
	public class B<TB>
	{
		public static void foo ()
		{
			Expression<Action> func = () => foo ();
		}

		class C<TC>
		{
			static void bar ()
			{
				B<TC>.foo ();
			}
		}
	}
}

class Program
{
	public static int Main ()
	{
		A<int>.B<ulong>.foo ();
		return 0;
	}
}
