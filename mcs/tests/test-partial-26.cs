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
	
	public partial class Y
	{
		partial void Foo ()
		{
			int i;
		}
	}
	
	public partial class Y
	{
		[CLSCompliant (true)]
		partial void Foo ();
	}

	class Program
	{
		public static int Main ()
		{
			var x = typeof (X).GetMethod ("Foo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetCustomAttributes (true);
			Console.WriteLine (x.Length);
			if (x.Length != 1)
				return 1;

			x = typeof (Y).GetMethod ("Foo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetCustomAttributes (true);
			Console.WriteLine (x.Length);
			if (x.Length != 1)
				return 2;
			
			return 0;
		}
	}
}
