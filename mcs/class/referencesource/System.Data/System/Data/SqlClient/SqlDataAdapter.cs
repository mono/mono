//------------------------------------------------------------------------------
// <copyright file="SqlDataAdapter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.SqlClient {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Diagnostics;

    [
    DefaultEvent("RowUpdated"),
    ToolboxItem("Microsoft.VSDesigner.Data.VS.SqlDataAdapterToolboxItem, " + AssemblyRef.MicrosoftVSDesigner),
    Designer("Microsoft.VSDesigner.Data.VS.SqlDataAdapterDesigner, " + AssemblyRef.MicrosoftVSDesigner)
    ]
    public sealed class SqlDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable {

        static private readonly object EventRowUpdated = new object();
        static private readonly object EventRowUpdating = new object();

        private SqlCommand _deleteCommand, _insertCommand, _selectCommand, _updateCommand;

        private SqlCommandSet       _commandSet;
        private int                 _updateBatchSize = 1;

        public SqlDataAdapter() : base() {
            GC.SuppressFinalize(this);
        }

        public SqlDataAdapter(SqlCommand selectCommand) : this() {
            SelectCommand = selectCommand;
        }

        public SqlDataAdapter(string selectCommandText, string selectConnectionString) : this() {
            SqlConnection connection = new SqlConnection(selectConnectionString);
            SelectCommand = new SqlCommand(selectCommandText, connection);
        }

        public SqlDataAdapter(string selectCommandText, SqlConnection selectConnection) : this() {
            SelectCommand = new SqlCommand(selectCommandText, selectConnection);
        }

        private SqlDataAdapter(SqlDataAdapter from) : base(from) { // Clone
            GC.SuppressFinalize(this);
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_DeleteCommand),
        ]
        new public SqlCommand DeleteCommand {
            get { return _deleteCommand; }
            set { _deleteCommand = value; }
        }

        IDbCommand IDbDataAdapter.DeleteCommand {
            get { return _deleteCommand; }
            set { _deleteCommand = (SqlCommand)value; }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_InsertCommand),
        ]
        new public SqlCommand InsertCommand {
            get { return _insertCommand; }
            set { _insertCommand = value; }
        }

        IDbCommand IDbDataAdapter.InsertCommand {
            get { return _insertCommand; }
            set { _insertCommand = (SqlCommand)value; }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Fill),
        ResDescriptionAttribute(Res.DbDataAdapter_SelectCommand),
        ]
        new public SqlCommand SelectCommand {
            get { return _selectCommand; }
            set { _selectCommand = value; }
        }

        IDbCommand IDbDataAdapter.SelectCommand {
            get { return _selectCommand; }
            set { _selectCommand = (SqlCommand)value; }
        }


        override public int UpdateBatchSize {
            get {
                return _updateBatchSize;
            }
            set {
                if (0 > value) { // WebData 98157
                    throw ADP.ArgumentOutOfRange("UpdateBatchSize");
                }
                _updateBatchSize = value;
                Bid.Trace("<sc.SqlDataAdapter.set_UpdateBatchSize|API> %d#, %d\n", ObjectID, value);
            }
        }

        [
        DefaultValue(null),
        Editor("Microsoft.VSDesigner.Data.Design.DBCommandEditor, " + AssemblyRef.MicrosoftVSDesigner, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_UpdateCommand),
        ]
        new public SqlCommand UpdateCommand {
            get { return _updateCommand; }
            set { _updateCommand = value; }
        }

        IDbCommand IDbDataAdapter.UpdateCommand {
            get { return _updateCommand; }
            set { _updateCommand = (SqlCommand)value; }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_RowUpdated),
        ]
        public event SqlRowUpdatedEventHandler RowUpdated {
            add {
                Events.AddHandler(EventRowUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventRowUpdated, value);
            }
        }

        [
        ResCategoryAttribute(Res.DataCategory_Update),
        ResDescriptionAttribute(Res.DbDataAdapter_RowUpdating),
        ]
        public event SqlRowUpdatingEventHandler RowUpdating {
            add {
                SqlRowUpdatingEventHandler handler = (SqlRowUpdatingEventHandler) Events[EventRowUpdating];

                // MDAC 58177, 64513
                // prevent someone from registering two different command builders on the adapter by
                // silently removing the old one
                if ((null != handler) && (value.Target is DbCommandBuilder)) {
                    SqlRowUpdatingEventHandler d = (SqlRowUpdatingEventHandler) ADP.FindBuilder(handler);
                    if (null != d) {
                        Events.RemoveHandler(EventRowUpdating, d);
                    }
                }
                Events.AddHandler(EventRowUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventRowUpdating, value);
            }
        }

        override protected int AddToBatch(IDbCommand command) {
            int commandIdentifier = _commandSet.CommandCount;
            _commandSet.Append((SqlCommand)command);
            return commandIdentifier;
        }

        override protected void ClearBatch() {
            _commandSet.Clear();
        }

        object ICloneable.Clone() {
            return new SqlDataAdapter(this);
        }

        override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            return new SqlRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) {
            return new SqlRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        override protected int ExecuteBatch() {
            Debug.Assert(null != _commandSet && (0 < _commandSet.CommandCount), "no commands");
            Bid.CorrelationTrace("<sc.SqlDataAdapter.ExecuteBatch|Info|Correlation> ObjectID%d#, ActivityID %ls\n", ObjectID);
            return _commandSet.ExecuteNonQuery();
        }

        override protected IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex) {
            Debug.Assert(commandIdentifier < _commandSet.CommandCount, "commandIdentifier out of range");
            Debug.Assert(parameterIndex < _commandSet.GetParameterCount(commandIdentifier), "parameter out of range");
            IDataParameter parameter = _commandSet.GetParameter(commandIdentifier, parameterIndex);
            return parameter;
        }
        
        override protected bool GetBatchedRecordsAffected(int commandIdentifier, out int recordsAffected, out Exception error) {
            Debug.Assert(commandIdentifier < _commandSet.CommandCount, "commandIdentifier out of range");
            return _commandSet.GetBatchedAffected(commandIdentifier, out recordsAffected, out error);
        }

        override protected void InitializeBatching() {
            Bid.Trace("<sc.SqlDataAdapter.InitializeBatching|API> %d#\n", ObjectID);
            _commandSet = new SqlCommandSet();
            SqlCommand command = SelectCommand;
            if (null == command) {
                command = InsertCommand;
                if (null == command) {
                    command = UpdateCommand;
                    if (null == command) {
                        command = DeleteCommand;
                    }
                }
            }
            if (null != command) {
                _commandSet.Connection = command.Connection;
                _commandSet.Transaction = command.Transaction;
                _commandSet.CommandTimeout = command.CommandTimeout;
            }
        }

        override protected void OnRowUpdated(RowUpdatedEventArgs value) {
            SqlRowUpdatedEventHandler handler = (SqlRowUpdatedEventHandler) Events[EventRowUpdated];
            if ((null != handler) && (value is SqlRowUpdatedEventArgs)) {
                handler(this, (SqlRowUpdatedEventArgs) value);
            }
            base.OnRowUpdated(value);
        }

        override protected void OnRowUpdating(RowUpdatingEventArgs value) {
            SqlRowUpdatingEventHandler handler = (SqlRowUpdatingEventHandler) Events[EventRowUpdating];
            if ((null != handler) && (value is SqlRowUpdatingEventArgs)) {
                handler(this, (SqlRowUpdatingEventArgs) value);
            }
            base.OnRowUpdating(value);
        }

        override protected void TerminateBatching() {
            if (null != _commandSet) {
                _commandSet.Dispose();
                _commandSet = null;
            }
        }
    }
}
