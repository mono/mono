// CS0411: The type arguments for method `C.Test<TR,TA>(C.Func<TR,TA>, C.Func<TR,TA>)' cannot be inferred from the usage. Try specifying the type arguments explicitly
// Line: 16
// Compiler options: -langversion:linq

public class C
{
	public delegate TR Func<TR, TA> (TA t);
	
   	public static TR Test<TR, TA> (Func<TR, TA> f, Func<TR, TA> f2)
	{
		return default (TR);
	}
	
	public static void Main()
	{
		int s = Test (delegate (int i) { return 0; }, delegate (int i) { return "a"; });
	}
}