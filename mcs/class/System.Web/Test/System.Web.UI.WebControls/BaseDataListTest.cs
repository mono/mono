//
// BaseDataListTest.cs
//	- Unit tests for System.Web.UI.WebControls.BaseDataList
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using NUnit.Framework;

namespace MonoTests.System.Web.UI.WebControls {

	public class TestBaseDataList : BaseDataList {

		private bool dataBindingCalled;


		public TestBaseDataList ()
			: base ()
		{
		}

		public string Tag {
			get { return base.TagName; }
		}

		public StateBag StateBag {
			get { return base.ViewState; }
		}
#if NET_2_0
		public bool IsDataBoundByDataSourceId {
			get { return base.IsBoundUsingDataSourceID; }
		}

		public bool IsInitialized {
			get { return base.Initialized; }
		}

		public bool RequiresDataBind {
			get { return base.RequiresDataBinding; }
		}

		public DataSourceSelectArguments Arguments {
			get { return base.SelectArguments; }
		}
#endif
		public void Add (object o)
		{
			base.AddParsedSubObject (o);
		}

		public string Render ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			HtmlTextWriter writer = new HtmlTextWriter (sw);
			base.Render (writer);
			return writer.InnerWriter.ToString ();
		}

		protected override void CreateControlHierarchy (bool useDataSource)
		{
		}

		protected override void PrepareControlHierarchy ()
		{
		}

		public void DoSelectedIndexChanged (EventArgs e)
		{
			OnSelectedIndexChanged (e);
		}
#if NET_2_0
		public DataSourceSelectArguments CreateArguments ()
		{
			return base.CreateDataSourceSelectArguments ();
		}

		public IEnumerable Data ()
		{
			return base.GetData ();
		}

		public void DataBindBool (bool raise)
		{
			DataBind (raise);
		}

		public void Ensure ()
		{
			base.EnsureDataBound ();
		}
#endif
		public bool DataBindingCalled {
			get { return dataBindingCalled; }
			set { dataBindingCalled = value; }
		}

		protected override void OnDataBinding (EventArgs e)
		{
			dataBindingCalled = true;
			base.OnDataBinding (e);
		}
#if NET_2_0
		private bool dataPropertyChangedCalled;
		private bool dataSourceViewChangedCalled;
		private bool initCalled;
		private bool loadCalled;
		private bool preRenderCalled;

		public bool DataPropertyChangedCalled {
			get { return dataPropertyChangedCalled; }
			set { dataPropertyChangedCalled = value; }
		}

		protected override void OnDataPropertyChanged ()
		{
			dataPropertyChangedCalled = true;
			base.OnDataPropertyChanged ();
		}

		public bool DataSourceViewChangedCalled {
			get { return dataSourceViewChangedCalled; }
			set { dataSourceViewChangedCalled = value; }
		}

		protected override void OnDataSourceViewChanged (object sender, EventArgs e)
		{
			dataSourceViewChangedCalled = true;
			base.OnDataSourceViewChanged (sender, e);
		}

		public void BaseOnDataSourceViewChanged (object sender, EventArgs e)
		{
			base.OnDataSourceViewChanged (sender, e);
		}

		public bool InitCalled {
			get { return initCalled; }
			set { initCalled = value; }
		}

		protected internal override void OnInit (EventArgs e)
		{
			initCalled = true;
			base.OnInit (e);
		}

		public void BaseOnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		public bool LoadCalled {
			get { return loadCalled; }
			set { loadCalled = value; }
		}

		protected internal override void OnLoad (EventArgs e)
		{
			loadCalled = true;
			base.OnLoad (e);
		}

		public void BaseOnLoad (EventArgs e)
		{
			base.OnLoad (e);
		}

		public bool PreRenderCalled {
			get { return preRenderCalled; }
			set { preRenderCalled = value; }
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			preRenderCalled = true;
			base.OnPreRender (e);
		}

		public void BaseOnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}
