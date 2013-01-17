
//
// This test is used to test the type information flow between arguments
// in a generic method invocation, where:
// 
//    1. We first infer the type of X from the first argument to F
// 
//    2. We use this information to infer from the type of f1 and Func
//       that X is a TimeSpan.
//
//    3. Use the X=String and Y=TimeSpan to infer the value for Z
//       which is double
//

using System;
public delegate TResult Func<TArg0, TResult> (TArg0 arg0);

class Demo {
	static Z F<X,Y,Z>(X value, Func<X,Y> f1, Func<Y,Z> f2)
	{
		return f2 (f1(value));
	}
	public static int Main ()
	{
		double d = F("1:15:30", s => TimeSpan.Parse(s), t => t.TotalSeconds);
		if (d < 4529 || d > 4531)
			return 1;
		return 0;
	}
}
