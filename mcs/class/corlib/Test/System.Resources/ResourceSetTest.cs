// 
// ResourceSetTest.cs: NUnit Test Cases for System.Resources.ResourceSet
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2008 Novell, Inc. (http://www.novell.com)
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
using System.Collections;
using System.IO;
using System.Resources;

using NUnit.Framework;

namespace MonoTests.System.Resources {

	public class DefaultResourceSet : ResourceSet {

		public DefaultResourceSet ()
		{
		}

		public Hashtable GetTable ()
		{
			return base.Table;
		}

		public IResourceReader GetReader ()
		{
			return base.Reader;
		}
	}

	public class ClonableObject : ICloneable {

		private int n = 0;

		private ClonableObject (int value)
		{
			n = value;
		}

		public ClonableObject ()
			: this (0)
		{
		}

		public int Value {
			get { return n; }
		}

		public object Clone ()
		{
			object clone = new ClonableObject (n);
			n++;
			return (ClonableObject) clone;
		}
	}

	public class CloneResourceSet : ResourceSet {

		public CloneResourceSet (ClonableObject c)
		{
			Table.Add ("clone", c);
		}
	}

	[TestFixture]
	public class ResourceSetTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_IResourceReader_Null ()
		{
			new ResourceSet ((IResourceReader) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_Stream_Null ()
		{
			new ResourceSet ((Stream) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Constructor_String_Null ()
		{
			new ResourceSet ((string) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void Constructor_String_Empty ()
		{
			new ResourceSet (String.Empty);
		}

		[Test]
		public void Defaults ()
		{
			DefaultResourceSet rs = new DefaultResourceSet ();
			Assert.IsNotNull (rs.GetTable (), "Table");
			Assert.IsNull (rs.GetReader (), "Reader");
			Assert.AreEqual (typeof (ResourceReader), rs.GetDefaultReader (), "DefaultReaderType");
			Assert.AreEqual (typeof (ResourceWriter), rs.GetDefaultWriter (), "DefaultWriterType");
			rs.Dispose ();
			Assert.IsNull (rs.GetTable (), "Disposed/Table");
			Assert.IsNull (rs.GetReader (), "Disposed/Reader");
			Assert.AreEqual (typeof (ResourceReader), rs.GetDefaultReader (), "Disposed/DefaultReaderType");
			Assert.AreEqual (typeof (ResourceWriter), rs.GetDefaultWriter (), "Disposed/DefaultWriterType");
		}

		[Test]
		public void Clonable ()
		{
			ClonableObject c0 = new ClonableObject ();
			Assert.AreEqual (0, c0.Value, "Original");
			CloneResourceSet rs = new CloneResourceSet (c0);
			ClonableObject c1 = (ClonableObject) rs.GetObject ("clone");
			Assert.AreEqual (c1.Value, c0.Value, "Clone");
			Assert.IsTrue (Object.ReferenceEquals (c0, c1), "Same");
		}

		[Test]
		public void Dispose ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
		}

		[Test]
		public void GetEnumerator ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			Assert.IsNotNull (rs.GetEnumerator ());
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetEnumerator_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			Assert.IsNotNull (rs.GetEnumerator ());
		}

		[Test]
		public void GetObject_DoesNotExists ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			Assert.IsNull (rs.GetObject ("doesnotexists"), "default");
			Assert.IsNull (rs.GetObject ("doesnotexists", true), "case");
			Assert.IsNull (rs.GetObject ("doesnotexists", false), "!case");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetObject_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			rs.GetObject ("doesnotexists");
		}

		[Test]
		public void GetString_DoesNotExists ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			Assert.IsNull (rs.GetString ("doesnotexists"), "default");
			Assert.IsNull (rs.GetString ("doesnotexists", true), "case");
			Assert.IsNull (rs.GetString ("doesnotexists", false), "!case");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetString_NotAString ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.GetString ("clone");
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetString_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			rs.GetString ("doesnotexists");
		}
	}
}
