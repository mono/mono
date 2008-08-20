//
// XmlBinaryWriterSessionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
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
	public class XmlBinaryWriterSessionTest
	{
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void TryAddDuplicate ()
		{
			XmlDictionary dic = new XmlDictionary ();
			XmlDictionaryString d1 = dic.Add ("foo");
			XmlBinaryWriterSession s = new XmlBinaryWriterSession ();
			int idx;
			s.TryAdd (d1, out idx);
			s.TryAdd (d1, out idx);
		}

		[Test]
		public void TryAddIndex ()
		{
			XmlDictionary dic = new XmlDictionary ();
			XmlDictionaryString d1 = dic.Add ("foo");
			XmlDictionaryString d2 = dic.Add ("bar");
			XmlDictionaryString d3 = dic.Add ("baz");
			XmlBinaryWriterSession s = new XmlBinaryWriterSession ();
			int idx;
			s.TryAdd (d1, out idx);
			Assert.AreEqual (0, idx, "#1");
			s.TryAdd (d3, out idx);
			Assert.AreEqual (1, idx, "#2"); // not 2
		}
	}
}
