using System;

public class Test {
	public static Comparison<U> WrapComparison<U>(Comparison<U> comparison)
	{
		return delegate(U x, U y) { return comparison(x, y); };
	}

	public delegate int MyComparison<V> (V x, V y);
	public static MyComparison<W> WrapMyComparison<W>(MyComparison<W> myComparison)
	{
		return delegate(W x, W y) { return myComparison(x, y); };
	}
}

public class Foo {
	static int compare (int x, int y)
	{ return x - y; }
	static int compare (string x, string y)
	{ return string.Compare (x, y); }
	static void test (int i)
	{
		if (i != 0)
			throw new Exception (""+i);
	}
	public static void Main ()
	{
		Comparison<int> ci = Test.WrapComparison<int> (compare);
		Comparison<string> cs = Test.WrapComparison<string> (compare);
		Test.MyComparison<int> mci = Test.WrapMyComparison<int> (compare);
		Test.MyComparison<string> mcs = Test.WrapMyComparison<string> (compare);
		test (ci (1,1));
		test (cs ("h", "h"));
		test (mci (2,2));
		test (mcs ("g", "g"));
	}
}

