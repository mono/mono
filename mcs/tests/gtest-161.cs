// Compiler options: -r:gtest-161-lib.dll

public class App
{
	public static void Main ()
	{
		string s = apply<int, string> (3, delegate (int x) {
			return x.ToString ();
		});

		int y = apply<int, int> (3, FP.identity<int>);
	}

	static U apply<T, U> (T obj, FP.Mapping<T, U> f)
	{
		return f (obj);
	}
}
