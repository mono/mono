using System;

namespace Test
{
	class MainClass
	{
		public static int Main ()
		{
			if (!Test_1 (new Derived ()))
				return 1;

			if (!Test_2 (new S ()))
				return 2;

			return 0;
		}

		static bool Test_1<T> (Templated<T> template)
		{
			return template is Derived;
		}

		static bool Test_2<U> (IA<U> arg)
		{
			return arg is S;
		}
	}

	public abstract class Templated<T>
	{
	}

	public class Derived : Templated<Derived>
	{
	}

	public interface IA<T>
	{
	}

	public struct S : IA<S>
	{
	}
}
