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
using System.Xml;

namespace System.IO.Packaging.Tests {

    [TestFixture]
    public class PackagePartTest : TestBase {

        const string PackagePropertiesType = "application/vnd.openxmlformats-package.core-properties+xml";
        
        //static void Main (string [] args)
        //{
        //    PackagePartTest t = new PackagePartTest ();
        //    t.FixtureSetup ();
        //    t.Setup ();

        //    t.AddThreeParts ();
        //}
        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void AddAbsoluteUri ()
        {
            package.CreatePart (new Uri ("file://lo/asd.asm", UriKind.Absolute), "aa/aa");
        }

        [Test]
        [Category ("NotWorking")]
        [Ignore ("This is a bug in the MS implementation. I don't think i can/should replicate it")]
        public void AddInvalidPartTwice ()
        {
            try {
                package.CreatePart (new Uri ("/file1.bmp", UriKind.Relative), "bmp");
            } catch (ArgumentException) {
                try {
                    package.CreatePart (new Uri ("/file1.bmp", UriKind.Relative), "bmp");
                } catch (InvalidOperationException) {
                    Assert.AreEqual (1, package.GetParts ().Count (), "Need to be buggy and return null");
                    Assert.AreEqual (null, package.GetParts ().ToArray () [0], "Be buggy and add null to the internal list");

                    return; // Success
                }
            }

            Assert.Fail ("Should have thrown an ArgumentException then InvalidOperationException");
        }

        [Test]
        public void AddThreeParts ()
        {
            foreach (Uri u in uris)
                package.CreatePart (u, "mime/type");

            Assert.AreEqual (3, package.GetParts ().Count (), "Should be three parts");
            PackagePartCollection c1 = package.GetParts ();
            package.CreatePart (new Uri ("/asdasdas", UriKind.Relative), "asa/s");
            PackagePartCollection c2 = package.GetParts ();
            bool eq = c1 == c2;
 
            Assert.IsNotNull (package.GetPart (new Uri (uris[0].ToString ().ToUpper (), UriKind.Relative)));
        }

        [Test]
        public void CheckPartProperties ()
        {
            package.CreatePart (new Uri ("/first", UriKind.Relative), "my/a");
            package.CreatePart (new Uri ("/second", UriKind.Relative), "my/b", CompressionOption.Maximum);
            package.CreatePart (new Uri ("/third", UriKind.Relative), "test/c", CompressionOption.SuperFast);
            package.Close ();
            package = Package.Open (new MemoryStream (stream.ToArray ()), FileMode.Open, FileAccess.Read);

            PackagePart[] parts = package.GetParts ().ToArray ();
            Assert.AreEqual (3, parts.Length);

            PackagePart part = parts[0];
            Assert.AreEqual (CompressionOption.NotCompressed, part.CompressionOption, "Compress option wrong1");
            Assert.AreEqual ("my/a", part.ContentType, "Content type wrong1");
            Assert.AreEqual (package, part.Package, "Wrong package1");
            Assert.AreEqual ("/first", part.Uri.ToString (), "Wrong package selected1");

            part = parts[1];
            Assert.AreEqual (CompressionOption.Maximum, part.CompressionOption, "Compress option wrong2");
            Assert.AreEqual ("my/b", part.ContentType, "Content type wrong2");
            Assert.AreEqual (package, part.Package, "Wrong package2");
            Assert.AreEqual ("/second", part.Uri.OriginalString, "Wrong package selected2");

            part = parts[2];
            Assert.AreEqual (CompressionOption.SuperFast, part.CompressionOption, "Compress option wrong3");
            Assert.AreEqual ("test/c", part.ContentType, "Content type wrong3");
            Assert.AreEqual (package, part.Package, "Wrong package3");
            Assert.AreEqual ("/third", part.Uri.ToString (), "Wrong package selected3");
        }
		
		[Test]
		public void SameExtensionDifferentContentTypeTest ()
		{
			// FIXME: Ideally we should be opening the zip and checking
			// exactly what was written to verify it's correct
			using (var stream = new MemoryStream ()) {
				var package = Package.Open (stream, FileMode.OpenOrCreate);
				package.CreatePart (uris [0], contentType + "1");
				package.CreatePart (uris [1], contentType + "2");
				package.CreatePart (uris [2], contentType + "2");
				package.Close ();
				
				package = Package.Open (new MemoryStream (stream.ToArray ()));
				Assert.AreEqual (contentType + "1", package.GetPart (uris [0]).ContentType, "#1");
				Assert.AreEqual (contentType + "2", package.GetPart (uris [1]).ContentType, "#2");
				Assert.AreEqual (contentType + "2", package.GetPart (uris [2]).ContentType, "#3");
			}
		}
		
