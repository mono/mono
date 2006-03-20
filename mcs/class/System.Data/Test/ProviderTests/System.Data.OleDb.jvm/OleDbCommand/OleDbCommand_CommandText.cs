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

using MonoTests.System.Data.Utils.Data;

using NUnit.Framework;

namespace MonoTests.System.Data.OleDb
{
	[TestFixture]
	public class OleDbCommand_CommandText : GHTBase
	{
		private Exception exp = null;
		private OleDbCommand cmd;
		const string TEST_CASE_ID = "48341_";
		public static void Main()
		{
			OleDbCommand_CommandText tc = new OleDbCommand_CommandText();
			Exception exp = null;
			try
			{
				tc.BeginTest("OleDBCommandText");
				tc.run();
			}
			catch(Exception ex){exp = ex;}
			finally	{tc.EndTest(exp);}
		}

		public void run()
		{
			SetInConstractor();
			SetByProperty();
			UseSemiColonAsValue();
			UseColonAsValue();
			UseQuestionMarkAsValue();
			UseExclamationMarkAsValue();
			UseApostropheAsValue();
			UseCommaAsValue();
			UseDotAsValue();
			UseAtAsValue();
			UseQuoteAsValue();
			UseDollarAsValue();
			UsePercentAsValue();
			UseHatAsValue();
			UseAmpersnadAsValue();
			UseStartAsValue();
			UseParentesesAsValue();
			UsePlusAsValue();
			UseMinusAsValue();
			UseUnderscoreAsValue();
			UseSpaceAsValue();
			UseEqualAsValue();
			UseSlashAsValue();
			UseBackSlashAsValue();
			UseTildeAsValue();
			UseNOTAsValue();
			UseORAsValue();
			UseANDAsValue();
			UseSELECTAsValue();
			UseFROMAsValue();
			UseWHEREAsValue();
			UseINSERTAsValue();
			UseINTOAsValue();
			UseVALUESAsValue();
			UseDELETEAsValue();
			UseUPDATEAsValue();
			UseEXECAsValue();
			UseQueryAsValue();
		}
		[Test] public void SetByProperty()
		{
			exp = null;
			cmd = new OleDbCommand();
			cmd.CommandText = "SELECT * FROM Employees";
			try
			{
				BeginCase("CommandText2");
				Compare(cmd.CommandText, "SELECT * FROM Employees");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
			}
		}

