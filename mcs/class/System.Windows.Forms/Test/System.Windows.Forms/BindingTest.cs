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
// Copyright (c) 2006 Novell, Inc.
//
// Authors:
//	Jackson Harper	jackson@ximian.com


using System;
using System.Data;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms.DataBinding {

	[TestFixture]
	public class BindingTest : TestHelper {
		
		[Test]
		public void CtorTest ()
		{
			string prop = "PROPERTY NAME";
			object data_source = new object ();
			string data_member = "DATA MEMBER";
			Binding b = new Binding (prop, data_source, data_member);

			Assert.IsNull (b.BindingManagerBase, "ctor1");
			Assert.IsNotNull (b.BindingMemberInfo, "ctor2");
			Assert.IsNull (b.Control, "ctor3");
			Assert.IsFalse (b.IsBinding, "ctor4");

			Assert.AreSame (b.PropertyName, prop, "ctor5");
			Assert.AreSame (b.DataSource, data_source, "ctor6");

			Assert.AreEqual (false, b.FormattingEnabled, "ctor7");
			Assert.AreEqual (String.Empty, b.FormatString, "ctor8");
			Assert.IsNull (b.FormatInfo, "ctor9");
			Assert.IsNull (b.NullValue, "ctor10");
			Assert.AreEqual (Convert.DBNull, b.DataSourceNullValue, "ctor11");
		}

		[Test]
		public void CtorNullTest ()
		{
			Binding b = new Binding (null, null, null);

			Assert.IsNull (b.PropertyName, "ctornull1");
			Assert.IsNull (b.DataSource, "ctornull2");
		}

		[Test]
		public void CtorEmptyProperty ()
		{
			Binding b = new Binding ("Text", 6, String.Empty);
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			c.DataBindings.Add (b);
			Assert.AreEqual ("6", c.Text, "A1");
		}

		[Test]
		public void BindingManagerBaseTest ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			Control c1 = new Control ();
			Control c2 = new Control ();
			Binding binding;

			c1.BindingContext = new BindingContext ();
			c2.BindingContext = c1.BindingContext;

			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsNull (binding.BindingManagerBase, "1");

			c1.CreateControl ();
			c2.CreateControl ();

			Assert.IsNull (binding.BindingManagerBase, "2");

			c2.DataBindings.Remove (binding);
			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Assert.IsTrue (binding.BindingManagerBase != null, "3");
		}

		[Test]
		/* create control and set binding context */
		public void BindingContextChangedTest ()
		{
			Control c = new Control ();
			// Test BindingContextChanged Event
			c.BindingContextChanged += new EventHandler (Event_Handler1);
			BindingContext bcG1 = new BindingContext ();
			eventcount = 0;
			c.BindingContext = bcG1;
			Assert.AreEqual (1, eventcount, "A1");
		}

		[Test]
		/* create control and show control */
		public void BindingContextChangedTest2 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;
			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			f.Show ();
			Assert.AreEqual (1, eventcount, "A1");
			f.Dispose();
		}

		[Test]
		/* create control, set binding context, and show control */
		public void BindingContextChangedTest3 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			Control c = new Control ();
			f.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			eventcount = 0;
			c.BindingContext = new BindingContext ();;
			f.Show ();
			Assert.AreEqual (1, eventcount, "A1");
			f.Dispose ();
		}

		[Test]
		public void BindingContextChangedTest4 ()
		{
			Form f = new Form ();
			f.ShowInTaskbar = false;

			ContainerControl cc = new ContainerControl ();

			Control c = new Control ();
			f.Controls.Add (cc);
			cc.Controls.Add (c);

			c.BindingContextChanged += new EventHandler (Event_Handler1);
			cc.BindingContextChanged += new EventHandler (Event_Handler1);
			f.BindingContextChanged += new EventHandler (Event_Handler1);

			eventcount = 0;
			f.Show ();
			Assert.AreEqual (5, eventcount, "A1");
			f.Dispose ();
		}

		int eventcount;
		public void Event_Handler1 (object sender, EventArgs e)
		{
			//Console.WriteLine (sender.GetType());
			//Console.WriteLine (Environment.StackTrace);
			eventcount++;
		}

		[Test]
		public void DataBindingCountTest1 ()
		{
			Control c = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "3");
			Assert.AreEqual (c, b.DataSource, "4");
			Assert.AreEqual ("Text", b.PropertyName, "5");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "6");
		}

		[Test]
		public void DataBindingCountTest2 ()
		{
			Control c = new Control ();
			Control c2 = new Control ();
			Assert.AreEqual (0, c.DataBindings.Count, "1");
			c.DataBindings.Add (new Binding ("Text", c2, "Name"));
			Assert.AreEqual (1, c.DataBindings.Count, "2");
			Assert.AreEqual (0, c2.DataBindings.Count, "3");

			Binding b = c.DataBindings[0];
			Assert.AreEqual (c, b.Control, "4");
			Assert.AreEqual (c2, b.DataSource, "5");
			Assert.AreEqual ("Text", b.PropertyName, "6");
			Assert.AreEqual ("Name", b.BindingMemberInfo.BindingField, "7");
		}

		[Test]
		public void DataSourceNullTest ()
		{
			ChildMockItem item = new ChildMockItem ();
			Control c = new Control ();
			c.Tag = null;
			item.ObjectValue = null;

			c.DataBindings.Add ("Tag", item, "ObjectValue");

			Form f = new Form ();
			f.Controls.Add (c);

			f.Show (); // Need this to init data binding

			Assert.AreEqual (DBNull.Value, c.Tag, "1");
			
			f.Dispose ();
			
		}

		// For this case to work, the data source property needs
		// to have an associated 'PropertyChanged' event.
		[Test]
		public void DataSourcePropertyChanged ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual ("B", c.Text, "#B1");
		}

		[Test]
		public void DataSourcePropertyChanged_Original ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual ("B", c.Text, "#B1");
		}

		[Test]
		public void DataSourcePropertyChanged_Original_BadName ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "xxxxxxTextXXXXX");

			try {
				c.DataBindings.Add (binding);
				Assert.Fail ("exc1");
			} catch (ArgumentException ex) {
				Assert.AreEqual ("dataMember", ex.ParamName, "ex.ParamName"); // (test is not locale dependent)
			}
		}

		[Test]
		public void DataSourcePropertyChanged_OneDeep ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			One parent = new One ();
			parent.MockItem = item;
			Binding binding = new Binding ("Text", parent, "MockItem.Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual ("B", c.Text, "#B1");
		}

		[Test]
		public void DataSourcePropertyChanged_ThreeDeep ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			One parent = new One ();
			parent.Two = new Two ();
			parent.Two.Three = new Three ();
			parent.Two.Three.MockItem = item;
			Binding binding = new Binding ("Text", parent, "Two.Three.MockItem.Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual ("B", c.Text, "#B1");

			Assert.AreEqual (1, c.DataBindings.Count, "c.DataBindings.Count");
			BindingMemberInfo bmi = c.DataBindings[0].BindingMemberInfo;
			Assert.AreEqual ("Two.Three.MockItem", bmi.BindingPath, "bmi.BindingPath");
			Assert.AreEqual ("Two.Three.MockItem.Text", bmi.BindingMember, "bmi.BindingMember");
			Assert.AreEqual ("Text", bmi.BindingField, "bmi.BindingField");
		}

		[Test]
		public void DataSourcePropertyChanged_DataSet ()
		{
			DataSet ds = new DataSet ();

			DataTable table1 = new DataTable ("Customers");
			table1.Columns.Add ("Id", typeof (int));
			table1.Columns.Add ("Name", typeof (string));
			table1.Rows.Add (new object[] {3, "customer1"});
			table1.Rows.Add (new object[] {7, "customer2"});
			ds.Tables.Add (table1);

			DataTable table2 = new DataTable ("Orders");
			table2.Columns.Add ("OrderId", typeof (int));
			table2.Columns.Add ("CustomerId", typeof (int));
			table2.Rows.Add (new object[] {56, 7});
			table2.Rows.Add (new object[] {57, 3});
			ds.Tables.Add (table2);

			DataRelation relation = new DataRelation ("CustomerOrders", table1.Columns ["Id"],
					table2.Columns ["CustomerId"]);
			ds.Relations.Add (relation);

			Control ctrl = new Control ();
			ctrl.BindingContext = new BindingContext ();
			ctrl.CreateControl ();

			ctrl.DataBindings.Add ("Text", ds, "Customers.CustomerOrders.OrderId");
			Assert.AreEqual ("57", ctrl.Text, "A1");
		}

		[Test]
		public void DataSourcePropertyDifferentType ()
		{
			Exception exc = new Exception (String.Empty, new ArgumentNullException ("PARAM"));

			// The type of the property is Exception, but we know that the value
			// is actually an ArgumentException, thus specify the ParamName property
			Control ctrl = new Control ();
			ctrl.BindingContext = new BindingContext ();
			ctrl.CreateControl ();

			ctrl.DataBindings.Add ("Text", exc, "InnerException.ParamName");
			Assert.AreEqual ("PARAM", ctrl.Text, "A1");
		}

		[Test]
		public void IsBindingTest ()
		{
			MockItem [] items = new MockItem [] { new MockItem ("A", 0) };
			Binding binding = new Binding ("Text", items, "Text");
			Binding binding2 = new Binding ("Text", items [0], "Text");
			Assert.IsFalse (binding.IsBinding, "#A1");
			Assert.IsFalse (binding2.IsBinding, "#A2");

			Control c = new Control ();
			Control c2 = new Control ();
			c.DataBindings.Add (binding);
			c2.DataBindings.Add (binding2);
			Assert.IsFalse (binding.IsBinding, "#B1");
			Assert.IsFalse (binding2.IsBinding, "#B2");

			c.CreateControl ();
			c2.CreateControl ();
			Assert.IsFalse (binding.IsBinding, "#C1");
			Assert.IsFalse (binding2.IsBinding, "#C2");

			Form form = new Form ();
			form.ShowInTaskbar = false;
			form.Controls.Add (c);
			form.Controls.Add (c2);
			Assert.IsTrue (binding.IsBinding, "#D1");
			Assert.IsTrue (binding2.IsBinding, "#D2");

			form.Show ();

			// Important part -
			// IsBinding is true ALWAYS with PropertyManager, even when
			// ResumeBinding has been called
			//
			CurrencyManager curr_manager = (CurrencyManager)form.BindingContext [items];
			PropertyManager prop_manager = (PropertyManager)form.BindingContext [items [0]];
			curr_manager.SuspendBinding ();
			prop_manager.SuspendBinding ();
			Assert.IsFalse (binding.IsBinding, "#E1"); 
			Assert.IsTrue (binding2.IsBinding, "#E2");

			curr_manager.ResumeBinding ();
			prop_manager.ResumeBinding ();
			Assert.IsTrue (binding.IsBinding, "#F1");
			Assert.IsTrue (binding2.IsBinding, "#F2");

			form.Dispose ();
		}

		[Test]
		public void ReadValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			ChildMockItem item = new ChildMockItem ();
			item.ObjectValue = "A";
			Binding binding = new Binding ("Tag", item, "ObjectValue");
			binding.ControlUpdateMode = ControlUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual (null, c.Tag, "#A1");

			item.ObjectValue = "B";
			Assert.AreEqual (null, c.Tag, "#B1");

			binding.ReadValue ();
			Assert.AreEqual ("B", c.Tag, "#C1");

			item.ObjectValue = "C";
			binding.ReadValue ();
			Assert.AreEqual ("C", c.Tag, "#D1");
			
			c.Dispose();
		}

		[Test]
		public void WriteValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ();
			item.Text = "A";
			Binding binding = new Binding ("Text", item, "Text");
			binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			c.Text = "B";
			Assert.AreEqual ("A", item.Text, "#B1");

			binding.WriteValue ();
			Assert.AreEqual ("B", item.Text, "#C1");
		}

		[Test]
		public void BindableComponentTest ()
		{
			Control c = new Control ();

			MockItem item = new MockItem (String.Empty, 0);
			Binding binding = new Binding ("Text", item, "Text");

			c.DataBindings.Add (binding);
			Assert.AreEqual (c, binding.Control, "#A1");
			Assert.AreEqual (c, binding.BindableComponent, "#A2");

			// 
			// Now use IBindableComponent - update binding when property changes
			// since ToolStripItem doesn't have validation at all
			//
			BindableToolStripItem toolstrip_item = new BindableToolStripItem ();
			toolstrip_item.BindingContext = new BindingContext ();
			Binding binding2 = new Binding ("Text", item, "Text");
			binding2.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;

			item.Text = "A";
			toolstrip_item.DataBindings.Add (binding2);
			Assert.AreEqual (null, binding2.Control, "#B1");
			Assert.AreEqual (toolstrip_item, binding2.BindableComponent, "#B2");
			Assert.AreEqual (item.Text, toolstrip_item.Text, "#B3");

			toolstrip_item.Text = "B";
			Assert.AreEqual (toolstrip_item.Text, item.Text, "#C1");
		}

		[Test]
		public void ControlUpdateModeTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");
			binding.ControlUpdateMode = ControlUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual (String.Empty, c.Text, "#A1");

			item.Text = "B";
			Assert.AreEqual (String.Empty, c.Text, "#B1");
		}

		[Test]
		public void DataSourceUpdateModeTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ("A", 0);
			Binding binding = new Binding ("Text", item, "Text");
			binding.DataSourceUpdateMode = DataSourceUpdateMode.Never;

			c.DataBindings.Add (binding);
			Assert.AreEqual ("A", c.Text, "#A1");

			c.Text = "B";
			Assert.AreEqual ("A", item.Text, "#B1");

			binding.DataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged;
			Assert.AreEqual ("A", item.Text, "#C1");

			c.Text = "C";
			Assert.AreEqual ("C", item.Text, "#D1");

			// This requires a Validation even, which we can't test
			// by directly modifying the property
			binding.DataSourceUpdateMode = DataSourceUpdateMode.OnValidation;

			c.Text = "D";
			Assert.AreEqual ("C", item.Text, "#E1");
		}

		[Test]
		public void DataSourceNullValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			ChildMockItem item = new ChildMockItem ();
			item.ObjectValue = "A";
			Binding binding = new Binding ("Tag", item, "ObjectValue");
			binding.DataSourceNullValue = "NonNull";

			c.DataBindings.Add (binding);
			Assert.AreEqual (c.Tag, "A", "#A1");

			// Since Tag property doesn't have a 
			// TagChanged event, we need to force an update
			c.Tag = null;
			binding.WriteValue ();
			Assert.AreEqual (item.ObjectValue, "NonNull", "#B1");
		}

		[Test]
		public void NullValueTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			DataTable table = new DataTable ();
			table.Columns.Add ("Name", typeof (string));
			table.Columns.Add ("Id", typeof (int));
			table.Rows.Add (null, DBNull.Value);

			Binding binding = new Binding ("Tag", table, "Name");
			Binding binding2 = new Binding ("Width", table, "Id");
			binding.FormattingEnabled = true;
			binding.NullValue = "non-null";
			binding2.FormattingEnabled = true;
			binding2.NullValue = 101;

			c.Width = 99;
			c.DataBindings.Add (binding);
			c.DataBindings.Add (binding2);

			Assert.AreEqual ("non-null", c.Tag, "#A1");
			Assert.AreEqual (101, c.Width, "#A2");
		}

		[Test]
		public void FormattingEnabledTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			MockItem item = new MockItem ();
			item.Value = 666;
			Binding binding = new Binding ("Text", item, "Value");
			binding.FormattingEnabled = true;
			binding.FormatString = "p";

			c.DataBindings.Add (binding);
			Assert.AreEqual ((666).ToString ("p"), c.Text, "#A1");

			binding.FormatString = "c";
			Assert.AreEqual ((666).ToString ("c"), c.Text, "#B1");

			binding.FormattingEnabled = false;
			Assert.AreEqual ((666).ToString (), c.Text, "#C1");
		}

		[Test]
		public void FormatStringTest ()
		{
			Binding binding = new Binding ("Text", null, "Text");
			binding.FormatString = null;

			Assert.AreEqual (String.Empty, binding.FormatString, "#A1");
		}

	}

	class ChildMockItem : MockItem
	{
		object value;

		public ChildMockItem ()
			: base (null, 0)
		{
		}

		public object ObjectValue
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}
	}

	class BindableToolStripItem : ToolStripItem, IBindableComponent
	{
		ControlBindingsCollection data_bindings;
		BindingContext binding_context;

		public ControlBindingsCollection DataBindings {
			get {
				if (data_bindings == null)
					data_bindings = new ControlBindingsCollection (this);

				return data_bindings;
			}
		}

		public BindingContext BindingContext
		{
			get {
				return binding_context;
			}
			set {
				binding_context = value;
			}
		}
	}

	class One
	{
		//----
		//private global::System.Collections.Generic.IList<Two> m_twoList
		//    = new global::System.Collections.Generic.List<Two> ();
		//
		//public global::System.Collections.Generic.IList<Two> TwoList
		//{
		//    get { return m_twoList; }
		//}

		//----
		private Two m_two;

		public Two Two
		{
			get { return m_two; }
			set { m_two = value; }
		}
	
		//----
		private MockItem m_MockItem;

		public MockItem MockItem
		{
			get { return m_MockItem; }
			set { m_MockItem = value; }
		}
	
		//----
		public override string ToString ()
		{
			return "!!! ToString on One !!!";
		}
	}

	class Two
	{
		//private global::System.Collections.Generic.IList<MockItem> m_MockItemList
		//    = new global::System.Collections.Generic.List<MockItem> ();
		//
		//public global::System.Collections.Generic.IList<MockItem> MockItemList
		//{
		//    get { return m_MockItemList; }
		//}

		//----
		private MockItem m_MockItem;

		public MockItem MockItem
		{
			get { return m_MockItem; }
			set { m_MockItem = value; }
		}

		private Three m_Three;

		public Three Three
		{
			get { return m_Three; }
			set { m_Three = value; }
		}

		public override string ToString ()
		{
			return "!!! ToString on Two !!!";
		}
	}

	class Three
	{
		private MockItem m_MockItem;

		public MockItem MockItem
		{
			get { return m_MockItem; }
			set { m_MockItem = value; }
		}

		public override string ToString ()
		{
			return "!!! ToString on Three !!!";
		}
	}

}

