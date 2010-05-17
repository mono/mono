// CS0120: An object reference is required to access non-static member `App.Test'
// Line: 15

class App
{
	Test a = new Test ();

	public Test Test
	{
		get { return a; }
	}

	public static void Main (string[] args)
	{
		Test.Run ();
	}
}

class Test
{
	public void Run () { }
}
