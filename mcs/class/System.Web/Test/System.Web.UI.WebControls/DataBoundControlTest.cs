//
// Tests for System.Web.UI.WebControls.DataBoundControl.cs 
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
using System.Text;
using System.Collections;
using System.Data;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls
{
	[TestFixture]	
	public class DataBoundControlTest {	
		class Poker : DataBoundControl {
			protected override void PerformSelect () 
			{
				Console.WriteLine ("PerformSelect\n{0}", Environment.StackTrace);
			}
		}

		class MyDataBoundControl : DataBoundControl
		{
			private StringBuilder dataBindTrace = new StringBuilder ();
			public string DataBindTrace {
				get { return dataBindTrace.ToString (); }
			}

			public override void DataBind () {
				dataBindTrace = new StringBuilder ();
				dataBindTrace.Append ("[Start DataBind]");
				base.DataBind ();
				dataBindTrace.Append ("[End DataBind]");
			}

			protected override void PerformSelect () {
				dataBindTrace.Append ("[Start PerformSelect]");
				base.PerformSelect ();
				dataBindTrace.Append ("[End PerformSelect]");
			}
			
			protected override void PerformDataBinding (IEnumerable data) {
				dataBindTrace.Append ("[Start PerformDataBinding]");
				base.PerformDataBinding (data);
				dataBindTrace.Append ("[End PerformDataBinding]");
			}

			protected override void OnDataBinding (EventArgs e) {
				dataBindTrace.Append ("[Start OnDataBinding]");
				base.OnDataBinding (e);
				dataBindTrace.Append ("[End OnDataBinding]");
			}

			protected override void OnDataBound (EventArgs e) {
				dataBindTrace.Append ("[Start OnDataBound]");
				base.OnDataBound (e);
				dataBindTrace.Append ("[End OnDataBound]");
			}

			protected override DataSourceView GetData () {
				dataBindTrace.Append ("[Start GetData]");
				DataSourceView d = base.GetData ();
				dataBindTrace.Append ("[End GetData]");
				return d;
			}

			public DataSourceView DoGetData () {
				return GetData ();
			}

			public IDataSource DoGetDataSource () {
				return GetDataSource ();
			}

			public void DoEnsureDataBound () {
				EnsureDataBound();
			}
		}

		[Test]
		public void DataBoundControl_GetData () {
			Page p = new Page ();
			MyDataBoundControl dc = new MyDataBoundControl ();
			p.Controls.Add (dc);

			DataSourceView data = dc.DoGetData ();
			Assert.IsNotNull (data, "GetData");

			IDataSource dataSource = dc.DoGetDataSource ();
			Assert.IsNotNull (dataSource, "GetDataSource");
		}

		[Test]
		public void DataBoundControl_DataBindFlow () {
			Page p = new Page ();
			MyDataBoundControl dc = new MyDataBoundControl ();
			p.Controls.Add (dc);
			dc.DataBind ();
			string expected = "[Start DataBind][Start PerformSelect][Start OnDataBinding][End OnDataBinding][Start GetData][End GetData][Start PerformDataBinding][End PerformDataBinding][Start OnDataBound][End OnDataBound][End PerformSelect][End DataBind]";
			Assert.AreEqual (expected, dc.DataBindTrace, "DataBindFlow");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void DataBoundControl_DataBindFlow2 () {
			new WebTest (PageInvoker.CreateOnLoad (DataBoundControl_DataBindFlow2_Load)).Run ();
		}

		public static void DataBoundControl_DataBindFlow2_Load(Page p){
			MyDataBoundControl dc = new MyDataBoundControl ();
			p.Controls.Add (dc);
			dc.DataSourceID = "ObjectDataSource1";
			ObjectDataSource ods = new ObjectDataSource (typeof(Control).FullName, "ToString");
			ods.ID = "ObjectDataSource1";
			p.Controls.Add (ods);
			dc.DataBind ();
			string expected = "[Start DataBind][Start PerformSelect][Start GetData][End GetData][Start OnDataBinding][End OnDataBinding][Start PerformDataBinding][End PerformDataBinding][Start OnDataBound][End OnDataBound][End PerformSelect][End DataBind]";
			Assert.AreEqual (expected, dc.DataBindTrace, "DataBindFlow");
		}

		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();
			Assert.AreEqual ("", p.DataMember, "A1");
			Assert.AreEqual ("", p.DataSourceID, "A2");
		}
	}
}
#endif
