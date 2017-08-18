// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#pragma warning disable 108, 114, 67

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.EnterpriseServices;
using System.Transactions;

namespace System.Data.OleDb
{
	[MonoTODO("OleDb is not implemented.")]
	public class OleDbCommand //: System.Data.Common.DbCommand, System.Data.IDbCommand, System.ICloneable, System.IDisposable
	{
		public String CommandText { get; set; }
		public Int32 CommandTimeout { get; set; }
		public CommandType CommandType { get; set; }
		public OleDbConnection Connection { get; set; }
		public DbConnection DbConnection { get; set; }
		public DbParameterCollection DbParameterCollection { get; set; }
		public DbTransaction DbTransaction { get; set; }
		public Boolean DesignTimeVisible { get; set; }
		public OleDbParameterCollection Parameters { get; set; }
		public OleDbTransaction Transaction { get; set; }
		public UpdateRowSource UpdatedRowSource { get; set; }
		public OleDbCommand () {}
		public OleDbCommand (String cmdText) => throw ADP.OleDb();
		public OleDbCommand (String cmdText, OleDbConnection connection) => throw ADP.OleDb();
		public OleDbCommand (String cmdText, OleDbConnection connection, OleDbTransaction transaction) => throw ADP.OleDb();
		public void Cancel () => throw ADP.OleDb();
		public OleDbCommand Clone () => throw ADP.OleDb();
		public DbParameter CreateDbParameter () => throw ADP.OleDb();
		public OleDbParameter CreateParameter () => throw ADP.OleDb();
		public void Dispose (Boolean disposing) => throw ADP.OleDb();
		public DbDataReader ExecuteDbDataReader (CommandBehavior behavior) => throw ADP.OleDb();
		public Int32 ExecuteNonQuery () => throw ADP.OleDb();
		public OleDbDataReader ExecuteReader () => throw ADP.OleDb();
		public OleDbDataReader ExecuteReader (CommandBehavior behavior) => throw ADP.OleDb();
		public Object ExecuteScalar () => throw ADP.OleDb();
		public void Prepare () => throw ADP.OleDb();
		public void ResetCommandTimeout () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbCommandBuilder : System.Data.Common.DbCommandBuilder
	{
		public OleDbCommandBuilder() => throw ADP.OleDb();
		public OleDbCommandBuilder (OleDbDataAdapter adapter) => throw ADP.OleDb();
		public new OleDbDataAdapter DataAdapter { get; set; }
		protected override void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, Boolean whereClause) => throw ADP.OleDb();
		public void DeriveParameters (OleDbCommand command) => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand () => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand () => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		protected override String GetParameterName (Int32 parameterOrdinal) => throw ADP.OleDb();
		protected override String GetParameterName (String parameterName) => throw ADP.OleDb();
		protected override String GetParameterPlaceholder (Int32 parameterOrdinal) => throw ADP.OleDb();
		public new OleDbCommand GetUpdateCommand () => throw ADP.OleDb();
		public new OleDbCommand GetUpdateCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public String QuoteIdentifier (String unquotedIdentifier) => throw ADP.OleDb();
		public String QuoteIdentifier (String unquotedIdentifier, OleDbConnection connection) => throw ADP.OleDb();
		protected override void SetRowUpdatingHandler (DbDataAdapter adapter) => throw ADP.OleDb();
		public override String UnquoteIdentifier (String quotedIdentifier) => throw ADP.OleDb();
		public String UnquoteIdentifier (String quotedIdentifier, OleDbConnection connection) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbConnection
	{
		public String ConnectionString { get; set; }
		public Int32 ConnectionTimeout { get; set; }
		public String Database { get; set; }
		public String DataSource { get; set; }
		public String Provider { get; set; }
		public String ServerVersion { get; set; }
		public ConnectionState State { get; set; }
		public OleDbConnection (String connectionString) => throw ADP.OleDb();
		public DbTransaction BeginDbTransaction (IsolationLevel isolationLevel) => throw ADP.OleDb();
		public OleDbTransaction BeginTransaction () => throw ADP.OleDb();
		public OleDbTransaction BeginTransaction (IsolationLevel isolationLevel) => throw ADP.OleDb();
		public void ChangeDatabase (String value) => throw ADP.OleDb();
		public void Close () => throw ADP.OleDb();
		public OleDbCommand CreateCommand () => throw ADP.OleDb();
		public DbCommand CreateDbCommand () => throw ADP.OleDb();
		public void Dispose (Boolean disposing) => throw ADP.OleDb();
		public void EnlistDistributedTransaction (ITransaction transaction) => throw ADP.OleDb();
		public void EnlistTransaction (Transaction transaction) => throw ADP.OleDb();
		public DataTable GetOleDbSchemaTable (Guid schema, Object[] restrictions) => throw ADP.OleDb();
		public DataTable GetSchema () => throw ADP.OleDb();
		public DataTable GetSchema (String collectionName) => throw ADP.OleDb();
		public DataTable GetSchema (String collectionName, String[] restrictionValues) => throw ADP.OleDb();
		public void Open () => throw ADP.OleDb();
		public void ReleaseObjectPool () => throw ADP.OleDb();
		public void ResetState () => throw ADP.OleDb();
		public event OleDbInfoMessageEventHandler InfoMessage;
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbConnectionStringBuilder
	{
		public String DataSource { get; set; }
		public String FileName { get; set; }
		public Object Item { get; set; }
		public ICollection Keys { get; set; }
		public Int32 OleDbServices { get; set; }
		public Boolean PersistSecurityInfo { get; set; }
		public String Provider { get; set; }
		public OleDbConnectionStringBuilder (String connectionString) => throw ADP.OleDb();
		public void Clear () => throw ADP.OleDb();
		public Boolean ContainsKey (String keyword) => throw ADP.OleDb();
		public void GetProperties (Hashtable propertyDescriptors) => throw ADP.OleDb();
		public Boolean Remove (String keyword) => throw ADP.OleDb();
		public Boolean TryGetValue (String keyword, Object value) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbDataAdapter
	{
		public OleDbCommand DeleteCommand { get; set; }
		public OleDbCommand InsertCommand { get; set; }
		public OleDbCommand SelectCommand { get; set; }
		public OleDbCommand UpdateCommand { get; set; }
		public OleDbDataAdapter (OleDbCommand selectCommand) => throw ADP.OleDb();
		public OleDbDataAdapter (String selectCommandText, OleDbConnection selectConnection) => throw ADP.OleDb();
		public OleDbDataAdapter (String selectCommandText, String selectConnectionString) => throw ADP.OleDb();
		public RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
		public RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
		public Int32 Fill (DataSet dataSet, Object ADODBRecordSet, String srcTable) => throw ADP.OleDb();
		public Int32 Fill (DataTable dataTable, Object ADODBRecordSet) => throw ADP.OleDb();
		public void OnRowUpdated (RowUpdatedEventArgs value) => throw ADP.OleDb();
		public void OnRowUpdating (RowUpdatingEventArgs value) => throw ADP.OleDb();
		public event OleDbRowUpdatedEventHandler RowUpdated;
		public event OleDbRowUpdatingEventHandler RowUpdating;
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbDataReader// : System.Data.Common.DbDataReader
	{
		public Int32 Depth { get; set; }
		public Int32 FieldCount { get; set; }
		public Boolean HasRows { get; set; }
		public Boolean IsClosed { get; set; }
		public Object Item { get; set; }
		public Int32 RecordsAffected { get; set; }
		public Int32 VisibleFieldCount { get; set; }
		public void Close () => throw ADP.OleDb();
		public Boolean GetBoolean (Int32 ordinal) => throw ADP.OleDb();
		public Byte GetByte (Int32 ordinal) => throw ADP.OleDb();
		public Int64 GetBytes (Int32 ordinal, Int64 dataIndex, Byte[] buffer, Int32 bufferIndex, Int32 length) => throw ADP.OleDb();
		public Char GetChar (Int32 ordinal) => throw ADP.OleDb();
		public Int64 GetChars (Int32 ordinal, Int64 dataIndex, Char[] buffer, Int32 bufferIndex, Int32 length) => throw ADP.OleDb();
		public OleDbDataReader GetData (Int32 ordinal) => throw ADP.OleDb();
		public String GetDataTypeName (Int32 index) => throw ADP.OleDb();
		public DateTime GetDateTime (Int32 ordinal) => throw ADP.OleDb();
		public DbDataReader GetDbDataReader (Int32 ordinal) => throw ADP.OleDb();
		public Decimal GetDecimal (Int32 ordinal) => throw ADP.OleDb();
		public Double GetDouble (Int32 ordinal) => throw ADP.OleDb();
		public IEnumerator GetEnumerator () => throw ADP.OleDb();
		public Type GetFieldType (Int32 index) => throw ADP.OleDb();
		public Single GetFloat (Int32 ordinal) => throw ADP.OleDb();
		public Guid GetGuid (Int32 ordinal) => throw ADP.OleDb();
		public Int16 GetInt16 (Int32 ordinal) => throw ADP.OleDb();
		public Int32 GetInt32 (Int32 ordinal) => throw ADP.OleDb();
		public Int64 GetInt64 (Int32 ordinal) => throw ADP.OleDb();
		public String GetName (Int32 index) => throw ADP.OleDb();
		public Int32 GetOrdinal (String name) => throw ADP.OleDb();
		public DataTable GetSchemaTable () => throw ADP.OleDb();
		public String GetString (Int32 ordinal) => throw ADP.OleDb();
		public TimeSpan GetTimeSpan (Int32 ordinal) => throw ADP.OleDb();
		public Object GetValue (Int32 ordinal) => throw ADP.OleDb();
		public Int32 GetValues (Object[] values) => throw ADP.OleDb();
		public Boolean IsDBNull (Int32 ordinal) => throw ADP.OleDb();
		public Boolean NextResult () => throw ADP.OleDb();
		public Boolean Read () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbEnumerator
	{
		public DataTable GetElements () => throw ADP.OleDb();
		public OleDbDataReader GetEnumerator (Type type) => throw ADP.OleDb();
		public OleDbDataReader GetRootEnumerator () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbError
	{
		internal OleDbError() { }
		public String Message => throw ADP.OleDb();
		public Int32 NativeError=> throw ADP.OleDb();
		public String Source => throw ADP.OleDb();
		public String SQLState => throw ADP.OleDb();
		public override String ToString () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbErrorCollection : System.Collections.ICollection, System.Collections.IEnumerable
	{
		internal OleDbErrorCollection() { }
		public Int32 Count => throw ADP.OleDb();
		public void CopyTo (Array array, Int32 index) => throw ADP.OleDb();
		public void CopyTo (OleDbError[] array, Int32 index) => throw ADP.OleDb();
		public IEnumerator GetEnumerator () => throw ADP.OleDb();
		public System.Data.OleDb.OleDbError this[int index] => throw ADP.OleDb();
        bool System.Collections.ICollection.IsSynchronized => throw ADP.OleDb();
        object System.Collections.ICollection.SyncRoot => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbException : System.Data.Common.DbException
    {
		internal OleDbException() { }
		public override int ErrorCode => throw ADP.OleDb();
		public OleDbErrorCollection Errors => throw ADP.OleDb();
		public override void GetObjectData (SerializationInfo si, StreamingContext context) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbFactory : System.Data.Common.DbProviderFactory
	{
		internal OleDbFactory() { }
		public static readonly System.Data.OleDb.OleDbFactory Instance;
		public override System.Data.Common.DbCommand CreateCommand() => throw ADP.OleDb();
		public override System.Data.Common.DbCommandBuilder CreateCommandBuilder() => throw ADP.OleDb();
		public override System.Data.Common.DbConnection CreateConnection() => throw ADP.OleDb();
		public override System.Data.Common.DbConnectionStringBuilder CreateConnectionStringBuilder() => throw ADP.OleDb();
		public override System.Data.Common.DbDataAdapter CreateDataAdapter() => throw ADP.OleDb();
		public override System.Data.Common.DbParameter CreateParameter() => throw ADP.OleDb();
		public override System.Security.CodeAccessPermission CreatePermission(System.Security.Permissions.PermissionState state) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbInfoMessageEventArgs : System.EventArgs
	{
		internal OleDbInfoMessageEventArgs() => throw ADP.OleDb();
		public Int32 ErrorCode => throw ADP.OleDb();
		public OleDbErrorCollection Errors => throw ADP.OleDb();
		public String Message => throw ADP.OleDb();
		public String Source => throw ADP.OleDb();
		public override String ToString () => throw ADP.OleDb();
	}

    public delegate void OleDbInfoMessageEventHandler(object sender, OleDbInfoMessageEventArgs e);

	[MonoTODO("OleDb is not implemented.")]
	public enum OleDbLiteral
	{
		Binary_Literal = 1,
		Catalog_Name = 2,
		Catalog_Separator = 3,
		Char_Literal = 4,
		Column_Alias = 5,
		Column_Name = 6,
		Correlation_Name = 7,
		Cube_Name = 21,
		Cursor_Name = 8,
		Dimension_Name = 22,
		Escape_Percent_Prefix = 9,
		Escape_Percent_Suffix = 29,
		Escape_Underscore_Prefix = 10,
		Escape_Underscore_Suffix = 30,
		Hierarchy_Name = 23,
		Index_Name = 11,
		Invalid = 0,
		Level_Name = 24,
		Like_Percent = 12,
		Like_Underscore = 13,
		Member_Name = 25,
		Procedure_Name = 14,
		Property_Name = 26,
		Quote_Prefix = 15,
		Quote_Suffix = 28,
		Schema_Name = 16,
		Schema_Separator = 27,
		Table_Name = 17,
		Text_Command = 18,
		User_Name = 19,
		View_Name = 20,
	}    

	[MonoTODO("OleDb is not implemented.")]
	public static partial class OleDbMetaDataCollectionNames
	{
		public static readonly string Catalogs;
		public static readonly string Collations;
		public static readonly string Columns;
		public static readonly string Indexes;
		public static readonly string ProcedureColumns;
		public static readonly string ProcedureParameters;
		public static readonly string Procedures;
		public static readonly string Tables;
		public static readonly string Views;
	}        

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbMetaDataColumnNames
	{
		public OleDbMetaDataColumnNames BooleanFalseLiteral;
		public OleDbMetaDataColumnNames BooleanTrueLiteral;
		public OleDbMetaDataColumnNames DateTimeDigits;
		public OleDbMetaDataColumnNames NativeDataType;
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbParameter
	{
		public DbType DbType { get; set; }
		public ParameterDirection Direction { get; set; }
		public Boolean IsNullable { get; set; }
		public Int32 Offset { get; set; }
		public OleDbType OleDbType { get; set; }
		public String ParameterName { get; set; }
		public Byte Precision { get; set; }
		public Byte Scale { get; set; }
		public Int32 Size { get; set; }
		public String SourceColumn { get; set; }
		public Boolean SourceColumnNullMapping { get; set; }
		public DataRowVersion SourceVersion { get; set; }
		public Object Value { get; set; }
		public OleDbParameter (String name, OleDbType dataType) => throw ADP.OleDb();
		public OleDbParameter (String name, OleDbType dataType, Int32 size) => throw ADP.OleDb();
		public OleDbParameter (String parameterName, OleDbType dbType, Int32 size, ParameterDirection direction, Boolean isNullable, Byte precision, Byte scale, String srcColumn, DataRowVersion srcVersion, Object value) => throw ADP.OleDb();
		public OleDbParameter (String parameterName, OleDbType dbType, Int32 size, ParameterDirection direction, Byte precision, Byte scale, String sourceColumn, DataRowVersion sourceVersion, Boolean sourceColumnNullMapping, Object value) => throw ADP.OleDb();
		public OleDbParameter (String name, OleDbType dataType, Int32 size, String srcColumn) => throw ADP.OleDb();
		public OleDbParameter (String name, Object value) => throw ADP.OleDb();
		public void ResetDbType () => throw ADP.OleDb();
		public void ResetOleDbType () => throw ADP.OleDb();
		public String ToString () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbParameterCollection
	{
		public Int32 Count { get; set; }
		public Boolean IsFixedSize { get; set; }
		public Boolean IsReadOnly { get; set; }
		public Boolean IsSynchronized { get; set; }
		public OleDbParameter Item { get; set; }
		public Object SyncRoot { get; set; }
		public OleDbParameter Add (OleDbParameter value) => throw ADP.OleDb();
		public Int32 Add (Object value) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType, Int32 size) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType, Int32 size, String sourceColumn) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, Object value) => throw ADP.OleDb();
		public void AddRange (Array values) => throw ADP.OleDb();
		public void AddRange (OleDbParameter[] values) => throw ADP.OleDb();
		public OleDbParameter AddWithValue (String parameterName, Object value) => throw ADP.OleDb();
		public void Clear () => throw ADP.OleDb();
		public Boolean Contains (OleDbParameter value) => throw ADP.OleDb();
		public Boolean Contains (Object value) => throw ADP.OleDb();
		public Boolean Contains (String value) => throw ADP.OleDb();
		public void CopyTo (Array array, Int32 index) => throw ADP.OleDb();
		public void CopyTo (OleDbParameter[] array, Int32 index) => throw ADP.OleDb();
		public IEnumerator GetEnumerator () => throw ADP.OleDb();
		public DbParameter GetParameter (Int32 index) => throw ADP.OleDb();
		public DbParameter GetParameter (String parameterName) => throw ADP.OleDb();
		public Int32 IndexOf (OleDbParameter value) => throw ADP.OleDb();
		public Int32 IndexOf (Object value) => throw ADP.OleDb();
		public Int32 IndexOf (String parameterName) => throw ADP.OleDb();
		public void Insert (Int32 index, OleDbParameter value) => throw ADP.OleDb();
		public void Insert (Int32 index, Object value) => throw ADP.OleDb();
		public void Remove (OleDbParameter value) => throw ADP.OleDb();
		public void Remove (Object value) => throw ADP.OleDb();
		public void RemoveAt (Int32 index) => throw ADP.OleDb();
		public void RemoveAt (String parameterName) => throw ADP.OleDb();
		public void SetParameter (Int32 index, DbParameter value) => throw ADP.OleDb();
		public void SetParameter (String parameterName, DbParameter value) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbRowUpdatedEventArgs : EventArgs
	{
		public OleDbCommand Command { get; set; }
		public OleDbRowUpdatedEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
	}

    public delegate void OleDbRowUpdatedEventHandler(object sender, OleDbRowUpdatedEventArgs e);

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbRowUpdatingEventArgs : EventArgs
	{
		public IDbCommand BaseCommand { get; set; }
		public OleDbCommand Command { get; set; }
		public OleDbRowUpdatingEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
	}

    public delegate void OleDbRowUpdatingEventHandler(object sender, OleDbRowUpdatingEventArgs e);

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbSchemaGuid
	{
		public static readonly System.Guid Assertions;
		public static readonly System.Guid Catalogs;
		public static readonly System.Guid Character_Sets;
		public static readonly System.Guid Check_Constraints;
		public static readonly System.Guid Check_Constraints_By_Table;
		public static readonly System.Guid Collations;
		public static readonly System.Guid Column_Domain_Usage;
		public static readonly System.Guid Column_Privileges;
		public static readonly System.Guid Columns;
		public static readonly System.Guid Constraint_Column_Usage;
		public static readonly System.Guid Constraint_Table_Usage;
		public static readonly System.Guid DbInfoKeywords;
		public static readonly System.Guid DbInfoLiterals;
		public static readonly System.Guid Foreign_Keys;
		public static readonly System.Guid Indexes;
		public static readonly System.Guid Key_Column_Usage;
		public static readonly System.Guid Primary_Keys;
		public static readonly System.Guid Procedure_Columns;
		public static readonly System.Guid Procedure_Parameters;
		public static readonly System.Guid Procedures;
		public static readonly System.Guid Provider_Types;
		public static readonly System.Guid Referential_Constraints;
		public static readonly System.Guid SchemaGuids;
		public static readonly System.Guid Schemata;
		public static readonly System.Guid Sql_Languages;
		public static readonly System.Guid Statistics;
		public static readonly System.Guid Table_Constraints;
		public static readonly System.Guid Table_Privileges;
		public static readonly System.Guid Table_Statistics;
		public static readonly System.Guid Tables;
		public static readonly System.Guid Tables_Info;
		public static readonly System.Guid Translations;
		public static readonly System.Guid Trustee;
		public static readonly System.Guid Usage_Privileges;
		public static readonly System.Guid View_Column_Usage;
		public static readonly System.Guid View_Table_Usage;
		public static readonly System.Guid Views;
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbTransaction : System.Data.Common.DbTransaction
	{
		internal OleDbTransaction() { }
		public new System.Data.OleDb.OleDbConnection Connection => throw ADP.OleDb();
        protected override System.Data.Common.DbConnection DbConnection => throw ADP.OleDb();
        public override System.Data.IsolationLevel IsolationLevel => throw ADP.OleDb();
		public OleDbTransaction Begin () => throw ADP.OleDb();
		public OleDbTransaction Begin (IsolationLevel isolevel) => throw ADP.OleDb();
		public override void Commit () => throw ADP.OleDb();
		protected override void Dispose (Boolean disposing) => throw ADP.OleDb();
		public override void Rollback () => throw ADP.OleDb();
	}
}
#pragma warning restore 108, 114, 67