		[Test]
		public void SameExtensionSameContentTypeTest ()
		{
			// FIXME: Ideally we should be opening the zip and checking
			// exactly what was written to verify it's correct
			using (var stream = new MemoryStream ()) {
				var package = Package.Open (stream, FileMode.OpenOrCreate);
				package.CreatePart (uris [0], contentType);
				package.CreatePart (uris [1], contentType);
				package.CreatePart (uris [2], contentType);
				package.Close ();
				
				package = Package.Open (new MemoryStream (stream.ToArray ()));
				Assert.AreEqual (contentType, package.GetPart (uris [0]).ContentType, "#1");
				Assert.AreEqual (contentType, package.GetPart (uris [1]).ContentType, "#2");
				Assert.AreEqual (contentType, package.GetPart (uris [2]).ContentType, "#3");
			}
		}
		
        [Test]
        public void CheckPartRelationships ()
        {
            AddThreeParts ();
            Assert.AreEqual (4, package.GetParts ().Count (), "#a");
            PackagePart part = package.GetPart (uris [0]);
            PackageRelationship r1 = part.CreateRelationship (part.Uri, TargetMode.Internal, "self");
            PackageRelationship r2 = package.CreateRelationship (part.Uri, TargetMode.Internal, "fake");
            PackageRelationship r3 = package.CreateRelationship (new Uri ("/fake/uri", UriKind.Relative), TargetMode.Internal, "self");

            Assert.AreEqual (6, package.GetParts ().Count (), "#b");
            Assert.AreEqual (1, part.GetRelationships ().Count (), "#1");
            Assert.AreEqual (1, part.GetRelationshipsByType ("self").Count (), "#2");
            Assert.AreEqual (r1, part.GetRelationship (r1.Id), "#3");
            Assert.AreEqual (2, package.GetRelationships ().Count (), "#4");
            Assert.AreEqual (1, package.GetRelationshipsByType ("self").Count (), "#5");
            Assert.AreEqual (r3, package.GetRelationship (r3.Id), "#6");

            Assert.AreEqual (6, package.GetParts ().Count (), "#c");
            Assert.AreEqual (part.Uri, r1.SourceUri, "#7");
            Assert.AreEqual (new Uri ("/", UriKind.Relative), r3.SourceUri, "#8");

            PackageRelationship r4 = part.CreateRelationship (uris [2], TargetMode.Internal, "other");
            Assert.AreEqual (part.Uri, r4.SourceUri);

            PackageRelationshipCollection relations = package.GetPart (uris [2]).GetRelationships ();
            Assert.AreEqual (0, relations.Count ());
            Assert.AreEqual (6, package.GetParts ().Count (), "#d");
        }

