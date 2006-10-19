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

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataReader_GetValue : ADONetTesterClass
	{
		Exception exp;
		OracleConnection con;
		char [] Result;

		[SetUp]
		public void SetUp()
		{
				base.PrepareDataForTesting(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);

				con = new OracleConnection(MonoTests.System.Data.Utils.ConnectedDataProvider.ConnectionString);
				Result = new char[100];
				con.Open();
		}

		[TearDown]
		public void TearDown()
		{
			if (con != null && con.State == ConnectionState.Open) con.Close();
		}

		public static void Main()
		{
			OracleDataReader_GetValue tc = new OracleDataReader_GetValue();
			tc.exp = null;
			try
			{
				tc.BeginTest("OracleDataReader_GetValue");
				tc.SetUp();
				tc.run();
			}
			catch (Exception ex)
			{
				tc.exp = ex;
			}
			finally	
			{
				tc.EndTest(tc.exp);
				tc.TearDown();
			}
		}

		[Test]
		public void run()
		{
			TypesTests(ConnectedDataProvider.GetSimpleDbTypesParameters());
			TypesTests(ConnectedDataProvider.GetExtendedDbTypesParameters());
		}

		[Test]
		public void SimpleTest()
		{
			OracleDataReader rdr = null;
			exp = null;
			try
			{
				BeginCase("check value");
				
				OracleCommand cmd = new OracleCommand("Select LastName From Employees Where EmployeeID = 100", con);
				rdr = cmd.ExecuteReader();
				rdr.Read();

				Object obj = rdr.GetValue(0); 
				Compare(obj.ToString(), "Last100");
			} 
			catch (Exception ex)
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
				exp = null;
			}
		}
		private void TypesTests(DbTypeParametersCollection typesToTest)
		{
			exp = null;

			DbTypeParametersCollection currentlyTested = new DbTypeParametersCollection(typesToTest.TableName);
			string rowId = "13289";
			object dbValue;
			OracleDataReader rdr = null;
			OracleConnection selectCon = null;
			DataBaseServer testedDbServer = ConnectedDataProvider.GetDbType();

			foreach (DbTypeParameter currentParamType in typesToTest)
			{
				BeginCase("Test value of db type: " + currentParamType.DbTypeName);
				//Prepare data:
				rowId = string.Format("13289_{0}", this.TestCaseNumber);
				currentlyTested.Clear();
				currentlyTested.Add(currentParamType);
				currentlyTested.ExecuteInsert(rowId);

				try
				{
					currentlyTested.ExecuteSelectReader(rowId, out rdr, out selectCon);
					rdr.Read();
					dbValue = WorkaroundOracleCharsPaddingLimitation(testedDbServer, currentParamType, rdr.GetValue(0));
					if (currentParamType.Value.GetType().IsArray)
					{
						Compare(dbValue as Array, currentParamType.Value as Array);
					}
					else
					{
						Compare(dbValue, currentParamType.Value);
					}
				} 
				catch(Exception ex)
				{
					exp = ex;
				}
				finally
				{
					EndCase(exp);
					exp = null;
					if (rdr != null && !rdr.IsClosed)
					{
						rdr.Close();
					}
					if (selectCon != null && selectCon.State != ConnectionState.Closed)
					{
						selectCon.Close();
					}
					currentlyTested.ExecuteDelete(rowId);
				}
			}
		}
		/// <summary>
		/// This is a workaround for the extra white spaces added in oracle to NCHAR & NVARCHAR values.
		/// The problem is a documented GH limitation, see bug #3417.
		/// The workaround is to trim the lemgth of the returned string to the specified length of the parameter/column.
		/// </summary>
		/// <param name="testedServer">The database server we are currently running on.</param>
		/// <param name="val">The value returned from the database.</param>
		/// <returns>The normalized value..</returns>
		private object WorkaroundOracleCharsPaddingLimitation(DataBaseServer testedServer, DbTypeParameter currentParam, object val)
		{
			object origVal = val;
			string dbTypeName = currentParam.DbTypeName.ToUpper();
			if ( (testedServer == DataBaseServer.Oracle) && (dbTypeName == "CHAR" || dbTypeName == "NCHAR") )
			{
				val = ((string)val).Substring(0, currentParam.Size);
				Log(string.Format("Worked around oracle chars padding limitation by triming '{0}' to '{1}'", origVal, val));
			}
			return val;
		}


	}
}