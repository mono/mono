// Test for bug #56442

public class Params
{
	public static readonly object[] test       = new object[] { 1,         "foo",         3.14         };
	public static readonly object[] test_types = new object[] { typeof(int), typeof(string), typeof(double) };
	
	public delegate void FOO(string s, params object[] args);
	
	public static void foo(string s, params object[] args)
	{
		if (args.Length != test.Length)
			throw new System.Exception("Length mismatch during " + s + " invocation");
		for (int i = 0; i < args.Length; ++i)
			if (args[i].GetType() != test_types[i])
				throw new System.Exception("Type mismatch: " + args[i].GetType() + " vs. " + test_types[i]);
	}

	public static void Main()
	{
		foo("normal", test);
		FOO f = new FOO(foo);
		f("delegate", test);
	}
}
