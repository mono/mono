//
// SqlDataAdapterTest.cs - NUnit Test Cases for testing the
//                        SqlDataAdapter class
// Author:
//      Gert Driesen (drieseng@users.sourceforge.net)
//
// Copyright (c) 2007 Gert Driesen
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
using System.Data.SqlClient;
#if !MOBILE
using System.Data.Odbc;
#endif

using NUnit.Framework;

namespace MonoTests.System.Data.SqlClient
{
	[TestFixture]
	public class SqlDataAdapterTest
	{
		[Test] // SqlDataAdapter ()
		public void Constructor1 ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNull (da.SelectCommand, "#11");
			Assert.IsNull (da.Site, "#12");
			Assert.IsNotNull (da.TableMappings, "#13");
			Assert.AreEqual (0, da.TableMappings.Count, "#14");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#15");
#endif
			Assert.IsNull (da.UpdateCommand, "#16");
		}

		[Test] // SqlDataAdapter (SqlCommand)
		public void Constructor2 ()
		{
			SqlCommand cmd = new SqlCommand ();
			SqlDataAdapter da = new SqlDataAdapter (cmd);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (cmd, da.SelectCommand, "#12");
			Assert.IsNull (da.Site, "#13");
			Assert.IsNotNull (da.TableMappings, "#14");
			Assert.AreEqual (0, da.TableMappings.Count, "#15");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#16");
#endif
			Assert.IsNull (da.UpdateCommand, "#17");
		}

