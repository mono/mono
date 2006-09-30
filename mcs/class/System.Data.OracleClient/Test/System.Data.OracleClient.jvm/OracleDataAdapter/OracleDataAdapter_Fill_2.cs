// 
// Copyright (c) 2006 Mainsoft Co.
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
using System.Data.OracleClient;
using NUnit.Framework;
using MonoTests.System.Data.Utils;


namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataAdapter_Fill_2 : ADONetTesterClass 
	{
		private string nonUniqueId;
		public static void Main()
		{
			OracleDataAdapter_Fill_2 tc = new OracleDataAdapter_Fill_2();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataAdapter_Fill_2");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			
			
			OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			con.Open();

			//DoTestThis(con);
			DoTestTypes1(con);
			//Don't know how to access diffrent database
			if ((ConnectedDataProvider.GetDbType(con) != DataBaseServer.DB2) && (ConnectedDataProvider.GetDbType(con) != DataBaseServer.PostgreSQL))
			{ 
				DoTestTypes2(con);
				DoTestTypes3(con);
			}
			
#if TARGET_JVM
			DoTestTypes4(con);
#endif
		//	DoTestTypes5(con);   //Table direct --> multipe tables
			DoTestTypes6(con);
			
			if ((ConnectedDataProvider.GetDbType(con) != DataBaseServer.Oracle) && 
				(ConnectedDataProvider.GetDbType(con) != DataBaseServer.PostgreSQL))
			{
				DoTestTypes7(con); //Diffrent owner
				
			}

			DoTestTypes8(con); //Diffrent owner

			//TBD!!
			//DoTestTypes9(con);
			
			if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.PostgreSQL)
			{
				DoTestTypes10(con);
			}

			CallStoredProcedureInPackage(con);
			StoredProcedurePackageambiguity_InsidePackage(con);
			StoredProcedurePackageambiguity_OutsidePackage(con);

			if (con.State == ConnectionState.Open) con.Close();
		}

		//[Test]
		public void DoTestThis(OracleConnection con)
		{
			Exception exp = null;
			OracleCommand cmd = new OracleCommand("GH_CREATETABLE", con);
				cmd.CommandType = CommandType.StoredProcedure;
				OracleDataAdapter da = new OracleDataAdapter(cmd);
				DataSet ds = new DataSet();

				try
				{
					BeginCase("Check effected rows after create table ddl in stored procedure.");
					int RowsAffected;
					RowsAffected = cmd.ExecuteNonQuery();
					int ExpectedRowsAffected;
					switch (ConnectedDataProvider.GetDbType(con))
					{
						case DataBaseServer.SQLServer:
						case DataBaseServer.Sybase:
							ExpectedRowsAffected = 3;
							break;
						case DataBaseServer.Oracle:
							//In .NET the ExpectedRowsAffected is '1', where as in Java it is '-1', this gap is because of jdbc driver for oracle.
							ExpectedRowsAffected = -1;

							break;
						case DataBaseServer.DB2:
							ExpectedRowsAffected = -1;
							break;
						default:
							string errMsg = string.Format("GHT: Test not implemented for DB type: {0}", ConnectedDataProvider.GetDbType(con));
							throw new NotImplementedException(errMsg);
					}
					Compare(RowsAffected ,ExpectedRowsAffected);
				}
				catch(Exception ex)	{exp = ex;}
				finally	{EndCase(exp); exp = null;}
		}


		#region Select by full table name in the same catalog
		//[Test]
		public void DoTestTypes1(OracleConnection conn)
		{
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;

			string tableName = getDbObjectName("Employees",conn);
			int expectedRowsCount = 8;

			#region Select by full table name in the same catalog

			string[] arr = new string[2];

			arr[0] = "LastName";
			arr[1] = "FirstName";

			prepareTableForTest(conn,expectedRowsCount,"Employees","EmployeeID",arr);
			comm.CommandText="select max(EmployeeID) from " + tableName;
			// on some databases the max is on a field which is decimal
			decimal maxEmployee = decimal.Parse(comm.ExecuteScalar().ToString()) - expectedRowsCount;

			comm.CommandText = "SELECT EmployeeID FROM " + tableName + " where EmployeeID > " +  maxEmployee.ToString() ;
			da.Fill(ds);

			Exception exp = null;
			try
			{
				BeginCase("Select by full table name in the same catalog");
				Compare(ds.Tables[0].Rows.Count ,expectedRowsCount );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{cleanTableAfterTest(conn,"Employees","EmployeeID",Convert.ToInt32(maxEmployee));
				EndCase(exp); exp = null;}

			#endregion //Select by full table name in the same catalog

		}

		#endregion

		#region Select by full table name in the different catalog
		//[Test]
		public void DoTestTypes2(OracleConnection conn)
		{
			BeginCase("Select by full table name in the different catalog");
			nonUniqueId = "48951_" +  TestCaseNumber.ToString(); 
			Exception exp=null;
			string tableName = getDbObjectName("Customers",conn,"GHTDB_EX");
			int expectedRowsCount = 5;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;

			insertIntoStandatTable(conn,tableName,expectedRowsCount,"CustomerID");
		
			comm.CommandText = "SELECT * FROM " + tableName + " where CustomerID='" + nonUniqueId + "'" ;
			ds.Tables.Clear();
			da.Fill(ds);

			try
			{
				
				Compare(ds.Tables[0].Rows.Count ,expectedRowsCount );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;
			cleanStandatTable(conn,tableName,"CustomerID");
			}
		}
		#endregion

		#region Call stored procedure in the different catalog
		//[Test]
		public void DoTestTypes3(OracleConnection conn)
		{
			BeginCase("Call stored procedure in the different catalog");
			nonUniqueId = "48951_" +  TestCaseNumber.ToString(); 
			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;

			string tableName = getDbObjectName("Customers",conn,"GHTDB_EX");
			int expectedRowsCount = 5;

			insertIntoStandatTable(conn,tableName,expectedRowsCount,"CustomerID");

			comm.CommandType = CommandType.StoredProcedure;
			comm.CommandText = getDbObjectName("GH_DUMMY",conn,"GHTDB_EX");

			comm.Parameters.Add(new OracleParameter("CustomerIDPrm",OracleType.Char));
			comm.Parameters.Add(new OracleParameter("result",OracleType.Cursor)).Direction = ParameterDirection.Output;

			
			comm.Parameters[0].Value = nonUniqueId;
			ds.Tables.Clear();
			

			try
			{
				da.Fill(ds);
				Compare(ds.Tables[0].Rows.Count ,expectedRowsCount );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;
			cleanStandatTable(conn,tableName,"CustomerID"); }
		}

#endregion // Call stored procedure in the different catalog

		#region Select using Table direct - single table
		//[Test]
		public void DoTestTypes4(OracleConnection conn)
		{
			
			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			string tableName = getDbObjectName("Customers",conn);
			//int expectedRowsCount = 5;

			comm.CommandText = tableName;

			comm.CommandType = CommandType.TableDirect;

			ds.Tables.Clear();
			da.Fill(ds);

			try
			{
				BeginCase("Select using Table direct - single table");
				Compare(ds.Tables[0].Rows.Count > 0 ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

#endregion // Select using Table direct - single table

		#region Select using Table direct - multiple tables

		//[Test]
		public void DoTestTypes5(OracleConnection conn)
		{

			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			//string tableName = getDbObjectName("Customers",conn);
			comm.CommandType = CommandType.TableDirect;
			comm.CommandText = "Categories;Employees";

			ds.Tables.Clear();
			da.Fill(ds);

			try
			{
				BeginCase("Select using Table direct - multiple tables");
				int result =  + ds.Tables[1].Rows.Count + ds.Tables[2].Rows.Count;
				Compare(ds.Tables[0].Rows.Count > 0  ,true );
				Compare(ds.Tables[1].Rows.Count > 0  ,true );
				Compare(ds.Tables[0].Rows.Count == ds.Tables[1].Rows.Count ,true );
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		}

#endregion // Select using Table direct - multiple tables


		#region Test view

		//[Test]
		public void DoTestTypes6(OracleConnection conn)
		{
			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			//string tableName = getDbObjectName("Customers",conn);
			comm.CommandType = CommandType.Text;

			switch (ConnectedDataProvider.GetDbType(conn))
			{
				case DataBaseServer.SQLServer:
				case DataBaseServer.Sybase:
					comm.CommandText = "select * from [Current Product List]";
					break;
				case DataBaseServer.Oracle:
				case DataBaseServer.PostgreSQL:
					comm.CommandText = "select * from Current_Product_List";
					break;
				default:
					comm.CommandText = "select * from DB2ADMIN.Current_Product_List";
					break;
			}


			ds.Tables.Clear();
			da.Fill(ds);

			try
			{
				BeginCase("Testing view");
				Compare(ds.Tables[0].Rows.Count >0,true);
				Compare(ds.Tables[0].Columns.Count,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}


		}
		#endregion

		#region select table with diffrent owner - diffrent name

		//[Test]
		public void DoTestTypes7(OracleConnection conn)
		{
			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			//string tableName = getDbObjectName("Customers",conn);
			comm.CommandType = CommandType.Text;

			//First change ownerShip

			//chageOwnerShip(conn,"Categories","mainsoft");
			comm.CommandText = "SELECT * FROM mainsoft.CategoriesNew";
			da.Fill(ds);

			try
			{
				BeginCase("select table with diffrent owner - diffrent name");
				Compare(ds.Tables[0].Rows.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

			try
			{
				BeginCase("select table with diffrent owner - diffrent name --> negetive");
				ds.Tables.Clear();
				comm.CommandText = "select * from " + getDbObjectName("CategoriesNew",conn);
				da.Fill(ds);
			}
			catch (OracleException ex)
			{
				ExpectedExceptionCaught(ex);
			}
			catch 	{ExpectedExceptionNotCaught("OracleException"); }
			finally	{EndCase(exp); exp = null;}

			//Change back

			//chageOwnerShip(conn,"mainsoft.Categories","dbo");
		}

		#endregion

		#region select table with diffrent owner - same name

		//[Test]
		public void DoTestTypes8(OracleConnection conn)
		{
			Exception exp =null;
			DataSet ds = new DataSet();
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			//string tableName = getDbObjectName("Customers",conn);
			comm.CommandType = CommandType.Text;

			//First change ownerShip

			//chageOwnerShip(conn,"Categories","mainsoft");
			comm.CommandText = "SELECT * FROM GHTDB_EX.Categories";
			da.Fill(ds);

			try
			{
				BeginCase("Select table with diffrent owner same name");
				Compare(ds.Tables[0].Rows.Count ,2);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}

		}

		#endregion

		#region select table with diffrent owner - SP

		//[Test]
		public void DoTestTypes9(OracleConnection conn)
		{
			Exception exp =null;
			DataSet ds = new DataSet();
			BeginCase("Select table with diffrent owner SP");
			nonUniqueId = "48951" ;
			int expectedRowsCount = 5;
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			string tableName = getDbObjectName("Employees",conn);
			comm.CommandType = CommandType.StoredProcedure;

			//insertIntoStandatTable(conn,tableName,5,"EmployeeID");
			string[] arr = new string[2];
			arr[0] = "LastName";
			arr[1] = "FirstName";

			int maxValue = prepareTableForTest(conn,expectedRowsCount,"Employees","EmployeeID",arr);

			comm.Parameters.Add("CustomerIdPrm",maxValue.ToString()); 
			comm.Parameters.Add(new OracleParameter("result",OracleType.Cursor)).Direction = ParameterDirection.Output;

			
			try
			{
				comm.CommandText = "GHTDB_EX.GH_DUMMY";
				da.Fill(ds);
				Compare(ds.Tables[0].Rows.Count ,expectedRowsCount);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;
			cleanTableAfterTest (conn,"Employees","EmployeeID",maxValue); }

		}

		#endregion

		#region select table with diffrent owner - and diffrent structure
		//[Test]
		public void DoTestTypes10(OracleConnection conn)
		{
			Exception exp =null;
			DataSet ds = new DataSet();
			BeginCase("Select table with diffrent owner and diffrent structure");
			nonUniqueId = "48951" ;
			OracleCommand comm = new OracleCommand("",conn);
			OracleDataAdapter da = new OracleDataAdapter();
			da.SelectCommand = comm;
			string tableName = getDbObjectName("Categories",conn);
			comm.CommandType = CommandType.Text;
			OracleDataReader reader = null;

			try
			{
				comm.CommandText = "select CategoryID,CategoryName from " + tableName;
				reader = comm.ExecuteReader();
				//da.Fill(ds);
			}
			catch (Exception  ex) 
			{
				EndCase(ex);
			}
			finally
			{
				reader.Close();
			}

			comm.CommandText="select CategoryID,CategoryName  from  GHTDB_EX.Categories  where CategoryID = :a";

			comm.Parameters.Add("a","10");
			da.Fill(ds);

			try
			{
				Compare(ds.Tables[0].Rows.Count ,1);
			}
			catch(Exception ex)	{exp = ex;}
			finally	
			{
				EndCase(exp); exp = null;}

		}

		#endregion

		#region Oracle - use stored procedure inside package
		//[Test(Description="Call a stored procedure which is defined within a package.")]
		public void CallStoredProcedureInPackage(OracleConnection con)
		{
			if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.Oracle)
			{
				//Packages exist only in oracle.
				return;
			}

			Exception exp = null;
			OracleDataReader rdr = null;
			try
			{
				BeginCase("Call a stored procedure which is defined within a package.");
				exp = null;
				DataSet ds = new DataSet();
				OracleDataAdapter da = new OracleDataAdapter();
				OracleCommand cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "ghtpkg.ghsp_inPkg";
				cmd.Parameters.Add("CustomerIdPrm", "ALFKI");
				cmd.Parameters.Add(new OracleParameter("result",OracleType.Cursor)).Direction = ParameterDirection.Output;
				da.SelectCommand = cmd;

				da.Fill(ds);
				Compare(ds.Tables[0].Rows.Count, 1);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (rdr != null)
				{
					rdr.Close();
				}
				EndCase(exp);
			}
		}
		//[Test(Description="Call a stored procedure ghsp_pkgAmbig from a package, where ghsp_pkgAmbig is defined both inside and outside of a package.")]
		public void StoredProcedurePackageambiguity_InsidePackage(OracleConnection con)
		{
			if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.Oracle)
			{
				//Packages exist only in oracle.
				return;
			}

			Exception exp = null;
			OracleDataReader rdr = null;
			try
			{
				BeginCase("Call a stored procedure ghsp_pkgAmbig from a package, where ghsp_pkgAmbig is defined both inside and outside of a package.");
				exp = null;
				DataSet ds = new DataSet();
				OracleDataAdapter da = new OracleDataAdapter();
				OracleCommand cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "ghtpkg.ghsp_pkgAmbig";
				cmd.Parameters.Add(new OracleParameter("res",OracleType.Cursor)).Direction = ParameterDirection.Output;
				da.SelectCommand = cmd;

				da.Fill(ds);
				Compare(ds.Tables[0].Rows[0]["IN_PKG"], "TRUE");
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (rdr != null)
				{
					rdr.Close();
				}
				EndCase(exp);
			}
		}
		//[Test(Description="Call a stored procedure ghsp_pkgAmbig not from a package, where ghsp_pkgAmbig is defined both inside and outside of a package.")]
		public void StoredProcedurePackageambiguity_OutsidePackage(OracleConnection con)
		{
			if (ConnectedDataProvider.GetDbType(con) != DataBaseServer.Oracle)
			{
				//Packages exist only in oracle.
				return;
			}

			Exception exp = null;
			OracleDataReader rdr = null;
			try
			{
				BeginCase("Call a stored procedure ghsp_pkgAmbig not from a package, where ghsp_pkgAmbig is defined both inside and outside of a package.");
				exp = null;
				DataSet ds = new DataSet();
				OracleDataAdapter da = new OracleDataAdapter();
				OracleCommand cmd = new OracleCommand();
				cmd.Connection = con;
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.CommandText = "ghsp_pkgAmbig";
				cmd.Parameters.Add(new OracleParameter("res",OracleType.Cursor)).Direction = ParameterDirection.Output;
				da.SelectCommand = cmd;

				da.Fill(ds);
				Compare(ds.Tables[0].Rows[0]["IN_PKG"], "FALSE");
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (rdr != null)
				{
					rdr.Close();
				}
				EndCase(exp);
			}
		}
		#endregion

		private string getDbObjectName(string objectName,OracleConnection con)
		{
			return getDbObjectName(objectName,con,string.Empty);
			
		}
		private string getDbObjectName(string objectName,OracleConnection con,string databaseName)
		{
			switch (ConnectedDataProvider.GetDbType(con))
			{
				case DataBaseServer.SQLServer:
				case DataBaseServer.Sybase:
				{
					if (databaseName == string.Empty)
					{
						return "GHTDB.dbo." + objectName;
					}
					else
					{
						return  databaseName +  ".dbo." + objectName;
					}
				}
				case DataBaseServer.PostgreSQL:
				{
					return "public." + objectName.ToUpper();
				}
				case DataBaseServer.Oracle:
				{
					if (databaseName == string.Empty)
					{
						return "GHTDB." + objectName.ToUpper();
					}
					else
					{
						return  databaseName.ToUpper() + "." + objectName.ToUpper();
					}
					
				}
				case DataBaseServer.DB2: 
				{
					if (databaseName == string.Empty)
					{
						return   "DB2ADMIN." + objectName;
					}
					else
					{
						return  databaseName +  ".DB2ADMIN." + objectName;
					}

					

				}
				default:
				{
					throw new NotImplementedException();
				}


			}

		}



		/// <summary>
		/// This method will prepare table for test
		/// </summary>
		private int prepareTableForTest(OracleConnection con,int recordsNumber,string baseTableName,string keyField
			,params string[] otherNonNullableFieldsName)
		{
			string tableName = getDbObjectName(baseTableName,con);
			OracleCommand cmd = new OracleCommand("select max(" + keyField + ") from " + tableName,con);
			string str_ret = cmd.ExecuteScalar().ToString();
//			Console.WriteLine("ExecuteScalar:" + str_ret);
			// on some databases the max is on a field which is decimal
			decimal maxRecord = decimal.Parse(str_ret);
			int resultCount = Convert.ToInt32(maxRecord)+recordsNumber;
			string sqlStmt = string.Empty;
			string valueStmt = string.Empty;

			//Constrcut the statemnet once : --> TODO://Move this logic to seperate method
			for(int i=0;i<otherNonNullableFieldsName.Length;i++)
			{
				sqlStmt+= otherNonNullableFieldsName[i] + ",";
				valueStmt+="'a',";
			}

			//Trim the last ","
			if (otherNonNullableFieldsName.Length > 0)
			{
				sqlStmt =  sqlStmt.Remove(sqlStmt.Length-1,1);
				sqlStmt = "," + sqlStmt;

				valueStmt =  valueStmt.Remove(valueStmt.Length-1,1);
				valueStmt = "," + valueStmt;
			}
				

			for (int index=Convert.ToInt32(maxRecord)+1;index<=resultCount;index++)
			{
				cmd.CommandText="Insert into " + tableName + " (" + keyField + sqlStmt + ") values ("
					+ index + valueStmt + ")";
				cmd.ExecuteNonQuery();
			}
			return Convert.ToInt32(maxRecord);

		}
		private void cleanTableAfterTest(OracleConnection con,string baseTableName, string keyField, int recordNumber)
		{
			string tableName = getDbObjectName(baseTableName, con);
			OracleCommand cmd = new OracleCommand("delete from " + tableName + " where " + keyField + " > " + recordNumber  ,con);
			cmd.ExecuteNonQuery();

		}

		private void insertIntoStandatTable(OracleConnection con,string tableName,int recordsNumber,string keyField)
		{
			OracleCommand cmd = new OracleCommand("delete from " + tableName + " where  "  + keyField + "= '" + nonUniqueId + "'",con);
			cmd.ExecuteNonQuery();

			for (int index=0;index<recordsNumber;index++)
			{
				cmd.CommandText = "Insert into " + tableName + "(" + keyField + ") values ('" + nonUniqueId  + "')";
				cmd.ExecuteNonQuery();
			}
		}
		private void cleanStandatTable(OracleConnection con,string tableName,string keyField)
		{
			OracleCommand cmd = new OracleCommand("delete from " + tableName + " where " + keyField + " = '" + nonUniqueId + "'",con);
			cmd.ExecuteNonQuery();

		}

		private void chageOwnerShip(OracleConnection con,string objectName,string newOwner)
		{
			OracleCommand cmd = new OracleCommand();
			cmd.Connection = con;

			switch (ConnectedDataProvider.GetDbType(con))
			{
				case DataBaseServer.SQLServer:
				case DataBaseServer.Sybase:
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.CommandText = "[dbo].[sp_changeobjectowner]";
					cmd.Parameters.Add("@objname",objectName);
					cmd.Parameters.Add("@newowner",newOwner);
					cmd.ExecuteNonQuery();
					return;

				}
				default:
				{
					throw new NotImplementedException();
				}
			}

		}
	}
}