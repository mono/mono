// System.Net.Sockets.TcpClientTest.cs
//
// Authors:
//    Brad Fitzpatrick (brad@danga.com)
//
// (C) Copyright 2003 Brad Fitzpatrick
//

using System;
using System.Net;
using System.Net.Sockets;
using NUnit.Framework;

namespace MonoTests.System.Net.Sockets
{
	[TestFixture]
	public class AsyncSocketTest
	{
		[Test]
		public void EndConnect ()
		{
		    IPAddress ipOne = IPAddress.Parse ("127.1.2.3");   // something bogus
		    IPEndPoint ipEP = new IPEndPoint (ipOne, 23483);  // something bogus
		    Socket sock = new Socket (ipEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
		    IAsyncResult ar = sock.BeginConnect (ipEP, null, null);
		    bool gotException = false;

		    try {
			sock.EndConnect (ar);  // should raise an exception because connect was bogus
		    } catch {
			gotException = true;
		    }

		    Assertion.AssertEquals ("A01", gotException, true);
		}	
		
	}

}

