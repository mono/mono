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
// Copyright (c) 2008 Novell, Inc. (http://www.novell.com)
//
// Authors:
//    Alan McGovern (amcgovern@novell.com)
//

using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;

namespace System.IO.Packaging.Tests {

    [TestFixture]
    public class FakePackagePartTests : TestBase {

        static void Main (string [] args)
        {
            FakePackagePartTests t = new FakePackagePartTests ();
            t.FixtureSetup ();
            t.Setup ();
            t.GetStream2 ();
        }

        FakePackagePart part;
        public override void Setup ()
        {
            base.Setup ();
            part = (FakePackagePart) new FakePackagePart(package, uris [0], contentType);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Constructor1 ()
        {
            FakePackagePart p = new FakePackagePart (null, null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Constructor2 ()
        {
            FakePackagePart p = new FakePackagePart (package, null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Constructor3 ()
        {
            FakePackagePart p = new FakePackagePart (null, uris [0]);
        }

        [Test]
        public void Constructor4 ()
        {
            new FakePackagePart (package, uris [0], null);
        }

        [Test]
        public void Constructor5 ()
        {
            new FakePackagePart (package, uris [0], "");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Constructor6 ()
        {
            new FakePackagePart (package, uris [0], "dgsdgdfgd");
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void CreateRelationship1 ()
        {
            part.CreateRelationship (null, TargetMode.External, null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void CreateRelationship2 ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreateRelationship3a ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreateRelationship3b ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "    ");
        }

        [Test]
        public void CreateRelationship4 ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "blah");
        }


        [Test]
        public void CreateRelationship5 ()
        {
            PackageRelationship r = part.CreateRelationship (uris [1], TargetMode.External, "blah", null);
            Assert.IsNotNull (r.Id, "#1");
            Assert.AreEqual (part.Uri, r.SourceUri, "#2");
            Assert.AreEqual (uris [1], r.TargetUri, "#3");
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void CreateRelationship6 ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "blah", "");
        }

        [Test]
        public void CreateRelationship7 ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "blah", "asda");
        }

        [Test]
        [ExpectedException (typeof (Xml.XmlException))]
        public void CreateDupeRelationship ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "blah", "asda");
            part.CreateRelationship (uris [1], TargetMode.External, "blah", "asda");
        }

        [Test]
        [ExpectedException (typeof (Xml.XmlException))]
        public void CreateDupeRelationshipId ()
        {
            part.CreateRelationship (uris [1], TargetMode.External, "blah", "asda");
            part.CreateRelationship (uris [2], TargetMode.Internal, "aaa", "asda");
        }

        [Test]
        public void EnumeratePartsBreak ()
        {
            FakePackage package = new FakePackage (FileAccess.ReadWrite, false);

            package.CreatePart (uris [0], "a/a");
            package.CreatePart (uris [1], "a/a");
            package.CreatePart (uris [2], "a/a");

            Assert.IsTrue (package.GetParts () == package.GetParts (), "#1");
            try {
                foreach (PackagePart part in package.GetParts ())
                    package.DeletePart (part.Uri);
                Assert.Fail ("This should throw an exception");
            } catch {
            }

            PackagePartCollection c = package.GetParts ();
            package.CreatePart (new Uri ("/dfds", UriKind.Relative), "a/a");
            int count = 0;
            foreach (PackagePart p in c) { count++; }
            Assert.AreEqual (3, count, "Three added, one deleted, one added");
        }

        [Test]
        public void GetStream1 ()
        {
            part.GetStream ();
            Assert.AreEqual (FileMode.OpenOrCreate, part.Modes [0], "#1");
            Assert.AreEqual (package.FileOpenAccess, part.Accesses [0], "#2");
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void PackagePartNoContentType ()
        {
            string s = new FakePackagePart (package, uris [0]).ContentType;
        }

        [Test]
        public void GetStream2 ()
        {
            Assert.IsNotNull (package.CreatePart (uris[1], contentType));
            Assert.AreEqual (1, new List<PackagePart> (package.GetParts()).Count, "#0a");
            package.Flush ();
			package.Close ();

            using (Package p = Package.Open (new MemoryStream (stream.ToArray ()))) {
				PackagePart part = new List<PackagePart>(p.GetParts ())[0];
                Stream s = part.GetStream ();
                Assert.IsTrue (s.CanRead, "#1");
                Assert.IsTrue (s.CanSeek, "#2");
                Assert.IsFalse (s.CanWrite, "#3");
            }
			
            using (Package p = Package.Open (new MemoryStream (stream.ToArray ()), FileMode.OpenOrCreate)) {
                PackagePart part = new List<PackagePart> (p.GetParts ()) [0];
                Stream s = part.GetStream ();
                Assert.IsTrue (s.CanRead, "#4");
                Assert.IsTrue (s.CanSeek, "#5");
                Assert.IsTrue (s.CanWrite, "#6");
            }

            using (Package p = Package.Open (new MemoryStream (stream.ToArray ()), FileMode.OpenOrCreate, FileAccess.ReadWrite)) {
                PackagePart part = new List<PackagePart> (p.GetParts ()) [0];
                Stream s = part.GetStream ();
                Assert.IsTrue (s.CanRead, "#7");
                Assert.IsTrue (s.CanSeek, "#8");
                Assert.IsTrue (s.CanWrite, "#9");
            }
        }
    }
}
