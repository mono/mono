using System;
using System.Runtime.CompilerServices;

public class Tests
{
	public static int Main (String[] args) {
		try {
			recurse ();
		} catch (InsufficientExecutionStackException) {
			return 0;
		}
		return 1;
	}

	static unsafe void recurse () {
		RuntimeHelpers.EnsureSufficientExecutionStack ();
		byte *p = stackalloc byte [16];
		recurse ();
	}
}
