using NUnit.Framework;
using System;
using System.Net.NetworkInformation;

namespace MonoTests.System.Net.NetworkInformation
{
	[TestFixture]
	public class PingTest
	{
		[Test] 
		public void PingTimeOut()
		{
			var p = new Ping ().Send ("192.0.2.0");
			Assert.AreEqual(p.Status, IPStatus.TimedOut);
		}

		[Test] 
		public void PingSuccess()
		{
			var p = new Ping ().Send ("127.0.0.1");
			Assert.AreEqual(p.Status, IPStatus.Success);
		}		
	}
}

