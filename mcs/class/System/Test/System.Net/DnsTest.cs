// DnsTest.cs - NUnit Test Cases for the System.Net.Dns class
//
// Author: Mads Pultz (mpultz@diku.dk)
//
// (C) Mads Pultz, 2001

using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;

public class DnsTest: TestCase {
	
	private String site1Name = "www.go-mono.com",
		site1Dot = "129.250.184.233",
		site2Name = "info.diku.dk",
		site2Dot = "130.225.96.4",
		noneExistingSite = "www.hopefullydoesnotexist.dk";
	private uint site1IP = 2180692201, site2IP = 2195808260;
	
	public DnsTest(String name): base(name) {
		Console.WriteLine("Assuming the following sites exist for this test suite:");
		Console.WriteLine("'{0}' with IP address '{1}' ({2})", site1Name, site1Dot, site1IP);
		Console.WriteLine("'{0}' with IP address '{1}' ({2})", site2Name, site2Dot, site2IP);
		Console.WriteLine("Assuming that '{0}' does not exist", noneExistingSite);
	}

	public static ITest Suite {
		get { return new TestSuite(typeof(DnsTest)); }
	}
	
	public void TestGetHostName() {
	}
	
	private void SubTestGetHostByName(string siteName, string siteDot) {
		IPHostEntry h = System.Net.Dns.GetHostByName(siteName);
		Assert(h.HostName.Equals(siteName));
		Assert(h.AddressList.Length > 0);
		Assert(h.AddressList[0].ToString() == siteDot);
		printIPHostEntry(h);
	}
	
	public void TestGetHostByName() {
		SubTestGetHostByName(site1Name, site1Dot);
		SubTestGetHostByName(site2Name, site2Dot);
		try {
			System.Net.Dns.GetHostByName(noneExistingSite);
			Fail("Should raise a SocketException (assuming that '" + noneExistingSite + "' does not exist)");
		} catch (SocketException) {
		} 
		try {
			System.Net.Dns.GetHostByName(null);
			Fail("Should raise an ArgumentNullException");
		} catch (ArgumentNullException) {
		} 
	}
	
	private void SubTestGetHostByAddressStringFormatException(string addr) {
		try {
			System.Net.Dns.GetHostByAddress(addr);
			Fail("Should raise a FormatException");
		} catch (FormatException) {
		} 
	}
	
	private void SubTestGetHostByAddressString(string addr) {
		IPHostEntry h = System.Net.Dns.GetHostByAddress(addr);
		Assert(h.HostName != null);
		Assert(h.AddressList.Length > 0);
		printIPHostEntry(h);
	}
	
	public void TestGetHostByAddressString() {
		try {
			String addr = null;
			System.Net.Dns.GetHostByAddress(addr);
			Fail("Should raise an ArgumentNullException");
		} catch (ArgumentNullException) {
		}
		SubTestGetHostByAddressStringFormatException("123.255.23");
		SubTestGetHostByAddressStringFormatException("123.256.34.10");
		SubTestGetHostByAddressStringFormatException("not an IP address");
		SubTestGetHostByAddressString(site1Dot);
		SubTestGetHostByAddressString(site2Dot);
	}
	
	private void SubTestGetHostByAddressIPAddress(IPAddress addr) {
		IPHostEntry h = System.Net.Dns.GetHostByAddress(addr);
		Assert(h.HostName != null);
		Assert(h.AddressList.Length > 0);
		Assert(h.AddressList[0] == addr);
		printIPHostEntry(h);
	}
	
	public void TestGetHostByAddressIPAddress() {
		try {
			IPAddress addr = null;
			System.Net.Dns.GetHostByAddress(addr);
			Fail("Should raise an ArgumentNullException");
		} catch (ArgumentNullException) {
		}
		SubTestGetHostByAddressIPAddress(new IPAddress(site1IP));
		SubTestGetHostByAddressIPAddress(new IPAddress(site2IP));
	}
	
	private void SubTestIpToString(int IpAddr) {
		String addr = System.Net.Dns.IpToString(IpAddr);
		Assert(addr != null);
	}
	
	public void TestIpToString() {
		SubTestIpToString((int)site1IP);
		SubTestIpToString((int)site2IP);
	}
	
	private void SubTestResolve(string addr) {
		IPHostEntry h = System.Net.Dns.Resolve(addr);
		Assert(h.HostName != null);
		Assert(h.AddressList.Length > 0);
		printIPHostEntry(h);
	}
	
	public void TestResolve() {
		SubTestResolve(site1Name);
		SubTestResolve(site2Name);
		SubTestResolve(site1Dot);
		SubTestResolve(site2Dot);
	}
	
	private static void printIPHostEntry(IPHostEntry h)
	{
	  Console.WriteLine("----------------------------------------------------");
	  Console.WriteLine("Host name:");
	  Console.WriteLine(h.HostName);
	  Console.WriteLine("IP addresses:");
	  IPAddress[] list = h.AddressList;
	  for(int i = 0; i < list.Length; ++i)
		Console.WriteLine(list[i]);
	  Console.WriteLine("Aliases:");
	  string[] aliases = h.Aliases;
	  for(int i = 0; i < aliases.Length; ++i)
		Console.WriteLine(aliases[i]);
	  Console.WriteLine("----------------------------------------------------");
	}
}

