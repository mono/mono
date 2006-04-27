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
using System.Windows.Forms;

using NUnit.Framework;

namespace MonoTests.System.Windows.Forms {

	[TestFixture]
	public class CurrencyManagerTest {

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
	}
}


