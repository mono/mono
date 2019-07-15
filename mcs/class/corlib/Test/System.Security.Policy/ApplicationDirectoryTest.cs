//
// ApplicationDirectoryTest.cs - NUnit Test Cases for ApplicationDirectory
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
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

using NUnit.Framework;
using System;
using System.IO;
using System.Security.Policy;
using System.Text;

namespace MonoTests.System.Security.Policy {

	[TestFixture]
	public class ApplicationDirectoryTest {

		private string Invalid (bool exception) 
		{
			StringBuilder sb = new StringBuilder ();
			foreach (char c in Path.InvalidPathChars)
				sb.Append (c);
			if ((exception) && (sb.Length < 1))
				throw new ArgumentException ("no invalid chars");
			return sb.ToString ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ApplicationDirectory_Null ()
		{
			new ApplicationDirectory (null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ApplicationDirectory_Empty ()
		{
			new ApplicationDirectory (String.Empty);
		}

		[Test]
		public void ApplicationDirectory_Invalid ()
		{
			new ApplicationDirectory (Invalid (false));
		}

		[Test]
		public void ApplicationDirectory_String ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("mono");
			Assert.AreEqual ("mono", ad.Directory, "Directory");
		}

		[Test]
		public void ApplicationDirectory_FileUrl ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("file://MONO");
			Assert.AreEqual ("file://MONO", ad.Directory, "Directory");
		}

		[Test]
		public void ApplicationDirectory_HttpUrl ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("http://www.example.com/");
			Assert.AreEqual ("http://www.example.com/", ad.Directory, "Directory");
		}

		[Test]
		public void Copy ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("novell");
			Assert.AreEqual ("novell", ad.Directory, "Directory");
			ApplicationDirectory copy = (ApplicationDirectory)ad.Copy ();
			Assert.IsTrue (ad.Equals (copy), "ad.Equals(copy)");
			Assert.IsTrue (copy.Equals (ad), "copy.Equals(ad)");
			Assert.IsFalse (Object.ReferenceEquals (ad, copy), "Copy");
			Assert.AreEqual (ad.GetHashCode (), copy.GetHashCode (), "GetHashCode");
			Assert.AreEqual (ad.ToString (), copy.ToString (), "ToString");
		}

		[Test]
		public void Equals ()
		{
			ApplicationDirectory ad1 = new ApplicationDirectory ("mono");
			Assert.IsFalse (ad1.Equals (null), "Equals(null)");
			Assert.IsFalse (ad1.Equals (String.Empty), "Equals(String.Empty)");
			Assert.IsFalse (ad1.Equals ("mono"), "Equals(mono)");
			Assert.IsTrue (ad1.Equals (ad1), "Equals(self)");
			ApplicationDirectory ad2 = new ApplicationDirectory (ad1.Directory);
			Assert.IsTrue (ad1.Equals (ad2), "Equals(ad2)");
			Assert.IsTrue (ad2.Equals (ad1), "Equals(ad2)");
			ApplicationDirectory ad3 = new ApplicationDirectory ("..");
			Assert.IsFalse (ad2.Equals (ad3), "Equals(ad3)");
		}

		[Test]
		public void GetHashCode_ ()
		{
			string linux = "/unix/path/mono";
			ApplicationDirectory ad1 = new ApplicationDirectory (linux);
			Assert.AreEqual (linux, ad1.Directory);
			Assert.AreEqual (linux.GetHashCode (), ad1.GetHashCode (), "GetHashCode-Linux");

			string windows = "\\windows\\path\\mono";
			ApplicationDirectory ad2 = new ApplicationDirectory (windows);
			Assert.AreEqual (windows, ad2.Directory);
			Assert.AreEqual (windows.GetHashCode (), ad2.GetHashCode (), "GetHashCode-Windows");
		}

		[Test]
		public void ToString_ ()
		{
			ApplicationDirectory ad = new ApplicationDirectory ("file://MONO");
			string ts = ad.ToString ();
			Assert.IsTrue (ts.StartsWith ("<System.Security.Policy.ApplicationDirectory"), "Tag");
			Assert.IsTrue (ts.IndexOf ("version=\"1\"") > 0, "Directory");
			Assert.IsTrue (ts.IndexOf ("<Directory>file://MONO</Directory>") > 0, "Directory");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Equals_Invalid ()
		{
			// funny one
			string appdir = Invalid (true);
			// constructor is ok with an invalid path...
			ApplicationDirectory ad = new ApplicationDirectory (appdir);
			// we can copy it...
			ApplicationDirectory copy = (ApplicationDirectory)ad.Copy ();
			// we can't get it's hash code
			Assert.AreEqual (appdir.GetHashCode (), ad.GetHashCode (), "GetHashCode");
			// we can convert it to string...
			Assert.IsTrue (ad.ToString ().IndexOf (appdir) > 0, "ToString");
			// ... but it throws in Equals - with self!
			Assert.IsTrue (ad.Equals (ad), "Equals(self)");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ToString_Invalid ()
		{
			new ApplicationDirectory (Invalid (true)).ToString ();
		}
	}
}
