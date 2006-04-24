using System;

public class Tests {

	public delegate bool FilterStackFrame(object o);

	public static void DumpException(FilterStackFrame fsf) {
	}

	public static void foo (out bool continueInteraction) {
		continueInteraction = false;

		try {
		}
		catch (Exception ex) {
			DumpException(delegate(object o) {
				return true;
			});
		}
	}

	public static void Main (String[] args) {
		bool b;

		foo (out b);
	}
}
