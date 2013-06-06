// CS1661: Cannot convert `anonymous method' to delegate type `System.Predicate<T>' since there is a parameter mismatch
// Line: 8

class Test<T>
{
	void test (Test<T> t, System.Predicate<T> p)
	{
		test (t, delegate (Test<T> item) {
			return false;
		});
	}
}
