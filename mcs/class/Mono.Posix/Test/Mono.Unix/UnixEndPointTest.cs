// UnixEndPointTest.cs: Unit tests for Mono.Unix.UnixListener
//
// Authors:
//  David Lechner (david@lechnology.com)
//
// (c) 2015 David Lechner
//

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

using NUnit.Framework;
using Mono.Unix;

namespace MonoTests.Mono.Unix {

    [TestFixture, Category ("NotOnWindows")]
    public class UnixEndPointTest {

        // Regression test for https://bugzilla.xamarin.com/show_bug.cgi?id=35004
        [Test]
        public void TestCreate ()
        {
            const string socketFile = "test";
            // mangledSocketFile simulates the socket file name with a null
            // terminator and junk after the null terminator. This can be present
            // in a SocketAddress when marshaled from native code.
            const string mangledSocketFile = socketFile + "\0junk";

            var bytes = Encoding.Default.GetBytes (mangledSocketFile);
            var socketAddress = new SocketAddress (AddressFamily.Unix, bytes.Length + 2);
            for (int i = 0; i < bytes.Length; i++) {
                socketAddress [i + 2] = bytes [i];
            }
            var dummyEndPoint = new UnixEndPoint (socketFile);

            // testing that the Create() method strips off the null terminator and the junk
            var endPoint = (UnixEndPoint)dummyEndPoint.Create (socketAddress);
            Assert.AreEqual (socketFile, endPoint.Filename, "#A01");
        }
    }
}
