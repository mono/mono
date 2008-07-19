using System;
using System.Collections.Generic;

public static class RunTests
{
	public static int Main ()
	{
		Test1.X.Run ();
		return 0;
	}
}

namespace Test1
{
	delegate int Foo ();

	public class X
	{
		public static void Test1<R> (R r, int a)
		{
			for (int b = a; b > 0; b--) {
				R s = r;
				Console.WriteLine (s);
				Foo foo = delegate {
					Console.WriteLine (b);
					Console.WriteLine (s);
					Console.WriteLine (a);
					Console.WriteLine (r);
					return 3;
				};
				a -= foo ();
			}
		}

		public static void Run ()
		{
			Test1 (500L, 2);
		}
	}
}

