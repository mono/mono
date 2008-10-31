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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingContextTest : TestHelper {

		private class BindingContextPoker : BindingContext {

			public int collection_changed;

			public void _Add (object data_source, BindingManagerBase list_manager)
			{
				Add (data_source, list_manager);
			}

			public void _AddCore (object data_source, BindingManagerBase list_manager)
			{
				AddCore (data_source, list_manager);
			}

			public void _Clear ()
			{
				Clear ();
			}

			public void _ClearCore ()
			{
				ClearCore ();
			}

			public void _Remove (object data_source)
			{
				Remove (data_source);
			}

			public void _RemoveCore (object data_source)
			{
				RemoveCore (data_source);
			}

			protected override void OnCollectionChanged (CollectionChangeEventArgs ce)
			{
				collection_changed = (int) ce.Action;
				base.OnCollectionChanged (ce);
			}
		}

		[Test]
		public void CtorTest ()
		{
			BindingContext bc = new BindingContext ();

			Assert.IsFalse (bc.IsReadOnly, "CT1");
			Assert.IsFalse (bc.Contains (this), "CT2");
			Assert.IsFalse (bc.Contains (this, String.Empty), "CT3");
			Assert.IsFalse (bc.Contains (this, "Me is String"), "CT4");

			// Test the ICollection interface
			ICollection ic = (ICollection) bc;
			Assert.AreEqual (ic.Count, 0, "CT5");
			Assert.IsFalse (ic.IsSynchronized, "CT6");
			Assert.IsNull (ic.SyncRoot, "CT7");
			object [] arr = new object [] { "A", "B", "C" };
			ic.CopyTo (arr, 0);
			Assert.AreEqual (0, ic.Count, "CT8");
			Assert.IsFalse (ic.GetEnumerator ().MoveNext (), "CT9");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestIndexerNull ()
		{
			BindingContext bc = new BindingContext ();
			BindingManagerBase a;

			a = bc [null];
			
			TestHelper.RemoveWarning (a, bc);
		}

		[Test]
		public void TestIndexerNoMember ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();
			BindingManagerBase a, b;

			data_source.AddRange (new string [] { "1", "2", "3", "4", "5" });

			a = bc [data_source];
			b = bc [data_source];
			Assert.AreSame (a, b, "INNM1");

			b = bc [data_source, String.Empty];
			Assert.AreSame (a, b, "INNM2");

			// Only one is added to the list
			Assert.AreEqual (((ICollection) bc).Count, 1);
		}

		[Test]
		public void TestIndexerWithMember ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();
			BindingManagerBase a, b, c, d;
			
			data_source.AddRange (new string [] { "1", "2", "3", "4", "5" });

			a = bc [data_source, "Length"];
			b = bc [data_source, "Length"];

			Assert.AreSame (a, b, "INWM1");

			b = bc [data_source];
			Assert.IsFalse (object.ReferenceEquals (a, b), "INWM2");

			c = bc [data_source];
			Assert.AreSame (b, c, "INWM3");
			
			b = bc [data_source, "Length"];
			Assert.AreSame (a, b, "INWM4");

			// Non List type
			MockItem item = new MockItem ("Mono", -1);
			MockContainer container = new MockContainer ();
			container.Item = item;

			d = bc [container, "Item.Text"];
			Assert.AreEqual ("Mono", d.Current, "INWM5");
			Assert.AreEqual (1, d.Count, "INWM6");
			Assert.AreEqual (0, d.Position, "INWM7");

			d = bc [container, "Item.Text.Length"];
			Assert.AreEqual (4, d.Current, "INWM8");
			Assert.AreEqual (1, d.Count, "INWM9");
			Assert.AreEqual (0, d.Position, "INWM10");
		}

#if NET_2_0
		[Test]
		public void TestIndexerICurrencyManagerProvider ()
		{
			BindingContext bc = new BindingContext ();
			BindingSource source = new BindingSource ();

			// This way the actual CurrencyManager instance is NOT added to
			// BindingContext
			CurrencyManager cm = (CurrencyManager) bc [source];
			Assert.AreSame (cm, source.CurrencyManager, "A1");
			Assert.AreEqual (false, bc.Contains (source), "A2");
			Assert.AreEqual (0, ((ICollection)bc).Count, "A3");
		}
