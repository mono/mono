// CS0628: `Test.print_argv(string[])': new protected member declared in sealed class
// Line: 8
// Compiler options: -warn:4 -warnaserror

using System;

internal sealed class Test {
	protected string print_argv (string[] argv)
	{
		if (argv == null)
			return "null";
		else
			return String.Join (":", argv);
	}

	static void Main () { }
}
