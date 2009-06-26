//
// IPEndPointTest.cs - NUnit Test Cases for System.Net.IPEndPoint
//
// Authors:
//   Lawrence Pit (loz@cable.a2000.nl)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using NUnit.Framework;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace MonoTests.System.Net
{

[TestFixture]
public class IPEndPointTest
{
        private const int MyPort = 42;
        private const int MyMaxPort = 65535;
        private const int MyMinPort = 0;
        private const string MyIPAddressString = "192.168.1.1";

        private IPAddress ipAddress;
        private long ip;
        private IPEndPoint endPoint1;
        private IPEndPoint endPoint2;

	[SetUp]
	public void GetReady()
        {
                ipAddress = IPAddress.Parse (MyIPAddressString);
                ip = ipAddress.Address;
                endPoint1 = new IPEndPoint (ipAddress, MyPort);
                endPoint2 = new IPEndPoint (ip, MyPort);
        }

	[Test]
        public void PublicFields ()
        {
                Assert.AreEqual (IPEndPoint.MinPort, MyMinPort, "MinPort");
                Assert.AreEqual (IPEndPoint.MaxPort, MyMaxPort, "MaxPort");
        }

	[Test]
        public void Constructors ()
        {
                try {
                        new IPEndPoint (null, 0);
                        Assert.Fail ("Should raise an ArgumentNullException");
                } catch (ArgumentNullException) {
                }
                try {
                        new IPEndPoint (ipAddress, MyMinPort - 1);
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #1");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        new IPEndPoint (ipAddress, MyMaxPort + 1);
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #2");
                } catch (ArgumentOutOfRangeException) {
                }

                try {
                        new IPEndPoint (ip, MyMinPort -1);
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #3");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        new IPEndPoint (ip, MyMaxPort + 1);
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #4");
                } catch (ArgumentOutOfRangeException) {
                }
        }

	[Test]
        public void PortProperty ()
        {
                try {
                        endPoint1.Port = MyMinPort - 1;
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #1");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        endPoint1.Port = MyMaxPort + 1;
                        Assert.Fail ("Should raise an ArgumentOutOfRangeException #2");
                } catch (ArgumentOutOfRangeException) {
                }
        }

	[Test]
        public void CreateAndSerialize()
        {
		SocketAddress addr = endPoint1.Serialize ();
		EndPoint endPoint3 = endPoint2.Create (addr);
		Assert.IsTrue (endPoint1.Equals (endPoint3), "#1");

		IPAddress ipAddress = IPAddress.Parse ("255.255.255.255");
                IPEndPoint endPoint4 = new IPEndPoint (ipAddress, MyMaxPort);
		addr = endPoint4.Serialize ();
		EndPoint endPoint5 = endPoint2.Create(addr);
		Assert.IsTrue (endPoint4.Equals (endPoint5), "#2");
		Assert.AreEqual (endPoint5.ToString (), "255.255.255.255:" + MyMaxPort, "#3");
	}

	[Test]
        public void Equals ()
        {
                Assert.IsTrue (endPoint1.Equals (endPoint2), "Equals");
                Assert.IsTrue (!endPoint1.Equals (new IPEndPoint (ip, MyPort + 1)), "Not Equals");
        }

	[Test]
        public void GetHashCodeTest ()
        {
                Assert.AreEqual (endPoint1.GetHashCode(), endPoint2.GetHashCode());
        }

	[Test]
        public void ToStringTest ()
        {
                Assert.AreEqual (endPoint1.ToString (), MyIPAddressString + ":" + MyPort, "ToString #1");
                Assert.AreEqual (endPoint2.ToString (), MyIPAddressString + ":" + MyPort, "ToString #2");
        }

}

}

