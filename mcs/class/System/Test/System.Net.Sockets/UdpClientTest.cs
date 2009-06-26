// System.Net.Sockets.UdpClientTest.cs
//
// Authors:
//	Chris Bacon <chris.bacon@docobo.co.uk>
//


using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {
#if TARGET_JVM
    [Ignore("UdpClient is not supported - since UDP sockets are not supported")]
#endif
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

			client.Close ();
		}

#if NET_2_0
		[Test]
		public void CloseInReceive ()
		{
			UdpClient client = new UdpClient (50000);
			new Thread(delegate() {
				Thread.Sleep(2000);
				client.Close();
				}).Start();

			bool got_exc = false;
			IPEndPoint ep = new IPEndPoint (IPAddress.Any, 0);
			try {
				client.Receive(ref ep);
			} catch (SocketException) {
				got_exc = true;
			} finally {
				client.Close ();
			}
			Assert.IsTrue (got_exc);
		}

		[Test]
		public void JoinMulticastGroup ()
		{
			UdpClient client = new UdpClient ();
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.23");
			IPAddress local_addr = Dns.GetHostEntry ("").AddressList[0];
			bool exThrown = false;
			
			/* So much for the documented note "You cannot
			 * call JoinMulticastGroup on a UdpClient
			 * constructed without a specific local port
			 * (that is, using the UdpClient or
			 * UdpClient(AddressFamily) constructor).
			 */
			try {
				client.JoinMulticastGroup (mcast_addr,
							   local_addr);
			} catch (Exception) {
				exThrown = true;
			} finally {
				client.Close ();
			}
			
			Assert.IsFalse (exThrown,
					"UdpClient JoinMulticastGroup #1");
		}
		
		// Test for bug 324033
		[Test]
		public void JoinMulticastGroupWithLocal ()
		{
			UdpClient client = new UdpClient (9001);
			IPAddress mcast_addr = IPAddress.Parse ("224.0.0.24");
			IPAddress local_addr = IPAddress.Any;
			
			bool exThrown = false;
			
			try {
				client.JoinMulticastGroup (mcast_addr,
							   local_addr);
			} catch (Exception) {
				exThrown = true;
			} finally {
				client.Close ();
			}
			
			Assert.IsFalse (exThrown, "UdpClient JoinMulticastGroupWithLocal #1");
		}
		
		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void BeginSendNull ()
		{
			UdpClient client = new UdpClient ();
			
			client.BeginSend (null, 0, null, null);

			client.Close ();
		}
		
		static bool BSSent = false;
		static int BSBytes;
		static ManualResetEvent BSCalledBack = new ManualResetEvent (false);
		
		private static void BSCallback (IAsyncResult asyncResult)
		{
			UdpClient client = (UdpClient)asyncResult.AsyncState;
			
			BSBytes = client.EndSend (asyncResult);
			
			BSSent = true;
			BSCalledBack.Set ();
		}
		
		[Test]
		public void BeginSend ()
		{
			UdpClient client = new UdpClient ();
			byte[] bytes = new byte[] {10, 11, 12, 13};

			try {
				client.BeginSend (bytes, bytes.Length, new AsyncCallback (BSCallback), client);
				Assert.Fail ("BeginSend #1");
			} catch (SocketException ex) {
				Assert.AreEqual (10057, ex.ErrorCode,
						 "BeginSend #2");
			}
			
			try {
				client.BeginSend (bytes, bytes.Length, null, new AsyncCallback (BSCallback), client);
				Assert.Fail ("BeginSend #3");
			} catch (SocketException ex) {
				Assert.AreEqual (10057, ex.ErrorCode,
						 "BeginSend #4");
			}

			IPEndPoint ep = new IPEndPoint (Dns.GetHostEntry ("").AddressList[0], 1236);
			
			BSCalledBack.Reset ();
			
			client.BeginSend (bytes, bytes.Length, ep,
					  new AsyncCallback (BSCallback),
					  client);
			if (BSCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginSend wait timed out");
			}
			
			Assert.AreEqual (true, BSSent, "BeginSend #5");
			Assert.AreEqual (4, BSBytes, "BeginSend #6");

			client.Close ();
		}
		
		static bool BRReceived = false;
		static byte[] BRBytes;
		static IPEndPoint BRFrom;
		static ManualResetEvent BRCalledBack = new ManualResetEvent (false);
		
		private static void BRCallback (IAsyncResult asyncResult)
		{
			UdpClient client = (UdpClient)asyncResult.AsyncState;
			
			BRBytes = client.EndReceive (asyncResult, ref BRFrom);
			
			BRReceived = true;
			BRCalledBack.Set ();
		}
		
		[Test]
		public void BeginReceive ()
		{
			UdpClient client = new UdpClient (1237);
			
			BRCalledBack.Reset ();
			
			client.BeginReceive (BRCallback, client);

			IPEndPoint ep = new IPEndPoint (Dns.GetHostEntry ("").AddressList[0], 1237);
			byte[] send_bytes = new byte[] {10, 11, 12, 13};
			client.Send (send_bytes, send_bytes.Length, ep);

			if (BRCalledBack.WaitOne (2000, false) == false) {
				Assert.Fail ("BeginReceive wait timed out");
			}
			
			Assert.AreEqual (true, BRReceived, "BeginReceive #1");
			Assert.AreEqual (4, BRBytes.Length, "BeginReceive #2");
			Assert.AreEqual (ep. Port, BRFrom.Port, "BeginReceive #3");
			Assert.AreEqual (ep.Address, BRFrom.Address, "BeginReceive #4");

			client.Close ();
		}
		
		[Test]
		public void Available ()
		{
			UdpClient client = new UdpClient (1238);
			IPEndPoint ep = new IPEndPoint (Dns.GetHostEntry ("").AddressList[0], 1238);
			byte[] bytes = new byte[] {10, 11, 12, 13};
			
			client.Send (bytes, bytes.Length, ep);
			int avail = client.Available;
			
			Assert.AreEqual (bytes.Length, avail, "Available #1");

			client.Close ();
		}
		
		[Test]
		[Category ("NotWorking")]  // Using PMTU settings workaround on Linux, default true
		public void DontFragmentDefault ()
		{
			UdpClient client = new UdpClient ();
			
			/* Ignore the docs, testing shows the default
			 * here is in fact false
			 */
			Assert.AreEqual (false, client.DontFragment, "DontFragmentDefault");

			client.Close ();
		}
		
		[Test]
		public void EnableBroadcastDefault ()
		{
			UdpClient client = new UdpClient ();
			
			Assert.AreEqual (false, client.EnableBroadcast, "EnableBroadcastDefault");

			client.Close ();
		}
		
		/* Can't test the default for ExclusiveAddressUse as
		 * it's different on different versions and service
		 * packs of windows
		 */
		[Test]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseUnbound ()
		{
			UdpClient client = new UdpClient ();

			client.ExclusiveAddressUse = true;

			Assert.AreEqual (true, client.ExclusiveAddressUse, "ExclusiveAddressUseUnbound");

			client.Close ();
		}
		
		[Test]
		[ExpectedException (typeof(InvalidOperationException))]
		[Category ("NotWorking")] // Not supported on Linux
		public void ExclusiveAddressUseBound ()
		{
			UdpClient client = new UdpClient (1239);

			client.ExclusiveAddressUse = true;

			client.Close ();
		}
		
		[Test]
		public void MulticastLoopbackDefault ()
		{
			UdpClient client = new UdpClient ();
			
			Assert.AreEqual (true, client.MulticastLoopback, "MulticastLoopbackDefault");

			client.Close ();
		}
		
		/* No test for Ttl default as it is platform dependent */
#endif
	}
}
