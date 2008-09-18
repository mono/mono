using System;

namespace AsStringProblem
{
	class MainClass
	{
		public static void Main ()
		{
			object o = "Hello World";
			Console.WriteLine (o as string + "blah");
			Console.WriteLine (o is string + "blah");
			Console.WriteLine ((o as string) + "blah");
			Console.WriteLine ("blah" + o as string);
		}
	}
}
