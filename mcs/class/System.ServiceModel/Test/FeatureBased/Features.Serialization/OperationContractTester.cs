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
	public class OperationContractTest : TestFixtureBase<OperationContractClient, OperationContractServer, MonoTests.Features.Contracts.IOperationContract>
	{
		[Test]
		[Category("NotWorking")]
		public void TestName () {
			Assert.AreEqual(Client.OrigMethod(),2,"Calling OrigMethod should actually call RenamedMethod");
			Assert.AreEqual(Client.RenamedMethod(),1,"Calling RenamedMethod should actually call OrigMethod");
		}

		[Test]
		[Category("NotWorking")]
		public void TestOneWay () {
			int sleepTime = 1 * 1000, failTime = 500; // Good times for inproc, no debugging.
			if (!Configuration.IsLocal) {
				sleepTime = 5 * 1000;
				failTime = 2 * 1000;
			}
			var sw = global::System.Diagnostics.Stopwatch.StartNew ();
			Client.Sleep (sleepTime);
			sw.Stop ();
			TimeSpan max = TimeSpan.FromMilliseconds(failTime);
			Assert.IsTrue (sw.Elapsed < max, "Sleep({0} milisec) must end in less than {1} seconds",sleepTime,failTime);
			if (sleepTime > (int) sw.ElapsedMilliseconds)
				Thread.Sleep (sleepTime - (int)sw.ElapsedMilliseconds); // wait for server thread to release itself
		}

		[Test]
		[Category ("NotWorking")]
		public void TestWsdl () {
			CheckWsdlImpl ();
		}
	}
}
#endif