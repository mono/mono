//------------------------------------------------------------------------------
// <copyright file="DbDataAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.ComponentModel;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.ProviderBase;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading;

    public abstract class DbDataAdapter : DataAdapter, IDbDataAdapter, ICloneable { // V1.0.3300, MDAC 69629
        public const string DefaultSourceTableName = "Table"; // V1.0.3300

        internal static readonly object ParameterValueNonNullValue = 0;
        internal static readonly object ParameterValueNullValue = 1;

        private IDbCommand      _deleteCommand, _insertCommand, _selectCommand, _updateCommand;

        private CommandBehavior _fillCommandBehavior;

        private struct BatchCommandInfo {
            internal int             CommandIdentifier;     // whatever AddToBatch returns, so we can reference the command later in GetBatchedParameter
            internal int             ParameterCount;        // number of parameters on the command, so we know how many to loop over when processing output parameters
            internal DataRow         Row;                   // the row that the command is intended to update
            internal StatementType   StatementType;         // the statement type of the command, needed for accept changes
            internal UpdateRowSource UpdatedRowSource;      // the UpdatedRowSource value from the command, to know whether we need to look for output parameters or not
            internal int?            RecordsAffected;
            internal Exception       Errors;
        }

        protected DbDataAdapter() : base() { // V1.0.3300
        }

        protected DbDataAdapter(DbDataAdapter adapter) : base(adapter) { // V1.0.5000
            CloneFrom(adapter);
        }

        private IDbDataAdapter _IDbDataAdapter {
            get {
                return (IDbDataAdapter)this;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public DbCommand DeleteCommand { // V1.2.3300
            get {
                return (DbCommand)(_IDbDataAdapter.DeleteCommand);
            }
            set {
                _IDbDataAdapter.DeleteCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.DeleteCommand { // V1.2.3300
            get {
                return _deleteCommand;
            }
            set {
                _deleteCommand = value;
            }
        }

        protected internal CommandBehavior FillCommandBehavior { // V1.2.3300, MDAC 87511
            get {
                //Bid.Trace("<comm.DbDataAdapter.get_FillCommandBehavior|API> %d#\n", ObjectID);
                return (_fillCommandBehavior | CommandBehavior.SequentialAccess);
            }
            set {
                // setting |= SchemaOnly;       /* similar to FillSchema (which also uses KeyInfo) */
                // setting |= KeyInfo;          /* same as MissingSchemaAction.AddWithKey */
                // setting |= SequentialAccess; /* required and always present */
                // setting |= CloseConnection;  /* close connection regardless of start condition */
                _fillCommandBehavior = (value | CommandBehavior.SequentialAccess);
                //Bid.Trace("<comm.DbDataAdapter.set_FillCommandBehavior|API> %d#, %d{ds.CommandBehavior}\n", (int)value);
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public DbCommand InsertCommand { // V1.2.3300
            get {
                return (DbCommand)(_IDbDataAdapter.InsertCommand);
            }
            set {
                _IDbDataAdapter.InsertCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.InsertCommand { // V1.2.3300
            get {
                return _insertCommand;
            }
            set {
                _insertCommand = value;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public DbCommand SelectCommand { // V1.2.3300
            get {
                return (DbCommand)(_IDbDataAdapter.SelectCommand);
            }
            set {
                _IDbDataAdapter.SelectCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.SelectCommand { // V1.2.3300
            get {
                return _selectCommand;
            }
            set {
                _selectCommand = value;
            }
        }

        [
        DefaultValue(1),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_UpdateBatchSize),
        ]
        virtual public int UpdateBatchSize {
            get {
                return 1;
            }
            set {
                if (1 != value) {
                    throw ADP.NotSupported();
                }
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public DbCommand UpdateCommand { // V1.2.3300
            get {
                return (DbCommand)(_IDbDataAdapter.UpdateCommand);
            }
            set {
                _IDbDataAdapter.UpdateCommand = value;
            }
        }

        IDbCommand IDbDataAdapter.UpdateCommand { // V1.2.3300
            get {
                return _updateCommand;
            }
            set {
                _updateCommand = value;
            }
        }

        private System.Data.MissingMappingAction UpdateMappingAction {
            get {
                if (System.Data.MissingMappingAction.Passthrough == MissingMappingAction) {
                    return System.Data.MissingMappingAction.Passthrough;
                }
                return System.Data.MissingMappingAction.Error;
            }
        }

        private System.Data.MissingSchemaAction UpdateSchemaAction {
            get {
                System.Data.MissingSchemaAction action = MissingSchemaAction;
                if ((System.Data.MissingSchemaAction.Add == action) || (System.Data.MissingSchemaAction.AddWithKey == action)) {
                    return System.Data.MissingSchemaAction.Ignore;
                }
                return System.Data.MissingSchemaAction.Error;
            }
        }

        protected virtual int AddToBatch(IDbCommand command) {
            // Called to add a single command to the batch of commands that need
            // to be executed as a batch, when batch updates are requested.  It
            // must return an identifier that can be used to identify the command
            // to GetBatchedParameter later.

            throw ADP.NotSupported();
        }

        virtual protected void ClearBatch() {
            // Called when batch updates are requested to clear out the contents
            // of the batch, whether or not it's been executed.

            throw ADP.NotSupported();
        }

        object ICloneable.Clone() { // V1.0.3300, MDAC 69629
#pragma warning disable 618 // ignore obsolete warning about CloneInternals
            DbDataAdapter clone = (DbDataAdapter)CloneInternals();
#pragma warning restore 618
            clone.CloneFrom(this);
            return clone;
        }

        private void CloneFrom(DbDataAdapter from) {
            IDbDataAdapter pfrom = from._IDbDataAdapter;
            _IDbDataAdapter.SelectCommand = CloneCommand(pfrom.SelectCommand);
            _IDbDataAdapter.InsertCommand = CloneCommand(pfrom.InsertCommand);
            _IDbDataAdapter.UpdateCommand = CloneCommand(pfrom.UpdateCommand);
            _IDbDataAdapter.DeleteCommand = CloneCommand(pfrom.DeleteCommand);
        }

        private IDbCommand CloneCommand(IDbCommand command) {
            return (IDbCommand) ((command is ICloneable) ? ((ICloneable) command).Clone() : null);
        }

        virtual protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) { // V1.0.3300
            return new RowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        virtual protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) { // V1.0.3300
            return new RowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected void Dispose(bool disposing) { // V1.0.3300, MDAC 69629
            if (disposing) { // release mananged objects
                IDbDataAdapter pthis = (IDbDataAdapter) this; // must cast to interface to obtain correct value
                pthis.SelectCommand = null;
                pthis.InsertCommand = null;
                pthis.UpdateCommand = null;
                pthis.DeleteCommand = null;
            }
            // release unmanaged objects

            base.Dispose(disposing); // notify base classes
        }

        protected virtual int ExecuteBatch() {
            // Called to execute the batched update command, returns the number
            // of rows affected, just as ExecuteNonQuery would.

            throw ADP.NotSupported();
        }

        public DataTable FillSchema(DataTable dataTable, SchemaType schemaType) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.FillSchema|API> %d#, dataTable, schemaType=%d{ds.SchemaType}\n", ObjectID, (int)schemaType);
            try {
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return FillSchema(dataTable, schemaType, selectCmd, cmdBehavior); // MDAC 67666
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        override public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType=%d{ds.SchemaType}\n", ObjectID, (int)schemaType);
            try {
                IDbCommand command = _IDbDataAdapter.SelectCommand;
                if (DesignMode && ((null == command) || (null == command.Connection) || ADP.IsEmpty(command.CommandText))) {
                    return new DataTable[0]; // design-time support
                }
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return FillSchema(dataSet, schemaType, command, DbDataAdapter.DefaultSourceTableName, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, string srcTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType=%d{ds.SchemaType}, srcTable=%ls%\n", ObjectID, (int)schemaType, srcTable);
            try {
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return FillSchema(dataSet, schemaType, selectCmd, srcTable, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected DataTable[] FillSchema(DataSet dataSet, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.FillSchema|API> %d#, dataSet, schemaType, command, srcTable, behavior=%d{ds.CommandBehavior}\n", ObjectID, (int)behavior);
            try {
                if (null == dataSet) {
                    throw ADP.ArgumentNull("dataSet");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType)) {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if (ADP.IsEmpty(srcTable)) {
                    throw ADP.FillSchemaRequiresSourceTableName("srcTable");
                }
                if (null == command) {
                    throw ADP.MissingSelectCommand(ADP.FillSchema);
                }
                return (DataTable[]) FillSchemaInternal(dataSet, null, schemaType, command, srcTable, behavior);
            } finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected DataTable FillSchema(DataTable dataTable, SchemaType schemaType, IDbCommand command, CommandBehavior behavior) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.FillSchema|API> %d#, dataTable, schemaType, command, behavior=%d{ds.CommandBehavior}\n", ObjectID, (int)behavior);
            try {
                if (null == dataTable) {
                    throw ADP.ArgumentNull("dataTable");
                }
                if ((SchemaType.Source != schemaType) && (SchemaType.Mapped != schemaType)) {
                    throw ADP.InvalidSchemaType(schemaType);
                }
                if (null == command) {
                    throw ADP.MissingSelectCommand(ADP.FillSchema);
                }
                string srcTableName = dataTable.TableName;
                int index = IndexOfDataSetTable(srcTableName);
                if (-1 != index) {
                   srcTableName = TableMappings[index].SourceTable;
                }
                return (DataTable) FillSchemaInternal(null, dataTable, schemaType, command, srcTableName, behavior | CommandBehavior.SingleResult);
            } finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private object FillSchemaInternal(DataSet dataset, DataTable datatable, SchemaType schemaType, IDbCommand command, string srcTable, CommandBehavior behavior) {
            object dataTables = null;
            bool restoreNullConnection = (null == command.Connection);
            try {
                IDbConnection activeConnection = DbDataAdapter.GetConnection3(this, command, ADP.FillSchema);
                ConnectionState originalState = ConnectionState.Open;

                try {
                    QuietOpen(activeConnection, out originalState);
                    using(IDataReader dataReader = command.ExecuteReader(behavior | CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo)) {
                        if (null != datatable) { // delegate to next set of protected FillSchema methods
                            dataTables = FillSchema(datatable, schemaType, dataReader);
                        }
                        else {
                            dataTables = FillSchema(dataset, schemaType, srcTable, dataReader);
                        }
                    }
                }
                finally {
                    QuietClose(activeConnection, originalState);
                }
            }
            finally {
                if (restoreNullConnection) {
                    command.Transaction = null;
                    command.Connection = null;
                }
            }
            return dataTables;
        }

        override public int Fill(DataSet dataSet) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataSet\n", ObjectID);
            try {
                // delegate to Fill4
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return Fill(dataSet, 0, 0, DbDataAdapter.DefaultSourceTableName, selectCmd, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Fill(DataSet dataSet, string srcTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, srcTable='%ls'\n", ObjectID, srcTable);
            try {
                // delegate to Fill4
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return Fill(dataSet, 0, 0, srcTable, selectCmd, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, startRecord=%d, maxRecords=%d, srcTable='%ls'\n", ObjectID, startRecord, maxRecords, srcTable);
            try {
                // delegate to Fill4
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return Fill(dataSet, startRecord, maxRecords, srcTable, selectCmd, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected int Fill(DataSet dataSet, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataSet, startRecord, maxRecords, srcTable, command, behavior=%d{ds.CommandBehavior}\n", ObjectID, (int)behavior);
            try {
                if (null == dataSet) {
                    throw ADP.FillRequires("dataSet");
                }
                if (startRecord < 0) {
                    throw ADP.InvalidStartRecord("startRecord", startRecord);
                }
                if (maxRecords < 0) {
                    throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
                }
                if (ADP.IsEmpty(srcTable)) {
                    throw ADP.FillRequiresSourceTableName("srcTable");
                }
                if (null == command) {
                    throw ADP.MissingSelectCommand(ADP.Fill);
                }
                return FillInternal(dataSet, null, startRecord, maxRecords, srcTable, command, behavior);
            }finally {

            Bid.ScopeLeave(ref hscp);
            }
        }

        public int Fill(DataTable dataTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataTable\n", ObjectID);
            try {
                // delegate to Fill8
                DataTable[] dataTables = new DataTable[1] { dataTable};
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return Fill(dataTables, 0, 0, selectCmd, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Fill(int startRecord, int maxRecords, params DataTable[] dataTables) { // V1.2.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, startRecord=%d, maxRecords=%d, dataTable[]\n", ObjectID, startRecord, maxRecords);
            try {
                // delegate to Fill8
                IDbCommand selectCmd = _IDbDataAdapter.SelectCommand;
                CommandBehavior cmdBehavior = FillCommandBehavior;
                return Fill(dataTables, startRecord, maxRecords, selectCmd, cmdBehavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected int Fill(DataTable dataTable, IDbCommand command, CommandBehavior behavior) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> dataTable, command, behavior=%d{ds.CommandBehavior}%d#\n", ObjectID, (int)behavior);
            try {
                // delegate to Fill8
                DataTable[] dataTables = new DataTable[1] { dataTable};
                return Fill(dataTables, 0, 0, command, behavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected int Fill(DataTable[] dataTables, int startRecord, int maxRecords, IDbCommand command, CommandBehavior behavior) { // V1.2.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Fill|API> %d#, dataTables[], startRecord, maxRecords, command, behavior=%d{ds.CommandBehavior}\n", ObjectID, (int)behavior);
            try {
                if ((null == dataTables) || (0 == dataTables.Length) || (null == dataTables[0])) {
                    throw ADP.FillRequires("dataTable");
                }
                if (startRecord < 0) {
                    throw ADP.InvalidStartRecord("startRecord", startRecord);
                }
                if (maxRecords < 0) {
                    throw ADP.InvalidMaxRecords("maxRecords", maxRecords);
                }
                if ((1 < dataTables.Length) && ((0 != startRecord) || (0 != maxRecords))) {
                    throw ADP.OnlyOneTableForStartRecordOrMaxRecords();
                }
                if (null == command) {
                    throw ADP.MissingSelectCommand(ADP.Fill);
                }
                if (1 == dataTables.Length) {
                    behavior |= CommandBehavior.SingleResult;
                }
                return FillInternal(null, dataTables, startRecord, maxRecords, null, command, behavior);
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private int FillInternal(DataSet dataset, DataTable[] datatables, int startRecord, int maxRecords, string srcTable, IDbCommand command, CommandBehavior behavior) {
            int rowsAddedToDataSet = 0;
            bool restoreNullConnection = (null == command.Connection);
            try {
                IDbConnection activeConnection = DbDataAdapter.GetConnection3(this, command, ADP.Fill);
                ConnectionState originalState = ConnectionState.Open;

                // the default is MissingSchemaAction.Add, the user must explicitly
                // set MisingSchemaAction.AddWithKey to get key information back in the dataset
                if (Data.MissingSchemaAction.AddWithKey == MissingSchemaAction) {
                    behavior |= CommandBehavior.KeyInfo;
                }

                try {
                    QuietOpen(activeConnection, out originalState);
                    behavior |= CommandBehavior.SequentialAccess;

                    IDataReader dataReader = null;
                    try {
                        dataReader = command.ExecuteReader(behavior);

                        if (null != datatables) { // delegate to next set of protected Fill methods
                            rowsAddedToDataSet = Fill(datatables, dataReader, startRecord, maxRecords);
                        }
                        else {
                            rowsAddedToDataSet = Fill(dataset, srcTable, dataReader, startRecord, maxRecords);
                        }
                    }
                    finally {
                        if (null != dataReader) {
                            dataReader.Dispose();
                        }
                    }
                }
                finally {
                    QuietClose(activeConnection, originalState);
                }
            }
            finally {
                if (restoreNullConnection) {
                    command.Transaction = null;
                    command.Connection = null;
                }
            }
            return rowsAddedToDataSet;
        }

        virtual protected IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex) {
            // Called to retrieve a parameter from a specific bached command, the
            // first argument is the value that was returned by AddToBatch when it
            // was called for the command.

            throw ADP.NotSupported();
        }

        virtual protected bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error) { // SQLBU 412467
            // Called to retrieve the records affected from a specific batched command,
            // first argument is the value that was returned by AddToBatch when it
            // was called for the command.

            // default implementation always returns 1, derived classes override for otherwise
            // otherwise DbConcurrencyException will only be thrown if sum of all records in batch is 0

            // return 0 to cause Update to throw DbConcurrencyException
            recordsAffected = 1;
            error = null;
            return true;
        }

        [ EditorBrowsableAttribute(EditorBrowsableState.Advanced) ] // MDAC 69508
        override public IDataParameter[] GetFillParameters() { // V1.0.3300
            IDataParameter[] value = null;
            IDbCommand select = _IDbDataAdapter.SelectCommand;
            if (null != select) {
                IDataParameterCollection parameters = select.Parameters;
                if (null != parameters) {
                    value = new IDataParameter[parameters.Count];
                    parameters.CopyTo(value, 0);
                }
            }
            if (null == value) {
                value = new IDataParameter[0];
            }
            return value;
        }

        internal DataTableMapping GetTableMapping(DataTable dataTable) {
            DataTableMapping tableMapping = null;
            int index = IndexOfDataSetTable(dataTable.TableName);
            if (-1 != index) {
                tableMapping = TableMappings[index];
            }
            if (null == tableMapping) {
                if (System.Data.MissingMappingAction.Error == MissingMappingAction) {
                    throw ADP.MissingTableMappingDestination(dataTable.TableName);
                }
                tableMapping = new DataTableMapping(dataTable.TableName, dataTable.TableName);
            }
            return tableMapping;
        }

        virtual protected void InitializeBatching() {
            // Called when batch updates are requested to prepare for processing
            // of a batch of commands.

            throw ADP.NotSupported();
        }

        virtual protected void OnRowUpdated(RowUpdatedEventArgs value) { // V1.0.3300
        }

        virtual protected void OnRowUpdating(RowUpdatingEventArgs value) { // V1.0.3300
        }

        private void ParameterInput(IDataParameterCollection parameters, StatementType typeIndex, DataRow row, DataTableMapping mappings) {
            Data.MissingMappingAction missingMapping = UpdateMappingAction;
            Data.MissingSchemaAction missingSchema = UpdateSchemaAction;

            foreach(IDataParameter parameter in parameters) {
                if ((null != parameter) && (0 != (ParameterDirection.Input & parameter.Direction))) {

                    string columnName = parameter.SourceColumn;
                    if (!ADP.IsEmpty(columnName)) {

                        DataColumn dataColumn = mappings.GetDataColumn(columnName, null, row.Table, missingMapping, missingSchema);
                        if (null != dataColumn) {
                            DataRowVersion version = DbDataAdapter.GetParameterSourceVersion(typeIndex, parameter);
                            parameter.Value = row[dataColumn, version];
                        }
                        else {
                            parameter.Value = null;
                        }

                        DbParameter dbparameter = (parameter as DbParameter);
                        if ((null != dbparameter) && dbparameter.SourceColumnNullMapping) {
                            Debug.Assert(DbType.Int32 == parameter.DbType, "unexpected DbType");
                            parameter.Value = ADP.IsNull(parameter.Value) ? ParameterValueNullValue : ParameterValueNonNullValue;
                        }
                    }
                }
            }
        }

        private void ParameterOutput(IDataParameter parameter, DataRow row, DataTableMapping mappings, MissingMappingAction missingMapping, MissingSchemaAction missingSchema) {
            if (0 != (ParameterDirection.Output & parameter.Direction)) {

                object value = parameter.Value;
                if (null != value) {
                    // null means default, meaning we leave the current DataRow value alone
                    string columnName = parameter.SourceColumn;
                    if (!ADP.IsEmpty(columnName)) {

                        DataColumn dataColumn = mappings.GetDataColumn(columnName, null, row.Table, missingMapping, missingSchema);
                        if (null != dataColumn) {
                            if (dataColumn.ReadOnly) {
                                try {
                                    dataColumn.ReadOnly = false;
                                    row[dataColumn] = value;
                                }
                                finally {
                                    dataColumn.ReadOnly = true;
                                }
                            }
                            else {
                                row[dataColumn] = value;
                            }
                        }
                    }
                }
            }
        }

        private void ParameterOutput(IDataParameterCollection parameters, DataRow row, DataTableMapping mappings) {
            Data.MissingMappingAction missingMapping = UpdateMappingAction;
            Data.MissingSchemaAction missingSchema = UpdateSchemaAction;

            foreach(IDataParameter parameter in parameters) {
                if (null != parameter) {
                    ParameterOutput(parameter, row, mappings, missingMapping, missingSchema);
                }
            }
         }

        virtual protected void TerminateBatching() {
            // Called when batch updates are requested to cleanup after a batch
            // update has been completed.

            throw ADP.NotSupported();
        }

        override public int Update(DataSet dataSet) { // V1.0.3300
            //if (!TableMappings.Contains(DbDataAdapter.DefaultSourceTableName)) { // MDAC 59268
            //    throw ADP.UpdateRequiresSourceTable(DbDataAdapter.DefaultSourceTableName);
            //}
            return Update(dataSet, DbDataAdapter.DefaultSourceTableName);
        }

        public int Update(DataRow[] dataRows) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Update|API> %d#, dataRows[]\n", ObjectID);
            try {
                int rowsAffected = 0;
                if (null == dataRows) {
                    throw ADP.ArgumentNull("dataRows");
                }
                else if (0 != dataRows.Length) {
                    DataTable dataTable = null;
                    for (int i = 0; i < dataRows.Length; ++i) {
                        if ((null != dataRows[i]) && (dataTable != dataRows[i].Table)) {
                            if (null != dataTable) {
                                throw ADP.UpdateMismatchRowTable(i);
                            }
                            dataTable = dataRows[i].Table;
                        }
                    }
                    if (null != dataTable) {
                        DataTableMapping tableMapping = GetTableMapping(dataTable);
                        rowsAffected = Update(dataRows, tableMapping);
                    }
                }
                return rowsAffected;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Update(DataTable dataTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Update|API> %d#, dataTable", ObjectID);
            try {
                if (null == dataTable) {
                    throw ADP.UpdateRequiresDataTable("dataTable");
                }

                DataTableMapping tableMapping = null;
                int index = IndexOfDataSetTable(dataTable.TableName);
                if (-1 != index) {
                    tableMapping = TableMappings[index];
                }
                if (null == tableMapping) {
                    if (System.Data.MissingMappingAction.Error == MissingMappingAction) {
                        throw ADP.MissingTableMappingDestination(dataTable.TableName);
                    }
                    tableMapping = new DataTableMapping(DbDataAdapter.DefaultSourceTableName, dataTable.TableName);
                }
                return UpdateFromDataTable(dataTable, tableMapping);
            } finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        public int Update(DataSet dataSet, string srcTable) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Update|API> %d#, dataSet, srcTable='%ls'", ObjectID, srcTable);
            try {
                if (null == dataSet) {
                    throw ADP.UpdateRequiresNonNullDataSet("dataSet");
                }
                if (ADP.IsEmpty(srcTable)) {
                    throw ADP.UpdateRequiresSourceTableName("srcTable");
                }
#if DEBUG
                //ADP.TraceDataSet("Update <" + srcTable + ">", dataSet);
#endif

                int rowsAffected = 0;

                System.Data.MissingMappingAction missingMapping = UpdateMappingAction;
                DataTableMapping tableMapping = GetTableMappingBySchemaAction(srcTable, srcTable, UpdateMappingAction);
                Debug.Assert(null != tableMapping, "null TableMapping when MissingMappingAction.Error");

                // the ad-hoc scenario of no dataTable just returns
                // ad-hoc scenario is defined as MissingSchemaAction.Add or MissingSchemaAction.Ignore
                System.Data.MissingSchemaAction schemaAction = UpdateSchemaAction;
                DataTable dataTable = tableMapping.GetDataTableBySchemaAction(dataSet, schemaAction);
                if (null != dataTable) {
                    rowsAffected = UpdateFromDataTable(dataTable, tableMapping);
                }
                else if (!HasTableMappings() || (-1 == TableMappings.IndexOf(tableMapping))) {
                    //throw error since the user didn't explicitly map this tableName to Ignore.
                    throw ADP.UpdateRequiresSourceTable(srcTable); // MDAC 72681
                }
                return rowsAffected;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        virtual protected int Update(DataRow[] dataRows, DataTableMapping tableMapping) { // V1.0.3300
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbDataAdapter.Update|API> %d#, dataRows[], tableMapping", ObjectID);
            try {
                Debug.Assert((null != dataRows) && (0 < dataRows.Length), "Update: bad dataRows");
                Debug.Assert(null != tableMapping, "Update: bad DataTableMapping");

                // If records were affected, increment row count by one - that is number of rows affected in dataset.
                int cumulativeDataRowsAffected = 0;

                IDbConnection[] connections = new IDbConnection[5]; // one for each statementtype
                ConnectionState[] connectionStates = new ConnectionState[5]; // closed by default (== 0)

                bool useSelectConnectionState = false; // MDAC 58710
                IDbCommand tmpcmd = _IDbDataAdapter.SelectCommand;
                if (null != tmpcmd) {
                    connections[0] = tmpcmd.Connection;
                    if (null != connections[0]) {
                        connectionStates[0] = connections[0].State;
                        useSelectConnectionState = true;
                    }
                }

                int maxBatchCommands = Math.Min(UpdateBatchSize, dataRows.Length);

                if (maxBatchCommands < 1) {  // batch size of zero indicates one batch, no matter how large...
                    maxBatchCommands = dataRows.Length;
                }

                BatchCommandInfo[] batchCommands = new BatchCommandInfo[maxBatchCommands];
                DataRow[] rowBatch = new DataRow[maxBatchCommands];
                int commandCount = 0;

                // the outer try/finally is for closing any connections we may have opened
                try {
                    try {
                        if (1 != maxBatchCommands) {
                            InitializeBatching();
                        }
                        StatementType statementType = StatementType.Select;
                        IDbCommand dataCommand = null;

                        // for each row which is either insert, update, or delete
                        foreach(DataRow dataRow in dataRows) {
                            if (null == dataRow) {
                                continue; // foreach DataRow
                            }
                            bool isCommandFromRowUpdating = false;

                            // obtain the appropriate command
                            switch (dataRow.RowState) {
                            case DataRowState.Detached:
                            case DataRowState.Unchanged:
                                continue; // foreach DataRow
                            case DataRowState.Added:
                                statementType = StatementType.Insert;
                                dataCommand = _IDbDataAdapter.InsertCommand;
                                break;
                            case DataRowState.Deleted:
                                statementType = StatementType.Delete;
                                dataCommand = _IDbDataAdapter.DeleteCommand;
                                break;
                            case DataRowState.Modified:
                                statementType = StatementType.Update;
                                dataCommand = _IDbDataAdapter.UpdateCommand;
                                break;
                            default:
                                Debug.Assert(false, "InvalidDataRowState");
                                throw ADP.InvalidDataRowState(dataRow.RowState); // out of Update without completing batch
                            }

                            // setup the event to be raised
                            RowUpdatingEventArgs rowUpdatingEvent = CreateRowUpdatingEvent(dataRow, dataCommand, statementType, tableMapping);

                            // this try/catch for any exceptions during the parameter initialization
                            try {
                                dataRow.RowError = null; // MDAC 67185
                                if (null != dataCommand) {
                                    // prepare the parameters for the user who then can modify them during OnRowUpdating
                                    ParameterInput(dataCommand.Parameters, statementType, dataRow, tableMapping);
                                }
                            }
                            catch (Exception e) {
                                // 
                                if (!ADP.IsCatchableExceptionType(e)) {
                                    throw;
                                }

                                ADP.TraceExceptionForCapture(e);

                                rowUpdatingEvent.Errors = e;
                                rowUpdatingEvent.Status = UpdateStatus.ErrorsOccurred;
                            }

                            OnRowUpdating(rowUpdatingEvent); // user may throw out of Update without completing batch

                            IDbCommand tmpCommand = rowUpdatingEvent.Command;
                            isCommandFromRowUpdating = (dataCommand != tmpCommand);
                            dataCommand = tmpCommand;
                            tmpCommand = null;

                            // handle the status from RowUpdating event
                            UpdateStatus rowUpdatingStatus = rowUpdatingEvent.Status;
                            if (UpdateStatus.Continue != rowUpdatingStatus) {
                                if (UpdateStatus.ErrorsOccurred == rowUpdatingStatus) {
                                    UpdatingRowStatusErrors(rowUpdatingEvent, dataRow);
                                    continue; // foreach DataRow
                                }
                                else if (UpdateStatus.SkipCurrentRow == rowUpdatingStatus) {
                                    if (DataRowState.Unchanged == dataRow.RowState) { // MDAC 66286
                                        cumulativeDataRowsAffected++;
                                    }
                                    continue; // foreach DataRow
                                }
                                else if (UpdateStatus.SkipAllRemainingRows == rowUpdatingStatus) {
                                    if (DataRowState.Unchanged == dataRow.RowState) { // MDAC 66286
                                        cumulativeDataRowsAffected++;
                                    }
                                    break; // execute existing batch and return
                                }
                                else {
                                    throw ADP.InvalidUpdateStatus(rowUpdatingStatus);  // out of Update
                                }
                            }
                            // else onward to Append/ExecuteNonQuery/ExecuteReader

                            rowUpdatingEvent = null;
                            RowUpdatedEventArgs rowUpdatedEvent = null;

                            if (1 == maxBatchCommands) {
                                if (null != dataCommand) {
                                    batchCommands[0].CommandIdentifier = 0;
                                    batchCommands[0].ParameterCount = dataCommand.Parameters.Count;
                                    batchCommands[0].StatementType = statementType;
                                    batchCommands[0].UpdatedRowSource = dataCommand.UpdatedRowSource;
                                }
                                batchCommands[0].Row = dataRow;
                                rowBatch[0] = dataRow; // not doing a batch update, just simplifying code...
                                commandCount = 1;
                            }
                            else {
                                Exception errors = null;

                                try {
                                    if (null != dataCommand) {
                                        if (0 == (UpdateRowSource.FirstReturnedRecord & dataCommand.UpdatedRowSource)) {
                                            // append the command to the commandset. If an exception
                                            // occurs, then the user must append and continue

                                            batchCommands[commandCount].CommandIdentifier = AddToBatch(dataCommand);
                                            batchCommands[commandCount].ParameterCount    = dataCommand.Parameters.Count;
                                            batchCommands[commandCount].Row               = dataRow;
                                            batchCommands[commandCount].StatementType     = statementType;
                                            batchCommands[commandCount].UpdatedRowSource  = dataCommand.UpdatedRowSource;

                                            rowBatch[commandCount] = dataRow;
                                            commandCount++;

                                            if (commandCount < maxBatchCommands) {
                                                continue; // foreach DataRow
                                            }
                                            // else onward execute the batch
                                         }
                                        else {
                                            // do not allow the expectation that returned results will be used
                                            errors = ADP.ResultsNotAllowedDuringBatch();
                                        }
                                    }
                                    else {
                                        // null Command will force RowUpdatedEvent with ErrorsOccured without completing batch
                                        errors = ADP.UpdateRequiresCommand(statementType, isCommandFromRowUpdating);
                                    }
                                }
                                catch (Exception e) { // try/catch for RowUpdatedEventArgs
                                    // 
                                    if (!ADP.IsCatchableExceptionType(e)) {
                                        throw;
                                    }

                                    ADP.TraceExceptionForCapture(e);
                                    errors = e;
                                }

                                if (null != errors) {
                                    rowUpdatedEvent = CreateRowUpdatedEvent(dataRow, dataCommand, StatementType.Batch, tableMapping);
                                    rowUpdatedEvent.Errors = errors;
                                    rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;

                                    OnRowUpdated(rowUpdatedEvent); // user may throw out of Update
                                    if (errors != rowUpdatedEvent.Errors) { // user set the error msg and we will use it
                                        for(int i = 0; i < batchCommands.Length; ++i) {
                                            batchCommands[i].Errors = null;
                                        }
                                    }

                                    cumulativeDataRowsAffected += UpdatedRowStatus(rowUpdatedEvent, batchCommands, commandCount);
                                    if (UpdateStatus.SkipAllRemainingRows == rowUpdatedEvent.Status) {
                                        break;
                                    }
                                    continue; // foreach datarow
                                }
                            }

                            rowUpdatedEvent = CreateRowUpdatedEvent(dataRow, dataCommand, statementType, tableMapping);

                            // this try/catch for any exceptions during the execution, population, output parameters
                            try {
                                if (1 != maxBatchCommands) {
                                    IDbConnection connection = DbDataAdapter.GetConnection1(this);

                                    ConnectionState state = UpdateConnectionOpen(connection, StatementType.Batch, connections, connectionStates, useSelectConnectionState);
                                    rowUpdatedEvent.AdapterInit(rowBatch);

                                    if (ConnectionState.Open == state) {
                                        UpdateBatchExecute(batchCommands, commandCount, rowUpdatedEvent);
                                    }
                                    else {
                                        // null Connection will force RowUpdatedEvent with ErrorsOccured without completing batch
                                        rowUpdatedEvent.Errors = ADP.UpdateOpenConnectionRequired(StatementType.Batch, false, state);
                                        rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                                    }
                                }
                                else if (null != dataCommand) {
                                    IDbConnection connection = DbDataAdapter.GetConnection4(this, dataCommand, statementType, isCommandFromRowUpdating);
                                    ConnectionState state = UpdateConnectionOpen(connection, statementType, connections, connectionStates, useSelectConnectionState);
                                    if (ConnectionState.Open == state) {
                                        UpdateRowExecute(rowUpdatedEvent, dataCommand, statementType);
                                        batchCommands[0].RecordsAffected = rowUpdatedEvent.RecordsAffected;
                                        batchCommands[0].Errors = null;
                                    }
                                    else {
                                        // null Connection will force RowUpdatedEvent with ErrorsOccured without completing batch
                                        rowUpdatedEvent.Errors = ADP.UpdateOpenConnectionRequired(statementType, isCommandFromRowUpdating, state);
                                        rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                                    }
                                }
                                else {
                                    // null Command will force RowUpdatedEvent with ErrorsOccured without completing batch
                                    rowUpdatedEvent.Errors = ADP.UpdateRequiresCommand(statementType, isCommandFromRowUpdating);
                                    rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                                }
                            }
                            catch (Exception e) { // try/catch for RowUpdatedEventArgs
                                // 
                                if (!ADP.IsCatchableExceptionType(e)) {
                                    throw;
                                }

                                ADP.TraceExceptionForCapture(e);

                                rowUpdatedEvent.Errors = e;
                                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                            }
                            bool clearBatchOnSkipAll = (UpdateStatus.ErrorsOccurred == rowUpdatedEvent.Status);

                            {
                                Exception errors = rowUpdatedEvent.Errors;
                                OnRowUpdated(rowUpdatedEvent); // user may throw out of Update
                                // NOTE: the contents of rowBatch are now tainted...
                                if (errors != rowUpdatedEvent.Errors) { // user set the error msg and we will use it
                                    for(int i = 0; i < batchCommands.Length; ++i) {
                                        batchCommands[i].Errors = null;
                                    }
                                }
                            }
                            cumulativeDataRowsAffected += UpdatedRowStatus(rowUpdatedEvent, batchCommands, commandCount);

                            if (UpdateStatus.SkipAllRemainingRows == rowUpdatedEvent.Status) {
                                if (clearBatchOnSkipAll && 1 != maxBatchCommands) {
                                    ClearBatch();
                                    commandCount = 0;
                                }
                                break; // from update
                            }

                            if (1 != maxBatchCommands) {
                                ClearBatch();
                                commandCount = 0;
                            }
                            for(int i = 0; i < batchCommands.Length; ++i) {
                                batchCommands[i] = default(BatchCommandInfo);
                            }
                            commandCount = 0;
                        } // foreach DataRow

                        // must handle the last batch
                        if (1 != maxBatchCommands && 0 < commandCount) {
                            RowUpdatedEventArgs rowUpdatedEvent = CreateRowUpdatedEvent(null, dataCommand, statementType, tableMapping);

                            try {
                                IDbConnection connection = DbDataAdapter.GetConnection1(this);

                                ConnectionState state = UpdateConnectionOpen(connection, StatementType.Batch, connections, connectionStates, useSelectConnectionState);

                                DataRow[] finalRowBatch = rowBatch;

                                if (commandCount < rowBatch.Length) {
                                    finalRowBatch = new DataRow[commandCount];
                                    Array.Copy(rowBatch, finalRowBatch, commandCount);
                                }
                                rowUpdatedEvent.AdapterInit(finalRowBatch);

                                if (ConnectionState.Open == state) {
                                    UpdateBatchExecute(batchCommands, commandCount, rowUpdatedEvent);
                                }
                                else {
                                    // null Connection will force RowUpdatedEvent with ErrorsOccured without completing batch
                                    rowUpdatedEvent.Errors = ADP.UpdateOpenConnectionRequired(StatementType.Batch, false, state);
                                    rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                                }
                            }
                            catch (Exception e) { // try/catch for RowUpdatedEventArgs
                                // 
                                if (!ADP.IsCatchableExceptionType(e)) {
                                    throw;
                                }

                                ADP.TraceExceptionForCapture(e);

                                rowUpdatedEvent.Errors = e;
                                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                            }
                            Exception errors = rowUpdatedEvent.Errors;
                            OnRowUpdated(rowUpdatedEvent); // user may throw out of Update
                            // NOTE: the contents of rowBatch are now tainted...
                            if (errors != rowUpdatedEvent.Errors) { // user set the error msg and we will use it
                                for(int i = 0; i < batchCommands.Length; ++i) {
                                    batchCommands[i].Errors = null;
                                }
                            }

                            cumulativeDataRowsAffected += UpdatedRowStatus(rowUpdatedEvent, batchCommands, commandCount);
                        }
                    }
                    finally {
                        if (1 != maxBatchCommands) {
                            TerminateBatching();
                        }
                    }
                }
                finally { // try/finally for connection cleanup
                    for(int i = 0; i < connections.Length; ++i) {
                        QuietClose(connections[i], connectionStates[i]);
                    }
                }
                return cumulativeDataRowsAffected;
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private void UpdateBatchExecute(BatchCommandInfo[] batchCommands, int commandCount, RowUpdatedEventArgs rowUpdatedEvent) {
            try {
                // the batch execution may succeed, partially succeed and throw an exception (or not), or totally fail
                int recordsAffected = ExecuteBatch();
                rowUpdatedEvent.AdapterInit(recordsAffected);
            }
            catch(DbException e) {
                // an exception was thrown be but some part of the batch may have been succesfull
                ADP.TraceExceptionForCapture(e);
                rowUpdatedEvent.Errors = e;
                rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
            }
            Data.MissingMappingAction missingMapping = UpdateMappingAction;
            Data.MissingSchemaAction  missingSchema  = UpdateSchemaAction;

            int checkRecordsAffected = 0;
            bool hasConcurrencyViolation = false;
            List<DataRow> rows = null;

            // walk through the batch to build the sum of recordsAffected
            //      determine possible indivdual messages per datarow
            //      determine possible concurrency violations per datarow
            //      map output parameters to the datarow
            for(int bc = 0; bc < commandCount; ++bc) {
                BatchCommandInfo batchCommand = batchCommands[bc];
                StatementType statementType = batchCommand.StatementType;

                // default implementation always returns 1, derived classes must override
                // otherwise DbConcurrencyException will only be thrown if sum of all records in batch is 0
                int rowAffected;
                if (GetBatchedRecordsAffected(batchCommand.CommandIdentifier, out rowAffected, out batchCommands[bc].Errors)) {
                    batchCommands[bc].RecordsAffected = rowAffected;
                }

                if ((null == batchCommands[bc].Errors) && batchCommands[bc].RecordsAffected.HasValue) {
                    // determine possible concurrency violations per datarow
                    if ((StatementType.Update == statementType) || (StatementType.Delete == statementType)) {
                        checkRecordsAffected++;
                        if (0 == rowAffected) {
                            if (null == rows) {
                                rows = new List<DataRow>();
                            }
                            batchCommands[bc].Errors = ADP.UpdateConcurrencyViolation(batchCommands[bc].StatementType, 0, 1, new DataRow[] { rowUpdatedEvent.Rows[bc] });
                            hasConcurrencyViolation = true;
                            rows.Add(rowUpdatedEvent.Rows[bc]);
                        }
                    }

                    // map output parameters to the datarow
                    if (((StatementType.Insert == statementType) || (StatementType.Update == statementType))
                        && (0 != (UpdateRowSource.OutputParameters & batchCommand.UpdatedRowSource)) && (0 != rowAffected))  // MDAC 71174
                    {
                        if (StatementType.Insert == statementType) { // MDAC 64199
                            // AcceptChanges for 'added' rows so backend generated keys that are returned
                            // propagte into the datatable correctly.
                            rowUpdatedEvent.Rows[bc].AcceptChanges();
                        }

                        for(int i = 0; i < batchCommand.ParameterCount; ++i) {
                            IDataParameter parameter = GetBatchedParameter(batchCommand.CommandIdentifier, i);
                            ParameterOutput(parameter, batchCommand.Row, rowUpdatedEvent.TableMapping, missingMapping, missingSchema);
                        }
                    }
                }
            }

            if (null == rowUpdatedEvent.Errors) {
                // Only error if RecordsAffect == 0, not -1.  A value of -1 means no count was received from server,
                // do not error in that situation (means 'set nocount on' was executed on server).
                if (UpdateStatus.Continue == rowUpdatedEvent.Status) {
                    if ((0 < checkRecordsAffected) && ((0 == rowUpdatedEvent.RecordsAffected) || hasConcurrencyViolation)) {
                        // bug50526, an exception if no records affected and attempted an Update/Delete
                        Debug.Assert(null == rowUpdatedEvent.Errors, "Continue - but contains an exception");
                        DataRow[] rowsInError = (null != rows) ? rows.ToArray() : rowUpdatedEvent.Rows;
                        rowUpdatedEvent.Errors = ADP.UpdateConcurrencyViolation(StatementType.Batch, commandCount - rowsInError.Length, commandCount, rowsInError); // MDAC 55735
                        rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                    }
                }
            }
        }

        private ConnectionState UpdateConnectionOpen(IDbConnection connection, StatementType statementType, IDbConnection[] connections, ConnectionState[] connectionStates, bool useSelectConnectionState) {
            Debug.Assert(null != connection, "unexpected null connection");
            Debug.Assert(null != connection, "unexpected null connection");
            int index = (int)statementType;
            if (connection != connections[index]) {
                // if the user has changed the connection on the command object
                // and we had opened that connection, close that connection
                QuietClose(connections[index], connectionStates[index]);

                connections[index] = connection;
                connectionStates[index] = ConnectionState.Closed; // required, open may throw

                QuietOpen(connection, out connectionStates[index]);
                if (useSelectConnectionState && (connections[0] == connection)) {
                    connectionStates[index] = connections[0].State;
                }
            }
            return connection.State;
        }

        private int UpdateFromDataTable(DataTable dataTable, DataTableMapping tableMapping) {
            int rowsAffected = 0;
            DataRow[] dataRows = ADP.SelectAdapterRows(dataTable, false);
            if ((null != dataRows) && (0 < dataRows.Length)) {
                rowsAffected = Update(dataRows, tableMapping);
            }
            return rowsAffected;
        }

        private void UpdateRowExecute(RowUpdatedEventArgs rowUpdatedEvent, IDbCommand dataCommand, StatementType cmdIndex) {
            Debug.Assert(null != rowUpdatedEvent, "null rowUpdatedEvent");
            Debug.Assert(null != dataCommand, "null dataCommand");
            Debug.Assert(rowUpdatedEvent.Command == dataCommand, "dataCommand differs from rowUpdatedEvent");

            bool insertAcceptChanges = true;
            UpdateRowSource updatedRowSource = dataCommand.UpdatedRowSource;
            if ((StatementType.Delete == cmdIndex) || (0 == (UpdateRowSource.FirstReturnedRecord & updatedRowSource))) {
                int recordsAffected = dataCommand.ExecuteNonQuery(); // MDAC 88441
                rowUpdatedEvent.AdapterInit(recordsAffected);
            }
            else if ((StatementType.Insert == cmdIndex) || (StatementType.Update == cmdIndex)) {
                // we only care about the first row of the first result
                using(IDataReader dataReader = dataCommand.ExecuteReader(CommandBehavior.SequentialAccess)) {
                    DataReaderContainer readerHandler = DataReaderContainer.Create(dataReader, ReturnProviderSpecificTypes);
                    try {
                        bool getData = false;
                        do {
                            // advance to the first row returning result set
                            // determined by actually having columns in the result set
                            if (0 < readerHandler.FieldCount) {
                                getData = true;
                                break;
                            }
                        } while (dataReader.NextResult());

                        if (getData && (0 != dataReader.RecordsAffected)) { // MDAC 71174
                            SchemaMapping mapping = new SchemaMapping(this, null, rowUpdatedEvent.Row.Table, readerHandler, false, SchemaType.Mapped, rowUpdatedEvent.TableMapping.SourceTable, true, null, null);

                            if ((null != mapping.DataTable) && (null != mapping.DataValues)) {
                                if (dataReader.Read()) {
                                    if ((StatementType.Insert == cmdIndex) && insertAcceptChanges) { // MDAC 64199
                                        rowUpdatedEvent.Row.AcceptChanges();
                                        insertAcceptChanges = false;
                                    }
                                    mapping.ApplyToDataRow(rowUpdatedEvent.Row);
                                }
                            }
                        }
                    }
                    finally {
                        // using Close which can optimize its { while(dataReader.NextResult()); } loop
                        dataReader.Close();

                        // RecordsAffected is available after Close, but don't trust it after Dispose
                        int recordsAffected = dataReader.RecordsAffected;
                        rowUpdatedEvent.AdapterInit(recordsAffected);
                    }
                }
            }
            else {
                // StatementType.Select, StatementType.Batch
                Debug.Assert(false, "unexpected StatementType");
            }

            // map the parameter results to the dataSet
            if (((StatementType.Insert == cmdIndex) || (StatementType.Update == cmdIndex))
                && (0 != (UpdateRowSource.OutputParameters & updatedRowSource)) && (0 != rowUpdatedEvent.RecordsAffected)) { // MDAC 71174

                if ((StatementType.Insert == cmdIndex) && insertAcceptChanges) { // MDAC 64199
                    rowUpdatedEvent.Row.AcceptChanges();
                }
                ParameterOutput(dataCommand.Parameters, rowUpdatedEvent.Row, rowUpdatedEvent.TableMapping);
            }

            // Only error if RecordsAffect == 0, not -1.  A value of -1 means no count was received from server,
            // do not error in that situation (means 'set nocount on' was executed on server).
            switch(rowUpdatedEvent.Status) {
            case UpdateStatus.Continue:
                switch(cmdIndex) {
                case StatementType.Update:
                case StatementType.Delete:
                    if (0 == rowUpdatedEvent.RecordsAffected) {
                        // bug50526, an exception if no records affected and attempted an Update/Delete
                        Debug.Assert(null == rowUpdatedEvent.Errors, "Continue - but contains an exception");
                        rowUpdatedEvent.Errors = ADP.UpdateConcurrencyViolation(cmdIndex, rowUpdatedEvent.RecordsAffected, 1, new DataRow[] { rowUpdatedEvent.Row }); // MDAC 55735
                        rowUpdatedEvent.Status = UpdateStatus.ErrorsOccurred;
                    }
                    break;
                }
                break;
            }
        }

        private int UpdatedRowStatus(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount) {
            Debug.Assert(null != rowUpdatedEvent, "null rowUpdatedEvent");
            int cumulativeDataRowsAffected = 0;
            switch (rowUpdatedEvent.Status) {
            case UpdateStatus.Continue:
                cumulativeDataRowsAffected = UpdatedRowStatusContinue(rowUpdatedEvent, batchCommands, commandCount);
                break; // return to foreach DataRow
            case UpdateStatus.ErrorsOccurred:
                cumulativeDataRowsAffected = UpdatedRowStatusErrors(rowUpdatedEvent, batchCommands, commandCount);
                break; // no datarow affected if ErrorsOccured
            case UpdateStatus.SkipCurrentRow:
            case UpdateStatus.SkipAllRemainingRows: // cancel the Update method
                cumulativeDataRowsAffected = UpdatedRowStatusSkip(batchCommands, commandCount);
                break; // foreach DataRow without accepting changes on this row (but user may haved accepted chagnes for us)
            default:
                throw ADP.InvalidUpdateStatus(rowUpdatedEvent.Status);
            } // switch RowUpdatedEventArgs.Status
            return cumulativeDataRowsAffected;
        }

        private int UpdatedRowStatusContinue(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount) {
            Debug.Assert(null != batchCommands, "null batchCommands?");
            int cumulativeDataRowsAffected = 0;
            // 1. We delay accepting the changes until after we fire RowUpdatedEvent
            //    so the user has a chance to call RejectChanges for any given reason
            // 2. If the DataSource return 0 records affected, its an indication that
            //    the command didn't take so we don't want to automatically
            //    AcceptChanges.
            // With 'set nocount on' the count will be -1, accept changes in that case too.
            // 3.  Don't accept changes if no rows were affected, the user needs
            //     to know that there is a concurrency violation

            // Only accept changes if the row is not already accepted, ie detached.
            bool acdu = AcceptChangesDuringUpdate;
            for (int i = 0; i < commandCount; i++) {
                DataRow row = batchCommands[i].Row;
                if ((null == batchCommands[i].Errors) && batchCommands[i].RecordsAffected.HasValue && (0 != batchCommands[i].RecordsAffected.Value)) {
                    Debug.Assert(null != row, "null dataRow?");
                    if (acdu) {
                        if (0 != ((DataRowState.Added|DataRowState.Deleted|DataRowState.Modified)&row.RowState)) {
                            row.AcceptChanges();
                        }
                    }
                    cumulativeDataRowsAffected++;
                }
            }
            return cumulativeDataRowsAffected;
        }

        private int UpdatedRowStatusErrors(RowUpdatedEventArgs rowUpdatedEvent, BatchCommandInfo[] batchCommands, int commandCount) {
            Debug.Assert(null != batchCommands, "null batchCommands?");
            Exception errors = rowUpdatedEvent.Errors;
            if (null == errors) {
                // user changed status to ErrorsOccured without supplying an exception message
                errors = ADP.RowUpdatedErrors();
                rowUpdatedEvent.Errors = errors;
            }

            int affected = 0;
            bool done = false;
            string message = errors.Message;

            for (int i = 0; i < commandCount; i++) {
                DataRow row = batchCommands[i].Row;
                Debug.Assert(null != row, "null dataRow?");

                if (null != batchCommands[i].Errors) { // will exist if 0 == RecordsAffected
                    string rowMsg = batchCommands[i].Errors.Message;
                    if (String.IsNullOrEmpty(rowMsg)) {
                        rowMsg = message;
                    }
                    row.RowError += rowMsg;
                    done = true;
                }
            }
            if (!done) { // all rows are in 'error'
                for (int i = 0; i < commandCount; i++) {
                    DataRow row = batchCommands[i].Row;
                    // its possible a DBConcurrencyException exists and all rows have records affected
                    // via not overriding GetBatchedRecordsAffected or user setting the exception
                    row.RowError += message; // MDAC 65808
                }
            }
            else {
                affected = UpdatedRowStatusContinue(rowUpdatedEvent, batchCommands, commandCount);
            }
            if (!ContinueUpdateOnError) { // MDAC 66900
                throw errors; // out of Update
            }
            return affected; // return the count of successful rows within the batch failure
        }

        private int UpdatedRowStatusSkip(BatchCommandInfo[] batchCommands, int commandCount){
            Debug.Assert(null != batchCommands, "null batchCommands?");

            int cumulativeDataRowsAffected = 0;

            for (int i = 0; i < commandCount; i++) {
                DataRow row = batchCommands[i].Row;
                Debug.Assert(null != row, "null dataRow?");
                if (0 != ((DataRowState.Detached|DataRowState.Unchanged) & row.RowState)) {
                    cumulativeDataRowsAffected++; // MDAC 66286
                }
            }
            return cumulativeDataRowsAffected;
        }

        private void UpdatingRowStatusErrors(RowUpdatingEventArgs rowUpdatedEvent, DataRow dataRow) {
            Debug.Assert(null != dataRow, "null dataRow");
            Exception errors = rowUpdatedEvent.Errors;

            if (null == errors) {
                // user changed status to ErrorsOccured without supplying an exception message
                errors = ADP.RowUpdatingErrors();
                rowUpdatedEvent.Errors = errors;
            }
            string message = errors.Message;
            dataRow.RowError += message; // MDAC 65808

            if (!ContinueUpdateOnError) { // MDAC 66900
                throw errors; // out of Update
            }
        }

        static private IDbConnection GetConnection1(DbDataAdapter adapter) {
            IDbCommand command = adapter._IDbDataAdapter.SelectCommand;
            if (null == command) {
                command = adapter._IDbDataAdapter.InsertCommand;
                if (null == command) {
                    command = adapter._IDbDataAdapter.UpdateCommand;
                    if (null == command) {
                        command = adapter._IDbDataAdapter.DeleteCommand;
                    }
                }
            }
            IDbConnection connection = null;
            if (null != command) {
                connection = command.Connection;
            }
            if (null == connection) {
                throw ADP.UpdateConnectionRequired(StatementType.Batch, false);
            }
            return connection;
        }

        static private IDbConnection GetConnection3(DbDataAdapter adapter, IDbCommand command, string method) {
            Debug.Assert(null != command, "GetConnection3: null command");
            Debug.Assert(!ADP.IsEmpty(method), "missing method name");
            IDbConnection connection = command.Connection;
            if (null == connection) {
                throw ADP.ConnectionRequired_Res(method);
            }
            return connection;
        }

        static private IDbConnection GetConnection4(DbDataAdapter adapter, IDbCommand command, StatementType statementType, bool isCommandFromRowUpdating) {
            Debug.Assert(null != command, "GetConnection4: null command");
            IDbConnection connection = command.Connection;
            if (null == connection) {
                throw ADP.UpdateConnectionRequired(statementType, isCommandFromRowUpdating);
            }
            return connection;
        }
        static private DataRowVersion GetParameterSourceVersion(StatementType statementType, IDataParameter parameter) {
            switch (statementType) {
            case StatementType.Insert: return DataRowVersion.Current;  // ignores parameter.SourceVersion
            case StatementType.Update: return parameter.SourceVersion;
            case StatementType.Delete: return DataRowVersion.Original; // ignores parameter.SourceVersion
            case StatementType.Select:
            case StatementType.Batch:
                throw ADP.UnwantedStatementType(statementType);
            default:
                throw ADP.InvalidStatementType(statementType);
            }
        }

        static private void QuietClose(IDbConnection connection, ConnectionState originalState) {
            // close the connection if:
            // * it was closed on first use and adapter has opened it, AND
            // * provider's implementation did not ask to keep this connection open
            if ((null != connection) && (ConnectionState.Closed == originalState)) {
                // we don't have to check the current connection state because
                // it is supposed to be safe to call Close multiple times
                connection.Close();
            }
        }

        // QuietOpen needs to appear in the try {} finally { QuietClose } block
        // otherwise a possibility exists that an exception may be thrown, i.e. ThreadAbortException
        // where we would Open the connection and not close it
        static private void QuietOpen(IDbConnection connection, out ConnectionState originalState) {
            Debug.Assert(null != connection, "QuietOpen: null connection");
            originalState = connection.State;
            if (ConnectionState.Closed == originalState) {
                connection.Open();
            }
        }
    }
}
