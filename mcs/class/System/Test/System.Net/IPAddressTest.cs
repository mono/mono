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
	}
	
	[Test]
	public void IsLoopback ()
	{
		IPAddress ip = IPAddress.Parse ("127.0.0.1");
		Assertion.AssertEquals ("IsLoopback #1", true, IPAddress.IsLoopback (ip));
	}
	
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
		IPAddress ip = IPAddress.Parse ("192.168.1.1");
		Assertion.Assert ("Parse #1", ip.ToString () == "192.168.1.1");

		ip = IPAddress.Parse ("0xff.0x7f.0x20.0x01");
		Assertion.Assert ("Parse #1b", ip.ToString () == "255.127.32.1");

		ip = IPAddress.Parse ("0xff.0x7f.0x20.0xf");
		Assertion.Assert ("Parse #1c", ip.ToString () == "255.127.32.15");

		ip = IPAddress.Parse ("0.0.0.0");
		Assertion.AssertEquals ("Parse #2", ip, IPAddress.Any);

		ip = IPAddress.Parse ("255.255.255.255");
		Assertion.AssertEquals ("Parse #3", ip, IPAddress.Broadcast);
		Assertion.AssertEquals ("Parse #4", ip, IPAddress.None);

		ip = IPAddress.Parse ("127.0.0.1");
		Assertion.AssertEquals ("Parse #5", IPAddress.IsLoopback (ip), true);

		ip = IPAddress.Parse ("12.1.1.3 ");
		Assertion.AssertEquals ("Parse #6", IPAddress.Parse ("12.1.1.3"), ip);

		ip = IPAddress.Parse (" 12.1.1.1");
		Assertion.AssertEquals ("Parse #7", IPAddress.Parse ("0.0.0.0"), ip);

		ip = IPAddress.Parse ("12.1 .1.2");
		Assertion.AssertEquals ("Parse #8", IPAddress.Parse ("12.0.0.1"), ip);

		ip = IPAddress.Parse (".1.1.6");
		Assertion.AssertEquals ("Parse #9", IPAddress.Parse ("0.1.1.6"), ip);

		ip = IPAddress.Parse ("12.1.7");
		Assertion.AssertEquals ("Parse #10", IPAddress.Parse ("12.1.0.7"), ip);

		ip = IPAddress.Parse ("12.1.8. ");
		Assertion.AssertEquals ("Parse #11", IPAddress.Parse ("12.1.8.0"), ip);

		ip = IPAddress.Parse ("12");
		Assertion.AssertEquals ("Parse #12", IPAddress.Parse ("0.0.0.12"), ip);	

		ip = IPAddress.Parse ("12.1 foo.1.2.3.4.5.bar");
		Assertion.AssertEquals ("Parse #13", IPAddress.Parse ("12.0.0.1"), ip);			

		ip = IPAddress.Parse ("12.1.2. ");
		Assertion.AssertEquals ("Parse #14", IPAddress.Parse ("12.1.2.0"), ip);			

		ip = IPAddress.Parse ("12.. .");
		Assertion.AssertEquals ("Parse #15", IPAddress.Parse ("12.0.0.0"), ip);			

		ip = IPAddress.Parse (" ");
		Assertion.AssertEquals ("Parse #16", IPAddress.Parse ("0.0.0.0"), ip);			

		ip = IPAddress.Parse (" foo");
		Assertion.AssertEquals ("Parse #17", IPAddress.Parse ("0.0.0.0"), ip);			
	}

	[Test]
	public void ParseWrong ()
	{
		IPAddress ip = IPAddress.None;
		try {
			ip = IPAddress.Parse ("12.+1.1.4");
                        Assertion.Fail("Should raise a FormatException #1");
		} catch (FormatException) {
		} catch (Exception e) {
			Assertion.Fail ("ParseWrong #1:" + e.ToString());
		}

		try {
			ip = IPAddress.Parse ("12.1.-1.5");
                        Assertion.Fail("Should raise a FormatException #2");
		} catch (FormatException) {
		} catch (Exception e) {
			Assertion.Fail ("ParseWrong #2:" + e.ToString());
		}

		try {
			ip = IPAddress.Parse ("257.1.1.9");
                        Assertion.Fail("Should raise a FormatException #3");
		} catch (FormatException) {
		} catch (Exception e) {
			Assertion.Fail ("ParseWrong #3:" + e.ToString());
		}

		try {
			ip = IPAddress.Parse (null);
                        Assertion.Fail("Should raise a ArgumentNullException #1");
		} catch (ArgumentNullException) {
		} catch (Exception e) {
			Assertion.Fail ("ParseWrong #4:" + e.ToString());
		}

		try {
			ip = IPAddress.Parse ("12.");
			Assertion.Fail ("ParseWrong #5: Should raise a FormatException");
		} catch (FormatException) {}

		try {
			ip = IPAddress.Parse ("12.1.2.");
			Assertion.Fail ("ParseWrong #6: Should raise a FormatException");
		} catch (FormatException) {}		

		try {
			ip = IPAddress.Parse ("12...");
			Assertion.Fail ("ParseWrong #7: Should raise a FormatException");
		} catch (FormatException) {}		
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

