// DataRowTest.cs - NUnit Test Cases for System.DataRow
//
// Authors:
//   Franklin Wise (gracenote@earthlink.net)
//   Daniel Morgan <danmorg@sc.rr.com>
//   Roopa Wilson (rowilson@novell.com)
//
// (C) Copyright 2002 Franklin Wise
// (C) Copyright 2003 Daniel Morgan
// (C) Copyright 2003 Martin Willemoes Hansen
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Data;

namespace MonoTests.System.Data
{
	[TestFixture]
	public class DataRowTest : Assertion {
	        private DataTable table;                                                
                private DataRow row;    

		[SetUp]
		public void GetReady() {
			table = MakeTable ();                                           
                        row = table.NewRow ();                                          
                        row ["FName"] = "Hello";                                        
                        row ["LName"] = "World";                                        
                        table.Rows.Add (row);  
		}
		
		private DataTable MakeTable ()
                {
                        DataTable namesTable = new DataTable ("Names");
                        DataColumn idColumn = new  DataColumn ();
                                                                                                    
                                                                                                    
                        idColumn.DataType = Type.GetType ("System.Int32");
                        idColumn.ColumnName = "Id";
                        idColumn.AutoIncrement = true;
                        namesTable.Columns.Add (idColumn);
                                                                                                    
                                                                                                    
                        DataColumn fNameColumn = new DataColumn ();
                        fNameColumn.DataType = Type.GetType ("System.String");
                        fNameColumn.ColumnName = "Fname";
                        fNameColumn.DefaultValue = "Fname";
                        namesTable.Columns.Add (fNameColumn);
                                                                                                    
                        DataColumn lNameColumn = new DataColumn ();
                        lNameColumn.DataType = Type.GetType ("System.String");
                        lNameColumn.ColumnName = "LName";
                        lNameColumn.DefaultValue="LName";
                        namesTable.Columns.Add (lNameColumn);
                                                                                                    
                                                                                                    
                        // Set the primary key for the table
                        DataColumn [] keys = new DataColumn [1];
                        keys [0] = idColumn;
                        namesTable.PrimaryKey = keys;
                        // Return the new DataTable.
                        return namesTable;
                }

		[Test]
                public void SetColumnErrorTest ()
                {
                        string errorString;
                        errorString = "Some error!";
                        // Set the error for the specified column of the row.
                        row.SetColumnError (1, errorString);
                        GetColumnErrorTest ();
                        GetAllErrorsTest ();
                }

                private  void GetColumnErrorTest ()
                {
                        // Print the error of a specified column.
                        AssertEquals ("#A01", "Some error!", row.GetColumnError (1));
                }

                private void GetAllErrorsTest ()
                {
                        DataColumn [] colArr;
                                                                                                    
                        if (row.HasErrors) {
                                colArr = row.GetColumnsInError ();
                                                                                                    
                                for (int i = 0; i < colArr.Length; i++) {
                                        AssertEquals ("#A02", table.Columns [1], colArr [i]);
                                }
                                row.ClearErrors ();
                        }
                }

                [Test]
                public void DeleteRowTest ()
                {
                        DataRow newRow;
                                                                                                    
                                                                                                    
                        for (int i = 1; i <= 2; i++) {
                                newRow = table.NewRow ();
                                newRow ["FName"] = "Name " + i;
                                newRow ["LName"] = " Last Name" + i;
                                table.Rows.Add (newRow);
                        }
                        table.AcceptChanges ();
                                                                                                    
                        int cnt = 1;
                        for (int i = 1; i < table.Rows.Count; i++) {
                                DataRow r = table.Rows [i];
                                AssertEquals ("#A03", "Name " + cnt, r ["fName"]);
                                cnt++;
                        }
                                                                                                    
                                                                                                    
                        // Create a DataView with the table.
                        DataRowCollection rc = table.Rows;
                        rc [0].Delete ();
		        rc [2].Delete ();
                                                                                                    
                                                                                                    
                        AssertEquals ("#A04", "Deleted", rc [0].RowState.ToString ());
                        AssertEquals ("#A05", "Deleted", rc [2].RowState.ToString ());
                                                                                                    
                                                                                                    
                        // Accept changes
                        table.AcceptChanges ();
                        AssertEquals ("#A06", "Name 1", (table.Rows [0]) [1]);
                        try  {
                                object o = rc [2];
                                Fail ("#A07");
                        }
                        catch (Exception e) {
				// Never premise English.
                                //AssertEquals ("#A08", "There is no row at position 2.", e.Message);
                        }
                }

