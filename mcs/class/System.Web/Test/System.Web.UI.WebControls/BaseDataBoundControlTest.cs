//
// Tests for System.Web.UI.WebControls.BaseDataBoundControl.cs 
//
// Author:
//	Chris Toshok (toshok@ximian.com)
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
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class BaseDataBoundControlTest {	
		class Poker : BaseDataBoundControl {
			
			public bool OnDataPropertyChangedCalled;
			public bool ValidateDataSourceCalled;

			public Poker () {
				TrackViewState ();
			}

			public object SaveState () {
				return SaveViewState ();
			}

			public void LoadState (object state) {
				LoadViewState (state);
			}

			protected override void PerformSelect () 
			{
				Assert.IsTrue (RequiresDataBinding);
				//Console.WriteLine ("PerformSelect\n{0}", Environment.StackTrace);
			}

			protected override void ValidateDataSource (object dataSource)
			{
				ValidateDataSourceCalled = true;
				//Console.WriteLine ("PerformSelect\n{0}", Environment.StackTrace);
			}

			public bool GetIsBoundUsingDataSourceID ()
			{
				return IsBoundUsingDataSourceID;
			}

			public bool GetInitialized ()
			{
				return Initialized;
			}

			protected override void OnDataPropertyChanged () {
				base.OnDataPropertyChanged ();
				OnDataPropertyChangedCalled = true;
			}

			public void SetRequiresDataBinding (bool val)
			{
				RequiresDataBinding = val;
			}

			public bool GetRequiresDataBinding ()
			{
				return RequiresDataBinding;
			}
			
			public override void DataBind ()
			{
				Assert.IsTrue (RequiresDataBinding);
				base.DataBind ();
				Assert.IsTrue (RequiresDataBinding);
			}

			public void DoEnsureDataBound ()
			{
				Assert.IsTrue (RequiresDataBinding);
				EnsureDataBound ();
				Assert.IsTrue (RequiresDataBinding);
			}
		}
		
		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();

			Assert.IsNull (p.DataSource, "A1");
			Assert.AreEqual ("", p.DataSourceID, "A2");
			Assert.IsFalse (p.GetIsBoundUsingDataSourceID(), "A3");
			Assert.IsFalse (p.GetInitialized(), "A4");
		}

		[Test]
		public void ViewState ()
		{
			Poker p = new Poker ();
			Poker copy = new Poker ();

			p.DataSourceID = "hi";
			object state = p.SaveState ();
			copy.LoadState (state);

			Assert.AreEqual ("hi", copy.DataSourceID, "A1");
		}

		[Test]
		public void OnDataPropertyChanged ()
		{
			Poker p = new Poker ();
			Assert.IsFalse (p.OnDataPropertyChangedCalled);

			p.DataSourceID = "hi";
			Assert.IsTrue (p.OnDataPropertyChangedCalled, "OnDataPropertyChanged: DataSourceID");
		}

		[Test]
		public void OnDataPropertyChanged2 ()
		{
			Poker p = new Poker ();
			Assert.IsFalse (p.OnDataPropertyChangedCalled);

			p.DataSource = null;
			Assert.IsTrue (p.OnDataPropertyChangedCalled, "OnDataPropertyChanged: DataSource");
		}

		[Test]
		public void DataBind ()
		{
			Poker p = new Poker ();
			p.DataSourceID = "DataSourceID";
			p.SetRequiresDataBinding (true);
			p.DataBind ();
		}

		[Test]
		public void EnsureDataBound ()
		{
			Poker p = new Poker ();
			p.DataSourceID = "DataSourceID";
			p.SetRequiresDataBinding (true);
			p.DoEnsureDataBound ();
		}

		[Test]
		public void DataSource_ValidateDataSource ()
		{
			Poker p = new Poker ();
			p.DataSource = null;
			Assert.AreEqual (false, p.ValidateDataSourceCalled);
			p.DataSource = new Object();
			Assert.AreEqual (true, p.ValidateDataSourceCalled);
		}
#if NET_4_0
		[Test]
		public void SupportsDisabledAttribute ()
		{
			var ver40 = new Version (4, 0);
			var ver35 = new Version (3, 5);
			var p = new Poker ();
			Assert.AreEqual (ver40, p.RenderingCompatibility, "#A1-1");
			Assert.IsFalse (p.SupportsDisabledAttribute, "#A1-2");

			p.RenderingCompatibility = new Version (3, 5);
			Assert.AreEqual (ver35, p.RenderingCompatibility, "#A2-1");
			Assert.IsTrue (p.SupportsDisabledAttribute, "#A2-2");
		}
#endif
	}
}
#endif
