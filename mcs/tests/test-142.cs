using System;

public class TestClass : TestBaseClass {

	public TestClass (EventHandler hndlr) : base ()
	{
		Blah += hndlr;
	}

	public static int Main ()
	{
		return 0;
	}
}

public class TestBaseClass {

	public event EventHandler Blah;

}
