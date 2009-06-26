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

namespace MonoTests.System.Net.Sockets
{
	/// <summary>
	/// Tests System.Net.Sockets.TcpClient
	/// </summary>
	[TestFixture]
	public class TcpClientTest
	{
		
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
			Assert.IsTrue (ret != 0);

			for (int i=0; i<len; i++) 
			{
				Assert.IsTrue (inBuf[i] == outBuf[i]);
			}

			// tidy up
			inSock.Close();
			outClient.Close();
			lSock.Close();
			
		}

		[Test] // bug #81105
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
				Assert.AreEqual (10049, ex.ErrorCode, "ConnectMultiAny #2");
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
				Assert.AreEqual (10061, ex.ErrorCode, "ConnectMultiRefused #2");
			} catch {
				Assert.Fail ("ConnectMultiRefused #3");
			}
		}
#endif
	}
}