#endif
	}

	public class TestDataSource : IListSource {

		private ArrayList list;


		public TestDataSource (ArrayList al)
		{
			list = al;
		}


		public bool ContainsListCollection {
			get { return true; }
		}

		public IList GetList ()
		{
			return list;
		}
	}

#if NET_2_0
	public class Test2DataSource : WebControl, IDataSource {

		public DataSourceView GetView (string viewName)
		{
			return new Test2DataSourceView (this, String.Empty);
		}

		public ICollection GetViewNames ()
		{
			return null;
		}

		public event EventHandler DataSourceChanged;
	}

	public class Test2DataSourceView : DataSourceView {

		public Test2DataSourceView (IDataSource owner, string viewName)
			: base (owner, viewName)
		{
		}

		protected internal override IEnumerable ExecuteSelect (DataSourceSelectArguments arguments)
		{
			ArrayList al = new ArrayList (3);
			for (int i=0; i < 3; i++)
				al.Add (i+1);
			return al;
		}
	}
#endif

	[TestFixture]
	public class BaseDataListTest {

		private HtmlTextWriter GetWriter ()
		{
			StringWriter sw = new StringWriter ();
			sw.NewLine = "\n";
			return new HtmlTextWriter (sw);
		}

		// IEnumerable (and IList) based
		private IEnumerable GetData (int n)
		{
			ArrayList al = new ArrayList ();
			for (int i = 0; i < n; i++) {
				al.Add (i.ToString ());
			}
			return (IEnumerable) al;
		}

		// IListSource based
		private TestDataSource GetDataSource (int n)
		{
			return new TestDataSource ((ArrayList)GetData (n));
		}

		[Test]
		public void DefaultProperties ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual ("span", bdl.Tag, "TagName");

			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, bdl.StateBag.Count, "ViewState.Count-1");
			
			Assert.AreEqual (String.Empty, bdl.Caption, "Caption");
			Assert.AreEqual (TableCaptionAlign.NotSet, bdl.CaptionAlign, "CaptionAlign");
			Assert.AreEqual (-1, bdl.CellPadding, "CellPadding");
			Assert.AreEqual (0, bdl.CellSpacing, "CellSpacing");
			Assert.AreEqual (0, bdl.Controls.Count, "Controls.Count");
			Assert.AreEqual (String.Empty, bdl.DataKeyField, "DataKeyField");
			Assert.AreEqual (String.Empty, bdl.DataMember, "DataMember");
			Assert.IsNull (bdl.DataSource, "DataSource");
			Assert.AreEqual (GridLines.Both, bdl.GridLines, "GridLines");
			Assert.AreEqual (HorizontalAlign.NotSet, bdl.HorizontalAlign, "HorizontalAlign");
			Assert.IsFalse (bdl.UseAccessibleHeader, "UseAccessibleHeader");
#if NET_2_0
			Assert.AreEqual (String.Empty, bdl.DataSourceID, "DataSourceID");
#endif
			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-2");
			Assert.AreEqual (0, bdl.StateBag.Count, "ViewState.Count-2");

			Assert.AreEqual (0, bdl.DataKeys.Count, "DataKeys.Count");
			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-3");
			// triggered by DataKeys, which makes DataKeysArray store its value.
			Assert.AreEqual (1, bdl.StateBag.Count, "ViewState.Count-3");
			Assert.AreEqual (typeof (ArrayList), bdl.StateBag ["DataKeys"].GetType (), "ViewState.Value-1");
		}

		[Test]
		public void NullProperties ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (0, bdl.StateBag.Count, "ViewState.Count-1");

			bdl.Caption = null;
			Assert.AreEqual (String.Empty, bdl.Caption, "Caption");
			bdl.CaptionAlign = TableCaptionAlign.NotSet;
			Assert.AreEqual (TableCaptionAlign.NotSet, bdl.CaptionAlign, "CaptionAlign");
			Assert.AreEqual (1, bdl.StateBag.Count, "ViewState.Count-2");

			// many properties can't be set without causing a InvalidCastException

			bdl.DataKeyField = null;
			Assert.AreEqual (String.Empty, bdl.DataKeyField, "DataKeyField");
			bdl.DataMember = null;
			Assert.AreEqual (String.Empty, bdl.DataMember, "DataMember");
			bdl.DataSource = null;
			Assert.IsNull (bdl.DataSource, "DataSource");
			bdl.UseAccessibleHeader = false;
			Assert.IsFalse (bdl.UseAccessibleHeader, "UseAccessibleHeader");
