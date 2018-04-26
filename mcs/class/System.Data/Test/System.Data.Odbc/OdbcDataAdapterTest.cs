//
// OdbcDataAdapterTest.cs - NUnit Test Cases for testing the
//                        OdbcDataAdapter class
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

#if !NO_ODBC

using System;
using System.Data;
using System.Data.Odbc;
using NUnit.Framework;
using SqlCommand = System.Data.SqlClient.SqlCommand;

namespace MonoTests.System.Data.Odbc
{
	[TestFixture]
	public class OdbcDataAdapterTest
	{
		[Test] // OdbcDataAdapter ()
		public void Constructor1 ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNull (da.SelectCommand, "#11");
			Assert.IsNull (da.Site, "#12");
			Assert.IsNotNull (da.TableMappings, "#13");
			Assert.AreEqual (0, da.TableMappings.Count, "#14");
			Assert.AreEqual (1, da.UpdateBatchSize, "#15");
			Assert.IsNull (da.UpdateCommand, "#16");
		}

		[Test] // OdbcDataAdapter (OdbcCommand)
		public void Constructor2 ()
		{
			OdbcCommand cmd = new OdbcCommand ();
			OdbcDataAdapter da = new OdbcDataAdapter (cmd);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (cmd, da.SelectCommand, "#12");
			Assert.IsNull (da.Site, "#13");
			Assert.IsNotNull (da.TableMappings, "#14");
			Assert.AreEqual (0, da.TableMappings.Count, "#15");
			Assert.AreEqual (1, da.UpdateBatchSize, "#16");
			Assert.IsNull (da.UpdateCommand, "#17");
		}

		[Test] // OdbcDataAdapter (OdbcCommand)
		public void Constructor2_SelectCommand_Null ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ((OdbcCommand) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNull (da.SelectCommand, "#11");
			Assert.IsNull (da.Site, "#12");
			Assert.IsNotNull (da.TableMappings, "#13");
			Assert.AreEqual (0, da.TableMappings.Count, "#14");
			Assert.AreEqual (1, da.UpdateBatchSize, "#15");
			Assert.IsNull (da.UpdateCommand, "#16");
		}

		[Test] // OdbcDataAdapter (string, OdbcCommand)
		public void Constructor3 ()
		{
			string selectCommandText = "SELECT * FROM Authors";
			OdbcConnection selectConnection = new OdbcConnection ();

			OdbcDataAdapter da = new OdbcDataAdapter (selectCommandText,
				selectConnection);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.AreSame (selectConnection, da.SelectCommand.Connection, "#13");
			Assert.IsNull (da.Site, "#14");
			Assert.IsNotNull (da.TableMappings, "#15");
			Assert.AreEqual (0, da.TableMappings.Count, "#16");
			Assert.AreEqual (1, da.UpdateBatchSize, "#17");
			Assert.IsNull (da.UpdateCommand, "#18");
		}

		[Test] // OdbcDataAdapter (string, OdbcConnection)
		public void Constructor3_SelectCommandText_Null ()
		{
			OdbcConnection selectConnection = new OdbcConnection ();

			OdbcDataAdapter da = new OdbcDataAdapter ((string) null,
				selectConnection);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.IsNotNull (da.SelectCommand.CommandText, "#12");
			Assert.AreEqual (string.Empty, da.SelectCommand.CommandText, "#13");
			Assert.AreSame (selectConnection, da.SelectCommand.Connection, "#14");
			Assert.IsNull (da.Site, "#15");
			Assert.IsNotNull (da.TableMappings, "#16");
			Assert.AreEqual (0, da.TableMappings.Count, "#17");
			Assert.AreEqual (1, da.UpdateBatchSize, "#18");
			Assert.IsNull (da.UpdateCommand, "#19");
		}

