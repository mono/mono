// System.Net.Sockets.TcpClientTest.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Copyright 2001 Phillip Pearson (http://www.myelin.co.nz)
// (C) Copyright 2003 Martin Willemoes Hansen
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {

	/// <summary>
	/// Tests System.Net.Sockets.TcpClient
	/// </summary>
	[TestFixture]
	public class TcpClientTest {
		
		/// <summary>
		/// Tests the TcpClient object
		/// (from System.Net.Sockets)
		/// </summary>
		[Test]
		public void TcpClient()
		{
			// set up a listening Socket
			Socket lSock = new Socket(AddressFamily.InterNetwork,
				SocketType.Stream, ProtocolType.Tcp);
			
			lSock.Bind(new IPEndPoint(IPAddress.Any, 8765));
			lSock.Listen(-1);


			// connect to it with a TcpClient
			TcpClient outClient = new TcpClient("localhost", 8765);
			Socket inSock = lSock.Accept();

			
			// now try exchanging data
			NetworkStream stream = outClient.GetStream();

			const int len = 1024;
			byte[] outBuf = new Byte[len];
			for (int i=0; i<len; i++) 
			{
				outBuf[i] = (byte)(i % 256);
			}

			// send it
			stream.Write(outBuf,0,len);

			// and see if it comes back
			byte[] inBuf = new Byte[len];
			int ret = inSock.Receive(inBuf, 0, len, 0);
			Assertion.Assert(ret != 0);

			for (int i=0; i<len; i++) 
			{
				Assertion.Assert(inBuf[i] == outBuf[i]);
			}
			

			// tidy up
			inSock.Close();
			outClient.Close();
			lSock.Close();
			
		}

		[Test] // bug #81105
		[Category ("NotWorking")]
/*
	This test flagged as not working as its producing this:

1) MonoTests.System.Net.Sockets.TcpClientTest.CloseTest : System.Net.Sockets.SocketException : Address already in use
  at System.Net.Sockets.Socket.Bind (System.Net.EndPoint local_end) [0x00059] in /home/cvs/mcs/class/System/System.Net.Sockets/Socket.cs:2015
  at System.Net.Sockets.TcpListener.Start (Int32 backlog) [0x00023] in /home/cvs/mcs/class/System/System.Net.Sockets/TcpListener.cs:265
  at System.Net.Sockets.TcpListener.Start () [0x00000] in /home/cvs/mcs/class/System/System.Net.Sockets/TcpListener.cs:240
  at MonoTests.System.Net.SocketResponder.Start () [0x00011] in /home/cvs/mcs/class/System/Test/System.Net/SocketResponder.cs:67
  at MonoTests.System.Net.Sockets.TcpClientTest.CloseTest () [0x0007d] in /home/cvs/mcs/class/System/Test/System.Net.Sockets/TcpClientTest.cs:111
  at <0x00000> <unknown method>
  at (wrapper managed-to-native) System.Reflection.MonoMethod:InternalInvoke (object,object[])
  at System.Reflection.MonoMethod.Invoke (System.Object obj, BindingFlags invokeAttr, System.Reflection.Binder binder, System.Object[] parameters, System.Globalization.CultureInfo culture) [0x00040] in /home/cvs/mcs/class/corlib/System.Reflection/MonoMethod.cs:143
*/
		public void CloseTest ()
		{
			IPEndPoint localEP = new IPEndPoint (IPAddress.Loopback, 8765);
			using (SocketResponder sr = new SocketResponder (localEP, new SocketRequestHandler (CloseRequestHandler))) {
				sr.Start ();

				TcpClient tcpClient = new TcpClient (IPAddress.Loopback.ToString (), 8765);
				NetworkStream ns = tcpClient.GetStream ();
				Assert.IsNotNull (ns, "#A1");
#if NET_2_0
				Assert.AreEqual (0, tcpClient.Available, "#A2");
				Assert.IsTrue (tcpClient.Connected, "#A3");
				// Assert.IsFalse (tcpClient.ExclusiveAddressUse, "#A4");
#endif
				tcpClient.Close ();
#if NET_2_0
				Assert.IsNotNull (tcpClient.Client, "#A5");
				try {
					int available = tcpClient.Available;
					Assert.Fail ("#A6: " + available);
				} catch (ObjectDisposedException) {
				}
				Assert.IsFalse (tcpClient.Connected, "#A7");
				// not supported on linux
				/*
				try {
					bool exclusive = tcpClient.ExclusiveAddressUse;
					Assert.Fail ("#A8: " + exclusive);
				} catch (ObjectDisposedException) {
				}
				*/
#endif
			}

			using (SocketResponder sr = new SocketResponder (localEP, new SocketRequestHandler (CloseRequestHandler))) {
				sr.Start ();

				TcpClient tcpClient = new TcpClient (IPAddress.Loopback.ToString (), 8765);
#if NET_2_0
				Assert.AreEqual (0, tcpClient.Available, "#B1");
				Assert.IsTrue (tcpClient.Connected, "#B2");
				// Assert.IsFalse (tcpClient.ExclusiveAddressUse, "#B3");
#endif
				tcpClient.Close ();
#if NET_2_0
				Assert.IsNull (tcpClient.Client, "#B4");
				try {
					int available = tcpClient.Available;
					Assert.Fail ("#B5: " + available);
				} catch (NullReferenceException) {
				}
				try {
					bool connected = tcpClient.Connected;
					Assert.Fail ("#B6: " + connected);
				} catch (NullReferenceException) {
				}
				// not supported on linux
				/*
				try {
					bool exclusive = tcpClient.ExclusiveAddressUse;
					Assert.Fail ("#B7: " + exclusive);
				} catch (NullReferenceException) {
				}
				*/
#endif
			}
		}

		byte [] CloseRequestHandler (Socket socket)
		{
			return new byte [0];
		}

#if NET_2_0
		[Test]
		[ExpectedException (typeof(ArgumentNullException))]
		public void ConnectMultiNull ()
		{
			TcpClient client = new TcpClient ();
			IPAddress[] ipAddresses = null;
			
			client.Connect (ipAddresses, 1234);
		}
		
		[Test]
		public void ConnectMultiAny ()
		{
			TcpClient client = new TcpClient ();
			IPAddress[] ipAddresses = new IPAddress[1];
			
			ipAddresses[0] = IPAddress.Any;
			
			try {
				client.Connect (ipAddresses, 1234);
				Assert.Fail ("ConnectMultiAny #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("ConnectMultiAny #2",
							10049, ex.ErrorCode);
			} catch {
				Assert.Fail ("ConnectMultiAny #3");
			}
		}
		
		[Test]
		public void ConnectMultiRefused ()
		{
			TcpClient client = new TcpClient ();
			IPAddress[] ipAddresses = new IPAddress[1];
			
			ipAddresses[0] = IPAddress.Loopback;
			
			try {
				client.Connect (ipAddresses, 1234);
				Assert.Fail ("ConnectMultiRefused #1");
			} catch (SocketException ex) {
				Assertion.AssertEquals ("ConnectMultiRefused #2", 10061, ex.ErrorCode);
			} catch {
				Assert.Fail ("ConnectMultiRefused #3");
			}
		}
		
#endif
		
	}

}
