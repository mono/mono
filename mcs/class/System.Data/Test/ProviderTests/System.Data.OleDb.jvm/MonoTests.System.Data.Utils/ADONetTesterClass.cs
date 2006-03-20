// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.Data;
using System.Data.OleDb ;
using System.IO;
using System.Collections;
using MonoTests.System.Data.Utils.Data;
using Sys = System;

namespace MonoTests.System.Data.Utils {
	public class ADONetTesterClass : GHTBase {
		#region " Base Constructors "
		protected ADONetTesterClass(Sys.IO.TextWriter Logger, bool LogOnSuccess) : base(Logger,LogOnSuccess){}
		protected ADONetTesterClass(bool LogOnSuccess):base(Console.Out, LogOnSuccess){}
		protected ADONetTesterClass():base(Console.Out, false){}

		#endregion

		private MonoTests.System.Data.Utils.DataBaseServer DBType ;


		#region "-----------  Build Update Commands --------------"
		protected void OleDbDataAdapter_BuildUpdateCommands(ref Sys.Data.OleDb.OleDbDataAdapter oleDBda) {
			Sys.Data.OleDb.OleDbConnection Conn = oleDBda.SelectCommand.Connection;

			oleDBda.DeleteCommand = new Sys.Data.OleDb.OleDbCommand();
			oleDBda.InsertCommand = new Sys.Data.OleDb.OleDbCommand();
			oleDBda.UpdateCommand = new Sys.Data.OleDb.OleDbCommand();

			oleDBda.DeleteCommand.Connection = Conn;
			oleDBda.InsertCommand.Connection = Conn;
			oleDBda.UpdateCommand.Connection = Conn;

			oleDBda.DeleteCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("EmployeeID",Sys.Data.OleDb.OleDbType.Integer)) ;
			oleDBda.DeleteCommand.Parameters["EmployeeID"].SourceVersion = DataRowVersion.Original;
			oleDBda.DeleteCommand.Parameters["EmployeeID"].SourceColumn = "EmployeeID";