		[Test] // OdbcDataAdapter (string, OdbcConnection)
		public void Constructor3_SelectConnection_Null ()
		{
			string selectCommandText = "SELECT * FROM Authors";

			OdbcDataAdapter da = new OdbcDataAdapter (selectCommandText,
				(OdbcConnection) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNull (da.SelectCommand.Connection, "#13");
			Assert.IsNull (da.Site, "#14");
			Assert.IsNotNull (da.TableMappings, "#15");
			Assert.AreEqual (0, da.TableMappings.Count, "#16");
			Assert.AreEqual (1, da.UpdateBatchSize, "#17");
			Assert.IsNull (da.UpdateCommand, "#18");
		}

		[Test] // OdbcDataAdapter (string, string)]
		public void Constructor4 ()
		{
			string selectCommandText = "SELECT * FROM Authors";
			string selectConnectionString = "Provider=SQLOLEDB;Data Source=SQLSRV;";

			OdbcDataAdapter da = new OdbcDataAdapter (selectCommandText,
				selectConnectionString);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNotNull (da.SelectCommand.Connection, "#13");
			Assert.AreEqual (selectConnectionString, da.SelectCommand.Connection.ConnectionString, "#14");
			Assert.IsNull (da.Site, "#15");
			Assert.IsNotNull (da.TableMappings, "#16");
			Assert.AreEqual (0, da.TableMappings.Count, "#17");
			Assert.AreEqual (1, da.UpdateBatchSize, "#18");
			Assert.IsNull (da.UpdateCommand, "#19");
		}

		[Test] // OdbcDataAdapter (string, string)]
		public void Constructor4_SelectCommandText_Null ()
		{
			string selectConnectionString = "Provider=SQLOLEDB;Data Source=SQLSRV;";

			OdbcDataAdapter da = new OdbcDataAdapter ((string) null,
				selectConnectionString);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.IsNotNull (da.SelectCommand.CommandText, "#12");
			Assert.AreEqual (string.Empty, da.SelectCommand.CommandText, "#13");
			Assert.IsNotNull (da.SelectCommand.Connection, "#14");
			Assert.AreEqual (selectConnectionString, da.SelectCommand.Connection.ConnectionString, "#15");
			Assert.IsNull (da.Site, "#16");
			Assert.IsNotNull (da.TableMappings, "#17");
			Assert.AreEqual (0, da.TableMappings.Count, "#18");
			Assert.AreEqual (1, da.UpdateBatchSize, "#19");
			Assert.IsNull (da.UpdateCommand, "#20");
		}

		[Test] // OdbcDataAdapter (string, string)]
		public void Constructor4_SelectConnectionString_Null ()
		{
			string selectCommandText = "SELECT * FROM Authors";

			OdbcDataAdapter da = new OdbcDataAdapter (selectCommandText,
				(string) null);
			Assert.IsTrue (da.AcceptChangesDuringFill, "#1");
			Assert.IsTrue (da.AcceptChangesDuringUpdate, "#2");
			Assert.IsNull (da.Container, "#3");
			Assert.IsFalse (da.ContinueUpdateOnError, "#4");
			Assert.IsNull (da.DeleteCommand, "#5");
			Assert.AreEqual (LoadOption.OverwriteChanges, da.FillLoadOption, "#6");
			Assert.IsNull (da.InsertCommand, "#7");
			Assert.AreEqual (MissingMappingAction.Passthrough, da.MissingMappingAction, "#8");
			Assert.AreEqual (MissingSchemaAction.Add, da.MissingSchemaAction, "#9");
			Assert.IsFalse (da.ReturnProviderSpecificTypes, "#10");
			Assert.IsNotNull (da.SelectCommand, "#11");
			Assert.AreSame (selectCommandText, da.SelectCommand.CommandText, "#12");
			Assert.IsNotNull (da.SelectCommand.Connection, "#14");
			Assert.AreEqual (string.Empty, da.SelectCommand.Connection.ConnectionString, "#15");
			Assert.IsNull (da.Site, "#16");
			Assert.IsNotNull (da.TableMappings, "#17");
			Assert.AreEqual (0, da.TableMappings.Count, "#18");
			Assert.AreEqual (1, da.UpdateBatchSize, "#19");
			Assert.IsNull (da.UpdateCommand, "#20");
		}

		[Test]
		public void DeleteCommand ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.DeleteCommand = cmd1;
			Assert.AreSame (cmd1, da.DeleteCommand, "#1");
			da.DeleteCommand = cmd2;
			Assert.AreSame (cmd2, da.DeleteCommand, "#2");
			da.DeleteCommand = null;
			Assert.IsNull (da.DeleteCommand, "#3");
		}

