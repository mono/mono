
// Supported by C# 3.0

public class C
{
	public delegate T Func<T> (T t);
	
	public static void Test<T, U> (Func<T> f, U u)
	{
	}
	
	public static void Main ()
	{
		Test<int, string> (delegate (int i) { return i; }, "");
		Test (delegate (int i) { return i; }, 1);
	}
}