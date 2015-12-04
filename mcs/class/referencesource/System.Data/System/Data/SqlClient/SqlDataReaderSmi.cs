//------------------------------------------------------------------------------
// <copyright file="SqlDataReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {
    using System;
    using System.Data;
    using System.Data.Sql;
    using System.Data.SqlTypes;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Diagnostics;        // for Conditional compilation
    using System.Diagnostics.CodeAnalysis;
    using System.Xml;
    using Microsoft.SqlServer.Server;
    using System.Data.ProviderBase;
    using System.Data.Common;
    using System.Threading.Tasks;

        // SqlServer provider's implementation of ISqlReader.
    //    Supports ISqlReader and ISqlResultSet objects.
    //
    //    User should never be able to create one of these themselves, nor subclass.
    //        This is accomplished by having no public override constructors.
    internal sealed class SqlDataReaderSmi : SqlDataReader {

    
    //
    // IDBRecord properties
    //
        public override int FieldCount {
            get {
                ThrowIfClosed( "FieldCount" );
                return InternalFieldCount;
            }
        }

        public override int VisibleFieldCount {
            get {
                ThrowIfClosed("VisibleFieldCount");

                if (FNotInResults()) {
                    return 0;
                }

                return _visibleColumnCount;
            }
        }

    //
    // IDBRecord Metadata Methods
    //
        public override String GetName(int ordinal) {
            EnsureCanGetMetaData( "GetName" );
            return _currentMetaData[ordinal].Name;
        }

        public override String GetDataTypeName(int ordinal) {
            EnsureCanGetMetaData( "GetDataTypeName" );
            SmiExtendedMetaData md = _currentMetaData[ordinal];
            if ( SqlDbType.Udt == md.SqlDbType ) {
                return md.TypeSpecificNamePart1 + "." + md.TypeSpecificNamePart2 + "." + md.TypeSpecificNamePart3;
            }
            else {
                return md.TypeName;
            }
        }

        public override Type GetFieldType(int ordinal) {
            EnsureCanGetMetaData( "GetFieldType" );
            if (SqlDbType.Udt == _currentMetaData[ordinal].SqlDbType) {
                return _currentMetaData[ordinal].Type;
            }
            else {
                return MetaType.GetMetaTypeFromSqlDbType(
                    _currentMetaData[ordinal].SqlDbType, _currentMetaData[ordinal].IsMultiValued).ClassType ;
            }
        }

        override public Type GetProviderSpecificFieldType(int ordinal) {
            EnsureCanGetMetaData( "GetProviderSpecificFieldType" );

            if (SqlDbType.Udt == _currentMetaData[ordinal].SqlDbType) {
                return _currentMetaData[ordinal].Type;
            }
            else {
                return MetaType.GetMetaTypeFromSqlDbType(
                    _currentMetaData[ordinal].SqlDbType, _currentMetaData[ordinal].IsMultiValued).SqlType ;
            }
        }

        public override int Depth {
            get{
                ThrowIfClosed( "Depth" );
                return 0;
            }
        } // 

        public override Object GetValue(int ordinal) {
            EnsureCanGetCol( "GetValue", ordinal);
            SmiQueryMetaData metaData = _currentMetaData[ordinal];
            if (_currentConnection.IsKatmaiOrNewer) {
                return ValueUtilsSmi.GetValue200(_readerEventSink, (SmiTypedGetterSetter)_currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
            }
            else {
                return ValueUtilsSmi.GetValue(_readerEventSink, _currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
            }
        }

        public override T GetFieldValue<T>(int ordinal) {
            EnsureCanGetCol( "GetFieldValue<T>", ordinal);
            SmiQueryMetaData metaData = _currentMetaData[ordinal];

            if (_typeofINullable.IsAssignableFrom(typeof(T))) {
                // If its a SQL Type or Nullable UDT
                if (_currentConnection.IsKatmaiOrNewer) {
                    return (T)ValueUtilsSmi.GetSqlValue200(_readerEventSink, (SmiTypedGetterSetter)_currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
                }
                else {
                    return (T)ValueUtilsSmi.GetSqlValue(_readerEventSink, _currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
                }
            }
            else {
                // Otherwise Its a CLR or non-Nullable UDT
                if (_currentConnection.IsKatmaiOrNewer) {
                    return (T)ValueUtilsSmi.GetValue200(_readerEventSink, (SmiTypedGetterSetter)_currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
                }
                else {
                    return (T)ValueUtilsSmi.GetValue(_readerEventSink, _currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
                }
            }
        }

        public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken) {
            // As per Async spec, Context Connections do not support async
            return ADP.CreatedTaskWithException<T>(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
        }

        override internal SqlBuffer.StorageType GetVariantInternalStorageType(int ordinal) {
            Debug.Assert(null != _currentColumnValuesV3, "Attempting to get variant internal storage type without calling GetValue first");
            if (IsDBNull(ordinal))
                return SqlBuffer.StorageType.Empty;

            SmiMetaData valueMetaData = _currentColumnValuesV3.GetVariantType(_readerEventSink, ordinal);
            if (valueMetaData == null)
                return SqlBuffer.StorageType.Empty;
            else
                return ValueUtilsSmi.SqlDbTypeToStorageType(valueMetaData.SqlDbType);
        }

        public override int GetValues(object[] values) {
            EnsureCanGetCol( "GetValues", 0);
            if (null == values) {
                throw ADP.ArgumentNull("values");
            }

            int copyLength = (values.Length < _visibleColumnCount) ? values.Length : _visibleColumnCount;
            for(int i=0; i<copyLength; i++) {
                values[_indexMap[i]] = GetValue(i);
            }
            return copyLength;
        }

        public override int GetOrdinal(string name) {
            EnsureCanGetMetaData( "GetOrdinal" );
            if (null == _fieldNameLookup) {
                _fieldNameLookup = new FieldNameLookup( (IDataReader) this, -1 ); // 
            }
            return _fieldNameLookup.GetOrdinal(name); // MDAC 71470
        }

        // Generic array access by column index (accesses column value)
        public override object this[int ordinal] {
            get {
                return GetValue( ordinal );
            }
        }

        // Generic array access by column name (accesses column value)
        public override object this[string strName] {
            get {
                return GetValue( GetOrdinal( strName ) );
            }
        }

    //
    // IDataRecord Data Access methods
    //
        public override bool IsDBNull(int ordinal) {
            EnsureCanGetCol( "IsDBNull", ordinal);
            return ValueUtilsSmi.IsDBNull(_readerEventSink, _currentColumnValuesV3, ordinal);
        }

        public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken) {
            // As per Async spec, Context Connections do not support async
            return ADP.CreatedTaskWithException<bool>(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
        }

        public override bool GetBoolean(int ordinal) {
            EnsureCanGetCol( "GetBoolean", ordinal);
            return ValueUtilsSmi.GetBoolean(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override byte GetByte(int ordinal) {
            EnsureCanGetCol( "GetByte", ordinal);
            return ValueUtilsSmi.GetByte(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override long GetBytes(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length) {
            EnsureCanGetCol( "GetBytes", ordinal);
            return ValueUtilsSmi.GetBytes(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], fieldOffset, buffer, bufferOffset, length, true);
        }

        // XmlReader support code calls this method.
        internal override long GetBytesInternal(int ordinal, long fieldOffset, byte[] buffer, int bufferOffset, int length) {
            EnsureCanGetCol( "GetBytes", ordinal);
            return ValueUtilsSmi.GetBytesInternal(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], fieldOffset, buffer, bufferOffset, length, false);
        }

        public override char GetChar(int ordinal) {
            throw ADP.NotSupported();
        }

        public override long GetChars(int ordinal, long fieldOffset, char[] buffer, int bufferOffset, int length) {
            EnsureCanGetCol( "GetChars", ordinal);
            SmiExtendedMetaData metaData = _currentMetaData[ordinal];
            if (IsCommandBehavior(CommandBehavior.SequentialAccess)) {
                if (metaData.SqlDbType == SqlDbType.Xml) {
                    return GetStreamingXmlChars(ordinal, fieldOffset, buffer, bufferOffset, length);
                }
            }
            return ValueUtilsSmi.GetChars(_readerEventSink, _currentColumnValuesV3, ordinal, metaData, fieldOffset, buffer, bufferOffset, length);
        }

        public override Guid GetGuid(int ordinal) {
            EnsureCanGetCol( "GetGuid", ordinal);
            return ValueUtilsSmi.GetGuid(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Int16 GetInt16(int ordinal) {
            EnsureCanGetCol( "GetInt16", ordinal);
            return ValueUtilsSmi.GetInt16(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Int32 GetInt32(int ordinal) {
            EnsureCanGetCol( "GetInt32", ordinal);
            return ValueUtilsSmi.GetInt32(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Int64 GetInt64(int ordinal) {
            EnsureCanGetCol( "GetInt64", ordinal);
            return ValueUtilsSmi.GetInt64(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Single GetFloat(int ordinal) {
            EnsureCanGetCol( "GetFloat", ordinal);
            return ValueUtilsSmi.GetSingle(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Double GetDouble(int ordinal) {
            EnsureCanGetCol( "GetDouble", ordinal);
            return ValueUtilsSmi.GetDouble(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override String GetString(int ordinal) {
            EnsureCanGetCol( "GetString", ordinal);
            return ValueUtilsSmi.GetString(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override Decimal GetDecimal(int ordinal) {
            EnsureCanGetCol( "GetDecimal", ordinal);
            return ValueUtilsSmi.GetDecimal(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override DateTime GetDateTime(int ordinal) {
            EnsureCanGetCol( "GetDateTime", ordinal);
            return ValueUtilsSmi.GetDateTime(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

    //
    // IDataReader properties
    //
        // Logically closed test. I.e. is this object closed as far as external access is concerned?
        public override bool IsClosed {
            get { 
                return IsReallyClosed(); 
            }
        }

        public override int RecordsAffected {
            get {
                return base.Command.InternalRecordsAffected;
            }
        }

    //
    // IDataReader methods
    //
        internal override void CloseReaderFromConnection() {
            // Context Connections do not support async - so there is no threading issues with closing from the connection
            CloseInternal(closeConnection: false);
        }

        public override void Close() {
            // Connection should be open at this point, so we can do multiple checks of HasEvents, and we may need to close the connection afterwards
            CloseInternal(closeConnection: IsCommandBehavior(CommandBehavior.CloseConnection));
        }

        private void CloseInternal(bool closeConnection) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<sc.SqlDataReaderSmi.Close|API> %d#", ObjectID);
            bool processFinallyBlock = true;
            try  {
                if(!IsClosed) {
                    _hasRows = false;

                    // Process the remaining events. This makes sure that environment changes are applied and any errors are picked up.
                    while(_eventStream.HasEvents) {
                        _eventStream.ProcessEvent( _readerEventSink );
                        _readerEventSink.ProcessMessagesAndThrow(true);
                    }

                    // Close the request executor
                    _requestExecutor.Close(_readerEventSink);
                    _readerEventSink.ProcessMessagesAndThrow(true);
                }
            }
            catch (Exception e) {
                processFinallyBlock = ADP.IsCatchableExceptionType(e);
                throw;
            }
            finally {
                if (processFinallyBlock) {
                    _isOpen = false;

                    if ((closeConnection) && (Connection != null)) {
                        Connection.Close();
                    }

                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        // Move to the next resultset
        public override unsafe bool NextResult() {
            ThrowIfClosed( "NextResult" );

            bool hasAnotherResult = InternalNextResult(false);

            return hasAnotherResult;
        }
        
        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        {
            // Async not supported on Context Connections
            return ADP.CreatedTaskWithException<bool>(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
        }

        internal unsafe bool InternalNextResult(bool ignoreNonFatalMessages) {
            IntPtr hscp = IntPtr.Zero;
            if (Bid.AdvancedOn) {
                Bid.ScopeEnter(out hscp, "<sc.SqlDataReaderSmi.InternalNextResult|ADV> %d#", ObjectID);
            }
            try {
                _hasRows = false;

                if( PositionState.AfterResults != _currentPosition )
                {
                    // Consume any remaning rows in the current result. 
                    
                    while( InternalRead(ignoreNonFatalMessages) ) {
                        // This space intentionally left blank
                    }

                    // reset resultset metadata - it will be created again if there is a pending resultset
                    ResetResultSet();

                    // Process the events until metadata is found or all of the
                    // available events have been consumed. If there is another
                    // result, the metadata for it will be available after the last
                    // read on the prior result.

                    while(null == _currentMetaData && _eventStream.HasEvents) {
                        _eventStream.ProcessEvent( _readerEventSink );
                        _readerEventSink.ProcessMessagesAndThrow(ignoreNonFatalMessages);
                    }
                }

                return PositionState.AfterResults != _currentPosition;
            }
            finally {
                if (Bid.AdvancedOn) {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }

        public override bool Read() {
            ThrowIfClosed( "Read" );
            bool hasAnotherRow = InternalRead(false);

            return hasAnotherRow;
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
        {
            // Async not supported on Context Connections
            return ADP.CreatedTaskWithException<bool>(ADP.ExceptionWithStackTrace(SQL.NotAvailableOnContextConnection()));
        }

        internal unsafe bool InternalRead(bool ignoreNonFatalErrors) {
            IntPtr hscp = IntPtr.Zero;
            if (Bid.AdvancedOn) {
                Bid.ScopeEnter(out hscp, "<sc.SqlDataReaderSmi.InternalRead|ADV> %d#", ObjectID);
            }
            try {
                // Don't move unless currently in results.
                if( FInResults() ) {
                
                    // Set current row to null so we can see if we get a new one
                    _currentColumnValues = null;
                    _currentColumnValuesV3 = null;
                    
                    // Reset blobs
                    if (_currentStream != null) {
                        _currentStream.SetClosed();
                        _currentStream = null;
                    }
                    if (_currentTextReader != null) {
                        _currentTextReader.SetClosed();
                        _currentTextReader = null;
                    }

                    // NOTE: SQLBUDT #386118 -- may indicate that we want to break this loop when we get a MessagePosted callback, but we can't prove that.
                    while(  null == _currentColumnValues &&                         // Did we find a row?
                            null == _currentColumnValuesV3 &&                       // Did we find a V3 row?
                            FInResults() &&                         // Was the batch terminated due to a serious error?
                            PositionState.AfterRows != _currentPosition &&              // Have we seen a statement completed event?
                            _eventStream.HasEvents ) {              // Have we processed all events?
                        _eventStream.ProcessEvent( _readerEventSink );
                        _readerEventSink.ProcessMessagesAndThrow(ignoreNonFatalErrors);
                    }
                }
                
                return PositionState.OnRow == _currentPosition;
            }
            finally {
                if (Bid.AdvancedOn) {
                    Bid.ScopeLeave(ref hscp);
                }
            }
        }    

        public override DataTable GetSchemaTable() {
            ThrowIfClosed( "GetSchemaTable" );

            if ( null == _schemaTable && FInResults() )
                {

                DataTable schemaTable = new DataTable( "SchemaTable" );
                schemaTable.Locale = System.Globalization.CultureInfo.InvariantCulture;
                schemaTable.MinimumCapacity = InternalFieldCount;

                DataColumn ColumnName                       = new DataColumn(SchemaTableColumn.ColumnName,                       typeof(System.String));
                DataColumn Ordinal                          = new DataColumn(SchemaTableColumn.ColumnOrdinal,                    typeof(System.Int32));
                DataColumn Size                             = new DataColumn(SchemaTableColumn.ColumnSize,                       typeof(System.Int32));
                DataColumn Precision                        = new DataColumn(SchemaTableColumn.NumericPrecision,                 typeof(System.Int16));
                DataColumn Scale                            = new DataColumn(SchemaTableColumn.NumericScale,                     typeof(System.Int16));

                DataColumn DataType                         = new DataColumn(SchemaTableColumn.DataType,                         typeof(System.Type));
                DataColumn ProviderSpecificDataType         = new DataColumn(SchemaTableOptionalColumn.ProviderSpecificDataType, typeof(System.Type));
                DataColumn ProviderType                     = new DataColumn(SchemaTableColumn.ProviderType,                     typeof(System.Int32));
                DataColumn NonVersionedProviderType         = new DataColumn(SchemaTableColumn.NonVersionedProviderType,         typeof(System.Int32));

                DataColumn IsLong                           = new DataColumn(SchemaTableColumn.IsLong,                           typeof(System.Boolean));
                DataColumn AllowDBNull                      = new DataColumn(SchemaTableColumn.AllowDBNull,                      typeof(System.Boolean));
                DataColumn IsReadOnly                       = new DataColumn(SchemaTableOptionalColumn.IsReadOnly,               typeof(System.Boolean));
                DataColumn IsRowVersion                     = new DataColumn(SchemaTableOptionalColumn.IsRowVersion,             typeof(System.Boolean));

                DataColumn IsUnique                         = new DataColumn(SchemaTableColumn.IsUnique,                         typeof(System.Boolean));
                DataColumn IsKey                            = new DataColumn(SchemaTableColumn.IsKey,                            typeof(System.Boolean));
                DataColumn IsAutoIncrement                  = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement,          typeof(System.Boolean));
                DataColumn IsHidden                         = new DataColumn(SchemaTableOptionalColumn.IsHidden,                 typeof(System.Boolean));

                DataColumn BaseCatalogName                  = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName,          typeof(System.String));
                DataColumn BaseSchemaName                   = new DataColumn(SchemaTableColumn.BaseSchemaName,                   typeof(System.String));
                DataColumn BaseTableName                    = new DataColumn(SchemaTableColumn.BaseTableName,                    typeof(System.String));
                DataColumn BaseColumnName                   = new DataColumn(SchemaTableColumn.BaseColumnName,                   typeof(System.String));

                // unique to SqlClient
                DataColumn BaseServerName                   = new DataColumn(SchemaTableOptionalColumn.BaseServerName,           typeof(System.String));
                DataColumn IsAliased                        = new DataColumn(SchemaTableColumn.IsAliased,                        typeof(System.Boolean));
                DataColumn IsExpression                     = new DataColumn(SchemaTableColumn.IsExpression,                     typeof(System.Boolean));
                DataColumn IsIdentity                       = new DataColumn("IsIdentity",                                       typeof(System.Boolean));
                // UDT specific. Holds UDT typename ONLY if the type of the column is UDT, otherwise the data type
                DataColumn DataTypeName                     = new DataColumn("DataTypeName",                                     typeof(System.String));
                DataColumn UdtAssemblyQualifiedName         = new DataColumn("UdtAssemblyQualifiedName",                         typeof(System.String));
                // Xml metadata specific
                DataColumn XmlSchemaCollectionDatabase      = new DataColumn("XmlSchemaCollectionDatabase",                      typeof(System.String));
                DataColumn XmlSchemaCollectionOwningSchema  = new DataColumn("XmlSchemaCollectionOwningSchema",                  typeof(System.String));
                DataColumn XmlSchemaCollectionName          = new DataColumn("XmlSchemaCollectionName",                          typeof(System.String));
                // SparseColumnSet
                DataColumn IsColumnSet = new DataColumn("IsColumnSet", typeof(System.Boolean));

                Ordinal.DefaultValue = 0;
                IsLong.DefaultValue = false;

                DataColumnCollection columns = schemaTable.Columns;

                // must maintain order for backward compatibility
                columns.Add(ColumnName);
                columns.Add(Ordinal);
                columns.Add(Size);
                columns.Add(Precision);
                columns.Add(Scale);
                columns.Add(IsUnique);
                columns.Add(IsKey);
                columns.Add(BaseServerName);
                columns.Add(BaseCatalogName);
                columns.Add(BaseColumnName);
                columns.Add(BaseSchemaName);
                columns.Add(BaseTableName);
                columns.Add(DataType);
                columns.Add(AllowDBNull);
                columns.Add(ProviderType);
                columns.Add(IsAliased);
                columns.Add(IsExpression);
                columns.Add(IsIdentity);
                columns.Add(IsAutoIncrement);
                columns.Add(IsRowVersion);
                columns.Add(IsHidden);
                columns.Add(IsLong);
                columns.Add(IsReadOnly);
                columns.Add(ProviderSpecificDataType);
                columns.Add(DataTypeName);
                columns.Add(XmlSchemaCollectionDatabase);
                columns.Add(XmlSchemaCollectionOwningSchema);
                columns.Add(XmlSchemaCollectionName);
                columns.Add(UdtAssemblyQualifiedName);
                columns.Add(NonVersionedProviderType);
                columns.Add(IsColumnSet);

                for (int i = 0; i < InternalFieldCount; i++) {
                    SmiQueryMetaData colMetaData = _currentMetaData[i];

                    long maxLength = colMetaData.MaxLength;
                        
                    MetaType metaType = MetaType.GetMetaTypeFromSqlDbType(colMetaData.SqlDbType, colMetaData.IsMultiValued);
                    if ( SmiMetaData.UnlimitedMaxLengthIndicator == maxLength ) {
                        metaType = MetaType.GetMaxMetaTypeFromMetaType( metaType );
                        maxLength = (metaType.IsSizeInCharacters && !metaType.IsPlp) ? (0x7fffffff / 2) : 0x7fffffff;
                    }

                    DataRow schemaRow = schemaTable.NewRow();

                    // NOTE: there is an impedence mismatch here - the server always 
                    // treats numeric data as variable length and sends a maxLength
                    // based upon the precision, whereas TDS always sends 17 for 
                    // the max length; rather than push this logic into the server,
                    // I've elected to make a fixup here instead.
                    if (SqlDbType.Decimal == colMetaData.SqlDbType) {
                        // 
                        maxLength = TdsEnums.MAX_NUMERIC_LEN;   // SQLBUDT 339686
                    }
                    else if (SqlDbType.Variant == colMetaData.SqlDbType) {
                        // 
                        maxLength = 8009;   // SQLBUDT 340726
                    }

                    schemaRow[ColumnName]   = colMetaData.Name;
                    schemaRow[Ordinal]      = i;
                    schemaRow[Size]         = maxLength;
                    
                    schemaRow[ProviderType] = (int) colMetaData.SqlDbType; // SqlDbType
                    schemaRow[NonVersionedProviderType] = (int) colMetaData.SqlDbType; // SqlDbType

                    if (colMetaData.SqlDbType != SqlDbType.Udt) {
                        schemaRow[DataType]                 = metaType.ClassType; // com+ type
                        schemaRow[ProviderSpecificDataType] = metaType.SqlType;
                    }
                    else {
                        schemaRow[UdtAssemblyQualifiedName] = colMetaData.Type.AssemblyQualifiedName;
                        schemaRow[DataType]                 = colMetaData.Type;
                        schemaRow[ProviderSpecificDataType] = colMetaData.Type;
                    }

                    // NOTE: there is also an impedence mismatch here - the server 
                    // has different ideas about what the precision value should be
                    // than does the client bits.  I tried fixing up the default
                    // meta data values in SmiMetaData, however, it caused the 
                    // server suites to fall over dead.  Rather than attempt to 
                    // bake it into the server, I'm fixing it up in the client.
                    byte precision = 0xff;  // default for everything, except certain numeric types.
                    
                    // 
                    switch (colMetaData.SqlDbType) {
                        case SqlDbType.BigInt:
                        case SqlDbType.DateTime:
                        case SqlDbType.Decimal:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.SmallInt:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            precision = colMetaData.Precision;  
                            break;
                        case SqlDbType.Float:       
                            precision = 15;  
                            break;
                        case SqlDbType.Real:        
                            precision = 7;  
                            break;
                        default:
                            precision = 0xff;   // everything else is unknown;
                            break;
                    }

                    schemaRow[Precision]        = precision;
                    
                    // 
                    if ( SqlDbType.Decimal == colMetaData.SqlDbType ||
                        SqlDbType.Time == colMetaData.SqlDbType ||
                        SqlDbType.DateTime2 == colMetaData.SqlDbType ||
                        SqlDbType.DateTimeOffset == colMetaData.SqlDbType) {
                        schemaRow[Scale]            = colMetaData.Scale;
                    }
                    else {
                        schemaRow[Scale]            = MetaType.GetMetaTypeFromSqlDbType(
                                                        colMetaData.SqlDbType, colMetaData.IsMultiValued).Scale;
                    }
                    
                    schemaRow[AllowDBNull]      = colMetaData.AllowsDBNull;
                    if ( !( colMetaData.IsAliased.IsNull ) ) {
                        schemaRow[IsAliased]        = colMetaData.IsAliased.Value;
                    }

                    if ( !( colMetaData.IsKey.IsNull ) ) {
                        schemaRow[IsKey]            = colMetaData.IsKey.Value;
                    }

                    if ( !( colMetaData.IsHidden.IsNull ) ) {
                        schemaRow[IsHidden]         = colMetaData.IsHidden.Value;
                    }

                    if ( !( colMetaData.IsExpression.IsNull ) ) {
                        schemaRow[IsExpression]     = colMetaData.IsExpression.Value;
                    }

                    schemaRow[IsReadOnly]       = colMetaData.IsReadOnly;
                    schemaRow[IsIdentity]       = colMetaData.IsIdentity;
                    schemaRow[IsColumnSet]      = colMetaData.IsColumnSet;
                    schemaRow[IsAutoIncrement]  = colMetaData.IsIdentity;
                    schemaRow[IsLong]           = metaType.IsLong;

                    // mark unique for timestamp columns
                    if ( SqlDbType.Timestamp == colMetaData.SqlDbType ) {
                        schemaRow[IsUnique]         = true;
                        schemaRow[IsRowVersion]     = true;
                    }
                    else {
                        schemaRow[IsUnique]         = false;
                        schemaRow[IsRowVersion]     = false;
                    }

                    if ( !ADP.IsEmpty( colMetaData.ColumnName ) ) {
                        schemaRow[BaseColumnName]   = colMetaData.ColumnName;
                    }
                    else if (!ADP.IsEmpty( colMetaData.Name)) {
                        // Use projection name if base column name is not present
                        schemaRow[BaseColumnName]   = colMetaData.Name;
                    }

                    if ( !ADP.IsEmpty(colMetaData.TableName ) ) {
                        schemaRow[BaseTableName]    = colMetaData.TableName;
                    }

                    if (!ADP.IsEmpty(colMetaData.SchemaName)) {
                        schemaRow[BaseSchemaName]   = colMetaData.SchemaName;
                    }

                    if (!ADP.IsEmpty(colMetaData.CatalogName)) {
                        schemaRow[BaseCatalogName]  = colMetaData.CatalogName;
                    }

                    if (!ADP.IsEmpty(colMetaData.ServerName)) {
                        schemaRow[BaseServerName]   = colMetaData.ServerName;
                    }

                    if ( SqlDbType.Udt == colMetaData.SqlDbType ) {
                        schemaRow[DataTypeName] = colMetaData.TypeSpecificNamePart1 + "." + colMetaData.TypeSpecificNamePart2 + "." + colMetaData.TypeSpecificNamePart3;
                    }
                    else {                        
                        schemaRow[DataTypeName] = metaType.TypeName;  
                    }

                    // Add Xml metadata
                    if ( SqlDbType.Xml == colMetaData.SqlDbType ) {
                        schemaRow[XmlSchemaCollectionDatabase]      = colMetaData.TypeSpecificNamePart1;
                        schemaRow[XmlSchemaCollectionOwningSchema]  = colMetaData.TypeSpecificNamePart2;
                        schemaRow[XmlSchemaCollectionName]          = colMetaData.TypeSpecificNamePart3;
                    }

                    schemaTable.Rows.Add(schemaRow);
                    schemaRow.AcceptChanges();
                }

                // mark all columns as readonly
                foreach(DataColumn column in columns) {
                    column.ReadOnly = true; // MDAC 70943
                }

                _schemaTable = schemaTable;
            }

            return _schemaTable;
        }

    //
    //    ISqlRecord methods
    //
        public override SqlBinary GetSqlBinary(int ordinal) {
            EnsureCanGetCol( "GetSqlBinary", ordinal);
            return ValueUtilsSmi.GetSqlBinary(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlBoolean GetSqlBoolean(int ordinal) {
            EnsureCanGetCol( "GetSqlBoolean", ordinal);
            return ValueUtilsSmi.GetSqlBoolean(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlByte GetSqlByte(int ordinal) {
            EnsureCanGetCol( "GetSqlByte", ordinal);
            return ValueUtilsSmi.GetSqlByte(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlInt16 GetSqlInt16(int ordinal) {
            EnsureCanGetCol( "GetSqlInt16", ordinal);
            return ValueUtilsSmi.GetSqlInt16(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlInt32 GetSqlInt32(int ordinal) {
            EnsureCanGetCol( "GetSqlInt32", ordinal);
            return ValueUtilsSmi.GetSqlInt32(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlInt64 GetSqlInt64(int ordinal) {
            EnsureCanGetCol( "GetSqlInt64", ordinal);
            return ValueUtilsSmi.GetSqlInt64(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlSingle GetSqlSingle(int ordinal) {
            EnsureCanGetCol( "GetSqlSingle", ordinal);
            return ValueUtilsSmi.GetSqlSingle(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlDouble GetSqlDouble(int ordinal) {
            EnsureCanGetCol( "GetSqlDouble", ordinal);
            return ValueUtilsSmi.GetSqlDouble(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlMoney GetSqlMoney(int ordinal) {
            EnsureCanGetCol( "GetSqlMoney", ordinal);
            return ValueUtilsSmi.GetSqlMoney(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlDateTime GetSqlDateTime(int ordinal) {
            EnsureCanGetCol( "GetSqlDateTime", ordinal);
            return ValueUtilsSmi.GetSqlDateTime(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }


        public override SqlDecimal GetSqlDecimal(int ordinal) {
            EnsureCanGetCol( "GetSqlDecimal", ordinal);
            return ValueUtilsSmi.GetSqlDecimal(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlString GetSqlString(int ordinal) {
            EnsureCanGetCol( "GetSqlString", ordinal);
            return ValueUtilsSmi.GetSqlString(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlGuid GetSqlGuid(int ordinal) {
            EnsureCanGetCol( "GetSqlGuid", ordinal);
            return ValueUtilsSmi.GetSqlGuid(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal]);
        }

        public override SqlChars GetSqlChars(int ordinal) {
            EnsureCanGetCol( "GetSqlChars", ordinal);
            return ValueUtilsSmi.GetSqlChars(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], _currentConnection.InternalContext);
        }

        public override SqlBytes GetSqlBytes(int ordinal) {
            EnsureCanGetCol( "GetSqlBytes", ordinal);
            return ValueUtilsSmi.GetSqlBytes(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], _currentConnection.InternalContext);
        }

        public override SqlXml GetSqlXml(int ordinal) {
            EnsureCanGetCol( "GetSqlXml", ordinal);
            return ValueUtilsSmi.GetSqlXml(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], _currentConnection.InternalContext);
        }

        public override TimeSpan GetTimeSpan(int ordinal) {
            EnsureCanGetCol("GetTimeSpan", ordinal);
            return ValueUtilsSmi.GetTimeSpan(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], _currentConnection.IsKatmaiOrNewer);
        }

        public override DateTimeOffset GetDateTimeOffset(int ordinal) {
            EnsureCanGetCol("GetDateTimeOffset", ordinal);
            return ValueUtilsSmi.GetDateTimeOffset(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], _currentConnection.IsKatmaiOrNewer);
        }

        public override object GetSqlValue(int ordinal) {
            EnsureCanGetCol( "GetSqlValue", ordinal);

            SmiMetaData metaData = _currentMetaData[ordinal];
            if (_currentConnection.IsKatmaiOrNewer) {
                return ValueUtilsSmi.GetSqlValue200(_readerEventSink, (SmiTypedGetterSetter)_currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext);
            }
            return ValueUtilsSmi.GetSqlValue(_readerEventSink, _currentColumnValuesV3, ordinal, metaData, _currentConnection.InternalContext); ;
        }

        public override int GetSqlValues(object[] values) {
            EnsureCanGetCol( "GetSqlValues", 0);

            if (null == values) {
                throw ADP.ArgumentNull("values");
            }

            int copyLength = (values.Length < _visibleColumnCount) ? values.Length : _visibleColumnCount;
            for(int i=0; i<copyLength; i++) {
                values[_indexMap[i]] = GetSqlValue(i);
            }

            return copyLength;
        }

    //
    //    ISqlReader methods/properties
    //
        public override bool HasRows {
            get {return _hasRows;}
        }

    //
    //    SqlDataReader method/properties
    //
        public override Stream GetStream(int ordinal) {
            EnsureCanGetCol("GetStream", ordinal);

            SmiQueryMetaData metaData = _currentMetaData[ordinal];

            // For non-null, non-variant types with sequential access, we support proper streaming
            if ((metaData.SqlDbType != SqlDbType.Variant) && (IsCommandBehavior(CommandBehavior.SequentialAccess)) && (!ValueUtilsSmi.IsDBNull(_readerEventSink, _currentColumnValuesV3, ordinal))) {
                if (HasActiveStreamOrTextReaderOnColumn(ordinal)) {
                    throw ADP.NonSequentialColumnAccess(ordinal, ordinal + 1);
                }
                _currentStream = ValueUtilsSmi.GetSequentialStream(_readerEventSink, _currentColumnValuesV3, ordinal, metaData);
                return _currentStream;
            }
            else {
                return ValueUtilsSmi.GetStream(_readerEventSink, _currentColumnValuesV3, ordinal, metaData);
            }
        }

        public override TextReader GetTextReader(int ordinal) {
            EnsureCanGetCol("GetTextReader", ordinal);
            
            SmiQueryMetaData metaData = _currentMetaData[ordinal];

            // For non-variant types with sequential access, we support proper streaming
            if ((metaData.SqlDbType != SqlDbType.Variant) && (IsCommandBehavior(CommandBehavior.SequentialAccess)) && (!ValueUtilsSmi.IsDBNull(_readerEventSink, _currentColumnValuesV3, ordinal))) {
                if (HasActiveStreamOrTextReaderOnColumn(ordinal)) {
                    throw ADP.NonSequentialColumnAccess(ordinal, ordinal + 1);
                }
                _currentTextReader = ValueUtilsSmi.GetSequentialTextReader(_readerEventSink, _currentColumnValuesV3, ordinal, metaData);
                return _currentTextReader;
            }
            else {
                return ValueUtilsSmi.GetTextReader(_readerEventSink, _currentColumnValuesV3, ordinal, metaData);
            }
        }

        public override XmlReader GetXmlReader(int ordinal) {
            // NOTE: sql_variant can not contain a XML data type: http://msdn.microsoft.com/en-us/library/ms173829.aspx
            
            EnsureCanGetCol("GetXmlReader", ordinal);
            if (_currentMetaData[ordinal].SqlDbType != SqlDbType.Xml) {
                throw ADP.InvalidCast();
            }

            Stream stream = null;
            if ((IsCommandBehavior(CommandBehavior.SequentialAccess)) && (!ValueUtilsSmi.IsDBNull(_readerEventSink, _currentColumnValuesV3, ordinal))) {
                if (HasActiveStreamOrTextReaderOnColumn(ordinal)) {
                    throw ADP.NonSequentialColumnAccess(ordinal, ordinal + 1);
                }
                // Need to bypass the type check since streams are not usually allowed on XML types
                _currentStream = ValueUtilsSmi.GetSequentialStream(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], bypassTypeCheck: true);
                stream = _currentStream;
            }
            else {
                stream = ValueUtilsSmi.GetStream(_readerEventSink, _currentColumnValuesV3, ordinal, _currentMetaData[ordinal], bypassTypeCheck: true);
            }

            return SqlXml.CreateSqlXmlReader(stream);
        }

    //
    //    Internal reader state
    //

        // Logical state of reader/resultset as viewed by the client
        //    Does not necessarily match up with server state.
        internal enum PositionState
            {
            BeforeResults,            // Before all resultset in request
            BeforeRows,                // Before all rows in current resultset
            OnRow,                    // On a valid row in the current resultset
            AfterRows,                // After all rows in current resultset
            AfterResults            // After all resultsets in request
            }
        private PositionState _currentPosition;        // Where is the reader relative to incoming results?


    //
    //    Fields
    //
        private bool                        _isOpen;                    // Is the reader open?
        private SmiQueryMetaData[]          _currentMetaData;           // Metadata for current resultset
        private int[]                       _indexMap;                  // map of indices for visible column
        private int                         _visibleColumnCount;        // number of visible columns
        private DataTable                   _schemaTable;               // Cache of user-visible extended metadata while in results.
        private ITypedGetters               _currentColumnValues;       // Unmanaged-managed data marshalers/cache
        private ITypedGettersV3             _currentColumnValuesV3;     // Unmanaged-managed data marshalers/cache for SMI V3
        private bool                        _hasRows;                   // Are there any rows in the current resultset?  Must be able to say before moving to first row.
        private SmiEventStream              _eventStream;               // The event buffer that receives the events from the execution engine.
        private SmiRequestExecutor          _requestExecutor;           // The used to request actions from the execution engine.
        private SqlInternalConnectionSmi    _currentConnection;
        private ReaderEventSink             _readerEventSink;           // The event sink that will process events from the event buffer.
        private FieldNameLookup             _fieldNameLookup;           // cached lookup object to improve access time based on field name
        private SqlSequentialStreamSmi      _currentStream;             // The stream on the current column (if any)
        private SqlSequentialTextReaderSmi  _currentTextReader;         // The text reader on the current column (if any)

    //
    // Internal methods for use by other classes in project
    //
        // Constructor
        //
        //  Assumes that if there were any results, the first chunk of them are in the data stream
        //      (up to the first actual row or the end of the resultsets).
        unsafe internal SqlDataReaderSmi (
                SmiEventStream              eventStream,        // the event stream that receives the events from the execution engine
                SqlCommand                  parent,             // command that owns reader
                CommandBehavior             behavior,           // behavior specified for this execution
                SqlInternalConnectionSmi    connection,         // connection that owns everybody
                SmiEventSink                parentSink,         // Event sink of parent command
                SmiRequestExecutor          requestExecutor
            ) : base( parent, behavior ) {  // 
            _eventStream = eventStream;
            _currentConnection = connection;
            _readerEventSink = new ReaderEventSink( this, parentSink );
            _currentPosition = PositionState.BeforeResults;
            _isOpen = true;
            _indexMap = null;
            _visibleColumnCount = 0;
            _currentStream = null;
            _currentTextReader = null;
            _requestExecutor = requestExecutor;
        }

        internal override SmiExtendedMetaData[] GetInternalSmiMetaData() {
            if (null == _currentMetaData || _visibleColumnCount == this.InternalFieldCount) {
                return _currentMetaData;
            }
            else {
#if DEBUG
                // DEVNOTE: Interpretation of returned array currently depends on hidden columns
                //  always appearing at the end, since there currently is no access to the index map
                //  outside of this class.  In Debug code, we check this assumption.
                bool sawHiddenColumn = false;
#endif
                SmiExtendedMetaData[] visibleMetaData = new SmiExtendedMetaData[_visibleColumnCount];
                for(int i=0; i<_visibleColumnCount; i++) {
#if DEBUG
                    if (_currentMetaData[_indexMap[i]].IsHidden.IsTrue) {
                        sawHiddenColumn = true;
                    }
                    else {
                        Debug.Assert(!sawHiddenColumn);
                    }
#endif
                    visibleMetaData[i] = _currentMetaData[_indexMap[i]];
                }

                return visibleMetaData;
            }
        }

        internal override int GetLocaleId(int ordinal) {
            EnsureCanGetMetaData( "GetLocaleId" );
            return (int)_currentMetaData[ordinal].LocaleId;
        }
    
        //
        // Private implementation methods
        //

        private int InternalFieldCount {
            get {
                if ( FNotInResults() ) {
                    return 0;
                }
                else {
                    return _currentMetaData.Length;
                }
            }
        }

        // Have we cleaned up internal resources?
        private bool IsReallyClosed() {
            return !_isOpen;
        }

        // Central checkpoint for closed recordset.
        //    Any code that requires an open recordset should call this method first!
        //    Especially any code that accesses unmanaged memory structures whose lifetime
        //      matches the lifetime of the unmanaged recordset.
        internal void ThrowIfClosed( string operationName ) {
            if (IsClosed)
                throw ADP.DataReaderClosed( operationName );
        }

        // Central checkpoint to ensure the requested column can be accessed.
        //    Calling this function serves to notify that it has been accessed by the user.
        [SuppressMessage("Microsoft.Performance", "CA1801:AvoidUnusedParameters")] // for future compatibility
        private void EnsureCanGetCol( string operationName, int ordinal) {
            EnsureOnRow( operationName );
        }

        internal void EnsureOnRow( string operationName ) {
            ThrowIfClosed( operationName );
            if (_currentPosition != PositionState.OnRow) {
                throw SQL.InvalidRead();
            }
        }

        internal void EnsureCanGetMetaData( string operationName ) {
            ThrowIfClosed( operationName );
            if (FNotInResults()) {
                throw SQL.InvalidRead(); // 
            }
        }

        private bool FInResults() {
            return !FNotInResults();
        }

        private bool FNotInResults() {
            return (PositionState.AfterResults == _currentPosition || PositionState.BeforeResults == _currentPosition);
        }

        private void MetaDataAvailable( SmiQueryMetaData[] md, bool nextEventIsRow ) {
            Debug.Assert( _currentPosition != PositionState.AfterResults );
            
            _currentMetaData = md;
            _hasRows = nextEventIsRow;
            _fieldNameLookup = null;
            _schemaTable = null; // will be rebuilt based on new metadata
            _currentPosition = PositionState.BeforeRows;

            // calculate visible column indices
            _indexMap = new int[_currentMetaData.Length];
            int i;
            int visibleCount = 0;
            for(i=0; i<_currentMetaData.Length; i++) {
                if (!_currentMetaData[i].IsHidden.IsTrue) {
                    _indexMap[visibleCount] = i;
                    visibleCount++;
                }
            }
            _visibleColumnCount = visibleCount;
        }

        private bool HasActiveStreamOrTextReaderOnColumn(int columnIndex) {
            bool active = false;

            active |= ((_currentStream != null) && (_currentStream.ColumnIndex == columnIndex));
            active |= ((_currentTextReader != null) && (_currentTextReader.ColumnIndex == columnIndex));

            return active;
        }

        // Obsolete V2- method
        private void RowAvailable( ITypedGetters row ) {
            Debug.Assert( _currentPosition != PositionState.AfterResults );

            _currentColumnValues = row;
            _currentPosition = PositionState.OnRow;
        }

        private void RowAvailable( ITypedGettersV3 row ) {
            Debug.Assert( _currentPosition != PositionState.AfterResults );

            _currentColumnValuesV3 = row;
            _currentPosition = PositionState.OnRow;
        }

        private void StatementCompleted( ) {
            Debug.Assert( _currentPosition != PositionState.AfterResults );

            _currentPosition = PositionState.AfterRows;
        }

        private void ResetResultSet() {
            _currentMetaData = null;
            _visibleColumnCount = 0;
            _schemaTable = null;
        }

        private void BatchCompleted() {
            Debug.Assert( _currentPosition != PositionState.AfterResults );

            ResetResultSet();

            _currentPosition = PositionState.AfterResults;
            _eventStream.Close( _readerEventSink );
        }

        // An implementation of the IEventSink interface that either performs
        // the required enviornment changes or forwards the events on to the
        // corresponding reader instance. Having the event sink be a separate
        // class keeps the IEventSink methods out of SqlDataReader's inteface.

        private sealed class ReaderEventSink : SmiEventSink_Default {
            private readonly SqlDataReaderSmi reader;

            internal ReaderEventSink( SqlDataReaderSmi reader, SmiEventSink parent )
                : base( parent ) {
                this.reader = reader;
            }

            internal override void MetaDataAvailable( SmiQueryMetaData[] md, bool nextEventIsRow ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.MetaDataAvailable|ADV> %d#, md.Length=%d nextEventIsRow=%d.\n", reader.ObjectID, (null != md) ? md.Length : -1, nextEventIsRow);
                
                    if (null != md) {
                        for (int i=0; i < md.Length; i++) {
                            Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.MetaDataAvailable|ADV> %d#, metaData[%d] is %ls%ls\n",
                                            reader.ObjectID, i, md[i].GetType().ToString(), md[i].TraceString());
                        }
                    }
                }
                this.reader.MetaDataAvailable( md, nextEventIsRow );
            }

            // Obsolete V2- method
            internal override void RowAvailable( ITypedGetters row ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (v2).\n", reader.ObjectID);
                }
                this.reader.RowAvailable( row );
            }

            internal override void RowAvailable( ITypedGettersV3 row ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (ITypedGettersV3).\n", reader.ObjectID);
                }
                this.reader.RowAvailable( row );
            }

            internal override void RowAvailable(SmiTypedGetterSetter rowData) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.RowAvailable|ADV> %d# (SmiTypedGetterSetter).\n", reader.ObjectID);
                }
                this.reader.RowAvailable(rowData);
            }

            internal override void StatementCompleted( int recordsAffected ) {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.StatementCompleted|ADV> %d# recordsAffected=%d.\n", reader.ObjectID, recordsAffected);
                }

                // devnote: relies on SmiEventSink_Default to pass event to parent
                // Both command and reader care about StatementCompleted, but for different reasons.

                base.StatementCompleted( recordsAffected );
                this.reader.StatementCompleted( );
            }

            internal override void BatchCompleted() {
                if (Bid.AdvancedOn) {
                    Bid.Trace("<sc.SqlDataReaderSmi.ReaderEventSink.BatchCompleted|ADV> %d#.\n", reader.ObjectID);
                }

                // devnote: relies on SmiEventSink_Default to pass event to parent
                //  parent's callback *MUST* come before reader's BatchCompleted, since
                //  reader will close the event stream during this call, and parent wants
                //  to extract parameter values before that happens.

                base.BatchCompleted();
                this.reader.BatchCompleted();
            }
        }
        
    }
}

