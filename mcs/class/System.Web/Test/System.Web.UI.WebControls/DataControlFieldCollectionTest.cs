//
// Tests for System.Web.UI.WebControls.DataControlFieldCollectionTest.cs
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
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;
using System.Data;



namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]
	public class DataControlFieldCollectionTest
	{
		private bool _fieldsChanged;
		public bool EventDone
		{
			get { return _fieldsChanged; }
			set { _fieldsChanged = value; }
		}

		private void ResetEvent()
		{
			 _fieldsChanged = false;
		}

		[Test]
		public void DataControlFieldCollection_DefaultProperty ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			Assert.AreEqual (0, collection.Count, "Count");
		}

		[Test]
		public void DataControlFieldCollection_Add ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField());
			Assert.AreEqual (1, collection.Count, "Add");
		}

		[Test]
		public void DataControlFieldCollection_Clear ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField ());
			Assert.AreEqual (1, collection.Count, "Add");
			collection.Clear ();
			Assert.AreEqual (0, collection.Count, "Clear");
		}

		[Test]
		public void DataControlFieldCollection_Clone ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField ());
			Assert.AreEqual (1, collection.Count, "Add");
			DataControlFieldCollection clone = collection.CloneFields ();
			Assert.AreEqual (1, clone.Count, "Clone");
		}

		[Test]
		public void DataControlFieldCollection_Contains ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			BoundField field = new BoundField ();
			collection.Add (field);
			bool result = collection.Contains (field);
			Assert.AreEqual (true, result, "Contains");
		}

		[Test]
		public void DataControlFieldCollection_CopyTo ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField ());
			DataControlField[] fields = new DataControlField[collection.Count];
			Array array = new DataControlField[collection.Count];
			collection.CopyTo (fields, 0);
			Assert.AreEqual (1, fields.Length, "CopyToDataControlField");
			collection.CopyTo (array, 0);
			Assert.AreEqual (1, array.Length, "CopyToArray");
		}

		[Test]
		public void DataControlFieldCollection_GetEnumerator ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField ());
			IEnumerator numerator = collection.GetEnumerator ();
			Assert.IsNotNull (numerator, "GetEnumerator");
			if (!(numerator is IEnumerator))
				Assert.Fail ("IEnumerator not been created");
		}

		[Test]
		public void DataControlFieldCollection_IndexOf ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			BoundField field = new BoundField ();
			int result;
			result = collection.IndexOf (field);
			Assert.AreEqual (-1, result, "NotExistFieldIndex");
			collection.Add (field);
			result = collection.IndexOf (field);
			Assert.AreEqual (0, result, "ExistFieldIndex");
		}

		[Test]
		public void DataControlFieldCollection_Insert ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			BoundField field = new BoundField ();
			collection.Add (new BoundField ());
			collection.Add (new BoundField ());
			Assert.AreEqual (2, collection.Count, "CollectionCount");
			collection.Insert (0, field);
			int result = collection.IndexOf (field);
			Assert.AreEqual (0, result, "Insert");
		}

		[Test]
		public void DataControlFieldCollection_Remove ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			BoundField field = new BoundField ();
			collection.Add (field);
			Assert.AreEqual (1, collection.Count, "CollectionCount");
 			collection.Remove(null);
			Assert.AreEqual (1, collection.Count, "RemoveNotExistField");
			collection.Remove (field);
			Assert.AreEqual (0, collection.Count, "RemoveExistField");
		}

		[Test]
		public void DataControlFieldCollection_RemoveAt ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.Add (new BoundField ());
			Assert.AreEqual (1, collection.Count, "CollectionCount");
			collection.RemoveAt (0);
			Assert.AreEqual (0, collection.Count, "RemoveAtIndex");
		}

		[Test]
		public void DataControlFieldCollection_FieldsChangedEvent ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.FieldsChanged += new EventHandler (collection_FieldsChanged);
			BoundField field = new BoundField ();
			collection.Add (field);
			Assert.AreEqual (true, EventDone, "FieldsChangedEvenAdd");
			ResetEvent ();
			collection.Clear ();
			Assert.AreEqual (true, EventDone, "FieldsChangedEvenClear");
			ResetEvent ();
			collection.Insert (0, field);
			Assert.AreEqual (true, EventDone, "FieldsChangedEvenInsert");
			ResetEvent ();
			collection.Remove (field);
			Assert.AreEqual (true, EventDone, "FieldsChangedEvenRemove");
		}

		private void collection_FieldsChanged (Object sender, EventArgs e)
		{
			EventDone = true;
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void DataControlFieldCollection_RemoveAtException ()
		{
			DataControlFieldCollection collection = new DataControlFieldCollection ();
			collection.RemoveAt (0);
		}
	}
}
#endif
