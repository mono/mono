// System.Net.Sockets.TcpListenerTest.cs
//
// Author:
//    Phillip Pearson (pp@myelin.co.nz)
//
// Copyright (C) 2001, Phillip Pearson
//    http://www.myelin.co.nz
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets {

	/// <summary>
	/// Tests System.Net.Sockets.TcpListener
	/// </summary>
	public class TcpListenerTest : TestCase {
		
		public TcpListenerTest(string name) : base(name) {}

		public static ITest Suite {
			get {
				return new TestSuite(typeof (TcpListenerTest));
			}
		}

		/// <summary>
		/// Tests the TcpListener object
		/// (from System.Net.Sockets)
		/// </summary>
		public void test_TcpListener()
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
			Assert(inListener.Pending());
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
			Assert(ret != 0);
			for (int i=0; i<len; i++) 
			{
				Assert(inBuf[i] == outBuf[i]);
			}


			// tidy up after ourselves
			inSock.Close();

			inListener.Stop();	
		}
	
		
	}

}
