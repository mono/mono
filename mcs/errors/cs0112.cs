// cs0112.cs: A static method can not be marked as virtual, abstract or override.
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
