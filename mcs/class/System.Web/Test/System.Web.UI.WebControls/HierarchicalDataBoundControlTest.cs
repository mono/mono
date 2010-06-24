//
// Tests for System.Web.UI.WebControls.HierarchicalDataBoundControl.cs
//
// Author:
//	Igor Zelmanovich (igorz@mainsoft.com)
//
//
// Copyright (C) 2006 Mainsoft, Inc (http://www.mainsoft.com)
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


using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.Adapters;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.Adapters;
using System.IO;
using System.Drawing;
using System.Threading;
using MyWebControl = System.Web.UI.WebControls;
using System.Collections;
using MonoTests.SystemWeb.Framework;
using MonoTests.stand_alone.WebHarness;
using System.Text.RegularExpressions;
using System.Reflection;

namespace MonoTests.System.Web.UI.WebControls
{
	[Serializable]
	[TestFixture]
	public class HierarchicalDataBoundControlTest
	{
		class MyHierarchicalDataBoundControl : HierarchicalDataBoundControl
		{
			private StringBuilder dataBindTrace;
			public string DataBindTrace {
				get { return dataBindTrace.ToString (); }
			}

			public override void DataBind () {
				dataBindTrace = new StringBuilder ();
				dataBindTrace.Append("[Start DataBind]");
				base.DataBind ();
				dataBindTrace.Append ("[End DataBind]");
			}

			protected override void PerformSelect () {
				dataBindTrace.Append ("[Start PerformSelect]");
				base.PerformSelect ();
				dataBindTrace.Append ("[End PerformSelect]");
			}

			protected override void PerformDataBinding () {
				dataBindTrace.Append ("[Start PerformDataBinding]");
				base.PerformDataBinding ();
				dataBindTrace.Append ("[End PerformDataBinding]");
			}

			public HierarchicalDataSourceView GetData (string path) {
				dataBindTrace.Append ("[Start GetData]");
				return base.GetData (path);
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

			internal void DoValidateDataSource (object dataSource)
			{
				ValidateDataSource (dataSource);
			}

			internal ControlAdapter controlAdapter;
			protected override global::System.Web.UI.Adapters.ControlAdapter ResolveAdapter ()
			{
				return controlAdapter;
			}
		}
		
		[Test]
		public void HierarchicalDataBoundControl_DataBindFlow () {
			Page p = new Page ();
			MyHierarchicalDataBoundControl hc = new MyHierarchicalDataBoundControl ();
			p.Controls.Add (hc);
			hc.DataBind ();
			string expected = "[Start DataBind][Start PerformSelect][Start OnDataBinding][End OnDataBinding][Start PerformDataBinding][End PerformDataBinding][Start OnDataBound][End OnDataBound][End PerformSelect][End DataBind]";
			Assert.AreEqual (expected, hc.DataBindTrace, "DataBindFlow");
		}

		[Test]
		public void HierarchicalDataBoundControl_ValidateDataSource_Null ()
		{
			MyHierarchicalDataBoundControl hc = new MyHierarchicalDataBoundControl ();
			hc.DoValidateDataSource (null);
		}

		class MyHierarchicalDataBoundControlAdapter : HierarchicalDataBoundControlAdapter
		{
			internal bool perform_data_binding_called;
			protected override void PerformDataBinding ()
			{
				perform_data_binding_called = true;
			}
		}
		
		class MyControlAdapter : ControlAdapter
		{
		}

		[Test]
		[Category ("NotDotNet")] // .NET doesn't use ResolveAdapter in this case
		public void PerformDataBinding_UsesAdapter ()
		{
			MyHierarchicalDataBoundControl c = new MyHierarchicalDataBoundControl ();
			MyHierarchicalDataBoundControlAdapter a = new MyHierarchicalDataBoundControlAdapter();;
			c.controlAdapter = a;
			c.DataBind ();
			Assert.IsTrue (a.perform_data_binding_called, "PerformDataBinding_UsesAdapter");
		}

		[Test]
		public void PerformDataBinding_WorksWithControlAdapter ()
		{
			MyHierarchicalDataBoundControl c = new MyHierarchicalDataBoundControl ();
			MyControlAdapter a = new MyControlAdapter();
			c.controlAdapter = a;
			c.DataBind ();
		}

		public class TestHierarchy : IHierarchicalEnumerable
		{
			List<TestNode> list;
			
			public TestHierarchy (List<TestNode> source)
			{
			    list = source;
			}

			public IHierarchyData GetHierarchyData (object enumeratedItem)
			{
			    return enumeratedItem as TestNode;
			}

			public IEnumerator GetEnumerator ()
			{
				return list.GetEnumerator ();
			}
		}

		public class TestNode : IHierarchyData
		{
			string name;
			TestNode parent;
			List<TestNode> childNodes;

			public TestNode (string name, TestNode parent, List<TestNode> children)
			{
				this.name = name;
				this.parent = parent;
				this.childNodes = children;
			}

			public string Name
			{
				get { return name; }
			}

			public IHierarchicalEnumerable GetChildren ()
			{
			    return new TestHierarchy (childNodes);
			}

			public IHierarchyData GetParent ()
			{
			    return parent;
			}

			public bool HasChildren
			{
			    get 
			    {
				if (childNodes == null)
				    return false;

				return childNodes.Count > 0; 
			    }
			}

			public object Item
			{
			    get { return this; }
			}

			public string Path
			{
			    get
			    {
				TestNode node = this;
				string s = name;
				while ((node = (TestNode)node.GetParent ()) != null)
				    s = node.Name + ": " + s;
				return s.Trim ();
			    }
			}

			public string Type
			{
			    get { return this.ToString (); }
			}
		}
		
		[Test]
		public void TestIHierarchicalEnumerableDataSource ()
		{
			List<TestNode> list = new List<TestNode> ();
			list.Add (new TestNode ("test", null, null));
			TestHierarchy hierarchy = new TestHierarchy (list);
			MyHierarchicalDataBoundControl c = new MyHierarchicalDataBoundControl ();
			c.DataSource = hierarchy;
			c.DataBind ();
			HierarchicalDataSourceView view = c.GetData ("");
			Assert.IsNotNull (view);
		}
	}
}

#endif
