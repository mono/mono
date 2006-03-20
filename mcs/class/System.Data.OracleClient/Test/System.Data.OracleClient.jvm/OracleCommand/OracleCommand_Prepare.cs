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
using System.Data.OracleClient ;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleCommand_Prepare : GHTBase
	{
		public static void Main()
		{
			OracleCommand_Prepare tc = new OracleCommand_Prepare();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleCommand_Prepare");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
			int intRecordsAffected = 0;

			string sql = "Update Shippers Set CompanyName=:CompName Where ShipperID = 2";
			OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
			OracleCommand cmd = new OracleCommand("", con);
			con.Open();


			//get expected result
			cmd.CommandText = "select count(*) from Shippers where ShipperID = 2";
			int ExpectedRows = int.Parse(cmd.ExecuteScalar().ToString());

			cmd.CommandText = sql;

			//Currently not running on DB2: .Net-Failed, GH:Pass
			//if (con.Provider.IndexOf("IBMDADB2") >= 0) return ;

			cmd.Parameters.Add(new OracleParameter()); 
			cmd.Parameters[0].ParameterName = "CompName";
			cmd.Parameters[0].OracleType = OracleType.VarChar; //System.InvalidOperationException:
			cmd.Parameters[0].Size = 20; //System.InvalidOperationException
			cmd.Parameters[0].SourceColumn = "CompanyName";
			cmd.Parameters[0].Value = "Comp1";

			try
			{
				BeginCase("Prepare Exception - missing OracleType");
				cmd.Prepare();
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			cmd.Parameters[0].OracleType = OracleType.VarChar; 

//			try
//			{
//				BeginCase("Prepare Exception - missing Size");
//				try
//				{
//					cmd.Parameters[0].Size = 0;
//					cmd.Prepare();
//				}
//				catch (Exception ex) {exp = ex;}
//				Compare(exp.GetType().FullName, typeof(InvalidOperationException).FullName );
//				exp=null;
//			} 
//			catch(Exception ex){exp = ex;}
//			finally{EndCase(exp); exp = null;}
//			cmd.Parameters[0].Size = 20;

			try
			{
				BeginCase("Prepare Exception - missing Size");
				try
				{
					con.Close();
					cmd.Prepare();
				}
				catch (Exception ex) {exp = ex;}
				Compare(exp.GetType().FullName, typeof(InvalidOperationException).FullName );
				exp=null;
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			con.Open();

			try
			{
				BeginCase("ExecuteNonQuery first time");
				intRecordsAffected = cmd.ExecuteNonQuery();
				Compare(intRecordsAffected , ExpectedRows);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}


			try
			{
				BeginCase("ExecuteNonQuery second time, chage value");
				cmd.Parameters[0].Value = "Comp2";
				intRecordsAffected  = cmd.ExecuteNonQuery();
				Compare(intRecordsAffected , ExpectedRows);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
	
			if (con.State == ConnectionState.Open) con.Close();

		}
	}

}