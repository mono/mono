using System;
using System.Collections;

public class X
{
	public static int Main ()
	{
		// This is a compilation-only test.
		return 0;
	}

	// All code paths throw an exception, no need to set out parameters.
	public static void test1 (out float f)
	{
		throw new NotSupportedException ();
	}

	// The while loop breaks but does not return, so this is ok.
	public static void test2 (int a, out float f)
	{
		while (a > 0) {
			if (a == 5)
				continue;

			Console.WriteLine (a);
		}

		f = 8.53F;
	}

	// a has been assigned in all code paths which do not return.
	public static void test3 (long[] b, int c)
	{
		ICollection a;
		if (b == null)
			throw new ArgumentException ();
		else
			a = (ICollection) b;

		Console.WriteLine (a);
	}

	// Forward goto, it's ok to set f after the target label.
	public static int test4 (int b, out float f)
	{
		long a;

		Console.WriteLine ("Hello World");

		a = 5;

		goto World;

	World:
		Console.WriteLine (a);

		f = 8.53F;

		return 0;
	}

	// try { ... } catch { ... } finally { ... } block
	static public int test5 (out float f, long d)
	{
                int a;
		long b = 8;

		try {
			f = 8.53F;

			if (d == 500)
				return 9;

			a = 5;
		} catch (NotSupportedException e) {
			a = 9;
		} catch (Exception e) {
			return 9;
		} finally {
			f = 9.234F;
		}

		return a;
        }

	// Passing out parameter to method invocation
	static public int test6 (out float f)
	{
		return test5 (out f, 50);
	}

	// Loop-variable of foreach() and for() loop.
	static public long test7 (int[] a, int stop)
	{
		long b = 0;
		foreach (int i in a)
			b += i;

		for (int i = 1; i < stop; i++)
			b *= i;

		return b;
	}

	// Initializing locals in initialize or test of for block
	static public long test8 (int stop)
	{
		int i;
		long b;
		for (i = 1; (b = stop) > 3; i++) {
			stop--;
			b += i;
		}
		return b;
	}

	// Initializing locals in test of while block
	static public long test9 (int stop)
	{
		long b;
		while ((b = stop) > 3) {
			stop--;
			b += stop;
		}
		return b;
	}

	// Return in subblock
	public static void test10 (int a, out float f)
	{
		if (a == 5) {
			f = 8.53F;
			return;
		}

		f = 9.0F;
	}

	// Switch block
	public static long test11 (int a)
	{
		long b;

		switch (a) {
		case 5:
			b = 1;
			break;

		case 9:
			b = 3;
			break;

		default:
			return 9;
		}

		return b;
	}

	// Try block which rethrows exception.
	public static void test12 (out float f)
	{
		try {
			f = 9.0F;
		} catch {
			throw new NotSupportedException ();
		}
	}

	// Return in subblock.
	public static void test13 (int a, out float f)
	{
		do {
			if (a == 8) {
				f = 8.5F;
				return;
			}
		} while (false);

		f = 1.3F;
		return;
	}

	// Switch block with goto case / goto default.
	public static long test14 (int a, out float f)
	{
		long b;

		switch (a) {
		case 1:
			goto case 2;

		case 2:
			f = 9.53F;
			return 9;

		case 3:
			goto default;

		default:
			b = 10;
			break;
		}

		f = 10.0F;

		return b;
	}

	// Forward goto, it's ok to set f before the jump.
	public static int test15 (int b, out float f)
	{
		long a;

		Console.WriteLine ("Hello World");

		a = 5;
		f = 8.53F;

		goto World;

	World:
		Console.WriteLine (a);

		return 0;
	}

	// `continue' breaks unless we're a loop block.
	public static void test16 ()
	{
                int value;

                for (int i = 0; i < 5; ++i) {
                        if (i == 0) {
                                continue;
                        } else if (i == 1) {
                                value = 2;
                        } else {
                                value = 0;
                        }
                        if (value > 0)
                                return;
                }
	}

	// `continue' in a nested if.
	public static void test17 ()
	{
		 int value;
		 long charCount = 9;
		 long testit = 5;

		 while (charCount > 0) {
			 --charCount;

			 if (testit == 8) {
				 if (testit == 9)
					 throw new Exception ();

				 continue;
			 } else {
				 value = 0;
			 }

			 Console.WriteLine (value);
		 }
	}

	// `out' parameter assigned after conditional exception.
	static void test18 (int a, out int f)
	{
		try {
			if (a == 5)
				throw new Exception ();

			f = 9;
		} catch (IndexOutOfRangeException) {
			throw new FormatException ();
		}
	}

