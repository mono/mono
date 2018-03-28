//------------------------------------------------------------------------------
// <copyright file="BridgeDataReader.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Query.ResultAssembly {

    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Data.Query.InternalTrees;
    using System.Data.Query.PlanCompiler;
    using System.Diagnostics;

    /// <summary>
    /// DbDataReader functionality for the bridge.
    /// </summary>
    internal sealed class BridgeDataReader : DbDataReader, IExtendedDataRecord {

        #region private state

        /// <summary>
        /// Object that holds the state needed by the coordinator and the root enumerator
        /// </summary>
        private Shaper<RecordState> Shaper;

        /// <summary>
        /// Enumerator over shapers for NextResult() calls. 
        /// Null for nested data readers (depth > 0);
        /// </summary>
        private IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> NextResultShaperInfoEnumerator;

        /// <summary>
        /// The coordinator we're responsible for returning results for.
        /// </summary>
        private CoordinatorFactory<RecordState> CoordinatorFactory;

        /// <summary>
        /// The default record (pre-read/past-end) state
        /// </summary>
        private RecordState DefaultRecordState;

        /// <summary>
        /// We delegate to this on our getters, to avoid duplicate code.
        /// </summary>
        private BridgeDataRecord DataRecord;

        /// <summary>
        /// Do we have a row to read?  Determined in the constructor and
        /// should not be changed.
        /// </summary>
        private bool _hasRows;

        /// <summary>
        /// Set to true only when we've been closed through the Close() method
        /// </summary>
        private bool _isClosed;

        #endregion

        #region constructors

        /// <summary>
        /// Constructor used by the ResultColumn when doing GetValue, and by the Create factory
        /// method.
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="isRoot"></param>
        /// <param name="nextResultShaperInfos">enumrator of the shapers for NextResult() calls</param>
        internal BridgeDataReader(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth, IEnumerator<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> nextResultShaperInfos)
            : base() {
            Debug.Assert(null != shaper, "null shaper?");
            Debug.Assert(null != coordinatorFactory, "null coordinatorFactory?");
            Debug.Assert(depth == 0 || nextResultShaperInfos == null, "Nested data readers should not have multiple result sets.");

            NextResultShaperInfoEnumerator = nextResultShaperInfos != null ? nextResultShaperInfos : null;
            SetShaper(shaper, coordinatorFactory, depth);
        }

        private void SetShaper(Shaper<RecordState> shaper, CoordinatorFactory<RecordState> coordinatorFactory, int depth)
        {
            Shaper = shaper;
            CoordinatorFactory = coordinatorFactory;
            DataRecord = new BridgeDataRecord(shaper, depth);

            // To determine whether there are any rows for this coordinator at this place in 
            // the root enumerator, we pretty much just look at it's current record (we'll read 
            // one if there isn't one waiting) and if it matches our coordinator, we've got rows.
            _hasRows = false;

            if (!Shaper.DataWaiting) {
                Shaper.DataWaiting = Shaper.RootEnumerator.MoveNext();
            }
            if (Shaper.DataWaiting) {
                RecordState currentRecord = Shaper.RootEnumerator.Current;

                if (null != currentRecord) {
                    _hasRows = (currentRecord.CoordinatorFactory == CoordinatorFactory);
                }
            }

            // Once we've created the root enumerator, we can get the default record state
            DefaultRecordState = coordinatorFactory.GetDefaultRecordState(Shaper);
            Debug.Assert(null != DefaultRecordState, "no default?");
        }

        /// <summary>
        /// The primary factory method to produce the BridgeDataReader; given a store data 
        /// reader and a column map, create the BridgeDataReader, hooking up the IteratorSources  
        /// and ResultColumn Hierarchy.  All construction of top level data readers go through
        /// this method.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="columnMap">column map of the first result set</param>
        /// <param name="nextResultColumnMaps">enumerable of the column maps for NextResult() calls.</param>
        /// <returns></returns>
        static internal DbDataReader Create(DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps) {
            Debug.Assert(storeDataReader != null, "null storeDataReaders?");
            Debug.Assert(columnMap != null, "null columnMap?");
            Debug.Assert(workspace != null, "null workspace?");

            var shaperInfo = CreateShaperInfo(storeDataReader, columnMap, workspace);
            DbDataReader result = new BridgeDataReader(shaperInfo.Key, shaperInfo.Value, /*depth:*/ 0, GetNextResultShaperInfo(storeDataReader, workspace, nextResultColumnMaps).GetEnumerator());
            return result;
        }

        private static KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>> CreateShaperInfo(DbDataReader storeDataReader, ColumnMap columnMap, MetadataWorkspace workspace)
        {
            Debug.Assert(storeDataReader != null, "null storeDataReaders?");
            Debug.Assert(columnMap != null, "null columnMap?");
            Debug.Assert(workspace != null, "null workspace?");

            System.Data.Common.QueryCache.QueryCacheManager cacheManager = workspace.GetQueryCacheManager();
            const System.Data.Objects.MergeOption NoTracking = System.Data.Objects.MergeOption.NoTracking;

            ShaperFactory<RecordState> shaperFactory = Translator.TranslateColumnMap<RecordState>(cacheManager, columnMap, workspace, null, NoTracking, true);
            Shaper<RecordState> recordShaper = shaperFactory.Create(storeDataReader, null, workspace, System.Data.Objects.MergeOption.NoTracking, true);

            return new KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>(recordShaper, recordShaper.RootCoordinator.TypedCoordinatorFactory);
        }

        private static IEnumerable<KeyValuePair<Shaper<RecordState>, CoordinatorFactory<RecordState>>> GetNextResultShaperInfo(DbDataReader storeDataReader, MetadataWorkspace workspace, IEnumerable<ColumnMap> nextResultColumnMaps)
        {
            foreach (var nextResultColumnMap in nextResultColumnMaps)
            {
                yield return CreateShaperInfo(storeDataReader, nextResultColumnMap, workspace);
            }
        }

        #endregion

        #region helpers

        /// <summary>
        /// Implicitly close this (nested) data reader; will be called whenever 
        /// the user has done a GetValue() or a Read() on a parent reader/record
        /// to ensure that we consume all our results.  We do that because we 
        /// our design requires us to be positioned at the next nested reader's
        /// first row.
        /// </summary>
        internal void CloseImplicitly() {
            Consume();
            DataRecord.CloseImplicitly();
        }

        /// <summary>
        /// Reads to the end of the source enumerator provided
        /// </summary>
        private void Consume() {
            while (ReadInternal()) ;
        }

        /// <summary>
        /// Figure out the CLR type from the TypeMetadata object; For scalars, 
        /// we can get this from the metadata workspace, but for the rest, we 
        /// just guess at "Object".  You need to use the DataRecordInfo property 
        /// to get better information for those.
        /// </summary>
        /// <param name="typeUsage"></param>
        /// <returns></returns>
        internal static Type GetClrTypeFromTypeMetadata(TypeUsage typeUsage) {
            Type result;

            PrimitiveType primitiveType;
            if (TypeHelpers.TryGetEdmType<PrimitiveType>(typeUsage, out primitiveType)) {
                result = primitiveType.ClrEquivalentType;
            }
            else {
                if (TypeSemantics.IsReferenceType(typeUsage)) {
                    result = typeof(EntityKey);
                }
                else if (TypeUtils.IsStructuredType(typeUsage)) {
                    result = typeof(DbDataRecord);
                }
                else if (TypeUtils.IsCollectionType(typeUsage)) {
                    result = typeof(DbDataReader);
                }
                else if (TypeUtils.IsEnumerationType(typeUsage)) {
                    result = ((EnumType)typeUsage.EdmType).UnderlyingType.ClrEquivalentType;
                }
                else {
                    result = typeof(object);
                }
            }
            return result;
        }

        #endregion

        #region data reader specific properties and methods

        /// <summary>
        /// implementation for DbDataReader.Depth property
        /// </summary>
        override public int Depth {
            get {
                AssertReaderIsOpen("Depth");
                return DataRecord.Depth;
            }
        }

        /// <summary>
        /// implementation for DbDataReader.HasRows property
        /// </summary>
        override public bool HasRows {
            get {
                AssertReaderIsOpen("HasRows");
                return _hasRows;
            }
        }

        /// <summary>
        /// implementation for DbDataReader.IsClosed property
        /// </summary>        
        override public bool IsClosed {
            get {
                // Rather that try and track this in two places; we just delegate
                // to the data record that we constructed; it has more reasons to 
                // have to know this than we do in the data reader.  (Of course, 
                // we look at our own closed state too...)
                return ((_isClosed) || DataRecord.IsClosed);
            }
        }

        /// <summary>
        /// implementation for DbDataReader.RecordsAffected property
        /// </summary>        
        override public int RecordsAffected {
            get {
                int result = -1; // For nested readers, return -1 which is the default for queries.

                // We defer to the store reader for rows affected count. Note that for queries,
                // the provider is generally expected to return -1.
                // 
                if (DataRecord.Depth == 0) {
                    result = Shaper.Reader.RecordsAffected;
                }
                return result;
            }
        }

        /// <summary>
        /// Ensures that the reader is actually open, and throws an exception if not
        /// </summary>
        private void AssertReaderIsOpen(string methodName) {
            if (IsClosed) {
                if (DataRecord.IsImplicitlyClosed) {
                    throw EntityUtil.ImplicitlyClosedDataReaderError();
                }
                if (DataRecord.IsExplicitlyClosed) {
                    throw EntityUtil.DataReaderClosed(methodName);
                }
            }
        }

        /// <summary>
        /// implementation for DbDataReader.Close() method
        /// </summary>
        override public void Close() {
            // Make sure we explicitly closed the data record, since that's what
            // where using to track closed state.
            DataRecord.CloseExplicitly();

            if (!_isClosed) {
                _isClosed = true;

                if (0 == DataRecord.Depth) {
                    // If we're the root collection, we want to ensure the remainder of
                    // the result column hierarchy is closed out, to avoid dangling
                    // references to it, should it be reused. We also want to physically 
                    // close out the source reader as well.
                    Shaper.Reader.Close();
                }
                else {
                    // For non-root collections, we have to consume all the data, or we'll
                    // not be positioned propertly for what comes afterward.
                    Consume();
                }
            }

            if (NextResultShaperInfoEnumerator != null)
            {
                NextResultShaperInfoEnumerator.Dispose();
                NextResultShaperInfoEnumerator = null;
            }
        }

        /// <summary>
        /// implementation for DbDataReader.GetEnumerator() method
        /// </summary>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        override public IEnumerator GetEnumerator() {
            IEnumerator result = new DbEnumerator((IDataReader)this, true); // We always want to close the reader; 
            return result;
        }

        /// <summary>
        /// implementation for DbDataReader.GetSchemaTable() method
        /// 
        /// This is awaiting some common code
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">GetSchemaTable is not supported at this time</exception>
        override public DataTable GetSchemaTable() {
            throw EntityUtil.NotSupported(System.Data.Entity.Strings.ADP_GetSchemaTableIsNotSupported);
        }

        /// <summary>
        /// implementation for DbDataReader.NextResult() method
        /// </summary>
        /// <returns></returns>
        override public bool NextResult() {
            AssertReaderIsOpen("NextResult");

            // If there is a next result set available, serve it.
            if (NextResultShaperInfoEnumerator != null && 
                Shaper.Reader.NextResult() &&
                NextResultShaperInfoEnumerator.MoveNext())
            {
                Debug.Assert(DataRecord.Depth == 0, "Nested data readers should not have multiple result sets.");
                var nextResultShaperInfo = NextResultShaperInfoEnumerator.Current;
                DataRecord.CloseImplicitly();
                SetShaper(nextResultShaperInfo.Key, nextResultShaperInfo.Value, depth: 0);
                return true;
            }

            if (0 == DataRecord.Depth) {
                // NOTE:: this is required to ensure that output parameter values 
                // are set in SQL Server, and other providers where they come after
                // the results.
                CommandHelper.ConsumeReader(Shaper.Reader);
            }
            else {
                // For nested readers, make sure we're positioned properly for 
                // the following columns...
                Consume();
            }

            // SQLBUDT #631726 - ensure we close the records that may be 
            // outstanding...
            // SQLBUDT #632494 - do this after we consume the underlying reader 
            // so we don't run result assembly through it...
            CloseImplicitly();

            // Reset any state on our attached data record, since we've now
            // gone past the end of the reader.
            DataRecord.SetRecordSource(null, false);

            return false;
        }

        /// <summary>
        /// implementation for DbDataReader.Read() method
        /// </summary>
        /// <returns></returns>
        override public bool Read() {
            AssertReaderIsOpen("Read");

            // First of all we need to inform each of the nested records that
            // have been returned that they're "implicitly" closed -- that is 
            // we've moved on.  This will also ensure that any records remaining
            // in any active nested readers are consumed
            DataRecord.CloseImplicitly();

            // OK, now go ahead and advance the source enumerator and set the 
            // record source up 
            bool result = ReadInternal();
            DataRecord.SetRecordSource(Shaper.RootEnumerator.Current, result);
            return result;
        }

        /// <summary>
        /// Internal read method; does the work of advancing the root enumerator
        /// as needed and determining whether it's current record is for our
        /// coordinator.  The public Read method does the assertions and such that
        /// we don't want to do when we're called from internal methods to do things
        /// like consume the rest of the reader's contents.
        /// </summary>
        /// <param name="rootEnumerator"></param>
        /// <returns></returns>
        private bool ReadInternal() {
            bool result = false;

            // If there's nothing waiting for the root enumerator, then attempt
            // to advance it. 
            if (!Shaper.DataWaiting) {
                Shaper.DataWaiting = Shaper.RootEnumerator.MoveNext();
            }

            // If we have some data (we may have just read it above) then figure
            // out who it belongs to-- us or someone else. We also skip over any
            // records that are for our children (nested readers); if we're being
            // asked to read, it's too late for them to read them.
            while (Shaper.DataWaiting
                        && Shaper.RootEnumerator.Current.CoordinatorFactory != CoordinatorFactory
                        && Shaper.RootEnumerator.Current.CoordinatorFactory.Depth > CoordinatorFactory.Depth) {
                Shaper.DataWaiting = Shaper.RootEnumerator.MoveNext();
            }

            if (Shaper.DataWaiting) {
                // We found something, go ahead and indicate to the shaper we want 
                // this record, set up the data record, etc.
                if (Shaper.RootEnumerator.Current.CoordinatorFactory == CoordinatorFactory) {
                    Shaper.DataWaiting = false;
                    Shaper.RootEnumerator.Current.AcceptPendingValues();
                    result = true;
                }
            }
            return result;
        }

        #endregion

        #region metadata properties and methods

        /// <summary>
        /// implementation for DbDataReader.DataRecordInfo property
        /// </summary>
        public DataRecordInfo DataRecordInfo {
            get {
                AssertReaderIsOpen("DataRecordInfo");

                DataRecordInfo result;
                if (DataRecord.HasData) {
                    result = DataRecord.DataRecordInfo;
                }
                else {
                    result = DefaultRecordState.DataRecordInfo;
                }
                return result;
            }
        }

        /// <summary>
        /// implementation for DbDataReader.FieldCount property
        /// </summary>
        override public int FieldCount {
            get {
                AssertReaderIsOpen("FieldCount");

                // In this method, we need to return a constant value, regardless
                // of how polymorphic the result is, because there is a lot of code
                // in the wild that expects it to be constant; Ideally, we'd return
                // the number of columns in the actual type that we have, but since
                // that would probably break folks, I'm leaving it at returning the
                // base set of columns that all rows will have.

                int result = DefaultRecordState.ColumnCount;
                return result;
            }
        }

        /// <summary>
        /// implementation for DbDataReader.GetDataTypeName() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public string GetDataTypeName(int ordinal) {
            AssertReaderIsOpen("GetDataTypeName");
            string result;
            if (DataRecord.HasData) {
                result = DataRecord.GetDataTypeName(ordinal);
            }
            else {
                result = TypeHelpers.GetFullName(DefaultRecordState.GetTypeUsage(ordinal));
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataReader.GetFieldType() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Type GetFieldType(int ordinal) {
            AssertReaderIsOpen("GetFieldType");
            Type result;
            if (DataRecord.HasData) {
                result = DataRecord.GetFieldType(ordinal);
            }
            else {
                result = GetClrTypeFromTypeMetadata(DefaultRecordState.GetTypeUsage(ordinal));
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataReader.GetName() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public string GetName(int ordinal) {
            AssertReaderIsOpen("GetName");
            string result;
            if (DataRecord.HasData) {
                result = DataRecord.GetName(ordinal);
            }
            else {
                result = DefaultRecordState.GetName(ordinal);
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataReader.GetOrdinal() method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        override public int GetOrdinal(string name) {
            AssertReaderIsOpen("GetOrdinal");
            int result;
            if (DataRecord.HasData) {
                result = DataRecord.GetOrdinal(name);
            }
            else {
                result = DefaultRecordState.GetOrdinal(name);
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataReader.GetProviderSpecificFieldType() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">GetProviderSpecificFieldType is not supported at this time</exception>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        override public Type GetProviderSpecificFieldType(int ordinal) {
            throw EntityUtil.NotSupported();
        }

        #endregion

        #region data record properties and methods

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        //
        // The remaining methods on this class delegate to the inner data record
        //
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////

        #region general getter methods and indexer properties

        /// <summary>
        /// implementation for DbDataReader[ordinal] indexer value getter
        /// </summary>
        override public object this[int ordinal] {
            get {
                return DataRecord[ordinal];
            }
        }

        /// <summary>
        /// implementation for DbDataReader[name] indexer value getter
        /// </summary>
        override public object this[string name] {
            get {
                int ordinal = GetOrdinal(name);
                return DataRecord[ordinal];
            }
        }

        /// <summary>
        /// implementation for DbDataReader.GetProviderSpecificValue() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">GetProviderSpecificValue is not supported at this time</exception>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public override object GetProviderSpecificValue(int ordinal) {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// implementation for DbDataReader.GetProviderSpecificValues() method
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">GetProviderSpecificValues is not supported at this time</exception>
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public override int GetProviderSpecificValues(object[] values) {
            throw EntityUtil.NotSupported();
        }

        /// <summary>
        /// implementation for DbDataReader.GetValue() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Object GetValue(int ordinal) {
            return DataRecord.GetValue(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetValues() method
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        override public int GetValues(object[] values) {
            return DataRecord.GetValues(values);
        }

        #endregion

        #region simple scalar value getter methods

        /// <summary>
        /// implementation for DbDataReader.GetBoolean() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public bool GetBoolean(int ordinal) {
            return DataRecord.GetBoolean(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetByte() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public byte GetByte(int ordinal) {
            return DataRecord.GetByte(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetChar() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public char GetChar(int ordinal) {
            return DataRecord.GetChar(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetDateTime() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public DateTime GetDateTime(int ordinal) {
            return DataRecord.GetDateTime(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetDecimal() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Decimal GetDecimal(int ordinal) {
            return DataRecord.GetDecimal(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetDouble() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public double GetDouble(int ordinal) {
            return DataRecord.GetDouble(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetFloat() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public float GetFloat(int ordinal) {
            return DataRecord.GetFloat(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetGuid() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Guid GetGuid(int ordinal) {
            return DataRecord.GetGuid(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetInt16() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int16 GetInt16(int ordinal) {
            return DataRecord.GetInt16(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetInt32() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int32 GetInt32(int ordinal) {
            return DataRecord.GetInt32(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetInt64() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int64 GetInt64(int ordinal) {
            return DataRecord.GetInt64(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetString() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public String GetString(int ordinal) {
            return DataRecord.GetString(ordinal);
        }


        /// <summary>
        /// implementation for DbDataReader.IsDBNull() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public bool IsDBNull(int ordinal) {
            return DataRecord.IsDBNull(ordinal);
        }

        #endregion

        #region array scalar value getter methods

        /// <summary>
        /// implementation for DbDataReader.GetBytes() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        override public long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
            return DataRecord.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        /// <summary>
        /// implementation for DbDataReader.GetChars() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        override public long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
            return DataRecord.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        #endregion

        #region complex type getters

        /// <summary>
        /// implementation for DbDataReader.GetData() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override protected DbDataReader GetDbDataReader(int ordinal) {
            return (DbDataReader)DataRecord.GetData(ordinal);
        }

        /// <summary>
        /// implementation for DbDataReader.GetDataRecord() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public DbDataRecord GetDataRecord(int ordinal) {
            return DataRecord.GetDataRecord(ordinal);
        }

        /// <summary>
        /// Used to return a nested result
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public DbDataReader GetDataReader(int ordinal) {
            return this.GetDbDataReader(ordinal);
        }

        #endregion

        #endregion
    }
}