		[Test] // SqlDataAdapter (SqlCommand)
		public void Constructor2_SelectCommand_Null ()
		{
			SqlDataAdapter da = new SqlDataAdapter ((SqlCommand) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNull (da.SelectCommand, "#11");
			Assert.IsNull (da.Site, "#12");
			Assert.IsNotNull (da.TableMappings, "#13");
			Assert.AreEqual (0, da.TableMappings.Count, "#14");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#15");
#endif
			Assert.IsNull (da.UpdateCommand, "#16");
		}

		[Test] // SqlDataAdapter (string, SqlConnection)
		public void Constructor3 ()
		{
			string selectCommandText = "SELECT * FROM Authors";
			SqlConnection selectConnection = new SqlConnection ();

			SqlDataAdapter da = new SqlDataAdapter (selectCommandText,
				selectConnection);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.AreSame (selectConnection, da.SelectCommand.Connection, "#13");
			Assert.IsNull (da.Site, "#14");
			Assert.IsNotNull (da.TableMappings, "#15");
			Assert.AreEqual (0, da.TableMappings.Count, "#16");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#17");
#endif
			Assert.IsNull (da.UpdateCommand, "#18");
		}

		[Test] // SqlDataAdapter (string, SqlConnection)
		public void Constructor3_SelectCommandText_Null ()
		{
			SqlConnection selectConnection = new SqlConnection ();

			SqlDataAdapter da = new SqlDataAdapter ((string) null,
				selectConnection);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.IsNotNull (da.SelectCommand.CommandText, "#12");
			Assert.AreEqual (string.Empty, da.SelectCommand.CommandText, "#13");
			Assert.AreSame (selectConnection, da.SelectCommand.Connection, "#14");
			Assert.IsNull (da.Site, "#15");
			Assert.IsNotNull (da.TableMappings, "#16");
			Assert.AreEqual (0, da.TableMappings.Count, "#17");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#18");
#endif
			Assert.IsNull (da.UpdateCommand, "#19");
		}

		[Test] // SqlDataAdapter (string, SqlConnection)
		public void Constructor3_SelectConnection_Null ()
		{
			string selectCommandText = "SELECT * FROM Authors";

			SqlDataAdapter da = new SqlDataAdapter (selectCommandText,
				(SqlConnection) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNull (da.SelectCommand.Connection, "#13");
			Assert.IsNull (da.Site, "#14");
			Assert.IsNotNull (da.TableMappings, "#15");
			Assert.AreEqual (0, da.TableMappings.Count, "#16");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#17");
#endif
			Assert.IsNull (da.UpdateCommand, "#18");
		}

		[Test] // SqlDataAdapter (string, string)]
		public void Constructor4 ()
		{
			string selectCommandText = "SELECT * FROM Authors";
			string selectConnectionString = "server=SQLSRV;database=Mono";

			SqlDataAdapter da = new SqlDataAdapter (selectCommandText,
				selectConnectionString);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNotNull (da.SelectCommand.Connection, "#13");
			Assert.AreEqual (selectConnectionString, da.SelectCommand.Connection.ConnectionString, "#14");
			Assert.IsNull (da.Site, "#15");
			Assert.IsNotNull (da.TableMappings, "#16");
			Assert.AreEqual (0, da.TableMappings.Count, "#17");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#18");
#endif
			Assert.IsNull (da.UpdateCommand, "#19");
		}

		[Test] // SqlDataAdapter (string, string)]
		public void Constructor4_SelectCommandText_Null ()
		{
			string selectConnectionString = "server=SQLSRV;database=Mono";

			SqlDataAdapter da = new SqlDataAdapter ((string) null,
				selectConnectionString);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.IsNotNull (da.SelectCommand.CommandText, "#12");
			Assert.AreEqual (string.Empty, da.SelectCommand.CommandText, "#13");
			Assert.IsNotNull (da.SelectCommand.Connection, "#14");
			Assert.AreEqual (selectConnectionString, da.SelectCommand.Connection.ConnectionString, "#15");
			Assert.IsNull (da.Site, "#16");
			Assert.IsNotNull (da.TableMappings, "#17");
			Assert.AreEqual (0, da.TableMappings.Count, "#18");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#19");
#endif
			Assert.IsNull (da.UpdateCommand, "#20");
		}

		[Test] // SqlDataAdapter (string, string)]
		public void Constructor4_SelectConnectionString_Null ()
		{
			string selectCommandText = "SELECT * FROM Authors";

			SqlDataAdapter da = new SqlDataAdapter (selectCommandText,
				(string) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
#if NET_2_0
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
#endif
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
#if NET_2_0
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
#endif
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
#if NET_2_0
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
#endif
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNotNull (da.SelectCommand.Connection, "#14");
			Assert.AreEqual (string.Empty, da.SelectCommand.Connection.ConnectionString, "#15");
			Assert.IsNull (da.Site, "#16");
			Assert.IsNotNull (da.TableMappings, "#17");
			Assert.AreEqual (0, da.TableMappings.Count, "#18");
#if NET_2_0
			Assert.AreEqual (1, da.UpdateBatchSize, "#19");
#endif
			Assert.IsNull (da.UpdateCommand, "#20");
		}

		[Test]
		public void DeleteCommand ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();

			da.DeleteCommand = cmd1;
			Assert.AreSame (cmd1, da.DeleteCommand, "#1");
			da.DeleteCommand = cmd2;
			Assert.AreSame (cmd2, da.DeleteCommand, "#2");
			da.DeleteCommand = null;
			Assert.IsNull (da.DeleteCommand, "#3");
		}

		[Test]
		public void Dispose ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			da.DeleteCommand = new SqlCommand ();
			da.InsertCommand = new SqlCommand ();
			da.SelectCommand = new SqlCommand ();
			da.UpdateCommand = new SqlCommand ();
			da.Dispose ();

			Assert.IsNull (da.DeleteCommand, "#1");
			Assert.IsNull (da.InsertCommand, "#2");
			Assert.IsNull (da.SelectCommand, "#3");
			Assert.IsNotNull (da.TableMappings, "#4");
			Assert.AreEqual (0, da.TableMappings.Count, "#5");
			Assert.IsNull (da.UpdateCommand, "#6");
		}

		[Test]
		public void InsertCommand ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();

			da.InsertCommand = cmd1;
			Assert.AreSame (cmd1, da.InsertCommand, "#1");
			da.InsertCommand = cmd2;
			Assert.AreSame (cmd2, da.InsertCommand, "#2");
			da.InsertCommand = null;
			Assert.IsNull (da.InsertCommand, "#3");
		}