#endif

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CantCreateChildList ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();

			BindingManagerBase a = bc [data_source, "Items"];
			
			TestHelper.RemoveWarning (a);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void CantCreateChildList2 ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();

			BindingManagerBase a = bc [data_source, "Count"];
			
			TestHelper.RemoveWarning (a);
		}

		[Test]
		public void CreateCurrencyManager ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();
			CurrencyManager a = bc [data_source] as CurrencyManager;

			Assert.IsNotNull (a, "CCM1");
		}

		[Test]
		public void CreatePropertyManager ()
		{
			BindingContext bc = new BindingContext ();
			object data_source = new object ();
			PropertyManager a = bc [data_source] as PropertyManager;

			Assert.IsNotNull (a, "CPM1");
		}

		private DataSet CreateRelatedDataSet ()
		{
			DataSet dataset = new DataSet ("DataSet");
			DataTable dt1 = new DataTable ("Table1");
			DataTable dt2 = new DataTable ("Table2");
			DataColumn column;

			column = new DataColumn ("One");
			column.DataType = typeof (int);
			column.Unique = true;
			dt1.Columns.Add (column);

			for (int i = 0; i < 10; i++) {
				DataRow row = dt1.NewRow ();
				row ["One"] = i;
				dt1.Rows.Add (row);
			}
			
			column = new DataColumn ("Two");
			column.DataType = typeof (int);
			column.Unique = true;
			dt2.Columns.Add (column);

			for (int i = 0; i < 10; i++) {
				DataRow row = dt2.NewRow ();
				row ["Two"] = i;
				dt2.Rows.Add (row);
			}

			dataset.Tables.Add (dt1);
			dataset.Tables.Add (dt2);
			dataset.Relations.Add ("Relation", dt1.Columns ["One"], dt2.Columns ["Two"]);

			return dataset;
		}

		[Test]
		public void CreateComplexManager ()
		{
			BindingContext bc = new BindingContext ();
			DataSet dataset = CreateRelatedDataSet ();
			CurrencyManager cm = bc [dataset, "Table1.Relation"] as CurrencyManager;

			Assert.IsNotNull (cm, "CCCM1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FailToCreateComplexManagerRelationDoesNotExist ()
		{
			BindingContext bc = new BindingContext ();
			DataSet dataset = CreateRelatedDataSet ();
			CurrencyManager cm = bc [dataset, "Table1.ImNotRelated"] as CurrencyManager;

			TestHelper.RemoveWarning (cm);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FailToCreateComplexManagerNoTableSpecified ()
		{
			BindingContext bc = new BindingContext ();
			DataSet dataset = CreateRelatedDataSet ();
			CurrencyManager cm = bc [dataset, "Relation"] as CurrencyManager;

			TestHelper.RemoveWarning (cm);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void FailToCreateComplexChildTableSpecified ()
		{
			BindingContext bc = new BindingContext ();
			DataSet dataset = CreateRelatedDataSet ();
			CurrencyManager cm = bc [dataset, "Table2.Relation"] as CurrencyManager;

			TestHelper.RemoveWarning (cm);
		}

		[Test]
		[ExpectedException (typeof (NotImplementedException))]
		public void CantSubscribeToCollectionChanged ()
		{
			BindingContext bc = new BindingContext ();

			bc.CollectionChanged += new CollectionChangeEventHandler (Dummy);
		}

		[Test]
		public void CantSubscribeToCollectionChanged2 ()
		{
			BindingContext bc = new BindingContext ();

			bc.CollectionChanged -= new CollectionChangeEventHandler (Dummy);
		}

		private void Dummy (object sender, CollectionChangeEventArgs e)
		{

		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNullDataSource ()
		{
			BindingContextPoker p = new BindingContextPoker ();

			p._Add (null, new PropertyManager ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void AddNullListManager ()
		{
			BindingContextPoker p = new BindingContextPoker ();

			p._Add (new object (), null);
		}

		[Test]
		public void Add ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p.collection_changed = -1;
			Assert.IsFalse (p.Contains (data_source), "ADD1");
			Assert.AreEqual (0, ((ICollection) p).Count, "ADD2");
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADD3");
			Assert.AreEqual (1, ((ICollection) p).Count, "ADD4");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Add, "ADD5");

			p.collection_changed = -1;
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADD6");
			Assert.AreEqual (1, ((ICollection) p).Count, "ADD7");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Add, "ADD8");

			p.collection_changed = -1;
			data_source = new object ();
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADD9");
			Assert.AreEqual (2, ((ICollection) p).Count, "ADD10");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Add, "ADD9");
		}

		[Test]
		public void AddCore ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p.collection_changed = -1;
			Assert.IsFalse (p.Contains (data_source), "ADDCORE1");
			Assert.AreEqual (0, ((ICollection) p).Count, "ADDCORE2");
			p._AddCore (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADDCORE3");
			Assert.AreEqual (1, ((ICollection) p).Count, "ADDCORE4");
			Assert.AreEqual (p.collection_changed, -1, "ADDCORE5");

			p.collection_changed = -1;
			p._AddCore (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADDCORE6");
			Assert.AreEqual (1, ((ICollection) p).Count, "ADDCORE7");
			Assert.AreEqual (p.collection_changed, -1, "ADDCORE8");

			p.collection_changed = -1;
			data_source = new object ();
			p._AddCore (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "ADDCORE9");
			Assert.AreEqual (2, ((ICollection) p).Count, "ADDCORE10");
			Assert.AreEqual (p.collection_changed, -1, "ADDCORE11");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void RemoveNull ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			p._Remove (null);
		}

		[Test]
		public void Remove ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p.collection_changed = -1;
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "REMOVE1");
			Assert.AreEqual (1, ((ICollection) p).Count, "REMOVE2");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Add, "REMOVE3");
			p._Remove (data_source);
			Assert.IsFalse (p.Contains (data_source), "REMOVE4");
			Assert.AreEqual (0, ((ICollection) p).Count, "REMOVE5");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Remove, "REMOVE6");

			// Double remove
			p.collection_changed = -1;
			p._Remove (data_source);
			Assert.IsFalse (p.Contains (data_source), "REMOVE7");
			Assert.AreEqual (0, ((ICollection) p).Count, "REMOVE8");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Remove, "REMOVE9");
		}

		[Test]
		public void RemoveCore ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p.collection_changed = -1;
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "REMOVECORE1");
			Assert.AreEqual (1, ((ICollection) p).Count, "REMOVECORE2");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Add, "REMOVECORE3");

			p.collection_changed = -1;
			p._RemoveCore (data_source);
			Assert.IsFalse (p.Contains (data_source), "REMOVECORE4");
			Assert.AreEqual (0, ((ICollection) p).Count, "REMOVECORE5");
			Assert.AreEqual (p.collection_changed, -1, "REMOVECORE6");

			// Double remove
			p.collection_changed = -1;
			p._Remove (data_source);
			Assert.IsFalse (p.Contains (data_source), "REMOVECORE7");
			Assert.AreEqual (0, ((ICollection) p).Count, "REMOVECORE8");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Remove, "REMOVECORE9");
		}

		[Test]
		public void Clear ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p._Add (data_source, new PropertyManager ());
			p.collection_changed = -1;
			p._Clear ();
			Assert.IsFalse (p.Contains (data_source), "CLEAR1");
			Assert.AreEqual (0, ((ICollection) p).Count, "CLEAR2");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Refresh, "CLEAR3");

			// Double clear
			p.collection_changed = -1;
			p._Clear ();
			Assert.IsFalse (p.Contains (data_source), "CLEAR1");
			Assert.AreEqual (0, ((ICollection) p).Count, "CLEAR2");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Refresh, "CLEAR3");
		}

		[Test]
		public void ClearCore ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();

			p._Add (data_source, new PropertyManager ());
			p.collection_changed = -1;
			p._Clear ();
			Assert.IsFalse (p.Contains (data_source), "CLEARCORE1");
			Assert.AreEqual (0, ((ICollection) p).Count, "CLEARCORE2");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Refresh, "CLEARCORE3");

			// Double clear
			p.collection_changed = -1;
			p._Clear ();
			Assert.IsFalse (p.Contains (data_source), "CLEARCORE4");
			Assert.AreEqual (0, ((ICollection) p).Count, "CLEARCORE5");
			Assert.AreEqual (p.collection_changed, (int) CollectionChangeAction.Refresh, "CLEARCORE6");
		}

		[Test]
		public void TestContains ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source), "#1");
			Assert.IsFalse (p.Contains ("nonexistent"), "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TestContainsNull ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			p.Contains (null);
		}

		[Test]
		public void TestContainsNull2 ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();
			p._Add (data_source, new PropertyManager ());
			Assert.IsTrue (p.Contains (data_source, null), "#1");
			Assert.IsFalse (p.Contains ("nonexistent", null), "#2");
		}


		[Test]
		public void TestGetEnumerator ()
		{
			BindingContextPoker p = new BindingContextPoker ();
			object data_source = new object ();
			PropertyManager pm = new PropertyManager ();
			p._Add (data_source, pm);
			IEnumerator e = ((IEnumerable) p).GetEnumerator ();
			Assert.IsNotNull (e, "#1");
			IDictionaryEnumerator de = e as IDictionaryEnumerator;
			Assert.IsNotNull (de, "#2");
			Assert.IsTrue (de.MoveNext (), "#3");
			// In .NET Key is its internal type.
			//Assert.AreEqual (data_source, de.Key, "#4");
			//Assert.AreEqual (pm, de.Value, "#5");
			Assert.IsFalse (de.MoveNext (), "#6");
		}
	}
}

