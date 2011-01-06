// CS0305: Using the generic method `C.Test<T,Y>(C.Func<T>)' requires `2' type argument(s)
// Line: 14

public class C
{
	public delegate int Func<T> (T t);
	
	public static void Test<T, Y> (Func<T> f)
	{
	}

	public static void Main ()
	{
		Test<int> (delegate (int i) { return i; });
	}
}