                [Test]
                public void EditModeTest ()
                {
                        try {
                                //Clear all existing values from table
                                for (int i = 0; i < table.Rows.Count; i++) {
                                        table.Rows[i].Delete ();
                                }
                                table.AcceptChanges ();
                                row = table.NewRow ();
                                row["FName"] = "My FName";
				table.Rows.Add (row);
                                                                                                    
                                                                                                    
                                // Stage 1
                                //Initially: After Add (Row) But Before Accept Changes");
                                AssertEquals ("#A09", "My FName", row [1, DataRowVersion.Default]);
                                AssertEquals ("#A10", "LName", row [2, DataRowVersion.Default]);
                                                                                                    
                                AssertEquals ("#A11", "My FName", row [1, DataRowVersion.Current]);
                                AssertEquals ("#A12", "LName", row [2, DataRowVersion.Current]);
                                                                                                    
                                try {
                                      object o = row [1, DataRowVersion.Original];
                                      o = row [1, DataRowVersion.Proposed];
                                        Fail ("#A13");
                                }
                                catch (Exception e) {
                                        if (e.GetType () != typeof (AssertionException)) {
                                                AssertEquals ("#A14", typeof (VersionNotFoundException), e.GetType ());
                                        }
                                }
                                                                                                    
                                // Stage 2
                                //After Accept Changes
                                table.AcceptChanges ();
                                AssertEquals ("#A15", "My FName", row [1, DataRowVersion.Default]);
                                AssertEquals ("#A16", "LName", row [2, DataRowVersion.Default]);
                                                                                                    
                                                                                                    
                                AssertEquals ("#A17", "My FName", row [1, DataRowVersion.Current]);
                                AssertEquals ("#A18", "LName", row [2, DataRowVersion.Current]);
                                
				try {
                                      object o = row [1, DataRowVersion.Proposed];
                                        Fail ("#A19");
                                }
                                catch (Exception e) {
                                        if (e.GetType () != typeof (AssertionException)) {
                                                AssertEquals ("#A20", typeof (VersionNotFoundException), e.GetType ());
                                        }
                                }
                                                                                                    
                                                                                                                                                                                                         
                                // Stage 3                                 // Edit Mode
                                table.Rows [0].BeginEdit ();
                                table.Rows [0] ["LName"] = "My LName";
                                                                                                    
                                AssertEquals ("#A21", "My FName", row [1, DataRowVersion.Default]);
                                AssertEquals ("#A22", "My LName", row [2, DataRowVersion.Default]);
                                                                                                                                                                                                         
                                AssertEquals ("#A23", "My FName", row [1, DataRowVersion.Current]);
                                AssertEquals ("#A24", "LName", row [2, DataRowVersion.Current]);
                                                                                                    
                                                                                                    
                                AssertEquals ("#A25", "My FName", row [1, DataRowVersion.Original]);                                AssertEquals ("#A26", "LName", row [2, DataRowVersion.Original]);
                                                                                                    
                                AssertEquals ("#A26", "My FName", row [1, DataRowVersion.Proposed]);
	                        AssertEquals ("#A27", "My LName", row [2, DataRowVersion.Proposed]);                                                                                                    
                                                                                                    
                                                                                                    
                                // Stage 4
                                //After Edit sessions
                                for (int i=0; i < table.Rows.Count;i++)
                                        table.Rows [i].EndEdit ();
                                AssertEquals ("#A28", "My FName", row [1, DataRowVersion.Default]);
                                AssertEquals ("#A29", "My LName", row [2, DataRowVersion.Default]);
                                                                                                                                                                                                         
                                AssertEquals ("#A30", "My FName", row [1, DataRowVersion.Original]);                                AssertEquals ("#A31", "LName", row [2, DataRowVersion.Original]);
                                                                                                    
                                                                                                    
                                AssertEquals ("#A32", "My FName", row [1, DataRowVersion.Current]);
                                AssertEquals ("#A33", "My LName", row [2, DataRowVersion.Current]);
                                                                                                    
                                try {
                                      object o = row [1, DataRowVersion.Proposed];
                                        Fail ("#A34");
                                }
                                catch (Exception e) {
                                        if (e.GetType ()!=typeof (AssertionException)) {
                                                AssertEquals ("#A35", typeof (VersionNotFoundException), e.GetType ());
                                        }
                                }
                                                                                                    
                                //Stage 5
                                //After Accept Changes
				table.AcceptChanges ();
				AssertEquals ("#A36", "My FName", row [1, DataRowVersion.Default]);
                                AssertEquals ("#A37", "My LName", row [2, DataRowVersion.Default]);
                                                                                                    
                                                                                                    
                                AssertEquals ("#A38", "My FName", row [1, DataRowVersion.Original]);                                AssertEquals ("#A39", "My LName", row [2, DataRowVersion.Original]);                                                                                                    
                                                                                                    
                                AssertEquals ("#A40", "My FName", row [1, DataRowVersion.Current]);
                                AssertEquals ("#A41", "My LName", row [2, DataRowVersion.Current]);
                                                                                                    
                                                                                                    
                                try {
                                      object o = row [1, DataRowVersion.Proposed];
                                        Fail ("#A42");
                                }
                                catch (Exception e) {
                                                if (e.GetType () != typeof (AssertionException)) {
                                                        AssertEquals ("#A43", typeof (VersionNotFoundException),
                                                                e.GetType ());
                                                }
                                        }
                                                                                                    
                                                                                                    
                        }
                        catch (Exception e){
//                              Console.WriteLine (e + "" + e.StackTrace);
                        }
                }                                                                                                     
                [Test]
                public void ParentRowTest ()
                {

                        //Clear all existing values from table
                        for (int i = 0; i < table.Rows.Count; i++) {
                                        table.Rows[i].Delete ();
                        }
                        table.AcceptChanges ();
                        row = table.NewRow ();
                        row["FName"] = "My FName";
                        row["Id"] = 0;
                        table.Rows.Add (row);
                                                                                                    
                        DataTable tableC = new DataTable ("Child");
                        DataColumn colC;
                        DataRow rowC;
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        colC.AutoIncrement=true;
                        tableC.Columns.Add (colC);
                                                                                                    
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.String");
                        colC.ColumnName = "Name";
                        tableC.Columns.Add (colC);
                                                                                                    
                        rowC = tableC.NewRow ();
                        rowC["Name"] = "My FName";
                        tableC.Rows.Add (rowC);
                        DataSet ds = new DataSet ();
                        ds.Tables.Add (table);
                        ds.Tables.Add (tableC);
                        DataRelation dr = new DataRelation ("PO", table.Columns ["Id"], tableC.Columns ["Id"]);
                        ds.Relations.Add (dr);
                                                                                                    
                        rowC.SetParentRow (table.Rows [0], dr);
                                                                                                    
                        AssertEquals ("#PRT-01", table.Rows [0], (tableC.Rows [0]).GetParentRow (dr));
			AssertEquals ("#PRT-02", tableC.Rows [0], (table.Rows [0]).GetChildRows (dr) [0]);

                        ds.Relations.Clear ();
                        dr = new DataRelation ("PO", table.Columns ["Id"], tableC.Columns ["Id"], false);
                        ds.Relations.Add (dr);
                        rowC.SetParentRow (table.Rows [0], dr);
                        AssertEquals ("#PRT-03", table.Rows [0], (tableC.Rows [0]).GetParentRow (dr));
			AssertEquals ("#PRT-04", tableC.Rows [0], (table.Rows [0]).GetChildRows (dr) [0]);

                        ds.Relations.Clear ();
                        dr = new DataRelation ("PO", table.Columns ["Id"], tableC.Columns ["Id"], false);
                        tableC.ParentRelations.Add (dr);
                        rowC.SetParentRow (table.Rows [0]);
                        AssertEquals ("#PRT-05", table.Rows [0], (tableC.Rows [0]).GetParentRow (dr));
                        AssertEquals ("#PRT-06", tableC.Rows [0], (table.Rows [0]).GetChildRows (dr) [0]);
						
                } 

