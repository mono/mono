using System;
using Generics;

namespace Test
{
	public class Bar : Foo
	{
		void Foo.Hello (Stack<int> stack)
		{
			Console.WriteLine ("Hello Generic World!");
			Console.WriteLine (stack);
			Console.WriteLine (stack.GetType ());
		}

		public static void Main ()
		{
			Foo foo = new Bar ();
			Driver.Init (foo);
		}
	}
}
