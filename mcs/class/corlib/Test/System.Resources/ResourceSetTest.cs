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

		[Test] // ctor (IResourceReader)
		public void Constructor1_Reader_Null ()
		{
			try {
				new ResourceSet ((IResourceReader) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("reader", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (Stream)
		public void Constructor2_Stream_Closed ()
		{
			MemoryStream ms = new MemoryStream ();
			ms.Close ();

			try {
				new ResourceSet (ms);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Stream was not readable
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (Stream)
		public void Constructor2_Stream_Null ()
		{
			try {
				new ResourceSet ((Stream) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("stream", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor3_FileName_Null ()
		{
			try {
				new ResourceSet ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.AreEqual ("path", ex.ParamName, "#5");
			}
		}

		[Test] // ctor (String)
		public void Constructor3_FileName_Empty ()
		{
			try {
				new ResourceSet (string.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty path name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
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
			rs.Dispose ();
		}

		[Test]
		public void GetEnumerator ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			Assert.IsNotNull (rs.GetEnumerator ());
		}

		[Test]
		public void GetEnumerator_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			try {
				rs.GetEnumerator ();
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a closed resource set
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
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
		public void GetObject_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			try {
				rs.GetObject ("doesnotexists");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a closed resource set
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
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
		public void GetString_NotAString ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			try {
				rs.GetString ("clone");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// This particular resource was not a String -
				// call GetObject instead.  Resource name: clone
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsTrue (ex.Message.IndexOf ("String") != -1, "#5");
				Assert.IsTrue (ex.Message.IndexOf ("GetObject") != -1, "#6");
				Assert.IsTrue (ex.Message.IndexOf ("clone") != -1, "#7");
			}
		}

		[Test]
		public void GetString_Disposed ()
		{
			CloneResourceSet rs = new CloneResourceSet (new ClonableObject ());
			rs.Dispose ();
			try {
				rs.GetString ("doesnotexists");
				Assert.Fail ("#1");
			} catch (ObjectDisposedException ex) {
				// Cannot access a closed resource set
				Assert.AreEqual (typeof (ObjectDisposedException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}
	}
}
