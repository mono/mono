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
			
			lSock.Bind(new IPEndPoint(IPAddress.Any, 1234));
			lSock.Listen(-1);


			// connect to it with a TcpClient
			TcpClient outClient = new TcpClient("localhost", 1234);
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
		
	}

}
