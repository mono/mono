// CS0119: Expression denotes a `type', where a `variable', `value' or `method group' was expected
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