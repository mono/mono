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

// #undef DebugCurrencyManager

using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

using NUnit.Framework;

using System.Collections.ObjectModel;

namespace MonoTests.System.Windows.Forms.DataBinding
{
	[TestFixture]
	public class CurrencyManagerTest : TestHelper
	{
		[Test]
		public void Defaults ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;

			Assert.AreSame (data_source, cm.List, "DEFAULTS1");
			Assert.AreEqual (0, cm.Count, "DEFAULTS2");
			Assert.AreEqual (-1, cm.Position, "DEFAULTS3");
		}

		[Test]
		[ExpectedException (typeof (IndexOutOfRangeException))]
		public void UninitializedCurrent ()
		{
			BindingContext bc = new BindingContext ();
			ArrayList data_source = new ArrayList ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;

			// This line should throw
			Assert.AreSame (null, cm.Current, "CurrentOfEmpty");
		}

		[Test]
		public void DataSetList ()
		{
			DataSet dataset = new DataSet ("DataSet");
			DataTable table = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm;

			dataset.Tables.Add (table);
			cm = bc [dataset] as CurrencyManager;

			Assert.AreEqual (typeof (DataViewManager), cm.List.GetType (), "DATASETLIST1");
			Assert.AreEqual (1, cm.Count, "DATASETLIST2");
			Assert.AreEqual (0, cm.Position, "DATASETLIST3");
//			Assert.AreEqual (typeof (DataViewManagerListItemTypeDescriptor), cm.Current.GetType (),
//					"DATASETLIST4");
		}

		[Test]
		public void DataSetListTable ()
		{
			DataSet dataset = new DataSet ("DataSet");
			DataTable table = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm;

			dataset.Tables.Add (table);
			cm = bc [dataset, "Table"] as CurrencyManager;

			Assert.AreEqual (typeof (DataView), cm.List.GetType (), "DATASETLIST1");
			Assert.AreEqual (0, cm.Count, "DATASETLIST2");
			Assert.AreEqual (-1, cm.Position, "DATASETLIST3");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataSetListTableBogusField ()
		{
			DataSet dataset = new DataSet ("DataSet");
			DataTable table = new DataTable ("Table.Column");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm;

			dataset.Tables.Add (table);

			// child list can't be created
			cm = bc [dataset, "Table"] as CurrencyManager;

			TestHelper.RemoveWarning (cm);
		}

		[Test] // bug #80107
		public void DataView ()
		{
			DataView dv = new DataView ();

			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [dv, string.Empty] as CurrencyManager;
			Assert.IsNotNull (cm, "#A1");
			Assert.AreEqual (0, cm.Count, "#A2");
			Assert.AreEqual (-1, cm.Position, "#A3");

			DataTable dt = new DataTable ("Testdata");
			dt.Columns.Add ("A");
			dt.Columns.Add ("B");
			dt.Rows.Add (new object [] { "A1", "B1" });
			dt.Rows.Add (new object [] { "A2", "B2" });
			dv.Table = dt;

			Assert.AreEqual (2, cm.Count, "#B1");
			Assert.AreEqual (0, cm.Position, "#B2");
		}

		[Test]
		public void IsBindingEmptyDataSource ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			BindingList<string> list = new BindingList<string> ();
			CurrencyManager cm = (CurrencyManager)c.BindingContext [list];

			Assert.AreEqual (true, cm.IsBindingSuspended, "A1");

			cm.ResumeBinding ();
			Assert.AreEqual (true, cm.IsBindingSuspended, "B1");

			list.Add ("A");
			Assert.AreEqual (false, cm.IsBindingSuspended, "D1");

			list.Clear ();
			Assert.AreEqual (true, cm.IsBindingSuspended, "E1");
		}

		[Test]
		public void MoveArrayListForward ()
		{
			ArrayList data_source = new ArrayList ();
			BindingContext bc = new BindingContext ();

			for (int i = 0; i < 10; i++)
				data_source.Add (new object ());

			CurrencyManager cm = bc [data_source] as CurrencyManager;
			for (int i = 0; i < 10; i++) {
				Assert.AreSame (data_source [i], cm.Current, "MOVEALF" + i);
				cm.Position++;
			}

			cm.Position++;
			cm.Position++;

			Assert.AreSame (data_source [9], cm.Current, "MOVEALFEND");
		}

		[Test]
		public void MoveArrayListBackward ()
		{
			ArrayList data_source = new ArrayList ();
			BindingContext bc = new BindingContext ();

			for (int i = 0; i < 10; i++)
				data_source.Add (new object ());

			CurrencyManager cm = bc [data_source] as CurrencyManager;
			cm.Position = 9;
			for (int i = 9; i >= 0; i--) {
				Assert.AreSame (data_source [i], cm.Current, "MOVEALB" + i);
				cm.Position--;
			}

			cm.Position--;
			cm.Position--;

			Assert.AreSame (data_source [0], cm.Current, "MOVEALBSTART");
		}

