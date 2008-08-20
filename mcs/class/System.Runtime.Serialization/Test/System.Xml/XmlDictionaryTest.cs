//
// XmlDictionaryTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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

using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xml
{
	[TestFixture]
	public class XmlDictionaryTest
	{
		[Test]
		public void SimpleUse ()
		{
			XmlDictionary d1 = new XmlDictionary ();
			XmlDictionaryString dns;
			Assert.IsFalse (d1.TryLookup (String.Empty, out dns), "#0");
			XmlDictionaryString s1 = d1.Add ("foo");
			XmlDictionary d2 = new XmlDictionary ();
			XmlDictionaryString s2 = d2.Add ("foo");
			XmlDictionaryString o = null;
			Assert.AreEqual ("foo", s1.ToString (), "#1");
			Assert.AreEqual (0, s1.Key, "#2");
			Assert.AreEqual (0, s1.Key, "#3");
			Assert.IsTrue (d1.TryLookup ("foo", out o), "#4");
			Assert.IsTrue (d1.TryLookup (s1, out o), "#5");
			Assert.IsFalse (d1.TryLookup (s2, out o), "#6");
			Assert.AreEqual (0, XmlDictionaryString.Empty.Key, "#7");
			Assert.AreEqual (false, XmlDictionaryString.Empty.Dictionary == XmlDictionary.Empty, "#8");
			XmlDictionaryString dummy;
			Assert.AreEqual (false, XmlDictionary.Empty.TryLookup ("", out dummy), "#9");

		}

		[Test]
		public void Empty ()
		{
			XmlDictionary d = new XmlDictionary ();
			XmlDictionaryString dns;
			d.Add (String.Empty);
			Assert.IsTrue (d.TryLookup (String.Empty, out dns), "#0");
			Assert.AreEqual (0, dns.Key, "#1");
		}

		[Test]
		public void EmptyAfterAdd ()
		{
			XmlDictionary d = new XmlDictionary ();
			XmlDictionaryString dns;
			d.Add ("foo");
			Assert.IsFalse (d.TryLookup (String.Empty, out dns), "#0");
			Assert.IsTrue (d.TryLookup ("foo", out dns), "#1");
			Assert.AreEqual (0, dns.Key, "#2");
		}

		[Test]
		public void Add ()
		{
			XmlDictionary d = new XmlDictionary ();
			Assert.AreEqual (0, d.Add ("foo").Key, "#1");
			Assert.AreEqual (0, d.Add ("foo").Key, "#2");
			Assert.AreEqual (1, d.Add ("bar").Key, "#3");
			Assert.AreEqual (2, d.Add ("baz").Key, "#4");
		}
	}
}
