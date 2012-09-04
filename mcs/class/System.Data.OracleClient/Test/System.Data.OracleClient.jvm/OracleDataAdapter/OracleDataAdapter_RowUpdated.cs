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
using System.Data.Common;
using System.Data.OracleClient;

using MonoTests.System.Data.Utils;


using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	[Category ("NotWorking")]
	public class OracleDataAdapter_RowUpdated : ADONetTesterClass
	{
		public static void Main()
		{
			OracleDataAdapter_RowUpdated tc = new OracleDataAdapter_RowUpdated();
			Exception exp = null;
			try
			{
				tc.BeginTest("OracleDataAdapter_RowUpdated");
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
			Exception exp = null;

			OracleDataAdapter oleDBda = new OracleDataAdapter();
			oleDBda.SelectCommand = new OracleCommand("",new OracleConnection());

			base.OracleDataAdapter_BuildUpdateCommands(ref oleDBda);		
			// --------- get data from DB -----------------
			DataSet ds = base.PrepareDBData_Update((DbDataAdapter)oleDBda);


			// add event handler
			oleDBda.RowUpdated += new OracleRowUpdatedEventHandler(oleDBda_RowUpdated);
			
			//insert ,delete, update
			drInsert = ds.Tables[0].NewRow();
			drInsert.ItemArray = new object[] {9991,"Ofer","Borshtein","Insert"};
			drDelete = ds.Tables[0].Rows.Find(9992);
			drUpdate = ds.Tables[0].Rows.Find(9993);
		
			ds.Tables[0].Rows.Add(drInsert);
			drDelete.Delete();
			drUpdate["Title"] = "Jack the ripper"; 

			//execute update to db, will raise events
			oleDBda.Update(ds);

			try
			{
				BeginCase("EventCounter ");
				Compare(EventCounter ,3);
			}
			catch(Exception ex)	{exp = ex;}
			finally	{EndCase(exp); exp = null;}
		
			oleDBda.RowUpdated -= new OracleRowUpdatedEventHandler(oleDBda_RowUpdated);
		
			//close connection
			if (  ((IDbDataAdapter)oleDBda).SelectCommand.Connection.State != ConnectionState.Closed )
				((IDbDataAdapter)oleDBda).SelectCommand.Connection.Close();
		}

		private void oleDBda_RowUpdated(object sender, OracleRowUpdatedEventArgs e)
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