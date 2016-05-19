//------------------------------------------------------------------------------
// <copyright file="RecordManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal sealed class RecordManager {
        private readonly DataTable table;

        private int lastFreeRecord;
        private int minimumCapacity = 50;
        private int recordCapacity = 0;
        private readonly List<int> freeRecordList = new List<int>();

        DataRow[] rows;

        internal RecordManager(DataTable table) {
            if (table == null) {
                throw ExceptionBuilder.ArgumentNull("table");
            }
            this.table = table;
        }

        private void GrowRecordCapacity() {
            if (NewCapacity(recordCapacity) < NormalizedMinimumCapacity(minimumCapacity))
                RecordCapacity = NormalizedMinimumCapacity(minimumCapacity);
            else
                RecordCapacity = NewCapacity(recordCapacity);

            // set up internal map : record --> row
            DataRow[] newRows = table.NewRowArray(recordCapacity);
            if (rows != null) {
                Array.Copy(rows, 0, newRows, 0, Math.Min(lastFreeRecord, rows.Length));
            }
            rows = newRows;
        }

        internal int LastFreeRecord {
            get { return lastFreeRecord; }
        }

        internal int MinimumCapacity {
            get {
                return minimumCapacity;
            }
            set {
                if (minimumCapacity != value) {
                    if (value < 0) {
                        throw ExceptionBuilder.NegativeMinimumCapacity();
                    }
                    minimumCapacity = value;
                }
            }
        }

        internal int RecordCapacity {
            get {
                return recordCapacity;
            }
            set {
                if (recordCapacity != value) {
                    for (int i = 0; i < table.Columns.Count; i++) {
                        table.Columns[i].SetCapacity(value);
                    }
                    recordCapacity = value;
                }
            }
        }

        internal static int NewCapacity(int capacity) {
            return (capacity < 128) ? 128 : (capacity + capacity);
        }

        // Normalization: 64, 256, 1024, 2k, 3k, ....
        private int NormalizedMinimumCapacity(int capacity) {
            if (capacity < 1024 - 10) {
                if (capacity < 256 - 10) {
                    if ( capacity < 54 )
                        return 64;
                    return 256;
                }
                return 1024;
            }

            return (((capacity + 10) >> 10) + 1) << 10;
        }
        internal int NewRecordBase() {
            int record;
            if (freeRecordList.Count != 0) {
                record = freeRecordList[freeRecordList.Count - 1];
                freeRecordList.RemoveAt(freeRecordList.Count - 1);
            }
            else {
                if (lastFreeRecord >= recordCapacity) {
                    GrowRecordCapacity();
                }
                record = lastFreeRecord;
                lastFreeRecord++;
            }
            Debug.Assert(record >=0 && record < recordCapacity, "NewRecord: Invalid record");
            return record;
        }

        internal void FreeRecord(ref int record) {
            Debug.Assert(-1 <= record && record < recordCapacity, "invalid record");
//            Debug.Assert(record < lastFreeRecord, "Attempt to Free() <outofbounds> record");
            if (-1 != record) {
                this[record] = null;

                int count = table.columnCollection.Count;
                for(int i = 0; i < count; ++i) {
                    table.columnCollection[i].FreeRecord(record);
                }

                // if freeing the last record, recycle it
                if (lastFreeRecord == record + 1) {
                    lastFreeRecord--;
                }
                else if (record < lastFreeRecord) {
//                    Debug.Assert(-1 == freeRecordList.IndexOf(record), "Attempt to double Free() record");
                    freeRecordList.Add(record);
                }
                record = -1;
            }
        }

        internal void Clear(bool clearAll) {
            if (clearAll) {
                for(int record = 0; record < recordCapacity; ++record) {
                    rows[record] = null;
                }
                int count = table.columnCollection.Count;
                for(int i = 0; i < count; ++i) {
                    // SQLBU 415729: Serious performance issue when calling Clear()
                    // this improves performance by caching the column instead of obtaining it for each row
                    DataColumn column = table.columnCollection[i];
                    for(int record = 0; record < recordCapacity; ++record) {
                        column.FreeRecord(record);
                    }
                }
                lastFreeRecord = 0;
                freeRecordList.Clear();
            }
            else { // just clear attached rows
                freeRecordList.Capacity = freeRecordList.Count + table.Rows.Count;
                for(int record = 0; record < recordCapacity; ++record) {
                    if (rows[record]!= null && rows[record].rowID != -1) {
                        int tempRecord = record;
                        FreeRecord(ref tempRecord);
                    }
                }
            }
        }
        
        internal DataRow this[int record] {
            get {
                Debug.Assert(record >= 0 && record < rows.Length, "Invalid record number");
                return rows[record];
            }
            set {
                Debug.Assert(record >= 0 && record < rows.Length, "Invalid record number");
                rows[record] = value;
            }
        }

        internal void SetKeyValues(int record, DataKey key, object[] keyValues) {
            for (int i = 0; i < keyValues.Length; i++) {
                key.ColumnsReference[i][record] = keyValues[i];
            }
        }

        // Increases AutoIncrementCurrent
        internal int ImportRecord(DataTable src, int record) {
            return CopyRecord(src, record, -1);
        }

        // No impact on AutoIncrementCurrent if over written
        internal int CopyRecord(DataTable src, int record, int copy) {
            Debug.Assert(src != null, "Can not Merge record without a table");
            
            if (record == -1) {
                return copy;
            }
            int newRecord = -1;
            try {
                if (copy == -1) {
                    newRecord = table.NewUninitializedRecord();
                }
                else {
                    newRecord = copy; 
                }

                int count = table.Columns.Count;
                for (int i = 0; i < count; ++i) {
                    DataColumn dstColumn = table.Columns[i];
                    DataColumn srcColumn = src.Columns[dstColumn.ColumnName];
                    if (null != srcColumn) {
                        object value = srcColumn[record];
                        ICloneable cloneableObject = value as ICloneable;
                        if (null != cloneableObject) {
                            dstColumn[newRecord] = cloneableObject.Clone();
                        }
                        else {                      
                            dstColumn[newRecord] = value;
                        }
                    }
                    else if (-1 == copy) {
                        dstColumn.Init(newRecord);
                    }
                }
            }
            catch (Exception e){
                // 
                if (Common.ADP.IsCatchableOrSecurityExceptionType(e)) {
                    if (-1 == copy) {
                        FreeRecord(ref newRecord);
                    }
                }
                throw;
            }
            return newRecord;
        }

        internal void SetRowCache(DataRow[] newRows) {
            rows = newRows;
            lastFreeRecord = rows.Length;
            recordCapacity = lastFreeRecord;
        }

        [Conditional("DEBUG")]
        internal void VerifyRecord(int record) {
            Debug.Assert((record < lastFreeRecord) && (-1 == freeRecordList.IndexOf(record)), "accesing free record");
            Debug.Assert((null == rows[record]) ||
                         (record == rows[record].oldRecord) ||
                         (record == rows[record].newRecord) ||
                         (record == rows[record].tempRecord), "record of a different row");
        }

        [Conditional("DEBUG")]
        internal void VerifyRecord(int record, DataRow row) {
            Debug.Assert((record < lastFreeRecord) && (-1 == freeRecordList.IndexOf(record)), "accesing free record");
            Debug.Assert((null == rows[record]) || (row == rows[record]), "record of a different row");
        }
    }
}
