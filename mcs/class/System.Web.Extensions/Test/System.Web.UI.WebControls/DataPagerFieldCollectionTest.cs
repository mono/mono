//
// System.Web.UI.WebControls.ListView
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
//

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
#if NET_3_5
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	class DataPagerFieldCollectionPoker : DataPagerFieldCollection
	{
		EventRecorder recorder;

		void RecordEvent (string suffix)
		{
			if (recorder == null)
				return;

			recorder.Record (suffix);
		}

		public DataPagerFieldCollectionPoker ()
			: base (null)
		{
		}
		
		public DataPagerFieldCollectionPoker (DataPager pager)
			: base (pager)
		{
		}

		public DataPagerFieldCollectionPoker (DataPager pager, EventRecorder recorder)
			: base (pager)
		{
			this.recorder = recorder;
		}
		
		public Type[] DoGetKnownTypes ()
		{
			return base.GetKnownTypes ();
		}

		public object DoCreateKnownType (int index)
		{
			return CreateKnownType (index);
		}

		public void DoOnValidate (object value)
		{
			OnValidate (value);
		}

		public void CatchFieldsChangedEvent ()
		{
			FieldsChanged += new EventHandler (OnFieldsChanged);
		}
		
		protected override void OnValidate (object o)
		{
			RecordEvent ("Enter");
			base.OnValidate (o);
			RecordEvent ("Leave");
		}

		protected override void OnClearComplete ()
		{
			RecordEvent ("Enter");
			base.OnClearComplete ();
			RecordEvent ("Leave");
		}

		protected override void OnInsertComplete (int index, object value)
		{
			RecordEvent ("Enter");
			base.OnInsertComplete (index, value);
			RecordEvent ("Leave");
		}

		protected override void OnRemoveComplete (int index, object value)
		{
			RecordEvent ("Enter");
			base.OnRemoveComplete (index, value);
			RecordEvent ("Leave");
		}

		void OnFieldsChanged (object sender, EventArgs args)
		{
			RecordEvent ("Enter");
			RecordEvent ("Leave");
		}
	}
	
	[TestFixture]
	public class DataPagerCollectionTest
	{
		[Test]
		public void GetKnownTypes_Test ()
		{
			var coll = new DataPagerFieldCollectionPoker ();
			Type[] knownTypes = coll.DoGetKnownTypes ();

			Assert.AreEqual (3, knownTypes.Length, "#A1");
			Assert.AreEqual (typeof (NextPreviousPagerField), knownTypes [0], "#A2");
			Assert.AreEqual (typeof (NumericPagerField), knownTypes [1], "#A3");
			Assert.AreEqual (typeof (TemplatePagerField), knownTypes [2], "#A4");
		}

		[Test]
		public void CreateKnownTypes_Types ()
		{
			var coll = new DataPagerFieldCollectionPoker ();

			Assert.AreEqual (typeof (NextPreviousPagerField), coll.DoCreateKnownType (0).GetType (), "#A1");
			Assert.AreEqual (typeof (NumericPagerField), coll.DoCreateKnownType (1).GetType (), "#A2");
			Assert.AreEqual (typeof (TemplatePagerField), coll.DoCreateKnownType (2).GetType (), "#A3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void CreateKnownTypes_Arguments ()
		{
			var coll = new DataPagerFieldCollectionPoker ();
			coll.DoCreateKnownType (3);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void OnValidate_Arguments_Null ()
		{
			var coll = new DataPagerFieldCollectionPoker ();
			coll.DoOnValidate (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void OnValidate_Arguments_NotDataPagerField ()
		{
			var coll = new DataPagerFieldCollectionPoker ();
			coll.DoOnValidate (String.Empty);
		}

		[Test]
		public void DataPagerFieldCollection_Events ()
		{
			var events = new EventRecorder ();
			var coll = new DataPagerFieldCollectionPoker (new DataPager (), events);
			coll.CatchFieldsChangedEvent ();
			
			coll.Insert (0, new NextPreviousPagerField ());
			Assert.AreEqual (6, events.Count);
			Assert.AreEqual ("OnValidate:Enter", events [0], "#A1");
			Assert.AreEqual ("OnValidate:Leave", events [1], "#A2");
			Assert.AreEqual ("OnInsertComplete:Enter", events [2], "#A3");
			Assert.AreEqual ("OnFieldsChanged:Enter", events [3], "#A4");
			Assert.AreEqual ("OnFieldsChanged:Leave", events [4], "#A5");
			Assert.AreEqual ("OnInsertComplete:Leave", events [5], "#A6");

			events.Clear ();
			coll.Clear ();
			Assert.AreEqual (4, events.Count);
			Assert.AreEqual ("OnClearComplete:Enter", events [0], "#B1");
			Assert.AreEqual ("OnFieldsChanged:Enter", events [1], "#B2");
			Assert.AreEqual ("OnFieldsChanged:Leave", events [2], "#B3");
			Assert.AreEqual ("OnClearComplete:Leave", events [3], "#B4");

			NextPreviousPagerField field = new NextPreviousPagerField ();
			coll.Insert (0, field);
			events.Clear ();
			coll.Remove (field);
			Assert.AreEqual (8, events.Count);
			Assert.AreEqual ("OnValidate:Enter", events [0], "#C1");
			Assert.AreEqual ("OnValidate:Leave", events [1], "#C2");
			Assert.AreEqual ("OnValidate:Enter", events [2], "#C3");
			Assert.AreEqual ("OnValidate:Leave", events [3], "#C4");
			Assert.AreEqual ("OnRemoveComplete:Enter", events [4], "#C5");
			Assert.AreEqual ("OnFieldsChanged:Enter", events [5], "#C6");
			Assert.AreEqual ("OnFieldsChanged:Leave", events [6], "#C7");
			Assert.AreEqual ("OnRemoveComplete:Leave", events [7], "#C8");

			coll.Insert (0, field);
			events.Clear ();
			coll.RemoveAt (0);

			Assert.AreEqual (4, events.Count);
			Assert.AreEqual ("OnRemoveComplete:Enter", events [0], "#D1");
			Assert.AreEqual ("OnFieldsChanged:Enter", events [1], "#D2");
			Assert.AreEqual ("OnFieldsChanged:Leave", events [2], "#D3");
			Assert.AreEqual ("OnRemoveComplete:Leave", events [3], "#D4");
		}
		
	}
}
#endif