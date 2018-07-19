#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using Proxy.MonoTests.Features.Client;
using NUnit.Framework;
using System.ServiceModel;
using MonoTests.Features.Contracts;
using System.Threading;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
	public class DualContractFirstTest : TestFixtureBase<object, DualContractServer, MonoTests.Features.Contracts.IFirstContract>
	{
		[Test]
		public void TestFirst () {
			Assert.AreEqual (Client.FirstMethod (), 1, "IFirstContract.FirstMethod");
		}
	}

	[TestFixture]
	public class DualContractSecondTest : TestFixtureBase<object, DualContractServer, MonoTests.Features.Contracts.ISecondContract>
	{
		[Test]
		public void TestSecond () {
			Assert.AreEqual (Client.SecondMethod (), 2, "ISecondContract.SecondMethod");
		}
	}

}
#endif