                [Test]
                public void ParentRowTest2 ()
                {
                        DataSet ds = new DataSet ();
                        DataTable tableP = ds.Tables.Add ("Parent");
                        DataTable tableC = ds.Tables.Add ("Child");
                        DataColumn colC;
                        DataRow rowC;
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        colC.AutoIncrement = true;
                        tableP.Columns.Add (colC);
                        
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        tableC.Columns.Add (colC);
 
                        row = tableP.Rows.Add (new object [0]);
                        rowC = tableC.NewRow ();
 
                        ds.EnforceConstraints = false;
                        DataRelation dr = new DataRelation ("PO", tableP.Columns ["Id"], tableC.Columns ["Id"]);
                        ds.Relations.Add (dr);

                        rowC.SetParentRow (row, dr);
                        DataRow [] rows = rowC.GetParentRows (dr);

                        AssertEquals ("#A49", 1, rows.Length);
                        AssertEquals ("#A50", tableP.Rows [0], rows [0]);

                        try{
                                rows = row.GetParentRows (dr);
                        }catch(InvalidConstraintException){
                                //Test done
                                return ;
                        }catch(Exception e){
                                Fail("#A51, InvalidConstraintException expected, got : " + e);
                        }
                        
                        Fail("#A52, InvalidConstraintException expected but got none.");
                }

