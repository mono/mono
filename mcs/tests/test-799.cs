using System;

public class Test2
{
	protected internal class Foo
	{
	}

	private class Bar
	{
		public Bar (Test2.Foo baseArg4)
		{
		}
	}
	static int Main ()
	{
		new Bar (new Foo ());
		return 0;
	}
}
