#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using Proxy.MonoTests.Features.Client;
using NUnit.Framework;
using System.ServiceModel;
using MonoTests.Features.Contracts;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
    public class FaultsTest : TestFixtureBase<FaultsTesterContractClient, FaultsTester, MonoTests.Features.Contracts.IFaultsTesterContract>
	{
		[Test]
		public void TestFault ()
		{
			try {
				Client.FaultMethod ("heh");
			}
			catch (FaultException e) {
				return;
            		}
			Assert.Fail ("No exception was thrown");
		}
	}

	[TestFixture]
    public class FaultsTestIncludeDetails : TestFixtureBase<FaultsTesterContractClientIncludeDetails, MonoTests.Features.Contracts.FaultsTesterIncludeDetails, MonoTests.Features.Contracts.IFaultsTesterContractIncludeDetails>
	{
		[Test]
		public void TestFault ()
		{
			try {
				Client.FaultMethod ("heh");
			}
			catch (FaultException<ExceptionDetail> e) {
				Assert.AreEqual ("heh", e.Message);
				return;
			}
			Assert.Fail ("No exception was thrown");
		}
	}
}
#endif