		[Test]
		public void SelectCommand ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();

			da.SelectCommand = cmd1;
			Assert.AreSame (cmd1, da.SelectCommand, "#1");
			da.SelectCommand = cmd2;
			Assert.AreSame (cmd2, da.SelectCommand, "#2");
			da.SelectCommand = null;
			Assert.IsNull (da.SelectCommand, "#3");
		}

		[Test]
		public void UpdateBatchSize ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			da.UpdateBatchSize = 0;
			Assert.AreEqual (0, da.UpdateBatchSize, "#1");
			da.UpdateBatchSize = int.MaxValue;
			Assert.AreEqual (int.MaxValue, da.UpdateBatchSize, "#2");
			da.UpdateBatchSize = 1;
			Assert.AreEqual (1, da.UpdateBatchSize, "#3");
		}
		
		[Test]
		public void UpdateBatchSize_Negative ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			try {
				da.UpdateBatchSize = -1;
				Assert.Fail ("#1");
			} catch (ArgumentOutOfRangeException ex) {
				Assert.AreEqual (typeof (ArgumentOutOfRangeException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("UpdateBatchSize", ex.ParamName, "#6");
			}
		}

		[Test]
		public void UpdateCommand ()
		{
			SqlDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();

			da.UpdateCommand = cmd1;
			Assert.AreSame (cmd1, da.UpdateCommand, "#1");
			da.UpdateCommand = cmd2;
			Assert.AreSame (cmd2, da.UpdateCommand, "#2");
			da.UpdateCommand = null;
			Assert.IsNull (da.UpdateCommand, "#3");
		}

#if !MOBILE
		[Test]
		public void DeleteCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();
			
			da.DeleteCommand = cmd1;
			Assert.AreSame (cmd1, da.DeleteCommand, "#A1");
			da.DeleteCommand = cmd2;
			Assert.AreSame (cmd2, da.DeleteCommand, "#A2");
			da.DeleteCommand = null;
			Assert.IsNull (da.DeleteCommand, "#A3");
			
			try {
				da.DeleteCommand = new OdbcCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		
		[Test]
		public void InsertCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();
			
			da.InsertCommand = cmd1;
			Assert.AreSame (cmd1, da.InsertCommand, "#A1");
			da.InsertCommand = cmd2;
			Assert.AreSame (cmd2, da.InsertCommand, "#A2");
			da.InsertCommand = null;
			Assert.IsNull (da.InsertCommand, "#A3");
			
			try {
				da.InsertCommand = new OdbcCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void SelectCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();
			
			da.SelectCommand = cmd1;
			Assert.AreSame (cmd1, da.SelectCommand, "#A1");
			da.SelectCommand = cmd2;
			Assert.AreSame (cmd2, da.SelectCommand, "#A2");
			da.SelectCommand = null;
			Assert.IsNull (da.SelectCommand, "#A3");
			
			try {
				da.SelectCommand = new OdbcCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void UpdateCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new SqlDataAdapter ();
			SqlCommand cmd1 = new SqlCommand ();
			SqlCommand cmd2 = new SqlCommand ();

			da.UpdateCommand = cmd1;
			Assert.AreSame (cmd1, da.UpdateCommand, "#A1");
			da.UpdateCommand = cmd2;
			Assert.AreSame (cmd2, da.UpdateCommand, "#A2");
			da.UpdateCommand = null;
			Assert.IsNull (da.UpdateCommand, "#A3");

			try {
				da.UpdateCommand = new OdbcCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}
#endif
	}
}
