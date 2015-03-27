using System;

namespace TestDelegateFinallyOut
{
	class Test
	{
		static void CallDelegate (Action test)
		{
			throw new Exception ("test");
		}

		private static bool TestMethod (out int test)
		{
			try {
				CallDelegate (delegate {
					return;
				});
			} catch (Exception) {
				Console.WriteLine ("caught exception");
			} finally {
			}
			test = 1;
			return false;
		}

		static int Main ()
		{
			int t;
			TestMethod (out t);
			if (t != 1)
				return 1;

			return 0;
		}
	}
}