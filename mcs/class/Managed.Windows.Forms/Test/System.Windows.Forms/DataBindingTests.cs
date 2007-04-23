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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Chris Toshok	toshok@ximian.com

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Windows.Forms.DataBinding
{
	public class DataBindingTest
	{
		protected int event_num;
		protected string event_log = "";

		protected void CurrentChanged (object sender, EventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: CurrentChanged\n", event_num++);
		}
		protected void PositionChanged (object sender, EventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: PositionChanged (to {1})\n", event_num++, ((CurrencyManager)sender).Position);
		}
		protected void ItemChanged (object sender, ItemChangedEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: ItemChanged (index = {1})\n", event_num++, args.Index);
		}
		protected void ListChanged (object sender, ListChangedEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: ListChanged ({1}, {2}, {3})\n", event_num++, args.ListChangedType, args.OldIndex, args.NewIndex);
		}
		protected void MetaDataChanged (object sender, EventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: MetaDataChanged\n", event_num++);
		}
#if NET_2_0
		protected void BindingComplete (object sender, BindingCompleteEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: BindingComplete\n", event_num++);
		}
		protected void CurrentItemChanged (object sender, EventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: CurrentItemChanged\n", event_num++);
		}
		protected void DataError (object sender, BindingManagerDataErrorEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: DataError\n", event_num++);
		}
#endif
		protected void Format (object sender, ConvertEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: Binding.Format\n", event_num++);
		}
		protected void Parse (object sender, ConvertEventArgs args)
		{
			Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: Binding.Parse\n", event_num++);
		}

		protected void HookupCurrencyManager (CurrencyManager cm)
		{
			cm.CurrentChanged += new EventHandler (CurrentChanged);
			cm.PositionChanged += new EventHandler (PositionChanged);
			cm.ItemChanged += new ItemChangedEventHandler (ItemChanged);
			cm.MetaDataChanged += new EventHandler (MetaDataChanged);
#if NET_2_0
			cm.BindingComplete += new BindingCompleteEventHandler (BindingComplete);
			cm.CurrentItemChanged += new EventHandler (CurrentItemChanged);
			cm.DataError += new BindingManagerDataErrorEventHandler (DataError);
#endif
		}

		protected void HookupPropertyManager (PropertyManager pm)
		{
			pm.CurrentChanged += new EventHandler (CurrentChanged);
			pm.PositionChanged += new EventHandler (PositionChanged);
#if NET_2_0
			pm.BindingComplete += new BindingCompleteEventHandler (BindingComplete);
			pm.CurrentItemChanged += new EventHandler (CurrentItemChanged);
			pm.DataError += new BindingManagerDataErrorEventHandler (DataError);
#endif
		}

		protected void HookupBinding (Binding b)
		{
			b.Parse += new ConvertEventHandler (Parse);
			b.Format += new ConvertEventHandler (Format);
		}
	}

	[TestFixture]
	public class CurrencyManagerTest2 : DataBindingTest
	{
		[Test]
		[Category ("NotWorking")]
		public void TestRowDelete ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			DataRow newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (1, cm.Count, "1");

			DataRowView row = (DataRowView)cm.Current;

			event_log = "";
			event_num = 0;

			row.Delete ();

			Console.WriteLine (event_log);

#if NET_2_0
			Assert.AreEqual ("0: PositionChanged (to -1)\n1: ItemChanged (index = -1)\n2: PositionChanged (to -1)\n3: CurrentChanged\n4: CurrentItemChanged\n5: ItemChanged (index = -1)\n", event_log, "2");
#else
			Assert.AreEqual ("0: PositionChanged (to -1)\n1: ItemChanged (index = -1)\n2: ItemChanged (index = -1)\n", event_log, "2");
#endif

			Assert.AreEqual (0, cm.Count, "3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestRowAdd ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (0, cm.Count, "1");

			event_log = "";
			event_num = 0;

			Console.WriteLine (">>>");
			DataRow newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			Console.WriteLine (event_log);
			Console.WriteLine ("<<<");

#if NET_2_0
			Assert.AreEqual ("0: PositionChanged (to 0)\n1: CurrentChanged\n2: CurrentItemChanged\n3: ItemChanged (index = -1)\n4: ItemChanged (index = -1)\n5: Binding.Format\n6: Binding.Format\n7: Binding.Format\n", event_log, "2");
#else
			Assert.AreEqual ("0: PositionChanged (to 0)\n1: CurrentChanged\n2: ItemChanged (index = -1)\n3: ItemChanged (index = -1)\n4: Binding.Format\n5: Binding.Format\n6: Binding.Format\n", event_log, "2");
#endif

			Assert.AreEqual (1, cm.Count, "3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestRowAddAnother ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (0, cm.Count, "1");

			DataRow newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			Console.WriteLine (">>>");

			event_log = "";
			event_num = 0;

			newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			Console.WriteLine (event_log);
			Console.WriteLine ("<<<");

			Assert.AreEqual ("0: ItemChanged (index = -1)\n1: Binding.Format\n", event_log, "2");

			Assert.AreEqual (1, cm.Count, "3");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestColumnAdd ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (0, cm.Count, "1");

			event_log = "";
			event_num = 0;

			dataSet1.Tables[0].Columns.Add();

			Console.WriteLine (event_log);

#if NET_2_0			
			Assert.AreEqual ("0: MetaDataChanged\n", event_log, "2");
#else
			Assert.AreEqual ("0: MetaDataChanged\n1: MetaDataChanged\n", event_log, "2");
#endif
			Assert.AreEqual (0, cm.Count, "3");
		}

		[Test]
		public void TestColumnRemove ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();
			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (0, cm.Count, "1");

			event_log = "";
			event_num = 0;

			dataSet1.Tables[0].Columns.Remove(dataSet1.Tables[0].Columns[1]);

			Console.WriteLine (event_log);
			
			Assert.AreEqual ("0: MetaDataChanged\n", event_log, "2");

			Assert.AreEqual (0, cm.Count, "3");
		}

		[Test]
		public void TestColumnRemoveBound ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();
			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.IsFalse (binding.IsBinding, "1");
			Assert.AreEqual (0, cm.Count, "2");

			event_log = "";
			event_num = 0;

			dataSet1.Tables[0].Columns.Remove(dataSet1.Tables[0].Columns[0]);

			Console.WriteLine (event_log);
			
			Assert.AreEqual ("0: MetaDataChanged\n", event_log, "3");

			Assert.AreEqual (0, cm.Count, "4");

			Assert.IsFalse (binding.IsBinding, "5");
		}

		[Test]
		public void TestColumnChangeName ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();

			dataSet1.Tables[0].Columns.CollectionChanged += delegate (object sender, CollectionChangeEventArgs e) { Console.WriteLine ("collection changed : {0} {1}", e.Action, e.Element.GetType()); };

			dataSet1.Tables[0].Columns.Add();

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];
			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], dataSet1.Tables[0].Columns[0].ColumnName);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.IsFalse (binding.IsBinding, "1");
			Assert.AreEqual (0, cm.Count, "2");

			event_log = "";
			event_num = 0;

			dataSet1.Tables[0].DefaultView.ListChanged += delegate (object sender, ListChangedEventArgs e) { Console.WriteLine ("list changed : {0} {1} {2}", e.ListChangedType, e.OldIndex, e.NewIndex); };

			dataSet1.Tables[0].Columns[0].ColumnName = "new name";

			Console.WriteLine (event_log);
			
			Assert.AreEqual ("0: MetaDataChanged\n", event_log, "3");

			Assert.AreEqual (0, cm.Count, "4");

			Assert.IsFalse (binding.IsBinding, "5");
		}

		[Test]
		[Category ("NotWorking")]
		public void TestRowModify ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;
			string column_name;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			DataRow newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			column_name = dataSet1.Tables[0].Columns[0].ColumnName;

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];

			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], column_name);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (1, cm.Count, "1");

			event_log = "";
			event_num = 0;

			DataRowView row = (DataRowView)cm.Current;
			row.BeginEdit ();
			row[column_name] = "hi";
			row.EndEdit ();

			Console.WriteLine (event_log);

