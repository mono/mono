using System;

class Program {
	static void Main (string[] args)
	{
		// If this runs without a TLE, the test passed.  A
		// TypeLoadException due to recursion during type
		// initialization is a failure.
		var subC = new SubClass ();
		Console.WriteLine (subC.GetTest ());
	}
}

public struct ValueTest<U> {
        // When U is instantiated with T, from BaseClass, we know it'll be a
	// reference field without having to fully initialize its parent
	// (namely BaseClass<T> itself), so we know the instantiation
	// ValueTest<T> won't be blittable.
	public readonly U value;
}

public abstract class BaseClass<T> where T : BaseClass<T> {
	public ValueTest<T> valueTest = default (ValueTest<T>);
}

public class SubClass : BaseClass<SubClass> {
	private string test = "test";

	public string GetTest()
	{
		return test;
	}
}
