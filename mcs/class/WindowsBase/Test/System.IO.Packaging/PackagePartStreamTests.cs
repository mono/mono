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
// Alan McGovern (amcgovern@novell.com)
//


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace System.IO.Packaging.Tests {
    
    [TestFixture]
    public class PackagePartStreamTests : TestBase {

        byte [] buffer;
        List<PackagePart> Parts = new List<PackagePart> ();
        static void Main (string [] args)
        {
            PackagePartStreamTests t = new PackagePartStreamTests ();
            t.FixtureSetup ();
            t.Setup ();
            t.CheckFlushTest ();
        }

        public override void FixtureSetup ()
        {
            base.FixtureSetup ();

            Random r = new Random ();
            buffer = new byte [1000];
            r.NextBytes (buffer);
        }

        public override void Setup ()
        {
            base.Setup ();

            Parts.Clear ();
            foreach (Uri u in uris)
                Parts.Add (package.CreatePart (u, "mime/type"));
        }

        [Test]
        public void SameStreamTest ()
        {
            Assert.AreEqual (0, stream.Length, "#a");
            package.Flush ();
            Assert.IsTrue (stream.Length > 0, "#b");

            Stream s1 = Parts [0].GetStream ();
            Stream s2 = Parts [0].GetStream ();
            Assert.IsFalse (s1 == s2, "#1");
            s1.WriteByte (5);
            Assert.AreEqual (1, s1.Position, "#2");
            Assert.AreEqual (0, s2.Position, "#3");
            Assert.AreEqual (s1.Length, s2.Length, "#4");
            Assert.AreEqual (5, s2.ReadByte (), "#5");
            s2.SetLength (0);
            Assert.AreEqual (0, s1.Length);
        }

        [Test]
        public void NoFlushTest ()
        {
            Parts [0].GetStream ().Write (buffer, 0, buffer.Length);
            Assert.AreEqual (0, stream.Length);
        }

        [Test]
        public void FlushIndividualTest ()
        {
            Parts [0].GetStream ().Write (buffer, 0, buffer.Length);
            Parts [0].GetStream ().Flush ();
            Assert.AreEqual (buffer.Length, Parts [0].GetStream ().Length, "#1");
            Assert.IsTrue (stream.Length > buffer.Length, "#2");
        }

        [Test]
        [Category ("NotWorking")]
        [Ignore ("This test only works on MS.NET. Behaviour probably not easily replicatable")]
        public void FlushPackageTest1 ()
        {
            FlushIndividualTest ();

            long count = stream.Length;
            package.Flush ();
            Assert.IsTrue (stream.Length > count, "#1");
        }

        [Test]
        [Category ("NotWorking")]
        [Ignore ("This test is useless i believe")]
        public void FlushOnlyPackage ()
        {
            NoFlushTest ();
            package.Flush ();
            long count = stream.Length;
            TearDown ();
            Setup ();
           // FlushPackageTest1 ();
            Assert.AreEqual (count, stream.Length, "#1");
        }

        [Test]
        public void GetMultipleStreams ()
        {
            foreach (PackagePart p in Parts) {
                p.GetStream ().Write (buffer, 0, buffer.Length);
                p.GetStream ().Flush ();
                Stream ssss = p.GetStream ();
                bool equal = p.GetStream () == p.GetStream ();
                stream.Flush ();
            }
            long position = stream.Length;
        }

        [Test]
        public void FlushThenTruncate ()
        {
            Parts [0].GetStream ().Write (buffer, 0, buffer.Length);
            package.Flush ();
            Assert.IsTrue (stream.Length > buffer.Length, "#1");
            
            Parts [0].GetStream ().SetLength (0);
            package.Flush ();
            Assert.IsTrue (stream.Length < buffer.Length, "#2");

            long length = stream.Length;
            foreach (PackagePart p in package.GetParts ().ToArray ())
                package.DeletePart (p.Uri);
            package.Flush ();

            Assert.IsTrue (stream.Length < length, "#3");
        }

        [Test]
//        [Category ("NotWorking")]
        public void CheckFlushTest ()
        {
            buffer = new byte [1024 * 1024];
            Assert.AreEqual (0, stream.Length, "#1");
            Parts [0].GetStream ().Write (buffer, 0, buffer.Length);
            Assert.AreEqual (0, stream.Length, "#2");
            Assert.AreEqual (Parts[0].GetStream ().Length, buffer.Length, "#2b");
            Parts [1].GetStream ().Write (buffer, 0, buffer.Length);
            Assert.AreEqual (0, stream.Length, "#3");
            Assert.AreEqual (Parts[1].GetStream ().Length, buffer.Length, "#3b");
            Parts [0].GetStream ().Flush ();
            Assert.IsTrue (stream.Length > buffer.Length * 2, "#4");
            long count = stream.Length;
            package.Flush ();

            // FIXME: On MS.NET this works. I don't think it's worth replicating
            //Assert.IsTrue (count < stream.Length, "#5");
        }

        [Test]
        public void CheckFlushTest2 ()
        {
            buffer = new byte [1024 * 1024];
            Assert.AreEqual (0, stream.Length, "#1");
            Parts [0].GetStream ().Write (buffer, 0, buffer.Length);
            Assert.AreEqual (0, stream.Length, "#2");
            Parts [1].GetStream ().Write (buffer, 0, buffer.Length);
            Assert.AreEqual (0, stream.Length, "#3");
            package.Flush ();
            Assert.IsTrue (stream.Length > buffer.Length * 2, "#4");
        }
    }
}