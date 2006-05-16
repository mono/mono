// #77358
using System;

public class Container<T>
	where T : IComparable<T>
{
}

public class ReferenceType : IComparable<ReferenceType>
{
	public int value;

	public int CompareTo (ReferenceType obj)
	{
		return 0;
	}
};

public struct MyValueType : IComparable<MyValueType>
{
	public int value;

	public int CompareTo (MyValueType obj)
	{
		return 0;
	}
};

public class Test
{
	public static void Main ()
	{
		// Compilation succeeds, constraint satisfied
		new Container<ReferenceType> ();

		// Compilation fails, constraint not satisfied according to mcs,
		// the unmodified testcase compiles successfully with csc
		new Container<MyValueType> ();
	}
};
