//
// IPEndPointTest.cs - NUnit Test Cases for System.Net.IPEndPoint
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using NUnit.Framework;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace MonoTests.System.Net
{

public class IPEndPointTest : TestCase
{
        private const int MyPort = 42;
        private const int MyMaxPort = 65535;
        private const int MyMinPort = 0;
        private const string MyIPAddressString = "192.168.1.1";

        private IPAddress ipAddress;
        private long ip;
        private IPEndPoint endPoint1;
        private IPEndPoint endPoint2;

        public IPEndPointTest () :
                base ("[MonoTests.System.Net.IPEndPointTest]") {}

        public IPEndPointTest (string name) : base (name) {}

        protected override void SetUp ()
        {
                ipAddress = IPAddress.Parse (MyIPAddressString);
                ip = ipAddress.Address;
                endPoint1 = new IPEndPoint (ipAddress, MyPort);
                endPoint2 = new IPEndPoint (ip, MyPort);
        }

        protected override void TearDown () {}

        public static ITest Suite
        {
                get {
                        return new TestSuite (typeof (IPEndPointTest));
                }
        }

        public void TestPublicFields ()
        {
                AssertEquals ("MinPort", IPEndPoint.MinPort, MyMinPort);
                AssertEquals ("MaxPort", IPEndPoint.MaxPort, MyMaxPort);
        }

        public void TestConstructors ()
        {
                try {
                        new IPEndPoint (null, 0);
                        Fail ("Should raise an ArgumentNullException");
                } catch (ArgumentNullException) {
                }
                try {
                        new IPEndPoint (ipAddress, MyMinPort - 1);
                        Fail ("Should raise an ArgumentOutOfRangeException #1");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        new IPEndPoint (ipAddress, MyMaxPort + 1);
                        Fail ("Should raise an ArgumentOutOfRangeException #2");
                } catch (ArgumentOutOfRangeException) {
                }

                try {
                        new IPEndPoint (ip, MyMinPort -1);
                        Fail ("Should raise an ArgumentOutOfRangeException #3");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        new IPEndPoint (ip, MyMaxPort + 1);
                        Fail ("Should raise an ArgumentOutOfRangeException #4");
                } catch (ArgumentOutOfRangeException) {
                }
        }

        public void TestPortProperty ()
        {
                try {
                        endPoint1.Port = MyMinPort - 1;
                        Fail ("Should raise an ArgumentOutOfRangeException #1");
                } catch (ArgumentOutOfRangeException) {
                }
                try {
                        endPoint1.Port = MyMaxPort + 1;
                        Fail ("Should raise an ArgumentOutOfRangeException #2");
                } catch (ArgumentOutOfRangeException) {
                }
        }

        public void TestCreateAndSerialize()
        {
		SocketAddress addr = endPoint1.Serialize ();
		EndPoint endPoint3 = endPoint2.Create (addr);
		Assert ("#1", endPoint1.Equals (endPoint3));

		IPAddress ipAddress = IPAddress.Parse ("255.255.255.255");
                IPEndPoint endPoint4 = new IPEndPoint (ipAddress, MyMaxPort);
		addr = endPoint4.Serialize ();
		EndPoint endPoint5 = endPoint2.Create(addr);
		Assert ("#2", endPoint4.Equals (endPoint5));
		AssertEquals ("#3", endPoint5.ToString (), "255.255.255.255:" + MyMaxPort);
	}

        public void TestEquals ()
        {
                Assert("Equals", endPoint1.Equals (endPoint2));
                Assert("Not Equals", !endPoint1.Equals (new IPEndPoint (ip, MyPort + 1)));
        }

        public void TestGetHashCode ()
        {
                AssertEquals(endPoint1.GetHashCode(), endPoint2.GetHashCode());
        }

        public void TestToString ()
        {
                AssertEquals("ToString #1", endPoint1.ToString (), MyIPAddressString + ":" + MyPort);
                AssertEquals("ToString #2", endPoint2.ToString (), MyIPAddressString + ":" + MyPort);
        }

}

}

