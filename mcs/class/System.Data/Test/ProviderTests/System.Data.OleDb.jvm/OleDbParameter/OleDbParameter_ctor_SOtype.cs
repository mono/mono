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
using System.Data.OleDb;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

#if DAAB
using Microsoft.ApplicationBlocks;
#endif
namespace MonoTests.System.Data.OleDb
{
[TestFixture]
public class OleDbParameter_ctor_SOtype : ADONetTesterClass
{
	private Exception exp;
	// transaction use was add for PostgreSQL
	OleDbTransaction tr;

	public static void Main()
	{
		OleDbParameter_ctor_SOtype tc = new OleDbParameter_ctor_SOtype();
		tc.exp = null;
		try
		{
			tc.BeginTest("OleDbParameter_ctor_SOtype on " + ConnectedDataProvider.GetDbType().ToString());
			tc.run();
		}
		catch(Exception ex){tc.exp = ex;}
		finally	{tc.EndTest(tc.exp);}
	}

	public void run()
	{
		Log(string.Format("DB Server={0}.", ConnectedDataProvider.GetDbType()));
		AllTypes();
		SimpleTypesWithDBNull();
	}

	[Test]
	public void AllTypes()
	{
		exp = null;
		OleDbParameter param = null;

		foreach (OleDbType dbtype in Enum.GetValues(typeof(OleDbType)))
		{
			// not supporting OleDbType.IDispatch and OleDbType.IUnknown
			if (( dbtype == OleDbType.IDispatch) || (dbtype == OleDbType.IUnknown) )
			{
				continue;
			}

			param = new OleDbParameter("myParam",dbtype);

			try
			{
				BeginCase("ctor " + dbtype.ToString());
				Compare(param != null, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			try
			{
				BeginCase("name " + dbtype.ToString());
				Compare(param.ParameterName ,"myParam");
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

		}
	}

	[Test]
	public void SimpleTypesWithDBNull()
	{
		OleDbConnection con=null;
		OleDbCommand cmd=null;
		OleDbDataReader rdr=null;
		try
		{
			exp = null;
			BeginCase("Test simple types with DBNull");

			string connectionString = ConnectedDataProvider.ConnectionString;
			con = new	OleDbConnection(connectionString);
			cmd = new OleDbCommand();
			con.Open();
			// transaction use was add for PostgreSQL
			tr = con.BeginTransaction();
			
			cmd = new OleDbCommand("", con, tr);
			cmd.CommandText = "GHSP_TYPES_SIMPLE_1";
			cmd.CommandType = CommandType.StoredProcedure;

			AddSimpleTypesNullParams(cmd);

#if !JAVA
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
#if DAAB

				rdr = Microsoft.ApplicationBlocks.Data.PostgresOleDbHelper.OLEDB4ODBCExecuteReader(cmd,true);
#endif

			}
			else
#endif
			{

				rdr = cmd.ExecuteReader();
			}

			rdr.Read();
			for (int i=0; i<rdr.FieldCount; i++)
			{
				Compare(DBNull.Value, rdr.GetValue(i));
			}
			rdr.Close();
		}
		catch (Exception ex)
		{
			exp = ex;
		}
		finally
		{
			if(rdr != null && !rdr.IsClosed)
			{
				rdr.Close();
			}
			if (con != null && con.State == ConnectionState.Open)
			{
				con.Close();
			}
			EndCase(exp);
			exp=null;
		}
	}

	private void AddSimpleTypesNullParams(OleDbCommand cmd)
	{
		OleDbParameter tmpParam;

		switch (ConnectedDataProvider.GetDbType())
		{
			case DataBaseServer.SQLServer:
				tmpParam = new OleDbParameter("bit", OleDbType.Boolean);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("tinyint", OleDbType.TinyInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("smallint", OleDbType.SmallInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("integer", OleDbType.Integer);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("bigint", OleDbType.BigInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("decimal", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("numeric", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("float", OleDbType.Double);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("real", OleDbType.Single);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("char", OleDbType.Char);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("nchar", OleDbType.WChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("varchar", OleDbType.VarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("nvarchar", OleDbType.VarWChar);
				cmd.Parameters.Add(tmpParam);
				break;
			case DataBaseServer.Sybase:
				tmpParam = new OleDbParameter("tinyint", OleDbType.TinyInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("smallint", OleDbType.SmallInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("integer", OleDbType.Integer);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("decimal", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("numeric", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("float", OleDbType.Double);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("real", OleDbType.Single);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("char", OleDbType.Char);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("nchar", OleDbType.WChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("varchar", OleDbType.VarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("nvarchar", OleDbType.VarWChar);
				cmd.Parameters.Add(tmpParam);
				break;
			case DataBaseServer.Oracle:
				tmpParam = new OleDbParameter("NUMBER", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("LONG", OleDbType.LongVarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("FLOAT", OleDbType.Single);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("VARCHAR", OleDbType.VarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("NVARCHAR", OleDbType.VarWChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("CHAR", OleDbType.Char);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("NCHAR", OleDbType.WChar);
				cmd.Parameters.Add(tmpParam);
				break;
			case DataBaseServer.PostgreSQL:
				tmpParam = new OleDbParameter("bool", OleDbType.Boolean);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("int2", OleDbType.SmallInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("int4", OleDbType.Integer);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("int8", OleDbType.BigInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("numeric", OleDbType.Numeric);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("float4", OleDbType.Single);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("float8", OleDbType.Double);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("varchar", OleDbType.VarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("CHAR", OleDbType.Char);//bpchar
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("NCHAR", OleDbType.Char);//bpchar
				cmd.Parameters.Add(tmpParam);
				break;
			case DataBaseServer.DB2:
				tmpParam = new OleDbParameter("SMALLINT", OleDbType.SmallInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("INTEGER", OleDbType.Integer);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("BIGINT", OleDbType.BigInt);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("DECIMAL", OleDbType.Decimal);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("REAL", OleDbType.Double);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("DOUBLE", OleDbType.Double);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("CHARACTER", OleDbType.Char);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("VARCHAR", OleDbType.VarChar);
				cmd.Parameters.Add(tmpParam);
				tmpParam = new OleDbParameter("LONGVARCHAR", OleDbType.LongVarChar);
				cmd.Parameters.Add(tmpParam);
				break;
			default:
				this.Fail("Unknown DataBaseServer type.");
				break;
		}

		foreach (OleDbParameter current in cmd.Parameters)
		{
			current.Value = DBNull.Value;
		}
	}
}
}