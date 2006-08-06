//
// Tests for System.Web.UI.WebControls.GridViewRowCollectionTest.cs
//
// Author:
//	Yoni Klein (yonik@mainsoft.com)
//
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


#if NET_2_0

using System;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using NUnit.Framework;


namespace MonoTests.System.Web.UI.WebControls
{
	class PokerGridViewRowCollection : GridViewRowCollection
	{
		public PokerGridViewRowCollection (ArrayList list)
			: base (list)
		{
			
		}
	}

	[TestFixture]
	public class GridViewRowCollectionTest
	{

		[Test]
		public void GridViewRowCollection_DefaultProperty ()
		{
			GridViewRowCollection collection = new GridViewRowCollection(new ArrayList());
			Assert.AreEqual (0, collection.Count, "Count");
			Assert.AreEqual (false, collection.IsSynchronized, "IsSynchronized");   //Always return false
		}

		[Test]
		public void GridViewRowCollection_DefaultPropertyNotWorking ()
		{
			GridViewRowCollection collection = new GridViewRowCollection (new ArrayList ());
			// Note : does not contain a definition for `IsReadOnly'
			//Assert.AreEqual (false, collection.IsReadOnly, "IsReadOnly");		//Always return false
			Assert.AreEqual (collection, collection.SyncRoot, "SyncRoot");
		}

		[Test]
		public void GridViewRowCollection_AssignProperty ()
		{
			ArrayList list = new ArrayList ();
			list.Add(new GridViewRow (0, 0, DataControlRowType.DataRow, DataControlRowState.Normal));
			GridViewRowCollection collection = new GridViewRowCollection (list);
			Assert.AreEqual (1, collection.Count, "Count");
			// Note : does not contain a definition for `IsReadOnly'
			//Assert.AreEqual (false, collection.IsReadOnly, "IsReadOnly");		//Always return false
			Assert.AreEqual (false, collection.IsSynchronized, "IsSynchronized");   //Always return false
			Assert.AreEqual (typeof(GridViewRow), collection[0].GetType (), "Item");
		}

		[Test]
		public void GridViewRowCollection_CopyTo ()
		{
			ArrayList list = new ArrayList ();
			list.Add (new GridViewRow (0, 0, DataControlRowType.DataRow, DataControlRowState.Normal));
			GridViewRowCollection collection = new GridViewRowCollection (list);
			GridViewRow[] rows = new GridViewRow[collection.Count];
			collection.CopyTo (rows, 0);
			Assert.AreEqual (collection.Count, rows.Length, "CopyToLenth");
			Assert.IsNotNull (rows[0], "CopyToTargetCreated");
			Assert.AreEqual (collection[0].GetType (), rows[0].GetType (), "CopyToTargetType");
		}

		[Test]
		public void GridViewRowCollection_GetEnumerator ()
		{
			ArrayList list = new ArrayList ();
			list.Add (new GridViewRow (0, 0, DataControlRowType.DataRow, DataControlRowState.Normal));
			GridViewRowCollection collection = new GridViewRowCollection (list);
			IEnumerator numerator = collection.GetEnumerator ();
			Assert.IsNotNull (numerator, "IEnumeratorCreated");
			numerator.Reset ();
			numerator.MoveNext ();
			Assert.AreEqual (numerator.Current, collection[0], "GetEnumeratorCurrent");
			Assert.AreEqual (typeof (GridViewRow), numerator.Current.GetType (), ""); 
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void GridViewRowCollection_ItemException ()
		{
			GridViewRowCollection collection = new GridViewRowCollection (new ArrayList ());
			Assert.AreEqual (null, collection[0], "Item");

		}
	}
}
#endif