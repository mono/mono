//
// OracleDataAdapterTest.cs - NUnit Test Cases for OracleDataAdapter
//
// Author:
//      Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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
using System.Data.SqlClient;

using NUnit.Framework;

namespace MonoTests.System.Data.OracleClient
{
	[TestFixture]
	public class OracleDataAdapterTest
	{
		[Test] // ctor ()
		public void Constructor1 ()
		{
			OracleDataAdapter da = new OracleDataAdapter ();
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

		[Test] // ctor (OracleCommand)
		public void Constructor2 ()
		{
			OracleCommand cmd = new OracleCommand ();
			OracleDataAdapter da = new OracleDataAdapter (cmd);
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

		[Test] // ctor (OracleCommand)
		public void Constructor2_SelectCommand_Null ()
		{
			OracleDataAdapter da = new OracleDataAdapter (
				(OracleCommand) null);
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

		[Test] // ctor (String, OracleConnection)
		public void Constructor3 ()
		{
			string selectCommandText = "SELECT * FROM dual";
			OracleConnection selectConnection = new OracleConnection ();

			OracleDataAdapter da = new OracleDataAdapter (
				selectCommandText, selectConnection);
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

		[Test] // ctor (String, OracleConnection)
		public void Constructor3_SelectCommandText_Null ()
		{
			OracleConnection selectConnection = new OracleConnection ();

			OracleDataAdapter da = new OracleDataAdapter (
				(string) null, selectConnection);
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

		[Test] // ctor (String, OracleConnection)
		public void Constructor3_SelectConnection_Null ()
		{
			string selectCommandText = "SELECT * FROM dual";

			OracleDataAdapter da = new OracleDataAdapter (
				selectCommandText, (OracleConnection) null);
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

		[Test] // ctor (String, String)]
		public void Constructor4 ()
		{
			string selectCommandText = "SELECT * FROM dual";
			string selectConnectionString = "Data Source=Oracle8i;Integrated Security=yes";

			OracleDataAdapter da = new OracleDataAdapter (
				selectCommandText, selectConnectionString);
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

		[Test] // ctor (String, String)]
		public void Constructor4_SelectCommandText_Null ()
		{
			string selectConnectionString = "Data Source=Oracle8i;Integrated Security=yes";

			OracleDataAdapter da = new OracleDataAdapter (
				(string) null, selectConnectionString);
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

		[Test] // ctor (String, String)]
		public void Constructor4_SelectConnectionString_Null ()
		{
			string selectCommandText = "SELECT * FROM dual";

			OracleDataAdapter da = new OracleDataAdapter (
				selectCommandText, (string) null);
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
			OracleDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			IDbDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			OracleDataAdapter da = new OracleDataAdapter ();
			da.DeleteCommand = new OracleCommand ();
			da.InsertCommand = new OracleCommand ();
			da.SelectCommand = new OracleCommand ();
			da.UpdateCommand = new OracleCommand ();
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
			OracleDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			IDbDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			OracleDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			IDbDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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

#if NET_2_0
		[Test]
		public void UpdateBatchSize ()
		{
			OracleDataAdapter da = new OracleDataAdapter ();
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
			OracleDataAdapter da = new OracleDataAdapter ();
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
#endif

		[Test]
		public void UpdateCommand ()
		{
			OracleDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
			IDbDataAdapter da = new OracleDataAdapter ();
			OracleCommand cmd1 = new OracleCommand ();
			OracleCommand cmd2 = new OracleCommand ();

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