		[Test]
		public void DeleteCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.DeleteCommand = cmd1;
			Assert.AreSame (cmd1, da.DeleteCommand, "#A1");
			da.DeleteCommand = cmd2;
			Assert.AreSame (cmd2, da.DeleteCommand, "#A2");
			da.DeleteCommand = null;
			Assert.IsNull (da.DeleteCommand, "#A3");

			try {
				da.DeleteCommand = new SqlCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void Dispose ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			da.DeleteCommand = new OdbcCommand ();
			da.InsertCommand = new OdbcCommand ();
			da.SelectCommand = new OdbcCommand ();
			da.UpdateCommand = new OdbcCommand ();
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
			OdbcDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.InsertCommand = cmd1;
			Assert.AreSame (cmd1, da.InsertCommand, "#1");
			da.InsertCommand = cmd2;
			Assert.AreSame (cmd2, da.InsertCommand, "#2");
			da.InsertCommand = null;
			Assert.IsNull (da.InsertCommand, "#3");
		}

		[Test]
		public void InsertCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.InsertCommand = cmd1;
			Assert.AreSame (cmd1, da.InsertCommand, "#A1");
			da.InsertCommand = cmd2;
			Assert.AreSame (cmd2, da.InsertCommand, "#A2");
			da.InsertCommand = null;
			Assert.IsNull (da.InsertCommand, "#A3");

			try {
				da.InsertCommand = new SqlCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void SelectCommand ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.SelectCommand = cmd1;
			Assert.AreSame (cmd1, da.SelectCommand, "#1");
			da.SelectCommand = cmd2;
			Assert.AreSame (cmd2, da.SelectCommand, "#2");
			da.SelectCommand = null;
			Assert.IsNull (da.SelectCommand, "#3");
		}

		[Test]
		public void SelectCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.SelectCommand = cmd1;
			Assert.AreSame (cmd1, da.SelectCommand, "#A1");
			da.SelectCommand = cmd2;
			Assert.AreSame (cmd2, da.SelectCommand, "#A2");
			da.SelectCommand = null;
			Assert.IsNull (da.SelectCommand, "#A3");

			try {
				da.SelectCommand = new SqlCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test]
		public void UpdateBatchSize ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			da.UpdateBatchSize = 1;
			Assert.AreEqual (1, da.UpdateBatchSize, "#A");

			try {
				da.UpdateBatchSize = -2;
				Assert.Fail ("#B1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}

			try {
				da.UpdateBatchSize = 0;
				Assert.Fail ("#C1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			try {
				da.UpdateBatchSize = -1;
				Assert.Fail ("#D1");
			} catch (NotSupportedException ex) {
				// Specified method is not supported
				Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
			}
		}

		[Test]
		public void UpdateCommand ()
		{
			OdbcDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.UpdateCommand = cmd1;
			Assert.AreSame (cmd1, da.UpdateCommand, "#1");
			da.UpdateCommand = cmd2;
			Assert.AreSame (cmd2, da.UpdateCommand, "#2");
			da.UpdateCommand = null;
			Assert.IsNull (da.UpdateCommand, "#3");
		}

		[Test]
		public void UpdateCommand_IDbDataAdapter ()
		{
			IDbDataAdapter da = new OdbcDataAdapter ();
			OdbcCommand cmd1 = new OdbcCommand ();
			OdbcCommand cmd2 = new OdbcCommand ();

			da.UpdateCommand = cmd1;
			Assert.AreSame (cmd1, da.UpdateCommand, "#A1");
			da.UpdateCommand = cmd2;
			Assert.AreSame (cmd2, da.UpdateCommand, "#A2");
			da.UpdateCommand = null;
			Assert.IsNull (da.UpdateCommand, "#A3");

			try {
				da.UpdateCommand = new SqlCommand ();
				Assert.Fail ("#B1");
			} catch (InvalidCastException ex) {
				Assert.AreEqual (typeof (InvalidCastException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}
	}
}

#endif