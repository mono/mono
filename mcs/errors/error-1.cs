// This test must produce a compilation error in each method.
using System;

public class X
{
	public static int Main ()
	{
		// This is a compilation-only test.
		return 0;
	}

	// Must assign out parameter.
	// CS0177
	public static void test1 (out float f)
	{
	}

	// Must assign it before returning.
	public static void test2 (int a, out float f)
	{
		// CS0177
		if (a == 5)
			return;

		f = 8.53F;
	}

	public static void test3 (out float f)
	{
		try {
			f = 8.53F;
		} catch {
			return;
		}
		// CS0177
	}

	public static int test4 ()
	{
		int a;

		try {
			a = 3;
		} catch {
			Console.WriteLine ("EXCEPTION");
		}

		// CS0165
		return a;
	}

	public static int test5 ()
	{
		int a;

		try {
			Console.WriteLine ("TRY");
			a = 8;
		} catch {
			a = 9;
		} finally {
			// CS0165
			Console.WriteLine (a);
		}

		return a;
	}

	public static void test6 (int a, out float f)
	{
		do {
			// CS0177
			if (a == 8) {
				Console.WriteLine ("Hello");
				return;
			}
		} while (false);

		f = 1.3F;
		return;
	}

	// CS0177
	public static void test7 (out float f)
	{
		goto World;
		// warning CS0162
		f = 8.0F;

	World:
		;
	}
}
