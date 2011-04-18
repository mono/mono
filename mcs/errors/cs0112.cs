// CS0112: A static member `X.Bar.Add(int, int)' cannot be marked as override, virtual or abstract
// Line: 13

namespace X
{
	public abstract class Foo
	{
		public abstract int Add (int a, int b);
	}

	public class Bar: Foo
	{
		virtual public static int Add (int a, int b)
		{
			int c;
			c = a + b;
			return c;
		}
		
		static int Main () 
		{
			return a;
		}
	}
}
