//
// Tests for System.Web.UI.WebControls.FormView.cs 
//
// Author:
//	Merav Sudri (meravs@mainsoft.com)
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
//

#if NET_2_0

using NUnit.Framework;
using System;
using System.Data;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;


namespace MonoTests.System.Web.UI.WebControls
{
	

	[TestFixture]	
	public class DetailsViewRowCollectionTest
	{
		
		ArrayList myArr = new ArrayList ();
		[TestFixtureSetUp]
		public void setup ()
		{
			myArr.Add ("Item1");
			myArr.Add ("Item2");
			myArr.Add ("Item3");
			myArr.Add ("Item4");
			myArr.Add ("Item5");
			myArr.Add ("Item6");			
		}

		//properties

		[Test]
		public void DetailsViewRowCollection_Properties ()
		{
			DetailsViewRowCollection coll = new DetailsViewRowCollection (myArr);
			Assert.AreEqual (6, coll.Count, "CountProeprty");
			Assert.AreEqual (false, coll.IsReadOnly, "IsReadOnlyProperty");			
			Assert.AreEqual (false, coll.IsSynchronized, "IsSynchronizedProperty");
			Assert.AreEqual (typeof (DetailsViewRowCollection), coll.SyncRoot.GetType() , "SyncRootProperty");
			Assert.AreEqual (coll, coll.SyncRoot, "SyncRoot2");			   
		}
		

		//Public Methods

		[Test]
		public void DetailsViewRowCollection_CopyTo ()
		{
			DetailsViewRow[] rows = new DetailsViewRow[6];
			ArrayList myRows = new ArrayList ();
			myRows.Add (new DetailsViewRow (0, DataControlRowType.DataRow, DataControlRowState.Insert));
			myRows.Add (new DetailsViewRow (1, DataControlRowType.Footer, DataControlRowState.Edit));
			myRows.Add (new DetailsViewRow (2, DataControlRowType.Header, DataControlRowState.Normal)); 			
			DetailsViewRowCollection coll = new DetailsViewRowCollection (myRows);						
			coll.CopyTo(rows,0);			
			Assert.AreEqual (6, rows.Length, "CopyTo1");
			Assert.AreEqual (0, rows[0].RowIndex, "CopyTo2");
			Assert.AreEqual (DataControlRowType.Footer , rows[1].RowType , "CopyTo3");
			Assert.AreEqual (DataControlRowState.Normal, rows[2].RowState, "CopyTo4");
			Assert.AreEqual (2, rows[2].RowIndex, "CopyTo5");
			Assert.AreEqual (null, rows[3], "CopyTo6");
			
		}

		[Test]
		public void DetailsViewRowCollection_GetEnumerator ()
		{
			DetailsViewRowCollection coll = new DetailsViewRowCollection (myArr);
			IEnumerator e=coll.GetEnumerator ();
			e.MoveNext ();
			Assert.AreEqual ("Item1", e.Current, "GetEnumerator1");
			Assert.AreEqual ("True", e.MoveNext ().ToString (), "GetEnumerator2");
			Assert.AreEqual ("Item2", e.Current, "GetEnumerator3");			 
 
		}
		
	}
}

#endif