                [Test]
                public void ChildRowTest ()
                {

                        //Clear all existing values from table
                        for (int i = 0; i < table.Rows.Count; i++) {
                                        table.Rows [i].Delete ();
                        }
                        table.AcceptChanges ();
                        row = table.NewRow ();
                        row ["FName"] = "My FName";
                        row ["Id"] = 0;
                        table.Rows.Add (row);
                                                                                                    
                        DataTable tableC = new DataTable ("Child");
                        DataColumn colC;
                        DataRow rowC;

                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        colC.AutoIncrement = true;
                        tableC.Columns.Add (colC);
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.String");
                        colC.ColumnName = "Name";
                        tableC.Columns.Add (colC);
                                                                                                    
                        rowC = tableC.NewRow ();
                        rowC ["Name"] = "My FName";
                        tableC.Rows.Add (rowC);
                        DataSet ds = new DataSet ();
                        ds.Tables.Add (table);
                        ds.Tables.Add (tableC);
                        DataRelation dr = new DataRelation ("PO", table.Columns ["Id"], tableC.Columns ["Id"]);
                        ds.Relations.Add (dr);
                                                                                                    
                        rowC.SetParentRow (table.Rows [0], dr);
                                                                                                    
                        DataRow [] rows = (table.Rows [0]).GetChildRows (dr);

                        AssertEquals ("#A45", 1, rows.Length);
                        AssertEquals ("#A46", tableC.Rows [0], rows [0]);
                        
                } 

