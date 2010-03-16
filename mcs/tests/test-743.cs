// Compiler options: -r:test-743-lib.dll

using System;

public class C : A
{
	public static void Main ()
	{
		new C ().Test ();
	}
	
	void Test ()
	{
		var a = new C ();
		Console.WriteLine (a.Prop);
		a [5] = "2";
	}
}