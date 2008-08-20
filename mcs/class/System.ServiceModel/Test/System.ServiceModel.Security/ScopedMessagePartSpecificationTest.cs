//
// ScopedMessagePartSpecificationTest.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class ScopedMessagePartSpecificationTest
	{
		[Test]
		public void DefaultValues ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			Assert.IsNotNull (s.ChannelParts, "#1");
			Assert.AreEqual (0, s.Actions.Count, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPartsNull ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			s.AddParts (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPartsNull2 ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			s.AddParts (null, "urn:foo");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddPartsNull3 ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			s.AddParts (new MessagePartSpecification (), null);
		}

		[Test]
		public void AddParts ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			Assert.IsFalse (s.ChannelParts.IsBodyIncluded, "#1");
			s.AddParts (new MessagePartSpecification (true));
			Assert.AreEqual (0, s.Actions.Count, "#2");
			Assert.IsTrue (s.ChannelParts.IsBodyIncluded, "#3");

			XmlQualifiedName foo = new XmlQualifiedName ("foo");
			XmlQualifiedName bar = new XmlQualifiedName ("bar");

			s.AddParts (new MessagePartSpecification (new XmlQualifiedName [] {foo}), "urn:foo");
			Assert.AreEqual (1, s.Actions.Count, "#4");
			MessagePartSpecification m;
			s.TryGetParts ("urn:foo", out m);
			Assert.IsNotNull (m, "#5");
			Assert.AreEqual (1, m.HeaderTypes.Count, "#6");

			s.AddParts (new MessagePartSpecification (true, new XmlQualifiedName [] {bar}), "urn:foo");
			Assert.AreEqual (1, s.Actions.Count, "#7");
			s.TryGetParts ("urn:foo", out m);
			Assert.IsNotNull (m, "#8");
			//List<XmlQualifiedName> l = new List<XmlQualifiedName> (m.HeaderTypes);
			Assert.AreEqual (2, m.HeaderTypes.Count, "#9");
			Assert.IsTrue (m.IsBodyIncluded, "#10");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void AddToReadOnlyCollection ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			s.MakeReadOnly ();
			Assert.AreEqual (true, s.IsReadOnly, "#1");
			s.AddParts (new MessagePartSpecification (), "urn:myaction");
		}

		[Test]
		public void TryGetParts ()
		{
			ScopedMessagePartSpecification s =
				new ScopedMessagePartSpecification ();
			MessagePartSpecification ret;
			Assert.IsFalse (s.TryGetParts ("urn:myaction", out ret));
			Assert.IsFalse (s.TryGetParts ("urn:myaction", true, out ret));
			Assert.IsFalse (s.TryGetParts ("urn:myaction", false, out ret));

			s.AddParts (new MessagePartSpecification (), "urn:myaction");
			Assert.IsTrue (s.TryGetParts ("urn:myaction", out ret));
			Assert.IsTrue (s.TryGetParts ("urn:myaction", true, out ret));
			Assert.IsTrue (s.TryGetParts ("urn:myaction", false, out ret));
		}
	}
}
