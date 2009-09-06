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
    [Category ("NotWorking")]
    [Ignore ("This depends on a fix to System.Uri to support the UriParser API")]
    public class PackUriHelperTests {
        static void Main (string [] args)
        {
            PackUriHelperTests t = new PackUriHelperTests ();
            t.ResolvePartUri2 ();
        }


        Uri a;
        Uri b;
        Uri part1 = new Uri ("/file1", UriKind.Relative);
        Uri part2 = new Uri ("/file2", UriKind.Relative);
        Uri main = new Uri ("/main.html", UriKind.Relative);

		[SetUpAttribute]
		public void Setup()
		{
			a = PackUriHelper.Create (new Uri ("http://www.test.com/pack1.pkg"));
			b = PackUriHelper.Create (new Uri ("http://www.test.com/pack2.pkg"));
			Console.WriteLine ("A is: {0}", a);
			Console.WriteLine("B is: {0}", b);
		}
		
        [Test]
        public void ComparePackUriTest ()
        {
            Assert.AreEqual (0, PackUriHelper.ComparePackUri (null, null), "#1");
            Assert.IsTrue (PackUriHelper.ComparePackUri (a, null) > 0, "#2");
            Assert.AreEqual (0, PackUriHelper.ComparePackUri (a, a), "#3");
            Assert.IsTrue (PackUriHelper.ComparePackUri (a, b) < 0, "#4");
        }

        [Test]
        [ExpectedException(typeof(UriFormatException))]
        public void CompareInvalidTest ()
        {
            Uri a = new Uri ("pack://url1");
            PackUriHelper.ComparePackUri (a, a);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void NonPackUriCompareTest ()
        {
            PackUriHelper.ComparePackUri (new Uri ("http://wtest.com"), a);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void NonPackUriCompareRelativeTest ()
        {
            PackUriHelper.ComparePackUri (new Uri ("wtest.com", UriKind.Relative), a);
        }

        [Test]
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
        public void GetPartUriTest ()
        {
            Assert.IsNull (PackUriHelper.GetPartUri (a), "#1");
            Assert.AreEqual (main, PackUriHelper.GetPartUri (PackUriHelper.Create (a, main)), "#2");
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
        }
    }
}