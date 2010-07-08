//
// Tests for System.Web.UI.WebControls.DataSourceControlTest.cs
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
	public class PokerDataSource : DataSourceControl
	{
		// View state Stuff
		public PokerDataSource ()
			: base ()
		{
			TrackViewState ();
		}

		public ICollection  DoGetViewNames()
		{
			 return base.GetViewNames();
		}
		
		public ControlCollection DoCreateControlCollection ()
		{
			return base.CreateControlCollection ();
		}
		public void DoRaiseDataSourceChangedEvent (EventArgs e)
		{
			base.RaiseDataSourceChangedEvent (e);
		}

		protected override DataSourceView GetView (string viewName)
		{
			throw new Exception ("The method or operation is not implemented.");
		}
	}

	[TestFixture]
	public class DataSourceControlTest
	{
		
		[Test]
		public void DataSourceControl_DefaultProperty ()
		{
			PokerDataSource ds = new PokerDataSource ();
#if NET_4_0
			Assert.AreEqual (String.Empty, ds.ClientID, "ClientID");
#else
			Assert.AreEqual (null, ds.ClientID, "ClientID");
#endif
			Assert.IsNotNull (ds.Controls, "Controls#1");
			Assert.AreEqual ( 0 , ds.Controls.Count , "Controls#2");
			Assert.AreEqual (false, ds.Visible, "Visible");
		}

		[Test]
		public void DataSourceControl_DefaultPropertyNotWorking ()
		{
			PokerDataSource ds = new PokerDataSource ();
			Assert.AreEqual (false, ds.EnableTheming, "EnableTheming");
		}

		[Test]
		public void DataSourceControl_ApplyStyleSheetSkin ()
		{
			// DataSourceControl EnableTheme property always set to false 
			// and have no render issue - this method would do nothing
		}

		[Test]
		public void DataSourceControl_FindControl ()
		{
			// DataSourceControl cannot have child controls on ControlCollection
			// this method cannot be applyed
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceControl_Focus ()
		{
			PokerDataSource ds = new PokerDataSource ();
			ds.Focus ();
		}

		[Test]
		public void DataSourceControl_HasControls ()
		{
			//Always return false
			PokerDataSource ds = new PokerDataSource ();
			Assert.AreEqual (false, ds.HasControls (), "HasControls");
		}

		[Test]
		public void DataSourceControl_CreateControlCollection ()
		{
			PokerDataSource ds = new PokerDataSource ();
			ControlCollection collection = ds.DoCreateControlCollection ();
			Assert.IsNotNull (collection, "CreateControlCollection#1");
			Assert.AreEqual (0, collection.Count ,"CreateControlCollection#2");
		}

		[Test]
		public void DataSourceControl_GetViewNames ()
		{
			PokerDataSource ds = new PokerDataSource ();
			ICollection viewnames = ds.DoGetViewNames ();
			Assert.IsNull (viewnames, "GetViewNames#1");
		}
		

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void DataSourceControl_Controls ()
		{
			Button bt = new Button ();
			bt.ID = "bt";
			PokerDataSource ds = new PokerDataSource ();
			ds.Controls.Add (bt);
		}


		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceControl_EnableThemingException ()
		{
			PokerDataSource ds = new PokerDataSource ();
			ds.EnableTheming = true;
		}

		[Test]
		[ExpectedException (typeof (NotSupportedException))]
		public void DataSourceControl_VisibleException ()
		{
			PokerDataSource ds = new PokerDataSource ();
			ds.Visible = true;
		}

		//Events
		[Test]
		public void DataSourceControl_Events ()
		{
			PokerDataSource ds = new PokerDataSource ();
			((IDataSource) ds).DataSourceChanged += new EventHandler (Eventchecker);
			ds.DoRaiseDataSourceChangedEvent (new EventArgs ());
			Eventassert("RaiseDataSourceChangedEventFail");
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
