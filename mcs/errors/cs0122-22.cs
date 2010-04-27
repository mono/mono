// CS0122: `Test.A.B' is inaccessible due to its protection level
// Line: 6

namespace Test
{
	public sealed class A
	{
		private class B
		{
			public static void Method ()
			{
			}
		}
	}
	
	class MainClass
	{
		public static void Main(string[] args)
		{
			A.B.Method ();
		}
	}
}
