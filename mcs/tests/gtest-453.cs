using System;

namespace Test
{
	public enum Enum
	{
		One,
		Two
	}

	class CompilerTest
	{
		public static int Main ()
		{
			ThisWorksFine ();
			ThisDoesNotWork ();
			return 0;
		}

		protected static int DoSomething<T> (string s, T t, ref T t2)
		{
			Console.WriteLine ("s={0}", s);
			Console.WriteLine ("t={0}", t.ToString ());
			Console.WriteLine ("t2={0}", t2.ToString ());

			t2 = default (T);
			return 0;
		}

		public static void ThisDoesNotWork ()
		{
			Enum? e = Enum.One;
			DoSomething ("abc", Enum.Two, ref e);
		}

		public static void ThisWorksFine ()
		{
			Enum e = Enum.Two;
			DoSomething ("abc", Enum.Two, ref e);
			Console.WriteLine ("e={0}", e.ToString ());
		}
	}
}
