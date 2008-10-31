using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class LinkAreaTest : TestHelper
	{	
#if NET_2_0
		[Test]
		public void LinkAreaToString ()
		{
			LinkArea la = new LinkArea ();
			Assert.AreEqual ("{Start=0, Length=0}", la.ToString (), "A1");

			la = new LinkArea (0, 0);
			Assert.AreEqual ("{Start=0, Length=0}", la.ToString (), "A2");

			la = new LinkArea (4, 75);
			Assert.AreEqual ("{Start=4, Length=75}", la.ToString (), "A3");		
		}

		[Test]
		public void Equality ()
		{
			LinkArea l1 = new LinkArea (2, 4);
			LinkArea l2 = new LinkArea (4, 6);
			LinkArea l3 = new LinkArea (2, 4);

			Assert.IsTrue (l1 == l3, "A1");
			Assert.IsFalse (l1 == l2, "A2");
			Assert.IsFalse (l2 == l3, "A3");
		}

		[Test]
		public void Inequality ()
		{
			LinkArea l1 = new LinkArea (2, 4);
			LinkArea l2 = new LinkArea (4, 6);
			LinkArea l3 = new LinkArea (2, 4);

			Assert.IsFalse (l1 != l3, "A1");
			Assert.IsTrue (l1 != l2, "A2");
			Assert.IsTrue (l2 != l3, "A3");
		}
#endif
	}
}