	// code after try/catch block is unreachable. always returns.
        static int test19 () {
                int res;
                int a = Environment.NewLine.Length;
                int fin = 0;

                try {
                        res = 10/a;
                        throw new NotImplementedException ();
                } catch (NotImplementedException e) {
                        fin = 2;
                        throw new NotImplementedException ();
                } finally {
                        fin = 1;
                }
                return fin;
        }

	// from bug #30487.
	static int test20 () {
		try {
			return 0;
		}
		catch (Exception) {
			throw;
		}
	}

	// from bug #31546
	static int test21 () {
		int res;

		try {
			res = 4;
			return 3;
		} catch (DivideByZeroException) {
			res = 33;
		} finally {
			// Do nothing
		}

		return res;
	}

	// the same, but without the finally block.
	static int test22 () {
		int res;

		try {
			res = 4;
			return 3;
		} catch (DivideByZeroException) {
			res = 33;
		}

		return res;
	}

	static int test23 (object obj, int a, out bool test) {
		if (obj == null)
			throw new ArgumentNullException ();

		if (a == 5) {
			test = false;
			return 4;
		} else {
			test = true;
			return 5;
		}
	}

	static long test24 (int a) {
		long b;

		switch (a) {
		case 0:
			return 4;
		}

		if (a > 2) {
			if (a == 5)
				b = 4;
			else if (a == 6)
				b = 5;
			else
				return 7;

			Console.WriteLine (b);
			return b;
		}

		return 4;
	}

	static long test25 (int a) {
		long b, c;

		try {
			b = 5;
		} catch (NotSupportedException) {
			throw new InvalidOperationException ();
		}

		try {
			c = 5;
		} catch {
			throw new InvalidOperationException ();
		}

		return b + c;
	}

	//
	// Tests that the flow analysis is preformed first in the for statement
	// and later on the `increment' part of the for
	//
	static void test26 ()
	{
		int j;
		for( int i=0; i<10; i=j ) 
			j = i+1;
	}

	//
	// Nested infinite loops.  Bug #40670.
	//
	static int test27 ()
	{
		while (true) {
			break;

			while (true)
				Console.WriteLine ("Test");
		}

		return 0;
	}

	//
	// Bug #41657.
	//
	static void test28 (out object value)
	{
		if (true) {
			try {
				value = null;
				return;
			} catch {
			}
		}
		value = null;
	}

	//
	// Bug #47095
	//
	static bool test29 (out int a)
	{
		try {
			a = 0;
			return true;
		} catch (System.Exception) {
			a = -1;
			return false;
		}
	}

	//
	// Bug #46949
	//
	public string test30 (out string outparam)
	{
		try {
			if (true) {
				outparam = "";
				return "";
			}
		} catch {
		}

		outparam = null;
		return null;
	}

	//
	// Bug #49153
	//
	public string test31 (int blah)
	{
		switch(blah) {
		case 1: return("foo"); break;
		case 2: return("bar"); break;
		case 3: return("baz"); break;

		default:
			throw new ArgumentException ("Value 0x"+blah.ToString ("x4")+" is not supported.");
		}
	}

	//
	// Bug #49359
	//
        public void test32 ()
	{
                while (true) {
                        System.Threading.Thread.Sleep (1);
                }

                Console.WriteLine ("Hello");
        }

	//
	// Bug 49602
	//
        public int test33 ()
        {
                int i = 0;
                return 0;
                if (i == 0)
                        return 0;
        }

	//
	// Bug 48962
	//
	public void test34 ()
	{
		int y, x = 3;
		if (x > 3) {
			y = 3;
			goto end;
		}
		return;
        end:
		x = y;
	}

	//
	// Bug 46640
	//
	public static void test35 (int a, bool test)
	{
		switch (a) {
		case 3:
			if (test)
				break;
			return;
		default:
			return;
		}
	}

	//
	// Bug 52625
	//
	public static void test36 ()
	{
		string myVar;
		int counter = 0;

		while (true)
		{
			if (counter < 3)
				counter++;
			else {
				myVar = "assigned";
				break;
			}
		}
		Console.WriteLine (myVar);
	}

	//
	// Bug 58322
	//
	public static void test37 ()
	{
		int x = 0;
		int y = 0;
		switch (x) {
		case 0:
			switch (y) {
			case 0:
				goto k_0;
			default:
				throw new Exception ();
			}
		}

	k_0:
		;
	}

	//
	// Bug 59429
	//
	public static int test38 ()
	{
		return 0;
	foo:
		;
	}

	static int test40 (int stop)
	{
		int service;

		int pos = 0;
		do {
			service = 1;
			break;
		} while (pos < stop);

		return service;
	}
}
