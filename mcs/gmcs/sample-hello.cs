using System;
using Generics;

namespace Test
{
	public class Bar : Foo
	{
		public void Hello (Stack<int> stack)
		{
			Console.WriteLine ("Hello Generic World!");
			Console.WriteLine (stack);
			Console.WriteLine (stack.GetType ());
		}

		public Stack<int> Test ()
		{
			return Driver.int_stack;
		}

		public static void Main ()
		{
			Foo foo = new Bar ();
			Driver.Init (foo);
			Stack<int> a = Driver.int_stack;
			Console.WriteLine ("STACK: {0}", a);
			foo.Hello (a);
		}
	}
}
