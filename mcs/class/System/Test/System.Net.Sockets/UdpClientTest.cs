// System.Net.Sockets.UdpClientTest.cs
//
// Authors:
//	Chris Bacon <chris.bacon@docobo.co.uk>
//


using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {
	[TestFixture]
	public class UdpClientTest {
		[Test]
		public void UdpClientBroadcastTest () 
		{
			bool exThrown = false;
			UdpClient client = new UdpClient (new IPEndPoint (IPAddress.Loopback, 1234));
			byte[] bytes = new byte[] {10, 11, 12, 13};

			try {
				client.Send (bytes, bytes.Length, new IPEndPoint (IPAddress.Broadcast, 1235));
			} catch (SocketException) {
				exThrown = true;
			}
			Assert.IsFalse(exThrown, "UdpClient Broadcast #1");
		}
	}
}
