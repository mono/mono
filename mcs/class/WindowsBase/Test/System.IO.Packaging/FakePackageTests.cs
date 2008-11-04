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
    public class FakePackageTests : TestBase {

        //static void Main (string [] args)
        //{
        //    FakePackageTests t = new FakePackageTests ();
        //    t.FixtureSetup ();
        //    t.Setup ();
        //    t.RelationshipPartGetStream ();
        //}
        
        private new FakePackage package;
        public override void Setup ()
        {
            package = new FakePackage (FileAccess.ReadWrite, true);
        }

        [Test]
        public void CheckAutomaticParts ()
        {
            package.CreatePart (uris [0], contentType);
            Assert.AreEqual (1, package.CreatedParts.Count (), "#1");
            Assert.AreEqual (uris [0], package.CreatedParts [0], "#2");
            Assert.AreEqual (0, package.DeletedParts.Count (), "#3");
            Assert.AreEqual (1, package.GetParts ().Count (), "#4");
        }

        [Test]
        public void CheckAutomaticParts2 ()
        {
            package.CreateRelationship (uris [0], TargetMode.External, "relationship");
            Assert.AreEqual (1, package.CreatedParts.Count (), "#1");
            Assert.AreEqual (relationshipUri, package.CreatedParts [0], "#2");
            Assert.AreEqual (0, package.DeletedParts.Count (), "#3");
            Assert.AreEqual (1, package.GetParts ().Count (), "#4");

            PackagePart p = package.GetPart (relationshipUri);
            Assert.AreEqual (package, p.Package, "#5");
            Assert.AreEqual (CompressionOption.NotCompressed, p.CompressionOption, "#6");
            Assert.AreEqual ("application/vnd.openxmlformats-package.relationships+xml", p.ContentType, "#7");
        }

        [Test]
        public void RelationshipPartGetRelationships ()
        {
            CheckAutomaticParts2 ();
            PackagePart p = package.GetPart (relationshipUri);

            try {
                p.CreateRelationship (uris[0], TargetMode.Internal, "asdas");
                Assert.Fail ("This should fail 1");
            } catch (InvalidOperationException) {

            }

            try {
                p.DeleteRelationship("aa");
                Assert.Fail ("This should fail 2");
            } catch (InvalidOperationException) {

            }

            try {
                p.GetRelationship ("id");
                Assert.Fail ("This should fail 3");
            } catch (InvalidOperationException) {

            }

            try {
                p.GetRelationships ();
                Assert.Fail ("This should fail 4");
            } catch (InvalidOperationException) {

            }

            try {
                p.GetRelationshipsByType ("type");
                Assert.Fail ("This should fail 5");
            } catch (InvalidOperationException) {

            }
            
            try {
                p.RelationshipExists ("id");
                Assert.Fail ("This should fail 6");
            } catch (InvalidOperationException) {
                
            }
        }

		[Test]
        public void TestProperties ()
        {
            Assert.IsNotNull (package.PackageProperties, "#1");
            package.PackageProperties.Title = "Title";
            package.Flush ();

            // the relationship part and packageproperties part
            Assert.AreEqual (2, package.CreatedParts.Count, "#2");
        }
    }
}
