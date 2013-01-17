//
// bug-4786.cs- 
//      NUnit Test Cases for Mono.Data.Tds.Protocol.TdsConnectionPool
//
// Author:
//      Robert Wilkens <robwilkens@gmail.com>
//
// Copyright (C) 2012 Robert Wilkens (http://www.robssoftwareprojects.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace bug4786test
{
	
  using NUnit.Framework;
  using Mono.Data.Tds.Protocol;
  using System;
  using System.Net;
  using System.Net.Sockets;


  [TestFixture]
  public class TdsConnectionPoolTest
  {
    const string SERVER="localhost";
    [Test]
    public void CheckNullException() 
    {

	//set up dummy sql listener, if there is a real sql server on this
	//machine at that port, in theory this part will fail, but that's ok
	//becuase something will be listening on the port and that's all we
	//require at this point: a listener on port 1433...

	try{
		Socket Listener = new Socket(AddressFamily.InterNetwork, 
                                         SocketType.Stream,
                                         ProtocolType.Tcp);
		IPAddress hostIP =Dns.GetHostEntry("localhost").AddressList[0];
        	IPEndPoint ep = new IPEndPoint(hostIP, 1433);
		Listener.Bind(ep); 
        	Listener.Listen(1);
	} catch (Exception){
		//ignore
	}

	//try to connect twice, in earlier failure would get null exception
	//on 2nd call to pool.GetConnection();
	//Most of this code ripped from sqlConnection.Open()

	TdsConnectionPool pool;
	
	TdsConnectionPoolManager sqlConnectionPools = 
		new TdsConnectionPoolManager(TdsVersion.tds80);	
	TdsConnectionInfo info=
		new TdsConnectionInfo(SERVER/*dummyhost*/,1433/*port*/,
		8192/*pktsize*/,15/*timeout*/,0/*minpoolsize*/,
		100/*maxpoolsize*/, 0/*lifetime*/);
	pool=sqlConnectionPools.GetConnectionPool("test",info);
	Tds tds=null;

	//this first one succeeded regardless as long as something answered
	//the phone on port 1433 of localhost
	tds=pool.GetConnection();

	pool.ReleaseConnection(tds);


	// 2nd time thru: This will fail with nullreferenceexception 
	// at pool.GetConnection() unless the patch by Rob Wilkens which 
	// adds "result=null;" before retry in pool.getConnection() source

	//First let's pretend we're calling this test fresh, as if we
	//call sqlConnection.Open() again :

	info=new TdsConnectionInfo(SERVER/*dummyhost*/,1433/*port*/,
		8192/*pktsize*/,15/*timeout*/,0/*minpoolsize*/,
		100/*maxpoolsize*/, 0/*lifetime*/);

	pool=sqlConnectionPools.GetConnectionPool("test",info);

	//Then: Test for failure (will raise uncaught exception which
	//causes failure of test if bug is not fixed
	tds=pool.GetConnection();

	pool.ReleaseConnection(tds);

	//exit
    }
  }
}

