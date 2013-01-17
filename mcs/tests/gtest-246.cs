using System;

namespace TestCase
{
	interface ITest
	{
	}

	class CTest : ITest
	{
		public static void Main()
		{
		}

		public void Bar()
		{
		}
	}

	class CGenericTest<T,V>
		where T : ITest
		where V : CTest, T, new()	
	{
		public V Foo()
		{	
			V TestObject = new V();
			TestObject.Bar();
			return TestObject;
		}
	}
}
