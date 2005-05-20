//
// SqlDataReaderTest.cs - NUnit Test Cases for testing the
//                          SqlDataReader class
// Author:
//      Umadevi S (sumadevi@novell.com)
//      Kornél Pál <http://www.kornelpal.hu/>
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
  public class SqlDataReaderTest : MSSqlTestClient {
          
          [SetUp]
          public void GetReady () {
                OpenConnection ();
          }

          [TearDown]
          public void Clean () {
		CloseConnection ();
          }


	 [Test]
	 public void ReadEmptyNTextFieldTest () {
		try {
			  using (conn) {
	 			 SqlCommand sqlCommand = conn.CreateCommand();
			         sqlCommand.CommandText = "CREATE TABLE #MonoTest (NAME ntext)";
			         sqlCommand.ExecuteNonQuery();
                                                                                                    
			         sqlCommand.CommandText = "INSERT INTO #MonoTest VALUES ('')"; //('')";
			         sqlCommand.ExecuteNonQuery();
                                                                                                    
			         sqlCommand.CommandText = "SELECT * FROM #MonoTest";
			         SqlDataReader dr = sqlCommand.ExecuteReader();
			         while (dr.Read()) {
			                Console.WriteLine(dr["NAME"].GetType().FullName);
					Assert.AreEqual("System.String",dr["NAME"].GetType().FullName);
            			 }
        		 }
		 }
                catch  (Exception e) {
                        Assert.Fail("A#01 Got an exception");
                        //Console.WriteLine(e.StackTrace);
                                                                                                    
                }
                                                                                                    
                finally { // try/catch is necessary to gracefully close connections^M
                                                                                                    
                        CloseConnection ();
                }
   


	 }		

		[Test]
		public void ReadBingIntTest() 
		{
			try 
			{
				string createquery = "SELECT CAST(548967465189498 AS bigint) AS Value";
				SqlCommand cmd = new SqlCommand();
				cmd.Connection = conn;
				cmd.CommandText = createquery;
				SqlDataReader r = cmd.ExecuteReader();
				using (r) 
				{
					if (r.Read()) 
					{
						long id = 0;
						try 
						{
							id = r.GetInt64(0);
						}
						catch (Exception e) 
						{
							Assert.Fail("A#01 Got an exception in GetInt64");
						}
						Assert.AreEqual(548967465189498, id);
						try 
						{
							id = r.GetSqlInt64(0).Value;
						}
						catch (Exception e) 
						{
							Assert.Fail("A#02 Got an exception in GetSqlInt64");
						}
						Assert.AreEqual(548967465189498, id);
					}
					else
						Assert.Fail("A#03 No rows returned");
				}
			}
			catch (Exception e) 
			{
				Assert.Fail("A#04 Got an exception");
				//Console.WriteLine(e.StackTrace);
			}
			finally 
			{ 
				// try/catch is necessary to gracefully close connections
				CloseConnection();
			}
		}
  }
}
