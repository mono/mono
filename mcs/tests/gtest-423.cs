using System;

namespace MonoTest
{
	public class A<TA>
	{
		class B<TB>
		{
			static void foo ()
			{
			}

			class C
			{
				static void bar ()
				{
					foo ();
					B<C>.foo ();
					A<C>.B<C>.foo ();
				}
			}
		}
	}

	class Program
	{
		static void Main ()
		{
		}
	}
}

