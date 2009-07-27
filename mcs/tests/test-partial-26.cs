using System;

namespace ConsoleApplication1
{
	public partial class X
	{
		[CLSCompliant (true)]
		partial void Foo ();
	}

	public partial class X
	{
		partial void Foo ()
		{
			int i;
		}
	}

	class Program
	{
		static int Main ()
		{
			var x = typeof (X).GetMethod ("Foo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetCustomAttributes (true);
			Console.WriteLine (x.Length);
			if (x.Length == 1)
				return 0;

			return 1;
		}
	}
}
