using System;

namespace InlineAssignmentTest
{
	public class Foo
	{
		public bool B = true;
	}

	public class MainClass
	{
		public static int Main ()
		{
			var foo = new Foo () { B = false };
			if (foo.B != false)
				return 1;

			return 0;
		}
	}
}