                [Test]
                public void ChildRowTest2 ()
                {
                        DataSet ds = new DataSet ();
                        DataTable tableP = ds.Tables.Add ("Parent");
                        DataTable tableC = ds.Tables.Add ("Child");
                        DataColumn colC;
                        DataRow rowC;
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        colC.AutoIncrement = true;
                        tableP.Columns.Add (colC);
                        
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        tableC.Columns.Add (colC);

                        row = tableP.NewRow ();
                        rowC = tableC.Rows.Add (new object [0]);

                        ds.EnforceConstraints = false;
                        DataRelation dr = new DataRelation ("PO", tableP.Columns ["Id"], tableC.Columns ["Id"]);
                        ds.Relations.Add (dr);

                        rowC.SetParentRow (row, dr);
                        DataRow [] rows = row.GetChildRows (dr);
                        
                        AssertEquals ("#A47", 1, rows.Length);
                        AssertEquals ("#A48", tableC.Rows [0], rows [0]);

                        try{
                            rows = rowC.GetChildRows (dr);
                        }catch(InvalidConstraintException){
                            //Test done
                            return ;
                        }catch(Exception e){
                            Fail("#A53, InvalidConstraintException expected, got : " + e);
                        }
                        
                        Fail("#A54, InvalidConstraintException expected but got none.");
                }

		 [Category ("NotWorking")] //Mismatch in Exception namespace/class reference
                [Test]
                public void ParentChildRowVersionTest ()
                {
                        DataSet ds = new DataSet ();
                        DataTable tableP = ds.Tables.Add ("Parent");
                        DataTable tableC = ds.Tables.Add ("Child");
                        DataColumn colC;
                        DataRow rowC;
                                                                                                    
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        colC.AutoIncrement = true;
                        tableP.Columns.Add (colC);
                        
                        colC = new DataColumn ();
                        colC.DataType = Type.GetType ("System.Int32");
                        colC.ColumnName = "Id";
                        tableC.Columns.Add (colC);

                        row = tableP.NewRow ();
                        rowC = tableC.Rows.Add (new object [0]);

                        ds.EnforceConstraints = false;
                        DataRelation dr = new DataRelation ("PO", tableP.Columns ["Id"], tableC.Columns ["Id"]);
                        ds.Relations.Add (dr);

                        rowC.SetParentRow (row, dr);
                        DataRow [] rows;

                        try {
                            rows = row.GetChildRows (dr, DataRowVersion.Current);
                        }catch (VersionNotFoundException v) {
                            //Check for GetParentRows
                            try{
                                //Child Row should be in Detached state for the next test
                                rowC = tableC.NewRow();

                                rows = rowC.GetParentRows (dr, DataRowVersion.Current);
                            }catch (VersionNotFoundException v2) {
                                //Test Done
                                return ;
                            }catch (Exception e){
                                Fail ("#A55, VersionNotFoundException expected, got : " + e);
                            }
                            Fail ("#A56, VersionNotFoundException expected but got none.");
                        }catch (Exception e){
                            Fail ("#A57, VersionNotFoundException expected, got : " + e);
                        }
                        
                        Fail("#A58, VersionNotFoundException expected but got none.");
                }

		// tests item at row, column in table to be DBNull.Value
		private void DBNullTest (string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, DBNull.Value, val);
		}