#if NET_2_0
			Assert.AreEqual ("0: CurrentItemChanged\n1: ItemChanged (index = 0)\n2: Binding.Format\n", event_log, "2");
#else
			Assert.AreEqual ("0: ItemChanged (index = 0)\n1: Binding.Format\n", event_log, "2");
#endif

			Assert.AreEqual (1, cm.Count, "3");
		}

		[Test]
		public void TestRowCancelModify ()
		{
			Control c = new Control ();
			c.CreateControl ();
			Binding binding;
			CurrencyManager cm;
			string column_name;

			DataSet dataSet1 = new DataSet();
			dataSet1.Tables.Add();
			dataSet1.Tables[0].Columns.Add();

			DataRow newrow = dataSet1.Tables[0].NewRow ();
			dataSet1.Tables[0].Rows.Add(newrow);

			column_name = dataSet1.Tables[0].Columns[0].ColumnName;

			c.BindingContext = new BindingContext ();
			cm = (CurrencyManager) c.BindingContext[dataSet1, dataSet1.Tables[0].TableName];

			binding = c.DataBindings.Add ("Text", dataSet1.Tables[0], column_name);

			HookupCurrencyManager (cm);
			HookupBinding (binding);

			cm.Position = 0;

			Assert.AreEqual (1, cm.Count, "1");

			event_log = "";
			event_num = 0;

			DataRowView row = (DataRowView)cm.Current;
			row.BeginEdit ();
			row[column_name] = "hi";
			cm.CancelCurrentEdit ();

			Console.WriteLine (event_log);
			Assert.AreEqual ("0: ItemChanged (index = 0)\n", event_log, "2");

			Assert.AreEqual (1, cm.Count, "3");
		}
	}

	[TestFixture]
	public class PropertyManagerTest2 : DataBindingTest
	{
		[Test]
		[Category ("NotWorking")]
		public void TestPropertyChange ()
		{
			Control c1 = new Control ();
			Control c2 = new Control ();

			c1.CreateControl ();
			c2.CreateControl ();

			Binding binding;
			PropertyManager pm;

			c1.BindingContext = new BindingContext ();
			c2.BindingContext = c1.BindingContext;

			pm = (PropertyManager) c2.BindingContext[c1, "Text"];

			binding = c2.DataBindings.Add ("Text", c1, "Text");

			Console.WriteLine (pm.GetType());
			Console.WriteLine (binding.BindingManagerBase.GetType());
			Assert.IsFalse (pm == binding.BindingManagerBase, "0");

			HookupPropertyManager (pm);
			HookupBinding (binding);

			event_log = "";
			event_num = 0;

			c1.Text = "hi";

			Console.WriteLine (event_log);

#if NET_2_0
			Assert.AreEqual ("0: CurrentChanged\n1: CurrentItemChanged\n2: Binding.Format\n3: CurrentChanged\n4: CurrentItemChanged\n", event_log, "1");
#else
			Assert.AreEqual ("0: CurrentChanged\n1: Binding.Format\n2: CurrentChanged\n", event_log, "1");
#endif
		}
	}
}
