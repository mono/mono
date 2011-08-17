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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.IO.Packaging.Tests {

    [TestFixture]
    public class PackageTest : TestBase {

        //static void Main (string [] args)
        //{
        //    PackageTest t = new PackageTest ();
        //    t.FixtureSetup ();
        //    t.Setup ();
        //    t.RelationshipPartGetStream ();
        //}
        string path = "test.package";

        public override void Setup ()
        {
            if (File.Exists (path))
                File.Delete (path);
        }

        public override void TearDown ()
        {
            try {
                if (package != null)
                    package.Close ();
            } catch {
                // FIXME: This shouldn't be required when i implement this
            }
            if (File.Exists (path))
                File.Delete (path);
        }

        [Test]
        public void CheckContentFile ()
        {
            MemoryStream stream = new MemoryStream ();
            package = Package.Open (stream, FileMode.Create, FileAccess.ReadWrite);
            package.CreatePart (uris[0], "custom/type");
            package.CreateRelationship (uris[1], TargetMode.External, "relType");

            package.Close ();
            package = Package.Open (new MemoryStream (stream.ToArray ()), FileMode.Open, FileAccess.ReadWrite);
            package.Close ();
            package = Package.Open (new MemoryStream (stream.ToArray ()), FileMode.Open, FileAccess.ReadWrite);

            Assert.AreEqual (2, package.GetParts ().Count (), "#1");
            Assert.AreEqual (1, package.GetRelationships ().Count (), "#2");
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void CorruptStream ()
        {
            stream = new FakeStream (true, true, true);
            stream.Write (new byte [1024], 0, 1024);
            package = Package.Open (stream);
        }

        [Test]
        [ExpectedException (typeof (NotSupportedException))]
        public void FileShareReadWrite ()
        {
            package = Package.Open (path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        }

        [Test]
        [ExpectedException (typeof (FileNotFoundException))]
        public void OpenNonExistantPath ()
        {
            package = Package.Open (path, FileMode.Open);
        }

        [Test]
        public void NonExistantPath ()
        {
            package = Package.Open (path);
        }

        [Test]
        public void PreExistingPath ()
        {
            package = Package.Open (path);
            package.Close ();
            package = Package.Open (path);
        }

        [Test]
        public void CreatePath ()
        {
            package = Package.Open (path, FileMode.Create);
            Assert.AreEqual (FileAccess.ReadWrite, package.FileOpenAccess, "#1");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreatePathReadonly ()
        {
            package = Package.Open (path, FileMode.Create, FileAccess.Read);
            package.Close ();
        }

        [Test]
        public void CreatePathTwice ()
        {
            package = Package.Open (path, FileMode.Create);
            package.Close ();
            package = Package.Open (path, FileMode.Open);
            Assert.AreEqual (FileAccess.ReadWrite, package.FileOpenAccess);
        }

        [Test]
        public void OpenPackageMultipleTimes ()
        {
            var filename = Path.GetTempFileName ();
            try {
                using (var file = File.Open (filename, FileMode.OpenOrCreate)) {
                    var package = Package.Open (file, FileMode.OpenOrCreate);
                    var part = package.CreatePart (new Uri ("/test", UriKind.Relative), "test/type");
                    using (var stream = part.GetStream ())
                        stream.Write (new byte [1024 * 1024], 0, 1024 * 1024);
                    package.Close ();
                }
                
                for (int i = 0; i < 10; i++) {
                    using (var file = File.Open (filename, FileMode.OpenOrCreate))
                    using (var package = Package.Open (file)) {
                        package.GetParts ();
                        package.GetRelationships ();
                    }
                }
            } finally {
                if (File.Exists (filename))
                    File.Delete (filename);
            }
        }
        
        [Test]
        public void OpenPathReadonly ()
        {
            package = Package.Open (path, FileMode.Create);
            package.CreatePart (uris[0], contentType);
            package.CreateRelationship (uris[1], TargetMode.External, "relType");
            package.Close ();
            package = Package.Open (path, FileMode.Open, FileAccess.Read);
            Assert.AreEqual (2, package.GetParts ().Count (), "#1");
            Assert.AreEqual (1, package.GetRelationships ().Count (), "#2");
            Assert.AreEqual (FileAccess.Read, package.FileOpenAccess, "Should be read access");
            try {
                package.CreatePart (uris [0], contentType);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }

            try {
                package.CreateRelationship (uris [0], TargetMode.Internal, contentType);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }

            try {
                package.DeletePart (uris [0]);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }

            try {
                package.DeleteRelationship (contentType);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void ReadableStream ()
        {
            stream = new FakeStream (true, false, false);
            package = Package.Open (stream);
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void ReadableSeekableStream ()
        {
            stream = new FakeStream (true, false, true);
            package = Package.Open (stream);

            try {
                package.DeleteRelationship (contentType);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void ReadableSeekableFullStream ()
        {
            stream = new FakeStream (true, false, true);
            stream.Write (new byte [10], 0, 10);
            package = Package.Open (stream);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void ReadOnlyAccess ()
        {
            stream = new FakeStream (true, false, true);
            package = Package.Open (path, FileMode.CreateNew, FileAccess.Read);

            try {
                package.DeleteRelationship (contentType);
                Assert.Fail ("Cannot modify a read-only package");
            } catch (IOException) {

            }
        }

        [Test]
        [Category ("NotWorking")]
        [Ignore ("I'm not supposed to write to the relation stream unless i'm flushing")]
        public void RelationshipPartGetStream ()
        {
            package = Package.Open (path);
            package.CreateRelationship (uris [0], TargetMode.External, "rel");
            PackagePart p = package.GetPart (relationshipUri);
            Assert.IsNotNull (p, "#0");
            Stream s = p.GetStream ();
            Assert.AreEqual (0, s.Length, "#1");
            Assert.IsTrue (s.CanRead, "#2");
            Assert.IsTrue (s.CanSeek, "#3");
            Assert.IsFalse (s.CanTimeout, "#4");
            Assert.IsTrue (s.CanWrite, "#5");
        }

        [Test]
        [ExpectedException (typeof (IOException))]
        public void SetFileModeOnUnwriteableStream ()
        {
            stream = new FakeStream (true, false, true);
            package = Package.Open (stream, FileMode.Truncate);
        }

        [Test]
        [ExpectedException (typeof (NotSupportedException))]
        public void SetAppendOnWriteableStream ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Append);
        }

        [Test]
        public void SetCreateNewOnWriteableStream ()
        {
            package = Package.Open (stream, FileMode.CreateNew);
        }

        [Test]
        [ExpectedException(typeof(IOException))]
        public void SetCreateNewOnWriteableStream2 ()
        {
            stream = new FakeStream (true, true, true);
            stream.Write (new byte [1000], 0, 1000);
            package = Package.Open (stream, FileMode.CreateNew);
            Assert.AreEqual (0, stream.Length, "#1");
        }

        [Test]
        public void SetCreateOnWriteableStream ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Create);
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void SetOpenOnWriteableStream ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Open);
        }

        [Test]
        public void SetOpenOrCreateOnWriteableStream ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.OpenOrCreate);
        }

        [Test]
        [ExpectedException (typeof (NotSupportedException))]
        public void SetTruncateOnWriteableStream ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Truncate);
        }

        [Test]
        [ExpectedException (typeof (NotSupportedException))]
        public void SetTruncateOnWriteablePath ()
        {
            stream = new FakeStream (true, true, true);
            File.Create (path).Close ();
            package = Package.Open (path, FileMode.Truncate);
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void StreamOpen ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Open);
        }

        [Test]
        public void StreamCreate ()
        {
            stream = new FakeStream (true, true, true);
            package = Package.Open (stream, FileMode.Create);
        }

        [Test]
        [ExpectedException (typeof (IOException))]
        public void UnusableStream ()
        {
            stream = new FakeStream (false, false, false);
            package = Package.Open (stream);
        }

        // Bug - I'm passing in FileAccess.Write but it thinks I've passed FileAccess.Read
        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void WriteAccessDoesntExist ()
        {
            package = Package.Open (path, FileMode.OpenOrCreate, FileAccess.Write);
        }

        [Test]
        public void ReadWriteAccessDoesntExist ()
        {
            package = Package.Open (path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        [Test]
        [ExpectedException (typeof (FileFormatException))]
        public void WriteOnlyAccessExists ()
        {
            System.IO.File.Create (path).Close ();
            package = Package.Open (path, FileMode.OpenOrCreate, FileAccess.Write);
        }
    }
}