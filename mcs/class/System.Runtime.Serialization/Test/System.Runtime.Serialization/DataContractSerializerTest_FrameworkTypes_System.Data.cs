//
// DataContractSerializerTest_FrameworkTypes_System.Data.cs
//
// Author:
//	Igor Zelmanovich <igorz@mainsoft.com>
//
// Copyright (C) 2008 Mainsoft.co http://www.mainsoft.com
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
//
// This test code contains tests for attributes in System.Runtime.Serialization
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;
using System.Globalization;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;

namespace MonoTests.System.Runtime.Serialization
{
	[TestFixture]
	[Category ("NotWorking")]
	public partial class DataContractSerializerTest_FrameworkTypes_System_Data
		: DataContractSerializerTest_FrameworkTypes
	{
		[Test]
		public void System_Data_AcceptRejectRule () {
			Test<global::System.Data.AcceptRejectRule> ();
		}
		[Test]
		public void System_Data_CommandBehavior () {
			Test<global::System.Data.CommandBehavior> ();
		}
		[Test]
		public void System_Data_ConnectionState () {
			Test<global::System.Data.ConnectionState> ();
		}
		[Test]
		public void System_Data_DataException () {
			Test<global::System.Data.DataException> ();
		}
		[Test]
		public void System_Data_ConstraintException () {
			Test<global::System.Data.ConstraintException> ();
		}
		[Test]
		public void System_Data_DeletedRowInaccessibleException () {
			Test<global::System.Data.DeletedRowInaccessibleException> ();
		}
		[Test]
		public void System_Data_DuplicateNameException () {
			Test<global::System.Data.DuplicateNameException> ();
		}
		[Test]
		public void System_Data_InRowChangingEventException () {
			Test<global::System.Data.InRowChangingEventException> ();
		}
		[Test]
		public void System_Data_InvalidConstraintException () {
			Test<global::System.Data.InvalidConstraintException> ();
		}
		[Test]
		public void System_Data_MissingPrimaryKeyException () {
			Test<global::System.Data.MissingPrimaryKeyException> ();
		}
		[Test]
		public void System_Data_NoNullAllowedException () {
			Test<global::System.Data.NoNullAllowedException> ();
		}
		[Test]
		public void System_Data_ReadOnlyException () {
			Test<global::System.Data.ReadOnlyException> ();
		}
		[Test]
		public void System_Data_RowNotInTableException () {
			Test<global::System.Data.RowNotInTableException> ();
		}
		[Test]
		public void System_Data_VersionNotFoundException () {
			Test<global::System.Data.VersionNotFoundException> ();
		}
		[Test]
		public void System_Data_DataRowAction () {
			Test<global::System.Data.DataRowAction> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Data_DataRowState () {
			Test<global::System.Data.DataRowState> ();
		}
		[Test]
		public void System_Data_SerializationFormat () {
			Test<global::System.Data.SerializationFormat> ();
		}
		[Test]
		public void System_Data_DataSet () {
			Test<global::System.Data.DataSet> ();
		}
		[Test]
		public void System_Data_DataViewRowState () {
			Test<global::System.Data.DataViewRowState> ();
		}
		[Test]
		[Category ("NotWorking")]
		public void System_Data_DBConcurrencyException () {
			Test<global::System.Data.DBConcurrencyException> ();
		}
		[Test]
		public void System_Data_DbType () {
			Test<global::System.Data.DbType> ();
		}
		[Test]
		public void System_Data_PropertyCollection () {
			Test<global::System.Data.PropertyCollection> ();
		}
		[Test]
		public void System_Data_Rule () {
			Test<global::System.Data.Rule> ();
		}
		[Test]
		public void System_Data_SqlDbType () {
			Test<global::System.Data.SqlDbType> ();
		}
		[Test]
		public void System_Data_StatementType () {
			Test<global::System.Data.StatementType> ();
		}
		[Test]
		public void System_Data_UpdateRowSource () {
			Test<global::System.Data.UpdateRowSource> ();
		}
		[Test]
		public void System_Data_UpdateStatus () {
			Test<global::System.Data.UpdateStatus> ();
		}
		[Test]
		public void System_Data_XmlReadMode () {
			Test<global::System.Data.XmlReadMode> ();
		}
		[Test]
		public void System_Data_XmlWriteMode () {
			Test<global::System.Data.XmlWriteMode> ();
		}
		[Test]
		public void System_Data_StrongTypingException () {
			Test<global::System.Data.StrongTypingException> ();
		}
		[Test]
		public void System_Data_TypedDataSetGeneratorException () {
			Test<global::System.Data.TypedDataSetGeneratorException> ();
		}
		[Test]
		public void System_Data_KeyRestrictionBehavior () {
			Test<global::System.Data.KeyRestrictionBehavior> ();
		}
		[Test]
		public void System_Data_Common_GroupByBehavior () {
			Test<global::System.Data.Common.GroupByBehavior> ();
		}
		[Test]
		public void System_Data_Common_IdentifierCase () {
			Test<global::System.Data.Common.IdentifierCase> ();
		}
		[Test]
		public void System_Data_Common_SupportedJoinOperators () {
			Test<global::System.Data.Common.SupportedJoinOperators> ();
		}
		[Test]
		public void System_Data_InvalidExpressionException () {
			Test<global::System.Data.InvalidExpressionException> ();
		}
		[Test]
		public void System_Data_EvaluateException () {
			Test<global::System.Data.EvaluateException> ();
		}
		[Test]
		public void System_Data_SyntaxErrorException () {
			Test<global::System.Data.SyntaxErrorException> ();
		}
		[Test]
		public void System_Data_Odbc_OdbcPermission () {
			Test<global::System.Data.Odbc.OdbcPermission> ();
		}
		[Test]
		public void System_Data_OleDb_OleDbLiteral () {
			Test<global::System.Data.OleDb.OleDbLiteral> ();
		}
		[Test]
		public void System_Data_OleDb_OleDbPermission () {
			Test<global::System.Data.OleDb.OleDbPermission> ();
		}
		[Test]
		public void System_Data_OleDb_OleDbType () {
			Test<global::System.Data.OleDb.OleDbType> ();
		}
		[Test]
		public void System_Data_PropertyAttributes () {
			Test<global::System.Data.PropertyAttributes> ();
		}
		//[Test]
		//public void System_Data_SqlClient_SortOrder () {
		//    Test<global::System.Data.SqlClient.SortOrder> ();
		//}
		[Test]
		public void System_Data_SqlClient_SqlBulkCopyOptions () {
			Test<global::System.Data.SqlClient.SqlBulkCopyOptions> ();
		}
		[Test]
		public void System_Data_SqlClient_SqlClientPermission () {
			Test<global::System.Data.SqlClient.SqlClientPermission> ();
		}
		[Test]
		public void System_Data_SqlClient_SqlNotificationInfo () {
			Test<global::System.Data.SqlClient.SqlNotificationInfo> ();
		}
		[Test]
		public void System_Data_SqlClient_SqlNotificationSource () {
			Test<global::System.Data.SqlClient.SqlNotificationSource> ();
		}
		[Test]
		public void System_Data_SqlClient_SqlNotificationType () {
			Test<global::System.Data.SqlClient.SqlNotificationType> ();
		}
	}
}
