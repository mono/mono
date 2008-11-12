// CS1729: The type `TestCases.ClassWithNonPublicConstructor' does not contain a constructor that takes `0' arguments
// Line: 10

namespace TestCases
{
	public class GmcsCtorBug
	{
		public static void Test ()
		{
			new ClassWithNonPublicConstructor ();
		}
	}

	public class ClassWithNonPublicConstructor
	{
		protected ClassWithNonPublicConstructor (int p)
		{
		}
	}
}
