using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using MonoTests.System.Data.Utils;

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlDataAdapter_RowUpdated : ADONetTesterClass
	{
		public static void Main()
		{
			SqlDataAdapter_RowUpdated tc = new SqlDataAdapter_RowUpdated();
			Exception exp = null;
			try
			{
				tc.BeginTest("SqlDataAdapter_RowUpdated");
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
			sqlDa.RowUpdated+=new SqlRowUpdatedEventHandler(sqlDa_RowUpdated);
			
		
				
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
			
			sqlDa.RowUpdated-= new SqlRowUpdatedEventHandler(sqlDa_RowUpdated);
			
			//close connection
			if (  ((IDbDataAdapter)sqlDa).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)sqlDa).SelectCommand.Connection.Close();
		}


		private void sqlDa_RowUpdated(object sender, SqlRowUpdatedEventArgs e)
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