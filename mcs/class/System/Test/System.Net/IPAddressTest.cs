//
// IPAddressTest.cs - NUnit Test Cases for System.Net.IPAddress
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc. http://www.ximian.com
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace MonoTests.System.Net
{

[TestFixture]
public class IPAddressTest
{
	static string[] ipv6AddressList = new string[] {
													   "::",					"0:0:0:0:0:0:0:0", 
													   "1::",					"1:0:0:0:0:0:0:0",
													   "2:2::",					"2:2:0:0:0:0:0:0",
													   "7:7:7:7:7:7:7:0",		"7:7:7:7:7:7:7:0",
													   "::1",					"0:0:0:0:0:0:0:1",
													   "0:7:7:7:7:7:7:7",		"0:7:7:7:7:7:7:7",
													   "E::1",					"E:0:0:0:0:0:0:1",
													   "E::2:2",				"E:0:0:0:0:0:2:2",
													   "E:0:6:6:6:6:6:6",		"E:0:6:6:6:6:6:6",
													   "E:E::1",				"E:E:0:0:0:0:0:1",
													   "E:E::2:2",				"E:E:0:0:0:0:2:2",
													   "E:E:0:5:5:5:5:5",		"E:E:0:5:5:5:5:5",
													   "E:E:E::1",				"E:E:E:0:0:0:0:1",
													   "E:E:E::2:2",			"E:E:E:0:0:0:2:2",
													   "E:E:E:0:4:4:4:4",		"E:E:E:0:4:4:4:4",
													   "E:E:E:E::1",			"E:E:E:E:0:0:0:1",
													   "E:E:E:E::2:2",			"E:E:E:E:0:0:2:2",
													   "E:E:E:E:0:3:3:3",		"E:E:E:E:0:3:3:3",
													   "E:E:E:E:E::1",			"E:E:E:E:E:0:0:1",
													   "E:E:E:E:E:0:2:2",		"E:E:E:E:E:0:2:2",
													   "E:E:E:E:E:E:0:1",		"E:E:E:E:E:E:0:1",
													   "::0.2.0.2",				"0:0:0:0:0:0:2:2",
													   "::FFFF:192.168.0.1",	"::FFFF:192.168.0.1",
													   "::FFFF:0.168.0.1",		"::FFFF:0.168.0.1",
													   "::FFFF",				"::0.0.255.255",
													   "::10.0.0.1",			"::10.0.0.1",
													   "1234::1234:0:0",		"1234:0:0:0:0:1234:0:0",
													   "1:0:1:0:1:0:1:0",		"1:0:1:0:1:0:1:0",
													   "1:1:1::1:1:0",			"1:1:1:0:0:1:1:0",
													   "::1234:0:0",			"0:0:0:0:0:1234:0:0",
													   "3ffe:38e1::100:1:1",	"3ffe:38e1::0100:1:0001",
													   "0:0:1:2::",				"0:0:1:2:00:00:000:0000",
													   "100:0:1:2::abcd",		"100:0:1:2:0:0:000:abcd",
													   "ffff::abcd",			"ffff:0:0:0:0:0:00:abcd",
													   "ffff:0:0:2::abcd",		"ffff:0:0:2:0:0:00:abcd",
													   "0:0:1:2::",				"0:0:1:2:0:00:0000:0000",
													   "::1:0:0",				"0000:0000::1:0000:0000",
													   "::111:234:5:6:789a:0",	"0:0:111:234:5:6:789A:0",
													   "11:22:33:44:55:66:77:8","11:22:33:44:55:66:77:8",
													   "0:0:7711:ab42:1230::",	"::7711:ab42:1230:0:0:0",
	};

	static string[] ipv4ParseOk = new string[] {
												   "192.168.1.1",				"192.168.1.1",
												   "0xff.0x7f.0x20.0x01",		"255.127.32.1",
												   "0xff.0x7f.0x20.0xf",		"255.127.32.15",
												   "0.0.0.0",					IPAddress.Any.ToString(),
												   "255.255.255.255",			IPAddress.Broadcast.ToString(),
												   "12.1.1.3 ",					"12.1.1.3",
												   " 12.1.1.1",					"0.0.0.0",
												   "12.1 .1.2",					"12.0.0.1",
												   ".1.1.6",					"0.1.1.6",
												   "12.1.7",					"12.1.0.7",
												   "12.1.8. ",					"12.1.8.0",
												   "12",						"0.0.0.12",
												   "12.1 foo.1.2.3.4.5.bar",	"12.0.0.1",		
												   "12.1.2. ",					"12.1.2.0",		
												   "12.. .",					"12.0.0.0",	
												   " ",							"0.0.0.0",		
												   " foo",						"0.0.0.0"
											   };

	static object[] ipv4ParseWrong = new object[] {
													  "12.+1.1.4",	typeof(FormatException),
													  "12.1.-1.5",	typeof(FormatException), 
													  "257.1.1.9",	typeof(FormatException), 
													  "12.",		typeof(FormatException),
													  "12.1.2.",	typeof(FormatException),
													  "12...",		typeof(FormatException),
													  null,			typeof(ArgumentNullException),
												  };

	[Test]
	public void PublicFields ()
	{
		Assertion.AssertEquals ("Any", IPAddress.Any.Address, (long) 0);
		Assertion.AssertEquals ("Broadcast", IPAddress.Broadcast.Address, (long) 0xFFFFFFFF);
		long loopback = IPAddress.HostToNetworkOrder (BitConverter.IsLittleEndian ? 
							      0x7f000001 : 
							      0x0100007f);
		Assertion.AssertEquals ("Loopback", IPAddress.Loopback.Address, loopback);
		Assertion.AssertEquals ("None", IPAddress.None.Address, (long) 0xFFFFFFFF);
	}

	[Test]
	public void ToStringTest ()
	{
		IPAddress ip = IPAddress.Parse ("192.168.1.1");
		Assertion.AssertEquals ("ToString #1", "192.168.1.1", ip.ToString ());
		Assertion.AssertEquals ("ToString #2", "0.0.0.0", IPAddress.Any.ToString ());
		Assertion.AssertEquals ("ToString #3", "255.255.255.255", IPAddress.Broadcast.ToString ());
		Assertion.AssertEquals ("ToString #4", "127.0.0.1", IPAddress.Loopback.ToString ());
		Assertion.AssertEquals ("ToString #5", "255.255.255.255", IPAddress.None.ToString ());

#if NET_1_1
		for(int i=0; i<ipv6AddressList.Length/2; i++) {
			string addr = IPAddress.Parse (ipv6AddressList[i*2+1]).ToString().ToLower();
			Assertion.AssertEquals ("ToStringIPv6 #" + i, ipv6AddressList[i*2].ToLower(), addr);
		}
#endif
	}
	
	[Test]
	public void IsLoopback ()
	{
		IPAddress ip = IPAddress.Parse ("127.0.0.1");
		Assertion.AssertEquals ("IsLoopback #1", true, IPAddress.IsLoopback (ip));

		ip = IPAddress.Parse ("::101");
		Assertion.AssertEquals ("IsLoopback #2", false, IPAddress.IsLoopback (ip));

		ip = IPAddress.IPv6Loopback;
		Assertion.AssertEquals ("IsLoopback #3", true, IPAddress.IsLoopback (ip));

		ip = IPAddress.IPv6None;
		Assertion.AssertEquals ("IsLoopback #7", false, IPAddress.IsLoopback (ip));

		ip = IPAddress.Any;
		Assertion.AssertEquals ("IsLoopback #5", false, IPAddress.IsLoopback (ip));

		ip = IPAddress.Loopback;
		Assertion.AssertEquals ("IsLoopback #6", true, IPAddress.IsLoopback (ip));
	}

#if NET_1_1
	[Test]
	public void GetAddressBytes()
	{
		byte[] dataIn	= { 10, 11, 12, 13 };
		byte[] dataOut	= IPAddress.Parse ("10.11.12.13").GetAddressBytes ();
		for(int i=0; i<dataIn.Length; i++)
			Assertion.AssertEquals ("GetAddressBytes #1", dataIn[i], dataOut[i]);	
	
		dataIn	= new byte[]{ 0x01, 0x23, 0x45, 0x67, 0x89, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x89, 0x98 };
		dataOut	= IPAddress.Parse ("123:4567:8998:7654:3210:0123:4567:8998").GetAddressBytes ();
		for(int i=0; i<dataIn.Length; i++)
			Assertion.AssertEquals ("GetAddressBytes #2", dataIn[i], dataOut[i]);

		dataIn	= new byte[]{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x01 };
		dataOut	= IPAddress.Parse ("::FFFF:127.0.0.1").GetAddressBytes ();
		for(int i=0; i<dataIn.Length; i++)
			Assertion.AssertEquals ("GetAddressBytes #3", dataIn[i], dataOut[i]);
	}
#endif
	
	[Test]
	public void Address ()
	{
		// hm, lame, anything is accepted by ms.net
		/*
		try {
			IPAddress ip1 = new IPAddress (0x0000000100000000);
			Assertion.Fail ("#1");
		} catch (ArgumentOutOfRangeException) {}
		IPAddress ip = IPAddress.Parse ("127.0.0.1");
		ip.Address = 0;
		ip.Address = 0xffffffff;
		try {
			ip.Address = -1;
			Assertion.Fail ("#2");
		} catch (ArgumentOutOfRangeException) {}
		try {
			ip.Address = 0x0000000100000000;
			Assertion.Fail ("#3");
		} catch (ArgumentOutOfRangeException) {}
		*/
	}

	[Test]
	public void ParseOk ()
	{
		for(int i=0; i<ipv4ParseOk.Length / 2; i++) {
			IPAddress ip = IPAddress.Parse (ipv4ParseOk [i*2]);
			Assertion.Assert ("ParseIPv4 #" + i, ip.ToString () == ipv4ParseOk [i*2+1]);
		}

#if NET_1_1
		for(int i=0; i<ipv6AddressList.Length / 2; i++) 
		{
			string source = ipv6AddressList [i*2].ToLower();

			IPAddress ip = IPAddress.Parse (source);
			Assertion.Assert (string.Format("ParseIPv6 #{0}-1: {1} != {2}", i,
				ip.ToString ().ToLower (), source), ip.ToString ().ToLower () == source);

			ip = IPAddress.Parse (ipv6AddressList [i*2+1].ToLower ());
			Assertion.Assert (string.Format("ParseIPv6 #{0}-2: {1} != {2}", i,
				ip.ToString ().ToLower (), source), ip.ToString ().ToLower () == source);
		}
#endif
	}

	[Test]
	public void ParseWrong ()
	{
		for(int i=0; i<ipv4ParseWrong.Length/2; i++) {
			Type	exception	= ipv4ParseWrong[i*2+1] as Type;
			string	ipAddress	= ipv4ParseWrong[i*2] as string;

			try {
				IPAddress ip = IPAddress.Parse (ipAddress);
				Assertion.Fail ("IPv4: Should raise a " + exception + " #" + i);
			} 
			catch (Exception e)  {
				if(!e.GetType ().Equals (exception))
					Assertion.Fail ("ParseWrongIPv4 #" + i + ": " + e.ToString());
			}
		}
	}

	[Test]
	public void NetworkHost ()
	{
		long [] tested = new long [] { 0, 1, 1, 1};
		long [] expectedLE = new long [] {0, 256, 16777216, 72057594037927936 };
		long [] expected;
		
		expected = BitConverter.IsLittleEndian ? expectedLE : tested;

		short short0 = IPAddress.NetworkToHostOrder ((short) tested [0]);
		Assertion.AssertEquals ("NetworkToHostOrder #1", short0, (short) expected [0]);
		short0 = IPAddress.HostToNetworkOrder (short0);
		Assertion.AssertEquals ("HostToNetworkOrder #1", short0, (short) tested [0]);

		int int0 = IPAddress.NetworkToHostOrder ((int) tested [0]);
		Assertion.AssertEquals ("NetworkToHostOrder #2", int0, (int) expected [0]);
		int0 = IPAddress.HostToNetworkOrder (int0);
		Assertion.AssertEquals ("HostToNetworkOrder #2", int0, (int) tested [0]);
		
		long long0 = IPAddress.NetworkToHostOrder (tested [0]);
		Assertion.AssertEquals ("NetworkToHostOrder #3", long0, expected [0]);
		long0 = IPAddress.HostToNetworkOrder (long0);
		Assertion.AssertEquals ("HostToNetworkOrder #3", long0, tested [0]);

		short0 = IPAddress.NetworkToHostOrder ((short) tested [1]);
		Assertion.AssertEquals ("NetworkToHostOrder #4", short0, (short) expected [1]);
		short0 = IPAddress.HostToNetworkOrder (short0);
		Assertion.AssertEquals ("HostToNetworkOrder #4", short0, (short) tested [1]);
		
		int0 = IPAddress.NetworkToHostOrder ((int) tested [2]);
		Assertion.AssertEquals ("NetworkToHostOrder #5", int0, (int) expected [2]);
		int0 = IPAddress.HostToNetworkOrder (int0);
		Assertion.AssertEquals ("HostToNetworkOrder #5", int0, (int) tested [2]);
		
		long0 = IPAddress.NetworkToHostOrder (tested [3]);
		Assertion.AssertEquals ("NetworkToHostOrder #6", long0, expected [3]);
		long0 = IPAddress.HostToNetworkOrder (long0);
		Assertion.AssertEquals ("HostToNetworkOrder #6", long0, tested [3]);
	}
}
}