		// tests item at row, column in table to be null
		private void NullTest (string message, DataTable dt, int row, int column) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, null, val);
		}

		// tests item at row, column in table to be 
		private void ValueTest (string message, DataTable dt, int row, int column, object value) 
		{
			object val = dt.Rows[row].ItemArray[column];
			AssertEquals(message, value, val);
		}

		// test set null, DBNull.Value, and ItemArray short count
		[Test]
		public void NullInItemArray () 
		{
			string zero = "zero";
			string one = "one";
			string two = "two";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));
			table.Columns.Add(new DataColumn(one, typeof(string)));
			table.Columns.Add(new DataColumn(two, typeof(string)));

			object[] obj = new object[3];
			// -- normal -----------------
			obj[0] = zero;
			obj[1] = one;
			obj[2] = two;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = "one"
			//   table.Rows[0].ItemArray.ItemArray[2] = "two"
			
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR1: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[1].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR2: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//   table.Rows[2].ItemArray.ItemArray[2] = "two"

			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR3: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			obj[1] = def;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = "def"
			//   table.Rows[3].ItemArray.ItemArray[2] = DBNull.Value;
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR4: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR5: normal value test", table, 0, 0, zero);
			ValueTest("DR6: normal value test", table, 0, 1, one);
			ValueTest("DR7: normal value test", table, 0, 2, two);

			// -- null ----------
			ValueTest("DR8: null value test", table, 1, 0, zero);
			ValueTest("DR9: null value test", table, 1, 1, DBNull.Value);
			ValueTest("DR10: null value test", table, 1, 2, two);

			// -- DBNull.Value -------------
			ValueTest("DR11: DBNull.Value value test", table, 2, 0, zero);
			ValueTest("DR12: DBNull.Value value test", table, 2, 1, DBNull.Value);
			ValueTest("DR13: DBNull.Value value test", table, 2, 2, two);

			// -- object array smaller than number of columns -----
			ValueTest("DR14: array smaller value test", table, 3, 0, abc);
			ValueTest("DR15: array smaller value test", table, 3, 1, def);
			ValueTest("DR16: array smaller value test", table, 3, 2, DBNull.Value);
		}
	
		// test DefaultValue when setting ItemArray
		[Test]
		public void DefaultValueInItemArray () {		
			string zero = "zero";

			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn("zero", typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.DefaultValue = 15;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR17: Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null ----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 15
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR18: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the v
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR19: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = DBNull.Value
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR20: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR20: normal value test", table, 0, 0, zero);
			ValueTest("DR21: normal value test", table, 0, 1, 8);
			
			// -- null ----------
			ValueTest("DR22: null value test", table, 1, 0, zero);
			ValueTest("DR23: null value test", table, 1, 1, 15);
			
			// -- DBNull.Value -------------
			ValueTest("DR24: DBNull.Value value test", table, 2, 0, zero);
			DBNullTest("DR25: DBNull.Value value test", table, 2, 1);
			
			// -- object array smaller than number of columns -----
			ValueTest("DR26: array smaller value test", table, 3, 0, abc);
			ValueTest("DR27: array smaller value test", table, 3, 1, 15);
		}

		// test AutoIncrement when setting ItemArray
		[Test]
		public void AutoIncrementInItemArray () {
			string zero = "zero";
			string num = "num";
			
			DataTable table = new DataTable();
			table.Columns.Add(new DataColumn(zero, typeof(string)));		
			
			DataColumn column = new DataColumn("num", typeof(int));
			column.AutoIncrement = true;
			table.Columns.Add(column);
			
			object[] obj = new object[2];
			// -- normal -----------------
			obj[0] = "zero";
			obj[1] = 8;
			// results:
			//   table.Rows[0].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[0].ItemArray.ItemArray[1] = 8
						
			DataRow row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e1) {
				Fail("DR28:  Exception Caught: " + e1);
			}
			
			table.Rows.Add(row);

			// -- null 1----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 9
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR29:  Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 2----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 10
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR30: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- null 3----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 11
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR31: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- DBNull.Value -------------
			obj[1] = DBNull.Value;
			// results:
			//   table.Rows[2].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[2].ItemArray.ItemArray[1] = DBNull.Value
			//      even though internally, the AutoIncrement value
			//      is incremented
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR32: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- null 4----------
			obj[1] = null;
			// results:
			//   table.Rows[1].ItemArray.ItemArray[0] = "zero"
			//   table.Rows[1].ItemArray.ItemArray[1] = 13
			
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e2) {
				Fail("DR48: Exception Caught: " + e2);
			}
			
			table.Rows.Add(row);

			// -- object array smaller than number of columns -----
			string abc = "abc";
			string def = "def";
			obj = new object[2];
			obj[0] = abc;
			// results:
			//   table.Rows[3].ItemArray.ItemArray[0] = "abc"
			//   table.Rows[3].ItemArray.ItemArray[1] = 14
						
			row = table.NewRow();
			
			try {
				row.ItemArray = obj;
			}
			catch(Exception e3) {
				Fail("DR33: Exception Caught: " + e3);
			}
			
			table.Rows.Add(row);

			// -- normal -----------------
			ValueTest("DR34: normal value test", table, 0, 0, zero);
			ValueTest("DR35: normal value test", table, 0, 1, 8);
			
			// -- null 1----------
			ValueTest("DR36: null value test", table, 1, 0, zero);
			ValueTest("DR37: null value test", table, 1, 1, 9);

			// -- null 2----------
			ValueTest("DR38: null value test", table, 2, 0, zero);
			ValueTest("DR39: null value test", table, 2, 1, 10);

			// -- null 3----------
			ValueTest("DR40: null value test", table, 3, 0, zero);
			ValueTest("DR41: null value test", table, 3, 1, 11);

			// -- DBNull.Value -------------
			ValueTest("DR42: DBNull.Value value test", table, 4, 0, zero);
			ValueTest("DR43: DBNull.Value value test", table, 4, 1, DBNull.Value);

			// -- null 4----------
			ValueTest("DR44: null value test", table, 5, 0, zero);
			ValueTest("DR45: null value test", table, 5, 1, 13);

			// -- object array smaller than number of columns -----
			ValueTest("DR46: array smaller value test", table, 6, 0, abc);
			ValueTest("DR47: array smaller value test", table, 6, 1, 14);
		}

		[Test]
		public void AutoIncrementColumnIntegrity ()
		{
			// AutoIncrement-column shouldn't raise index out of range
			// exception because of size mismatch of internal itemarray.
			DataTable dt = new DataTable ();
			dt.Columns.Add ("foo");
			dt.Rows.Add (new object [] {"value"});
			DataColumn col = new DataColumn ("bar");
			col.AutoIncrement = true;
			dt.Columns.Add (col);
			dt.Rows [0] [0] = "test";
		}

		[Test]
		public void EnforceConstraint ()
		{
			 int id = 100;
   		        // Setup stuff
		        DataSet ds = new DataSet();
		        DataTable parent = ds.Tables.Add("parent");
		        parent.Columns.Add("id", typeof(int));
		        DataTable child = ds.Tables.Add("child");
		        child.Columns.Add("idref", typeof(int));
		        Constraint uniqueId = null;
		        parent.Constraints.Add(uniqueId = new UniqueConstraint("uniqueId",
                                      new DataColumn[] {parent.Columns["id"]}, true));
			ForeignKeyConstraint fkc = new ForeignKeyConstraint("ParentChildConstraint",                                      new DataColumn[] { parent.Columns["id"] },
				      new DataColumn[] { child.Columns["idref"]});
        
		        child.Constraints.Add(fkc);
        
		        DataRelation relateParentChild = new DataRelation("relateParentChild",
                                             new DataColumn[] {parent.Columns["id"] },
                                             new DataColumn[] {child.Columns["idref"] },
                                             false);
		        ds.Relations.Add(relateParentChild);
        
			ds.EnforceConstraints = false;
		        DataRow parentRow = parent.Rows.Add(new object[] { id });
		        DataRow childRow = child.Rows.Add(new object[] { id });
		        if (parentRow == childRow.GetParentRow(relateParentChild)) {
		            foreach(DataColumn dc in parent.Columns)
				AssertEquals(100,parentRow[dc]);
		            
		        }
		            		
        
    		}

		[Test]
		[ExpectedException (typeof (RowNotInTableException))]
		public void DetachedRowItemException ()
		{
			DataTable dt = new DataTable ("table");
			dt.Columns.Add ("col");
			dt.Rows.Add ((new object [] {"val"}));

			DataRow dr = dt.NewRow ();
			AssertEquals (DataRowState.Detached, dr.RowState);
			dr.CancelEdit ();
			AssertEquals (DataRowState.Detached, dr.RowState);
			object o = dr ["col"];
		}
	}
}
