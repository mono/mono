//
// MonoTests.System.Collections.Generic.Test.CollectionTest
//
// Authors:
//	David Waite (mass@akuma.org)
//
// Copyright (C) 2005 David Waite
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Collections.ObjectModel
{
	[TestFixture]
	public class CollectionTest
	{
		[Test]
		public void UsableSyncLockTest ()
		{
			List <int> list = new List <int> ();
			Collection <int> c = new Collection <int> (list);

			object listLock = ((ICollection) list).SyncRoot;
			object cLock = ((ICollection) c).SyncRoot;

			Assert.AreSame (listLock, cLock);
		}

		[Test]
		public void UnusableSyncLockTest ()
		{
			UnimplementedList <int> list = new UnimplementedList <int> ();
			Collection <int> c = new Collection <int> (list);

			object cLock = ((ICollection) c).SyncRoot;

			Assert.IsNotNull (cLock);
		}

		[Test]
		public void ICollection_CopyTo ()
		{
			Collection <int> c = new Collection <int> ();
			c.Add (10);
			c.Add (7);

			Array array = Array.CreateInstance (typeof (int), 2);
			((ICollection) c).CopyTo (array, 0);
			Assert.AreEqual (10, array.GetValue (0), "#A1");
			Assert.AreEqual (7, array.GetValue (1), "#A2");

			array = Array.CreateInstance (typeof (int), 5);
			((ICollection) c).CopyTo (array, 2);
			Assert.AreEqual (0, array.GetValue (0), "#B1");
			Assert.AreEqual (0, array.GetValue (1), "#B2");
			Assert.AreEqual (10, array.GetValue (2), "#B3");
			Assert.AreEqual (7, array.GetValue (3), "#B4");
			Assert.AreEqual (0, array.GetValue (4), "#B5");

			array = Array.CreateInstance (typeof (object), 5);
			((ICollection) c).CopyTo (array, 2);
			Assert.IsNull (array.GetValue (0), "#C1");
			Assert.IsNull (array.GetValue (1), "#C2");
			Assert.AreEqual (10, array.GetValue (2), "#C3");
			Assert.AreEqual (7, array.GetValue (3), "#C4");
			Assert.IsNull (array.GetValue (4), "#C2");
		}

		class UnimplementedList <T> : IList <T>
		{

			#region IList <T> Members

			int IList <T>.IndexOf (T item)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			void IList <T>.Insert (int index, T item)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			void IList <T>.RemoveAt (int index)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			T IList <T>.this [int index]
			{
				get
				{
					throw new Exception ("The method or operation is not implemented.");
				}
				set
				{
					throw new Exception ("The method or operation is not implemented.");
				}
			}

			#endregion

			#region ICollection <T> Members

			void ICollection <T>.Add (T item)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			void ICollection <T>.Clear ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			bool ICollection <T>.Contains (T item)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			void ICollection <T>.CopyTo (T [] array, int arrayIndex)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			int ICollection <T>.Count
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			bool ICollection <T>.IsReadOnly
			{
				get { throw new Exception ("The method or operation is not implemented."); }
			}

			bool ICollection <T>.Remove (T item)
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			#endregion

			#region IEnumerable <T> Members

			IEnumerator <T> IEnumerable <T>.GetEnumerator ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator ()
			{
				throw new Exception ("The method or operation is not implemented.");
			}

			#endregion
}
	}
}

