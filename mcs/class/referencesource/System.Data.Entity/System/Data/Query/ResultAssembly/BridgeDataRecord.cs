//------------------------------------------------------------------------------
// <copyright file="BridgeDataRecord.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Query.ResultAssembly {

    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Internal.Materialization;
    using System.Data.Metadata.Edm;
    using System.Data.Query.PlanCompiler;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;

    /// <summary>
    /// DbDataRecord functionality for the bridge.
    /// </summary>
    sealed internal class BridgeDataRecord : DbDataRecord, IExtendedDataRecord {

        #region state

        /// <summary>
        /// How deep down the hierarchy are we?
        /// </summary>
        internal readonly int Depth;

        /// <summary>
        /// Where the data comes from
        /// </summary>
        private readonly Shaper<RecordState> Shaper;

        /// <summary>
        /// The current record that we're responsible for; this will change from row to row
        /// on the source data reader.  Will be set to null when parent the enumerator has  
        /// returned false.
        /// </summary>
        private RecordState _source;

        /// <summary>
        /// Current state of the record; 
        /// </summary>
        private Status _status;
        private enum Status {
            Open = 0,
            ClosedImplicitly = 1,
            ClosedExplicitly = 2,
        };

        /// <summary>
        /// the column ordinal of the last column read, used to enforce sequential access
        /// </summary>
        private int _lastColumnRead;

        /// <summary>
        /// the last data offset of a read returned, used to enforce sequential access
        /// </summary>
        private long _lastDataOffsetRead;

        /// <summary>
        /// the last ordinal that IsDBNull was called for; used to avoid re-reading the value; 
        /// </summary>
        private int _lastOrdinalCheckedForNull;

        /// <summary>
        /// value, of the last column that IsDBNull was called for; used to avoid re-reading the value; 
        /// </summary>
        private object _lastValueCheckedForNull;
        
        /// <summary>
        /// Set to the current data record when we hand them out.  (For data reader columns,
        /// we use it's attached data record) The Close, GetValue and Read methods ensures 
        /// that this is implicitly closed when we move past it.
        /// </summary>
        private BridgeDataReader _currentNestedReader;
        private BridgeDataRecord _currentNestedRecord;

        #endregion

        #region constructors

        internal BridgeDataRecord(Shaper<RecordState> shaper, int depth)
            : base() {
            Debug.Assert(null != shaper, "null shaper?");
            Shaper = shaper;
            Depth = depth;
            // Rest of state is set through the SetRecordSource method.
        }

        #endregion

        #region state management

        /// <summary>
        /// Called by our owning datareader when it is explicitly closed; will
        /// not be called for nested structures, they go through the ClosedImplicitly.
        /// path instead.
        /// </summary>
        internal void CloseExplicitly() {
            _status = Status.ClosedExplicitly;
            _source = null; // can't have data any longer once we're closed.
            CloseNestedObjectImplicitly();
        }

        /// <summary>
        /// Called by our parent object to ensure that we're marked as implicitly 
        /// closed;  will not be called for root level data readers.
        /// </summary>
        internal void CloseImplicitly() {
            _status = Status.ClosedImplicitly;
            _source = null; // can't have data any longer once we're closed.
            CloseNestedObjectImplicitly();
        }

        /// <summary>
        /// Ensure that whatever column we're currently processing is implicitly closed;
        /// </summary>
        private void CloseNestedObjectImplicitly() {
            // it would be nice to use Interlocked.Exchange to avoid multi-thread `race condition risk
            // when the the bridge is being misused by the user accessing it with multiple threads.
            // but this is called frequently enough to have a performance impact
            BridgeDataRecord currentNestedRecord = _currentNestedRecord;
            if (null != currentNestedRecord) {
                _currentNestedRecord = null;
                currentNestedRecord.CloseImplicitly();
            }
            BridgeDataReader currentNestedReader = _currentNestedReader;
            if (null != currentNestedReader) {
                _currentNestedReader = null;
                currentNestedReader.CloseImplicitly();
            }
        }

        /// <summary>
        /// Should be called after each Read on the data reader.
        /// </summary>
        internal void SetRecordSource(RecordState newSource, bool hasData) {
            Debug.Assert(null == _currentNestedRecord, "didn't close the nested record?");
            Debug.Assert(null == _currentNestedReader, "didn't close the nested reader?");

            // A peculiar behavior of IEnumerator is that when MoveNext() returns
            // false, the Current still points to the last value, which is not 
            // what we really want to reflect here.
            if (hasData) {
                Debug.Assert(null != newSource, "hasData but null newSource?"); // this shouldn't happen...
                _source = newSource;
            }
            else {
                _source = null;
            }
            _status = Status.Open;

            _lastColumnRead = -1;
            _lastDataOffsetRead = -1;
            _lastOrdinalCheckedForNull = -1;
            _lastValueCheckedForNull = null;
        }

        #endregion

        #region assertion helpers

        /// <summary>
        /// Ensures that the reader is actually open, and throws an exception if not
        /// </summary>
        private void AssertReaderIsOpen() {
            if (IsExplicitlyClosed) {
                throw EntityUtil.ClosedDataReaderError();
            }
            if (IsImplicitlyClosed) {
                throw EntityUtil.ImplicitlyClosedDataReaderError();
            }
        }

        /// <summary>
        /// Helper method.
        /// </summary>
        private void AssertReaderIsOpenWithData() {
            AssertReaderIsOpen();

            if (!HasData) {
                throw EntityUtil.NoData();
            }
        }

        /// <summary>
        /// Ensures that sequential access rules are being obeyed for non-array
        /// getter methods, throws the appropriate exception if not.  Also ensures
        /// that the last column and array offset is set appropriately.
        /// </summary>
        /// <param name="ordinal"></param>
        private void AssertSequentialAccess(int ordinal) {
            Debug.Assert(null != _source, "null _source?"); // we should have already called AssertReaderIsOpen.

            if (ordinal < 0 || ordinal >= _source.ColumnCount) {
                throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
            if (_lastColumnRead >= ordinal) {
                throw EntityUtil.NonSequentialColumnAccess(ordinal, _lastColumnRead + 1);
            }
            _lastColumnRead = ordinal;
            // SQLBUDT #442001 -- we need to mark things that are not using GetBytes/GetChars
            //                    in a way that prevents them from being read a second time 
            //                    using those methods.  Pointing past any potential data is
            //                    how we do that.
            _lastDataOffsetRead = long.MaxValue;
        }

        /// <summary>
        /// Ensures that sequential access rules are being obeyed for array offset
        /// getter methods, throws the appropriate exception if not.  Also ensures
        /// that the last column and array offset is set appropriately.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="methodName"></param>
        private void AssertSequentialAccess(int ordinal, long dataOffset, string methodName) {
            Debug.Assert(null != _source, "null _source?"); // we should have already called AssertReaderIsOpen.

            if (ordinal < 0 || ordinal >= _source.ColumnCount) {
                throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
            if (_lastColumnRead > ordinal || (_lastColumnRead == ordinal && _lastDataOffsetRead == long.MaxValue)) {
                throw EntityUtil.NonSequentialColumnAccess(ordinal, _lastColumnRead + 1);
            }
            if (_lastColumnRead == ordinal) {
                if (_lastDataOffsetRead >= dataOffset) {
                    throw EntityUtil.NonSequentialArrayOffsetAccess(dataOffset, _lastDataOffsetRead + 1, methodName);
                }
                // _lastDataOffsetRead will be set by GetBytes/GetChars, since we need to set it
                // to the last offset that was actually read, which isn't necessarily what was 
                // requested.
            }
            else {
                // Doin' a new thang...
                _lastColumnRead = ordinal;
                _lastDataOffsetRead = -1;
            }
        }

        /// <summary>
        /// True when the record has data (SetRecordSource was called with true)
        /// </summary>
        internal bool HasData {
            get {
                bool result = (_source != null);
                return result;
            }
        }

        /// <summary>
        /// True so long as we haven't been closed either implicity or explictly
        /// </summary>
        internal bool IsClosed {
            get {
                return (_status != Status.Open);
            }
        }

        /// <summary>
        /// Determine whether we have been explicitly closed by our owning 
        /// data reader; only data records that are responsible for processing 
        /// data reader requests can be explicitly closed;
        /// </summary>
        internal bool IsExplicitlyClosed {
            get {
                return (_status == Status.ClosedExplicitly);
            }
        }

        /// <summary>
        /// Determine whether the parent data reader or record moved on from
        /// where we can be considered open, (because the consumer of the 
        /// parent data reader/record called either the GetValue() or Read() 
        /// methods on the parent);
        /// </summary>
        internal bool IsImplicitlyClosed {
            get {
                return (_status == Status.ClosedImplicitly);
            }
        }
        #endregion

        #region metadata properties and methods

        /// <summary>
        /// implementation of DbDataRecord.DataRecordInfo property
        /// </summary>
        public DataRecordInfo DataRecordInfo {
            get {
                AssertReaderIsOpen();
                DataRecordInfo result = _source.DataRecordInfo;
                return result;
            }
        }

        /// <summary>
        /// implementation of DbDataRecord.FieldCount property
        /// </summary>
        override public int FieldCount {
            get {
                AssertReaderIsOpen();
                return _source.ColumnCount;
            }
        }

        /// <summary>
        /// Helper method to get the edm TypeUsage for the specified column;
        /// 
        /// If the column requested is a record, we'll pick up whatever the
        /// current record says it is, otherwise we'll take whatever was stored
        /// on our record state.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        private TypeUsage GetTypeUsage(int ordinal) {
            // Some folks are picky about the exception we throw
            if (ordinal < 0 || ordinal >= _source.ColumnCount) {
                throw EntityUtil.ArgumentOutOfRange("ordinal");
            }
            TypeUsage result;

            // 
            RecordState recordState = _source.CurrentColumnValues[ordinal] as RecordState;
            if (null != recordState) {
                result = recordState.DataRecordInfo.RecordType;
            }
            else {
                result = _source.GetTypeUsage(ordinal);
            }
            return result;
        }

        /// <summary>
        /// implementation of DbDataRecord.GetDataTypeName() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public string GetDataTypeName(int ordinal) {
            AssertReaderIsOpenWithData();
            return TypeHelpers.GetFullName(GetTypeUsage(ordinal));
        }

        /// <summary>
        /// implementation of DbDataRecord.GetFieldType() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Type GetFieldType(int ordinal) {
            AssertReaderIsOpenWithData();
            return BridgeDataReader.GetClrTypeFromTypeMetadata(GetTypeUsage(ordinal));
        }

        /// <summary>
        /// implementation of DbDataRecord.GetName() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>        
        override public string GetName(int ordinal) {
            AssertReaderIsOpen();
            return _source.GetName(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetOrdinal() method
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        override public int GetOrdinal(string name) {
            AssertReaderIsOpen();
            return _source.GetOrdinal(name);
        }

        #endregion

        #region general getter methods and indexer properties

        /// <summary>
        /// implementation for DbDataRecord[ordinal] indexer property
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public object this[int ordinal] {
            get {
                return GetValue(ordinal);
            }
        }

        /// <summary>
        /// implementation for DbDataRecord[name] indexer property
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        override public object this[string name] {
            get {
                return GetValue(GetOrdinal(name));
            }
        }

        /// <summary>
        /// implementation for DbDataRecord.GetValue() method
        /// 
        /// This method is used by most of the column getters on this
        /// class to retrieve the value from the source reader.  Therefore,
        /// it asserts all the good things, like that the reader is open,
        /// and that it has data, and that you're not trying to circumvent
        /// sequential access requirements.
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Object GetValue(int ordinal) {
            AssertReaderIsOpenWithData();
            AssertSequentialAccess(ordinal);

            object result = null;

            if (ordinal == _lastOrdinalCheckedForNull) {
                result = _lastValueCheckedForNull;
            }
            else {
                _lastOrdinalCheckedForNull = -1;
                _lastValueCheckedForNull = null;

                CloseNestedObjectImplicitly();

                result = _source.CurrentColumnValues[ordinal];

                // If we've got something that's nested, then make sure we
                // update the current nested record with it so we can be certain
                // to close it implicitly when we move past it.
                if (_source.IsNestedObject(ordinal)) {
                    result = GetNestedObjectValue(result);
                }
            }
            return result;
        }

        /// <summary>
        /// For nested objects (records/readers) we have a bit more work to do; this
        /// method extracts it all out from the main GetValue method so it doesn't 
        /// have to be so big.
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        private object GetNestedObjectValue(object result) {
            if (result != DBNull.Value) {
                RecordState recordState = result as RecordState;
                if (null != recordState) {
                    if (recordState.IsNull) {
                        result = DBNull.Value;
                    }
                    else {
                        BridgeDataRecord nestedRecord = new BridgeDataRecord(Shaper, Depth + 1);
                        nestedRecord.SetRecordSource(recordState, true);
                        result = nestedRecord;
                        _currentNestedRecord = nestedRecord;
                        _currentNestedReader = null;
                    }
                }
                else {
                    Coordinator<RecordState> coordinator = result as Coordinator<RecordState>;
                    if (null != coordinator) {
                        BridgeDataReader nestedReader = new BridgeDataReader(Shaper, coordinator.TypedCoordinatorFactory, Depth + 1, nextResultShaperInfos: null);
                        result = nestedReader;
                        _currentNestedRecord = null;
                        _currentNestedReader = nestedReader;
                    }
                    else {
                        Debug.Fail("unexpected type of nested object result: " + result.GetType().ToString());
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataRecord.GetValues() method
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        override public int GetValues(object[] values) {
            EntityUtil.CheckArgumentNull(values, "values");

            int copy = Math.Min(values.Length, FieldCount);
            for (int i = 0; i < copy; ++i) {
                values[i] = GetValue(i);
            }
            return copy;
        }

        #endregion

        #region simple scalar value getter methods

        /// <summary>
        /// implementation of DbDataRecord.GetBoolean() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public bool GetBoolean(int ordinal) {
            return (bool)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetByte() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public byte GetByte(int ordinal) {
            return (byte)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetChar() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public char GetChar(int ordinal) {
            return (char)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetDateTime() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public DateTime GetDateTime(int ordinal) {
            return (DateTime)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetDecimal() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Decimal GetDecimal(int ordinal) {
            return (Decimal)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetDouble() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public double GetDouble(int ordinal) {
            return (double)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetFloat() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public float GetFloat(int ordinal) {
            return (float)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetGuid() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Guid GetGuid(int ordinal) {
            return (Guid)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetInt16() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int16 GetInt16(int ordinal) {
            return (Int16)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetInt32() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int32 GetInt32(int ordinal) {
            return (Int32)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetInt64() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public Int64 GetInt64(int ordinal) {
            return (Int64)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.GetString() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public String GetString(int ordinal) {
            return (String)GetValue(ordinal);
        }

        /// <summary>
        /// implementation of DbDataRecord.IsDBNull() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override public bool IsDBNull(int ordinal) {
            // This seems like a hack, but the the problem is that I need 
            // to make sure I don't monkey with caching things, and if I
            // call IsDBNull directly on the store reader, I'll potentially
            // lose data because I'm expecting SequentialAccess rules.

            object columnValue = GetValue(ordinal);

            // Need to backup one because we technically didn't read the
            // value yet but the GetValue method advanced our pointer to
            // what the value was.  Another hack, but it's way less code
            // than trying to avoid advancing to begin with.
            _lastColumnRead--;
            _lastDataOffsetRead = -1;

            // So as to avoid reconstructing nested records, readers, and
            // rereading data from the iterator source cache, we just cache
            // the value we read and the ordinal it came from, so if someone
            // is doing the right thing(TM) and calling IsDBNull before calling
            // GetValue, we won't construct another one.
            _lastValueCheckedForNull = columnValue;
            _lastOrdinalCheckedForNull = ordinal;

            bool result = (DBNull.Value == columnValue);

            return result;
        }

        #endregion

        #region array scalar value getter methods

        /// <summary>
        /// implementation for DbDataRecord.GetBytes() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        override public long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length) {
            AssertReaderIsOpenWithData();
            AssertSequentialAccess(ordinal, dataOffset, "GetBytes");

            long result = _source.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);

            if (buffer != null) {
                _lastDataOffsetRead = dataOffset + result - 1; // just what was read, nothing more.
            }
            return result;
        }

        /// <summary>
        /// implementation for DbDataRecord.GetChars() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <param name="dataOffset"></param>
        /// <param name="buffer"></param>
        /// <param name="bufferOffset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        override public long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length) {
            AssertReaderIsOpenWithData();
            AssertSequentialAccess(ordinal, dataOffset, "GetChars");

            long result = _source.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);

            if (buffer != null) {
                _lastDataOffsetRead = dataOffset + result - 1; // just what was read, nothing more.
            }
            return result;
        }

        #endregion

        #region complex type getters

        /// <summary>
        /// implementation for DbDataRecord.GetData() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        override protected DbDataReader GetDbDataReader(int ordinal) {
            return (DbDataReader)GetValue(ordinal);
        }

        /// <summary>
        /// implementation for DbDataRecord.GetDataRecord() method
        /// </summary>
        /// <param name="ordinal"></param>
        /// <returns></returns>
        public DbDataRecord GetDataRecord(int ordinal) {
            return (DbDataRecord)GetValue(ordinal);
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
    }
}
