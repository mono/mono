//
// ReadPostgresData.cs
//
// Uses the PostgresLibrary to retrieve a recordset.
// This is not meant to be used in Production, but as a
// learning aid in coding class System.Data.SqlClient.SqlDataReader.
//
// Author:
//	Daniel Morgan <danmorg@sc.rr.com>
//
// (C) 2002 Daniel Morgan
//

using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace LearnToCreateSqlDataReader
{
	sealed public class PostgresHelper {

		public static object OidTypeToSystem (int oid, string value) {
			object obj = null;

			switch(oid) {
			case 1043: // varchar
				Console.WriteLine("oid 1023 varchar ==> String found");
				obj = (object) String.Copy(value); // String
				break;
			case 25: // text
				Console.WriteLine("oid 25 text ==> String found");
				obj = (object) String.Copy(value); // String
				break;
			case 18: // char
				Console.WriteLine("oid 18 char ==> String found");
				obj = (object) String.Copy(value); // String
				break;
			case 16: // bool
				Console.WriteLine("oid 16 bool ==> Boolean found");
				obj = (object) Boolean.Parse(value);
				break;
			case 21: // int2
				Console.WriteLine("oid 21 int2 ==> Int16 found");
				obj = (object) Int16.Parse(value);
				break;
			case 23: // int4
				Console.WriteLine("oid 23 int4 ==> Int32 found");
				obj = (object) Int32.Parse(value);
				break;
			case 20: // int8
				Console.WriteLine("oid 20 int8 ==> Int64 found");
				obj = (object) Int64.Parse(value);
				break;
			default:
				Console.WriteLine("OidTypeToSystem Not Done Yet: oid: " +
					oid + " Value: " + value);
				break;

			}

			return obj;
		}

		public static Type OidToType (int oid) {
			Type typ = null;

			switch(oid) {
			case 1043: // varchar
			case 25: // text
			case 18: // char
				typ = typeof(String);
				break;
			case 16: // bool
				typ = typeof(Boolean);
				break;
			case 21: // int2
				typ = typeof(Int16);
				break;
			case 23: // int4
				typ = typeof(Int32);
				break;
			case 20: // int8
				typ = typeof(Int64);
				break;
			default:
				throw new NotImplementedException(
					"PGNI2: PostgreSQL oid type " + oid +
					" not mapped to .NET System Type.");
			}
			return typ;
		}

	}

	sealed public class PostgresLibrary {

		public enum ConnStatusType {
			CONNECTION_OK,
			CONNECTION_BAD,
			CONNECTION_STARTED,
			CONNECTION_MADE,
			CONNECTION_AWAITING_RESPONSE,
			CONNECTION_AUTH_OK,			 
			CONNECTION_SETENV		
		} 

		public enum PostgresPollingStatusType {
			PGRES_POLLING_FAILED = 0,
			PGRES_POLLING_READING,
			PGRES_POLLING_WRITING,
			PGRES_POLLING_OK,
			PGRES_POLLING_ACTIVE
		}

		public enum ExecStatusType {
			PGRES_EMPTY_QUERY = 0,
			PGRES_COMMAND_OK,			
			PGRES_TUPLES_OK,			
			PGRES_COPY_OUT,				
			PGRES_COPY_IN,				
			PGRES_BAD_RESPONSE,			
			PGRES_NONFATAL_ERROR,
			PGRES_FATAL_ERROR
		}


		[DllImport("pq")]
		public static extern string PQerrorMessage (IntPtr conn);
		// char *PQerrorMessage(const PGconn *conn);

		[DllImport("pq")]
		public static extern IntPtr PQconnectdb(String conninfo);
		// PGconn *PQconnectdb(const char *conninfo)

		[DllImport("pq")]
		public static extern void PQfinish(IntPtr conn);
		// void PQfinish(PGconn *conn)
		
		[DllImport("pq")]
		public static extern IntPtr PQexec(IntPtr conn,
			String query);
		// PGresult *PQexec(PGconn *conn,	const char *query);

		[DllImport("pq")]
		public static extern int PQntuples (IntPtr res);
		// int PQntuples(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQnfields (IntPtr res);
		// int PQnfields(const PGresult *res);

		[DllImport("pq")]
		public static extern ConnStatusType PQstatus (IntPtr conn);
		// ConnStatusType PQstatus(const PGconn *conn);
		[DllImport("pq")]
		public static extern ExecStatusType PQresultStatus (IntPtr res);
		// ExecStatusType PQresultStatus(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQresStatus (ExecStatusType status);
		// char *PQresStatus(ExecStatusType status);

		[DllImport("pq")]
		public static extern string PQresultErrorMessage (IntPtr res);
		// char *PQresultErrorMessage(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQbinaryTuples (IntPtr res);
		// int PQbinaryTuples(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQfname (IntPtr res,
			int field_num);
		// char *PQfname(const PGresult *res,
		//      int field_num);

		[DllImport("pq")]
		public static extern int PQfnumber (IntPtr res,
			string field_name);
		// int PQfnumber(const PGresult *res, 
		//      const char *field_name);


		[DllImport("pq")]
		public static extern int PQfmod (IntPtr res, int field_num);
		// int PQfmod(const PGresult *res, int field_num);

		[DllImport("pq")]
		public static extern int PQftype (IntPtr res,
			int field_num);
		// Oid PQftype(const PGresult *res,
		//      int field_num);

		[DllImport("pq")]
		public static extern int PQfsize (IntPtr res,
			int field_num);
		// int PQfsize(const PGresult *res,
		//      int field_num);

		[DllImport("pq")]
		public static extern string PQcmdStatus (IntPtr res);
		// char *PQcmdStatus(PGresult *res);

		[DllImport("pq")]
		public static extern string PQoidStatus (IntPtr res);
		// char *PQoidStatus(const PGresult *res);

		[DllImport("pq")]
		public static extern int PQoidValue (IntPtr res);
		// Oid PQoidValue(const PGresult *res);

		[DllImport("pq")]
		public static extern string PQcmdTuples (IntPtr res);
		// char *PQcmdTuples(PGresult *res);

		[DllImport("pq")]
		public static extern string PQgetvalue (IntPtr res,
			int tup_num, int field_num);
		// char *PQgetvalue(const PGresult *res,
		//      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern int PQgetlength (IntPtr res,
			int tup_num, int field_num);
		// int PQgetlength(const PGresult *res,
		//      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern int PQgetisnull (IntPtr res,
			int tup_num, int field_num);
		// int PQgetisnull(const PGresult *res,
		//      int tup_num, int field_num);

		[DllImport("pq")]
		public static extern void PQclear (IntPtr res);
		// void PQclear(PGresult *res);


	}

	public class ReadPostgresData
	{

		static void Test() {
			String errorMessage;

			IntPtr pgConn;
			String sConnInfo;
			PostgresLibrary.ConnStatusType connStatus;

			String sQuery;
			IntPtr pgResult;

			sConnInfo = "host=localhost dbname=test user=danmorg password=viewsonic";
			
			sQuery = 
				"select tid, tdesc " +
				"from sometable ";
		
			pgConn = PostgresLibrary.PQconnectdb (sConnInfo);

			connStatus = PostgresLibrary.PQstatus (pgConn);
			if(connStatus == 
				PostgresLibrary.
				ConnStatusType.CONNECTION_OK) {

				Console.WriteLine("CONNECTION_OK");

				pgResult = PostgresLibrary.PQexec(pgConn, sQuery);

				PostgresLibrary.ExecStatusType execStatus;

				execStatus = PostgresLibrary.
					PQresultStatus (pgResult);

				if(execStatus == 
					PostgresLibrary.
					ExecStatusType.PGRES_TUPLES_OK) 
				{
					Console.WriteLine("PGRES_TUPLES_OK");
					
					int nRows = PostgresLibrary.
						PQntuples(pgResult);
					Console.WriteLine("Rows: " + nRows);

					int nFields = PostgresLibrary.
						PQnfields(pgResult);
					Console.WriteLine("Columns: " + nFields);


					String fieldName;
					
					// get meta data fromm result set (schema)
					// for each column (field)
					for(int fieldIndex = 0; 
						fieldIndex < nFields; 
						fieldIndex ++) {

						// get column name
						fieldName = PostgresLibrary.
							PQfname(pgResult, fieldIndex);

						Console.WriteLine("Field " + 
							fieldIndex + ": " +
							fieldName);

						int oid;
						// get PostgreSQL data type (OID)
						oid = PostgresLibrary.
							PQftype(pgResult, fieldIndex);

						Console.WriteLine("Data Type oid: " + oid);

						int definedSize;
						// get defined size of column
						definedSize = PostgresLibrary.
							PQfsize(pgResult, fieldIndex);

						Console.WriteLine("definedSize: " +
							definedSize);
					}

					// for each row and column, get the data value
					for(int row = 0; 
						row < nRows; 
						row++) {

						for(int col = 0; 
							col < nFields; 
							col++) {

							String value;
							// get data value
							value = PostgresLibrary.
									PQgetvalue(
										pgResult,
										row, col);

							Console.WriteLine("Row: " + row +
									" Col: " + col);
							Console.WriteLine("Value: " +
									value);

							int columnIsNull;
							// is column NULL?
							columnIsNull = PostgresLibrary.
								PQgetisnull(pgResult,
									row, col);

							Console.WriteLine("Data is " + 
								(columnIsNull == 0 ? "NOT NULL" : "NULL"));


							int actualLength;
							// get Actual Length
							actualLength = PostgresLibrary.
								PQgetlength(pgResult,
									row, col);

							Console.WriteLine("Actual Length: " +
								actualLength);
						}
					}

					// close result set
					PostgresLibrary.PQclear (pgResult);
				}
				else {
					// display execution error				
					errorMessage = PostgresLibrary.
						PQresStatus(execStatus);

					errorMessage += " " + PostgresLibrary.
						PQresultErrorMessage(pgResult);

					Console.WriteLine(errorMessage);
				}

				// close database conneciton
				PostgresLibrary.PQfinish(pgConn);

			}
			else {
				errorMessage = PostgresLibrary.
					PQerrorMessage (pgConn);
				errorMessage += ": Could not connect to database.";
				Console.WriteLine(errorMessage);
			}	
			
		}

		public static object ExecuteScalar(string sql) {
			object obj = null; // return

			int nRow;
			int nCol;

			String errorMessage;

			IntPtr pgConn;
			String sConnInfo;
			PostgresLibrary.ConnStatusType connStatus;

			String sQuery;
			IntPtr pgResult;

			sConnInfo = "host=localhost dbname=test user=danmorg password=viewsonic";
			
			sQuery = sql;
		
			pgConn = PostgresLibrary.PQconnectdb (sConnInfo);

			connStatus = PostgresLibrary.PQstatus (pgConn);
			if(connStatus == 
				PostgresLibrary.
				ConnStatusType.CONNECTION_OK) {

				Console.WriteLine("CONNECTION_OK");

				pgResult = PostgresLibrary.PQexec(pgConn, sQuery);

				PostgresLibrary.ExecStatusType execStatus;

				execStatus = PostgresLibrary.
					PQresultStatus (pgResult);

				if(execStatus == 
					PostgresLibrary.
					ExecStatusType.PGRES_TUPLES_OK) {

					Console.WriteLine("PGRES_TUPLES_OK");
					
					int nRows = PostgresLibrary.
						PQntuples(pgResult);
					Console.WriteLine("Rows: " + nRows);

					int nFields = PostgresLibrary.
						PQnfields(pgResult);
					Console.WriteLine("Columns: " + nFields);
					if(nRows > 0 && nFields > 0) {
						nRow = 0;
						nCol = 0;

						// get column name
						String fieldName;
						fieldName = PostgresLibrary.
							PQfname(pgResult, nCol);

						Console.WriteLine("Field " + 
							nCol + ": " +
							fieldName);

						int oid;
						
						// get PostgreSQL data type (OID)
						oid = PostgresLibrary.
							PQftype(pgResult, nCol);

						Console.WriteLine("Data Type oid: " + oid);

						int definedSize;
						// get defined size of column
						definedSize = PostgresLibrary.
							PQfsize(pgResult, nCol);

						Console.WriteLine("DefinedSize: " + 
							definedSize);

						String value;
						// get data value
						value = PostgresLibrary.
							PQgetvalue(
							pgResult,
							nRow, nCol);
                                                
						Console.WriteLine("Row: " + nRow +
							" Col: " + nCol);
						Console.WriteLine("Value: " + value);

						int columnIsNull;
						// is column NULL?
						columnIsNull = PostgresLibrary.
							PQgetisnull(pgResult,
							nRow, nCol);

						Console.WriteLine("Data is " + 
							(columnIsNull == 0 ? "NOT NULL" : "NULL"));

						int actualLength;
						// get Actual Length
						actualLength = PostgresLibrary.
							PQgetlength(pgResult,
							nRow, nCol);

						Console.WriteLine("Actual Length: " +
							actualLength);
						
						obj = PostgresHelper.
							OidTypeToSystem (oid, value);
					}

					// close result set
					PostgresLibrary.PQclear (pgResult);
				}
				else {
					// display execution error				
					errorMessage = PostgresLibrary.
						PQresStatus(execStatus);

					errorMessage += " " + PostgresLibrary.
						PQresultErrorMessage(pgResult);

					Console.WriteLine(errorMessage);
				}

				// close database conneciton
				PostgresLibrary.PQfinish(pgConn);

			}
			else {
				errorMessage = PostgresLibrary.
					PQerrorMessage (pgConn);
				errorMessage += ": Could not connect to database.";
				Console.WriteLine(errorMessage);
			}
			
			return obj;
		}

		static void TestExecuteScalar() {
			String selectStatement;

			try {
				selectStatement = 
					"select count(*) " +
					"from sometable";
				Int64 myCount = (Int64) ExecuteScalar(selectStatement);
				Console.WriteLine("Count: " + myCount);

				selectStatement = 
					"select max(tdesc) " +
					"from sometable";			
				string myMax = (string) ExecuteScalar(selectStatement);
				Console.WriteLine("Max: " + myMax);
			}
			catch(Exception e) {
				Console.WriteLine(e);
			}                        
			
		}

		[STAThread]
		static void Main(string[] args)
		{
			// Test();

			// TestExecuteScalar();

			Type t;
			int oid;

			oid = 1043;
			t = PostgresHelper.OidToType(oid); // varchar ==> String
			Console.WriteLine("OidToType varchar oid: " + oid +
				" ==> t: " + t.ToString());

			oid = 23;
			t = PostgresHelper.OidToType(oid);  // int4 ==> Int32
			Console.WriteLine("OidToType int4 oid: " + oid +
				" ==> t: " + t.ToString());

		}
	}
}
