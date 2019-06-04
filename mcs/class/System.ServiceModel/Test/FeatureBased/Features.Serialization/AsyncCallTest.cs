#if !MOBILE && !XAMMAC_4_5
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Proxy.MonoTests.Features.Client;
using MonoTests.Features.Contracts;
using System.Threading;

namespace MonoTests.Features.Serialization
{
	[TestFixture]
	public class AsyncCallTest : TestFixtureBase<AsyncCallTesterContractClient, AsyncCallTester, MonoTests.Features.Contracts.IAsyncCallTesterContract>
	{
		bool client_QueryCompleted;
		string s = string.Empty;
        AutoResetEvent ev;
        Exception err = null;

        public AsyncCallTest()
        {
        }
		[Test]
		[Category ("NotWorking")]
		public void TestAsyncCall ()
		{
            ev = new AutoResetEvent(false);
			client_QueryCompleted = false;

			ClientProxy.QueryCompleted += new EventHandler<QueryCompletedEventArgs>(Client_QueryCompleted);
			ClientProxy.QueryAsync ("heh");
            ev.WaitOne(2000, true);
            Assert.IsTrue(client_QueryCompleted, "async call completed");
            Assert.AreEqual("hehheh", s, "#1");
            if (err != null) throw err;
		}

		private void Client_QueryCompleted (object sender, QueryCompletedEventArgs e)
		{
			client_QueryCompleted = true;
            try
            {
                s = e.Result;
            }
            catch (Exception _e)
            {
                err = _e;
            }
            ev.Set();	
		}
	}
}
#endif
