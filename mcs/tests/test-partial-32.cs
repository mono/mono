namespace A
{
	using X;

	partial class C
	{
		private class N : N1
		{
		}

		public static void Main ()
		{			
		}
	}
}

namespace A
{
	using X;

	partial class C : C1
	{
	}
}


namespace X
{
	public class C1
	{
		public class N1
		{

		}
	}
}