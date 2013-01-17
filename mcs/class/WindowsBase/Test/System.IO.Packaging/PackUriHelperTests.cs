// PackUriHelperTests.cs created with MonoDevelop
// User: alan at 13:39 28/10/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.IO.Packaging;
using NUnit.Framework;

namespace System.IO.Packaging.Tests {
    
    [TestFixture]
    public class PackUriHelperTests {

        Uri a {
            get { return PackUriHelper.Create (new Uri ("http://www.test.com/pack1.pkg")); }
        }
        Uri b {
            get { return  PackUriHelper.Create (new Uri ("http://www.test.com/pack2.pkg")); }
        }

        Uri part1 = new Uri ("/file1", UriKind.Relative);
        Uri part2 = new Uri ("/file2", UriKind.Relative);
        Uri main = new Uri ("/main.html", UriKind.Relative);

        [Test]
        [Category("NotWorking")]
        public void ComparePackUriTest ()
        {
            Assert.AreEqual (0, PackUriHelper.ComparePackUri (null, null), "#1");
            Assert.IsTrue (PackUriHelper.ComparePackUri (a, null) > 0, "#2");
            Assert.AreEqual (0, PackUriHelper.ComparePackUri (a, a), "#3");
            Assert.IsTrue (PackUriHelper.ComparePackUri (a, b) < 0, "#4");
        }

        [Test]
        [Category("NotWorking")]
        [ExpectedException(typeof(UriFormatException))]
        public void CompareInvalidTest ()
        {
            Uri a = new Uri ("pack://url1");
            PackUriHelper.ComparePackUri (a, a);
        }

        [Test]
        [Category("NotWorking")]
        [ExpectedException (typeof (ArgumentException))]
        public void NonPackUriCompareTest ()
        {
            PackUriHelper.ComparePackUri (new Uri ("http://wtest.com"), a);
        }

        [Test]
        [Category("NotWorking")]
        [ExpectedException (typeof (ArgumentException))]
        public void NonPackUriCompareRelativeTest ()
        {
            PackUriHelper.ComparePackUri (new Uri ("wtest.com", UriKind.Relative), a);
        }

        [Test]
        [Category("NotWorking")]
        [ExpectedException (typeof (ArgumentException))]
        public void InvalidPartUriCompareTest ()
        {
            PackUriHelper.ComparePartUri (a, b);
        }

        [Test]
        public void PartUriCompareTest ()
        {
            Assert.AreEqual (0, PackUriHelper.ComparePartUri (null, null), "#1");
            Assert.IsTrue (PackUriHelper.ComparePartUri (part1, null) > 0, "#2");
            Assert.IsTrue (PackUriHelper.ComparePartUri (part1, part2) < 0, "#3");
        }

        [Test]
        [Category("NotWorking")]
        public void CreateTest ()
        {
            Assert.AreEqual ("pack://http:,,www.test.com,pack.pkg/",
                             PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg")).ToString (), "#1");
            Assert.AreEqual ("pack://http:,,www.test.com,pack.pkg/",
                             PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"), null, null).ToString (), "#2");
            Assert.AreEqual ("pack://http:,,www.test.com,pack.pkg/main.html#frag",
                             PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"),
                                                   new Uri ("/main.html", UriKind.Relative), "#frag").ToString (), "#3");
            Assert.AreEqual ("pack://http:,,www.test.com,pack.pkg/main.html#frag",
                             PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"),
                                                   new Uri ("/main.html", UriKind.Relative), "#frag").ToString (), "#3");
        }

