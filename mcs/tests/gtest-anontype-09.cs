using System;

namespace My.System
{
	static class Test
	{
		public static int Main ()
		{
			var a = new { X = 1 };
			Console.WriteLine(a);
			
			var foo = new { Value = default(string) };
			if (foo.Value != null)
				return 1;

	        return 0;
	    }
	}
}
