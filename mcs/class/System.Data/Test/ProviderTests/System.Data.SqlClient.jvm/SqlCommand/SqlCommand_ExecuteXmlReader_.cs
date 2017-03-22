using System;
using System.Data;
using System.Data.SqlClient;

using System.Xml;
using System.Text;
using System.IO;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlCommand_ExecuteXmlReader_ : ADONetTesterClass
	{
		public static void Main()
		{
			SqlCommand_ExecuteXmlReader_ tc = new SqlCommand_ExecuteXmlReader_();
			Exception exp = null;
			try
			{
				// Every Test must begin with BeginTest
				tc.BeginTest("SqlCommand_ExecuteXmlReader");

				//testing only on SQLServer
				if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) return ; 

				tc.run();
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				// Every Test must End with EndTest
				tc.EndTest(exp);
			}
		}

		[Test]
		public void run()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer) {
				//All tests in this class are only for MSSQLServer.
				Log(string.Format("All tests in this class are only for MSSQLServer and cannot be tested on {0}", ConnectedDataProvider.GetDbType()));
				return;
			}

			Exception exp = null;

			// Start Sub Test
			try
			{
				// Every Sub Test must begin with BeginCase
				BeginCase("ExecuteXmlReader 1");

				SqlConnection con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient);

				con.Open();
				string selectStr =	"SELECT * FROM Products WHERE PRODUCTID=1 FOR XML AUTO, XMLDATA;" + 
					"SELECT * FROM Orders WHERE ORDERID=1 FOR XML AUTO, XMLDATA;" + 
					"SELECT * FROM Customers WHERE CustomerID like 'A%' FOR XML AUTO, XMLDATA";
			
				SqlCommand comm = new SqlCommand(selectStr,con);
				// ExecuteXmlReader is not supported yet
				XmlReader xr = comm.ExecuteXmlReader();

				StringBuilder sb = new StringBuilder();
				while(xr.Read()) 
				{
					sb.Append(xr.ReadOuterXml());
				}
				// Every Sub Test must have a Compare
				string strXml = null;
				Compare(sb.ToString().Length,4391);
			} 
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				// Every Sub Test must end with EndCase
				EndCase(exp);
				exp = null;
			}
			// End Sub Test
		}


	}
}