		[Test] public void SetInConstractor()
		{
			exp = null;
			cmd = new OleDbCommand("SELECT * FROM Employees");
			try
			{
				BeginCase("CommandText1");
				Compare(cmd.CommandText, "SELECT * FROM Employees");
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
			}
		}
		[Test] public void UseSemiColonAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", ";");
		}
		[Test] public void UseColonAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", ":");
		}
		[Test] public void UseQuestionMarkAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "?");
		}
		[Test] public void UseExclamationMarkAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "?");
		}
		[Test] public void UseApostropheAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "'");
		}
		[Test] public void UseCommaAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", ",");
		}
		[Test] public void UseDotAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", ".");
		}
		[Test] public void UseAtAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "@");
		}
		[Test] public void UseQuoteAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "\"");
		}
		[Test] public void UseDiezAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "#");
		}
		[Test] public void UseDollarAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "$");
		}
		[Test] public void UsePercentAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "%");
		}
		[Test] public void UseHatAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "^");
		}
		[Test] public void UseAmpersnadAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "&");
		}
		[Test] public void UseStartAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "*");
		}
		[Test] public void UseParentesesAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "(");
			RunValueInColumnTest("T_VARCHAR", "()");
			RunValueInColumnTest("T_VARCHAR", ")");
			RunValueInColumnTest("T_VARCHAR", "{");
			RunValueInColumnTest("T_VARCHAR", "{}");
			RunValueInColumnTest("T_VARCHAR", "}");
			RunValueInColumnTest("T_VARCHAR", "[");
			RunValueInColumnTest("T_VARCHAR", "[]");
			RunValueInColumnTest("T_VARCHAR", "]");
			RunValueInColumnTest("T_VARCHAR", "<");
			RunValueInColumnTest("T_VARCHAR", "<>");
			RunValueInColumnTest("T_VARCHAR", ">");
		}
		[Test] public void UsePlusAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "+");
		}
		[Test] public void UseMinusAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "-");
		}
		[Test] public void UseUnderscoreAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "_");
		}
		[Test] public void UseSpaceAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", " ");
		}
		[Test] public void UseEqualAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "=");
		}
		[Test] public void UseSlashAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "\\");
		}
		[Test] public void UseBackSlashAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "/");
		}
		[Test] public void UseTildeAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "~");
		}
		[Test] public void UseNOTAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "NOT");
		}
		[Test] public void UseORAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "OR");
		}
		[Test] public void UseANDAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "AND");
		}
		[Test] public void UseSELECTAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "SELECT");
		}
		[Test] public void UseFROMAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "FROM");
		}
		[Test] public void UseWHEREAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "WHERE");
		}
		[Test] public void UseINSERTAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "INSERT");
		}
		[Test] public void UseINTOAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "INTO");
		}
		[Test] public void UseVALUESAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "VALUES");
		}
		[Test] public void UseDELETEAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "DELETE");
		}
		[Test] public void UseUPDATEAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "UPDATE");
		}
		[Test] public void UseEXECAsValue()
		{
			RunValueInColumnTest("T_VARCHAR", "EXEC");
		}

		[Test] public void UseQueryAsValue()
		{
			string columnName;
			switch (ConnectedDataProvider.GetDbType())
			{
				case DataBaseServer.SQLServer:
					columnName = "T_VARCHAR";
					break;
				case DataBaseServer.Oracle:
					columnName = "T_LONG";
					break;
				case DataBaseServer.DB2:
					columnName = "T_LONGVARCHAR";
					break;
				default:
					columnName = "T_VARCHAR";
					break;

			}
			RunValueInColumnTest(columnName, "SELECT * FROM TYPES_SIMPLE");
		}

		private void RunValueInColumnTest(string columnToTest, string valueToTest)
		{
			UnQuotedValueInColumn(columnToTest, valueToTest);
			QuotedValueInColumn(columnToTest, valueToTest);
		}
		private void QuotedValueInColumn(string columnToTest, string valueToTest)
		{
			ValueInColumn(columnToTest, string.Format("'{0}'", valueToTest));
		}
		private void UnQuotedValueInColumn(string columnToTest, string valueToTest)
		{
			ValueInColumn(columnToTest, valueToTest);
		}
		private void ValueInColumn(string columnToTest, string valueToTest)
		{
			exp = null;
			OleDbDataReader rdr = null;
			OleDbConnection con = null;
			DbTypeParametersCollection row = ConnectedDataProvider.GetSimpleDbTypesParameters();
			BeginCase(string.Format("Use {0} as value", valueToTest));
			string rowId = TEST_CASE_ID + TestCaseNumber.ToString();
			try
			{
				foreach(DbTypeParameter param in row)
				{
					param.Value = DBNull.Value;
				}
				row[columnToTest].Value = valueToTest;
				Log("rowId:" + rowId + " columnToTest:" + columnToTest + " valueToTest:" + valueToTest);
				row.ExecuteInsert(rowId);
				row.ExecuteSelectReader(rowId, out rdr, out con);
				rdr.Read();
				int columnOrdinal = rdr.GetOrdinal(columnToTest);
				//this.Log(valueToTest);
				Compare(valueToTest, rdr.GetValue(columnOrdinal));
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				row.ExecuteDelete(rowId);
				if (con != null && con.State != ConnectionState.Closed)
				{
					con.Close();
				}
				if (rdr != null && !rdr.IsClosed)
				{
					rdr.Close();
				}
			}
		}

	}
}