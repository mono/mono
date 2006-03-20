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

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataReader_GetBoolean : ADONetTesterClass
	{
		public static void Main()
		{
			OracleDataReader_GetBoolean tc = new OracleDataReader_GetBoolean();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataReader_GetBoolean");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		[Test]
		public void run()
		{
			Exception exp = null;
		
			OracleConnection con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
#if !JAVA
			// PostgreSQL ODBC treats Type BOOL as String, so we don't run it on .NET
			if ((ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL))
			{
				return; 
			}
#endif

			//Do not test with oracle or DB2, because boolean does not exist in their types.
			if ( (ConnectedDataProvider.GetDbType(con) == DataBaseServer.Oracle) || (ConnectedDataProvider.GetDbType(con) == DataBaseServer.DB2) )
			{
				return; 
			}

			con.Open();
			string fieldName = "t_bit";
			if (ConnectedDataProvider.GetDbType(con) == DataBaseServer.PostgreSQL)
			{
				fieldName ="t_bool";
			}
			
			OracleCommand cmd = new OracleCommand("Select " + fieldName + " From TYPES_SIMPLE Where id = '0'",con);
			OracleDataReader rdr = cmd.ExecuteReader();
			rdr.Read();

			try
			{
				BeginCase("GetBoolean true");
				Boolean blnValue;
				Compare(rdr.IsDBNull(0), false);
				Compare("System.Boolean", rdr.GetValue(0).GetType().FullName);
				blnValue = rdr.GetBoolean(0);
				Compare(blnValue, true);
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}

			rdr.Close();
              
			
			if (con.State == ConnectionState.Open) con.Close();

		}
	}
}