			oleDBda.InsertCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("EmployeeID",Sys.Data.OleDb.OleDbType.Integer));
			oleDBda.InsertCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("LastName",Sys.Data.OleDb.OleDbType.VarWChar ,20));
			oleDBda.InsertCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("FirstName",Sys.Data.OleDb.OleDbType.VarWChar,10));
			oleDBda.InsertCommand.Parameters["EmployeeID"].SourceColumn = "EmployeeID";
			oleDBda.InsertCommand.Parameters["LastName"].SourceColumn = "LastName";
			oleDBda.InsertCommand.Parameters["FirstName"].SourceColumn = "FirstName";

			oleDBda.UpdateCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("Title",Sys.Data.OleDb.OleDbType.VarWChar,30));
			oleDBda.UpdateCommand.Parameters.Add(new Sys.Data.OleDb.OleDbParameter("EmployeeID",Sys.Data.OleDb.OleDbType.Integer));
			oleDBda.UpdateCommand.Parameters["EmployeeID"].SourceColumn = "EmployeeID";
			oleDBda.UpdateCommand.Parameters["Title"].SourceColumn = "Title";


			//for OleDB, ODBC
			string deleteSQL = "DELETE FROM Employees WHERE EmployeeID = ?";
			string insertSQL = "INSERT INTO Employees (EmployeeID, LastName, FirstName) VALUES (?, ?, ?)";
			string updateSQL = "UPDATE Employees SET Title = ? WHERE EmployeeID = ?";

			oleDBda.DeleteCommand.CommandText = deleteSQL;
			oleDBda.InsertCommand.CommandText = insertSQL;
			oleDBda.UpdateCommand.CommandText = updateSQL;

		}
		protected void SqlDataAdapter_BuildUpdateCommands(ref Sys.Data.SqlClient.SqlDataAdapter Sqlda) {
			Sys.Data.SqlClient.SqlConnection Conn = Sqlda.SelectCommand.Connection;

			Sqlda.DeleteCommand = new Sys.Data.SqlClient.SqlCommand();
			Sqlda.InsertCommand = new Sys.Data.SqlClient.SqlCommand();
			Sqlda.UpdateCommand = new Sys.Data.SqlClient.SqlCommand();

			Sqlda.DeleteCommand.Connection = Conn;
			Sqlda.InsertCommand.Connection = Conn;
			Sqlda.UpdateCommand.Connection = Conn;

			Sqlda.DeleteCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@EmployeeID",DbType.Int32)) ;
			Sqlda.DeleteCommand.Parameters["@EmployeeID"].SourceVersion = DataRowVersion.Original;
			Sqlda.DeleteCommand.Parameters["@EmployeeID"].SourceColumn = "EmployeeID";

			Sqlda.InsertCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@EmployeeID",DbType.Int32));
			Sqlda.InsertCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@LastName",Sys.Data.SqlDbType.VarChar ,20));
			Sqlda.InsertCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@FirstName",Sys.Data.SqlDbType.VarChar ,10));
			Sqlda.InsertCommand.Parameters["@EmployeeID"].SourceColumn = "EmployeeID";
			Sqlda.InsertCommand.Parameters["@LastName"].SourceColumn = "LastName";
			Sqlda.InsertCommand.Parameters["@FirstName"].SourceColumn = "FirstName";

			Sqlda.UpdateCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@Title",Sys.Data.SqlDbType.VarChar,30));
			Sqlda.UpdateCommand.Parameters.Add(new Sys.Data.SqlClient.SqlParameter("@EmployeeID",DbType.Int32));
			Sqlda.UpdateCommand.Parameters["@EmployeeID"].SourceColumn = "EmployeeID";
			Sqlda.UpdateCommand.Parameters["@Title"].SourceColumn = "Title";

			//for Sql Client
			string deleteSql = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID";
			string insertSql = "INSERT INTO Employees (EmployeeID, LastName, FirstName) VALUES (@EmployeeID, @LastName, @FirstName)";
			string updateSql = "UPDATE Employees SET Title = @Title WHERE EmployeeID = @EmployeeID";

			Sqlda.DeleteCommand.CommandText = deleteSql;
			Sqlda.InsertCommand.CommandText = insertSql;
			Sqlda.UpdateCommand.CommandText = updateSql;

		}
		#endregion

		#region "-----------  Sys.Data.Common.DBDataAdapter --------------"

		#region " DBDataAdapter - Fill / Fill Schema "

		protected void DbDataAdapter_Fill_Ds(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult, dsExpected ;
			int ExpectedRowsCount,ResultRowsCount;
			DataSet ds = new DataSet();

			ExpectedRowsCount = ReadDBData_Fill(dbDA,ref ds,false);

			// create expected dataset to compare result to
			dsExpected = ds.Copy();

			//make some changes, the fill method will overides those changes with data from DB.
			foreach (DataRow dr in ds.Tables[0].Select())
				dr["Country"] = "NeverNeverLand";
			ds.Tables[0].Columns.Remove("HomePhone"); //remove column, this column will be addedd during the fill process
			//ds.Tables.Remove(ds.Tables[1]); //remove the table, this table will be addedd during the fill process
			ds.AcceptChanges();
	
			// create source dataset to be filled
			dsResult = ds.Copy();
		
			//execute fill
			ResultRowsCount = dbDA.Fill(dsResult);
		
			CompareResults_Fill(dsResult,dsExpected);

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}

		protected void DbDataAdapter_Fill_Dt(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult, dsExpected ;
			int ExpectedRowsCount,ResultRowsCount;
			DataSet ds = new DataSet();

			ExpectedRowsCount = ReadDBData_Fill(dbDA,ref ds, false);

			//ds.Tables.Remove(ds.Tables[1]); //remove the table, fill only one table

			// create expected dataset to compare result to
			dsExpected = ds.Copy();

			//make some changes, the fill method will overides those changes with data from DB.
			foreach (DataRow dr in ds.Tables[0].Select())
				dr["Country"] = "NeverNeverLand";
			ds.Tables[0].Columns.Remove("HomePhone"); //remove column, this column will be addedd during the fill process
			ds.AcceptChanges();
		
			// create source dataset to be filled
			dsResult = ds.Copy();
		
			//execute fill
			ResultRowsCount = dbDA.Fill(dsResult.Tables["Table"]);

			CompareResults_Fill(dsResult,dsExpected);

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();


		}

		protected void DbDataAdapter_Fill_Ds_Str(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult, dsExpected ;
			int ExpectedRowsCount,ResultRowsCount;
			DataSet ds = new DataSet();

			ExpectedRowsCount = ReadDBData_Fill(dbDA,ref ds, false);

			// create expected dataset to compare result to
			dsExpected = ds.Copy();

			//make some changes, the fill method will overides those changes with data from DB.
			foreach (DataRow dr in ds.Tables[0].Select())
				dr["Country"] = "NeverNeverLand";
			ds.Tables[0].Columns.Remove("HomePhone"); //remove column, this column will be addedd during the fill process
			//ds.Tables.Remove(ds.Tables[1]); //remove the table, this table will be addedd during the fill process
			ds.AcceptChanges();
		
			// create source dataset to be filled
			dsResult = ds.Copy();
		
			//execute fill
			ResultRowsCount = dbDA.Fill(dsResult,dsResult.Tables[0].TableName );

			CompareResults_Fill(dsResult,dsExpected);

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}

		protected void DbDataAdapter_Fill_Ds_Int_Int_Str(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult, dsExpected ;
			int ExpectedRowsCount,ResultRowsCount;
			DataSet ds = new DataSet();

			ExpectedRowsCount = ReadDBData_Fill(dbDA,ref ds, false);
			// create expected dataset to compare result to
			dsExpected = ds.Copy();

			//make some changes, the fill method will overides those changes with data from DB.
			foreach (DataRow dr in ds.Tables[0].Select())
				dr["Country"] = "NeverNeverLand";	
			ds.Tables[0].Columns.Remove("HomePhone"); //remove column, this column will be addedd during the fill process
			//ds.Tables.Remove(ds.Tables[1]); //remove the table, this table will be addedd during the fill process
			ds.AcceptChanges();
		
			// create source dataset to be filled
			dsResult = ds.Copy();
		
			//execute fill
			ResultRowsCount = dbDA.Fill(dsResult,0,0,dsResult.Tables[0].TableName);
			CompareResults_Fill(dsResult,dsExpected);
		
			dsResult = ds.Copy();
			//modify expected dataset to match the expected result
			for (int i=0; i < dsExpected.Tables[0].Rows.Count ; i++) {
				if (i < 5 || i > 14) {
					dsExpected.Tables[0].Rows[i]["Country"] = "NeverNeverLand";
					dsExpected.Tables[0].Rows[i]["HomePhone"] = DBNull.Value; 
				}
			}
			ResultRowsCount = dbDA.Fill(dsResult,5,10,dsResult.Tables[0].TableName);
			CompareResults_Fill(dsResult,dsExpected);

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}


		protected void DbDataAdapter_FillSchema_Ds_SchemaType(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult = new DataSet(); 
			DataSet dsExpected = new DataSet();
		
			// create expected dataset to compare result to
			ReadDBData_Fill(dbDA,ref dsExpected ,true);

			//  Note:   When handling batch SQL statements that return multiple results, 
			//	the implementation of FillSchema for the .NET Framework Data Provider for OLEDB 
			//	retrieves schema information for only the first result. 
			//	To retrieve schema information for multiple results, use Fill with the MissingSchemaAction set to AddWithKey
			//			if (dbDA.GetType() == typeof(Sys.Data.OleDb.OleDbDataAdapter)) 
			//				dsExpected.Tables.Remove(dsExpected.Tables[1]);

			//execute FillSchema

			//dsResult = dsExpected.Copy();
			DataTable[] dtArr = dbDA.FillSchema(dsResult,SchemaType.Mapped );

			//************  Fix .Net bug? (FillSchema method add AutoIncrement=true) *******************
			dsResult.Tables[0].Columns["EmployeeID"].AutoIncrement = false;
		
			CompareResults_Fill(dsResult,dsExpected);

			Exception exp = null;
			try {
				BeginCase("Check return value - Table[0]");
				Compare(dtArr[0],dsResult.Tables[0] );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//			if (dbDA.GetType() != typeof(Sys.Data.OleDb.OleDbDataAdapter)) 
			//				try
			//				{
			//					BeginCase("Check return value - Table[1]");
			//					Compare(dtArr[1],dsResult.Tables[1]);
			//				}
			//				catch(Exception ex)	{exp = ex;}
			//				finally	{EndCase(exp); exp = null;}
	
			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}

		protected void DbDataAdapter_FillSchema_Dt_SchemaType(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult = new DataSet(); 
			DataSet dsExpected = new DataSet();
		
			// create expected dataset to compare result to
			ReadDBData_Fill(dbDA,ref dsExpected ,true);

			//dsExpected.Tables.Remove(dsExpected.Tables[1]);

			//execute FillSchema

			dsResult.Tables.Add("Table");
			DataTable dt = dbDA.FillSchema(dsResult.Tables[0],SchemaType.Mapped );

			//************  Fix .Net bug? (FillSchema method add AutoIncrement=true) *******************
			dsResult.Tables[0].Columns["EmployeeID"].AutoIncrement = false;

			CompareResults_Fill(dsResult,dsExpected);
			Exception exp = null;
			try {
				BeginCase("Check return value - Table[0]");
				Compare(dt,dsResult.Tables[0] );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}

		protected void DbDataAdapter_FillSchema_Ds_SchemaType_Str(Sys.Data.Common.DbDataAdapter dbDA) {
			DataSet dsResult = new DataSet(); 
			DataSet dsExpected = new DataSet();
		
			// create expected dataset to compare result to
			ReadDBData_Fill(dbDA,ref dsExpected ,true);

			//dsExpected.Tables.Remove(dsExpected.Tables[1]);

			//execute FillSchema

			dsResult.Tables.Add("Table");
			DataTable[] dtArr = dbDA.FillSchema(dsResult,SchemaType.Mapped,dsResult.Tables[0].TableName);

			//************  Fix .Net bug? (FillSchema method add AutoIncrement=true) *******************
			dsResult.Tables[0].Columns["EmployeeID"].AutoIncrement = false;

			CompareResults_Fill(dsResult,dsExpected);
			Exception exp = null;
			try {
				BeginCase("Check return value - Table[0]");
				Compare(dtArr[0],dsResult.Tables[0] );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}


		private int ReadDBData_Fill(Sys.Data.Common.DbDataAdapter dbDA, ref DataSet ds, bool ReadSchemaOnly) {
			int ExpectedRowsCount = 0;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IDataReader Idr;
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			ICmd.CommandText = "SELECT EmployeeID, LastName, FirstName, Title, Address, City, Region, Country, Extension, HomePhone  FROM Employees "  ;
			IConn.Open();

			//get db type
			DBType = ConnectedDataProvider.GetDbType(((IDbDataAdapter)dbDA).SelectCommand.Connection.ConnectionString);

			// Execute data Reader - Get Expected results
			Idr = ICmd.ExecuteReader();

			// create temp dataset to insert results
			ExpectedRowsCount = DataReaderFill_Fill(ref ds,ref Idr,ReadSchemaOnly);
			Idr.Close();
			return ExpectedRowsCount;
		}
		private void CompareResults_Fill(DataSet dsResult,DataSet dsExpected ) {
			Exception exp = null;

			//			try
			//			{
			//				BeginCase("Compare Rows count");
			//				// ???????   Fill return count for first table only    ??????
			//				Compare(ExpectedRowsCount  ,ResultRowsCount );
			//			}
			//			catch(Exception ex)	{exp = ex;}
			//			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("Compare data");
				Compare(dsResult.GetXml() ,dsExpected.GetXml());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("Compare schema");
				Compare(dsResult.GetXmlSchema() ,dsExpected.GetXmlSchema());
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		private int DataReaderFill_Fill(ref DataSet dsExpected, ref IDataReader Idr,bool ReadSchemaOnly) {
			bool blnNextResults;
			int RowsAffected = 0;
			object[] objArr = null;
			DataTable SchemaTable = null;
			do {
				SchemaTable = Idr.GetSchemaTable();

				//add new table with the right amount of columns, the first table must be named "Table"
				if (dsExpected.Tables.Count == 0)
					dsExpected.Tables.Add(new DataTable("Table"));
				else
					dsExpected.Tables.Add();
				for (int i = 0 ; i < Idr.FieldCount; i++) {
					dsExpected.Tables[dsExpected.Tables.Count-1].Columns.Add(new DataColumn(Idr.GetName(i),Idr.GetFieldType(i)));
					if (ReadSchemaOnly) {	// add schema info
						dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].AllowDBNull = (bool)SchemaTable.Rows[i]["AllowDBNull"];
						dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].AutoIncrement = (bool)SchemaTable.Rows[i]["IsAutoIncrement"];
						dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].ReadOnly = (bool)SchemaTable.Rows[i]["IsReadOnly"];
						dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].Unique = (bool)SchemaTable.Rows[i]["IsUnique"];
						if (dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].DataType == typeof(string))
							dsExpected.Tables[dsExpected.Tables.Count-1].Columns[i].MaxLength = (int)SchemaTable.Rows[i]["ColumnSize"];
					}
				}

				if (!ReadSchemaOnly) {
					//array that holds the current rows values
					objArr = new object[Idr.FieldCount];

					//fill the new table
					while (Idr.Read()) {
						Idr.GetValues(objArr);
						//update existing row, if no row is found - add it as new row
						dsExpected.Tables[dsExpected.Tables.Count-1].LoadDataRow(objArr,false);
						RowsAffected++;
					}
				}

				//get next record set 
				blnNextResults = Idr.NextResult(); 
			} 
			while (blnNextResults);

			// add primary key, fill method will update existing rows instead of insert new ones
			dsExpected.Tables[0].PrimaryKey = new DataColumn[] {dsExpected.Tables[0].Columns["EmployeeID"]};
			//if (ReadSchemaOnly)	dsExpected.Tables[1].PrimaryKey = new DataColumn[] {dsExpected.Tables[1].Columns["CustomerID"]};
			dsExpected.AcceptChanges();

			return RowsAffected;
		
		}

		#endregion

		#region " DBDataAdapter - FillError "
		private bool blnReadDBData_Fill = false;

		protected void DbDataAdapter_FillError(Sys.Data.Common.DbDataAdapter dbDA) {
			Exception exp = null;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers ";
			IConn.Open();

			DataSet ds = new DataSet();
			ds.Tables.Add(new DataTable("Customers"));
			ds.Tables[0].Columns.Add("CustomerID",typeof(byte));
        
			//check FillError event
			dbDA.FillError += new FillErrorEventHandler(dbDA_FillError);
			blnReadDBData_Fill = false;
			try {
				BeginCase("FillError");
				try {
					dbDA.Fill(ds,"Customers");
				}
				catch (Exception ){};
				Compare(blnReadDBData_Fill ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dbDA.FillError -= new FillErrorEventHandler(dbDA_FillError);

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}
		private void dbDA_FillError(object sender, FillErrorEventArgs args) {
			blnReadDBData_Fill = true;
			args.Continue = false;
		}

		#endregion

		#region " DBDataAdapter - Update "
		protected void DbDataAdapter_Update_Ds(Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;

			// --------- get data from DB -----------------
			DataSet ds = PrepareDBData_Update(dbDA);

			// --------- prepare dataset for update method -----------------
			DataSet dsDB1 = ds.Copy(); 
		
			// --------- prepare dataset for DBConcurrencyException -----------------
			DataSet dsDB2 = ds.Copy(); 
		
			//update expecteed dataset
			dsDB2.Tables[0].Rows.Add(new object[] {9994,"Ofer", "Borshtein", "Delete"});
			dsDB2.Tables[0].Rows.Add(new object[] {9995,"Ofer", "Borshtein", "Update"});
			dsDB2.Tables[0].Rows.Find(9996).Delete();
			dsDB2.AcceptChanges();

			//make changes to the DataBase (through the dataset)
			dsDB1.Tables[0].Rows.Add(new object[] {9991,"Ofer","Borshtein","Insert"});
			dsDB1.Tables[0].Rows.Find(9992).Delete();
			dsDB1.Tables[0].Rows.Find(9993)["Title"] = "Jack the ripper"; 

			//execute update to db
			NumberOfAffectedRows = dbDA.Update(dsDB1);
		
			try {
				BeginCase("Number Of Affected Rows");
				Compare(NumberOfAffectedRows ,3 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//get result from db in order to check them
			DataSet dsExpected = new DataSet(); //ds.Reset();
			dbDA.Fill(dsExpected);
			dsExpected.Tables[0].PrimaryKey = new DataColumn[] {dsExpected.Tables[0].Columns["EmployeeID"]};

			CompareResults_Update(dsDB1,dsDB2,ref dbDA);
			CompareResults_Update_Ds_Exception(dsDB2,ref dbDA);

			//Create rows which not exists in the DB but exists in the DS with row state = deleted
			//this will cause the Update to fail.
			dsDB1.Tables[0].Rows.Add(new object[] {9997,"Ofer", "Borshtein", "Delete"});
			dsDB1.Tables[0].Rows.Add(new object[] {9998,"Ofer", "Borshtein", "Delete"});
			dsDB1.AcceptChanges();
			dsDB1.Tables[0].Rows.Find(9997).Delete();
			dsDB1.Tables[0].Rows.Find(9998).Delete();

			//Check Sys.Data.DBConcurrencyException
			//The exception that is thrown by the DataAdapter during the update operation if the number of rows affected equals zero.
			try {
				BeginCase("Check DBConcurrencyException");
				try {
					NumberOfAffectedRows = dbDA.Update(dsDB1);
				}
				catch (DBConcurrencyException ex) {exp=ex;}
				Compare(exp.GetType(),typeof(DBConcurrencyException) );
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();


		}

		private void CompareResults_Update_Ds_Exception(DataSet dsResultException,ref Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;
			Exception e = null;

		
			// --------- check for DBConcurrencyException /UniqueConstraint -----------------
			//	call AcceptChanges after each exception check in order to make sure that we check only the the current row 


			try {
				BeginCase("DBConcurrencyException - Delete");
				dsResultException.Tables[0].Rows.Find(9994).Delete();
				//no row with row version delete exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Delete Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

			try {
				BeginCase("DBConcurrencyException - Update");
				dsResultException.Tables[0].Rows.Find(9995)["Title"] = "Jack the ripper"; 
				//no row with row version Update exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Update Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


			try {
				BeginCase("DBConcurrencyException - Insert");
				dsResultException.Tables[0].Rows.Add(new object[] {9996,"Ofer","Borshtein","Insert"});
				//no row with row version Insert exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException);}
				catch (Exception dbExp){e = dbExp;} //throw Sys.Exception
				Compare(e != null ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Insert Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


		}
	

		protected void DbDataAdapter_Update_Dt(Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;

			// --------- get data from DB -----------------
			DataSet ds = PrepareDBData_Update(dbDA);			

			// --------- prepare dataset for update method -----------------
			DataSet dsDB1 = ds.Copy(); 
		
			// --------- prepare dataset for DBConcurrencyException -----------------
			DataSet dsDB2 = ds.Copy(); 

			//update dataset
			dsDB2.Tables[0].Rows.Add(new object[] {9994,"Ofer", "Borshtein", "Delete"});
			dsDB2.Tables[0].Rows.Add(new object[] {9995,"Ofer", "Borshtein", "Update"});
			dsDB2.Tables[0].Rows.Find(9996).Delete();
			dsDB2.AcceptChanges();

		
			dsDB1.Tables[0].Rows.Add(new object[] {9991,"Ofer","Borshtein","Insert"});
			dsDB1.Tables[0].Rows.Find(9992).Delete();
			dsDB1.Tables[0].Rows.Find(9993)["Title"] = "Jack the ripper"; 

			//execute update to db
			NumberOfAffectedRows = dbDA.Update(dsDB1.Tables[0]);
		
			try {
				BeginCase("Number Of Affected Rows");
				Compare(NumberOfAffectedRows ,3 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//get result from db in order to check them
			DataSet dsExpected = new DataSet(); //ds.Reset();
			dbDA.Fill(dsExpected);
			dsExpected.Tables[0].PrimaryKey = new DataColumn[] {dsExpected.Tables[0].Columns["EmployeeID"]};

			CompareResults_Update(dsDB1,dsDB2,ref dbDA);
			CompareResults_Update_Dt_Exception(dsDB2,ref dbDA);

			//Create rows which not exists in the DB but exists in the DS with row state = deleted
			//this will cause the Update to fail.
			dsDB1.Tables[0].Rows.Add(new object[] {9997,"Ofer", "Borshtein", "Delete"});
			dsDB1.Tables[0].Rows.Add(new object[] {9998,"Ofer", "Borshtein", "Delete"});
			dsDB1.AcceptChanges();
			dsDB1.Tables[0].Rows.Find(9997).Delete();
			dsDB1.Tables[0].Rows.Find(9998).Delete();

			//Check Sys.Data.DBConcurrencyException
			//The exception that is thrown by the DataAdapter during the update operation if the number of rows affected equals zero.
			try {
				BeginCase("Check DBConcurrencyException");
				try {
					NumberOfAffectedRows = dbDA.Update(dsDB1.Tables[0]);
				}
				catch (DBConcurrencyException ex) {exp=ex;}
				Compare(exp.GetType(),typeof(DBConcurrencyException) );
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();


		}

		private void CompareResults_Update_Dt_Exception(DataSet dsResultException,ref Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;
			Exception e = null;


			// --------- check for DBConcurrencyException /UniqueConstraint -----------------
			//	call AcceptChanges after each exception check in order to make sure that we check only the the current row 


			try {
				BeginCase("DBConcurrencyException - Delete");
				dsResultException.Tables[0].Rows.Find(9994).Delete();
				//no row with row version delete exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException.Tables[0]);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Delete Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

			try {
				BeginCase("DBConcurrencyException - Update");
				dsResultException.Tables[0].Rows.Find(9995)["Title"] = "Jack the ripper"; 
				//no row with row version Update exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException.Tables[0]);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Update Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


			try {
				BeginCase("DBConcurrencyException - Insert");
				dsResultException.Tables[0].Rows.Add(new object[] {9996,"Ofer","Borshtein","Insert"});
				//no row with row version Insert exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException.Tables[0]);}
				catch (Exception dbExp){e = dbExp;} //throw Sys.Exception
				Compare(e != null ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Insert Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

		}


		protected void DbDataAdapter_Update_Dr(Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;

			// --------- get data from DB -----------------
			DataSet ds = PrepareDBData_Update(dbDA);

			// --------- prepare dataset for update method -----------------
			DataSet dsDB1 = ds.Copy(); 
		
			// --------- prepare dataset for DBConcurrencyException -----------------
			DataSet dsDB2 = ds.Copy(); 

			//update dataset
			dsDB2.Tables[0].Rows.Add(new object[] {9994,"Ofer", "Borshtein", "Delete"});
			dsDB2.Tables[0].Rows.Add(new object[] {9995,"Ofer", "Borshtein", "Update"});
			dsDB2.Tables[0].Rows.Find(9996).Delete();
			dsDB2.AcceptChanges();

		
			dsDB1.Tables[0].Rows.Add(new object[] {9991,"Ofer","Borshtein","Insert"});
			dsDB1.Tables[0].Rows.Find(9992).Delete();
			dsDB1.Tables[0].Rows.Find(9993)["Title"] = "Jack the ripper"; 

			//execute update to db
		
			DataRow[] drArr = new DataRow[dsDB1.Tables[0].Rows.Count] ;
			dsDB1.Tables[0].Rows.CopyTo(drArr,0);
			NumberOfAffectedRows = dbDA.Update(drArr);
		
			try {
				BeginCase("Number Of Affected Rows");
				Compare(NumberOfAffectedRows ,3 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//get result from db in order to check them
			DataSet dsExpected = new DataSet(); //ds.Reset();
			dbDA.Fill(dsExpected);
			dsExpected.Tables[0].PrimaryKey = new DataColumn[] {dsExpected.Tables[0].Columns["EmployeeID"]};

			CompareResults_Update(dsDB1,dsDB2,ref dbDA);
			CompareResults_Update_Dr_Exception(dsDB2,ref dbDA);

			//Create rows which not exists in the DB but exists in the DS with row state = deleted
			//this will cause the Update to fail.
			dsDB1.Tables[0].Rows.Add(new object[] {9997,"Ofer", "Borshtein", "Delete"});
			dsDB1.Tables[0].Rows.Add(new object[] {9998,"Ofer", "Borshtein", "Delete"});
			dsDB1.AcceptChanges();
			dsDB1.Tables[0].Rows.Find(9997).Delete();
			dsDB1.Tables[0].Rows.Find(9998)[1] = "Updated!";
		
			drArr = new DataRow[dsDB1.Tables[0].Rows.Count];
			dsDB1.Tables[0].Rows.CopyTo(drArr,0);

			//Check Sys.Data.DBConcurrencyException
			//The exception that is thrown by the DataAdapter during the update operation if the number of rows affected equals zero.
			try {
				BeginCase("Check DBConcurrencyException");
				try {
					NumberOfAffectedRows = dbDA.Update(drArr);
				}
				catch (DBConcurrencyException ex) {exp=ex;}
				Compare(exp.GetType(),typeof(DBConcurrencyException) );
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();


		}

		private void CompareResults_Update_Dr_Exception(DataSet dsResultException,ref Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;
			Exception e = null;
		
			DataRow[] drArr = new DataRow[dsResultException.Tables[0].Rows.Count];
		
			// --------- check for DBConcurrencyException /UniqueConstraint -----------------
			//	call AcceptChanges after each exception check in order to make sure that we check only the the current row 

			try {
				BeginCase("DBConcurrencyException - Delete");
				dsResultException.Tables[0].Rows.Find(9994).Delete();
				dsResultException.Tables[0].Rows.CopyTo(drArr,0);
				//no row with row version delete exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(drArr);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Delete Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

			try {
				BeginCase("DBConcurrencyException - Update");
				dsResultException.Tables[0].Rows.Find(9995)["Title"] = "Jack the ripper"; 
				dsResultException.Tables[0].Rows.CopyTo(drArr,0);
				//no row with row version Update exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(drArr);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Update Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


			try {
				BeginCase("DBConcurrencyException - Insert");
				dsResultException.Tables[0].Rows.Add(new object[] {9996,"Ofer","Borshtein","Insert"});
				dsResultException.Tables[0].Rows.CopyTo(drArr,0);
				//no row with row version Insert exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(drArr);}
				catch (Exception dbExp){e = dbExp;} //throw Sys.Exception
				Compare(e != null ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Insert Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

		}


		protected void DbDataAdapter_Update_Ds_Str(Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;

			// --------- get data from DB -----------------
			DataSet ds = PrepareDBData_Update(dbDA);

			// --------- prepare dataset for update method -----------------
			DataSet dsDB1 = ds.Copy(); 
		
			// --------- prepare dataset for DBConcurrencyException -----------------
			DataSet dsDB2 = ds.Copy(); 
		
			//update dataset
			dsDB2.Tables[0].Rows.Add(new object[] {9994,"Ofer", "Borshtein", "Delete"});
			dsDB2.Tables[0].Rows.Add(new object[] {9995,"Ofer", "Borshtein", "Update"});
			dsDB2.Tables[0].Rows.Find(9996).Delete();
			dsDB2.AcceptChanges();

		
			dsDB1.Tables[0].Rows.Add(new object[] {9991,"Ofer","Borshtein","Insert"});
			dsDB1.Tables[0].Rows.Find(9992).Delete();
			dsDB1.Tables[0].Rows.Find(9993)["Title"] = "Jack the ripper"; 

			//execute update to db
			NumberOfAffectedRows = dbDA.Update(dsDB1,dsDB1.Tables[0].TableName);
		
			try {
				BeginCase("Number Of Affected Rows");
				Compare(NumberOfAffectedRows ,3 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//get result from db in order to check them
			DataSet dsExpected = new DataSet(); //ds.Reset();
			dbDA.Fill(dsExpected);
			dsExpected.Tables[0].PrimaryKey = new DataColumn[] {dsExpected.Tables[0].Columns["EmployeeID"]};

			CompareResults_Update(dsDB1,dsDB2,ref dbDA);
			CompareResults_Update_Ds_Str_Exception(dsDB2,ref dbDA);

			//Create rows which not exists in the DB but exists in the DS with row state = deleted
			//this will cause the Update to fail.
			dsDB1.Tables[0].Rows.Add(new object[] {9997,"Ofer", "Borshtein", "Delete"});
			dsDB1.Tables[0].Rows.Add(new object[] {9998,"Ofer", "Borshtein", "Delete"});
			dsDB1.AcceptChanges();
			dsDB1.Tables[0].Rows.Find(9997).Delete();
			dsDB1.Tables[0].Rows.Find(9998).Delete();


			//Check Sys.Data.DBConcurrencyException
			//The exception that is thrown by the DataAdapter during the update operation if the number of rows affected equals zero.
			try {
				BeginCase("Check DBConcurrencyException");
				try {
					NumberOfAffectedRows = dbDA.Update(dsDB1,dsDB1.Tables[0].TableName);
				}
				catch (DBConcurrencyException ex) {exp=ex;}
				Compare(exp.GetType(),typeof(DBConcurrencyException) );
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();


		}

		private void CompareResults_Update_Ds_Str_Exception(DataSet dsResultException,ref Sys.Data.Common.DbDataAdapter dbDA) {
			int NumberOfAffectedRows = 0;
			Exception exp = null;
			Exception e = null;

		
			// --------- check for DBConcurrencyException /UniqueConstraint -----------------
			//	call AcceptChanges after each exception check in order to make sure that we check only the the current row 


			try {
				BeginCase("DBConcurrencyException - Delete");
				dsResultException.Tables[0].Rows.Find(9994).Delete();
				//no row with row version delete exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException,dsResultException.Tables[0].TableName);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Delete Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();

			try {
				BeginCase("DBConcurrencyException - Update");
				dsResultException.Tables[0].Rows.Find(9995)["Title"] = "Jack the ripper"; 
				//no row with row version Update exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException,dsResultException.Tables[0].TableName);}
				catch (DBConcurrencyException dbExp){e = dbExp;}
				Compare(e.GetType(),typeof(DBConcurrencyException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Update Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


			try {
				BeginCase("DBConcurrencyException - Insert");
				dsResultException.Tables[0].Rows.Add(new object[] {9996,"Ofer","Borshtein","Insert"});
				//no row with row version Insert exists - records affected = 0
				NumberOfAffectedRows = -1;
				try {NumberOfAffectedRows = dbDA.Update(dsResultException,dsResultException.Tables[0].TableName);}
				catch (Exception dbExp){e = dbExp;} //throw Sys.Exception
				Compare(e != null ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null; e = null;}
			try {
				BeginCase("Number Of Affected Rows - Insert Exception");
				Compare(NumberOfAffectedRows ,-1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			dsResultException.AcceptChanges();


		}

		/*
			* 
			* insert/update the database with data that will be used in testings
			* 
			*/
		protected void PrepareDataForTesting(string ConnectionString) {
			int iExists = 0;
			try {

				//create database specific date value
				string strBirthDateValue = "";
				switch (ConnectedDataProvider.GetDbType(ConnectionString)) {
					case DataBaseServer.DB2:
						strBirthDateValue = "'1988-05-31-15.33.44'";
						break;
					case DataBaseServer.Oracle:
						strBirthDateValue = "to_date('1988-05-31 15:33:44', 'yyyy-mm-dd HH24:mi:ss')";
						break;
					case DataBaseServer.PostgreSQL:
						strBirthDateValue = "to_timestamp('1988-05-31 15:33:44', 'yyyy-mm-dd HH24:mi:ss')";
						break;
					case DataBaseServer.SQLServer:
					case DataBaseServer.Sybase:
						strBirthDateValue = "'1988-May-31 15:33:44'";
						break;
				}
				OleDbConnection con = new OleDbConnection(ConnectionString);
				try {
					con.Open();
				}
				catch (Exception ex) {
					throw new Exception("PrepareDataForTesting failed trying to connect to DB using Connection string:" + ConnectionString + "\nException:" +  ex.ToString(),ex);
				}

				OleDbCommand cmd = new OleDbCommand("",con);

				//update / insert to table Employees
				for (int i = 100; i <= 700; i +=100) {
					cmd.CommandText = "select 1 from Employees where EmployeeID = " + i.ToString();
					iExists =  Sys.Convert.ToInt32(cmd.ExecuteScalar());
					if (iExists != 1) {
						cmd.CommandText = "insert into Employees (EmployeeID, LastName, FirstName, Title, BirthDate) " + 
							" Values (" + i.ToString() + 
							",'Last" + i.ToString() 
							+ "','First" + i.ToString() 
							+ "', null, " 
							+ strBirthDateValue + ")";
						cmd.ExecuteNonQuery();
					}
					else {
						cmd.CommandText = "update Employees set " 
							+ " LastName = 'Last" + i.ToString() 
							+ "', FirstName = 'First" + i.ToString() 
							+ "', Title = null, BirthDate = " 
							+ strBirthDateValue 
							+ " where EmployeeID = " + i.ToString() ;
						Log(cmd.CommandText);
						cmd.ExecuteNonQuery();
					}
				}

				//update / insert to table Customers
				for (int i = 100; i <= 700; i +=100) {
					cmd.CommandText = "select 1 from Customers where CustomerID = 'GH" + i.ToString() + "'";
					iExists =  Sys.Convert.ToInt32(cmd.ExecuteScalar());
					if (iExists != 1) {
						cmd.CommandText = "insert into Customers (CustomerID , CompanyName, Phone) Values ('GH" + i.ToString() + "','Company" + i.ToString() + "','00-" + i.ToString() + i.ToString() + "')";
						cmd.ExecuteNonQuery();
					}
					else {
						cmd.CommandText = "update Customers set CompanyName = 'Company" + i.ToString() + "', Phone = '00-" + i.ToString() + i.ToString() + "' where CustomerID = 'GH" + i.ToString() + "'";
						cmd.ExecuteNonQuery();
					}
				}

				cmd.CommandText = "select 1 from Customers where CustomerID = 'GH200'";
				iExists =  Sys.Convert.ToInt32(cmd.ExecuteScalar());
				if (iExists != 1) {
					cmd.CommandText = "insert into Customers (CustomerID , CompanyName) Values ('GH200','Company200')";
					cmd.ExecuteNonQuery();
				}
				else {
					cmd.CommandText = "update Customers set CompanyName = 'Company200' where CustomerID = 'GH200'";
					cmd.ExecuteNonQuery();
				}

				con.Close();
			}
			catch (Exception ex) {
				throw new Exception("PrepareDataForTesting failed with exception:" + ex.ToString(),ex);
			}
		}

		/*
		*	used to insert data to the database in order to check DataAdapter Update metods 
		*/
		protected DataSet PrepareDBData_Update(Sys.Data.Common.DbDataAdapter dbDA) {
			return PrepareDBData_Update(dbDA,false); 

		}
		protected DataSet PrepareDBData_Update(Sys.Data.Common.DbDataAdapter dbDA,bool sqlConnectionString) {
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand;
			ICmd.CommandText = "SELECT EmployeeID, LastName, FirstName, Title FROM Employees WHERE EmployeeID in (9991,9992,9993,9994,9995,9996)";  
			IDbConnection IConn = ICmd.Connection; 
			if (!sqlConnectionString) {
				IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			}
			IConn.Open();
					
			//Insert rows to be updated (insert,delete,update)
			IDbCommand cmd;
			if (sqlConnectionString) {
				cmd = new Sys.Data.SqlClient.SqlCommand();
				cmd.Connection = (Sys.Data.SqlClient.SqlConnection)IConn; 
			}
			else {
				cmd = new Sys.Data.OleDb.OleDbCommand();
				cmd.Connection = (Sys.Data.OleDb.OleDbConnection)IConn;
			}


			//run execute after each command because DB2 doesn't support multiple commands
			cmd.CommandText =  "DELETE FROM Employees WHERE EmployeeID in (9991,9992,9993,9994,9995,9996,9997,9998)";
			cmd.ExecuteNonQuery();

			//only for SQL Server
			DataBaseServer DBType =  ConnectedDataProvider.GetDbType(IConn.ConnectionString);

			cmd.CommandText = "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9992, 'Ofer', 'Borshtein', 'delete')";
			//if (DBType == DataBaseServer.SQLServer) cmd.CommandText = "SET IDENTITY_INSERT Employees ON;" + cmd.CommandText;
			cmd.ExecuteNonQuery();
			cmd.CommandText = "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9993, 'Ofer', 'Borshtein', 'Update')";
			//if (DBType == DataBaseServer.SQLServer) cmd.CommandText = "SET IDENTITY_INSERT Employees ON;" + cmd.CommandText;
			cmd.ExecuteNonQuery();
			cmd.CommandText = "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9996, 'Ofer', 'Borshtein', 'Exp')";
			//if (DBType == DataBaseServer.SQLServer) cmd.CommandText = "SET IDENTITY_INSERT Employees ON;" + cmd.CommandText;
			cmd.ExecuteNonQuery();

			//cmd.CommandText += "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9991, 'Ofer', 'Borshtein', 'Insert'); ";
			//cmd.CommandText += "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9994, 'Ofer', 'Borshtein', 'Exp'); ";
			//cmd.CommandText += "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9995, 'Ofer', 'Borshtein', 'Exp'); ";
			//cmd.CommandText += "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9997, 'Ofer', 'Borshtein', 'delete'); ";
			//cmd.CommandText += "INSERT INTO Employees (EmployeeID, LastName, FirstName, Title) VALUES(9998, 'Ofer', 'Borshtein', 'delete'); ";

			DataSet ds = new DataSet();
			dbDA.Fill(ds);
			ds.Tables[0].PrimaryKey = new DataColumn[] {ds.Tables[0].Columns["EmployeeID"]};
			return ds;

		}


		private void CompareResults_Update(DataSet dsResult,DataSet dsResultException,ref Sys.Data.Common.DbDataAdapter dbDA) {
			Exception exp = null;
		
			//----------------- Compare dsDB with dsExcepted -----------------
			try {
				BeginCase("Insert Row ");
				Compare(dsResult.Tables[0].Rows.Find(9991) == null ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("Update Row ");
				Compare(dsResult.Tables[0].Rows.Find(9993)["Title"],"Jack the ripper");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			try {
				BeginCase("Delete Row ");
				Compare(dsResult.Tables[0].Rows.Find(9992) ,null);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}			

		}

		#endregion

		protected void DBDataAdapter_DefaultSourceTableName() {
			Exception exp = null;
			try {
				BeginCase("DefaultSourceTableName");
				Compare(Sys.Data.Common.DbDataAdapter.DefaultSourceTableName , "Table");
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

		#endregion

		#region "-----------  Sys.Data.Common.DataAdapter --------------"
		protected void DataAdapter_AcceptChangesDuringFill(Sys.Data.Common.DbDataAdapter dbDA) {
			Exception exp = null;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			IConn.Open();

			PrepareDataForTesting( MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			//get the total rows count
			ICmd.CommandText = "SELECT Count(*) FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";
			int ExpectedRows = Sys.Convert.ToInt32(ICmd.ExecuteScalar());
			try {
				BeginCase("Check that Expected rows count > 0");
				Compare(ExpectedRows > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";

			DataSet ds = new DataSet();
			dbDA.AcceptChangesDuringFill = false;

        
			try {
				BeginCase("Execute Fill - check return rows count");
				int i = dbDA.Fill(ds);
				Compare(i ,ExpectedRows );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
				

			bool blnAcceptChanges = false;

			foreach (DataRow dr in ds.Tables[0].Rows) {
				if (dr.RowState != DataRowState.Added ) {
					blnAcceptChanges = true;
					break;
				}
			}
			try {
				BeginCase("AcceptChangesDuringFill - false");
				Compare(blnAcceptChanges ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			ds.Reset();
			dbDA.AcceptChangesDuringFill = true;
			dbDA.Fill(ds);
			blnAcceptChanges = false;            			
			foreach (DataRow dr in ds.Tables[0].Rows) {
				if (dr.RowState != DataRowState.Unchanged ) {
					blnAcceptChanges = true;
					break;
				}
			}
			try {
				BeginCase("AcceptChangesDuringFill - true");
				Compare(blnAcceptChanges ,false );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();

		}

		protected void DataAdapter_ContinueUpdateOnError(Sys.Data.Common.DbDataAdapter dbDA) {
			/*
				!!!!!! Not working (TestName "ContinueUpdateOnError - true, check value 2")!!!!!
				If ContinueUpdateOnError is set to true, no exception is thrown when an error occurs during the update of a row. 
				The update of the row is skipped and the error information is placed in the RowError property of the row in error. 
				The DataAdapter continues to update subsequent rows.
				If ContinueUpdateOnError is set to false, an exception is thrown when an error occurs during the update of a row.
			*/
			Exception exp = null;

	
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			IConn.Open();

			PrepareDataForTesting( MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			//get the total rows count
			ICmd.CommandText = "SELECT Count(*) FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";
			int ExpectedRows = Sys.Convert.ToInt32(ICmd.ExecuteScalar());
			try {
				BeginCase("Check that Expected rows count > 0");
				Compare(ExpectedRows > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";


			DataSet dsMem = new DataSet();	//Disconected dataset
			DataSet dsDB = new DataSet();	//DataBase data
			dbDA.AcceptChangesDuringFill = true;
			//get data from DB
			try {
				BeginCase("Execute Fill - check return rows count");
				int i = dbDA.Fill(dsMem);
				Compare(i ,ExpectedRows );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		

			//update data with invalid information (Max. length for Phone is 24)
			//					123456789012345678901234
			string newValue1 = "Very Long String That Will Raise An Error Yep!";
			string oldValue1 = dsMem.Tables[0].Rows[3]["Phone"].ToString();
			string oldValue2 = dsMem.Tables[0].Rows[4]["Phone"].ToString();
			string newValue2 = "03-1234";


			dsMem.Tables[0].Rows[3]["Phone"] = newValue1;
			dsMem.Tables[0].Rows[4]["Phone"] = newValue2;
			dbDA.ContinueUpdateOnError = true;
        
			//will not throw exception
			try {
				BeginCase("ContinueUpdateOnError - true, check exception");
				try {
					dbDA.Update(dsMem);
				}
				catch(Exception ex){exp = ex;}
				Compare(exp == null,true);
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			dbDA.Fill(dsDB); //get data from DB to check the update operation

			try {
				BeginCase("ContinueUpdateOnError - true, check RowError");
				Compare(dsMem.Tables[0].Rows[3].RowError.Length > 0 , true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("ContinueUpdateOnError - true, check value 1");
				Compare(dsDB.Tables[0].Rows[3]["Phone"] , oldValue1);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


			/*		- Test excluded, it is not working in .NET too!
					//should continue the update
					try
					{
						BeginCase("ContinueUpdateOnError - true, check value 2");
						Compare(dsDB.Tables[0].Rows[4]["Phone"] , newValue2);  //--------- NOT WORKING !!! -----------
					}
					catch(Exception ex)	{exp = ex;}
					finally	{EndCase(exp); exp = null;}
			*/
			dsMem.Reset();
			dsDB.Reset();
			dbDA.Fill(dsMem);
			dsMem.Tables[0].Rows[3]["Phone"] = newValue1 ;
			dsMem.Tables[0].Rows[4]["Phone"] = newValue2;
			dbDA.ContinueUpdateOnError = false;
        
			try {
				BeginCase("ContinueUpdateOnError - false, check exception");
				try {
					dbDA.Update(dsMem);
				}
				catch(Exception ex){exp = ex;}
				Compare(exp == null,false);
				exp = null;
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			dbDA.Fill(dsDB); //get data from DB to check the update operation
			try {
				BeginCase("ContinueUpdateOnError - false,check RowError");
				Compare(dsMem.Tables[0].Rows[3].RowError.Length > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			try {
				BeginCase("ContinueUpdateOnError - false,check value 1");
				Compare(dsDB.Tables[0].Rows[3]["Phone"] , oldValue1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


			try {
				BeginCase("ContinueUpdateOnError - false,check value 2");
				Compare(dsDB.Tables[0].Rows[4]["Phone"] , oldValue2 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();

		}
		protected void DataAdapter_MissingMappingAction(Sys.Data.Common.DbDataAdapter dbDA) {
			Exception exp = null;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;

			IConn.Open();

			//get the total rows count

			PrepareDataForTesting( MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			ICmd.CommandText = "SELECT Count(*) FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";
			int ExpectedRows = Sys.Convert.ToInt32(ICmd.ExecuteScalar());
			try {
				BeginCase("Check that Expected rows count > 0");
				Compare(ExpectedRows > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";


			//init dataset
			DataSet ds = new DataSet();
			try {
				BeginCase("Execute Fill - check return rows count");
				int i = dbDA.Fill(ds);
				Compare(i ,ExpectedRows );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			//make dataset schema mismatch with DB
			ds.Tables[0].Columns.Remove("Country");
			ds.Tables[0].Clear();
        
			//--- Default value ---
        
			try {
				BeginCase("MissingMappingAction Default value");
				Compare(dbDA.MissingMappingAction , MissingMappingAction.Passthrough);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	
			//--- MissingMappingAction.Error ---
			ds.Tables[0].Clear();
			dbDA.MissingMappingAction  = MissingMappingAction.Error ;
			Exception ExMissingMappingAction = null;
			try {
				BeginCase("MissingMappingAction.Error");
				try {
					dbDA.Fill(ds);
				}
				catch (InvalidOperationException e) {
					ExMissingMappingAction = e;
				}
				Compare(ExMissingMappingAction.GetType() ,typeof(InvalidOperationException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Error, Row.Count = 0");
				Compare(ds.Tables[0].Rows.Count , 0 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Error, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country")  , -1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//--- MissingMappingAction.Ignore ---
			ds.Tables[0].Clear();
			dbDA.MissingMappingAction  = MissingMappingAction.Ignore  ;
			ExMissingMappingAction = null;
			try {
				BeginCase("MissingMappingAction.Ignore");
				try {
					dbDA.Fill(ds);
				}
				catch (InvalidOperationException e) {
					ExMissingMappingAction = e;
				}
				Compare(ExMissingMappingAction ,null);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Ignore, Row.Count = 0");
				Compare(ds.Tables[0].Rows.Count ,0);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Ignore, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country")  , -1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//--- MissingMappingAction.Passthrough ---
			ds.Tables[0].Clear();
			dbDA.MissingMappingAction  = MissingMappingAction.Passthrough   ;
			ExMissingMappingAction = null;
			try {
				BeginCase("MissingMappingAction.Passthrough");
				try {
					dbDA.Fill(ds);
				}
				catch (InvalidOperationException e) {
					ExMissingMappingAction = e;
				}
				Compare(ExMissingMappingAction ,null);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Passthrough, Row.Count > 0");
				Compare(ds.Tables[0].Rows.Count >= 0 ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingMappingAction.Passthrough, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country") >= 0  ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}
		protected void DataAdapter_MissingSchemaAction(Sys.Data.Common.DbDataAdapter dbDA) {
			Exception exp = null;
			IDbDataAdapter Ida = (IDbDataAdapter)dbDA;
			IDbCommand ICmd = Ida.SelectCommand; 
			IDbConnection IConn = ICmd.Connection; 
			IConn.ConnectionString = MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString;
			IConn.Open();

			PrepareDataForTesting( MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

			//get the total rows count
			ICmd.CommandText = "SELECT Count(*) FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";
			int ExpectedRows = Sys.Convert.ToInt32(ICmd.ExecuteScalar());
			try {
				BeginCase("Check that Expected rows count > 0");
				Compare(ExpectedRows > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			ICmd.CommandText = "SELECT CustomerID, CompanyName, City, Country, Phone FROM Customers where CustomerID in ('GH100','GH200','GH300','GH400','GH500','GH600','GH700')";

			//get db type
			DBType = ConnectedDataProvider.GetDbType(((IDbDataAdapter)dbDA).SelectCommand.Connection.ConnectionString);

			//init dataset
			DataSet ds = new DataSet();
			try {
				BeginCase("Execute Fill - check return rows count");
				int i = dbDA.Fill(ds);
				Compare(i ,ExpectedRows );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//make dataset schema mismatch with DB
			ds.Tables[0].Columns.Remove("Country");
			ds.Tables[0].Clear();
        

			//--- Default value ---
        
			try {
				BeginCase("MissingSchemaAction Default value");
				Compare(dbDA.MissingSchemaAction, MissingSchemaAction.Add);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	
			//--- MissingSchemaAction.Error ---
			ds.Tables[0].Clear();
			dbDA.MissingSchemaAction  = MissingSchemaAction.Error  ;
			Exception ExMissingSchemaAction = null;
			try {
				BeginCase("MissingSchemaAction.Error");
				try {
					dbDA.Fill(ds);
				}
				catch (InvalidOperationException e) {
					ExMissingSchemaAction = e;
				}
				Compare(ExMissingSchemaAction.GetType() ,typeof(InvalidOperationException));
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Error, Row.Count = 0");
				Compare(ds.Tables[0].Rows.Count , 0 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Error, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country")  , -1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
	

			//--- MissingSchemaAction.Ignore ---
			try {
				//catch any exception that might occure 
				BeginCase("MissingSchemaAction.Ignore - invoke");
				ds.Tables[0].Clear();
				dbDA.MissingSchemaAction  = MissingSchemaAction.Ignore  ;
				ExMissingSchemaAction = null;
				dbDA.Fill(ds);
				Compare(true ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Ignore, Row.Count = 0");
				Compare(ds.Tables[0].Rows.Count > 0 ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Ignore, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country")  , -1 );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

#if !KNOWN_BUG //BUG_NUM:1951
			try {
				BeginCase("MissingSchemaAction.Ignore, PrimaryKey");
				Compare(ds.Tables[0].PrimaryKey.Length == 0 ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
#endif

			//--- MissingSchemaAction.Add ---
			try {
				//catch any exception that might occure 
				BeginCase("MissingSchemaAction.Add - invoke");
				ds.Tables[0].Clear();
				dbDA.MissingSchemaAction  = MissingSchemaAction.Add   ;
				ExMissingSchemaAction = null;
				dbDA.Fill(ds);
				Compare(true ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Add, Row.Count > 0");
				Compare(ds.Tables[0].Rows.Count >= 0 ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.Add, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country") >= 0  ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

#if !KNOWN_BUG //BUG_NUM:1952
			//DB2 don't return primary key
			if (DBType != DataBaseServer.DB2) {
				try {
					BeginCase("MissingSchemaAction.AddWithKey, PrimaryKey");
					Compare(ds.Tables[0].PrimaryKey.Length  ,0);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}

			//--- MissingSchemaAction.AddWithKey ---
			try {
				//catch any exception that might occure 
				BeginCase("MissingSchemaAction.AddWithKey - invoke");
				ds.Tables[0].Clear();
				ds.Tables[0].Columns.Remove("Country");
				dbDA.MissingSchemaAction  = MissingSchemaAction.AddWithKey    ;
				ExMissingSchemaAction = null;
				dbDA.Fill(ds);
				Compare(true ,true);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.AddWithKey, Row.Count > 0");
				Compare(ds.Tables[0].Rows.Count >= 0 ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try {
				BeginCase("MissingSchemaAction.AddWithKey, Column");
				Compare(ds.Tables[0].Columns.IndexOf("Country") >= 0  ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			//DB2 don't return primary key
			if (DBType != DataBaseServer.DB2 &&
				DBType != DataBaseServer.Oracle) {
				try {
					BeginCase("MissingSchemaAction.AddWithKey, PrimaryKey");
					Compare(ds.Tables[0].PrimaryKey.Length > 0 ,true );
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
			}
#endif

			//close connection
			if (  ((IDbDataAdapter)dbDA).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)dbDA).SelectCommand.Connection.Close();
		}
		#endregion

		public override void BeginTest(string testName) {
			base.BeginTest (testName);
			Log(String.Format("DataBase Type is {0}", ConnectedDataProvider.GetDbType()));
		}

	}
}