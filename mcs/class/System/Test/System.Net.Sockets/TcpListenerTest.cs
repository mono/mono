// System.Net.Sockets.TcpListenerTest.cs
//
// Authors:
//    Phillip Pearson (pp@myelin.co.nz)
//    Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Copyright 2001 Phillip Pearson (http://www.myelin.co.nz)
// (C) Copyright 2003 Martin Willemoes Hansen (mwh@sysrq.dk)
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {

	/// <summary>
	/// Tests System.Net.Sockets.TcpListener
	/// </summary>
	[TestFixture]
	public class TcpListenerTest {
		
		/// <summary>
		/// Tests the TcpListener object
		/// (from System.Net.Sockets)
		/// </summary>
		[Test]
		public void TcpListener()
		{
			// listen with a new listener
			TcpListener inListener = new TcpListener(1234);
			inListener.Start();
			

			// connect to it from a new socket
			Socket outSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
				ProtocolType.IP);
			IPHostEntry hostent = Dns.GetHostByAddress("127.0.0.1");
			IPEndPoint remote = new IPEndPoint(hostent.AddressList[0], 1234);
			outSock.Connect(remote);

			
			// make sure the connection arrives
			Assertion.Assert(inListener.Pending());
			Socket inSock = inListener.AcceptSocket();


			// now send some data and see if it comes out the other end
			const int len = 1024;
			byte[] outBuf = new Byte[len];
			for (int i=0; i<len; i++) 
			{
				outBuf[i] = (byte)(i % 256);
			}

			outSock.Send(outBuf, 0, len, 0);

			byte[] inBuf = new Byte[len];
			int ret = inSock.Receive(inBuf, 0, len, 0);


			// let's see if it arrived OK
			Assertion.Assert(ret != 0);
			for (int i=0; i<len; i++) 
			{
				Assertion.Assert(inBuf[i] == outBuf[i]);
			}


			// tidy up after ourselves
			inSock.Close();

			inListener.Stop();	
		}
	}
}
