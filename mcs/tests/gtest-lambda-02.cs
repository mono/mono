
//
// Lambda expression test overload resolution with parameterless arguments
//

using System;
delegate string funcs (string s);
delegate int funci (int i);

class X {
	static void Foo (funci fi)
	{
		int res = fi (10);
		Console.WriteLine (res);
	}
	
	static void Foo (funcs fs)
	{
		string res = fs ("hello");
		Console.WriteLine (res);
	}

	public static void Main ()
	{
		Foo (x => x + "dingus");
	}
}
