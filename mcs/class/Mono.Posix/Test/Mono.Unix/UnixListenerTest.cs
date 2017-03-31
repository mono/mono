// UnixListenerTest.cs: Unit tests for Mono.Unix.UnixListener
//
// Authors:
//  David Lechner (david@lechnology.com)
//
// (c) 2015 David Lechner
//

using System;
using System.IO;

using NUnit.Framework;
using Mono.Unix;

namespace MonoTests.Mono.Unix {

    [TestFixture, Category ("NotOnWindows")]
    public class UnixListenerTest {

        // test that a socket file is created and deleted by the UnixListener
        [Test]
        public void TestSocketFileCreateDelete ()
        {
            var socketFile = Path.GetTempFileName ();
            // we just want the file name, not the file
            File.Delete (socketFile);

            using (var listener = new UnixListener (socketFile)) {
                // creating an instance of UnixListener should create the file
                Assert.IsTrue (File.Exists (socketFile), "#A01");
            }
            // and disposing the UnixListener should delete the file
            Assert.IsFalse (File.Exists (socketFile), "#A02");
        }
    }
}
