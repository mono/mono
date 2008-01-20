//
// MonoTests.System.Collections.Generic.Test.ReadOnlyCollectionTest
//
// Authors:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (C) 2008 Gert Driesen
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System.Collections.ObjectModel
{
	class SyncPretendingList<T> : List<T>, ICollection
	{
		bool ICollection.IsSynchronized {
			get { return true; }
		}
	}

	[TestFixture]
	public class ReadOnlyCollectionTest
	{
		[Test]
		public void Constructor0 ()
		{
			Collection <int> c = new Collection <int> ();
			c.Add (10);
			c.Add (7);

			ReadOnlyCollection <int> r = new ReadOnlyCollection <int> (c);
			Assert.AreEqual (10, r [0], "#1");
			Assert.AreEqual (7, r [1], "#2");
		}
		
		[Test]
		public void IsSimpleWrapper ()
		{
			Collection <int> c = new Collection <int> ();
			c.Add (1);
			
			ReadOnlyCollection <int> r = new ReadOnlyCollection <int> (c);
			Assert.AreEqual (1, r.Count, "#1");			

			c.Remove (1);
			Assert.AreEqual (0, r.Count, "#2");			
		}
		
		[Test]
		public void IList_Properties ()
		{
			List <int> l = new List <int> ();
			ReadOnlyCollection <int> r = new ReadOnlyCollection <int> (l);

			Assert.IsTrue (((IList)r).IsReadOnly, "#1");
			Assert.IsTrue (((IList)r).IsFixedSize, "#2");
		}
		
		[Test]
		public void ICollection_Properties ()
		{
			List <int> l = new SyncPretendingList <int> ();
			ReadOnlyCollection <int> r = new ReadOnlyCollection <int> (l);

			Assert.IsFalse (((ICollection)r).IsSynchronized, "#1");
		}

		[Test]
		public void Constructor0_List_Null ()
		{
			try {
				new ReadOnlyCollection <int> ((List <int>) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("list", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ICollection_CopyTo ()
		{
			Collection <int> c = new Collection <int> ();
			c.Add (10);
			c.Add (7);

			ReadOnlyCollection <int> r = new ReadOnlyCollection <int> (c);
			Array array = Array.CreateInstance (typeof (int), 2);
			((ICollection) c).CopyTo (array, 0);
			Assert.AreEqual (10, array.GetValue (0), "#A1");
			Assert.AreEqual (7, array.GetValue (1), "#A2");

			array = Array.CreateInstance (typeof (object), 2);
			((ICollection) c).CopyTo (array, 0);
			Assert.AreEqual (10, array.GetValue (0), "#B1");
			Assert.AreEqual (7, array.GetValue (1), "#B2");
		}
	}
}

#endif
