//
// Tests for System.Web.UI.WebControls.Repeater.cs 
//
// Author:
//	Chris Toshok (toshok@novell.com)
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

using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.SystemWeb.Framework;
using System.Collections;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class RepeaterTest {	
		class Poker : Repeater {
			
			public void TrackState () 
			{
				TrackViewState ();
			}
			
			public object SaveState ()
			{
				return SaveViewState ();
			}
			
			public void LoadState (object o)
			{
				LoadViewState (o);
			}

#if NET_2_0
			public DataSourceSelectArguments GetSelectArguments()
			{
				return SelectArguments;
			}

			public DataSourceSelectArguments DoCreateDataSourceSelectArguments ()
			{
				return base.CreateDataSourceSelectArguments();
			}

			public new void EnsureDataBound ()
			{
				base.EnsureDataBound ();
			}

			public global::System.Collections.IEnumerable DoGetData ()
			{
				return base.GetData ();
			}

			public new bool Initialized
			{
				get { return base.Initialized; }
			}

			public new bool IsBoundUsingDataSourceID
			{
				get { return base.IsBoundUsingDataSourceID; }
			}

			protected override void OnDataPropertyChanged ()
			{
				eventChecker = true;
				base.OnDataPropertyChanged ();
			}

			public void DoOnDataSourceViewChanged (object sender, EventArgs e)
			{
				base.OnDataSourceViewChanged (sender, e);
			}

			public new bool RequiresDataBinding
			{
				get { return base.RequiresDataBinding; }
				set { base.RequiresDataBinding = value; }
			}

			bool eventChecker;
			public bool EventChecker
			{
				get { return eventChecker; }
				set { throw new NotImplementedException (); }
			}

			public void clearEvents ()
			{
				eventChecker = false;
			}
#endif
		}

#if NET_2_0
		[Test]
		public void Repeater_DefaultsSelectArguments ()
		{
			Poker p = new Poker ();
			DataSourceSelectArguments args, args2;

			args = p.GetSelectArguments();
			args2 = p.DoCreateDataSourceSelectArguments();

			Assert.AreEqual (args, args2, "call == prop");

			Assert.IsNotNull (args, "property null check");
			Assert.IsTrue    (args != DataSourceSelectArguments.Empty, "property != Empty check");
			Assert.IsTrue    (args.Equals (DataSourceSelectArguments.Empty), "property but they are empty check");

			Assert.IsNotNull (args2, "method null check");
			Assert.IsTrue    (args2 != DataSourceSelectArguments.Empty, "method != Empty check");
			Assert.IsTrue    (args2.Equals (DataSourceSelectArguments.Empty), "method but they are empty check");

			/* check to see whether multiple calls give us different refs */
			args = p.DoCreateDataSourceSelectArguments();
			
			Assert.AreEqual (args, args2, "multiple calls, same ref");
			Assert.AreEqual (string.Empty, p.DataSourceID, "DataSourceID");
			Assert.AreEqual (false, p.RequiresDataBinding, "RequiresDataBinding");
		}

		[Test]
		public void Repeater_Defaults ()
		{
			Poker p = new Poker ();
			Assert.AreEqual (true, p.EnableTheming, "EnableTheming");
		}


		
		[Test]
		[Category("NunitWeb")]
		[ExpectedException(typeof(HttpException))]
		public void  EnsureDataBound ()
		{
			WebTest t = new WebTest (PageInvoker.CreateOnInit (EnsureDataBound_Init));
			string html = t.Run ();
		}

		public static void EnsureDataBound_Init (Page p)
		{
			Poker r = new Poker ();
			r.DataSourceID = "Fake";
			p.Form.Controls.Add (r);
			r.EnsureDataBound ();
		}

		[Test]
		public void GetData ()
		{
			Poker p = new Poker ();
			p.DataSource = Databound ();
			ArrayList data = (ArrayList)p.DoGetData ();
			Assert.AreEqual (3, data.Count, "GetData#1");
			Assert.AreEqual (1, data[0], "GetData#2");
		}

		#region help_class
		static ArrayList Databound ()
		{
			ArrayList list = new ArrayList ();
			list.Add (1);
			list.Add (2);
			list.Add (3);
			return list;
		}
		#endregion

		[Test]
		[Category ("NunitWeb")]
		public void Initialized ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Init = new PageDelegate (Initialized_Init);
			pd.Load = new PageDelegate (Initialized_Load);
			t.Invoker = new PageInvoker (pd);
			string html = t.Run ();
		}

		public static void Initialized_Init (Page p)
		{
			Poker r = new Poker ();
			r.ID = "Rep";
			Assert.AreEqual (false, r.Initialized, "Initialized#1");
			r.DataSource = Databound ();
			p.Form.Controls.Add (r);
		}

		public static void Initialized_Load (Page p)
		{
			Poker r = p.FindControl ("Rep") as Poker; 
			Assert.AreEqual (true, r.Initialized, "Initialized#2");
		}

		[Test]
		public void IsBoundUsingDataSourceID ()
		{
			Poker p = new Poker ();
			Assert.AreEqual (false, p.IsBoundUsingDataSourceID, "IsBoundUsingDataSourceID#1");
			p.DataSourceID = "Fake";
			Assert.AreEqual (true, p.IsBoundUsingDataSourceID, "IsBoundUsingDataSourceID#2");
		}

		[Test]
		public void OnDataPropertyChanged ()
		{
			Poker p = new Poker ();
			p.clearEvents ();
			p.DataSourceID = "Fake";
			Assert.AreEqual (true, p.EventChecker, "OnDataPropertyChanged#1");
		}

		[Test]
		public void OnDataSourceViewChanged ()
		{
			Poker p = new Poker ();
			Assert.AreEqual (false, p.RequiresDataBinding, "OnDataSourceViewChanged#1");
			p.DoOnDataSourceViewChanged (p, new EventArgs ());
			Assert.AreEqual (true, p.RequiresDataBinding, "OnDataSourceViewChanged#2");
		}

		#region help_class_for_select_args
		class PokerS : Repeater
		{

			public void TrackState ()
			{
				TrackViewState ();
			}

			public object SaveState ()
			{
				return SaveViewState ();
			}

			public void LoadState (object o)
			{
				LoadViewState (o);
			}

#if NET_2_0
			public DataSourceSelectArguments GetSelectArguments ()
			{
				return SelectArguments;
			}

			protected override DataSourceSelectArguments CreateDataSourceSelectArguments ()
			{
				DataSourceSelectArguments arg = new DataSourceSelectArguments ("SortExp");
				return arg;
			}
#endif
		}
		#endregion

		[Test]
		public void GetSelectArguments ()
		{
			PokerS p = new PokerS ();
			DataSourceSelectArguments arg = p.GetSelectArguments ();
			Assert.AreEqual ("SortExp", arg.SortExpression, "GetSelectArguments");
		}

		[TestFixtureTearDown]
		public void TearDown ()
		{
			WebTest.Unload ();
		}
#endif
	}
}
