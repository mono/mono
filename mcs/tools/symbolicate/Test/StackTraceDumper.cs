using System;

class StackTraceDumper {
	public static void Main () {
		// Stacktrace with no depth
		try {
			throw new Exception ();
		} catch (Exception e) {
			Console.WriteLine (e);
		}
		// Stacktrace with depth of 1
		try {
			ThrowException ();
		} catch (Exception e) {
			Console.WriteLine (e);
		}
		// Stacktrace with depth of 2
		try {
			ThrowException2 ();
		} catch (Exception e) {
			Console.WriteLine (e);
		}
	}

	public static void ThrowException () {
		Console.WriteLine ("Exception is not in the first line!");
		throw new Exception ();
	}

	public static void ThrowException2 () {
		ThrowException ();
	}

}