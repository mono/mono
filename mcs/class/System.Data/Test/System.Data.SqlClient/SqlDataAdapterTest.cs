//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                          SqlDataAdapter class
// Author:
//      Umadevi S (sumadevi@novell.com)
//
// Copyright (c) 2004 Novell Inc., and the individuals listed
// on the ChangeLog entries.
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
using System.Data.Common;
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{

  [TestFixture]
  public class SqlDataAdapterTest : MSSqlTestClient {
          
          [SetUp]
          public void GetReady () {
                OpenConnection ();
          }

          [TearDown]
          public void Clean () {
		CloseConnection ();
          }

	 [Test]
	  /**
	  The below test will not run everytime, since the region id column is unique
	   so change the regionid if you want the test to pass.
	  **/	
          public void UpdateTest () {
		try    {
			DataTable dt = new DataTable();
			SqlDataAdapter da = null;
			da = new SqlDataAdapter("Select * from Region;", conn);
			da.Fill(dt);
			SqlCommandBuilder cb = new SqlCommandBuilder (da);
			DataRow dr = dt.NewRow();

			dr["RegionID"] = 101;
			dr["RegionDescription"] = "Boston";
	
			dt.Rows.Add(dr);
			da.Update(dt);
	

                        }
		catch  (Exception e) {
			Assert.Fail("A#01 Got an exception");
			//Console.WriteLine(e.StackTrace);
			
		}
                 
		finally { // try/catch is necessary to gracefully close connections
                       
                        CloseConnection ();
                }
          }


	[Test]
	public void FillSchemaTest() {
		try {
			

			string sql = "select * from Region;";
			SqlCommand c = conn.CreateCommand();
			c.CommandText = sql;
			SqlDataReader dr =
c.ExecuteReader(CommandBehavior.KeyInfo|CommandBehavior.SchemaOnly);
			DataTable schema = dr.GetSchemaTable();
			// check the schema details. 
			/*foreach (DataColumn col in schema.Columns)
				Console.Write(col.ColumnName+' ');*/
			DataRowCollection drc = schema.Rows;
			DataRow r = drc[0];
			Assert.AreEqual("RegionID",r[0].ToString());
                                                                                                    
                }
                                                                   
                catch (Exception e) {
                        Assert.Fail("A#02 : Got an exception");
			Console.WriteLine(e.StackTrace);
                                                                                                    
                }
                                                                                                    
                finally { // try/catch is necessary to gracefully close connections^M
                                                                                                    
                        CloseConnection ();
                }

		

	}


	
	
    }
}
