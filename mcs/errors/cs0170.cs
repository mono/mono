// CS0170: Use of possibly unassigned field `a'
// Line: 21

namespace CS0170
{
	public struct Foo {
		public int a;
	}

	public class Bar
	{
		public void Inc (int x)
		{
			++x;
		}		

		static void Main ()
		{
			Foo f;
			Bar b = new Bar();
			b.Inc (f.a);
		}
	}
}
