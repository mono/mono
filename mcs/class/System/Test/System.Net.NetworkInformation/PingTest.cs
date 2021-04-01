using NUnit.Framework;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;

namespace MonoTests.System.Net.NetworkInformation
{
	[TestFixture]
	[Category("NotWasm")]
	public partial class PingTest
	{
		partial void AndroidShouldPingWork (ref bool shouldWork);

		[Test]
		public void PingFail()
		{
#if MONOTOUCH
			Assert.Ignore ("Ping implementation is broken on MT (requires sudo access)");
#else
			var p = new Ping ().Send ("192.0.2.0");
			Assert.AreNotEqual(IPStatus.Success, p.Status);
#endif
		}

		[Test]
		public void PingSuccess()
		{
#if MONOTOUCH
			Assert.Ignore ("Ping implementation is broken on MT (requires sudo access)");
#else
			bool shouldWork = true;
			AndroidShouldPingWork (ref shouldWork);
			if (shouldWork) {
				var p = new Ping ().Send ("127.0.0.1");
				Assert.AreEqual(IPStatus.Success, p.Status);
			} else
				Assert.Ignore ("Ping will not work on this Android device");
#endif
		}		

		[Test]
#if MONOTOUCH
		[Ignore("Ping implementation is broken on MT (requires sudo access)")]
#endif
		public void SendAsyncIPV4Succeeds()
		{
			var testIp = IPAddress.Loopback;
			var ping = new Ping ();
			PingReply reply = null;

			using (var waiter = new AutoResetEvent (false)) {
				ping.PingCompleted += new PingCompletedEventHandler ( 
					(s, e) => {
						reply = e.Reply;
						(e.UserState as AutoResetEvent) ?.Set ();
					});

				ping.SendAsync (testIp, waiter);

				waiter.WaitOne (TimeSpan.FromSeconds (8));
			}

			Assert.AreEqual (IPStatus.Success, reply.Status);
		}

		[Test]
#if MONOTOUCH
		[Ignore("Ping implementation is broken on MT (requires sudo access)")]
#endif
		public void SendAsyncIPV4Fails()
		{
			var testIp = IPAddress.Parse("192.0.2.0");
			var ping = new Ping ();
			PingReply reply = null;

			using (var waiter = new AutoResetEvent (false)) {
				ping.PingCompleted += new PingCompletedEventHandler ( 
					(s, e) => {
						reply = e.Reply;
						(e.UserState as AutoResetEvent) ?.Set ();
					});

				ping.SendAsync (testIp, waiter);

				waiter.WaitOne (TimeSpan.FromSeconds (8));
			}

			Assert.AreNotEqual (IPStatus.Success, reply.Status);
		}

		[Test]
		[Category("MultiThreaded")]
#if MONOTOUCH
		[Ignore("Ping implementation is broken on MT (requires sudo access)")]
#endif
		public void SendPingAsyncIPV4Succeeds()
		{
			var testIp = IPAddress.Loopback;
			var ping = new Ping ();
			var task = ping.SendPingAsync (testIp);

			task.Wait();

			var result = task.Result;

			Assert.AreEqual (IPStatus.Success, result.Status);
		}

		[Test]
		[Category("MultiThreaded")]
#if MONOTOUCH
		[Ignore("Ping implementation is broken on MT (requires sudo access)")]
#endif
		public void SendPingAsyncIPV4Fails()
		{
			var testIp = IPAddress.Parse("192.0.2.0");
			var ping = new Ping ();
			var task = ping.SendPingAsync (testIp);

			task.Wait();

			var result = task.Result;

			Assert.AreNotEqual (IPStatus.Success, result.Status);
		}
	}
}

