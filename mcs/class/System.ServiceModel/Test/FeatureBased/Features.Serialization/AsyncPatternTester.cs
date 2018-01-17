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
	public class AsyncPatternTest : TestFixtureBase<AsyncPatternClient, AsyncPatternServer, MonoTests.Features.Contracts.IAsyncPattern>
	{

		[Test]
		public void TestAsync () {
			Assert.AreEqual (ClientProxy.AsyncMethod (), 3, "Called method with AsyncPattern=true");
		}
	}
}
#endif