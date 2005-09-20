//
// System.Configuration.SettingsPropertyCollectionTest.cs - Unit tests
// for System.Configuration.SettingsPropertyCollection.
//
// Author:
//	Chris Toshok  <toshok@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.System.Configuration {

	[TestFixture]
	public class SettingsPropertyCollectionTest {

		[Test]
		public void Add ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			SettingsProperty test_prop = new SettingsProperty ("test_prop");

			Assert.AreEqual (0, col.Count, "A1");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void AddDuplicate ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			SettingsProperty test_prop = new SettingsProperty ("test_prop");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A1");

			test_prop = new SettingsProperty ("test_prop");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A2");
		}

		[Test]
		public void Remove ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			SettingsProperty test_prop = new SettingsProperty ("test_prop");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A1");

			col.Remove ("test_prop");

			Assert.AreEqual (0, col.Count, "A2");
		}

		[Test]
		public void Remove_NonExistant ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			SettingsProperty test_prop = new SettingsProperty ("test_prop");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A1");

			col.Remove ("test_prop2");

			Assert.AreEqual (1, col.Count, "A2");
		}

		[Test]
		public void Clear ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			SettingsProperty test_prop = new SettingsProperty ("test_prop");

			col.Add (test_prop);

			Assert.AreEqual (1, col.Count, "A1");

			col.Clear ();

			Assert.AreEqual (0, col.Count, "A2");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Add ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();

			col.SetReadOnly ();

			SettingsProperty test_prop = new SettingsProperty ("test_prop");
			col.Add (test_prop);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Remove ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();

			SettingsProperty test_prop = new SettingsProperty ("test_prop");
			col.Add (test_prop);

			col.SetReadOnly ();

			col.Remove ("test_prop");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void ReadOnly_Clear ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();

			SettingsProperty test_prop = new SettingsProperty ("test_prop");
			col.Add (test_prop);

			col.SetReadOnly ();

			col.Clear ();
		}
	}

}

#endif
