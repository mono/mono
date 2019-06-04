using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

class StackTraceDumper {

	public static void Main ()
	{
		try {
			throw new Exception ("Stacktrace with 1 frame");
		} catch (Exception e) {
			Console.WriteLine (e);
			Console.WriteLine ("Stacktrace:");
			Console.WriteLine (new System.Diagnostics.StackTrace(e));
		}

		Catch (() => {throw new Exception ("Stacktrace with 2 frames");});

		Catch (() => ThrowException ("Stacktrace with 3 frames", 1));

		Catch (() => ThrowException ("Stacktrace with 4 frames", 2));

		Catch (() => {
			var message = "Stack frame with method overload using ref parameter";
			ThrowException (ref message);
		});

		Catch (() => {
			int i;
			ThrowException ("Stack frame with method overload using out parameter", out i);
		});

		Catch (() => ThrowExceptionGeneric<double> ("Stack frame with 1 generic parameter"));

		Catch (() => ThrowExceptionGeneric<double,string> ("Stack frame with 2 generic parameters"));

		Catch (() => ThrowExceptionGeneric (12));

		Catch (() => InnerClass.ThrowException ("Stack trace with inner class"));

		Catch (() => InnerGenericClass<string>.ThrowException ("Stack trace with inner generic class"));

		Catch (() => InnerGenericClass<string>.ThrowException ("Stack trace with inner generic class and method generic parameter", "string"));

		Catch (() => InnerGenericClass<string>.ThrowException<string> ("Stack trace with inner generic class and generic overload", "string"));

		Catch (() => InnerGenericClass<string>.InnerInnerGenericClass<int>.ThrowException ("Stack trace with 2 inner generic class and generic overload"));

		Catch (() => InnerGenericClass<int>.InnerInnerGenericClass<string>.ThrowException ("Stack trace with 2 inner generic class and generic overload"));

		Catch (() => InnerGenericClass<int>.ThrowException ("Stack trace with nested type argument", "string", null));

		Catch (() => {
			var d = new Dictionary<string, string> ();
			d.ContainsKey (null); // ArgumentNullException
		});

		Catch (() => TestAsync ().Wait ());

		Catch (() => TestIterator ().ToArray ());

		/*
		The following test include ambiguous methods we can't resolve. Testing this is hard, so I'm leaving a test behind but disabling it for the time being
		In this case the ambiguous methods are:
			public static void Foo<K> (int a, bool hard_crash, GenClass<T> arg, List<int> zz)
			public static void Foo<K> (int a, bool hard_crash, GenClass<T> arg, List<double> zz)

		The are ambiguous because the only difference is the instantiation on the last parameter which we can't
		figure out from a stacktrace.
		*/
		//Catch (() => ComplicatedTestCase.Run ());
	}

	public static void Catch (Action action)
	{
		try {
			action ();
		} catch (Exception e) {
			Console.WriteLine();
			Console.WriteLine (e);
			Console.WriteLine ("Stacktrace:");
			Console.WriteLine (new System.Diagnostics.StackTrace (e));
		}
	}

	public static void ThrowException (string message)
	{
		throw new Exception (message);
	}

	public static void ThrowException (ref string message)
	{
		throw new Exception (message);
	}

	public static void ThrowException (string message, int i)
	{
		if (i > 1)
			ThrowException (message, --i);

		throw new Exception (message);
	}

	public static void ThrowException (string message, out int o)
	{
		throw new Exception (message);
	}

	public static void ThrowExceptionGeneric<T> (string message)
	{
		throw new Exception (message);
	}

	public static void ThrowExceptionGeneric<T> (T a1)
	{
		throw new Exception ("Stack frame with generic method overload");
	}

	public static void ThrowExceptionGeneric<T> (List<string> a1)
	{
		throw new Exception ("Stack frame with generic method overload");
	}

	public static void ThrowExceptionGeneric<T> (List<T> a1)
	{
		throw new Exception ("Stack frame with generic method overload");
	}

	public static void ThrowExceptionGeneric<T1,T2> (string message)
	{
		throw new Exception (message);
	}

	public static async Task TestAsync ()
	{
		await Task.Yield ();
		throw new ApplicationException ();
	}

	public static IEnumerable<int> TestIterator ()
	{
		yield return 1;
		yield return 3;
		throw new ApplicationException ();
	}

	class InnerClass {
		public static void ThrowException (string message)
		{
			throw new Exception (message);
		}
	}

	class InnerGenericClass<T> {
		public static void ThrowException (string message)
		{
			throw new Exception (message);
		}

		public static void ThrowException (string message, T arg)
		{
			Console.WriteLine ("Generic to string:" + arg.ToString());
			throw new Exception (message);
		}

		public static void ThrowException<T1> (string message, T1 arg)
		{
			throw new Exception (message);
		}

		public static void ThrowException<T1> (string message, T1 arg, InnerGenericClass<T1> _ignore)
		{
			throw new Exception (message as string);
		}

		public class InnerInnerGenericClass<T2> {
			public static void ThrowException (T message)
			{
				throw new Exception (message as string);
			}

			public static void ThrowException (T2 message)
			{
				throw new Exception (message as string);
			}
		}
	}

	class GenClass<T> {
		public static void Foo (int a, bool hard_crash) {
			GenPair<T>.Foo<object> (a, hard_crash, new GenClass<T> (), new List<int> ());
		}
	}

	class GenPair<T> {
		public static void Foo<K> (int a, bool hard_crash, GenClass<T> arg, List<int> zz) {
			Foo<K,K,K> (a, hard_crash, null, null);
		}

		public static void Foo<K,J,F> (int a, bool hard_crash, GenClass<J> arg, List<int> zz) {
			Foo<double> (a, hard_crash, null, new List<double> ());
		}

		public static void Foo<K> (int a, bool hard_crash, GenClass<T> arg, List<double> zz) {
			ComplicatedTestCase.ArrayAndRef (a, new int[2], new int[2,2], ref hard_crash);
		}
	}

	class ComplicatedTestCase {
		public static int ArrayAndRef (int a, int[] b, int[,] c, ref bool hard_crash) {
			Object o = null;
			for (int x = 0; x < a; ++x)
				throw new Exception ("Stack trace with ambiguity");
			return 99;
		}

		public static void Foo (int a, bool hard_crash) {
			GenClass<string>.Foo (a, hard_crash);
		}

		public static void Run () {
			Foo (10, false);
		}
	}
}