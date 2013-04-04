// CS1502: The best overloaded method match for `Foo.Test(int, string)' has some invalid arguments
// Line: 14

using System.Runtime.CompilerServices;

public class Foo
{
	public void Test (int arg, [CallerMemberName] string s = null)
	{
	}

	void X ()
	{
		Test ("");
	}
}