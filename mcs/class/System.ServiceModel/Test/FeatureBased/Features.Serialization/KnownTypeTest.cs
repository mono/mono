#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using MonoTests.Features.Contracts;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
    public class KnownTypeTest : TestFixtureBase<object, MonoTests.Features.Contracts.KnownTypeTester, MonoTests.Features.Contracts.IKnownTypeTesterContract>
	{
		[Test]
		public void TestKnownType ()
		{
			Point2D p1 = new Point2D ();
			p1.X = 1;
			p1.Y = 1;

			Point2D p2 = new Point2D ();
			p2.X = 2;
			p2.Y = 3;

			Point2D r = Client.Move (p1, p2);
			Assert.IsNotNull (r, "#1");
			Assert.AreEqual (typeof (AdvPoint2D), r.GetType (), "#2");
			Assert.AreEqual (((AdvPoint2D) r).ZeroDistance, 5, "#3");

		}

		[Test]
		public void TestKnowType2 () {
			BaseContract [] x = Client.foo ();
		}
	}
}
#endif