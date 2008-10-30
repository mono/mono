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

namespace Tests.System.Web.UI.WebControls
{
	internal sealed class ListViewPoker : ListView
	{
		EventRecorder recorder;

		void RecordEvent (string suffix)
		{
			if (recorder == null)
				return;

			recorder.Record (suffix);
		}

		public ListViewPoker ()
			: base ()
		{
		}
		
		public ListViewPoker (EventRecorder recorder)
		{
			this.recorder = recorder;
		}
		
		protected override void OnItemCanceling (ListViewCancelEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCanceling (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemCommand (ListViewCommandEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCommand (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemCreated (ListViewItemEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemCreated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDataBound (ListViewItemEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDataBound (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDeleted (ListViewDeletedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDeleted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemDeleting (ListViewDeleteEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemDeleting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemEditing (ListViewEditEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemEditing (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemInserted (ListViewInsertedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemInserted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemInserting (ListViewInsertEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemInserting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemUpdated (ListViewUpdatedEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemUpdated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnItemUpdating (ListViewUpdateEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnItemUpdating (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnLayoutCreated (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnLayoutCreated (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnPagePropertiesChanged (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnPagePropertiesChanged (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnPagePropertiesChanging (PagePropertiesChangingEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnPagePropertiesChanging (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSelectedIndexChanged (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSelectedIndexChanged (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSelectedIndexChanging (ListViewSelectEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSelectedIndexChanging (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSorted (EventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSorted (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnSorting (ListViewSortEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnSorting (e);
			RecordEvent ("Leave");
		}
	
		protected override void OnTotalRowCountAvailable (PageEventArgs e)
		{
			RecordEvent ("Enter");
			base.OnTotalRowCountAvailable (e);
			RecordEvent ("Leave");
		}

		public void DoSetPageProperties (int startRowIndex, int maximumRows, bool databind)
		{
			SetPageProperties (startRowIndex, maximumRows, databind);
		}

		public int GetMaximumRowsProperty ()
		{
			return MaximumRows;
		}

		public int GetStartRowIndexProperty ()
		{
			return StartRowIndex;
		}
	}
	
	[TestFixture]
	public class ListViewTest
	{
		[Test]
		public void ListView_InitialValues ()
		{
			ListViewPoker lvp = new ListViewPoker (null);

			Assert.AreEqual (-1, lvp.GetMaximumRowsProperty ());
			Assert.AreEqual (0, lvp.GetStartRowIndexProperty ());
		}
		
		[Test]
		public void ListView_SetPageProperties_Events ()
		{
			EventRecorder events = new EventRecorder ();
			ListViewPoker lvp = new ListViewPoker (events);

			// No events expected: databind is false
			events.Clear ();
			lvp.DoSetPageProperties (0, 1, false);

			// No events expected: startRowIndex and maximumRows don't change values
			events.Clear ();
			lvp.DoSetPageProperties (0, 1, true);
			Assert.AreEqual (0, events.Count, "#A1");
			
			// No events expected: startRowIndex changes, but databind is false
			events.Clear();
			lvp.DoSetPageProperties(1, 1, false);
			Assert.AreEqual (0, events.Count, "#A2");
			
			// No events expected: maximumRows changes, but databind is false
			events.Clear();
			lvp.DoSetPageProperties(1, 2, false);
			Assert.AreEqual (0, events.Count, "#A3");
			
			// No events expected: maximumRows and startRowIndex change but databind is
			// false
			events.Clear();
			lvp.DoSetPageProperties(3, 4, false);
			Assert.AreEqual (0, events.Count, "#A4");
			
			// Events expected: maximumRows and startRowIndex change and databind is
			// true
			events.Clear();
			lvp.DoSetPageProperties(5, 6, true);
			Assert.AreEqual (4, events.Count, "#A5");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A6");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A7");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A8");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A9");

			// Events expected: maximumRows changes and databind is true
			events.Clear();
			lvp.DoSetPageProperties(5, 7, true);
			Assert.AreEqual (4, events.Count, "#A10");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A11");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A12");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A13");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A14");

			// Events expected: startRowIndex changes and databind is true
			events.Clear();
			lvp.DoSetPageProperties(6, 7, true);
			Assert.AreEqual (4, events.Count, "#A15");
			Assert.AreEqual ("OnPagePropertiesChanging:Enter", events [0], "#A16");
			Assert.AreEqual ("OnPagePropertiesChanging:Leave", events [1], "#A17");
			Assert.AreEqual ("OnPagePropertiesChanged:Enter", events [2], "#A18");
			Assert.AreEqual ("OnPagePropertiesChanged:Leave", events [3], "#A19");
			
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_SetPageProperties_Parameters1 ()
		{
			ListViewPoker lvp = new ListViewPoker ();
			lvp.DoSetPageProperties (-1, 1, false);
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void ListView_SetPageProperties_Parameters2 ()
		{
			ListViewPoker lvp = new ListViewPoker ();
			lvp.DoSetPageProperties (0, 0, false);
		}
	}
}
#endif
