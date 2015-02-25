using NUnit.Framework;
using System;
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{
	[TestFixture]
	public class PingTest
	{
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
			var p = new Ping ().Send ("127.0.0.1");
			Assert.AreEqual(IPStatus.Success, p.Status);
#endif
		}		
	}
}

