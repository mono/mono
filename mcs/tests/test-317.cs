// Compiler options: /warn:4 /warnaserror
interface IFoo
{
	void Test (int t);
}

interface IBar : IFoo
{
	new int Test (int t);
}

class X
{
	public static void Main ()
	{ }
}