        [Test]
        [Category("NotWorking")]
        public void CreateTest2()
        {
                Uri uri = PackUriHelper.Create(new Uri("http://www.test.com/pack1.pkg"));
                Assert.AreEqual("pack://pack:,,http:%2C%2Cwww.test.com%2Cpack1.pkg,/", PackUriHelper.Create(uri).ToString());
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreateInvalidTest ()
        {
            PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"), new Uri ("/main.html", UriKind.Relative), "notfrag");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreateInvalidTest2 ()
        {
            PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"), new Uri ("/main.html", UriKind.Relative), "");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void CreateInvalidTest3 ()
        {
            PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"), new Uri ("/main.html", UriKind.Relative), "");
        }

        [Test]
        [Category("NotWorking")]
        public void CreateInvalidTest4 ()
        {
            PackUriHelper.Create (new Uri ("http://www.test.com/pack.pkg"), new Uri ("/main.html", UriKind.Relative));
        }

        [Test]
        public void CreatePartUri ()
        {
            Assert.IsFalse (PackUriHelper.CreatePartUri (part1).IsAbsoluteUri, "#1");
            Assert.AreEqual (new Uri (part1.ToString (), UriKind.Relative), PackUriHelper.CreatePartUri (part1), "#2");
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void CreatePartUri2 ()
        {
            PackUriHelper.CreatePartUri (null);
        }

        [Test]
        public void GetNormalizedPartUriTest ()
        {
            Uri uri = new Uri ("/test.com".ToUpperInvariant (), UriKind.Relative);
            Assert.IsTrue (uri == PackUriHelper.GetNormalizedPartUri (uri));
        }
        [Test]
        public void GetNormalisedPartUritest4 ()
        {
            PackUriHelper.GetNormalizedPartUri (part1);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void GetNormalizedPartUriTest2 ()
        {
            PackUriHelper.GetNormalizedPartUri (null);
        }

        [Test]
        [ExpectedException (typeof (UriFormatException))]
        public void GetNormalizedPartUriTest3 ()
        {
            Assert.AreEqual (new Uri (a.ToString ().ToUpperInvariant (), UriKind.Relative), PackUriHelper.GetNormalizedPartUri (a));
        }

        [Test]
        [Category("NotWorking")]
        public void GetPackageUriTest ()
        {
            Assert.AreEqual (a, PackUriHelper.GetPackageUri (PackUriHelper.Create (a, new Uri ("/test.html", UriKind.Relative))));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void GetPackageUri2 ()
        {
            PackUriHelper.GetPackageUri (null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void GetPackageUri3 ()
        {
            PackUriHelper.GetPackageUri (part1);
        }

        [Test]
        [Category("NotWorking")]
        public void GetPartUriTest ()
        {
                var pack = PackUriHelper.Create(new Uri("http://www.test.com/pack1.pkg"));
                var part = new Uri("/main.html", UriKind.Relative);
                var pack_part = new Uri(@"pack://pack:,,http:%2C%2Cwww.test.com%2Cpack1.pkg,/main.html");

                Assert.IsNull(PackUriHelper.GetPartUri(pack), "#1");
                Assert.AreEqual(pack_part, PackUriHelper.Create(pack, part), "#2");
                Assert.AreEqual(part, PackUriHelper.GetPartUri(PackUriHelper.Create(pack, part)), "#3");
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void GetPartUriTest2 ()
        {
            PackUriHelper.GetPartUri (null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void GetPartUriTest3 ()
        {
            PackUriHelper.GetPartUri (part1);
        }

        [Test]
        public void GetRelationshipPartUriTest ()
        {
            Assert.AreEqual ("/_rels/file1.rels", PackUriHelper.GetRelationshipPartUri (part1).ToString());
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void GetRelationshipPartUriTest2 ()
        {
            PackUriHelper.GetRelationshipPartUri (null);
        }

        [Test]
        public void GetRelativeUriTest ()
        {
            Uri src = new Uri ("/1/2/3/file.jpg", UriKind.Relative);
            Uri dest = new Uri ("/file2.png", UriKind.Relative);
            Assert.AreEqual (new Uri ("../../../file2.png", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#1");

            dest = new Uri ("/1/2", UriKind.Relative);
            Assert.AreEqual (new Uri ("../../2", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#2");

            dest = new Uri ("/1/2/", UriKind.Relative);
            Assert.AreEqual (new Uri ("", UriKind.Relative), PackUriHelper.GetRelativeUri (src, src), "#4");

            // See: http://msdn.microsoft.com/en-us/library/system.io.packaging.packurihelper.getrelativeuri.aspx

            src = new Uri("/mydoc/markup/page.xml", UriKind.Relative);
            dest = new Uri("/mydoc/markup/picture.jpg", UriKind.Relative);
            Assert.AreEqual (new Uri ("picture.jpg", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#5");

            src = new Uri("/mydoc/markup/page.xml", UriKind.Relative);
            dest = new Uri("/mydoc/picture.jpg", UriKind.Relative);
            Assert.AreEqual (new Uri ("../picture.jpg", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#6");

            src = new Uri("/mydoc/markup/page.xml", UriKind.Relative);
            dest = new Uri("/mydoc/images/picture.jpg", UriKind.Relative);
            Assert.AreEqual (new Uri ("../images/picture.jpg", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#7");

        }

        [Test]
        [ExpectedException (typeof(ArgumentException))]
        public void GetRelativeUriTest2 ()
        {
            Uri src = new Uri ("/1/2/3/file.jpg", UriKind.Relative);
            Uri dest = new Uri ("/1/2/", UriKind.Relative);
            Assert.AreEqual (new Uri ("../file2.png", UriKind.Relative), PackUriHelper.GetRelativeUri (src, dest), "#3");
        }

        [Test]
        public void IsRelationshipPartUriTest ()
        {
            Assert.IsFalse (PackUriHelper.IsRelationshipPartUri (new Uri ("/_rels/Whatever", UriKind.Relative)));
            Assert.IsTrue (PackUriHelper.IsRelationshipPartUri (new Uri ("/_rels/Whatever.rels", UriKind.Relative)));
        }

        [Test]
        public void IsRelationshipPartUriTest2 ()
        {
            Uri uri = new Uri ("/test/uri", UriKind.Relative);
            PackUriHelper.IsRelationshipPartUri (uri);
        }

        [Test]
        [ExpectedException(typeof(UriFormatException))]
        public void ResolvePartUri ()
        {
            Uri src = new Uri ("/1/2/3/4", UriKind.Relative);
            Uri dest = new Uri ("/MyFile", UriKind.Relative);

            // Can't be empty url
            Assert.AreEqual (new Uri (""), PackUriHelper.ResolvePartUri (src, dest), "#1");
        }


        [Test]
        //[ExpectedException (typeof (UriFormatException))]
        public void ResolvePartUri2 ()
        {
            Uri src = new Uri ("/1/2/3/4", UriKind.Relative);
            Uri dest = new Uri ("/1/2/MyFile", UriKind.Relative);

            // Can't be empty url
            Assert.AreEqual (new Uri ("/1/2/MyFile", UriKind.Relative), PackUriHelper.ResolvePartUri (src, dest), "#1");
            Assert.AreEqual (new Uri ("/1/2/3/4", UriKind.Relative), PackUriHelper.ResolvePartUri (dest, src), "#1");
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void ResolvePartUri3 ()
        {
            Uri src = new Uri ("/1/2/3/4", UriKind.Relative);
            Uri dest = new Uri ("http://www.example.com", UriKind.Absolute);
            PackUriHelper.ResolvePartUri (src, dest);
        }

        [Test]
        public void ResolvePartUri4 ()
        {
            Uri src = new Uri ("/", UriKind.Relative);
            Uri dest = new Uri ("word/document.xml", UriKind.Relative);

            Uri result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.IsFalse(result.IsAbsoluteUri, "#1");
            Assert.AreEqual ("/word/document.xml", result.ToString(), "#2");

            // See: http://msdn.microsoft.com/en-us/library/system.io.packaging.packurihelper.resolveparturi.aspx

            src = new Uri ("/mydoc/markup/page.xml", UriKind.Relative);
            dest = new Uri("picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/mydoc/markup/picture.jpg", result.ToString(), "#3");

            dest = new Uri("images/picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/mydoc/markup/images/picture.jpg", result.ToString(), "#4");

            dest = new Uri("./picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/mydoc/markup/picture.jpg", result.ToString(), "#5");

            dest = new Uri("../picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/mydoc/picture.jpg", result.ToString(), "#6");

            dest = new Uri("../images/picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/mydoc/images/picture.jpg", result.ToString(), "#7");

            src = new Uri ("/", UriKind.Relative);
            dest = new Uri("images/picture.jpg", UriKind.Relative);
            result = PackUriHelper.ResolvePartUri (src, dest);
            Assert.AreEqual ("/images/picture.jpg", result.ToString(), "#8");
        }
    }
}
