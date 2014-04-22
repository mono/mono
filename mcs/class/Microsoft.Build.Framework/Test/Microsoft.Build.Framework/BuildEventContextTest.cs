using System;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace MonoTests.Microsoft.Build.Framework
{
	[TestFixture]
	public class BuildEventContextTest
	{
		[Test]
		public void Compare ()
		{
			Assert.IsTrue (BuildEventContext.Invalid == BuildEventContext.Invalid, "#1");
			Assert.IsFalse (BuildEventContext.Invalid != BuildEventContext.Invalid, "#2");
			var inst = new BuildEventContext (0, 0, 0, 0);
			Assert.IsFalse (BuildEventContext.Invalid == inst, "#3");
			Assert.IsTrue (BuildEventContext.Invalid != inst, "#4");
			Assert.IsFalse (BuildEventContext.Invalid == null, "#5");
			Assert.IsTrue (BuildEventContext.Invalid != null, "#6");
			Assert.IsFalse (BuildEventContext.Invalid.Equals (null), "#7");
			Assert.IsFalse (BuildEventContext.Invalid.Equals (inst), "#8");
			Assert.IsTrue (BuildEventContext.Invalid.Equals (BuildEventContext.Invalid), "#9");
			Assert.IsFalse (inst.Equals (null), "#10");
			Assert.IsTrue (inst.Equals (inst), "#11");
			Assert.IsFalse (inst.Equals (BuildEventContext.Invalid), "#12");
		}
	}
}

