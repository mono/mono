//
// IPAddressTest.cs - NUnit Test Cases for System.Net.IPAddress
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) Ximian, Inc. http://www.ximian.com
// (C) 2003 Martin Willemoes Hansen
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace MonoTests.System.Net
{

[TestFixture]
public class IPAddressTest
{
	static string[] ipv6AddressList = new string[] {
		   "::", "0:0:0:0:0:0:0:0", 
		   "1::", "1:0:0:0:0:0:0:0",
		   "2:2::", "2:2:0:0:0:0:0:0",
		   "7:7:7:7:7:7:7:0", "7:7:7:7:7:7:7:0",
		   "::1", "0:0:0:0:0:0:0:1",
		   "::2", "0:0:0:0:0:0:0:2",
		   "::F", "0:0:0:0:0:0:0:F",
		   "::10", "0:0:0:0:0:0:0:10",
		   "::A0", "0:0:0:0:0:0:0:A0",
		   "::F0", "0:0:0:0:0:0:0:F0",
		   "::FF", "0:0:0:0:0:0:0:FF",
		   "::0.1.0.0", "0:0:0:0:0:0:1:0",
		   "::0.2.0.0", "0:0:0:0:0:0:2:0",
		   "::0.15.0.0", "0:0:0:0:0:0:F:0",
		   "::0.16.0.0", "0:0:0:0:0:0:10:0",
		   "::0.160.0.0", "0:0:0:0:0:0:A0:0",
		   "::0.240.0.0", "0:0:0:0:0:0:F0:0",
		   "::0.255.0.0", "0:0:0:0:0:0:FF:0",
		   "::1001", "0:0:0:0:0:0:0:1001",
		   "::1002", "0:0:0:0:0:0:0:1002",
		   "::100F", "0:0:0:0:0:0:0:100F",
		   "::1010", "0:0:0:0:0:0:0:1010",
		   "::10A0", "0:0:0:0:0:0:0:10A0",
		   "::10F0", "0:0:0:0:0:0:0:10F0",
		   "::10FF", "0:0:0:0:0:0:0:10FF",
		   "::0.1.0.1", "0:0:0:0:0:0:1:1",
		   "::0.2.0.2", "0:0:0:0:0:0:2:2",
		   "::0.15.0.15", "0:0:0:0:0:0:F:F",
		   "::0.16.0.16", "0:0:0:0:0:0:10:10",
		   "::0.160.0.160", "0:0:0:0:0:0:A0:A0",
		   "::0.240.0.240", "0:0:0:0:0:0:F0:F0",
		   "::0.255.0.255", "0:0:0:0:0:0:FF:FF",
		   "::FFFF:0:1", "0:0:0:0:0:FFFF:0:1",
		   "::FFFF:0:2", "0:0:0:0:0:FFFF:0:2",
		   "::FFFF:0:F", "0:0:0:0:0:FFFF:0:F",
		   "::FFFF:0:10", "0:0:0:0:0:FFFF:0:10",
		   "::FFFF:0:A0", "0:0:0:0:0:FFFF:0:A0",
		   "::FFFF:0:F0", "0:0:0:0:0:FFFF:0:F0",
		   "::FFFF:0:FF", "0:0:0:0:0:FFFF:0:FF",
		   "::FFFF:0.1.0.0", "0:0:0:0:0:FFFF:1:0",
		   "::FFFF:0.2.0.0", "0:0:0:0:0:FFFF:2:0",
		   "::FFFF:0.15.0.0", "0:0:0:0:0:FFFF:F:0",
		   "::FFFF:0.16.0.0", "0:0:0:0:0:FFFF:10:0",
		   "::FFFF:0.160.0.0", "0:0:0:0:0:FFFF:A0:0",
		   "::FFFF:0.240.0.0", "0:0:0:0:0:FFFF:F0:0",
		   "::FFFF:0.255.0.0", "0:0:0:0:0:FFFF:FF:0",
		   "::FFFF:0:1001", "0:0:0:0:0:FFFF:0:1001",
		   "::FFFF:0:1002", "0:0:0:0:0:FFFF:0:1002",
		   "::FFFF:0:100F", "0:0:0:0:0:FFFF:0:100F",
		   "::FFFF:0:1010", "0:0:0:0:0:FFFF:0:1010",
		   "::FFFF:0:10A0", "0:0:0:0:0:FFFF:0:10A0",
		   "::FFFF:0:10F0", "0:0:0:0:0:FFFF:0:10F0",
		   "::FFFF:0:10FF", "0:0:0:0:0:FFFF:0:10FF",
		   "::FFFF:0.1.0.1", "0:0:0:0:0:FFFF:1:1",
		   "::FFFF:0.2.0.2", "0:0:0:0:0:FFFF:2:2",
		   "::FFFF:0.15.0.15", "0:0:0:0:0:FFFF:F:F",
		   "::FFFF:0.16.0.16", "0:0:0:0:0:FFFF:10:10",
		   "::FFFF:0.160.0.160", "0:0:0:0:0:FFFF:A0:A0",
		   "::FFFF:0.240.0.240", "0:0:0:0:0:FFFF:F0:F0",
		   "::FFFF:0.255.0.255", "0:0:0:0:0:FFFF:FF:FF",
		   "0:7:7:7:7:7:7:7", "0:7:7:7:7:7:7:7",
		   "E::1", "E:0:0:0:0:0:0:1",
		   "E::2:2", "E:0:0:0:0:0:2:2",
		   "E:0:6:6:6:6:6:6", "E:0:6:6:6:6:6:6",
		   "E:E::1", "E:E:0:0:0:0:0:1",
		   "E:E::2:2", "E:E:0:0:0:0:2:2",
		   "E:E:0:5:5:5:5:5", "E:E:0:5:5:5:5:5",
		   "E:E:E::1", "E:E:E:0:0:0:0:1",
		   "E:E:E::2:2", "E:E:E:0:0:0:2:2",
		   "E:E:E:0:4:4:4:4", "E:E:E:0:4:4:4:4",
		   "E:E:E:E::1", "E:E:E:E:0:0:0:1",
		   "E:E:E:E::2:2", "E:E:E:E:0:0:2:2",
		   "E:E:E:E:0:3:3:3", "E:E:E:E:0:3:3:3",
		   "E:E:E:E:E::1", "E:E:E:E:E:0:0:1",
		   "E:E:E:E:E:0:2:2", "E:E:E:E:E:0:2:2",
		   "E:E:E:E:E:E:0:1", "E:E:E:E:E:E:0:1",
		   "::0.2.0.2", "0:0:0:0:0:0:2:2",
		   "::FFFF:192.168.0.1", "::FFFF:192.168.0.1",
		   "::FFFF:0.168.0.1", "::FFFF:0.168.0.1",
		   "::FFFF", "::0.0.255.255",
		   "::EEEE:A00:1", "::EEEE:10.0.0.1",
		   "::10.0.0.1", "::10.0.0.1",
		   "1234::1234:0:0", "1234:0:0:0:0:1234:0:0",
		   "1:0:1:0:1:0:1:0", "1:0:1:0:1:0:1:0",
		   "1:1:1::1:1:0", "1:1:1:0:0:1:1:0",
		   "::1234:0:0", "0:0:0:0:0:1234:0:0",
		   "3ffe:38e1::100:1:1", "3ffe:38e1::0100:1:0001",
		   "0:0:1:2::", "0:0:1:2:00:00:000:0000",
		   "100:0:1:2::abcd", "100:0:1:2:0:0:000:abcd",
		   "ffff::abcd", "ffff:0:0:0:0:0:00:abcd",
		   "ffff:0:0:2::abcd", "ffff:0:0:2:0:0:00:abcd",
		   "0:0:1:2::", "0:0:1:2:0:00:0000:0000",
		   "::1:0:0", "0000:0000::1:0000:0000",
		   "::111:234:5:6:789a:0", "0:0:111:234:5:6:789A:0",
		   "11:22:33:44:55:66:77:8", "11:22:33:44:55:66:77:8",
		   "0:0:7711:ab42:1230::", "::7711:ab42:1230:0:0:0",
		   "fec0:0:0:ffff::1%1",
	};

	static string[] ipv6ParseWrong = new string[] {
		   ":::4df",
		   "4df:::",
		   "0:::4df",
		   "4df:::0",
		   "::4df:::",
		   "0::4df:::",
		   " ::1",
		   ":: 1",
		   ":",
		   "0:0:0:0:0:0:0:0:0",
		   "0:0:0:0:0:0:0",
		   "0FFFF::",
		   "FFFF0::",
		   "[::1",
	};

	static string[] ipv4ParseOk = new string[] {
		"192.168.1.1", "192.168.1.1",
		"0xff.0x7f.0x20.0x01", "255.127.32.1",
		"0xff.0x7f.0x20.0xf", "255.127.32.15",
		"0.0.0.0", IPAddress.Any.ToString(),
		"255.255.255.255", IPAddress.Broadcast.ToString(),
		"12.1.7", "12.1.0.7",
		"12", "0.0.0.12",
		"65536", "0.1.0.0",
		"65535", "0.0.255.255",
		"20.65535", "20.0.255.255",
		"0313.027035210", "203.92.58.136", // bug #411920
		"0313.0134.035210", "203.92.58.136", // too
		"1434328179", "85.126.28.115", // too
		"3397943208", "202.136.127.168", // too
	};

	static string [] ipv4ParseWrong = new string [] {
		" foo",
		"12.. .",
		"12.1.2. ",
		"12.1.8. ",
		".1.1.6",
		" 12.1.1.1",
		"12.+1.1.4",
		"12.1.-1.5",
		"257.1.1.9",
		"255.1.1.256",
		"12.1.1.3 ",
		"12.1 foo.1.2.3.4.5.bar",
		"12.1 foo.1.2.3.4.5.",
		"12.1.1.3 g",
		" ",
		"",
		"12.1.foo.1.2.3.4.5.bar",
		"12.",
		"12.1.2.",
		"12...",
		"  ",
		"7848198702",
		"12.1.1.3 abc",
		"12.1 .1.2",
		"12.1 .zzzz.2",
		"12.5.3 foo.67.test.test.7FFFFFFFFFfaFFF789FFFFFFFFFFFFFFF",
		"12.1 foo.bar.test.test.baf",
		"12.1.4.6 foo.bar.test.test.baf",
		"12.3 foo.bar.test.test.4",
		"12 foo.bar.test.test.baf",
	};

	static byte [] ipv4MappedIPv6Prefix = new byte [] { 0,0, 0,0, 0,0, 0,0, 0,0, 0xFF,0xFF };

	[Test]
	public void PublicFields ()
	{
		Assert.AreEqual ((long) 0, IPAddress.Any.Address, "#1");
		Assert.AreEqual ((long) 0xFFFFFFFF, IPAddress.Broadcast.Address, "#2");
		long loopback = IPAddress.HostToNetworkOrder (BitConverter.IsLittleEndian ? 
								  0x7f000001 : 
								  0x0100007f);
		Assert.AreEqual (loopback, IPAddress.Loopback.Address, "#3");
		Assert.AreEqual ((long) 0xFFFFFFFF, IPAddress.None.Address, "#4");
	}

	[Test]
	public void ToStringV4 ()
	{
		IPAddress ip = IPAddress.Parse ("192.168.1.1");
		Assert.AreEqual ("192.168.1.1", ip.ToString (), "#1");
		Assert.AreEqual ("0.0.0.0", IPAddress.Any.ToString (), "#2");
		Assert.AreEqual ("255.255.255.255", IPAddress.Broadcast.ToString (), "#3");
		Assert.AreEqual ("127.0.0.1", IPAddress.Loopback.ToString (), "#4");
		Assert.AreEqual ("255.255.255.255", IPAddress.None.ToString (), "#5");
	}

	[Test]
	public void ToStringV6 ()
	{
		for(int i=0; i<ipv6AddressList.Length/2; i++) {
			string addr = IPAddress.Parse (ipv6AddressList[i*2+1]).ToString().ToLower();
			Assert.AreEqual (ipv6AddressList[i*2].ToLower(), addr, "ToStringIPv6 #" + i);
		}
	}

	[Test]
	public void IsLoopbackV4 ()
	{
		IPAddress ip;

		ip = IPAddress.Parse ("127.0.0.1");
		Assert.IsTrue (IPAddress.IsLoopback (ip), "#1");
		ip = IPAddress.Any;
		Assert.IsFalse (IPAddress.IsLoopback (ip), "#2");
		ip = IPAddress.Loopback;
		Assert.IsTrue (IPAddress.IsLoopback (ip), "#3");
		ip = IPAddress.Parse ("::101");
		Assert.IsFalse (IPAddress.IsLoopback (ip), "#4");
	}

	[Test]
	public void IsLoopbackV6 ()
	{
		IPAddress ip = IPAddress.IPv6Loopback;
		Assert.IsTrue (IPAddress.IsLoopback (ip), "#1");

		ip = IPAddress.IPv6None;
		Assert.IsFalse (IPAddress.IsLoopback (ip), "#2");
	}

	[Test]
	public void GetAddressBytesV4 ()
	{
		byte[] dataIn	= { 10, 11, 12, 13 };
		byte[] dataOut	= IPAddress.Parse ("10.11.12.13").GetAddressBytes ();
		for (int i = 0; i < dataIn.Length; i++)
			Assert.AreEqual (dataOut [i], dataIn [i], "GetAddressBytesV4");
	}

	[Test]
	public void GetAddressBytesV6 ()
	{
		byte[] dataIn	= new byte[]{ 0x01, 0x23, 0x45, 0x67, 0x89, 0x98, 0x76, 0x54, 0x32, 0x10, 0x01, 0x23, 0x45, 0x67, 0x89, 0x98 };
		byte[] dataOut	= IPAddress.Parse ("123:4567:8998:7654:3210:0123:4567:8998").GetAddressBytes ();
		for (int i = 0; i < dataIn.Length; i++)
			Assert.AreEqual (dataOut [i], dataIn [i], "GetAddressBytesV6 #1");

		dataIn	= new byte[]{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0x7F, 0x00, 0x00, 0x01 };
		dataOut	= IPAddress.Parse ("::FFFF:127.0.0.1").GetAddressBytes ();
		for (int i = 0; i < dataIn.Length; i++)
			Assert.AreEqual (dataOut [i], dataIn [i], "GetAddressBytesV6 #2");
	}

	[Test]
	public void Address ()
	{
		try {
			IPAddress ip1 = new IPAddress (0x0000000100000000);
			Assert.Fail ("#1");
		} catch (ArgumentOutOfRangeException) {}
		
		IPAddress ip = IPAddress.Parse ("127.0.0.1");
		ip.Address = 0;
		ip.Address = 0xffffffff;
		ip.Address = -1;
		ip.Address = 0x0000000100000000;
	}

	[Test]
	public void ParseOkV4 ()
	{
		for (int i = 0; i < ipv4ParseOk.Length / 2; i++) {
			IPAddress ip;
			try {
				ip = IPAddress.Parse (ipv4ParseOk [i*2]);
				Assert.AreEqual (ipv4ParseOk [i * 2 + 1], ip.ToString (), "ParseOkV4:" + i);
			} catch (FormatException) {
				Assert.Fail ("Cannot parse test i=" + i + ": '" + ipv4ParseOk [i*2] + "'");
			}
		}
	}

	[Test]
	public void ParseOkV6 ()
	{
		for (int i = 0; i < ipv6AddressList.Length / 2; i++) {
			string source = ipv6AddressList [i*2].ToLower();

			IPAddress ip = IPAddress.Parse (source);
			Assert.AreEqual (ip.ToString ().ToLower (), source,
				string.Format("ParseIPv6 #{0}-1: {1} != {2}", i,
					ip.ToString ().ToLower (), source));

			ip = IPAddress.Parse (ipv6AddressList [i*2+1].ToLower ());
			Assert.AreEqual (ip.ToString ().ToLower (), source,
				string.Format("ParseIPv6 #{0}-2: {1} != {2}", i,
					ip.ToString ().ToLower (), source));
		}
	}

	[Test]
	public void ParseWrongV4 ()
	{
		for (int i = 0; i < ipv4ParseWrong.Length; i++) {
			string ipAddress = ipv4ParseWrong [i];

			try {
				IPAddress ip = IPAddress.Parse (ipAddress);
				Assert.Fail ("#1:" + i + " (" + ipAddress + ")");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#2:" + i);
				Assert.IsNotNull (ex.InnerException, "#3:" + i);
				Assert.IsNotNull (ex.Message, "#4:" + i);
			}
		}
	}

	[Test]
	public void Parse_IpString_Null ()
	{
		try {
			IPAddress.Parse ((string) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("ipString", ex.ParamName, "#5");
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
		Assert.AreEqual ((short) expected [0], short0, "#A1");
		short0 = IPAddress.HostToNetworkOrder (short0);
		Assert.AreEqual ((short) tested [0], short0, "#A2");

		int int0 = IPAddress.NetworkToHostOrder ((int) tested [0]);
		Assert.AreEqual ((int) expected [0], int0, "#B1");
		int0 = IPAddress.HostToNetworkOrder (int0);
		Assert.AreEqual ((int) tested [0], int0, "#B2");
		
		long long0 = IPAddress.NetworkToHostOrder (tested [0]);
		Assert.AreEqual (expected [0], long0, "#C1");
		long0 = IPAddress.HostToNetworkOrder (long0);
		Assert.AreEqual (tested [0], long0, "#C2");

		short0 = IPAddress.NetworkToHostOrder ((short) tested [1]);
		Assert.AreEqual ((short) expected [1], short0, "#D1");
		short0 = IPAddress.HostToNetworkOrder (short0);
		Assert.AreEqual ((short) tested [1], short0, "#D2");
		
		int0 = IPAddress.NetworkToHostOrder ((int) tested [2]);
		Assert.AreEqual ((int) expected [2], int0, "#E1");
		int0 = IPAddress.HostToNetworkOrder (int0);
		Assert.AreEqual ((int) tested [2], int0, "#E2");
		
		long0 = IPAddress.NetworkToHostOrder (tested [3]);
		Assert.AreEqual (expected [3], long0, "#F1");
		long0 = IPAddress.HostToNetworkOrder (long0);
		Assert.AreEqual (tested [3], long0, "#F2");
	}

	[Test]
	public void LoopbackIPv6 ()
	{
		Assert.IsTrue (new Uri("http://[0:0:0:0::127.0.0.1]/").IsLoopback, "#1");
		Assert.IsFalse (new Uri ("http://[0:0:0:0::127.1.2.3]/").IsLoopback, "#2");
		Assert.IsTrue (new Uri ("http://[0:0:0:0::0.0.0.1]/").IsLoopback, "#3");
	}

	[Test] // bug #76792
	public void Constructor0_Address_4Byte ()
	{
		byte[] bytes = new byte[4] { 192, 202, 112, 37 };
		IPAddress i = new IPAddress (bytes);
		Assert.AreEqual (bytes, i.GetAddressBytes (), "#1");
		Assert.AreEqual ("192.202.112.37", i.ToString (), "#2");
	}

	[Test]
	public void Constructor0_Address_Invalid ()
	{
		try {
			new IPAddress (new byte [0]);
			Assert.Fail ("#A1");
		} catch (ArgumentException ex) {
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#A2");
			Assert.IsNull (ex.InnerException, "#A3");
			Assert.IsNotNull (ex.Message, "#A4");
			Assert.IsFalse (ex.Message == "address", ex.Message, "#A5");
			Assert.IsNotNull (ex.ParamName, "#A6");
			Assert.AreEqual ("address", ex.ParamName, "#A7");
		}

		try {
			new IPAddress (new byte [3] { 192, 202, 112 });
			Assert.Fail ("#B1");
		} catch (ArgumentException ex) {
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#B2");
			Assert.IsNull (ex.InnerException, "#B3");
			Assert.IsNotNull (ex.Message, "#B4");
			Assert.IsFalse (ex.Message == "address", ex.Message, "#B5");
			Assert.IsNotNull (ex.ParamName, "#B6");
			Assert.AreEqual ("address", ex.ParamName, "#B7");
		}

		try {
			new IPAddress (new byte [5] { 192, 202, 112, 142, 25 });
			Assert.Fail ("#C1");
		} catch (ArgumentException ex) {
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#C2");
			Assert.IsNull (ex.InnerException, "#C3");
			Assert.IsNotNull (ex.Message, "#C4");
			Assert.IsFalse (ex.Message == "address", "#C5");
			Assert.IsNotNull (ex.ParamName, "#C6");
			Assert.AreEqual ("address", ex.ParamName, "#C7");
		}
	}

	[Test]
	public void Constructor0_Address_Null ()
	{
		try {
			new IPAddress ((byte []) null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("address", ex.ParamName, "#5");
		}
	}

	[Test]
	public void Constructor1_Address_4Byte ()
	{
		byte [] bytes = new byte [4] { 192, 202, 112, 37 };
		try {
			new IPAddress (bytes, 0);
			Assert.Fail ("#1");
		} catch (ArgumentException ex) {
			Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.IsFalse (ex.Message == "address", "#5");
			Assert.IsNotNull (ex.ParamName, "#6");
			Assert.AreEqual ("address", ex.ParamName, "#7");
		}
	}

	[Test]
	public void Constructor1_Address_Null ()
	{
		try {
			new IPAddress ((byte []) null, 5);
			Assert.Fail ("#1");
		} catch (ArgumentNullException ex) {
			Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
			Assert.IsNull (ex.InnerException, "#3");
			Assert.IsNotNull (ex.Message, "#4");
			Assert.AreEqual ("address", ex.ParamName, "#5");
		}
	}

	[Test]
	public void FromBytes1 ()
	{
		byte[] val1 = new byte[4];
		val1[0] = 82;
		val1[1] = 165;
		val1[2] = 240;
		val1[3] = 134;
		CompareIPs (val1, "82.165.240.134");

		byte[] val2 = new byte[4];
		val2[0] = 123;
		val2[1] = 124;
		val2[2] = 125;
		val2[3] = 126;
		CompareIPs (val2, "123.124.125.126");
	}

	void CompareIPs (byte [] bytes, string address)
	{
		IPAddress ip = new IPAddress (bytes);
		IPAddress ip2 = IPAddress.Parse (address);
		Assert.IsTrue (ip2.Equals (ip), "#A-" + address);
		Assert.IsTrue (ip.Equals (ip2), "#B-" + address);
	}

	[Test]
	public void TryParse_IpString_Null ()
	{
		IPAddress i;
		
		bool val1 = IPAddress.TryParse ((string) null, out i);
		
		Assert.IsFalse (val1, "#1");
		Assert.IsNull (i, "#2");
	}

	[Test]
	public void TryParse ()
	{
		IPAddress i;
		Assert.IsTrue (IPAddress.TryParse ("0.0.0.0", out i), "#1");
		Assert.IsTrue (IPAddress.TryParse ("127.0.0.1", out i), "#2");
		Assert.IsFalse (IPAddress.TryParse ("www.example.com", out i), "#3");
		Assert.IsTrue (IPAddress.TryParse ("0001:0002:0003:0004:0005:0006:0007:0008", out i), "#4");
		Assert.IsTrue (IPAddress.TryParse ("1:2:3:4:5:6:7:8", out i), "#5");
		Assert.IsTrue (IPAddress.TryParse ("1::8", out i), "#6");
		Assert.IsTrue (IPAddress.TryParse ("1::3:4:5:6:7:8", out i), "#7");
		Assert.IsFalse (IPAddress.TryParse ("1::2:3:4:5::6:7:8", out i), "#8"); // :: ::
		Assert.IsFalse (IPAddress.TryParse ("1::2:3:4:5:6:7:8", out i), "#9");
		Assert.IsFalse (IPAddress.TryParse ("1;2:3:4:5:6:7:8", out i), "#10"); // ;
		// FIXME:
		// Assert.IsFalse (IPAddress.TryParse ("1:2:3:4:5:6:7:8/10", out i), "#11"); // subnet
	}

	[Test]
	public void TryParseOkV4 ()
	{
		for (int i = 0; i < ipv4ParseOk.Length / 2; i++) {
			IPAddress ip;
			Assert.IsTrue (IPAddress.TryParse (ipv4ParseOk [i * 2], out ip), "#1:" + i);
			Assert.AreEqual (ipv4ParseOk [i * 2 + 1], ip.ToString (), "#2:" + i);
		}
	}

	[Test]
	public void TryParseWrongV4 ()
	{
		for (int i = 0; i < ipv4ParseWrong.Length; i++) {
			IPAddress ip;
			string ipAddress = ipv4ParseWrong [i];

			Assert.IsFalse (IPAddress.TryParse (ipAddress, out ip), "#1:" + i);
			Assert.IsNull (ip, "#2:" + i);
		}
	}

	[Test]
	public void IsIPv6LinkLocal ()
	{
		Assert.IsTrue (IPAddress.Parse ("FE80::1").IsIPv6LinkLocal, "#1");
		Assert.IsTrue (IPAddress.Parse ("FE81::1").IsIPv6LinkLocal, "#2");
		Assert.IsFalse (IPAddress.Parse ("FD81::1").IsIPv6LinkLocal, "#3");
		Assert.IsFalse (IPAddress.Parse ("FF80::1").IsIPv6LinkLocal, "#4");
		Assert.IsTrue (IPAddress.Parse ("FE91::1").IsIPv6LinkLocal, "#5");
		Assert.IsTrue (IPAddress.Parse ("FEA0::1").IsIPv6LinkLocal, "#6");
		Assert.IsFalse (IPAddress.Parse ("FEC0::1").IsIPv6LinkLocal, "#7");
	}

	[Test]
	public void IsIPv6SiteLocal ()
	{
		Assert.IsTrue (IPAddress.Parse ("FEC0::1").IsIPv6SiteLocal, "#1");
		Assert.IsTrue (IPAddress.Parse ("FEC1::1").IsIPv6SiteLocal, "#2");
		Assert.IsFalse (IPAddress.Parse ("FE81::1").IsIPv6SiteLocal, "#3");
		Assert.IsFalse (IPAddress.Parse ("FFC0::1").IsIPv6SiteLocal, "#4");
		Assert.IsTrue (IPAddress.Parse ("FED1::1").IsIPv6SiteLocal, "#5");
		Assert.IsTrue (IPAddress.Parse ("FEE1::1").IsIPv6SiteLocal, "#6");
	}

	[Test]
	public void IsIPv6Multicast ()
	{
		Assert.IsTrue (IPAddress.Parse ("FF00::1").IsIPv6Multicast, "#1");
		Assert.IsTrue (IPAddress.Parse ("FF01::1").IsIPv6Multicast, "#2");
		Assert.IsFalse (IPAddress.Parse ("FE00::1").IsIPv6Multicast, "#3");
	}

	[Test]
	public void IsIPv6Teredo ()
	{
		Assert.IsTrue (IPAddress.Parse ("2001::1").IsIPv6Teredo, "#1");
		Assert.IsFalse (IPAddress.Parse ("2002::1").IsIPv6Teredo, "#2");
	}

	[Test]
	public void ParseWrongV6 ()
	{
		for (int i = 0; i < ipv6ParseWrong.Length; i++) {
			string ipAddress = ipv6ParseWrong [i];

			try {
				IPAddress ip = IPAddress.Parse (ipAddress);
				Assert.Fail ("#1:" + i + " (" + ipAddress + ")");
			} catch (FormatException ex) {
				Assert.AreEqual (typeof (FormatException), ex.GetType (), "#2:" + i);
				Assert.AreEqual(typeof(SocketException), ex.InnerException.GetType (), "#3:" + i);
				Assert.IsNotNull (ex.Message, "#4:" + i);
			}
		}
	}

	[Test]
	public void MapToIPv6 ()
	{
		for (int i = 0; i < ipv4ParseOk.Length / 2; i++) {
			IPAddress v4 = IPAddress.Parse (ipv4ParseOk [i * 2]);
			byte [] v4bytes = v4.GetAddressBytes ();
			IPAddress v6 = v4.MapToIPv6 ();
			byte [] v6bytes = v6.GetAddressBytes ();
			IPAddress v4back = v6.MapToIPv4 ();

			Assert.IsTrue (StartsWith (v6bytes, ipv4MappedIPv6Prefix), "MapToIPv6 #" + i + ".1");
			Assert.IsTrue (v6bytes [12] == v4bytes [0], "MapToIPv6 #" + i + ".2");
			Assert.IsTrue (v6bytes [13] == v4bytes [1], "MapToIPv6 #" + i + ".3");
			Assert.IsTrue (v6bytes [14] == v4bytes [2], "MapToIPv6 #" + i + ".4");
			Assert.IsTrue (v6bytes [15] == v4bytes [3], "MapToIPv6 #" + i + ".5");
			Assert.IsTrue (v4.Equals (v4back), "MapToIPv4 #" + i);
		}

		//TODO: Test using MapToIPv4/6 with anything other than IPv4/6 addresses.
		//Currently it is not possible to do with the IPAddress implementation.
	}

	static bool StartsWith (byte [] a, byte [] b)
	{
		if (a.Length < b.Length)
			return false;
		for (int i = 0; i < b.Length; i++)
		{
			if (a [i] != b [i])
				return false;
		}
		return true;
	}

	[Test]
	public void EqualsFromBytes ()
	{
		for (int i = 0; i < ipv4ParseOk.Length / 2; i++) {
			IPAddress ip = IPAddress.Parse (ipv4ParseOk [i * 2]);
			IPAddress ipFromBytes = new IPAddress (ip.GetAddressBytes ());
			Assert.IsTrue (ip.Equals (ipFromBytes), "EqualsFromBytes #" + i);
		}

	}
}
}