#if NET_2_0
			bdl.DataSourceID = String.Empty;
			Assert.AreEqual (String.Empty, bdl.DataSourceID, "DataSourceID");
			Assert.AreEqual (3, bdl.StateBag.Count, "ViewState.Count-3");
#else
			Assert.AreEqual (2, bdl.StateBag.Count, "ViewState.Count-3");
#endif
			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-2");
		}

		[Test]
		public void CleanProperties ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();

			bdl.Caption = "Mono";
			Assert.AreEqual ("Mono", bdl.Caption, "Caption");
			bdl.CaptionAlign = TableCaptionAlign.Top;
			Assert.AreEqual (TableCaptionAlign.Top, bdl.CaptionAlign, "CaptionAlign");
			// many properties can't be set without causing a InvalidCastException
			bdl.DataKeyField = "key";
			Assert.AreEqual ("key", bdl.DataKeyField, "DataKeyField");
			bdl.DataMember = "member";
			Assert.AreEqual ("member", bdl.DataMember, "DataMember");
			bdl.DataSource = GetData (2);
			Assert.IsNotNull (bdl.DataSource, "DataSource");
			bdl.UseAccessibleHeader = true;
			Assert.IsTrue (bdl.UseAccessibleHeader, "UseAccessibleHeader");

			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-1");
			Assert.AreEqual (5, bdl.StateBag.Count, "ViewState.Count-1");

			bdl.Caption = null;
			Assert.AreEqual (String.Empty, bdl.Caption, "-Caption");
			bdl.CaptionAlign = TableCaptionAlign.NotSet;
			Assert.AreEqual (TableCaptionAlign.NotSet, bdl.CaptionAlign, "-CaptionAlign");
			// many properties can't be set without causing a InvalidCastException
			bdl.DataKeyField = null;
			Assert.AreEqual (String.Empty, bdl.DataKeyField, "-DataKeyField");
			bdl.DataMember = null;
			Assert.AreEqual (String.Empty, bdl.DataMember, "-DataMember");
			bdl.DataSource = null;
			Assert.IsNull (bdl.DataSource, "-DataSource");
			bdl.UseAccessibleHeader = false;
			Assert.IsFalse (bdl.UseAccessibleHeader, "-UseAccessibleHeader");

			Assert.AreEqual (0, bdl.Attributes.Count, "Attributes.Count-2");
			// CaptionAlign and UseAccessibleHeader aren't removed
			Assert.AreEqual (2, bdl.StateBag.Count, "ViewState.Count-2");
		}

		[Test]
		public void TableCaption ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			foreach (TableCaptionAlign tca in Enum.GetValues (typeof (TableCaptionAlign))) {
				bdl.CaptionAlign = tca;
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]
		public void TableCaption_Int32MaxValue ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.CaptionAlign = (TableCaptionAlign)Int32.MaxValue;
		}

		[Test]
		public void DataSource_IEnumerable ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.DataSource = GetData (2);
			Assert.IsNotNull (bdl.DataSource, "DataSource");
		}

		[Test]
		public void DataSource_IListSource ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.DataSource = GetDataSource (3);
			Assert.IsNotNull (bdl.DataSource, "DataSource");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataSource_Other ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.DataSource = new object ();
		}

		// CreateControlStyle isn't overriden so we can't assign it's properties

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void CellPadding_InvalidCastException ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (-1, bdl.CellPadding, "CellPadding");
			bdl.CellPadding = -1;
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void CellSpacing_InvalidCastException ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (0, bdl.CellSpacing, "CellSpacing");
			bdl.CellSpacing = 0;
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void GridLines_InvalidCastException ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (GridLines.Both, bdl.GridLines, "GridLines");
			bdl.GridLines = GridLines.Both;
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))]
		public void HorizontalAlign_InvalidCastException ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (HorizontalAlign.NotSet, bdl.HorizontalAlign, "HorizontalAlign");
			bdl.HorizontalAlign = HorizontalAlign.NotSet;
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void IsBindableType_Null ()
		{
			BaseDataList.IsBindableType (null);
		}

		[Test]
		public void AddParsedSubObject ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.Add (null);
			bdl.Add (new LiteralControl ("mono"));
			bdl.Add (new DataListItem (0, ListItemType.Item));
			bdl.Add (String.Empty);
			bdl.Add (new Control ());
			Assert.AreEqual (0, bdl.Controls.Count, "Controls");
			Assert.AreEqual (0, bdl.StateBag.Count, "StateBag");
		}

		[Test]
		public void Render_Empty ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.AreEqual (String.Empty, bdl.Render ());
		}

		[Test]
		public void Render ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.DataSource = GetDataSource (3);
			bdl.DataBind ();
			Assert.AreEqual (String.Empty, bdl.Render ());
		}

		private bool selectedIndexChangedEvent;

		private void SelectedIndexChangedHandler (object sender, EventArgs e)
		{
			selectedIndexChangedEvent = true;
		}

		[Test]
		public void Events ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			selectedIndexChangedEvent = false;
			bdl.SelectedIndexChanged += new EventHandler (SelectedIndexChangedHandler);
			bdl.DoSelectedIndexChanged (new EventArgs ());
			Assert.IsTrue (selectedIndexChangedEvent, "selectedIndexChangedEvent");
		}

		[Test]
		public void OnDataBinding ()
		{
			// does DataBind calls base.OnDataBinding (like most sample do)
			// or does it call the overriden OnDataBinding (which seems logical)
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.DataBindingCalled, "Before DataBind");
			bdl.DataSource = GetDataSource (3);
			bdl.DataBind ();
			Assert.IsTrue (bdl.DataBindingCalled, "After DataBind");
		}