		[Test]
		public void SetPositionArrayList ()
		{
			ArrayList data_source = new ArrayList ();
			BindingContext bc = new BindingContext ();

			for (int i = 0; i < 10; i++)
				data_source.Add (new object ());

			CurrencyManager cm = bc [data_source] as CurrencyManager;
			for (int i = 3; i >= 0; i--) {
				cm.Position = i;
				Assert.AreSame (data_source [i], cm.Current, "MOVEAL1-" + i);
			}

			cm.Position--;

			for (int i = 0; i < 10; i++) {
				cm.Position = i;
				Assert.AreSame (data_source [i], cm.Current, "MOVEAL2-" + i);
			}

			for (int i = 5; i < 10; i++) {
				cm.Position = i;
				Assert.AreSame (data_source [i], cm.Current, "MOVEAL3-" + i);
			}
		}

		[Test]
		public void LateBuildDataTable ()
		{
			DataTable data_source = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;

			Assert.AreEqual (-1, cm.Position, "LATEBUILDTABLE1");
			Assert.AreEqual (0, cm.Count, "LATEBUILDTABLE2");

			DataColumn column = new DataColumn ("Column");
			column.DataType = typeof (int);
			data_source.Columns.Add (column);
			
			for (int i = 0; i < 10; i++) {
				DataRow row = data_source.NewRow ();
				row ["Column"] = i;
				data_source.Rows.Add (row);
			}

			Assert.AreEqual (0, cm.Position, "LATEBUILDTABLE3");
			Assert.AreEqual (10, cm.Count, "LATEBUILDTABLE4");
		}

		[Test]
		public void LateBuildArrayList ()
		{
			ArrayList data_source = new ArrayList ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;

			Assert.AreEqual (-1, cm.Position, "LATEBUILDLIST1");
			Assert.AreEqual (0, cm.Count, "LATEBUILDLIST2");

			data_source.AddRange (new object [] { 1, 2, 3, 4, 5, 6, 7 });

			Assert.AreEqual (-1, cm.Position, "LATEBUILDLIST3");
			Assert.AreEqual (7, cm.Count, "LATEBUILDLIST4");
		}

		[Test]
		public void MoveDataTableForward ()
		{
			DataTable data_source = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;
			DataColumn column = new DataColumn ("Column");

			column.DataType = typeof (int);
			data_source.Columns.Add (column);
			for (int i = 0; i < 10; i++) {
				DataRow row = data_source.NewRow ();
				row ["Column"] = i;
				data_source.Rows.Add (row);
			}


			for (int i = 0; i < 10; i++) {
				DataRowView row = cm.Current as DataRowView;
				Assert.IsFalse (row == null, "MOVETABLEF-NULL-" + i);
				Assert.AreEqual (row ["Column"], i, "MOVETABLEF-" + i);
				cm.Position++;
			}

			cm.Position++;
			cm.Position++;

			Assert.AreEqual (9, ((DataRowView) cm.Current) ["Column"], "MOVETABLEF-END");
		}

		[Test]
		public void MoveDataTableBackward ()
		{
			DataTable data_source = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;
			DataColumn column = new DataColumn ("Column");

			column.DataType = typeof (int);
			data_source.Columns.Add (column);
			for (int i = 0; i < 10; i++) {
				DataRow row = data_source.NewRow ();
				row ["Column"] = i;
				data_source.Rows.Add (row);
			}


			cm.Position = 9;
			for (int i = 9; i >= 0; i--) {
				DataRowView row = cm.Current as DataRowView;
				Assert.IsFalse (row == null, "MOVETABLEB-NULL-" + i);
				Assert.AreEqual (row ["Column"], i, "MOVETABLEB-" + i);
				cm.Position--;
			}

			cm.Position--;
			cm.Position--;

			Assert.AreEqual (0, ((DataRowView) cm.Current) ["Column"], "MOVETABLEB-START");
		}

		[Test]
		public void SetPositionDataTable ()
		{
			DataTable data_source = new DataTable ("Table");
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source] as CurrencyManager;
			DataColumn column = new DataColumn ("Column");

			column.DataType = typeof (int);
			data_source.Columns.Add (column);
			for (int i = 0; i < 10; i++) {
				DataRow row = data_source.NewRow ();
				row ["Column"] = i;
				data_source.Rows.Add (row);
			}


			for (int i = 5; i < 10; i++) {
				cm.Position = i;
				DataRowView row = cm.Current as DataRowView;
				Assert.IsFalse (row == null, "SETTABLE1-NULL-" + i);
				Assert.AreEqual (row ["Column"], i, "SETTABLE1-" + i);
			}

