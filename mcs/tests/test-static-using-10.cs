namespace A
{
	public class T
	{
		public class N
		{

		}
	}
}

namespace B
{
	using static A.T;

	static class Program
	{
		static void Main ()
		{
			var t = typeof (N);
			var u = new N ();
		}
	}
}