#if NET_2_0
		[Test]
		public void DataSourceID ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID-1");
			bdl.DataSourceID = "mono";
			Assert.IsTrue (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID-2");
			bdl.DataBind ();
			Assert.IsTrue (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID-3");
		}

		[Test]
		// LAMESPEC ??? [ExpectedException (typeof (HttpException))]
		[ExpectedException (typeof (InvalidOperationException))]
		public void DataSourceID_WithDataSource ()
		{
			Page p = new Page ();
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.Page = p;
			bdl.DataSource = GetDataSource (1);

			Test2DataSource ds = new Test2DataSource ();
			ds.ID = "mono";
			ds.Page = p;
			p.Controls.Add (ds);
			p.Controls.Add (bdl);
			bdl.DataSourceID = "mono";
			Assert.IsNotNull (bdl.Data (), "GetData");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		[Ignore ("LAMESPEC -and/or- bad test")]
		public void DataSource_WithDataSourceID ()
		{
			Test2DataSource ds = new Test2DataSource ();
			ds.ID = "mono";
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			ds.Page = p;
			p.Controls.Add (ds);
			p.Controls.Add (bdl);
			bdl.DataSourceID = "mono";
			Assert.IsNotNull (bdl.Data (), "GetData");

			bdl.DataSource = GetDataSource (1);
			bdl.DataBind ();
		}

		[Test]
		public void EnsureDataBound_WithoutDataSourceID ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.DataBindingCalled, "Before EnsureDataBound");
			bdl.Ensure ();
			Assert.IsFalse (bdl.DataBindingCalled, "After EnsureDataBound");
		}

		[Test]
		public void EnsureDataBound_WithDataSourceID ()
		{
			XmlDataSource ds = new XmlDataSource ();
			ds.Data = "";
			ds.ID = "mono";
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			p.Controls.Add (ds);
			p.Controls.Add (bdl);
			bdl.DataSourceID = "mono";

			Assert.IsFalse (bdl.DataBindingCalled, "Before EnsureDataBound");
			bdl.Ensure ();
			Assert.IsFalse (bdl.DataBindingCalled, "After EnsureDataBound");

			bdl.BaseOnLoad (EventArgs.Empty);
			bdl.Ensure ();
			Assert.IsTrue (bdl.DataBindingCalled, "After BaseOnLoad|RequiresDataBinding");
		}

		[Test]
		public void GetData ()
		{
			Test2DataSource ds = new Test2DataSource ();
			ds.ID = "mono";
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			ds.Page = p;
			p.Controls.Add (ds);
			p.Controls.Add (bdl);
			bdl.DataSourceID = "mono";
			Assert.IsNotNull (bdl.Data (), "GetData");
		}

		[Test]
		public void GetData_WithoutDataSourceID ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsNull (bdl.Data (), "GetData");
		}

		[Test]
		[ExpectedException (typeof (HttpException))]
		public void GetData_WithUnexistingDataSourceID ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.Page = new Page ();
			bdl.DataSourceID = "mono";
			bdl.Data ();
		}

		[Test]
		public void OnDataBinding_True ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.DataBindingCalled, "Before DataBind");
			bdl.DataSource = GetDataSource (3);
			bdl.DataBindBool (true);
			Assert.IsTrue (bdl.DataBindingCalled, "After DataBind");
		}

		[Test]
		public void OnDataBinding_False ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.DataBindingCalled, "Before DataBind");
			bdl.DataSource = GetDataSource (3);
			bdl.DataBindBool (false);
			Assert.IsFalse (bdl.DataBindingCalled, "After DataBind");
		}

		[Test]
		public void OnDataPropertyChanged ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.DataPropertyChangedCalled = false;
			bdl.DataMember = String.Empty;
			Assert.IsTrue (bdl.DataPropertyChangedCalled, "OnDataPropertyChanged-DataMember");
			Assert.IsFalse (bdl.IsInitialized, "Initialized-DataMember");

			bdl.DataPropertyChangedCalled = false;
			bdl.DataSource = null;
			Assert.IsTrue (bdl.DataPropertyChangedCalled, "OnDataPropertyChanged-DataSource");
			Assert.IsFalse (bdl.IsInitialized, "Initialized-DataSource");

			bdl.DataPropertyChangedCalled = false;
			bdl.DataSourceID = String.Empty;
			Assert.IsTrue (bdl.DataPropertyChangedCalled, "OnDataPropertyChanged-DataSourceID");
			Assert.IsFalse (bdl.IsInitialized, "Initialized-DataSourceID");
		}

		[Test]
		public void OnInit ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.IsInitialized, "Initialized-1");
			bdl.BaseOnInit (EventArgs.Empty);
			Assert.IsFalse (bdl.IsInitialized, "Initialized-2");
			// OnInit doesn't set Initialized to true
		}

		[Test]
		public void OnDataSourceViewChanged ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Assert.IsFalse (bdl.RequiresDataBind, "RequiresDataBind-1");
			bdl.BaseOnDataSourceViewChanged (this, EventArgs.Empty);
			Assert.IsTrue (bdl.RequiresDataBind, "RequiresDataBind-2");
		}

		[Test]
		public void OnLoad_WithoutPage ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();

			Assert.IsFalse (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID");
			Assert.IsTrue (bdl.EnableViewState, "EnabledViewState");
			Assert.IsNull (bdl.Page, "Page");
			bdl.BaseOnLoad (EventArgs.Empty);
			Assert.IsTrue (bdl.IsInitialized, "IsInitialized");
			Assert.IsFalse (bdl.RequiresDataBind, "RequiresDataBind");
		}

		[Test]
		public void OnLoad_WithoutPageWithoutViewState ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			bdl.EnableViewState = false;

			Assert.IsFalse (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID");
			Assert.IsFalse (bdl.EnableViewState, "EnabledViewState");
			Assert.IsNull (bdl.Page, "Page");
			bdl.BaseOnLoad (EventArgs.Empty);
			Assert.IsTrue (bdl.IsInitialized, "IsInitialized");
			Assert.IsFalse (bdl.RequiresDataBind, "RequiresDataBind");
		}

		[Test]
		public void OnLoad_WithPage ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			Assert.IsFalse (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID-2");
			Assert.IsTrue (bdl.EnableViewState, "EnabledViewState-2");
			Assert.IsFalse (bdl.Page.IsPostBack, "IsPostBack-2");
			bdl.BaseOnLoad (EventArgs.Empty);
			Assert.IsTrue (bdl.IsInitialized, "IsInitialized-2");
			Assert.IsTrue (bdl.RequiresDataBind, "RequiresDataBind-2");
		}

		[Test]
		public void OnLoad_WithPageWithoutViewState ()
		{
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			bdl.EnableViewState = false;
			Assert.IsFalse (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID");
			Assert.IsFalse (bdl.EnableViewState, "EnabledViewState");
			Assert.IsFalse (bdl.Page.IsPostBack, "IsPostBack");
			bdl.BaseOnLoad (EventArgs.Empty);
			Assert.IsTrue (bdl.IsInitialized, "IsInitialized");
			Assert.IsTrue (bdl.RequiresDataBind, "RequiresDataBind");
		}

		[Test]
		public void OnLoad_WithDataSource ()
		{
			XmlDataSource ds = new XmlDataSource ();
			ds.ID = "mono";
			TestBaseDataList bdl = new TestBaseDataList ();
			Page p = new Page ();
			bdl.Page = p;
			p.Controls.Add (ds);
			p.Controls.Add (bdl);
			bdl.DataSourceID = "mono";
			Assert.IsTrue (bdl.IsDataBoundByDataSourceId, "IsBoundUsingDataSourceID");
			Assert.IsTrue (bdl.EnableViewState, "EnabledViewState");
			Assert.IsFalse (bdl.Page.IsPostBack, "IsPostBack");
			bdl.BaseOnLoad (EventArgs.Empty);
			Assert.IsTrue (bdl.IsInitialized, "IsInitialized");
			Assert.IsTrue (bdl.RequiresDataBind, "RequiresDataBind");
		}
#endif
		[Test]
		public void IsBindableType ()
		{
			// documented
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (bool)), "bool");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (byte)), "byte");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (sbyte)), "sbyte");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (short)), "short");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (ushort)), "ushort");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (int)), "int");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (uint)), "uint");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (long)), "long");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (ulong)), "ulong");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (char)), "char");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (double)), "double");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (float)), "float");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (DateTime)), "DateTime");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (decimal)), "decimal");
			Assert.IsTrue (BaseDataList.IsBindableType (typeof (string)), "string");
			// and others (from TypeCode)
			Assert.IsFalse (BaseDataList.IsBindableType (typeof (object)), "object");
			Assert.IsFalse (BaseDataList.IsBindableType (typeof (DBNull)), "DBNull");
			// and junk
			Assert.IsFalse (BaseDataList.IsBindableType (this.GetType ()), "this");
		}
#if NET_4_0
		[Test]
		public void SupportsDisabledAttribute ()
		{
			var ver40 = new Version (4, 0);
			var ver35 = new Version (3, 5);
			var p = new TestBaseDataList ();
			Assert.AreEqual (ver40, p.RenderingCompatibility, "#A1-1");
			Assert.IsFalse (p.SupportsDisabledAttribute, "#A1-2");

			p.RenderingCompatibility = new Version (3, 5);
			Assert.AreEqual (ver35, p.RenderingCompatibility, "#A2-1");
			Assert.IsTrue (p.SupportsDisabledAttribute, "#A2-2");
		}
#endif
	}
}
