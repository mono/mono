// CS0118: `MonoTests.System.Data.Test' is a `namespace' but a `type' was expected
// Line: 22
using NUnit.Framework;
using System;

namespace NUnit.Framework
{
	public class Test : Attribute
	{ }
}

namespace MonoTests.System.Data.Test.Utils
{
	public class Y
	{ }
}

namespace MonoTests.System.Data.SqlTypes
{
	public class X
	{
		[Test]
		public void Hello ()
		{
		}

		static void Main ()
		{ }
	}
}
