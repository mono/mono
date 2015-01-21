using System;

class X
{
	static int TestGeneral ()
	{
		int x = -1;
		try {
			throw new ApplicationException ();
		} catch when (x > 0) {
			return 1;
		} catch when (x < 0) {
			return 0;
		} catch {
			return 2;
		}
	}

	static int TestSpecific ()
	{
		try {
			throw new ApplicationException ();
		} catch (Exception e) when (Foo (delegate { Console.WriteLine (e); })) {
			Action a = delegate {
				Console.WriteLine (e);
			};
			return 1;
		} catch (Exception e) when (e is InvalidOperationException) {
			Console.WriteLine (e);

			int paramIndex = 0;
			while (paramIndex < 3) {
				paramIndex++;
			}
						
			return 1;
		} catch (ApplicationException) {
			return 0;
		}
	}

	static bool Foo (Action a)
	{
		a ();
		return false;
	}

	public static int Main ()
	{
		var r = TestGeneral ();
		if (r != 0)
			return r;

		r = TestSpecific ();
		if (r != 0)
			return 10 + r;

		Console.WriteLine ("ok");
		return 0;
	}
}