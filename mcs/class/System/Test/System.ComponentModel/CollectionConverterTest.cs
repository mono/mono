//
// System.ComponentModel.CollectionConverterTest.cs -
//	NUnit Test Cases for System.ComponentModel.CollectionConverter
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MonoTests.System.ComponentModel {

	[TestFixture]
	public class CollectionConverterTest {

		private CollectionConverter cc;
		private StringCollection sc;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			cc = new CollectionConverter ();
			sc = new StringCollection ();
		}

		[Test]
		public void ApplicableTypes ()
		{
			Type t = cc.GetType ();
			Assert.AreEqual (t, TypeDescriptor.GetConverter (typeof (StringCollection)).GetType (), "StringCollection");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString_Null ()
		{
			cc.ConvertFromString (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString_Empty ()
		{
			cc.ConvertFromString (String.Empty);
		}

		private const string array_of_strings  = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <string>go</string>\r\n  <string>mono</string>\r\n  </ArrayOfString>";

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFromString ()
		{
			cc.ConvertFromString (array_of_strings);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom ()
		{
			cc.ConvertFrom (array_of_strings);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertFrom_XmlSerializer ()
		{
			XmlSerializer xs = new XmlSerializer (typeof (string[]));
			object o = xs.Deserialize (new StringReader (array_of_strings));
			cc.ConvertFrom (o);
		}

		[Test]
		public void ConvertTo ()
		{
			Assert.AreEqual (String.Empty, cc.ConvertTo (null, null, null, typeof (string)), "null");
			Assert.AreEqual ("(Collection)", cc.ConvertTo (null, null, sc, typeof (string)), "0");
			sc.Add ("some string value");
			Assert.AreEqual ("(Collection)", cc.ConvertTo (null, null, sc, typeof (string)), "1");
			sc.Clear ();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConvertTo_TypeNull ()
		{
			cc.ConvertTo (null, null, sc, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ConvertTo_TypeNotString ()
		{
			cc.ConvertTo (null, null, sc, typeof (int));
		}

		[Test]
		public void GetProperties ()
		{
			// documented to always return null
			Assert.IsNull (cc.GetProperties (null), "null");
			Assert.IsNull (cc.GetProperties (null, null), "null,null");
			Assert.IsNull (cc.GetProperties (null, null, null), "null,null,null");
		}

		[Test]
		public void GetPropertiesSupported ()
		{
			// documented to always return false
			Assert.IsFalse (cc.GetPropertiesSupported (), "empty");
			Assert.IsFalse (cc.GetPropertiesSupported (null), "null");
		}
	}
}
