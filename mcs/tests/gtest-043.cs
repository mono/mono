// Static fields in generic types: this is a runtime/JIT-only test
//
// We need to make sure that we're instantiating each closed generic
// type (ie. "Test<int>") only once.

using System;

public class Test<T>
{
	public static int Count;

	public void Foo ()
	{
		Count++;
	}

	public int GetCount ()
	{
		return Count;
	}
}

class X
{
	static int DoTheTest<T> ()
	{
		Test<T> test = new Test<T> ();

		test.Foo ();
		if (test.GetCount () != 1)
			return 1;
		if (Test<T>.Count != 1)
			return 2;

		test.Foo ();
		if (test.GetCount () != 2)
			return 3;
		if (Test<T>.Count != 2)
			return 4;

		test.Foo ();
		if (test.GetCount () != 3)
			return 5;
		if (Test<T>.Count != 3)
			return 6;

		return 0;
	}

	public static int Main ()
	{
		int result = DoTheTest<int> ();
		if (result != 0)
			return result;

		result = DoTheTest<long> () + 10;
		if (result != 10)
			return result;

		Test<int>.Count = 0;
		++Test<long>.Count;

		result = DoTheTest<int> () + 20;
		if (result != 20)
			return result;

		if (Test<int>.Count != 3)
			return 31;
		if (Test<long>.Count != 4)
			return 32;
		Test<float>.Count = 5;
		if (Test<int>.Count != 3)
			return 33;
		if (Test<long>.Count != 4)
			return 34;

		return 0;
	}
}
