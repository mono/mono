// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

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
	public class OleDbCommand
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
	public class OleDbCommandBuilder
	{
		public OleDbDataAdapter DataAdapter { get; set; }
		public OleDbCommandBuilder (OleDbDataAdapter adapter) => throw ADP.OleDb();
		public void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, Boolean whereClause) => throw ADP.OleDb();
		public void DeriveParameters (OleDbCommand command) => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand () => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand () => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public String GetParameterName (Int32 parameterOrdinal) => throw ADP.OleDb();
		public String GetParameterName (String parameterName) => throw ADP.OleDb();
		public String GetParameterPlaceholder (Int32 parameterOrdinal) => throw ADP.OleDb();
		public OleDbCommand GetUpdateCommand () => throw ADP.OleDb();
		public OleDbCommand GetUpdateCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public String QuoteIdentifier (String unquotedIdentifier) => throw ADP.OleDb();
		public String QuoteIdentifier (String unquotedIdentifier, OleDbConnection connection) => throw ADP.OleDb();
		public void SetRowUpdatingHandler (DbDataAdapter adapter) => throw ADP.OleDb();
		public String UnquoteIdentifier (String quotedIdentifier) => throw ADP.OleDb();
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
	public class OleDbDataReader
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
		public String Message { get; set; }
		public Int32 NativeError { get; set; }
		public String Source { get; set; }
		public String SQLState { get; set; }
		public String ToString () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbErrorCollection
	{
		public Int32 Count { get; set; }
		public OleDbError Item { get; set; }
		public void CopyTo (Array array, Int32 index) => throw ADP.OleDb();
		public void CopyTo (OleDbError[] array, Int32 index) => throw ADP.OleDb();
		public IEnumerator GetEnumerator () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbException
	{
		public Int32 ErrorCode { get; set; }
		public OleDbErrorCollection Errors { get; set; }
		public void GetObjectData (SerializationInfo si, StreamingContext context) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbFactory : DbProviderFactory
	{
		public OleDbFactory Instance;
		public DbCommand CreateCommand () => throw ADP.OleDb();
		public DbCommandBuilder CreateCommandBuilder () => throw ADP.OleDb();
		public DbConnection CreateConnection () => throw ADP.OleDb();
		public DbConnectionStringBuilder CreateConnectionStringBuilder () => throw ADP.OleDb();
		public DbDataAdapter CreateDataAdapter () => throw ADP.OleDb();
		public DbParameter CreateParameter () => throw ADP.OleDb();
		public CodeAccessPermission CreatePermission (PermissionState state) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbInfoMessageEventArgs
	{
		public Int32 ErrorCode { get; set; }
		public OleDbErrorCollection Errors { get; set; }
		public String Message { get; set; }
		public String Source { get; set; }
		public String ToString () => throw ADP.OleDb();
	}

    public delegate void OleDbInfoMessageEventHandler(object sender, OleDbInfoMessageEventArgs e);

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbLiteral
	{
		public OleDbLiteral Binary_Literal;
		public OleDbLiteral Catalog_Name;
		public OleDbLiteral Catalog_Separator;
		public OleDbLiteral Char_Literal;
		public OleDbLiteral Column_Alias;
		public OleDbLiteral Column_Name;
		public OleDbLiteral Correlation_Name;
		public OleDbLiteral Cube_Name;
		public OleDbLiteral Cursor_Name;
		public OleDbLiteral Dimension_Name;
		public OleDbLiteral Escape_Percent_Prefix;
		public OleDbLiteral Escape_Percent_Suffix;
		public OleDbLiteral Escape_Underscore_Prefix;
		public OleDbLiteral Escape_Underscore_Suffix;
		public OleDbLiteral Hierarchy_Name;
		public OleDbLiteral Index_Name;
		public OleDbLiteral Invalid;
		public OleDbLiteral Level_Name;
		public OleDbLiteral Like_Percent;
		public OleDbLiteral Like_Underscore;
		public OleDbLiteral Member_Name;
		public OleDbLiteral Procedure_Name;
		public OleDbLiteral Property_Name;
		public OleDbLiteral Quote_Prefix;
		public OleDbLiteral Quote_Suffix;
		public OleDbLiteral Schema_Name;
		public OleDbLiteral Schema_Separator;
		public OleDbLiteral Table_Name;
		public OleDbLiteral Text_Command;
		public OleDbLiteral User_Name;
		public OleDbLiteral value__;
		public OleDbLiteral View_Name;
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbMetaDataCollectionNames
	{
		public OleDbMetaDataCollectionNames Catalogs;
		public OleDbMetaDataCollectionNames Collations;
		public OleDbMetaDataCollectionNames Columns;
		public OleDbMetaDataCollectionNames Indexes;
		public OleDbMetaDataCollectionNames ProcedureColumns;
		public OleDbMetaDataCollectionNames ProcedureParameters;
		public OleDbMetaDataCollectionNames Procedures;
		public OleDbMetaDataCollectionNames Tables;
		public OleDbMetaDataCollectionNames Views;
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
	public class OleDbSchemaGuid
	{
		public OleDbSchemaGuid Assertions;
		public OleDbSchemaGuid Catalogs;
		public OleDbSchemaGuid Character_Sets;
		public OleDbSchemaGuid Check_Constraints;
		public OleDbSchemaGuid Check_Constraints_By_Table;
		public OleDbSchemaGuid Collations;
		public OleDbSchemaGuid Column_Domain_Usage;
		public OleDbSchemaGuid Column_Privileges;
		public OleDbSchemaGuid Columns;
		public OleDbSchemaGuid Constraint_Column_Usage;
		public OleDbSchemaGuid Constraint_Table_Usage;
		public OleDbSchemaGuid DbInfoKeywords;
		public OleDbSchemaGuid DbInfoLiterals;
		public OleDbSchemaGuid Foreign_Keys;
		public OleDbSchemaGuid Indexes;
		public OleDbSchemaGuid Key_Column_Usage;
		public OleDbSchemaGuid Primary_Keys;
		public OleDbSchemaGuid Procedure_Columns;
		public OleDbSchemaGuid Procedure_Parameters;
		public OleDbSchemaGuid Procedures;
		public OleDbSchemaGuid Provider_Types;
		public OleDbSchemaGuid Referential_Constraints;
		public OleDbSchemaGuid SchemaGuids;
		public OleDbSchemaGuid Schemata;
		public OleDbSchemaGuid Sql_Languages;
		public OleDbSchemaGuid Statistics;
		public OleDbSchemaGuid Table_Constraints;
		public OleDbSchemaGuid Table_Privileges;
		public OleDbSchemaGuid Table_Statistics;
		public OleDbSchemaGuid Tables;
		public OleDbSchemaGuid Tables_Info;
		public OleDbSchemaGuid Translations;
		public OleDbSchemaGuid Trustee;
		public OleDbSchemaGuid Usage_Privileges;
		public OleDbSchemaGuid View_Column_Usage;
		public OleDbSchemaGuid View_Table_Usage;
		public OleDbSchemaGuid Views;
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbTransaction
	{
		public OleDbConnection Connection { get; set; }
		public DbConnection DbConnection { get; set; }
		public IsolationLevel IsolationLevel { get; set; }
		public OleDbTransaction Begin () => throw ADP.OleDb();
		public OleDbTransaction Begin (IsolationLevel isolevel) => throw ADP.OleDb();
		public void Commit () => throw ADP.OleDb();
		public void Dispose (Boolean disposing) => throw ADP.OleDb();
		public void Rollback () => throw ADP.OleDb();
	}
}