        [Test]
        public void CheckIndividualRelationships ()
        {
            PackagePart part = package.CreatePart (uris [0], contentType);
            part.CreateRelationship (uris [1], TargetMode.Internal, "relType");
            part.CreateRelationship (uris [2], TargetMode.External, "relType");

            package.Flush ();
            Assert.AreEqual (2, package.GetParts ().Count(), "#1");
            part = package.GetPart (new Uri ("/_rels" + uris [0].ToString () + ".rels", UriKind.Relative));
            Assert.IsNotNull (part);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void DeletePartsAfterAddingRelationships ()
        {
            CheckPartRelationships ();
            foreach (PackagePart p in new List<PackagePart> (package.GetParts ()))
                package.DeletePart (p.Uri);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void DeleteRelsThenParts ()
        {
            CheckPartRelationships ();
            foreach (PackageRelationship r in new List<PackageRelationship> (package.GetRelationships ()))
                package.DeleteRelationship (r.Id);
            foreach (PackagePart p in new List<PackagePart> (package.GetParts ()))
                package.DeletePart (p.Uri);
        }

        [Test]
        public void CreateValidPart ()
        {
            PackagePart part = package.CreatePart (uris [0], "img/bmp");
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void CreateDuplicatePart ()
        {
            CreateValidPart ();
            CreateValidPart ();
        }

        [Test]
        public void CreateValidPartTwice ()
        {
            CreateValidPart ();
            package.DeletePart (uris [0]);
            CreateValidPart ();

            Assert.AreEqual (1, package.GetParts ().Count (), "#1");
            Assert.AreEqual (uris [0], package.GetParts ().ToArray () [0].Uri, "#2");
            package.DeletePart (uris [0]);
            package.DeletePart (uris [0]);
            package.DeletePart (uris [0]);
        }

        [Test]
        public void IterateParts ()
        {
            List<PackagePart> parts = new List<PackagePart> ();
            parts.Add (package.CreatePart (new Uri ("/a", UriKind.Relative), "mime/type"));
            parts.Add (package.CreatePart (new Uri ("/b", UriKind.Relative), "mime/type"));
            List<PackagePart> found = new List<PackagePart> (package.GetParts ());
            Assert.AreEqual (parts.Count, found.Count, "Invalid number of parts");
            Assert.IsTrue (found.Contains (parts [0]), "Doesn't contain first part");
            Assert.IsTrue (found.Contains (parts [1]), "Doesn't contain second part");

            Assert.IsTrue (found [0] == parts [0] || found [0] == parts [1], "Same object reference should be used");
            Assert.IsTrue (found [1] == parts [0] || found [1] == parts [1], "Same object reference should be used");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void NoStartingSlashPartialUri ()
        {
            PackagePart part = package.CreatePart (new Uri ("file1.bmp", UriKind.Relative), "bmp");
        }

        [Test]
        public void PackagePropertiesTest ()
        {
            package.PackageProperties.Category = "category";
            package.PackageProperties.ContentStatus = "status";
            package.PackageProperties.ContentType = "contentttype";
            package.PackageProperties.Created = new DateTime (2008, 12, 12, 2, 3, 4);
            package.PackageProperties.Creator = "mono";
            package.PackageProperties.Description = "description";
            package.PackageProperties.Identifier = "id";
            package.PackageProperties.Keywords = "key words";
            package.PackageProperties.Language = "english";
            package.PackageProperties.LastModifiedBy = "modified";
            package.PackageProperties.LastPrinted = new DateTime (2007, 12, 12, 2, 3, 4);
            package.PackageProperties.Modified = new DateTime (2008, 12, 12, 3, 4, 5);
            package.PackageProperties.Revision = "reviison";
            package.PackageProperties.Subject = "subject";
            package.PackageProperties.Title = "title";
            package.PackageProperties.Version = "version";
            Assert.AreEqual (0, package.GetParts ().Count (), "#1");
            package.Flush ();
            Assert.AreEqual (2, package.GetParts ().Count (), "#2");
            var part = package.GetParts ().Where (p => p.ContentType == PackagePropertiesType).ToList ().First ();
            Assert.IsNotNull (part);
            Assert.IsTrue (part.Uri.OriginalString.StartsWith ("/package/services/metadata/core-properties/"), "#3");
            Assert.IsTrue (part.Uri.OriginalString.EndsWith (".psmdcp"), "#4");
            package.Close ();
            package = Package.Open (new MemoryStream (stream.ToArray ()));

            Assert.AreEqual (package.PackageProperties.Category, "category", "#5");
            Assert.AreEqual (package.PackageProperties.ContentStatus, "status", "#6");
            Assert.AreEqual (package.PackageProperties.ContentType, "contentttype", "#7");
            Assert.AreEqual (package.PackageProperties.Created, new DateTime (2008, 12, 12, 2, 3, 4), "#8");
            Assert.AreEqual (package.PackageProperties.Creator, "mono", "#9");
            Assert.AreEqual (package.PackageProperties.Description, "description", "#10");
            Assert.AreEqual (package.PackageProperties.Identifier, "id", "#11");
            Assert.AreEqual (package.PackageProperties.Keywords, "key words", "#12");
            Assert.AreEqual (package.PackageProperties.Language, "english", "#13");
            Assert.AreEqual (package.PackageProperties.LastModifiedBy, "modified", "#14");
            Assert.AreEqual (package.PackageProperties.LastPrinted, new DateTime (2007, 12, 12, 2, 3, 4), "#15");
            Assert.AreEqual (package.PackageProperties.Modified, new DateTime (2008, 12, 12, 3, 4, 5), "#16");
            Assert.AreEqual (package.PackageProperties.Revision, "reviison", "#17");
            Assert.AreEqual (package.PackageProperties.Subject, "subject", "#18");
            Assert.AreEqual (package.PackageProperties.Title, "title", "#19");
            Assert.AreEqual (package.PackageProperties.Version, "version", "#20");
        }

        [Test]
        public void PackagePropertiestest2 ()
        {
            package.PackageProperties.Title = "TITLE";
            package.Close ();
            package = Package.Open (new MemoryStream (stream.ToArray ()));
            Assert.AreEqual (null, package.PackageProperties.Category, "#1");
        }
        
        [Test]
        [ExpectedException (typeof (IOException))]
        public void PackagePropertiesReadonly ()
        {
            PackagePropertiesTest ();
            package.PackageProperties.Title = "Title";
        }
        
        [Test]
        public void PartExists ()
        {
            CreateValidPart ();
            Assert.IsNotNull (package.GetPart (uris [0]), "Part could not be found");
            Assert.IsTrue (package.PartExists (uris [0]), "Part didn't exist");
            Assert.AreEqual (1, package.GetParts ().Count (), "Only one part");
        }

        [Test]
        public void RemoveThreeParts ()
        {
            AddThreeParts ();
            foreach (PackagePart p in new List<PackagePart> (package.GetParts ()))
                package.DeletePart (p.Uri);
            Assert.AreEqual (0, package.GetParts ().Count (), "Should contain no parts");
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void RemoveThreePartsBreak ()
        {
            AddThreeParts ();
            foreach (PackagePart p in package.GetParts ())
                package.DeletePart (p.Uri);
        }

        [Test]
        public void CheckContentTypes ()
        {
	        Assert.IsFalse (package.PartExists(new Uri ("[Content_Types].xml", UriKind.Relative)));
        }
    }
}
