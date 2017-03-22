using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlDataAdapter_RowUpdating : ADONetTesterClass
	{
		public static void Main()
		{
			SqlDataAdapter_RowUpdating tc = new SqlDataAdapter_RowUpdating();
			Exception exp = null;
			try
			{
				tc.BeginTest("SqlDataAdapter_RowUpdating");
				tc.run();
			}
			catch(Exception ex)
			{
				exp = ex;
			}
			finally
			{
				tc.EndTest(exp);
			}
		}


		//public TestClass():base(true){}

		//Activate this constructor to log Failures to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, false){}


		//Activate this constructor to log All to a log file
		//public TestClass(System.IO.TextWriter tw):base(tw, true){}

		//BY DEFAULT LOGGING IS DONE TO THE STANDARD OUTPUT ONLY FOR FAILURES

		int EventCounter = 0;
		DataRow drInsert,drDelete,drUpdate;
		[Test] 
		public void run()
		{
			if (ConnectedDataProvider.GetDbType() != DataBaseServer.SQLServer)
			{
				Log("Test \"SqlDataAdapter_RowUpdated\" skipped: [Test applies only to sql server]");
				return;
			}

			Exception exp = null;

			SqlDataAdapter  sqlDa = new SqlDataAdapter();
			SqlConnection con = new SqlConnection(ConnectedDataProvider.ConnectionStringSQLClient); 

			sqlDa.SelectCommand = new SqlCommand("",con);

			base.SqlDataAdapter_BuildUpdateCommands(ref sqlDa);		
			// --------- get data from DB -----------------

			DataSet ds = base.PrepareDBData_Update((DbDataAdapter)sqlDa,true);


			// add event handler
			sqlDa.RowUpdating+=new SqlRowUpdatingEventHandler(sqlDa_RowUpdating);
			
			
			
		
				
			//insert ,delete, update
			drInsert = ds.Tables[0].NewRow();
			drInsert.ItemArray = new object[] {9991,"Ofer","Borshtein","Insert"};
			drDelete = ds.Tables[0].Rows.Find(9992);
			drUpdate = ds.Tables[0].Rows.Find(9993);
			
			ds.Tables[0].Rows.Add(drInsert);
			drDelete.Delete();
			drUpdate["Title"] = "Jack the ripper"; 

			//execute update to db, will raise events
			sqlDa.Update(ds);

			try
			{
				BeginCase("EventCounter ");
				Compare(EventCounter ,3);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
			
			sqlDa.RowUpdating-= new SqlRowUpdatingEventHandler(sqlDa_RowUpdating);
			
			//close connection
			if (  ((IDbDataAdapter)sqlDa).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)sqlDa).SelectCommand.Connection.Close();
		}


		private void sqlDa_RowUpdating(object sender, SqlRowUpdatingEventArgs e)
		{
			Exception exp = null;
			switch (e.StatementType)
			{
				case StatementType.Insert: 
					try
					{
						BeginCase("RowInsert");
						Compare(drInsert ,e.Row );
					}
					catch(Exception ex)	{exp = ex;}
					finally	{EndCase(exp); exp = null;}
					EventCounter++;
					break;
				case StatementType.Delete:
					try
					{
						BeginCase("RowDelete");
						Compare(drDelete ,e.Row );
					}
					catch(Exception ex)	{exp = ex;}
					finally	{EndCase(exp); exp = null;}
					EventCounter++;
					break;
				case StatementType.Update:
					try
					{
						BeginCase("RowUpdate");
						Compare(drUpdate ,e.Row );
					}
					catch(Exception ex)	{exp = ex;}
					finally	{EndCase(exp); exp = null;}
					EventCounter++;
					break;
			}

		}
	}
}