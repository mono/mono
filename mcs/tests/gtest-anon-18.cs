// Compiler options: -langversion:linq
// Supported by C# 3.0

public class C
{
	public delegate TR Func<TR, TA> (TA t);
	
	public static TR Test<TR, TA> (Func<TR, TA> f)
	{
		return default (TR);
	}
	
	public static void Test2<T> ()
	{
		// FIXME:
		//T r = Test (delegate (T i) { return i; });
	}
	
	public static void Main()
	{
		int r = Test (delegate (int i) { return i < 1 ? 'a' : i; });
	}
}