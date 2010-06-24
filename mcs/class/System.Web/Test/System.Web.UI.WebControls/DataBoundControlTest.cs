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
using System.Web.UI.Adapters;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;
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
				//Console.WriteLine ("PerformSelect\n{0}", Environment.StackTrace);
				Assert.IsTrue (RequiresDataBinding);
				base.PerformSelect ();
				Assert.IsFalse (RequiresDataBinding);
			}

			protected override void PerformDataBinding (IEnumerable data)
			{
				Assert.IsFalse (RequiresDataBinding);
				base.PerformDataBinding (data);
			}

			public void DoValidateDataSource (object dataSource)
			{
				ValidateDataSource (dataSource);
			}
			
			public bool GetInitialized ()
			{
				return Initialized;
			}

			public bool GetRequiresDataBinding ()
			{
				return RequiresDataBinding;
			}
			
			public void SetRequiresDataBinding (bool value)
			{
				RequiresDataBinding = value;
			}

			public override void DataBind ()
			{
				Assert.IsTrue (RequiresDataBinding);
				base.DataBind ();
				Assert.IsFalse (RequiresDataBinding);
			}

			public void DoEnsureDataBound ()
			{
				Assert.IsTrue (RequiresDataBinding);
				EnsureDataBound ();
				Assert.IsFalse (RequiresDataBinding);
			}
		}

		class MyDataBoundControl : DataBoundControl
		{
			public int CreateDataSourceSelectArgumentsCalled;
			public DataSourceSelectArguments CreatedDataSourceSelectArguments;
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

			protected override DataSourceSelectArguments CreateDataSourceSelectArguments () {
				CreateDataSourceSelectArgumentsCalled++;
				CreatedDataSourceSelectArguments = base.CreateDataSourceSelectArguments ();
				return CreatedDataSourceSelectArguments;
			}

			public DataSourceSelectArguments GetSelectArguments () {
				return SelectArguments;
			}
			
			internal ControlAdapter controlAdapter;
			protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
			{
				return controlAdapter;
			}

		}

		[TestFixtureTearDown]
		public void Unload ()
		{
			WebTest.Unload ();
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
#if TARGET_JVM
		[Ignore ("TD #6665")]
#endif
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
		public void DataBoundControl_DataBindFlow3 () {
			Page p = new Page ();
			MyDataBoundControl dc = new MyDataBoundControl ();
			p.Controls.Add (dc);
			DataSourceSelectArguments arg1 = dc.GetSelectArguments ();
			Assert.AreEqual (1, dc.CreateDataSourceSelectArgumentsCalled, "CreateDataSourceSelectArgumentsCalled#1");
			dc.DataBind ();
			DataSourceSelectArguments argCreated2 = dc.CreatedDataSourceSelectArguments;
			DataSourceSelectArguments arg2 = dc.GetSelectArguments ();
			Assert.AreEqual (2, dc.CreateDataSourceSelectArgumentsCalled, "CreateDataSourceSelectArgumentsCalled#2");
			dc.DataBind ();
			DataSourceSelectArguments argCreated3 = dc.CreatedDataSourceSelectArguments;
			Assert.AreEqual (3, dc.CreateDataSourceSelectArgumentsCalled, "CreateDataSourceSelectArgumentsCalled#3");
			Assert.IsTrue (object.ReferenceEquals (argCreated2, arg2), "CreateDataSourceSelectArgumentsCalled#4");
		}
		
		[Test]
		public void Defaults ()
		{
			Poker p = new Poker ();
			Assert.AreEqual ("", p.DataMember, "A1");
			Assert.AreEqual ("", p.DataSourceID, "A2");
		}
		
		[Test]
		public void ValidateDataSource () {
			Poker p = new Poker ();
			// Allows null
			p.DoValidateDataSource (null);
		}

		// MSDN: The ConfirmInitState method sets the initialized state of the data-bound 
		// control. The method is called by the DataBoundControl class in its OnLoad method.
		[Test]
		[Category ("NunitWeb")]
		public void Initialized ()
		{
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Load = Initialized_Load;
			pd.PreRenderComplete = Initialized_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
		}

		public static void Initialized_Load (Page p)
		{
			Poker c = new Poker ();
			p.Form.Controls.Clear ();
			p.Form.Controls.Add (c);
			Assert.IsFalse (c.GetInitialized (), "Initialized_Load");
			Assert.IsFalse (c.GetRequiresDataBinding (), "RequiresDataBinding_Load");
		}

		public static void Initialized_PreRender (Page p)
		{
			Poker c = (Poker) p.Form.Controls [0];
			Assert.IsTrue (c.GetInitialized (), "Initialized_PreRender");
			Assert.IsTrue (c.GetRequiresDataBinding (), "RequiresDataBinding_PreRender");
		}
		
		[Test]
		[Category ("NunitWeb")]
		public void Initialized2 () {
			WebTest t = new WebTest ();
			PageDelegates pd = new PageDelegates ();
			pd.Init = Initialized2_Init;
			pd.Load = Initialized2_Load;
			pd.PreRenderComplete = Initialized2_PreRender;
			t.Invoker = new PageInvoker (pd);
			t.Run ();
		}

		public static void Initialized2_Init (Page p) {
			Poker c = new Poker ();
			p.Form.Controls.Clear ();
			p.Form.Controls.Add (c);
			Assert.IsFalse (c.GetInitialized (), "Initialized_Init");
			Assert.IsFalse (c.GetRequiresDataBinding (), "RequiresDataBinding_Init");
		}

		public static void Initialized2_Load (Page p) {
			Poker c = (Poker) p.Form.Controls [0];
			Assert.IsTrue (c.GetInitialized (), "Initialized_Load");
			Assert.IsTrue (c.GetRequiresDataBinding (), "RequiresDataBinding_Load");
			c.SetRequiresDataBinding (false);
		}

		public static void Initialized2_PreRender (Page p) {
			Poker c = (Poker) p.Form.Controls [0];
			Assert.IsTrue (c.GetInitialized (), "Initialized_PreRender");
			Assert.IsFalse (c.GetRequiresDataBinding (), "RequiresDataBinding_PreRender");
		}

		[Test]
		public void DataBind ()
		{
			Page p = new Page ();
			
			ObjectDataSource ods = new ObjectDataSource (typeof (Control).FullName, "ToString");
			ods.ID = "ObjectDataSource1";
			p.Controls.Add (ods);
			
			Poker c = new Poker ();
			c.DataSourceID = "ObjectDataSource1";
			c.SetRequiresDataBinding (true);
			p.Controls.Add (c);
			
			c.DataBind ();
		}

		[Test]
		public void EnsureDataBound ()
		{
			Page p = new Page ();

			ObjectDataSource ods = new ObjectDataSource (typeof (Control).FullName, "ToString");
			ods.ID = "ObjectDataSource1";
			p.Controls.Add (ods);

			Poker c = new Poker ();
			c.DataSourceID = "ObjectDataSource1";
			c.SetRequiresDataBinding (true);
			p.Controls.Add (c);
			
			c.DoEnsureDataBound ();
		}

		class MyControlAdapter : ControlAdapter
		{
		}
		
		class MyDataBoundControlAdapter : DataBoundControlAdapter
		{
			internal bool perform_data_binding_called;
			protected override void PerformDataBinding (IEnumerable data)
			{
				perform_data_binding_called = true;
			}
		}

		[Test]
		[Category ("NotDotNet")] // Adapter binding does work on .NET but not by calling ResolveAdapter
		public void PerformDataBinding_UsesAdapter ()
		{
			MyDataBoundControl c = new MyDataBoundControl ();
			MyDataBoundControlAdapter a = new MyDataBoundControlAdapter();;
			c.controlAdapter = a;
			c.DataBind ();
			Assert.IsTrue (a.perform_data_binding_called, "PerformDataBinding_UsesAdapter");
		}

		[Test]
		public void PerformDataBinding_WorksWithControlAdapter ()
		{
			MyDataBoundControl c = new MyDataBoundControl ();
			ControlAdapter a = new MyControlAdapter();;
			c.controlAdapter = a;
			c.DataBind ();
		}
	}
}
#endif
