// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#pragma warning disable 108, 114, 67, 3006

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
	public sealed partial class OleDbCommand : System.Data.Common.DbCommand, System.Data.IDbCommand, System.ICloneable, System.IDisposable
	{
		public OleDbCommand () {}
		public OleDbCommand (String cmdText) => throw ADP.OleDb();
		public OleDbCommand (String cmdText, OleDbConnection connection) => throw ADP.OleDb();
		public OleDbCommand (String cmdText, OleDbConnection connection, OleDbTransaction transaction) => throw ADP.OleDb();
		public override string CommandText { get { throw ADP.OleDb(); } set { } }
		public override int CommandTimeout { get { throw ADP.OleDb(); } set { } }
		public override System.Data.CommandType CommandType { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbConnection Connection { get { throw ADP.OleDb(); } set { } }
		protected override System.Data.Common.DbConnection DbConnection { get { throw ADP.OleDb(); } set { } }
		protected override System.Data.Common.DbParameterCollection DbParameterCollection => throw ADP.OleDb();
		protected override System.Data.Common.DbTransaction DbTransaction { get { throw ADP.OleDb(); } set { } }
		public override bool DesignTimeVisible { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbParameterCollection Parameters => throw ADP.OleDb();
		public new System.Data.OleDb.OleDbTransaction Transaction { get { throw ADP.OleDb(); } set { } }
		public override System.Data.UpdateRowSource UpdatedRowSource { get { throw ADP.OleDb(); } set { } }
		public override void Cancel() { }		
		public OleDbCommand Clone () => throw ADP.OleDb();
		protected override System.Data.Common.DbParameter CreateDbParameter() => throw ADP.OleDb();
		public new OleDbParameter CreateParameter () => throw ADP.OleDb();
		protected override void Dispose (Boolean disposing) => throw ADP.OleDb();
		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior) => throw ADP.OleDb();
		public override Int32 ExecuteNonQuery () => throw ADP.OleDb();
		public new OleDbDataReader ExecuteReader () => throw ADP.OleDb();
		public new OleDbDataReader ExecuteReader (CommandBehavior behavior) => throw ADP.OleDb();
		public override Object ExecuteScalar () => throw ADP.OleDb();
		public override void Prepare () => throw ADP.OleDb();
		public void ResetCommandTimeout () => throw ADP.OleDb();
		System.Data.IDataReader System.Data.IDbCommand.ExecuteReader() => throw ADP.OleDb();	
		System.Data.IDataReader System.Data.IDbCommand.ExecuteReader(System.Data.CommandBehavior behavior) => throw ADP.OleDb();
		object System.ICloneable.Clone() => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbCommandBuilder : System.Data.Common.DbCommandBuilder
	{
		public OleDbCommandBuilder() => throw ADP.OleDb();
		public OleDbCommandBuilder (OleDbDataAdapter adapter) => throw ADP.OleDb();
		public new OleDbDataAdapter DataAdapter { get { throw ADP.OleDb(); } set { } }
		protected override void ApplyParameterInfo (DbParameter parameter, DataRow datarow, StatementType statementType, Boolean whereClause) => throw ADP.OleDb();
		public static void DeriveParameters (OleDbCommand command) => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand () => throw ADP.OleDb();
		public OleDbCommand GetDeleteCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand () => throw ADP.OleDb();
		public OleDbCommand GetInsertCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		protected override String GetParameterName (Int32 parameterOrdinal) => throw ADP.OleDb();
		protected override String GetParameterName (String parameterName) => throw ADP.OleDb();
		protected override String GetParameterPlaceholder (Int32 parameterOrdinal) => throw ADP.OleDb();
		public new OleDbCommand GetUpdateCommand () => throw ADP.OleDb();
		public new OleDbCommand GetUpdateCommand (Boolean useColumnsForParameterNames) => throw ADP.OleDb();
		public override String QuoteIdentifier (String unquotedIdentifier) => throw ADP.OleDb();
		public String QuoteIdentifier (String unquotedIdentifier, OleDbConnection connection) => throw ADP.OleDb();
		protected override void SetRowUpdatingHandler (DbDataAdapter adapter) => throw ADP.OleDb();
		public override String UnquoteIdentifier (String quotedIdentifier) => throw ADP.OleDb();
		public String UnquoteIdentifier (String quotedIdentifier, OleDbConnection connection) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbConnection : System.Data.Common.DbConnection, System.Data.IDbConnection, System.ICloneable, System.IDisposable
	{
		public OleDbConnection() => throw ADP.OleDb();
		public OleDbConnection (String connectionString) => throw ADP.OleDb();
		public override String ConnectionString { get { throw ADP.OleDb(); } set { } }
		public override Int32 ConnectionTimeout => throw ADP.OleDb();
		public override String Database => throw ADP.OleDb();
		public override String DataSource => throw ADP.OleDb();
		public String Provider => throw ADP.OleDb();
		public override String ServerVersion => throw ADP.OleDb();
		public override ConnectionState State => throw ADP.OleDb();
		protected override  DbTransaction BeginDbTransaction (IsolationLevel isolationLevel) => throw ADP.OleDb();
		public new OleDbTransaction BeginTransaction () => throw ADP.OleDb();
		public new OleDbTransaction BeginTransaction (IsolationLevel isolationLevel) => throw ADP.OleDb();
		public override void ChangeDatabase (String value) => throw ADP.OleDb();
		public override void Close () => throw ADP.OleDb();
		public new OleDbCommand CreateCommand () => throw ADP.OleDb();
		protected override  DbCommand CreateDbCommand () => throw ADP.OleDb();
		protected override  void Dispose (Boolean disposing) => throw ADP.OleDb();
		public void EnlistDistributedTransaction (ITransaction transaction) => throw ADP.OleDb();
		public override void EnlistTransaction (Transaction transaction) => throw ADP.OleDb();
		public DataTable GetOleDbSchemaTable (Guid schema, Object[] restrictions) => throw ADP.OleDb();
		public override DataTable GetSchema () => throw ADP.OleDb();
		public override DataTable GetSchema (String collectionName) => throw ADP.OleDb();
		public override DataTable GetSchema (String collectionName, String[] restrictionValues) => throw ADP.OleDb();
		public override void Open () => throw ADP.OleDb();
		public static void ReleaseObjectPool () => throw ADP.OleDb();
		public void ResetState () => throw ADP.OleDb();
		public event OleDbInfoMessageEventHandler InfoMessage;
		object System.ICloneable.Clone() => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbConnectionStringBuilder : System.Data.Common.DbConnectionStringBuilder
	{
		public OleDbConnectionStringBuilder() => throw ADP.OleDb();
		public OleDbConnectionStringBuilder (String connectionString) => throw ADP.OleDb();
		public string DataSource { get { throw ADP.OleDb(); } set { } }
		public string FileName { get { throw ADP.OleDb(); } set { } }
		public object Item { get { throw ADP.OleDb(); } set { } }
		public ICollection Keys { get { throw ADP.OleDb(); } set { } }
		public int OleDbServices { get { throw ADP.OleDb(); } set { } }
		public bool PersistSecurityInfo { get { throw ADP.OleDb(); } set { } }
		public string Provider { get { throw ADP.OleDb(); } set { } }
		public override void Clear () => throw ADP.OleDb();
		public override Boolean ContainsKey (String keyword) => throw ADP.OleDb();
		protected override void GetProperties (Hashtable propertyDescriptors) => throw ADP.OleDb();
		public override Boolean Remove (String keyword) => throw ADP.OleDb();
		public Boolean TryGetValue (String keyword, Object value) => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbDataAdapter : System.Data.Common.DbDataAdapter, System.Data.IDataAdapter, System.Data.IDbDataAdapter, System.ICloneable
	{
		public new System.Data.OleDb.OleDbCommand DeleteCommand { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbCommand InsertCommand { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbCommand SelectCommand { get { throw ADP.OleDb(); } set { } }
		System.Data.IDbCommand System.Data.IDbDataAdapter.DeleteCommand { get { throw ADP.OleDb(); } set { } }
		System.Data.IDbCommand System.Data.IDbDataAdapter.InsertCommand { get { throw ADP.OleDb(); } set { } }
		System.Data.IDbCommand System.Data.IDbDataAdapter.SelectCommand { get { throw ADP.OleDb(); } set { } }
		System.Data.IDbCommand System.Data.IDbDataAdapter.UpdateCommand { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbCommand UpdateCommand { get { throw ADP.OleDb(); } set { } }
		public OleDbDataAdapter() { }
		public OleDbDataAdapter (OleDbCommand selectCommand) => throw ADP.OleDb();
		public OleDbDataAdapter (String selectCommandText, OleDbConnection selectConnection) => throw ADP.OleDb();
		public OleDbDataAdapter (String selectCommandText, String selectConnectionString) => throw ADP.OleDb();
		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) => throw ADP.OleDb();
		public Int32 Fill (DataSet dataSet, Object ADODBRecordSet, String srcTable) => throw ADP.OleDb();
		public Int32 Fill (DataTable dataTable, Object ADODBRecordSet) => throw ADP.OleDb();
		protected override void OnRowUpdated (RowUpdatedEventArgs value) => throw ADP.OleDb();
		protected override void OnRowUpdating (RowUpdatingEventArgs value) => throw ADP.OleDb();
		public event OleDbRowUpdatedEventHandler RowUpdated;
		public event OleDbRowUpdatingEventHandler RowUpdating;
		object System.ICloneable.Clone() => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbDataReader : System.Data.Common.DbDataReader
	{
		internal OleDbDataReader() { }
		public override Int32 Depth => throw ADP.OleDb();
		public override Int32 FieldCount => throw ADP.OleDb();
		public override Boolean HasRows => throw ADP.OleDb();
		public override Boolean IsClosed => throw ADP.OleDb();
		public override Int32 RecordsAffected => throw ADP.OleDb();
		public override Int32 VisibleFieldCount => throw ADP.OleDb();
		public override void Close () => throw ADP.OleDb();
		public override Boolean GetBoolean (Int32 ordinal) => throw ADP.OleDb();
		public override Byte GetByte (Int32 ordinal) => throw ADP.OleDb();
		public override Int64 GetBytes (Int32 ordinal, Int64 dataIndex, Byte[] buffer, Int32 bufferIndex, Int32 length) => throw ADP.OleDb();
		public override Char GetChar (Int32 ordinal) => throw ADP.OleDb();
		public override Int64 GetChars (Int32 ordinal, Int64 dataIndex, Char[] buffer, Int32 bufferIndex, Int32 length) => throw ADP.OleDb();
		public new OleDbDataReader GetData (Int32 ordinal) => throw ADP.OleDb();
		protected override System.Data.Common.DbDataReader GetDbDataReader(int ordinal) => throw ADP.OleDb();
		public override String GetDataTypeName (Int32 index) => throw ADP.OleDb();
		public override DateTime GetDateTime (Int32 ordinal) => throw ADP.OleDb();
		public override Decimal GetDecimal (Int32 ordinal) => throw ADP.OleDb();
		public override Double GetDouble (Int32 ordinal) => throw ADP.OleDb();
		public override IEnumerator GetEnumerator () => throw ADP.OleDb();
		public override Type GetFieldType (Int32 index) => throw ADP.OleDb();
		public override Single GetFloat (Int32 ordinal) => throw ADP.OleDb();
		public override Guid GetGuid (Int32 ordinal) => throw ADP.OleDb();
		public override Int16 GetInt16 (Int32 ordinal) => throw ADP.OleDb();
		public override Int32 GetInt32 (Int32 ordinal) => throw ADP.OleDb();
		public override Int64 GetInt64 (Int32 ordinal) => throw ADP.OleDb();
		public override String GetName (Int32 index) => throw ADP.OleDb();
		public override Int32 GetOrdinal (String name) => throw ADP.OleDb();
		public override DataTable GetSchemaTable () => throw ADP.OleDb();
		public override String GetString (Int32 ordinal) => throw ADP.OleDb();
		public TimeSpan GetTimeSpan(int ordinal) => throw ADP.OleDb();
		public override Object GetValue (Int32 ordinal) => throw ADP.OleDb();
		public override Int32 GetValues (Object[] values) => throw ADP.OleDb();
		public override Boolean IsDBNull (Int32 ordinal) => throw ADP.OleDb();
		public override Boolean NextResult () => throw ADP.OleDb();
		public override Boolean Read () => throw ADP.OleDb();
		public override object this[int index] => throw ADP.OleDb();	  
		public override object this[string name] => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbEnumerator
	{
		public DataTable GetElements () => throw ADP.OleDb();
		public static OleDbDataReader GetEnumerator (Type type) => throw ADP.OleDb();
		public static OleDbDataReader GetRootEnumerator () => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbError
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
	public static partial class OleDbMetaDataColumnNames
	{
		public static readonly string BooleanFalseLiteral;
		public static readonly string BooleanTrueLiteral;
		public static readonly string DateTimeDigits;
		public static readonly string NativeDataType;
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbParameter : System.Data.Common.DbParameter, System.Data.IDataParameter, System.Data.IDbDataParameter, System.ICloneable
	{
		public override DbType DbType { get { throw ADP.OleDb(); } set { } }
		public override ParameterDirection Direction { get { throw ADP.OleDb(); } set { } }
		public override Boolean IsNullable { get { throw ADP.OleDb(); } set { } }
		public int Offset { get { throw ADP.OleDb(); } set { } }
		public System.Data.OleDb.OleDbType OleDbType { get { throw ADP.OleDb(); } set { } }
		public override String ParameterName { get { throw ADP.OleDb(); } set { } }
		public new Byte Precision { get { throw ADP.OleDb(); } set { } }
		public new Byte Scale { get { throw ADP.OleDb(); } set { } }
		public override Int32 Size { get { throw ADP.OleDb(); } set { } }
		public override String SourceColumn { get { throw ADP.OleDb(); } set { } }
		public override Boolean SourceColumnNullMapping { get { throw ADP.OleDb(); } set { } }
		public override DataRowVersion SourceVersion { get { throw ADP.OleDb(); } set { } }
		public override Object Value { get { throw ADP.OleDb(); } set { } }
		public OleDbParameter() { }
		public OleDbParameter (String name, OleDbType dataType) => throw ADP.OleDb();
		public OleDbParameter (String name, OleDbType dataType, Int32 size) => throw ADP.OleDb();
		public OleDbParameter (String parameterName, OleDbType dbType, Int32 size, ParameterDirection direction, Boolean isNullable, Byte precision, Byte scale, String srcColumn, DataRowVersion srcVersion, Object value) => throw ADP.OleDb();
		public OleDbParameter (String parameterName, OleDbType dbType, Int32 size, ParameterDirection direction, Byte precision, Byte scale, String sourceColumn, DataRowVersion sourceVersion, Boolean sourceColumnNullMapping, Object value) => throw ADP.OleDb();
		public OleDbParameter (String name, OleDbType dataType, Int32 size, String srcColumn) => throw ADP.OleDb();
		public OleDbParameter (String name, Object value) => throw ADP.OleDb();
		public override void ResetDbType () => throw ADP.OleDb();
		public override String ToString () => throw ADP.OleDb();
		object System.ICloneable.Clone() => throw ADP.OleDb();
		public void ResetOleDbType() => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public class OleDbParameterCollection : System.Data.Common.DbParameterCollection
	{
		internal OleDbParameterCollection() { }
		public OleDbParameter Add (OleDbParameter value) => throw ADP.OleDb();
		public override Int32 Add (Object value) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType, Int32 size) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, OleDbType oleDbType, Int32 size, String sourceColumn) => throw ADP.OleDb();
		public OleDbParameter Add (String parameterName, Object value) => throw ADP.OleDb();
		public override void AddRange (Array values) => throw ADP.OleDb();
		public void AddRange (OleDbParameter[] values) => throw ADP.OleDb();
		public OleDbParameter AddWithValue (String parameterName, Object value) => throw ADP.OleDb();
		public override void Clear () => throw ADP.OleDb();
		public Boolean Contains (OleDbParameter value) => throw ADP.OleDb();
		public override Boolean Contains (Object value) => throw ADP.OleDb();
		public override Boolean Contains (String value) => throw ADP.OleDb();
		public override void CopyTo (Array array, Int32 index) => throw ADP.OleDb();
		public void CopyTo (OleDbParameter[] array, Int32 index) => throw ADP.OleDb();
		public override IEnumerator GetEnumerator () => throw ADP.OleDb();
		protected override DbParameter GetParameter (Int32 index) => throw ADP.OleDb();
		protected override DbParameter GetParameter (String parameterName) => throw ADP.OleDb();
		public Int32 IndexOf (OleDbParameter value) => throw ADP.OleDb();
		public override Int32 IndexOf (Object value) => throw ADP.OleDb();
		public override Int32 IndexOf (String parameterName) => throw ADP.OleDb();
		public void Insert (Int32 index, OleDbParameter value) => throw ADP.OleDb();
		public override void Insert (Int32 index, Object value) => throw ADP.OleDb();
		public void Remove (OleDbParameter value) => throw ADP.OleDb();
		public override void Remove (Object value) => throw ADP.OleDb();
		public override void RemoveAt (Int32 index) => throw ADP.OleDb();
		public override void RemoveAt (String parameterName) => throw ADP.OleDb();
		protected override void SetParameter (Int32 index, DbParameter value) => throw ADP.OleDb();
		protected override void SetParameter (String parameterName, DbParameter value) => throw ADP.OleDb();
		public override int Count => throw ADP.OleDb();
		public override bool IsFixedSize => throw ADP.OleDb();
		public override bool IsReadOnly => throw ADP.OleDb();
		public override bool IsSynchronized => throw ADP.OleDb();
		public new System.Data.OleDb.OleDbParameter this[int index] { get { throw ADP.OleDb(); } set { } }
		public new System.Data.OleDb.OleDbParameter this[string parameterName] { get { throw ADP.OleDb(); } set { } }
		public override object SyncRoot => throw ADP.OleDb();
	}

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbRowUpdatedEventArgs : System.Data.Common.RowUpdatedEventArgs	  
	{
		public new OleDbCommand Command => throw ADP.OleDb();
		public OleDbRowUpdatedEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (default(System.Data.DataRow), default(System.Data.IDbCommand), default(System.Data.StatementType), default(System.Data.Common.DataTableMapping))
			=> throw ADP.OleDb();
	}

	public delegate void OleDbRowUpdatedEventHandler(object sender, OleDbRowUpdatedEventArgs e);

	[MonoTODO("OleDb is not implemented.")]
	public sealed partial class OleDbRowUpdatingEventArgs : System.Data.Common.RowUpdatingEventArgs
	{
		protected override IDbCommand BaseCommand { get { throw ADP.OleDb(); } set { } }
		public new OleDbCommand Command { get { throw ADP.OleDb(); } set { } }
		public OleDbRowUpdatingEventArgs (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
			: base (default(System.Data.DataRow), default(System.Data.IDbCommand), default(System.Data.StatementType), default(System.Data.Common.DataTableMapping))
			=> throw ADP.OleDb();
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
	
	[MonoTODO("OleDb is not implemented.")]
	internal sealed class OleDbConnectionString : DbConnectionOptions 
	{ 
		internal OleDbConnectionString(string connectionString, bool validate) : base(connectionString, null) { }
	}
}
#pragma warning restore 108, 114, 67, 3006