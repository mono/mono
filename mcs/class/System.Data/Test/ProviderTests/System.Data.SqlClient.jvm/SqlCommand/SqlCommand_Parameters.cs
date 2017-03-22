using System;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommand_Parameters : ADONetTesterClass
	{
		Exception exp;

		public static void Main()
		{
			SqlCommand_Parameters tc = new SqlCommand_Parameters();
			tc.exp = null;
			try
			{
				tc.BeginTest("SqlCommand_Parameters");
				tc.run();
			}
			catch(Exception ex)
			{
				tc.exp = ex;
			}
			finally
			{
				tc.EndTest(tc.exp);
			}
		}

		[Test] 
		public void run()
		{
			// testing only SQLServerr
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				Log("This test is relevant only for MSSQLServer!");
				return;
			}

			CommandParameterTreatBitAsBoolean();
			DoTestparametersBindByNameOnMSSQLServer();
		
		}

		//Bug 2814 - MSSQL - Command.Parameters treat bit as Boolean ---- 
		public void CommandParameterTreatBitAsBoolean()
		{
			exp=null;
			SqlConnection con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
			try
			{
				BeginCase("Bug 2814 - MSSQL - Command.Parameters treat bit as Boolean");
				SqlCommand cmd = new SqlCommand("SELECT * FROM Products where ProductID = @ProductID AND Discontinued = @Discontinued",con);
				cmd.Connection = con;
				con.Open();
				cmd.CommandType = CommandType.Text;
						
				cmd.Parameters.Add( new SqlParameter("@ProductID", SqlDbType.Int, 4));
				cmd.Parameters.Add( new SqlParameter("@Discontinued", SqlDbType.Int, 4));
				
				cmd.Parameters["@ProductID"].Value = 5;
				cmd.Parameters["@Discontinued"].Value = 1;
		
				SqlDataReader dr = cmd.ExecuteReader();	
				if (dr.HasRows)
				{
					dr.Read();
					Compare(dr.GetValue(0).ToString(),"5");
				}
				else
				{
					Fail("HasRows is not 0.");
				}					
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				if (con.State == ConnectionState.Open) 
				{
					con.Close();
				}
				EndCase(exp);
				exp = null;
			}
		}
		/// <summary>
		/// Binding parameters in MSSQLServer should be done by parameter name, regardless of their order.
		/// </summary>
		public void DoTestparametersBindByNameOnMSSQLServer()
		{
			SqlConnection conn = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);
			SqlDataReader rdr;
			try
			{
				BeginCase("Insert parameters of the same types in different order.");
				SqlCommand cmd = new SqlCommand();
				conn.Open();
				cmd.Connection = conn;

				cmd.CommandText = "SalesByCategory";
				cmd.CommandType = CommandType.StoredProcedure;
				
				//Stored procedure is declared as "SalesByCategory @CategoryName nvarchar(15), @OrdYear nvarchar(4) = '1998'"
				//The test declares them in reverse order.
				cmd.Parameters.Add("@OrdYear", "1996");
				cmd.Parameters.Add("@CategoryName", "Beverages");

				rdr = cmd.ExecuteReader();
				int actualAffectedRows = 0;
				while (rdr.Read())
				{
					actualAffectedRows++;
				}
				Compare(actualAffectedRows, 12);
			}
			catch (Exception ex)
			{
				exp = ex;
			}
			finally
			{
				EndCase(exp);
				exp = null;
				if (conn.State != ConnectionState.Closed)
				{
					conn.Close();
				}
			}
		}
	}
}