//------------------------------------------------------------------------------
// <copyright file="SqlDataSourceView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    using ConflictOptions = System.Web.UI.ConflictOptions;


    /// <devdoc>
    /// Represents a single view of a SqlDataSource.
    /// </devdoc>
    public class SqlDataSourceView : DataSourceView, IStateManager {

        private const int MustDeclareVariableSqlExceptionNumber = 137;
        private const int ProcedureExpectsParameterSqlExceptionNumber = 201;

        private static readonly object EventDeleted = new object();
        private static readonly object EventDeleting = new object();
        private static readonly object EventFiltering = new object();
        private static readonly object EventInserted = new object();
        private static readonly object EventInserting = new object();
        private static readonly object EventSelected = new object();
        private static readonly object EventSelecting = new object();
        private static readonly object EventUpdated = new object();
        private static readonly object EventUpdating = new object();

        private HttpContext _context;
        private SqlDataSource _owner;
        private bool _tracking;

        private bool _cancelSelectOnNullParameter = true;
        private ConflictOptions _conflictDetection = ConflictOptions.OverwriteChanges;
        private string _deleteCommand;
        private SqlDataSourceCommandType _deleteCommandType = SqlDataSourceCommandType.Text;
        private ParameterCollection _deleteParameters;
        private string _filterExpression;
        private ParameterCollection _filterParameters;
        private string _insertCommand;
        private SqlDataSourceCommandType _insertCommandType = SqlDataSourceCommandType.Text;
        private ParameterCollection _insertParameters;
        private string _oldValuesParameterFormatString;
        private string _selectCommand;
        private SqlDataSourceCommandType _selectCommandType = SqlDataSourceCommandType.Text;
        private ParameterCollection _selectParameters;
        private string _sortParameterName;
        private string _updateCommand;
        private SqlDataSourceCommandType _updateCommandType = SqlDataSourceCommandType.Text;
        private ParameterCollection _updateParameters;


        /// <devdoc>
        /// Creates a new instance of SqlDataSourceView.
        /// </devdoc>
        public SqlDataSourceView(SqlDataSource owner, string name, HttpContext context) : base(owner, name) {
            _owner = owner;
            _context = context;
        }


        public bool CancelSelectOnNullParameter {
            get {
                return _cancelSelectOnNullParameter;
            }
            set {
                if (CancelSelectOnNullParameter != value) {
                    _cancelSelectOnNullParameter = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Indicates that the view can delete rows.
        /// </devdoc>
        public override bool CanDelete {
            get {
                return (DeleteCommand.Length != 0);
            }
        }

        /// <devdoc>
        /// Indicates that the view can add new rows.
        /// </devdoc>
        public override bool CanInsert {
            get {
                return (InsertCommand.Length != 0);
            }
        }

        /// <devdoc>
        /// Indicates that the view can page the datasource on the server.
        /// </devdoc>
        public override bool CanPage {
            get {
                return false;
            }
        }

        /// <devdoc>
        /// Indicates that the view can return the total number of rows returned by the query.
        /// </devdoc>
        public override bool CanRetrieveTotalRowCount {
            get {
                return false;
            }
        }

        /// <devdoc>
        /// Indicates that the view can sort rows.
        /// </devdoc>
        public override bool CanSort {
            get {
                return (_owner.DataSourceMode == SqlDataSourceMode.DataSet) || (SortParameterName.Length > 0);
            }
        }

        /// <devdoc>
        /// Indicates that the view can update rows.
        /// </devdoc>
        public override bool CanUpdate {
            get {
                return (UpdateCommand.Length != 0);
            }
        }

        /// <devdoc>
        /// Whether commands pass old values in the parameter collection.
        /// </devdoc>
        public ConflictOptions ConflictDetection {
            get {
                return _conflictDetection;
            }
            set {
                if ((value < ConflictOptions.OverwriteChanges) || (value > ConflictOptions.CompareAllValues)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _conflictDetection = value;
                OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// The command to execute when Delete() is called on the SqlDataSourceView.
        /// </devdoc>
        public string DeleteCommand {
            get {
                if (_deleteCommand == null) {
                    return String.Empty;
                }
                return _deleteCommand;
            }
            set {
                _deleteCommand = value;
            }
        }

        public SqlDataSourceCommandType DeleteCommandType {
            get {
                return _deleteCommandType;
            }
            set {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _deleteCommandType = value;
            }
        }

        /// <devdoc>
        /// Collection of parameters used in Delete().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.SqlDataSource_DeleteParameters),
        ]
        public ParameterCollection DeleteParameters {
            get {
                if (_deleteParameters == null) {
                    _deleteParameters = new ParameterCollection();
                }
                return _deleteParameters;
            }
        }

        /// <devdoc>
        /// The filter to apply when Select() is called on the SqlDataSourceView.
        /// </devdoc>
        public string FilterExpression {
            get {
                if (_filterExpression == null) {
                    return String.Empty;
                }
                return _filterExpression;
            }
            set {
                if (FilterExpression != value) {
                    _filterExpression = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        /// <devdoc>
        /// Collection of parameters used in the FilterExpression property.
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.SqlDataSource_FilterParameters),
        ]
        public ParameterCollection FilterParameters {
            get {
                if (_filterParameters == null) {
                    _filterParameters = new ParameterCollection();

                    _filterParameters.ParametersChanged += new EventHandler(SelectParametersChangedEventHandler);

                    if (_tracking) {
                        ((IStateManager)_filterParameters).TrackViewState();
                    }
                }
                return _filterParameters;
            }
        }

        /// <devdoc>
        /// The command to execute when Insert() is called on the SqlDataSourceView.
        /// </devdoc>
        public string InsertCommand {
            get {
                if (_insertCommand == null) {
                    return String.Empty;
                }
                return _insertCommand;
            }
            set {
                _insertCommand = value;
            }
        }

        public SqlDataSourceCommandType InsertCommandType {
            get {
                return _insertCommandType;
            }
            set {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _insertCommandType = value;
            }
        }

        /// <devdoc>
        /// Collection of values used in Insert().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.SqlDataSource_InsertParameters),
        ]
        public ParameterCollection InsertParameters {
            get {
                if (_insertParameters == null) {
                    _insertParameters = new ParameterCollection();
                }
                return _insertParameters;
            }
        }

        /// <devdoc>
        /// Returns whether this object is tracking view state.
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return _tracking;
            }
        }

        /// <devdoc>
        /// The format string applied to the names of the old values parameters
        /// </devdoc>
        [
        DefaultValue("{0}"),
        WebCategory("Data"),
        WebSysDescription(SR.DataSource_OldValuesParameterFormatString),
        ]
        public string OldValuesParameterFormatString {
            get {
                if (_oldValuesParameterFormatString == null) {
                    return "{0}";
                }
                return _oldValuesParameterFormatString;
            }
            set {
                _oldValuesParameterFormatString = value;
                OnDataSourceViewChanged(EventArgs.Empty);
            }
        }

        /// <devdoc>
        /// Indicates the prefix for parameters.
        /// </devdoc>
        protected virtual string ParameterPrefix {
            get {
                if (String.IsNullOrEmpty(_owner.ProviderName) ||
                    String.Equals(_owner.ProviderName, "System.Data.SqlClient", StringComparison.OrdinalIgnoreCase)) {
                    return "@";
                }
                else {
                    return String.Empty;
                }
            }
        }

        /// <devdoc>
        /// The command to execute when Select() is called on the SqlDataSourceView.
        /// </devdoc>
        public string SelectCommand {
            get {
                if (_selectCommand == null) {
                    return String.Empty;
                }
                return _selectCommand;
            }
            set {
                if (SelectCommand != value) {
                    _selectCommand = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }

        public SqlDataSourceCommandType SelectCommandType {
            get {
                return _selectCommandType;
            }
            set {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _selectCommandType = value;
            }
        }


        /// <devdoc>
        /// The command to execute when Select is called on the SqlDataSourceView and the total rows is requested.
        /// </devdoc>
        /*public string SelectCountCommand {
            get {
                if (_selectCountCommand == null) {
                    return String.Empty;
                }
                return _selectCountCommand;
            }
            set {
                if (SelectCountCommand != value) {
                    _selectCountCommand = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }*/


        /// <devdoc>
        /// Collection of parameters used in Select().
        /// </devdoc>
        public ParameterCollection SelectParameters {
            get {
                if (_selectParameters == null) {
                    _selectParameters = new ParameterCollection();

                    _selectParameters.ParametersChanged += new EventHandler(SelectParametersChangedEventHandler);

                    if (_tracking) {
                        ((IStateManager)_selectParameters).TrackViewState();
                    }
                }
                return _selectParameters;
            }
        }


        /// <devdoc>
        /// The name of the parameter in the SelectCommand that specifies the
        /// sort expression. This parameter's value will be automatically set
        /// at runtime with the appropriate sort expression. This is only
        /// supported for stored procedure commands.
        /// </devdoc>
        public string SortParameterName {
            get {
                if (_sortParameterName == null) {
                    return String.Empty;
                }
                return _sortParameterName;
            }
            set {
                if (SortParameterName != value) {
                    _sortParameterName = value;
                    OnDataSourceViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// The command to execute when Update() is called on the SqlDataSourceView.
        /// </devdoc>
        public string UpdateCommand {
            get {
                if (_updateCommand == null) {
                    return String.Empty;
                }
                return _updateCommand;
            }
            set {
                _updateCommand = value;
            }
        }

        public SqlDataSourceCommandType UpdateCommandType {
            get {
                return _updateCommandType;
            }
            set {
                if ((value < SqlDataSourceCommandType.Text) || (value > SqlDataSourceCommandType.StoredProcedure)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _updateCommandType = value;
            }
        }

        /// <devdoc>
        /// Collection of parameters used in Update().
        /// </devdoc>
        [
        DefaultValue(null),
        Editor("System.Web.UI.Design.WebControls.ParameterCollectionEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.SqlDataSource_UpdateParameters),
        ]
        public ParameterCollection UpdateParameters {
            get {
                if (_updateParameters == null) {
                    _updateParameters = new ParameterCollection();
                }
                return _updateParameters;
            }
        }


        /// <devdoc>
        /// This event is raised after the Delete operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        public event SqlDataSourceStatusEventHandler Deleted {
            add {
                Events.AddHandler(EventDeleted, value);
            }
            remove {
                Events.RemoveHandler(EventDeleted, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Delete operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        public event SqlDataSourceCommandEventHandler Deleting {
            add {
                Events.AddHandler(EventDeleting, value);
            }
            remove {
                Events.RemoveHandler(EventDeleting, value);
            }
        }

        public event SqlDataSourceFilteringEventHandler Filtering {
            add {
                Events.AddHandler(EventFiltering, value);
            }
            remove {
                Events.RemoveHandler(EventFiltering, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Insert operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        public event SqlDataSourceStatusEventHandler Inserted {
            add {
                Events.AddHandler(EventInserted, value);
            }
            remove {
                Events.RemoveHandler(EventInserted, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Insert operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        public event SqlDataSourceCommandEventHandler Inserting {
            add {
                Events.AddHandler(EventInserting, value);
            }
            remove {
                Events.RemoveHandler(EventInserting, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Select operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        public event SqlDataSourceStatusEventHandler Selected {
            add {
                Events.AddHandler(EventSelected, value);
            }
            remove {
                Events.RemoveHandler(EventSelected, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Select operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        public event SqlDataSourceSelectingEventHandler Selecting {
            add {
                Events.AddHandler(EventSelecting, value);
            }
            remove {
                Events.RemoveHandler(EventSelecting, value);
            }
        }

        /// <devdoc>
        /// This event is raised after the Update operation has completed.
        /// Handle this event if you need to examine the values of output parameters.
        /// </devdoc>
        public event SqlDataSourceStatusEventHandler Updated {
            add {
                Events.AddHandler(EventUpdated, value);
            }
            remove {
                Events.RemoveHandler(EventUpdated, value);
            }
        }

        /// <devdoc>
        /// This event is raised before the Update operation has been executed.
        /// Handle this event if you want to perform additional initialization operations
        /// that are specific to your application. You can also handle this event if you
        /// need to validate the values of parameters or change their values.
        /// When this event is raised, the database connection is not open yet, and you
        /// can cancel the event by setting the Cancel property of the DataCommandEventArgs
        /// to true.
        /// </devdoc>
        public event SqlDataSourceCommandEventHandler Updating {
            add {
                Events.AddHandler(EventUpdating, value);
            }
            remove {
                Events.RemoveHandler(EventUpdating, value);
            }
        }


        /// <devdoc>
        /// Adds parameters to an DbCommand from an IOrderedDictionary.
        /// The exclusion list contains parameter names that should not be added
        /// to the command's parameter collection.
        /// </devdoc>
        private void AddParameters(DbCommand command, ParameterCollection reference, IDictionary parameters, IDictionary exclusionList, string oldValuesParameterFormatString) {
            Debug.Assert(command != null);

            IDictionary caseInsensitiveExclusionList = null;
            if (exclusionList != null) {
                caseInsensitiveExclusionList = new ListDictionary(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry de in exclusionList) {
                    caseInsensitiveExclusionList.Add(de.Key, de.Value);
                }
            }

            if (parameters != null) {
                string parameterPrefix = ParameterPrefix;
                foreach (DictionaryEntry de in parameters) {
                    string rawParamName = (string)de.Key;

                    if ((caseInsensitiveExclusionList != null) && (caseInsensitiveExclusionList.Contains(rawParamName))) {
                        // If we have an exclusion list and it contains this parameter, skip it
                        continue;
                    }

                    string formattedParamName;
                    if (oldValuesParameterFormatString == null) {
                        formattedParamName = rawParamName;
                    }
                    else {
                        formattedParamName = String.Format(CultureInfo.InvariantCulture, oldValuesParameterFormatString, rawParamName);
                    }
                    object value = de.Value;

                    // If the reference collection contains this parameter, we will use
                    // the Parameter's settings to format the value
                    Parameter parameter = reference[formattedParamName];
                    if (parameter != null) {
                        value = parameter.GetValue(de.Value, false);
                    }

                    formattedParamName = parameterPrefix + formattedParamName;

                    if (command.Parameters.Contains(formattedParamName)) {
                        // We never overwrite an existing value with a null value
                        if (value != null) {
                            command.Parameters[formattedParamName].Value = value;
                        }
                    }
                    else {
                        // Parameter does not exist, add a new one
                        DbParameter dbParameter = _owner.CreateParameter(formattedParamName, value);
                        command.Parameters.Add(dbParameter);
                    }
                }
            }
        }

        /// <devdoc>
        /// Builds a custom exception for specific database errors.
        /// Currently the only custom exception text supported is for SQL Server
        /// when a parameter is present in the command but not in the parameters
        /// collection.
        /// The isCustomException parameter indicates whether a custom exception
        /// was created or not. This way the caller can determine whether it wants
        /// to rethrow the original exception or throw the new custom exception.
        /// </devdoc>
        private Exception BuildCustomException(Exception ex, DataSourceOperation operation, DbCommand command, out bool isCustomException) {
            System.Data.SqlClient.SqlException sqlException = ex as System.Data.SqlClient.SqlException;
            if (sqlException != null) {
                if ((sqlException.Number == MustDeclareVariableSqlExceptionNumber) ||
                    (sqlException.Number == ProcedureExpectsParameterSqlExceptionNumber)) {
                    string parameterNames;
                    if (command.Parameters.Count > 0) {
                        StringBuilder sb = new StringBuilder();
                        bool firstParameter = true;
                        foreach (DbParameter p in command.Parameters) {
                            if (!firstParameter) {
                                sb.Append(", ");
                            }
                            sb.Append(p.ParameterName);
                            firstParameter = false;
                        }
                        parameterNames = sb.ToString();
                    }
                    else {
                        parameterNames = SR.GetString(SR.SqlDataSourceView_NoParameters);
                    }
                    isCustomException = true;
                    return new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_MissingParameters, operation, _owner.ID, parameterNames));
                }
            }
            isCustomException = false;
            return ex;
        }

        public int Delete(IDictionary keys, IDictionary oldValues) {
            return ExecuteDelete(keys, oldValues);
        }

        /// <devdoc>
        /// Executes a DbCommand and returns the number of rows affected.
        /// </devdoc>
        private int ExecuteDbCommand(DbCommand command, DataSourceOperation operation) {
            int rowsAffected = 0;

            bool eventRaised = false;
            try {
                if (command.Connection.State != ConnectionState.Open) {
                    command.Connection.Open();
                }

                rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0) {
                    OnDataSourceViewChanged(EventArgs.Empty);

                    DataSourceCache cache = _owner.Cache;
                    if ((cache != null) && (cache.Enabled)) {
                        _owner.InvalidateCacheEntry();
                    }
                }

                // Raise appropriate event
                eventRaised = true;
                SqlDataSourceStatusEventArgs eventArgs = new SqlDataSourceStatusEventArgs(command, rowsAffected, null);
                switch (operation) {
                    case DataSourceOperation.Delete:
                        OnDeleted(eventArgs);
                        break;
                    case DataSourceOperation.Insert:
                        OnInserted(eventArgs);
                        break;
                    case DataSourceOperation.Update:
                        OnUpdated(eventArgs);
                        break;
                }
            }
            catch (Exception ex) {
                if (!eventRaised) {
                    // Raise appropriate event
                    SqlDataSourceStatusEventArgs eventArgs = new SqlDataSourceStatusEventArgs(command, rowsAffected, ex);
                    switch (operation) {
                        case DataSourceOperation.Delete:
                            OnDeleted(eventArgs);
                            break;
                        case DataSourceOperation.Insert:
                            OnInserted(eventArgs);
                            break;
                        case DataSourceOperation.Update:
                            OnUpdated(eventArgs);
                            break;
                    }
                    if (!eventArgs.ExceptionHandled) {
                        throw;
                    }
                }
                else {
                    bool isCustomException;
                    ex = BuildCustomException(ex, operation, command, out isCustomException);
                    if (isCustomException) {
                        throw ex;
                    }
                    else {
                        throw;
                    }
                }
            }
            finally {
                if (command.Connection.State == ConnectionState.Open) {
                    command.Connection.Close();
                }
            }

            return rowsAffected;
        }


        /// <devdoc>
        /// Deletes rows from the data source with given parameters.
        /// </devdoc>
        protected override int ExecuteDelete(IDictionary keys, IDictionary oldValues) {
            if (!CanDelete) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_DeleteNotSupported, _owner.ID));
            }

            DbConnection connection = _owner.CreateConnection(_owner.ConnectionString);

            if (connection == null) {
                throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_CouldNotCreateConnection, _owner.ID));
            }

            // Create command and add parameters
            string oldValuesParameterFormatString = OldValuesParameterFormatString;
            DbCommand command = _owner.CreateCommand(DeleteCommand, connection);
            InitializeParameters(command, DeleteParameters, oldValues);
            AddParameters(command, DeleteParameters, keys, null, oldValuesParameterFormatString);
            if (ConflictDetection == ConflictOptions.CompareAllValues) {
                if (oldValues == null || oldValues.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_delete), _owner.ID, "values"));
                }
                AddParameters(command, DeleteParameters, oldValues, null, oldValuesParameterFormatString);
            }
            command.CommandType = GetCommandType(DeleteCommandType);

            // Raise event to allow customization and cancellation
            SqlDataSourceCommandEventArgs eventArgs = new SqlDataSourceCommandEventArgs(command);
            OnDeleting(eventArgs);

            // If the operation was cancelled, exit immediately
            if (eventArgs.Cancel) {
                return 0;
            }

            // Replace null values in parameters with DBNull.Value
            ReplaceNullValues(command);

            return ExecuteDbCommand(command, DataSourceOperation.Delete);
        }


        /// <devdoc>
        /// Inserts a new row with data from a name/value collection.
        /// </devdoc>
        protected override int ExecuteInsert(IDictionary values) {
            if (!CanInsert) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_InsertNotSupported, _owner.ID));
            }

            DbConnection connection = _owner.CreateConnection(_owner.ConnectionString);

            if (connection == null) {
                throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_CouldNotCreateConnection, _owner.ID));
            }

            // Create command and add parameters
            DbCommand command = _owner.CreateCommand(InsertCommand, connection);
            InitializeParameters(command, InsertParameters, null);
            AddParameters(command, InsertParameters, values, null, null);
            command.CommandType = GetCommandType(InsertCommandType);

            // Raise event to allow customization and cancellation
            SqlDataSourceCommandEventArgs eventArgs = new SqlDataSourceCommandEventArgs(command);
            OnInserting(eventArgs);

            // If the operation was cancelled, exit immediately
            if (eventArgs.Cancel) {
                return 0;
            }

            // Replace null values in parameters with DBNull.Value
            ReplaceNullValues(command);

            return ExecuteDbCommand(command, DataSourceOperation.Insert);
        }


        /// <devdoc>
        /// Returns all the rows of the datasource.
        /// Parameters are taken from the SqlDataSource.Parameters property collection.
        /// If DataSourceMode is set to DataSet then a DataView is returned.
        /// If DataSourceMode is set to DataReader then a DataReader is returned, and it must be closed when done.
        /// </devdoc>
        protected internal override IEnumerable ExecuteSelect(DataSourceSelectArguments arguments) {
            if (SelectCommand.Length == 0) {
                return null;
            }

            DbConnection connection = _owner.CreateConnection(_owner.ConnectionString);

            if (connection == null) {
                throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_CouldNotCreateConnection, _owner.ID));
            }

            DataSourceCache cache = _owner.Cache;
            bool cacheEnabled = (cache != null) && (cache.Enabled);
            //int startRowIndex = arguments.StartRowIndex;
            //int maximumRows = arguments.MaximumRows;
            string sortExpression = arguments.SortExpression;

            if (CanPage) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Page);
            }

            if (CanSort) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.Sort);
            }

            if (CanRetrieveTotalRowCount) {
                arguments.AddSupportedCapabilities(DataSourceCapabilities.RetrieveTotalRowCount);
            }


            // If caching is enabled, load DataSet from cache
            if (cacheEnabled) {
                if (_owner.DataSourceMode != SqlDataSourceMode.DataSet) {
                    throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_CacheNotSupported, _owner.ID));
                }

                arguments.RaiseUnsupportedCapabilitiesError(this);

                DataSet dataSet = _owner.LoadDataFromCache(0, -1) as DataSet;

                if (dataSet != null) {
                    /*if (arguments.RetrieveTotalRowCount) {
                        int cachedTotalRowCount = _owner.LoadTotalRowCountFromCache();
                        if (cachedTotalRowCount >= 0) {
                            arguments.TotalRowCount = cachedTotalRowCount;
                        }
                        else {
                            // query for row count and then save it in cache
                            cachedTotalRowCount = QueryTotalRowCount(connection, arguments);
                            arguments.TotalRowCount = cachedTotalRowCount;
                            _owner.SaveTotalRowCountToCache(cachedTotalRowCount);
                        }
                    }*/
                    IOrderedDictionary parameterValues = FilterParameters.GetValues(_context, _owner);
                    if (FilterExpression.Length > 0) {
                        SqlDataSourceFilteringEventArgs filterArgs = new SqlDataSourceFilteringEventArgs(parameterValues);
                        OnFiltering(filterArgs);
                        if (filterArgs.Cancel) {
                            return null;
                        }
                    }
                    return FilteredDataSetHelper.CreateFilteredDataView(dataSet.Tables[0], sortExpression, FilterExpression, parameterValues);
                }
            }

            // Create command and add parameters
            DbCommand command = _owner.CreateCommand(SelectCommand, connection);
            InitializeParameters(command, SelectParameters, null);
            command.CommandType = GetCommandType(SelectCommandType);

            // Raise event to allow customization and cancellation
            SqlDataSourceSelectingEventArgs selectingEventArgs = new SqlDataSourceSelectingEventArgs(command, arguments);
            OnSelecting(selectingEventArgs);

            // If the operation was cancelled, exit immediately
            if (selectingEventArgs.Cancel) {
                return null;
            }

            // Add the sort parameter to allow for custom stored procedure sorting, if necessary
            string sortParameterName = SortParameterName;
            if (sortParameterName.Length > 0) {
                if (command.CommandType != CommandType.StoredProcedure) {
                    throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_SortParameterRequiresStoredProcedure, _owner.ID));
                }
                command.Parameters.Add(_owner.CreateParameter(ParameterPrefix + sortParameterName, sortExpression));

                // We reset the sort expression here so that we pretend as
                // though we're not really sorting (since the developer is
                // worrying about it instead of us).
                arguments.SortExpression = String.Empty;
            }

            arguments.RaiseUnsupportedCapabilitiesError(this);

            // reset these values, since they might have changed in the OnSelecting event
            sortExpression = arguments.SortExpression;
            //startRowIndex = arguments.StartRowIndex;
            //maximumRows = arguments.MaximumRows;

            // Perform null check if user wants to cancel on any null parameter value
            if (CancelSelectOnNullParameter) {
                int paramCount = command.Parameters.Count;
                for (int i = 0; i < paramCount; i++) {
                    DbParameter parameter = command.Parameters[i];
                    if ((parameter != null) &&
                        (parameter.Value == null) &&
                        ((parameter.Direction == ParameterDirection.Input) || (parameter.Direction == ParameterDirection.InputOutput))) {
                        return null;
                    }
                }
            }

            // Replace null values in parameters with DBNull.Value
            ReplaceNullValues(command);

            /*if (arguments.RetrieveTotalRowCount && SelectCountCommand.Length > 0) {
                int cachedTotalRowCount = -1;
                if (cacheEnabled) {
                    cachedTotalRowCount = _owner.LoadTotalRowCountFromCache();
                    if (cachedTotalRowCount >= 0) {
                        arguments.TotalRowCount = cachedTotalRowCount;
                    }
                }
                if (cachedTotalRowCount < 0) {
                    cachedTotalRowCount = QueryTotalRowCount(connection, arguments);
                    arguments.TotalRowCount = cachedTotalRowCount;
                    if (cacheEnabled) {
                        _owner.SaveTotalRowCountToCache(cachedTotalRowCount);
                    }
                }
            }*/

            IEnumerable selectResult = null;

            switch (_owner.DataSourceMode) {
                case SqlDataSourceMode.DataSet:
                {
                    SqlCacheDependency cacheDependency = null;
                    if (cacheEnabled && cache is SqlDataSourceCache) {
                        SqlDataSourceCache sqlCache = (SqlDataSourceCache)cache;
                        if (String.Equals(sqlCache.SqlCacheDependency, SqlDataSourceCache.Sql9CacheDependencyDirective, StringComparison.OrdinalIgnoreCase)) {
                            if (!(command is System.Data.SqlClient.SqlCommand)) {
                                throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_CommandNotificationNotSupported, _owner.ID));
                            }
                            cacheDependency = new SqlCacheDependency((System.Data.SqlClient.SqlCommand)command);
                        }
                    }

                    DbDataAdapter adapter = _owner.CreateDataAdapter(command);

                    DataSet dataSet = new DataSet();
                    int rowsAffected = 0;

                    bool eventRaised = false;
                    try {
                        rowsAffected = adapter.Fill(dataSet, Name);

                        // Raise the Selected event
                        eventRaised = true;
                        SqlDataSourceStatusEventArgs selectedEventArgs = new SqlDataSourceStatusEventArgs(command, rowsAffected, null);
                        OnSelected(selectedEventArgs);
                    }
                    catch (Exception ex) {
                        if (!eventRaised) {
                            // Raise the Selected event
                            SqlDataSourceStatusEventArgs selectedEventArgs = new SqlDataSourceStatusEventArgs(command, rowsAffected, ex);
                            OnSelected(selectedEventArgs);
                            if (!selectedEventArgs.ExceptionHandled) {
                                throw;
                            }
                        }
                        else {
                            bool isCustomException;
                            ex = BuildCustomException(ex, DataSourceOperation.Select, command, out isCustomException);
                            if (isCustomException) {
                                throw ex;
                            }
                            else {
                                throw;
                            }
                        }
                    }
                    finally {
                        if (connection.State == ConnectionState.Open) {
                            connection.Close();
                        }
                    }

                    // If caching is enabled, save DataSet to cache
                    DataTable dataTable = (dataSet.Tables.Count > 0 ? dataSet.Tables[0] : null);
                    if (cacheEnabled && dataTable != null) {
                        _owner.SaveDataToCache(0, -1, dataSet, cacheDependency);
                    }

                    if (dataTable != null) {
                        IOrderedDictionary parameterValues = FilterParameters.GetValues(_context, _owner);
                        if (FilterExpression.Length > 0) {
                            SqlDataSourceFilteringEventArgs filterArgs = new SqlDataSourceFilteringEventArgs(parameterValues);
                            OnFiltering(filterArgs);
                            if (filterArgs.Cancel) {
                                return null;
                            }
                        }
                        selectResult = FilteredDataSetHelper.CreateFilteredDataView(dataTable, sortExpression, FilterExpression, parameterValues);
                    }
                    break;
                }

                case SqlDataSourceMode.DataReader:
                {
                    if (FilterExpression.Length > 0) {
                        throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_FilterNotSupported, _owner.ID));
                    }

                    if (sortExpression.Length > 0) {
                        throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_SortNotSupported, _owner.ID));
                    }

                    bool eventRaised = false;
                    try {
                        if (connection.State != ConnectionState.Open) {
                            connection.Open();
                        }

                        selectResult = command.ExecuteReader(CommandBehavior.CloseConnection);

                        // Raise the Selected event
                        eventRaised = true;
                        SqlDataSourceStatusEventArgs selectedEventArgs = new SqlDataSourceStatusEventArgs(command, 0, null);
                        OnSelected(selectedEventArgs);
                    }
                    catch (Exception ex) {
                        if (!eventRaised) {
                            // Raise the Selected event
                            SqlDataSourceStatusEventArgs selectedEventArgs = new SqlDataSourceStatusEventArgs(command, 0, ex);
                            OnSelected(selectedEventArgs);
                            if (!selectedEventArgs.ExceptionHandled) {
                                throw;
                            }
                        }
                        else {
                            bool isCustomException;
                            ex = BuildCustomException(ex, DataSourceOperation.Select, command, out isCustomException);
                            if (isCustomException) {
                                throw ex;
                            }
                            else {
                                throw;
                            }
                        }
                    }
                    break;
                }
            }
            return selectResult;
        }


        /// <devdoc>
        /// Updates rows matching the parameter collection and setting new values from the name/value values collection.
        /// </devdoc>
        protected override int ExecuteUpdate(IDictionary keys, IDictionary values, IDictionary oldValues) {
            if (!CanUpdate) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_UpdateNotSupported, _owner.ID));
            }

            DbConnection connection = _owner.CreateConnection(_owner.ConnectionString);

            if (connection == null) {
                throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_CouldNotCreateConnection, _owner.ID));
            }

            // Create command and add parameters
            string oldValuesParameterFormatString = OldValuesParameterFormatString;
            DbCommand command = _owner.CreateCommand(UpdateCommand, connection);
            InitializeParameters(command, UpdateParameters, keys);
            AddParameters(command, UpdateParameters, values, null, null);
            AddParameters(command, UpdateParameters, keys, null, oldValuesParameterFormatString);
            if (ConflictDetection == ConflictOptions.CompareAllValues) {
                if (oldValues == null || oldValues.Count == 0) {
                    throw new InvalidOperationException(SR.GetString(SR.SqlDataSourceView_Pessimistic, SR.GetString(SR.DataSourceView_update), _owner.ID, "oldValues"));
                }
                AddParameters(command, UpdateParameters, oldValues, null, oldValuesParameterFormatString);
            }
            command.CommandType = GetCommandType(UpdateCommandType);

            // Raise event to allow customization and cancellation
            SqlDataSourceCommandEventArgs eventArgs = new SqlDataSourceCommandEventArgs(command);
            OnUpdating(eventArgs);

            // If the operation was cancelled, exit immediately
            if (eventArgs.Cancel) {
                return 0;
            }

            // Replace null values in parameters with DBNull.Value
            ReplaceNullValues(command);

            return ExecuteDbCommand(command, DataSourceOperation.Update);
        }

        /// <devdoc>
        /// Converts a SqlDataSourceCommandType to a System.Data.CommandType.
        /// </devdoc>
        private static CommandType GetCommandType(SqlDataSourceCommandType commandType) {
            if (commandType == SqlDataSourceCommandType.Text) {
                return CommandType.Text;
            }
            return CommandType.StoredProcedure;
        }

        /// <devdoc>
        /// Initializes a DbCommand with parameters from a ParameterCollection.
        /// The exclusion list contains parameter names that should not be added
        /// to the command's parameter collection.
        /// </devdoc>
        private void InitializeParameters(DbCommand command, ParameterCollection parameters, IDictionary exclusionList) {
            Debug.Assert(command != null);
            Debug.Assert(parameters != null);

            string parameterPrefix = ParameterPrefix;

            IDictionary caseInsensitiveExclusionList = null;
            if (exclusionList != null) {
                caseInsensitiveExclusionList = new ListDictionary(StringComparer.OrdinalIgnoreCase);
                foreach (DictionaryEntry de in exclusionList) {
                    caseInsensitiveExclusionList.Add(de.Key, de.Value);
                }
            }

            IOrderedDictionary values = parameters.GetValues(_context, _owner);
            for (int i = 0; i < parameters.Count; i++) {
                Parameter parameter = parameters[i];
                if ((caseInsensitiveExclusionList == null) || (!caseInsensitiveExclusionList.Contains(parameter.Name))) {
                    DbParameter dbParameter = _owner.CreateParameter(parameterPrefix + parameter.Name, values[i]);
                    dbParameter.Direction = parameter.Direction;
                    dbParameter.Size = parameter.Size;
                    if (parameter.DbType != DbType.Object || (parameter.Type != TypeCode.Empty && parameter.Type != TypeCode.DBNull)) {
                        SqlParameter sqlParameter = dbParameter as SqlParameter;
                        if (sqlParameter == null) {
                            dbParameter.DbType = parameter.GetDatabaseType();
                        }
                        else {
                            // In Whidbey, the DbType Date and Time members mapped to SqlDbType.DateTime since there
                            // were no SqlDbType equivalents. SqlDbType has since been modified to include the new
                            // Katmai types, including Date and Time. For backwards compatability SqlParameter's DbType
                            // setter doesn't support Date and Time, so the SqlDbType property should be used instead.
                            // Other new SqlServer 2008 types (DateTime2, DateTimeOffset) can be set using DbType.
                            DbType dbType = parameter.GetDatabaseType();
                            switch (dbType) {
                                case DbType.Time:
                                    sqlParameter.SqlDbType = SqlDbType.Time;
                                    break;
                                case DbType.Date:
                                    sqlParameter.SqlDbType = SqlDbType.Date;
                                    break;
                                default:
                                    dbParameter.DbType = parameter.GetDatabaseType();
                                    break;
                            }
                        }
                    }
                    command.Parameters.Add(dbParameter);
                }
            }
        }

        public int Insert(IDictionary values) {
            return ExecuteInsert(values);
        }


        /// <devdoc>
        /// Loads view state.
        /// </devdoc>
        protected virtual void LoadViewState(object savedState) {
            if (savedState == null)
                return;

            Pair myState = (Pair)savedState;

            if (myState.First != null)
                ((IStateManager)SelectParameters).LoadViewState(myState.First);

            if (myState.Second != null)
                ((IStateManager)FilterParameters).LoadViewState(myState.Second);
        }


        /// <devdoc>
        /// Raises the Deleted event.
        /// </devdoc>
        protected virtual void OnDeleted(SqlDataSourceStatusEventArgs e) {
            SqlDataSourceStatusEventHandler handler = Events[EventDeleted] as SqlDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Deleting event.
        /// </devdoc>
        protected virtual void OnDeleting(SqlDataSourceCommandEventArgs e) {
            SqlDataSourceCommandEventHandler handler = Events[EventDeleting] as SqlDataSourceCommandEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void OnFiltering(SqlDataSourceFilteringEventArgs e) {
            SqlDataSourceFilteringEventHandler handler = Events[EventFiltering] as SqlDataSourceFilteringEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Inserted event.
        /// </devdoc>
        protected virtual void OnInserted(SqlDataSourceStatusEventArgs e) {
            SqlDataSourceStatusEventHandler handler = Events[EventInserted] as SqlDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Inserting event.
        /// </devdoc>
        protected virtual void OnInserting(SqlDataSourceCommandEventArgs e) {
            SqlDataSourceCommandEventHandler handler = Events[EventInserting] as SqlDataSourceCommandEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Selected event.
        /// </devdoc>
        protected virtual void OnSelected(SqlDataSourceStatusEventArgs e) {
            SqlDataSourceStatusEventHandler handler = Events[EventSelected] as SqlDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Selecting event.
        /// </devdoc>
        protected virtual void OnSelecting(SqlDataSourceSelectingEventArgs e) {
            SqlDataSourceSelectingEventHandler handler = Events[EventSelecting] as SqlDataSourceSelectingEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }
        

        /// <devdoc>
        /// Raises the Updated event.
        /// </devdoc>
        protected virtual void OnUpdated(SqlDataSourceStatusEventArgs e) {
            SqlDataSourceStatusEventHandler handler = Events[EventUpdated] as SqlDataSourceStatusEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the Updating event.
        /// </devdoc>
        protected virtual void OnUpdating(SqlDataSourceCommandEventArgs e) {
            SqlDataSourceCommandEventHandler handler = Events[EventUpdating] as SqlDataSourceCommandEventHandler;
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Executes the SelectCountCommand to retrieve the total row count.
        /// </devdoc>
        /*protected virtual int QueryTotalRowCount(DbConnection connection, DataSourceSelectArguments arguments) {
            int totalRowCount = 0;
            bool eventRaised = false;

            if (SelectCountCommand.Length > 0) {
                // Create command and add parameters
                DbCommand command = _owner.CreateCommand(SelectCountCommand, connection);
                InitializeParameters(command, SelectParameters);
                command.CommandType = GetCommandType(SelectCountCommand, SelectCompareString);

                // Raise event to allow customization and cancellation
                SqlDataSourceSelectingEventArgs selectCountingEventArgs = new SqlDataSourceSelectingEventArgs(command, arguments, true);
                OnSelecting(selectCountingEventArgs);

                // If the operation was cancelled, exit immediately
                if (selectCountingEventArgs.Cancel) {
                    return totalRowCount;
                }

                // the arguments may have been changed
                arguments.RaiseUnsupportedCapabilitiesError(this);

                // 






































*/


        protected internal override void RaiseUnsupportedCapabilityError(DataSourceCapabilities capability) {
            if (!CanPage && ((capability & DataSourceCapabilities.Page) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_NoPaging, _owner.ID));
            }

            if (!CanSort && ((capability & DataSourceCapabilities.Sort) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_NoSorting, _owner.ID));
            }

            if (!CanRetrieveTotalRowCount && ((capability & DataSourceCapabilities.RetrieveTotalRowCount) != 0)) {
                throw new NotSupportedException(SR.GetString(SR.SqlDataSourceView_NoRowCount, _owner.ID));
            }
            base.RaiseUnsupportedCapabilityError(capability);
        }

        /// <devdoc>
        /// Replace null values in parameters with DBNull.Value.
        /// </devdoc>
        private void ReplaceNullValues(DbCommand command) {
            int paramCount = command.Parameters.Count;
            foreach (DbParameter parameter in command.Parameters) {
                if (parameter.Value == null) {
                    parameter.Value = DBNull.Value;
                }
            }
        }

        /// <devdoc>
        /// Saves view state.
        /// </devdoc>
        protected virtual object SaveViewState() {
            Pair myState = new Pair();

            myState.First = (_selectParameters != null) ? ((IStateManager)_selectParameters).SaveViewState() : null;
            myState.Second = (_filterParameters != null) ? ((IStateManager)_filterParameters).SaveViewState() : null;

            if ((myState.First == null) &&
                (myState.Second == null)) {
                return null;
            }

            return myState;
        }

        public IEnumerable Select(DataSourceSelectArguments arguments) {
            return ExecuteSelect(arguments);
        }

        /// <devdoc>
        /// Event handler for SelectParametersChanged event.
        /// </devdoc>
        private void SelectParametersChangedEventHandler(object o, EventArgs e) {
            OnDataSourceViewChanged(EventArgs.Empty);
        }


        /// <devdoc>
        /// Starts tracking view state.
        /// </devdoc>
        protected virtual void TrackViewState() {
            _tracking = true;

            if (_selectParameters != null) {
                ((IStateManager)_selectParameters).TrackViewState();
            }
            if (_filterParameters != null) {
                ((IStateManager)_filterParameters).TrackViewState();
            }
        }

        public int Update(IDictionary keys, IDictionary values, IDictionary oldValues) {
            return ExecuteUpdate(keys, values, oldValues);
        }


        #region IStateManager implementation
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }

        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion
    }
}

