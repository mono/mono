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
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.IO.Packaging
{

    [TestFixture]
    public class FakePackageTests : TestBase
    {

        //static void Main (string [] args)
        //{
        //    FakePackageTests t = new FakePackageTests ();
        //    t.FixtureSetup ();
        //    t.Setup ();
        //    t.RelationshipPartGetStream ();
        //}

        private new FakePackage package;
        public override void Setup()
        {
            package = new FakePackage(FileAccess.ReadWrite, true);
        }

        [Test]
        public void CheckAutomaticParts()
        {
            package.CreatePart(uris[0], contentType);
            Assert.AreEqual(1, package.CreatedParts.Count(), "#1");
            Assert.AreEqual(uris[0], package.CreatedParts[0], "#2");
            Assert.AreEqual(0, package.DeletedParts.Count(), "#3");
            Assert.AreEqual(1, package.GetParts().Count(), "#4");
        }

        [Test]
        public void CheckAutomaticParts2()
        {
            package.CreateRelationship(uris[0], TargetMode.External, "relationship");
            Assert.AreEqual(1, package.CreatedParts.Count(), "#1");
            Assert.AreEqual(relationshipUri, package.CreatedParts[0], "#2");
            Assert.AreEqual(0, package.DeletedParts.Count(), "#3");
            Assert.AreEqual(1, package.GetParts().Count(), "#4");

            PackagePart p = package.GetPart(relationshipUri);
            Assert.AreEqual(package, p.Package, "#5");
            Assert.AreEqual(CompressionOption.NotCompressed, p.CompressionOption, "#6");
            Assert.AreEqual("application/vnd.openxmlformats-package.relationships+xml", p.ContentType, "#7");
        }

        [Test]
        public void CheckProperties()
        {
            Assert.AreEqual(0, package.GotParts.Count, "#1");
            object o = package.PackageProperties;
            Assert.AreEqual(1, package.GotParts.Count, "#2");
            Assert.AreEqual("/_rels/.rels", package.GotParts[0].ToString(), "#3");
        }

        [Test]
        public void RelationshipPartGetRelationships()
        {
            CheckAutomaticParts2();
            PackagePart p = package.GetPart(relationshipUri);

            try
            {
                p.CreateRelationship(uris[0], TargetMode.Internal, "asdas");
                Assert.Fail("This should fail 1");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                p.DeleteRelationship("aa");
                Assert.Fail("This should fail 2");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                p.GetRelationship("id");
                Assert.Fail("This should fail 3");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                p.GetRelationships();
                Assert.Fail("This should fail 4");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                p.GetRelationshipsByType("type");
                Assert.Fail("This should fail 5");
            }
            catch (InvalidOperationException)
            {

            }

            try
            {
                p.RelationshipExists("id");
                Assert.Fail("This should fail 6");
            }
            catch (InvalidOperationException)
            {

            }
        }

        [Test]
        public void TestProperties()
        {
            Assert.IsNotNull(package.PackageProperties, "#1");
            package.PackageProperties.Title = "Title";
            package.Flush();

            // the relationship part and packageproperties part
            Assert.AreEqual(2, package.CreatedParts.Count, "#2");
        }

        [Test]
        public void TestWordDoc()
        {
            MemoryStream stream = new MemoryStream();
            Package package = CreateWordDoc(stream);
            Assert.IsTrue(package.PartExists(new Uri("/word/document.xml", UriKind.Relative)), "#1");
            Assert.IsTrue(package.RelationshipExists("rel1"), "#2");
            package.Close();
            package = Package.Open(new MemoryStream(stream.ToArray()), FileMode.Open);
            Assert.AreEqual(10, package.GetParts().Count(), "#3");
            Assert.AreEqual (9, package.GetRelationships ().Count (), "#4");
            Assert.IsTrue(package.PartExists(new Uri("/word/document.xml", UriKind.Relative)), "#5");
            Assert.IsTrue(package.RelationshipExists("rel1"), "#6");
        }

        Package CreateWordDoc(Stream stream)
        {
            Package pack = Package.Open(stream, FileMode.Create);

            // Create package parts.
            PackagePart wordDocument = pack.CreatePart(new Uri("/word/document.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml");
            PackagePart wordNumbering = pack.CreatePart(new Uri("/word/numbering.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml");
            PackagePart wordStyles = pack.CreatePart(new Uri("/word/styles.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml");
            PackagePart docPropsApp = pack.CreatePart(new Uri("/docProps/app.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.extended-properties+xml");
            PackagePart wordSettings = pack.CreatePart(new Uri("/word/settings.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml");
            PackagePart wordTheme1 = pack.CreatePart(new Uri("/word/theme/theme1.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.theme+xml");
            PackagePart wordFontTable = pack.CreatePart(new Uri("/word/fontTable.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml");
            PackagePart wordWebSettings = pack.CreatePart(new Uri("/word/webSettings.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.wordprocessingml.webSettings+xml");
            PackagePart docPropsCore = pack.CreatePart(new Uri("/docProps/core.xml", UriKind.Relative), "application/vnd.openxmlformats-package.core-properties+xml");

            // Create relationships for package.
            pack.CreateRelationship(new Uri("docProps/app.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties");
            pack.CreateRelationship(new Uri("docProps/core.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties");
            pack.CreateRelationship(new Uri("word/document.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");

            // Create document relationships.
            pack.CreateRelationship(new Uri("settings.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings", "rel1");
            pack.CreateRelationship(new Uri("styles.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", "rel2");
            pack.CreateRelationship(new Uri("numbering.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering", "rel3");
            pack.CreateRelationship(new Uri("theme/theme1.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/theme", "rel4");
            pack.CreateRelationship(new Uri("fontTable.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/fontTable", "rel5");
            pack.CreateRelationship(new Uri("webSettings.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/webSettings", "rel6");

            // Load some basic data into the different parts.
            foreach (PackagePart part in package.GetParts())
                using (Stream s = part.GetStream())
                    s.Write(new byte[10], 0, 10);
            
            return pack;
        }

        Package CreateSpreadsheet(Stream stream)
        {
            Package pack = Package.Open(stream, FileMode.Create);

            // Create package parts.
            PackagePart workbookPart = pack.CreatePart(new Uri("/xl/workbook.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
            PackagePart sharedStringsPart = pack.CreatePart(new Uri("/xl/sharedStrings.xml", UriKind.Relative), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");

            workbookPart.CreateRelationship(new Uri("/xl/sharedStrings.xml", UriKind.Relative), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", "rel1");

            // Load some basic data into the different parts.
            foreach (PackagePart part in package.GetParts())
                using (Stream s = part.GetStream())
                    s.Write(new byte[10], 0, 10);

            return pack;
        }

        [Test]
        public void TestExcelWorkbook()
        {
            MemoryStream stream = new MemoryStream();
            Package package = CreateSpreadsheet(stream);
            Assert.IsTrue(package.PartExists(new Uri("/xl/workbook.xml", UriKind.Relative)), "#1");
            Assert.IsTrue(package.PartExists(new Uri("/xl/sharedStrings.xml", UriKind.Relative)), "#2");

            package.Close();
            package = Package.Open(new MemoryStream(stream.ToArray()), FileMode.Open);

            PackagePart workbookPart = package.GetPart(new Uri("/xl/workbook.xml", UriKind.Relative));
            Assert.IsTrue(workbookPart.RelationshipExists("rel1"), "#3");

            var r = workbookPart.GetRelationship("rel1");
            Assert.IsFalse(r.TargetUri.IsAbsoluteUri, "#4");
            package.Close();
        }
    }
}
