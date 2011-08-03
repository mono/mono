//
// Tests for System.Web.UI.WebControls.DataSourceViewTest.cs
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
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	public class PokerDataSourceView : DataSourceView
	{
		// View state Stuff
		public PokerDataSourceView (IDataSource owner , string name)
			: base(owner, name)
		{
			
		}

		public EventHandlerList events {
			get{
				return base.Events;
			}
		}

		public int DoExecuteDelete (IDictionary keys, IDictionary oldValues)
		{
			return base.ExecuteDelete (keys, oldValues);
		}

		public int DoExecuteInsert (IDictionary values)
		{
			return base.ExecuteInsert (values);
		}

		public int DoExecuteUpdate (IDictionary keys, IDictionary values, IDictionary oldValues)
		{
			return base.ExecuteUpdate (keys, values, oldValues);
		}

		public void DoOnDataSourceViewChanged (EventArgs e)
		{
			base.OnDataSourceViewChanged (e);
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public void DoRaiseUnsupportedCapabilityError (DataSourceCapabilities capability)
		{
			base.RaiseUnsupportedCapabilityError (capability);
		}
	}

	[TestFixture]
	public class DataSourceViewTest
	{
		
		[Test]
		public void DataSourceView_DefaultProperty ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			Assert.AreEqual (false, view.CanDelete, "CanDelete");
			Assert.AreEqual (false, view.CanInsert, "CanInsert");
			Assert.AreEqual (false, view.CanPage, "CanPage");
			Assert.AreEqual (false, view.CanRetrieveTotalRowCount, "CanRetrieveTotalRowCount");
			Assert.AreEqual (false, view.CanSort, "CanSort");
			Assert.AreEqual (false, view.CanUpdate, "CanUpdate");
			Assert.AreEqual ("View", view.Name, "Name");

			//protected properties
			EventHandlerList list = view.events;
			Assert.IsNotNull (list, "Events");
		}

		//Events

		[Test]
		public void DataSourceView_Events ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DataSourceViewChanged += new EventHandler (Eventchecker);
			view.DoOnDataSourceViewChanged (new EventArgs ());
			Eventassert ("DataSourceViewChanged");
		}

		[Test]
		public void DataSourceView_Events2 () {
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DataSourceViewChanged += new EventHandler (Eventchecker);
			ds.DoRaiseDataSourceChangedEvent (new EventArgs ());
			Eventassert ("DataSourceViewChanged");
		}

		[Test]
		public void DataSourceView_RaiseUnsupportedCapabilityError1 ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoRaiseUnsupportedCapabilityError (DataSourceCapabilities.None);
		}
		


		//Exceptions

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void DataSourceView_RaiseUnsupportedCapabilityError2 ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoRaiseUnsupportedCapabilityError (DataSourceCapabilities.Page);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceView_RaiseUnsupportedCapabilityError3 ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoRaiseUnsupportedCapabilityError (DataSourceCapabilities.RetrieveTotalRowCount);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceView_RaiseUnsupportedCapabilityError4 ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoRaiseUnsupportedCapabilityError (DataSourceCapabilities.Sort);
		}
		
	
		[Test]
		[ExpectedException(typeof(ArgumentNullException))]
		public void DataSourceView_Insert ()
		{
			// ExecuteInsert must be implemented at first 
			Hashtable table = new Hashtable ();
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			table.Add ("ID", "1000");
			view.Insert (table, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DataSourceView_Update ()
		{
			// ExecuteUpdate must be implemented at first 
			Hashtable table = new Hashtable ();
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			table.Add ("ID", "1000");
			view.Update (table, table, null, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DataSourceView_Delete()
		{
			// ExecuteDelete must be implemented at first
			Hashtable table = new Hashtable ();
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			table.Add ("ID", "1000");
			view.Delete(table, table,  null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DataSourceView_Select ()
		{
			// ExecuteSelect must be implemented at first
			Hashtable table = new Hashtable ();
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			table.Add ("ID", "1000");
			view.Select(new DataSourceSelectArguments(), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void DataSourceView_ConstractorException ()
		{
			PokerDataSourceView view = new PokerDataSourceView (null, "View");
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceView_ExecuteDelete ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoExecuteDelete (null, null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceView_ExecuteInsert ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoExecuteInsert (null);
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceView_ExecuteUpdate ()
		{
			PokerDataSource ds = new PokerDataSource ();
			PokerDataSourceView view = new PokerDataSourceView (ds, "View");
			view.DoExecuteUpdate (null, null, null);
		}

		
		// Helper for event checking
		private bool event_checker;

		private void Eventchecker (object o, EventArgs e)
		{
			event_checker = true;
		}

		private void Eventassert (string message)
		{
			Assert.IsTrue (event_checker, message);
			event_checker = false;
		}
	}
}
#endif
