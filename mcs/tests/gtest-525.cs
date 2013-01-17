using System;

class A
{
	static void MA<T> (string s)
	{
	}

	static void F ()
	{
	}

	public class C
	{
		Func<int> MA;
		int F;
		
		void Foo ()
		{
			F ();
			MA<int> ("");
		}
		
		public static void Main ()
		{
			new C ().Foo ();
		}
	}
}