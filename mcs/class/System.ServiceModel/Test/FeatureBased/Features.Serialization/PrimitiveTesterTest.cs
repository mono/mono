
#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using MonoTests.Features;
using MonoTests.Features.Contracts;
using NUnit.Framework;
using Proxy.MonoTests.Features.Client;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
    public class PrimitiveTesterTest : TestFixtureBase<PrimitiveTesterContractClient, PrimitiveTester, MonoTests.Features.Contracts.IPrimitiveTesterContract>
	{
		[Test]
		public void TestDoNothing () 
		{
			Client.DoNothing ();
		}

		[Test]
		public void TestDouble () {
			Assert.IsTrue (Client.AddDouble (1, 1) == 2);
		}

		[Test]
		public void TestByte () {
			Assert.IsTrue (Client.AddByte (1, 1) == 2);
		}

		[Test]
		public void TestSByte () {
			Assert.IsTrue (Client.AddSByte (1, 1) == 2);
		}

		[Test]
		public void TestShort () {
			Assert.IsTrue (Client.AddShort (1, 1) == 2);
		}

		[Test]
		public void TestUShort () {
			Assert.IsTrue (Client.AddUShort (1, 1) == 2);
		}

		[Test]
		public void TestInt () {
			Assert.IsTrue (Client.AddInt (1, 1) == 2);
		}

		[Test]
		public void TestUInt () {
			Assert.IsTrue (Client.AddUInt (1, 1) == 2);
		}

		[Test]
		public void TestLong () {
			Assert.AreEqual (2, Client.AddLong (1, 1));
		}

		[Test]
		public void TestULong () {
			Assert.IsTrue (Client.AddULong (1, 1) == 2);
		}

		[Test]
		public void TestFloat () {
			Assert.IsTrue (Client.AddFloat (1, 1) == 2);
		}

		[Test]
		public void TestChar () {
			Assert.AreEqual (Client.AddChar ((char) 1, (char) 1), (char) 2);
		}

		[Test]
		public void TestByRef () {
			double d;
			double res = ClientProxy.AddByRef (out d, 1, 1);
			Assert.IsTrue(d == res);
		}

		[Test]
		[Category ("NotWorking")]
		public void TestNullableInt() {
			int? x1 = Client.NullableInt(3);
			Assert.AreEqual(x1,4,"TestNullableInt(3)==4");
			int? x2 = Client.NullableInt (null);
			Assert.IsNull (x2, "TestNullableInt(null)==null");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestNullableFloat () {
			float? x1 = Client.NullableFloat ((float)1.5);
			Assert.AreEqual (x1, 2.5, "TestNullableFloat(1.5)==2.5");
			float? x2 = Client.NullableFloat (null);
			Assert.IsNull (x2, "TestNullableFloat(null)==null");
		}

		[Test]
		public void TestTimeSpan () {
			TimeSpan t1 = new TimeSpan (12345);
			TimeSpan t2 = new TimeSpan (12345);
			TimeSpan t3 = Client.AddTimeSpan (t1, t2);
			Assert.AreEqual (t3.Ticks, 24690);
		}

		[Test]
		public void TestByteArray () {
			byte [] b1 = new byte [] { 1, 2, 3, 4, 5 };
			byte [] b2 = new byte [] { 6, 7, 8, 9, 10 };
			byte [] sum = Client.AddByteArray (b1, b2);
			Assert.AreEqual (sum.Length, b1.Length, "Length of return array");
			Assert.AreEqual (sum [4], b1 [4] + b2 [4], "fourth element in return array");
		}
	}
}
#endif