			for (int i = 5; i >= 0; i--) {
				cm.Position = i;
				DataRowView row = cm.Current as DataRowView;
				Assert.IsFalse (row == null, "SETTABLE2-NULL-" + i);
				Assert.AreEqual (row ["Column"], i, "SETTABLE2-" + i);
			}
		}

		[Test]
		public void NavigateDataSetToTable ()
		{
			DataSet data_source = new DataSet ("DataSet");
			DataTable table = new DataTable ("Table");
			DataColumn column = new DataColumn ("Column");
			BindingContext bc = new BindingContext ();

			data_source.Tables.Add (table);

			column.DataType = typeof (int);
			table.Columns.Add (column);
			for (int i = 0; i < 10; i++) {
				DataRow row = table.NewRow ();
				row ["Column"] = i;
				table.Rows.Add (row);
			}

			CurrencyManager cm = bc [data_source, "Table"] as CurrencyManager;

			Assert.AreEqual (0, cm.Position, "NAVSETTOTABLE1");
			Assert.AreEqual (10, cm.Count, "NAVSETTOTABLE2");
			Assert.AreEqual (typeof (DataView), cm.List.GetType (), "NAVSETTOTABLE3");

			for (int i = 0; i < 10; i++) {
				DataRowView row = cm.Current as DataRowView;
				Assert.IsFalse (row == null, "NAVSETTOTABLE-NULL-" + i);
				Assert.AreEqual (i, row ["Column"], "NAVSETTOTABLE-" + i);
				cm.Position++;
			}

			cm.Position++;
			cm.Position++;

			Assert.AreEqual (9, ((DataRowView) cm.Current) ["Column"], "NAVSETTOTABLE-END");
		}

		[Test]
		public void NavigateDataSetToColumn ()
		{
			DataSet data_source = new DataSet ("DataSet");
			DataTable table = new DataTable ("Table");
			DataColumn column = new DataColumn ("Column");
			BindingContext bc = new BindingContext ();

			data_source.Tables.Add (table);

			column.DataType = typeof (int);
			table.Columns.Add (column);
			for (int i = 0; i < 10; i++) {
				DataRow row = table.NewRow ();
				row ["Column"] = i;
				table.Rows.Add (row);
			}

			CurrencyManager cm = bc [data_source, "Table.Column"] as CurrencyManager;

			Assert.AreEqual (null, cm, "NAVSETTOCOLUMN1");
		}

		[Test]
		public void NavigateDataSetToParentRelation ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1.Relation"] as CurrencyManager;

			Assert.AreEqual (0, cm.Position, "NAVSETTORELATION1");
			Assert.AreEqual (1, cm.Count, "NAVSETTORELATION2");
			Assert.IsTrue (cm.List is DataView, "NAVSETTORELATION3");

			DataRowView row = cm.Current as DataRowView;
			Assert.IsFalse (row == null, "NAVSETTORELATION-NULL-VALUE");
			Assert.AreEqual (0, row ["Two"], "NAVSETTORELATION-VALUE");

			cm.Position++;
			cm.Position++;

			Assert.AreEqual (0, ((DataRowView) cm.Current) ["Two"], "NAVSETTORELATION-END");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void DataSetToChildRelation ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();

			// Can't create a list on a child relation
			CurrencyManager cm = bc [data_source, "Table2.Relation"] as CurrencyManager;

			TestHelper.RemoveWarning (cm);
		}

		[Test]
		public void DataSetToParentRelationField ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();

			CurrencyManager cm = bc [data_source, "Table1.Relation.Two"] as CurrencyManager;

			Assert.AreEqual (null, cm, "SETTOPARENTRELATIONFIELD");
		}

		[Test]
		public void MultiColumnedRelation ()
		{
			DataSet dataset = new DataSet ();
			DataTable sports = new DataTable ("Sports");
			DataTable athletes = new DataTable ("Athletes");
		
			DataColumn column;
			DataRow row;

			column = new DataColumn ();
			column.DataType = typeof (int);
			column.ColumnName = "SportID";
			column.Unique = true;
			sports.Columns.Add (column);

			column = new DataColumn ();
			column.DataType = typeof (string);
			column.ColumnName = "SportName";
			sports.Columns.Add (column);


			string [] sports_names = new string [] { "Hockey", "Baseball", "Basketball", "Football", "Boxing", "Surfing" };
			for (int i = 0; i < sports_names.Length; i++) {
				row = sports.NewRow ();
				row ["SportID"] = i;
				row ["SportName"] = sports_names [i];
				sports.Rows.Add (row);
			}
		
		
			// Athletes table
			column = new DataColumn ();
			column.DataType = typeof (int);
			column.ColumnName = "AthleteID";
			column.Unique = true;
			athletes.Columns.Add (column);

			column = new DataColumn ();
			column.DataType = typeof (int);
			column.ColumnName = "Sport";
			athletes.Columns.Add (column);

			column = new DataColumn ();
			column.DataType = typeof (string);
			column.ColumnName = "AthleteName";
			athletes.Columns.Add (column);

			string [] athlete_names = new string [] { "@alp", "@lupus", "@tjfontaine", "duncan", "marv", "WindowsUninstall",
								  "@jackson", "@migHome", "_Synced[work]", "GodZhila", "Raboo",
								  "@jchambers", "@mkestner", "barbosa", "IzeBurn", "squinky86",
								  "@kangaroo",  "@paco", "Demian", "logiclrd", "tenshiKur0" };
			for (int i = 0; i < athlete_names.Length; i++) {
				row = athletes.NewRow ();
				row ["AthleteID"] = i;
				row ["Sport"] = i % sports_names.Length;
				row ["AthleteName"] = athlete_names [i];
				athletes.Rows.Add (row);
			}

			dataset.Tables.Add (sports);
			dataset.Tables.Add (athletes);
			dataset.Relations.Add ("AthletesSports", sports.Columns ["SportID"], athletes.Columns ["Sport"]);

			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [dataset, "Sports.AthletesSports"] as CurrencyManager;

			Assert.AreEqual (0, cm.Position, "MC1");
			Assert.AreEqual (4, cm.Count, "MC2");

			DataRowView rowview = cm.Current as DataRowView;
			Assert.IsFalse (rowview == null, "MC3");
			Assert.AreEqual (0, rowview ["AthleteID"], "MC4");
			Assert.AreEqual ("@alp", rowview ["AthleteName"], "MC5");
			Assert.AreEqual (0, rowview ["Sport"], "MC6");

			cm.Position++;

			rowview = cm.Current as DataRowView;
			Assert.IsFalse (rowview == null, "MC7");
			Assert.AreEqual (6, rowview ["AthleteID"], "MC8");
			Assert.AreEqual ("@jackson", rowview ["AthleteName"], "MC9");
			Assert.AreEqual (0, rowview ["Sport"], "MC10");

			cm.Position++;

			rowview = cm.Current as DataRowView;
			Assert.IsFalse (rowview == null, "MC11");
			Assert.AreEqual (12, rowview ["AthleteID"], "MC12");
			Assert.AreEqual ("@mkestner", rowview ["AthleteName"], "MC13");
			Assert.AreEqual (0, rowview ["Sport"], "MC14");

			cm.Position++;

			rowview = cm.Current as DataRowView;
			Assert.IsFalse (rowview == null, "MC15");
			Assert.AreEqual (18, rowview ["AthleteID"], "MC16");
			Assert.AreEqual ("Demian", rowview ["AthleteName"], "MC17");
			Assert.AreEqual (0, rowview ["Sport"], "MC18");

			cm.Position++;

			rowview = cm.Current as DataRowView;
			Assert.IsFalse (rowview == null, "MC19");
			Assert.AreEqual (18, rowview ["AthleteID"], "MC20");
			Assert.AreEqual ("Demian", rowview ["AthleteName"], "MC21");
			Assert.AreEqual (0, rowview ["Sport"], "MC22");
		}

		private DataSet CreateRelatedDataSet ()
		{
			DataSet dataset = new DataSet ("DataSet");
			DataTable dt1 = new DataTable ("Table1");
			DataTable dt2 = new DataTable ("Table2");
			DataColumn column;

			column = new DataColumn ("One");
			column.DataType = typeof (int);
			column.Unique = true;
			dt1.Columns.Add (column);

			for (int i = 0; i < 10; i++) {
				DataRow row = dt1.NewRow ();
				row ["One"] = i;
				dt1.Rows.Add (row);
			}
			
			column = new DataColumn ("Two");
			column.DataType = typeof (int);
			column.Unique = true;
			dt2.Columns.Add (column);

			for (int i = 0; i < 10; i++) {
				DataRow row = dt2.NewRow ();
				row ["Two"] = i;
				dt2.Rows.Add (row);
			}

			dataset.Tables.Add (dt1);
			dataset.Tables.Add (dt2);
			dataset.Relations.Add ("Relation", dt1.Columns ["One"], dt2.Columns ["Two"]);

			return dataset;
		}
		[Test]
		public void EndUninitializedEdit ()
		{
			ArrayList list = new ArrayList ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [list] as CurrencyManager;

			cm.EndCurrentEdit ();
		}

		[Test]
		public void CancelUninitializedEdit ()
		{
			ArrayList list = new ArrayList ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [list] as CurrencyManager;

			cm.CancelCurrentEdit ();
		}

		[Test]
		public void CheckPositionOfRelatedSibling1 ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1.Relation"] as CurrencyManager;
			CurrencyManager scm = bc [data_source, "Table1"] as CurrencyManager;

			cm.Position++;
			cm.Position++;

			// position is not updated
			Assert.AreEqual (0, scm.Position, "#8");
		}

		[Test]
		public void CheckPositionOfRelatedSibling2 ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1.Relation"] as CurrencyManager;
			CurrencyManager scm = bc [data_source, "Table1"] as CurrencyManager;

			Assert.AreEqual (0, cm.Position, "#1");

			scm.Position++;

			Assert.AreEqual (0, cm.Position, "#2");
		}

		int event_num;
		int current_changed;
		int position_changed;
		int item_changed;
		int metadata_changed;
		string event_log = "";
		ItemChangedEventArgs item_changed_args;
		bool list_changed_called;
		ListChangedEventArgs list_changed_args;

		void CurrentChanged (object sender, EventArgs args)
		{
			current_changed = ++event_num;
			DebugWriteLine ("current_changed = {0}", current_changed);
			//Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: CurrentChanged\n", current_changed);
		}
		void PositionChanged (object sender, EventArgs args)
		{
			position_changed = ++event_num;
			DebugWriteLine ("position_changed = {0}", position_changed);
			//Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: PositionChanged (to {1})\n", position_changed, ((CurrencyManager)sender).Position);
		}
		void ItemChanged (object sender, ItemChangedEventArgs args)
		{
			item_changed = ++event_num;
			item_changed_args = args;
			DebugWriteLine ("item_changed = {0}, index = {1}", item_changed, args.Index);
			//Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: ItemChanged (index = {1})\n", item_changed, args.Index);
		}
		void ListChanged (object sender, ListChangedEventArgs args)
		{
			DebugWriteLine ("ListChanged ({0},{1},{2})", args.ListChangedType, args.OldIndex, args.NewIndex);
			//Console.WriteLine (Environment.StackTrace);
			event_log += String.Format (" : ListChanged ({0}, {1}, {2})\n", args.ListChangedType, args.OldIndex, args.NewIndex);
		}
		void MetaDataChanged (object sender, EventArgs args)
		{
			metadata_changed = ++event_num;
			DebugWriteLine ("metadata_changed = {0}", metadata_changed);
			//Console.WriteLine (Environment.StackTrace);
			event_log += String.Format ("{0}: MetaDataChanged\n", metadata_changed);
		}
		// CurrencyManager.ListChanged handler, not IBindingList.ListChanged
		void ListChangedEvent (object sender, ListChangedEventArgs args)
		{
			list_changed_called = true;
			list_changed_args = args;
			DebugWriteLine ("CurrencyManager.ListChanged ({0},{1},{2})", args.ListChangedType, args.OldIndex, args.NewIndex);

		}

		[Test]
		public void AddNew ()
		{
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1"] as CurrencyManager;

			event_num = current_changed = position_changed = -1;
			cm.CurrentChanged += new EventHandler (CurrentChanged);
			cm.PositionChanged += new EventHandler (PositionChanged);
			cm.ItemChanged += new ItemChangedEventHandler (ItemChanged);
			list_changed_called = false;
			cm.ListChanged += new ListChangedEventHandler (ListChangedEvent);

			Assert.AreEqual (0, cm.Position, "AddNew1");
			Assert.AreEqual (10, cm.Count, "AddNew2");
			Assert.AreEqual (cm.Count, cm.List.Count, "AddNew2.5");

			cm.AddNew ();

			Assert.AreEqual (10, cm.Position, "AddNew3");
			Assert.AreEqual (11, cm.Count, "AddNew4");
			Assert.AreEqual (cm.Count, cm.List.Count, "AddNew4.5");

			Assert.AreEqual (0, item_changed, "AddNew5");
			Assert.AreEqual (-1, item_changed_args.Index, "AddNew6");
			Assert.AreEqual (1, current_changed, "AddNew7");
			Assert.AreEqual (2, position_changed, "AddNew8");
			Assert.AreEqual (true, list_changed_called, "AddNew9");
			Assert.AreEqual (-1, list_changed_args.OldIndex, "AddNew10");
			Assert.AreEqual (10, list_changed_args.NewIndex, "AddNew11");

			cm.CurrentChanged -= new EventHandler (CurrentChanged);
			cm.PositionChanged -= new EventHandler (PositionChanged);
			cm.ListChanged -= new ListChangedEventHandler (ListChangedEvent);
		}

		[Test]
		public void CancelAddNew ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment");
			}

			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1"] as CurrencyManager;

			DataView dv = cm.List as DataView;

			event_num = current_changed = position_changed = -1;
			cm.CurrentChanged += new EventHandler (CurrentChanged);
			cm.PositionChanged += new EventHandler (PositionChanged);
			cm.ItemChanged += new ItemChangedEventHandler (ItemChanged);
			dv.ListChanged += new ListChangedEventHandler (ListChanged);

			Assert.AreEqual (0, cm.Position, "CancelAddNew1");
			Assert.AreEqual (10, cm.Count, "CancelAddNew2");
			Assert.AreEqual (cm.Count, cm.List.Count, "AddNew2.5");

			cm.AddNew ();

			Assert.AreEqual (0, item_changed, "CancelAddNew3");
			Assert.AreEqual (-1, item_changed_args.Index, "CancelAddNew4");
			Assert.AreEqual (1, current_changed, "CancelAddNew5");
			Assert.AreEqual (2, position_changed, "CancelAddNew6");

			cm.CancelCurrentEdit ();

			Assert.AreEqual (6, item_changed, "CancelAddNew7");
			Assert.AreEqual (9, item_changed_args.Index, "CancelAddNew8");
			Assert.AreEqual (3, current_changed, "CancelAddNew9");
			Assert.AreEqual (4, position_changed, "CancelAddNew10");

			Assert.AreEqual (9, cm.Position, "CancelAddNew11");
			Assert.AreEqual (10, cm.Count, "CancelAddNew12");
			Assert.AreEqual (cm.Count, cm.List.Count, "AddNew12.5");

			cm.CurrentChanged -= new EventHandler (CurrentChanged);
			cm.PositionChanged -= new EventHandler (PositionChanged);
		}

		class CancelAddNewList<T> : Collection<T>, ICancelAddNew
		{
			public bool EndNewCalled;
			public bool CancelNewCalled;
			public int LastIndex = -1;

			public void EndNew (int index)
			{
				EndNewCalled = true;
				LastIndex = index;
			}

			public void CancelNew (int index)
			{
				CancelNewCalled = true;
				LastIndex = index;
			}

			public void Reset ()
			{
				EndNewCalled = CancelNewCalled = false;
				LastIndex = -1;
			}
		}

		// Support for ICancelNew interface
		[Test]
		public void CancelAddNew2 ()
		{
			BindingContext bc = new BindingContext ();
			CancelAddNewList<int> list = new CancelAddNewList<int> ();
			list.Add (4);
			list.Add (6);

			CurrencyManager cm = (CurrencyManager)bc [list];

			Assert.AreEqual (false, list.EndNewCalled, "A1");
			Assert.AreEqual (false, list.CancelNewCalled, "A2");
			Assert.AreEqual (-1, list.LastIndex, "A3");

			cm.CancelCurrentEdit ();
			Assert.AreEqual (false, list.EndNewCalled, "B1");
			Assert.AreEqual (true, list.CancelNewCalled, "B2");
			Assert.AreEqual (0, list.LastIndex, "B3");

			cm.Position = 1;
			list.Reset ();

			cm.CancelCurrentEdit ();
			Assert.AreEqual (false, list.EndNewCalled, "C1");
			Assert.AreEqual (true, list.CancelNewCalled, "C2");
			Assert.AreEqual (1, list.LastIndex, "C3");
		}

		[Test]
		public void EndAddNew ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails with 2.0 profile");
			}
			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source.Tables["Table1"], ""] as CurrencyManager;

			data_source.Tables["Table1"].DefaultView.ListChanged += 
				new ListChangedEventHandler (DataView_ListChanged);

			event_num = current_changed = position_changed = -1;
			cm.CurrentChanged += new EventHandler (CurrentChanged);
			cm.PositionChanged += new EventHandler (PositionChanged);
			cm.ItemChanged += new ItemChangedEventHandler (ItemChanged);

			Assert.AreEqual (0, cm.Position, "EndAddNew1");
			Assert.AreEqual (10, cm.Count, "EndAddNew2");

			cm.AddNew ();
			DebugWriteLine ("position = {0}", cm.Position);

			Assert.AreEqual (0, item_changed, "EndAddNew3");
			Assert.AreEqual (-1, item_changed_args.Index, "EndAddNew4");
			Assert.AreEqual (1, current_changed, "EndAddNew5");
			Assert.AreEqual (2, position_changed, "EndAddNew6");

			cm.EndCurrentEdit ();
			DebugWriteLine ("position = {0}", cm.Position);

			Assert.AreEqual (3, item_changed, "EndAddNew7");
			Assert.AreEqual (-1, item_changed_args.Index, "EndAddNew8");
			Assert.AreEqual (1, current_changed, "EndAddNew9");
			Assert.AreEqual (2, position_changed, "EndAddNew10");

			Assert.AreEqual (10, cm.Position, "EndAddNew11");
			Assert.AreEqual (11, cm.Count, "EndAddNew12");

			cm.CurrentChanged -= new EventHandler (CurrentChanged);
			cm.PositionChanged -= new EventHandler (PositionChanged);
		}

		void DataView_ListChanged (object sender, ListChangedEventArgs e)
		{
			DebugWriteLine ("{0} {1} {2}", e.ListChangedType, e.OldIndex, e.NewIndex);
		}

		// Support for ICancelNew interface
		[Test]
		public void EndAddNew2 ()
		{
			BindingContext bc = new BindingContext ();
			CancelAddNewList<int> list = new CancelAddNewList<int> ();
			list.Add (4);
			list.Add (6);

			CurrencyManager cm = (CurrencyManager)bc [list];

			Assert.AreEqual (false, list.EndNewCalled, "A1");
			Assert.AreEqual (false, list.CancelNewCalled, "A2");
			Assert.AreEqual (-1, list.LastIndex, "A3");

			cm.EndCurrentEdit ();
			Assert.AreEqual (true, list.EndNewCalled, "B1");
			Assert.AreEqual (false, list.CancelNewCalled, "B2");
			Assert.AreEqual (0, list.LastIndex, "B3");

			cm.Position = 1;
			list.Reset ();

			cm.EndCurrentEdit ();
			Assert.AreEqual (true, list.EndNewCalled, "C1");
			Assert.AreEqual (false, list.CancelNewCalled, "C2");
			Assert.AreEqual (1, list.LastIndex, "C3");
		}

		[Test]
		public void AddNew2 ()
		{
			if (TestHelper.RunningOnUnix) {
				Assert.Ignore ("Fails at the moment due to a System.Data constraint violation");
			}

			DataSet data_source = CreateRelatedDataSet ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Table1"] as CurrencyManager;

			DataView dv = cm.List as DataView;

			event_num = current_changed = position_changed = -1;
			cm.CurrentChanged += new EventHandler (CurrentChanged);
			cm.PositionChanged += new EventHandler (PositionChanged);
			cm.ItemChanged += new ItemChangedEventHandler (ItemChanged);
			dv.ListChanged += new ListChangedEventHandler (ListChanged);

			Assert.AreEqual (0, cm.Position, "AddNew1");
			Assert.AreEqual (10, cm.Count, "AddNew2");

			cm.AddNew ();

			Assert.AreEqual (10, cm.Position, "AddNew3");
			Assert.AreEqual (11, cm.Count, "AddNew4");

			// this does an implicit EndCurrentEdit
			cm.AddNew ();

			Assert.AreEqual (11, cm.Position, "AddNew5");
			Assert.AreEqual (12, cm.Count, "AddNew6");
		}


		DataSet CreateRelatedDataSetLarge ()
		{
			DataSet dataset = new DataSet ("CustomerSet");
			DataTable dt1 = new DataTable ("Customers");
			DataTable dt2 = new DataTable ("Orders");
			DataTable dt3 = new DataTable ("Addresses");
			DataColumn column;

			// customer table
			column = new DataColumn ("CustomerID");
			column.DataType = typeof (int);
			column.Unique = true;
			dt1.Columns.Add (column);

			column = new DataColumn ("CustomerName");
			column.DataType = typeof (string);
			column.Unique = false;
			dt1.Columns.Add (column);

			// order table
			column = new DataColumn ("OrderID");
			column.DataType = typeof (int);
			column.Unique = true;
			dt2.Columns.Add (column);

			column = new DataColumn ("ItemName");
			column.DataType = typeof (string);
			column.Unique = false;
			dt2.Columns.Add (column);

			column = new DataColumn ("CustomerID");
			column.DataType = typeof (int);
			column.Unique = false;
			dt2.Columns.Add (column);

			column = new DataColumn ("AddressID");
			column.DataType = typeof (int);
			column.Unique = false;
			dt2.Columns.Add (column);

			// address table
			column = new DataColumn ("AddressID");
			column.DataType = typeof (int);
			column.Unique = true;
			dt3.Columns.Add (column);

			column = new DataColumn ("AddressString");
			column.DataType = typeof (string);
			column.Unique = false;
			dt3.Columns.Add (column);

			column = new DataColumn ("CustomerID");
			column.DataType = typeof (int);
			column.Unique = false;
			dt3.Columns.Add (column);

			for (int i = 0; i < 10; i ++) {
				DataRow row = dt1.NewRow ();
				row["CustomerID"] = i;
				row["CustomerName"] = String.Format ("Customer Name #{0}", i);
				dt1.Rows.Add (row);
			}

			int ordernum = 0;
			for (int i = 0; i < 10; i ++) {
				for (int j = 0; j < (i < 5 ? 3 : 5); j ++) {
					DataRow row = dt2.NewRow ();
					row["OrderID"] = ordernum++;
					row["ItemName"] = String.Format ("Item order #{0}", j);
					row["CustomerID"] = i;
					row["AddressID"] = j;
					dt2.Rows.Add (row);
				}
			}

			int addressid = 0;
			for (int i = 0; i < 4; i ++) {
				for (int j = 0; j < 4; j ++) {
					DataRow row = dt3.NewRow ();
					row["AddressID"] = addressid++;
					row["AddressString"] = String.Format ("Customer Address {0}", j);
					row["CustomerID"] = i;
					dt3.Rows.Add (row);
				}
			}

			dataset.Tables.Add (dt1);
			dataset.Tables.Add (dt2);
			dataset.Tables.Add (dt3);
			dataset.Relations.Add ("Customer_Orders", dt1.Columns["CustomerID"], dt2.Columns["CustomerID"]);
			dataset.Relations.Add ("Customer_Addresses", dt1.Columns["CustomerID"], dt3.Columns["CustomerID"]);
			dataset.Relations.Add ("Address_Orders", dt3.Columns["AddressID"], dt2.Columns["AddressID"]);

			return dataset;
		}

		[Test]
		public void RelatedCurrencyManagerTest ()
		{
			DataSet data_source = CreateRelatedDataSetLarge ();
			BindingContext bc = new BindingContext ();
			CurrencyManager cm = bc [data_source, "Customers"] as CurrencyManager;
			CurrencyManager rcm = bc [data_source, "Customers.Customer_Orders"] as CurrencyManager;

			IList list = rcm.List;
			Assert.AreEqual (3, rcm.Count, "count1");
			Assert.AreEqual (3, list.Count, "listcount1");

			cm.Position = 1;
			Assert.AreEqual (3, rcm.Count, "count2");
			Assert.AreEqual (3, list.Count, "listcount2");

			cm.Position = 5;
			Assert.AreEqual (5, rcm.Count, "count3");
			Assert.AreEqual (3, list.Count, "listcount3");
		}

		[Test]
		public void TestCurrencyManagerBindings ()
		{
			DataSet data_source = CreateRelatedDataSetLarge ();
			BindingContext bc = new BindingContext ();

			CurrencyManager cm = bc [data_source] as CurrencyManager;

			DebugWriteLine ("cm properties:");
			foreach (PropertyDescriptor pd in cm.GetItemProperties ())
				DebugWriteLine (" + {0}", pd.Name);
			DebugWriteLine ();

			DebugWriteLine ("dataset:");
			DebugWriteLine ("cm = {0}", cm.GetType());
			DebugWriteLine ("cm.Count = {0}", cm.Count);
			cm.Position = 0;
			DebugWriteLine ("cm.Current = {0}", cm.Current);
			DebugWriteLine ("cm.Current properties");
			foreach (PropertyDescriptor pd in ((ICustomTypeDescriptor)cm.Current).GetProperties ())
				DebugWriteLine (" + {0}", pd.Name);
			DebugWriteLine ();

			cm = bc [data_source.Tables["Customers"]] as CurrencyManager;
			DebugWriteLine ("datatable:");
			DebugWriteLine ("cm = {0}", cm.GetType());
			DebugWriteLine ("cm.Count = {0}", cm.Count);
			cm.Position = 0;
			DebugWriteLine ("cm.Current = {0}", cm.Current);
			DebugWriteLine ("cm.Current properties");
			foreach (PropertyDescriptor pd in ((ICustomTypeDescriptor)cm.Current).GetProperties ())
				DebugWriteLine (" + {0}", pd.Name);

			DebugWriteLine ();

			DataViewManager vm = new DataViewManager (data_source);
			DebugWriteLine ("vm properties:");
			foreach (PropertyDescriptor pd in ((ITypedList)vm).GetItemProperties (null))
				DebugWriteLine (" + {0}", pd.Name);
			DebugWriteLine ();

		}

		Type GetFinalType (CurrencyManager cm)
		{
			FieldInfo fi = cm.GetType().GetField ("finalType", BindingFlags.NonPublic | BindingFlags.Instance);

			return (Type)fi.GetValue (cm);
		}

		[Test]
		public void FinalTypeTest ()
		{
			BindingContext bc = new BindingContext ();
			CurrencyManager cm;
			ArrayList al;
			DataSet data_source = CreateRelatedDataSetLarge ();

			/* empty arraylist */
			al = new ArrayList ();
			cm = bc[al] as CurrencyManager;
			Assert.AreEqual (typeof (ArrayList), GetFinalType (cm), "A1");

			/* arraylist with a string element*/
			al = new ArrayList ();
			al.Add ("hi");
			cm = bc[al] as CurrencyManager;
			Assert.AreEqual (typeof (ArrayList), GetFinalType (cm), "A2");

			/* string array */
			string[] s = new string[1];
			s[0] = "hi";
			cm = bc[s] as CurrencyManager;
			Assert.AreEqual (typeof (string[]), GetFinalType (cm), "A3");

			/* dataview */
			cm = bc [data_source, "Customers"] as CurrencyManager;
			Assert.AreEqual (typeof (DataView), GetFinalType (cm), "A4");

			/* relatedview */
			cm = bc [data_source, "Customers.Customer_Orders"] as CurrencyManager;
			/* on MS this is a RelatedView, on Mono a RelatedDataView.  both subclass from DataView, so let's check that. */
			Assert.IsFalse (typeof (DataView) == GetFinalType (cm), "A5");
			Assert.IsTrue (typeof (DataView).IsAssignableFrom (GetFinalType (cm)), "A6");
		}

		[Test]
		public void ListChangedEventTest ()
		{
			Control c = new Control ();
			c.BindingContext = new BindingContext ();
			c.CreateControl ();

			BindingListChild<MockItem> binding_list = new BindingListChild<MockItem> ();
			CurrencyManager currency_manager = (CurrencyManager)c.BindingContext [binding_list];
			currency_manager.ListChanged += new ListChangedEventHandler(ListChangedEvent);

			ClearListChangedLog ();

			MockItem item = binding_list.AddNew ();
			binding_list.EndNew (binding_list.IndexOf (item));
			Assert.IsTrue (list_changed_called, "#A1");
			Assert.AreEqual (ListChangedType.ItemAdded, list_changed_args.ListChangedType, "#A2");

			ClearListChangedLog ();

			binding_list.Insert (0, new MockItem ());
			Assert.IsTrue (list_changed_called, "#B1");
			Assert.AreEqual (ListChangedType.ItemAdded, list_changed_args.ListChangedType, "#B2");

			ClearListChangedLog ();

			binding_list.RemoveAt (0);
			Assert.IsTrue (list_changed_called, "#D1");
			Assert.AreEqual (ListChangedType.ItemDeleted, list_changed_args.ListChangedType, "#D2");

			ClearListChangedLog ();

			binding_list [0] = new MockItem ();
			Assert.IsTrue (list_changed_called, "#E1");
			Assert.AreEqual (ListChangedType.ItemChanged, list_changed_args.ListChangedType, "#E2");

			ClearListChangedLog ();

			binding_list.DoResetItem (0);
			Assert.IsTrue (list_changed_called, "#F1");
			Assert.AreEqual (ListChangedType.ItemChanged, list_changed_args.ListChangedType, "#F2");

			ClearListChangedLog ();

			binding_list.DoResetBinding ();
			Assert.IsTrue (list_changed_called, "#G1");
			Assert.AreEqual (ListChangedType.Reset, list_changed_args.ListChangedType, "#G2");

			binding_list.Clear ();
			Assert.IsTrue (list_changed_called, "#F1");
			Assert.AreEqual (ListChangedType.Reset, list_changed_args.ListChangedType, "#F2");

			currency_manager.ListChanged -= ListChangedEvent;
		}

		void ClearListChangedLog ()
		{
			list_changed_called = false;
			list_changed_args = null;
		}

		public class BindingListChild<T> : BindingList<T>
		{
			public void DoResetItem (int position)
			{
				ResetItem (position);
			}

			public void DoResetBinding ()
			{
				ResetBindings ();
			}
		}

		[global::System.Diagnostics.Conditional("DebugCurrencyManager")]
		void DebugWriteLine ()
		{
			Console.WriteLine ();
		}

		[global::System.Diagnostics.Conditional("DebugCurrencyManager")]
		void DebugWriteLine (string text, params object[] p)
		{
			Console.WriteLine (text, p);
		}
	}
}
