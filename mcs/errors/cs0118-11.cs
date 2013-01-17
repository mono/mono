// CS0118: `Test.SomeDel' is a `type' but a `variable' was expected
// Line: 14

using System;

namespace Test
{
	public delegate void SomeDel (Action a);

	public class TestClass
	{
		public void TestMethod ()
		{
			SomeDel (() => { });
		}
	}
}