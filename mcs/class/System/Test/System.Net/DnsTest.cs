// DnsTest.cs - NUnit Test Cases for the System.Net.Dns class
//
// Authors: 
//   Mads Pultz (mpultz@diku.dk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2001 Mads Pultz
// (C) 2003 Martin Willemoes Hansen
// 
// This test assumes the following:
// 1) The following Internet sites exist:
//        www.go-mono.com with IP address 129.250.184.233
//        info.diku.dk with IP address 130.225.96.4
// 2) The following DNS name does not exist:
//        www.hopefullydoesnotexist.dk
//

using NUnit.Framework;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;

namespace MonoTests.System.Net {

[TestFixture]
public class DnsTest {
        
        private String site1Name = "www.go-mono.com",
                site1Dot = "129.250.184.233",
                site2Name = "info.diku.dk",
                site2Dot = "130.225.96.4",
                noneExistingSite = "www.hopefullydoesnotexist.dk";
        private uint site1IP = 2180692201, site2IP = 2195808260; // Big-Endian
        
        private void Callback(IAsyncResult ar) { 
                IPHostEntry h;
                h = Dns.EndGetHostByName(ar);
                SubTestValidIPHostEntry(h);
        }

	[Test]
        public void AsyncGetHostByName(){
                IAsyncResult r;
                r = Dns.BeginGetHostByName(site1Name, new AsyncCallback(Callback), null);
                
		IAsyncResult async = Dns.BeginGetHostByName (site1Name, null, null);
		IPHostEntry entry = Dns.EndGetHostByName (async);                
		SubTestValidIPHostEntry(entry);
		Assertion.AssertEquals ("#1", "www.go-mono.com", entry.HostName);
        }
        
	[Test]
        public void AsyncResolve() {
                IAsyncResult r;
                r = Dns.BeginResolve(site1Name, new AsyncCallback(Callback), null);

		IAsyncResult async = Dns.BeginResolve (site1Dot, null, null);
		IPHostEntry entry = Dns.EndResolve (async);                
		SubTestValidIPHostEntry(entry);
		Assertion.AssertEquals ("#1", "129.250.184.233", entry.HostName);
        }
        
	[Test]
        public void GetHostName() {
                string hostName = Dns.GetHostName();
                Assertion.Assert(hostName != null);
        }
        
        private void SubTestGetHostByName(string siteName, string siteDot) {
                IPHostEntry h = Dns.GetHostByName(siteName);
                SubTestValidIPHostEntry(h);
                Assertion.Assert(h.HostName.Equals(siteName));
                Assertion.Assert(h.AddressList[0].ToString() == siteDot);
        }
        
	[Test]
        public void GetHostByName() {
                SubTestGetHostByName(site1Name, site1Dot);
                SubTestGetHostByName(site2Name, site2Dot);
                try {
                        Dns.GetHostByName(noneExistingSite);
                        Assertion.Fail("Should raise a SocketException (assuming that '" + noneExistingSite + "' does not exist)");
                } catch (SocketException) {
                } 
                try {
                        Dns.GetHostByName(null);
                        Assertion.Fail("Should raise an ArgumentNullException");
                } catch (ArgumentNullException) {
                } 
        }
        
        private void SubTestGetHostByAddressStringFormatException(string addr) {
                try {
                        Dns.GetHostByAddress(addr);
                        Assertion.Fail("Should raise a FormatException");
                } catch (FormatException) {
                } 
        }
        
        private void SubTestGetHostByAddressString(string addr) {
                IPHostEntry h = Dns.GetHostByAddress(addr);
                SubTestValidIPHostEntry(h);
        }
        
	[Test]
        public void GetHostByAddressString() {
                try {
                        String addr = null;
                        Dns.GetHostByAddress(addr);
                        Assertion.Fail("Should raise an ArgumentNullException");
                } catch (ArgumentNullException) {
                }
                SubTestGetHostByAddressStringFormatException("123.255.23");
                SubTestGetHostByAddressStringFormatException("123.256.34.10");
                SubTestGetHostByAddressStringFormatException("not an IP address");
                SubTestGetHostByAddressString(site1Dot);
                SubTestGetHostByAddressString(site2Dot);
        }
        
        private void SubTestGetHostByAddressIPAddress(IPAddress addr) {
                IPHostEntry h = Dns.GetHostByAddress(addr);
                SubTestValidIPHostEntry(h);
                Assertion.Assert(h.AddressList[0].ToString() == addr.ToString());
        }
        
	[Test]
        public void GetHostByAddressIPAddress() {
                try {
                        IPAddress addr = null;
                        Dns.GetHostByAddress(addr);
                        Assertion.Fail("Should raise an ArgumentNullException");
                } catch (ArgumentNullException) {
                }
                SubTestGetHostByAddressIPAddress(new IPAddress(IPAddress.NetworkToHostOrder((int)site1IP)));
                SubTestGetHostByAddressIPAddress(new IPAddress(IPAddress.NetworkToHostOrder((int)site2IP)));
        }
        
        private void SubTestResolve(string addr) {
                IPHostEntry h = Dns.Resolve(addr);
                SubTestValidIPHostEntry(h);
        }
        
	[Test]
        public void Resolve() {
                SubTestResolve(site1Name);
                SubTestResolve(site2Name);
                SubTestResolve(site1Dot);
                SubTestResolve(site2Dot);
        }
        
        private void SubTestValidIPHostEntry(IPHostEntry h) {
                Assertion.Assert(h.HostName != null);
                Assertion.Assert(h.AddressList != null);
                Assertion.Assert(h.AddressList.Length > 0);
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

}
