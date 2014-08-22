using System;

namespace TestAttributesCollecting
{
	class A : Attribute
	{
	}

	public partial class X
	{
		[A]
		partial void Foo<[A] T>(/*[A]*/ int p);
	}

	public partial class X
	{
		partial void Foo<T> (int p)
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
			var m = typeof (X).GetMethod ("Foo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			var x = m.GetCustomAttributes (true);
			Console.WriteLine (x.Length);
			if (x.Length != 1)
				return 1;

			var ga = m.GetGenericArguments ();
			x = ga [0].GetCustomAttributes (false);
			if (x.Length != 1)
				return 2;

			x = typeof (Y).GetMethod ("Foo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetCustomAttributes (true);
			Console.WriteLine (x.Length);
			if (x.Length != 1)
				return 3;

			return 0;
		}
	}
}
