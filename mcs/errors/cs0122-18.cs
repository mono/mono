// cs0122-18.cs: `Test.TestClass.TestClass()' is inaccessible due to its protection level
// Line: 17

namespace Test
{
	public class TestClass
	{
		private TestClass() : base()
		{
		}
	}

	class Class1
	{
		static void Main(string[] args)
		{
			TestClass test = new TestClass();
		}
	}
} 