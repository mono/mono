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

using MonoTests.System.Data.Utils;


using NUnit.Framework;

#if DAAB
using Microsoft.ApplicationBlocks;
#endif
namespace MonoTests.System.Data.OracleClient
{
[TestFixture]
[Category ("NotWorking")]
public class OracleParameter_ctor_SOtype : ADONetTesterClass
{
	private Exception exp;
	// transaction use was add for PostgreSQL
	OracleTransaction tr;

	public static void Main()
	{
		OracleParameter_ctor_SOtype tc = new OracleParameter_ctor_SOtype();
		tc.exp = null;
		try
		{
			tc.BeginTest("OracleParameter_ctor_SOtype on " + ConnectedDataProvider.GetDbType().ToString());
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
		OracleParameter param = null;

		foreach (OracleType dbtype in Enum.GetValues(typeof(OracleType)))
		{

			param = new OracleParameter("myParam",dbtype);

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
//#if !TARGET_JVM
//	[Category("NotWorking")]
//#endif
	public void SimpleTypesWithDBNull()
	{
		OracleConnection con=null;
		OracleCommand cmd=null;
		OracleDataReader rdr=null;
		try
		{
			exp = null;
			BeginCase("Test simple types with DBNull");

			string connectionString = ConnectedDataProvider.ConnectionString;
			con = new	OracleConnection(connectionString);
			cmd = new OracleCommand();
			con.Open();
			// transaction use was add for PostgreSQL
			tr = con.BeginTransaction();
			
			cmd = new OracleCommand("", con, tr);
			cmd.CommandText = "GHSP_TYPES_SIMPLE_1";
			cmd.CommandType = CommandType.StoredProcedure;

			AddSimpleTypesNullParams(cmd);
			cmd.Parameters.Add(new OracleParameter("result",OracleType.Cursor)).Direction = ParameterDirection.Output;

#if !JAVA
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
#if DAAB

				rdr = Microsoft.ApplicationBlocks.Data.PostgresOracleHelper.OLEDB4ODBCExecuteReader(cmd,true);
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
			EndCase(exp);
			if(rdr != null && !rdr.IsClosed)
			{
				rdr.Close();
			}
			if (con != null && con.State == ConnectionState.Open)
			{
				con.Close();
			}
			exp=null;
		}
	}

	private void AddSimpleTypesNullParams(OracleCommand cmd)
	{
		OracleParameter tmpParam;
		tmpParam = new OracleParameter("T_NUMBER", OracleType.Number);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_LONG", OracleType.LongVarChar);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_FLOAT", OracleType.Float);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_VARCHAR", OracleType.VarChar);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_NVARCHAR", OracleType.NVarChar);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_CHAR", OracleType.Char);
		cmd.Parameters.Add(tmpParam);
		tmpParam = new OracleParameter("T_NCHAR", OracleType.NChar);
		cmd.Parameters.Add(tmpParam);

		foreach (OracleParameter current in cmd.Parameters)
		{
			current.Value = DBNull.Value;
		}
	}
}
}