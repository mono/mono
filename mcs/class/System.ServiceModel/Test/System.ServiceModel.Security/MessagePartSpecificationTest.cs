//
// MessagePartSpecificationTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class MessagePartSpecificationTest
	{
		[Test]
		public void DefaultValues ()
		{
			MessagePartSpecification s =
				new MessagePartSpecification ();
			Assert.IsFalse (s.IsBodyIncluded, "#1");
			Assert.AreEqual (0, s.HeaderTypes.Count, "#2");

			s = new MessagePartSpecification (new XmlQualifiedName [] {new XmlQualifiedName ("foo", "urn:foo")});
			Assert.IsFalse (s.IsBodyIncluded, "#3");
			Assert.AreEqual (1, s.HeaderTypes.Count, "#4");
		}

		[Test]
		public void Union ()
		{
			XmlQualifiedName q1, q2, q3;
			q1 = new XmlQualifiedName ("foo");
			q2 = new XmlQualifiedName ("bar");
			q3 = new XmlQualifiedName ("baz");
			MessagePartSpecification p1 =
				new MessagePartSpecification (false, new XmlQualifiedName [] {q1, q2});
			MessagePartSpecification p2 =
				new MessagePartSpecification (true, new XmlQualifiedName [] {q3, q2});
			p1.Union (p2);
			Assert.IsTrue (p1.IsBodyIncluded, "#1");
			// Sigh. It does not exclude duplicates.
			Assert.AreEqual (4, p1.HeaderTypes.Count, "#1-2");
			Assert.IsTrue (p1.HeaderTypes.Contains (q1), "#2");
			Assert.IsTrue (p1.HeaderTypes.Contains (q2), "#3");
			Assert.IsTrue (p1.HeaderTypes.Contains (q3), "#4");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void UnionReadOnlyPart ()
		{
			MessagePartSpecification s =
				new MessagePartSpecification ();
			s.MakeReadOnly ();
			Assert.AreEqual (true, s.IsReadOnly, "#1");
			s.Union (new MessagePartSpecification ());
		}
	}
}
