//------------------------------------------------------------------------------
// <copyright file="DataTableReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
// <owner current="false" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Data.Common;
    using System.Data.SqlTypes;
    using System.Collections;
    using System.ComponentModel;

    public sealed class DataTableReader : DbDataReader  {

        private readonly DataTable[] tables = null;
        private bool isOpen = true;
        private DataTable schemaTable = null;

        private int tableCounter = -1;
        private int rowCounter = -1;
        private DataTable currentDataTable = null;
        private DataRow currentDataRow = null;

        private bool hasRows= true;
        private bool reachEORows = false;
        private bool currentRowRemoved = false;
        private bool schemaIsChanged = false;
        private bool started = false;
        private bool readerIsInvalid = false;
        private DataTableReaderListener listener = null;
        private bool tableCleared = false;

       public  DataTableReader(DataTable dataTable) {
            if (dataTable == null)
                throw ExceptionBuilder.ArgumentNull("DataTable");
            tables = new DataTable[1] {dataTable};

            init();
//            schemaTable = GetSchemaTableFromDataTable(currentDataTable);
        }

        public  DataTableReader(DataTable [] dataTables) {
           if (dataTables == null)
                throw ExceptionBuilder.ArgumentNull("DataTable");

           if (dataTables.Length == 0)
               throw ExceptionBuilder.DataTableReaderArgumentIsEmpty();


           tables = new DataTable[dataTables.Length];
           for (int i = 0; i < dataTables.Length ; i++) {
               if (dataTables[i] == null)
                   throw ExceptionBuilder.ArgumentNull("DataTable");
               tables[i] = dataTables[i];
           }

           init();
//           schemaTable = GetSchemaTableFromDataTable(currentDataTable);
        }

        private bool ReaderIsInvalid {
            get {
                return readerIsInvalid;
            }
            set {
                if (readerIsInvalid == value)
                    return;
                readerIsInvalid = value;
                if (readerIsInvalid && listener != null) {
                    listener.CleanUp();
                }
            }
        }

        private bool IsSchemaChanged {
            get {
                return schemaIsChanged;
            }
            set {
                if (!value || schemaIsChanged == value) //once it is set to false; should not change unless in init() or NextResult()
                    return;
                schemaIsChanged  = value;
                if (listener != null) {
                    listener.CleanUp();
                }
            }
        }

        internal DataTable CurrentDataTable {
            get {
                return currentDataTable;
            }
        }

        private void init() {
            tableCounter = 0;
            reachEORows = false;
            schemaIsChanged = false;
            currentDataTable = tables[tableCounter];
            hasRows = (currentDataTable.Rows.Count > 0);
            ReaderIsInvalid = false;
 
// we need to listen to current tables event so create a listener, it will listen to events and call us back.
            listener = new DataTableReaderListener(this);
        }


        override public void Close() {
            if (!isOpen)
                return;
// no need to listen to events after close
            if (listener != null)
                listener.CleanUp();

            listener = null;
            schemaTable = null;

            isOpen = false;
        }

        override public DataTable GetSchemaTable(){
            ValidateOpen("GetSchemaTable");
            ValidateReader();

// each time, we just get schema table of current table for once, no need to recreate each time, if schema is changed, reader is already
// is invalid
            if (schemaTable == null)
                schemaTable = GetSchemaTableFromDataTable(currentDataTable);

            return schemaTable;
        }

        override public bool  NextResult() {
// next result set; reset everything
            ValidateOpen("NextResult");

            if ((tableCounter ==  tables.Length -1))
                return false;

           currentDataTable = tables[++tableCounter];

           if (listener != null)
                listener.UpdataTable(currentDataTable); // it will unsubscribe from preveous tables events and subscribe to new table's events

           schemaTable = null;
           rowCounter = -1;
           currentRowRemoved = false;
           reachEORows = false;
           schemaIsChanged = false;
           started = false;
           ReaderIsInvalid = false;
           tableCleared = false;

            hasRows = (currentDataTable.Rows.Count > 0);

           return true;
        }

        override public bool Read() {

/*            else if (tableCleared) {
                return false;
                throw  ExceptionBuilder.EmptyDataTableReader(currentDataTable.TableName);
            }
  */
            if (!started) {
                started = true;
            }
            /*else {
                ValidateRow(rowCounter);
            }*/

            ValidateOpen("Read");

            ValidateReader();


            if(reachEORows) {
                return false;
            }

            if (rowCounter >=  currentDataTable.Rows.Count  -1 ) {
                reachEORows = true;
                if (listener != null)
                    listener.CleanUp();
               return false;
            }

            rowCounter ++;
            ValidateRow(rowCounter);
            currentDataRow = currentDataTable.Rows[rowCounter];

            while (currentDataRow.RowState == DataRowState.Deleted) {
                rowCounter++;
                if (rowCounter ==  currentDataTable.Rows.Count) {
                    reachEORows = true;
                    if (listener != null)
                        listener.CleanUp();
                    return false;
                }
                ValidateRow(rowCounter);
                currentDataRow = currentDataTable.Rows[rowCounter];
            }
            if (currentRowRemoved)
                currentRowRemoved = false;

            return true;
        }

        override public int Depth {
           get {
               ValidateOpen("Depth");
               ValidateReader();
               return 0;
           }
        }

        override public bool IsClosed {
            get {
                return (!isOpen);
            }
        }

        override public int RecordsAffected {
            get {
                ValidateReader();
                return 0;
            }
        }

        override public bool HasRows {
            get {
                ValidateOpen("HasRows");
                ValidateReader();
                return hasRows;
            }
        }

        override public object this[int ordinal] {
            get {
                ValidateOpen("Item");
                ValidateReader();
                if ((currentDataRow == null) || (currentDataRow.RowState == DataRowState.Deleted)) {
                    ReaderIsInvalid = true;
                    throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);
                }
                try {
                    return currentDataRow[ordinal];
                }
                catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                    ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                    throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
                }
            }
        }

        override public object this[string name] {
            get {
                ValidateOpen("Item");
                ValidateReader();
                if ((currentDataRow == null) || (currentDataRow.RowState == DataRowState.Deleted)) {
                    ReaderIsInvalid = true;
                    throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);
                }
                return currentDataRow[name];
            }
        }

        override public Int32 FieldCount {
            get {
                ValidateOpen("FieldCount");
                ValidateReader();
                return currentDataTable.Columns.Count;
            }
        }

        override public Type GetProviderSpecificFieldType(int ordinal) {
            ValidateOpen("GetProviderSpecificFieldType");
            ValidateReader();
            return GetFieldType(ordinal);
        }

        override public Object GetProviderSpecificValue(int ordinal) {
            ValidateOpen("GetProviderSpecificValue");
            ValidateReader();
            return GetValue(ordinal);
        }

        override  public int GetProviderSpecificValues(object[] values) {
            ValidateOpen("GetProviderSpecificValues");
            ValidateReader();
            return GetValues(values);
        }

        override public  bool GetBoolean (int ordinal) {
            ValidateState("GetBoolean");
            ValidateReader();
            try {
                return (bool) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  byte GetByte (int ordinal) {
            ValidateState("GetByte");
            ValidateReader();
            try {
                return (byte) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

       override public long GetBytes(int ordinal, long dataIndex, byte[] buffer, int bufferIndex, int length) {
            ValidateState("GetBytes");
            ValidateReader();
            byte[] tempBuffer;
            try {
                tempBuffer = (byte[]) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
            if (buffer == null) {
                return tempBuffer.Length;
            }
            int srcIndex = (int) dataIndex;
            int byteCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0) {
                throw ADP.InvalidSourceBufferIndex(tempBuffer.Length, srcIndex, "dataIndex");
            }
            else if ((bufferIndex < 0) || (bufferIndex > 0 && bufferIndex >= buffer.Length)) {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < byteCount) {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, byteCount);
            }
            else if (length < 0) {
                throw ADP.InvalidDataLength(length);
            }
            else {
                byteCount = 0;
            }
            return byteCount;

        }

        override public  char GetChar (int ordinal) {
            ValidateState("GetChar");
            ValidateReader();
            try {
                return (char) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

       override public long GetChars(int ordinal, long dataIndex, char[] buffer, int bufferIndex, int length) {
            ValidateState("GetChars");
            ValidateReader();
            char[] tempBuffer;
            try {
                tempBuffer = (char[]) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }

            if (buffer == null) {
                return tempBuffer.Length;
            }

            int srcIndex = (int) dataIndex;
            int charCount = Math.Min(tempBuffer.Length - srcIndex, length);
            if (srcIndex < 0) {
                throw ADP.InvalidSourceBufferIndex(tempBuffer.Length, srcIndex, "dataIndex");
            }
            else if ((bufferIndex < 0) || (bufferIndex > 0 && bufferIndex >= buffer.Length)) {
                throw ADP.InvalidDestinationBufferIndex(buffer.Length, bufferIndex, "bufferIndex");
            }

            if (0 < charCount) {
                Array.Copy(tempBuffer, dataIndex, buffer, bufferIndex, charCount);
            }
            else if (length < 0) {
                throw ADP.InvalidDataLength(length);
            }
            else {
                charCount = 0;
            }
            return charCount;
        }

        override public  String GetDataTypeName (int ordinal) {
            ValidateOpen("GetDataTypeName");
            ValidateReader();
            return ((Type)GetFieldType(ordinal)).Name;
        }

        override public  DateTime GetDateTime (int ordinal) {
            ValidateState("GetDateTime");
            ValidateReader();
            try {
                return (DateTime) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Decimal GetDecimal (int ordinal) {
            ValidateState("GetDecimal");
            ValidateReader();
            try {
                return (Decimal) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Double GetDouble (int ordinal) {
            ValidateState("GetDouble");
            ValidateReader();
            try {
                return (double) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Type GetFieldType (int ordinal) {
            ValidateOpen("GetFieldType");
            ValidateReader();
            try {
                return (currentDataTable.Columns[ordinal].DataType);
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Single GetFloat (int ordinal) {
            ValidateState("GetFloat");
            ValidateReader();
            try {
                return (Single) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Guid GetGuid (int ordinal) {
            ValidateState("GetGuid");
            ValidateReader();
            try {
                return (Guid) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Int16 GetInt16 (int ordinal) {
            ValidateState("GetInt16");
            ValidateReader();
            try {
                return (Int16) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public Int32 GetInt32 (int ordinal) {
            ValidateState("GetInt32");
            ValidateReader();
            try {
                return (Int32) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Int64 GetInt64 (int ordinal) {
            ValidateState("GetInt64");
            ValidateReader();
            try {
                return (Int64) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  String GetName (int ordinal) {
            ValidateOpen("GetName");
            ValidateReader();
            try {
                return (currentDataTable.Columns[ordinal].ColumnName);
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Int32 GetOrdinal (string name) {
            ValidateOpen("GetOrdinal");
            ValidateReader();
            DataColumn dc = currentDataTable.Columns[name];

            if (dc != null) {
                return dc.Ordinal;// WebData 113248
            }
            else{
                throw ExceptionBuilder.ColumnNotInTheTable(name, currentDataTable.TableName);
            }
        }

        override public  string GetString (int ordinal) {
            ValidateState("GetString");
            ValidateReader();
            try {
                return (string) currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }


        override public  object GetValue (int ordinal) {
            ValidateState("GetValue");
            ValidateReader();
            try {
                return currentDataRow[ordinal];
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

        override public  Int32 GetValues (object[] values) {
            ValidateState("GetValues");
            ValidateReader();

            if (values ==null)
                throw ExceptionBuilder.ArgumentNull("values");

            Array.Copy(currentDataRow.ItemArray, values,  currentDataRow.ItemArray.Length > values.Length ? values.Length : currentDataRow.ItemArray.Length);
            return (currentDataRow.ItemArray.Length > values.Length ? values.Length : currentDataRow.ItemArray.Length);
        }
        override public bool IsDBNull (int ordinal) {
            ValidateState("IsDBNull");
            ValidateReader();
            try {
                return (currentDataRow.IsNull(ordinal));
            }
            catch(IndexOutOfRangeException e) { // thrown by DataColumnCollection
                ExceptionBuilder.TraceExceptionWithoutRethrow(e);                
                throw ExceptionBuilder.ArgumentOutOfRange("ordinal");
            }
        }

// IEnumerable
        override public IEnumerator GetEnumerator() {
            ValidateOpen("GetEnumerator");
            return new DbEnumerator((IDataReader)this);
        }

        static internal DataTable GetSchemaTableFromDataTable(DataTable table) {
            if (table == null) {
                throw ExceptionBuilder.ArgumentNull("DataTable");
            }

            DataTable tempSchemaTable = new DataTable("SchemaTable");
            tempSchemaTable.Locale = System.Globalization.CultureInfo.InvariantCulture;

            DataColumn ColumnName          = new DataColumn(SchemaTableColumn.ColumnName,          typeof(System.String));
            DataColumn ColumnOrdinal       = new DataColumn(SchemaTableColumn.ColumnOrdinal,       typeof(System.Int32));
            DataColumn ColumnSize          = new DataColumn(SchemaTableColumn.ColumnSize,          typeof(System.Int32));
            DataColumn NumericPrecision    = new DataColumn(SchemaTableColumn.NumericPrecision,    typeof(System.Int16));
            DataColumn NumericScale        = new DataColumn(SchemaTableColumn.NumericScale,        typeof(System.Int16));
            DataColumn DataType            = new DataColumn(SchemaTableColumn.DataType,            typeof(System.Type));
            DataColumn ProviderType        = new DataColumn(SchemaTableColumn.ProviderType,        typeof(System.Int32));
            DataColumn IsLong              = new DataColumn(SchemaTableColumn.IsLong,              typeof(System.Boolean));
            DataColumn AllowDBNull         = new DataColumn(SchemaTableColumn.AllowDBNull,         typeof(System.Boolean));
            DataColumn IsReadOnly          = new DataColumn(SchemaTableOptionalColumn.IsReadOnly,   typeof(System.Boolean));
            DataColumn IsRowVersion        = new DataColumn(SchemaTableOptionalColumn.IsRowVersion, typeof(System.Boolean));
            DataColumn IsUnique            = new DataColumn(SchemaTableColumn.IsUnique,            typeof(System.Boolean));
            DataColumn IsKeyColumn         = new DataColumn(SchemaTableColumn.IsKey,               typeof(System.Boolean));
            DataColumn IsAutoIncrement     = new DataColumn(SchemaTableOptionalColumn.IsAutoIncrement,     typeof(System.Boolean));
            DataColumn BaseSchemaName      = new DataColumn(SchemaTableColumn.BaseSchemaName,      typeof(System.String));
            DataColumn BaseCatalogName     = new DataColumn(SchemaTableOptionalColumn.BaseCatalogName,     typeof(System.String));
            DataColumn BaseTableName       = new DataColumn(SchemaTableColumn.BaseTableName,       typeof(System.String));
            DataColumn BaseColumnName      = new DataColumn(SchemaTableColumn.BaseColumnName,      typeof(System.String));
            DataColumn AutoIncrementSeed   = new DataColumn(SchemaTableOptionalColumn.AutoIncrementSeed,   typeof(System.Int64));
            DataColumn AutoIncrementStep   = new DataColumn(SchemaTableOptionalColumn.AutoIncrementStep,   typeof(System.Int64));
            DataColumn DefaultValue        = new DataColumn(SchemaTableOptionalColumn.DefaultValue,        typeof(System.Object));
            DataColumn Expression          = new DataColumn(SchemaTableOptionalColumn.Expression,          typeof(System.String));
            DataColumn ColumnMapping       = new DataColumn(SchemaTableOptionalColumn.ColumnMapping,       typeof(System.Data.MappingType));
            DataColumn BaseTableNamespace  = new DataColumn(SchemaTableOptionalColumn.BaseTableNamespace,  typeof(System.String));
            DataColumn BaseColumnNamespace = new DataColumn(SchemaTableOptionalColumn.BaseColumnNamespace, typeof(System.String));

           ColumnSize.DefaultValue = -1;

           if (table.DataSet != null)
               BaseCatalogName.DefaultValue =  table.DataSet.DataSetName;

           BaseTableName.DefaultValue = table.TableName;
           BaseTableNamespace.DefaultValue = table.Namespace;
           IsRowVersion.DefaultValue = false;
           IsLong.DefaultValue = false;
           IsReadOnly.DefaultValue = false;
           IsKeyColumn.DefaultValue = false;
           IsAutoIncrement.DefaultValue = false;
           AutoIncrementSeed.DefaultValue = 0;
           AutoIncrementStep.DefaultValue = 1;


           tempSchemaTable.Columns.Add(ColumnName);
           tempSchemaTable.Columns.Add(ColumnOrdinal);
           tempSchemaTable.Columns.Add(ColumnSize);
           tempSchemaTable.Columns.Add(NumericPrecision);
           tempSchemaTable.Columns.Add(NumericScale);
           tempSchemaTable.Columns.Add(DataType);
           tempSchemaTable.Columns.Add(ProviderType);
           tempSchemaTable.Columns.Add(IsLong);
           tempSchemaTable.Columns.Add(AllowDBNull);
           tempSchemaTable.Columns.Add(IsReadOnly);
           tempSchemaTable.Columns.Add(IsRowVersion);
           tempSchemaTable.Columns.Add(IsUnique);
           tempSchemaTable.Columns.Add(IsKeyColumn);
           tempSchemaTable.Columns.Add(IsAutoIncrement);
           tempSchemaTable.Columns.Add(BaseCatalogName);
           tempSchemaTable.Columns.Add(BaseSchemaName);
           // specific to datatablereader
           tempSchemaTable.Columns.Add(BaseTableName);
           tempSchemaTable.Columns.Add(BaseColumnName);
           tempSchemaTable.Columns.Add(AutoIncrementSeed);
           tempSchemaTable.Columns.Add(AutoIncrementStep);
           tempSchemaTable.Columns.Add(DefaultValue);
           tempSchemaTable.Columns.Add(Expression);
           tempSchemaTable.Columns.Add(ColumnMapping);
           tempSchemaTable.Columns.Add(BaseTableNamespace);
           tempSchemaTable.Columns.Add(BaseColumnNamespace);

           foreach (DataColumn dc in table.Columns) {
               DataRow dr = tempSchemaTable.NewRow();

               dr[ColumnName] = dc.ColumnName;
               dr[ColumnOrdinal] = dc.Ordinal;
               dr[DataType] = dc.DataType;

               if (dc.DataType == typeof(string)) {
                   dr[ColumnSize] = dc.MaxLength;
               }

               dr[AllowDBNull] = dc.AllowDBNull;
               dr[IsReadOnly] = dc.ReadOnly;
               dr[IsUnique] = dc.Unique;

               if (dc.AutoIncrement) {
                   dr[IsAutoIncrement] = true;
                   dr[AutoIncrementSeed] = dc.AutoIncrementSeed;
                   dr[AutoIncrementStep] = dc.AutoIncrementStep;
               }

               if (dc.DefaultValue != DBNull.Value)
                   dr[DefaultValue] =  dc.DefaultValue;

               if (dc.Expression.Length  != 0)  {
                   bool hasExternalDependency = false;
                   DataColumn[] dependency = dc.DataExpression.GetDependency();
                   for (int j = 0; j < dependency.Length; j++) {
                       if (dependency[j].Table != table) {
                           hasExternalDependency = true;
                           break;
                       }
                   }
                   if (!hasExternalDependency)
                       dr[Expression] =  dc.Expression;
               }

               dr[ColumnMapping] =  dc.ColumnMapping;
               dr[BaseColumnName] = dc.ColumnName;
               dr[BaseColumnNamespace] = dc.Namespace;

               tempSchemaTable.Rows.Add(dr);
           }

           foreach(DataColumn key in table.PrimaryKey) {
               tempSchemaTable.Rows[key.Ordinal][IsKeyColumn] = true;
           }


               tempSchemaTable.AcceptChanges();

           return tempSchemaTable;
        }

        private void ValidateOpen(string caller) {
           if (!isOpen)
               throw ADP.DataReaderClosed(caller);
       }

       private void ValidateReader() {
           if (ReaderIsInvalid)
               throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);

           if (IsSchemaChanged) {
               throw  ExceptionBuilder.DataTableReaderSchemaIsInvalid(currentDataTable.TableName); // may be we can use better error message!
           }

       }

       private void ValidateState(string caller) {
           ValidateOpen(caller);
           if (tableCleared) {
               throw  ExceptionBuilder.EmptyDataTableReader(currentDataTable.TableName);
           }
           // see if without any event raising, if our curent row has some changes!if so reader is invalid.
           if ((currentDataRow == null) || (currentDataTable == null) ) {//|| (currentDataRow != currentDataTable.Rows[rowCounter])) do we need thios check!
               ReaderIsInvalid = true;
               throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);
           }
           //See if without any event raing, if our rows are deleted, or removed! Reader is not invalid, user should be able to read and reach goo row WebData98325
           if ((currentDataRow.RowState == DataRowState.Deleted) || (currentDataRow.RowState == DataRowState.Detached) ||currentRowRemoved)
                throw  ExceptionBuilder.InvalidCurrentRowInDataTableReader();
           // user may have called clear (which removes the rows without raing event) or deleted part of rows without raising event!if so reader is invalid.
           if (0 > rowCounter ||currentDataTable.Rows.Count <= rowCounter) {
                ReaderIsInvalid = true;
               throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);
           }
           else {

           }
       }

       private void ValidateRow(Int32 rowPosition) {
           if (ReaderIsInvalid)
               throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);

           if (0 > rowPosition ||currentDataTable.Rows.Count <= rowPosition) {
               ReaderIsInvalid = true;
               throw  ExceptionBuilder.InvalidDataTableReader(currentDataTable.TableName);
           }
       }
 
// Event Call backs from DataTableReaderListener,  will invoke these methods
       internal void SchemaChanged() {
           IsSchemaChanged = true;
       }
       internal void DataTableCleared() {
           if (!started)
               return;

            rowCounter = -1;
            if (!reachEORows)
                currentRowRemoved = true;
       }

       internal void DataChanged(DataRowChangeEventArgs args ) {
           if ((!started) ||(rowCounter == -1 && !tableCleared))
               return;
/*           if (rowCounter == -1 && tableCleared && args.Action == DataRowAction.Add) {
               tableCleared = false;
               return;
           }
*/
           switch (args.Action) {
               case DataRowAction.Add:
                   ValidateRow(rowCounter + 1);
/*                   if (tableCleared) {
                       tableCleared = false;
                       rowCounter++;
                       currentDataRow = currentDataTable.Rows[rowCounter];
                       currentRowRemoved = false;
                   }
                   else 
*/
                    if (currentDataRow == currentDataTable.Rows[rowCounter + 1]) { // check if we moved one position up
                       rowCounter++;  // if so, refresh the datarow and fix the counter
                   }
                   break;
               case DataRowAction.Delete: // delete
               case DataRowAction.Rollback:// rejectchanges
               case DataRowAction.Commit: // acceptchanges
                   if ( args.Row.RowState == DataRowState.Detached ) {
                       if (args.Row != currentDataRow) {
                           if (rowCounter == 0) // if I am at first row and no previous row exist,NOOP
                               break;
                           ValidateRow(rowCounter -1);
                           if (currentDataRow == currentDataTable.Rows[rowCounter - 1]) { // one of previous rows is detached, collection size is changed!
                               rowCounter--;
                           }
                       }
                       else { // we are proccessing current datarow
                           currentRowRemoved = true;
                           if (rowCounter > 0) {  // go back one row, no matter what the state is
                               rowCounter--;
                               currentDataRow = currentDataTable.Rows[rowCounter];
                           }
                           else {  // we are on 0th row, so reset data to initial state!
                               rowCounter = -1;
                               currentDataRow = null;
                           }
                       }
                   }
                   break;
               default:
                   break;
           }
       }

    }
}
