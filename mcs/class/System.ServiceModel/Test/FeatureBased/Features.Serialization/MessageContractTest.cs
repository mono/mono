#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using Proxy.MonoTests.Features.Client;
using NUnit.Framework;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
	public class MessageContractTest : TestFixtureBase<MessageContractTesterContractClient, MonoTests.Features.Contracts.MessageContractTester, MonoTests.Features.Contracts.IMessageContractTesterContract>
	{
		[Test]
		[Ignore ("fails under .NET; I never bothered to fix the test")]
		public void TestMessageContract ()
		{
			TestMessage msg = new TestMessage ();
			msg.Date = new DateTime (2014, 1, 1);
			msg.FormatString = "yyyy-MM-dd";

			TestMessage r = ((IMessageContractTesterContract) Client).FormatDate (msg);

			Assert.AreEqual ("2014-01-01", r.FormattedDate, "#1");
		}
	}
}
#endif
