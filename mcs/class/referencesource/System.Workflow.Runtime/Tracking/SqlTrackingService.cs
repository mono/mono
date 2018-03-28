using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using System.Timers;
using System.Diagnostics;
using System.Reflection;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime.Hosting;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using System.Globalization;
using System.Workflow.ComponentModel.Serialization;
using System.ComponentModel.Design.Serialization;
using System.Xml;
using System.Configuration;


namespace System.Workflow.Runtime.Tracking
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class SqlTrackingService : TrackingService, IProfileNotification
    {
        #region Private/Protected Members

        private bool _isTrans = true;
        private bool _partition = false;
        private bool _defaultProfile = true;
        private bool _enableRetries = false;
        private bool _ignoreCommonEnableRetries = false;

        private DateTime _lastProfileCheck;
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private double _interval = 60000;
        //private static int _deadlock = 1205;
        private TypeKeyedCollection _types = new TypeKeyedCollection();
        private object _typeCacheLock = new object();
        private WorkflowCommitWorkBatchService _transactionService;
        private DbResourceAllocator _dbResourceAllocator;

        private static Version UnknownProfileVersionId = new Version(0, 0);

        // Saved from constructor input to be used in service start initialization        
        private NameValueCollection _parameters;
        string _unvalidatedConnectionString;

        private delegate void ExecuteRetriedDelegate(object param);

        #endregion

        #region Configuration Properties

        public string ConnectionString
        {
            get { return _unvalidatedConnectionString; }
        }
        /// <summary>
        /// Determines if tracking data should be held and transactionally written to the database at persistence points.
        /// </summary>
        /// <value></value>
        public bool IsTransactional
        {
            get { return _isTrans; }
            set
            {
                _isTrans = value;
            }
        }
        /// <summary>
        /// Indicates that records should be moved from the active instance tables to the appropriate parition tables when the instance completes.
        /// </summary>
        public bool PartitionOnCompletion
        {
            get { return _partition; }
            set { _partition = value; }
        }
        /// <summary>
        /// Determines if the default profile should be used for workflow types that do not have a profile specified for them.
        /// </summary>
        /// <value></value>
        public bool UseDefaultProfile
        {
            get { return _defaultProfile; }
            set { _defaultProfile = value; }
        }

        /// <summary>
        /// The time interval, in milliseconds, at which to check the database for changes to profiles.  
        /// Default is 60000.
        /// </summary>
        /// <remarks>
        /// Setting the interval results in the next check to occur the specified number of millisecond 
        /// from the time at which the property is set.
        /// </remarks>
        public double ProfileChangeCheckInterval
        {
            get { return _interval; }
            set
            {
                if (value <= 0)
                    throw new ArgumentException(ExecutionStringManager.InvalidProfileCheckValue);
                _interval = value;
                //
                // Set the timer's interval.
                // This will reset the timer
                _timer.Interval = _interval;
            }
        }

        public bool EnableRetries
        {
            get { return _enableRetries; }
            set
            {
                _enableRetries = value;
                _ignoreCommonEnableRetries = true;
            }
        }

        internal DbResourceAllocator DbResourceAllocator
        {
            get { return this._dbResourceAllocator; }
        }

        #endregion

        #region Construction

        public SqlTrackingService(string connectionString)
        {
            if (String.IsNullOrEmpty(connectionString))
                throw new ArgumentNullException("connectionString", ExecutionStringManager.MissingConnectionString);

            _unvalidatedConnectionString = connectionString;
        }

        public SqlTrackingService(NameValueCollection parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters", ExecutionStringManager.MissingParameters);

            if (parameters.Count > 0)
            {
                foreach (string key in parameters.Keys)
                {
                    if (0 == string.Compare("IsTransactional", key, StringComparison.OrdinalIgnoreCase))
                        _isTrans = bool.Parse(parameters[key]);
                    else if (0 == string.Compare("UseDefaultProfile", key, StringComparison.OrdinalIgnoreCase))
                        _defaultProfile = bool.Parse(parameters[key]);
                    else if (0 == string.Compare("PartitionOnCompletion", key, StringComparison.OrdinalIgnoreCase))
                        _partition = bool.Parse(parameters[key]);
                    else if (0 == string.Compare("ProfileChangeCheckInterval", key, StringComparison.OrdinalIgnoreCase))
                    {
                        _interval = double.Parse(parameters[key], NumberFormatInfo.InvariantInfo);
                        if (_interval <= 0)
                            throw new ArgumentException(ExecutionStringManager.InvalidProfileCheckValue);
                    }
                    else if (0 == string.Compare("ConnectionString", key, StringComparison.OrdinalIgnoreCase))
                        _unvalidatedConnectionString = parameters[key];
                    else if (0 == string.Compare("EnableRetries", key, StringComparison.OrdinalIgnoreCase))
                    {
                        _enableRetries = bool.Parse(parameters[key]);
                        _ignoreCommonEnableRetries = true;
                    }
                }
            }

            _parameters = parameters;
        }

        #endregion

        #region WorkflowRuntimeService

        override protected internal void Start()
        {
            _lastProfileCheck = DateTime.UtcNow;

            _dbResourceAllocator = new DbResourceAllocator(this.Runtime, _parameters, _unvalidatedConnectionString);

            // Check connection string mismatch if using SharedConnectionWorkflowTransactionService
            _transactionService = this.Runtime.GetService<WorkflowCommitWorkBatchService>();
            _dbResourceAllocator.DetectSharedConnectionConflict(_transactionService);

            //
            // If we didn't find a local value for enable retries
            // check in the common section
            if ((!_ignoreCommonEnableRetries) && (null != base.Runtime))
            {
                NameValueConfigurationCollection commonConfigurationParameters = base.Runtime.CommonParameters;
                if (commonConfigurationParameters != null)
                {
                    // Then scan for connection string in the common configuration parameters section
                    foreach (string key in commonConfigurationParameters.AllKeys)
                    {
                        if (string.Compare("EnableRetries", key, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _enableRetries = bool.Parse(commonConfigurationParameters[key].Value);
                            break;
                        }
                    }
                }
            }

            _timer.Interval = _interval;
            _timer.AutoReset = false; // ensure that only one timer thread is checking for profile changes at a time
            _timer.Elapsed += new ElapsedEventHandler(CheckProfileChanges);
            _timer.Start();

            base.Start();
        }

        #endregion WorkflowRuntimeService

        #region IProfileNotification Implementation

        protected internal override TrackingChannel GetTrackingChannel(TrackingParameters parameters)
        {
            if (null == parameters)
                throw new ArgumentNullException("parameters");

            //
            // Return a new channel for this instance
            // Give it the parameters and this to store
            return new SqlTrackingChannel(parameters, this);
        }

        public event EventHandler<ProfileUpdatedEventArgs> ProfileUpdated;

        public event EventHandler<ProfileRemovedEventArgs> ProfileRemoved;

        protected internal override TrackingProfile GetProfile(Type workflowType, Version profileVersion)
        {
            if (null == workflowType)
                throw new ArgumentNullException("workflowType");

            // parameter wantToCreateDefault = false:
            // looking for a specific version that has already been running with this instance; don't use a default here
            return GetProfileByScheduleType(workflowType, profileVersion, false);
        }

        protected internal override bool TryGetProfile(Type workflowType, out TrackingProfile profile)
        {
            if (null == workflowType)
                throw new ArgumentNullException("workflowType");

            profile = GetProfileByScheduleType(workflowType, SqlTrackingService.UnknownProfileVersionId, _defaultProfile);

            if (null == profile)
                return false;
            else
                return true;
        }

        protected internal override TrackingProfile GetProfile(Guid scheduleInstanceId)
        {
            TrackingProfile profile = null;
            GetProfile(scheduleInstanceId, out profile);
            return profile;
        }

        private bool GetProfile(Guid scheduleInstanceId, out TrackingProfile profile)
        {
            profile = null;

            DbCommand cmd = this._dbResourceAllocator.NewCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "[dbo].[GetInstanceTrackingProfile]";
            cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@InstanceId", scheduleInstanceId));

            DbDataReader reader = null;
            try
            {
                reader = ExecuteReaderRetried(cmd, CommandBehavior.CloseConnection);
                //
                // Should only reach here in non exception state
                if (!reader.HasRows)
                {
                    //
                    // Didn't find a specific profile for this instance
                    reader.Close();
                    profile = null;
                    return false;
                }
                else
                {
                    if (!reader.Read())
                    {
                        reader.Close();
                        profile = null;
                        return false;
                    }

                    if (reader.IsDBNull(0))
                        profile = null;
                    else
                    {
                        string tmp = reader.GetString(0);
                        TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                        StringReader pReader = null;

                        try
                        {
                            pReader = new StringReader(tmp);
                            profile = serializer.Deserialize(pReader);
                        }
                        finally
                        {
                            if (null != pReader)
                                pReader.Close();
                        }
                    }
                    return true;
                }
            }
            finally
            {
                if ((null != reader) && (!reader.IsClosed))
                    reader.Close();

                if ((null != cmd) && (null != cmd.Connection) && (ConnectionState.Closed != cmd.Connection.State))
                    cmd.Connection.Close();
            }
        }

        protected internal override bool TryReloadProfile(Type workflowType, Guid scheduleInstanceId, out TrackingProfile profile)
        {
            if (null == workflowType)
                throw new ArgumentNullException("workflowType");

            bool found = GetProfile(scheduleInstanceId, out profile);

            if (found)
                return true;
            else
            {
                profile = null;
                return false;
            }
        }

        #endregion

        #region Profile Management Methods

        private void CheckProfileChanges(object sender, ElapsedEventArgs e)
        {
            DbCommand cmd = null;
            DbDataReader reader = null;
            try
            {
                if ((null == ProfileUpdated) && (null == ProfileRemoved))
                    return; // no one to notify

                Debug.WriteLine("Checking for updated profiles...");

                cmd = this._dbResourceAllocator.NewCommand();
                cmd.CommandText = "GetUpdatedTrackingProfiles";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@LastCheckDateTime", _lastProfileCheck));

                DbParameter param = this._dbResourceAllocator.NewDbParameter();
                param.ParameterName = "@MaxCheckDateTime";
                param.DbType = DbType.DateTime;
                param.Direction = System.Data.ParameterDirection.Output;

                cmd.Parameters.Add(param);

                reader = ExecuteReaderRetried(cmd, CommandBehavior.CloseConnection);
                //
                // No changes
                if (!reader.HasRows)
                    return;

                while (reader.Read())
                {
                    Type t = null;
                    string tmp = null;
                    TrackingProfile profile = null;

                    t = Assembly.Load(reader[1] as string).GetType(reader[0] as string);

                    if (null == t)
                        continue;

                    tmp = reader[2] as string;

                    if (null == tmp)
                    {
                        if (null != ProfileRemoved)
                            ProfileRemoved(this, new ProfileRemovedEventArgs(t));
                    }
                    else
                    {
                        TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                        StringReader pReader = null;

                        try
                        {
                            pReader = new StringReader(tmp);
                            profile = serializer.Deserialize(pReader);
                        }
                        finally
                        {
                            if (null != pReader)
                                pReader.Close();
                        }

                        if (null != ProfileUpdated)
                            ProfileUpdated(this, new ProfileUpdatedEventArgs(t, profile));
                    }
                    Debug.WriteLine(ExecutionStringManager.UpdatedProfile + t.FullName);
                }
            }
            finally
            {
                if ((null != reader) && (!reader.IsClosed))
                    reader.Close();

                //
                // This should never be null/empty unless the proc failed which should throw
                if (null != cmd)
                {
                    //
                    // If the value is null we error'd so keep the same last time for the next check
                    if (null != cmd.Parameters[1].Value)
                        _lastProfileCheck = (DateTime)cmd.Parameters[1].Value;
                }

                if ((null != cmd) && (null != cmd.Connection) && (ConnectionState.Closed != cmd.Connection.State))
                    cmd.Connection.Close();
                //
                // Start the timer again (autoreset is false to avoid multiple threads checking for profile changes)
                _timer.Start();
            }
        }

        #endregion

        #region Private Methods

        private void ExecuteRetried(ExecuteRetriedDelegate executeRetried, object param)
        {
            short count = 0;

            DbRetry dbRetry = new DbRetry(_enableRetries);
            while (true)
            {
                try
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    executeRetried(param);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteRetried caught exception: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteRetried " + executeRetried.Method.Name + " retrying.");
                        continue;
                    }
                    throw;
                }
            }
        }

        private DbDataReader ExecuteReaderRetried(DbCommand command, CommandBehavior behavior)
        {
            DbDataReader reader = null;
            short count = 0;
            DbRetry dbRetry = new DbRetry(_enableRetries);
            while (true)
            {
                try
                {
                    ResetConnectionForCommand(command);

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried ExecuteReader start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    reader = command.ExecuteReader(behavior);
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried ExecuteReader end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteReaderRetried caught exception from ExecuteReader: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteReaderRetried retrying.");
                        continue;
                    }
                    throw;
                }
            }

            return reader;
        }

        private void ExecuteNonQueryRetried(DbCommand command)
        {
            short count = 0;
            DbRetry dbRetry = new DbRetry(_enableRetries);
            while (true)
            {
                try
                {
                    ResetConnectionForCommand(command);

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    command.ExecuteNonQuery();
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                    break;
                }
                catch (Exception e)
                {
                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteNonQueryRetried caught exception from ExecuteNonQuery: " + e.ToString());

                    if (dbRetry.TryDoRetry(ref count))
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryRetried retrying.");
                        continue;
                    }
                    throw;
                }
            }
        }

        private void ExecuteNonQueryWithTxRetried(DbCommand command)
        {
            try
            {
                short count = 0;
                DbRetry dbRetry = new DbRetry(_enableRetries);
                while (true)
                {
                    try
                    {
                        ResetConnectionForCommand(command);

                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried ExecuteNonQuery start: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                        command.Transaction = command.Connection.BeginTransaction();
                        command.ExecuteNonQuery();
                        command.Transaction.Commit();
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried ExecuteNonQuery end: " + DateTime.UtcNow.ToString("G", System.Globalization.CultureInfo.InvariantCulture));
                        break;
                    }
                    catch (Exception e)
                    {
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried caught exception from ExecuteNonQuery: " + e.ToString());

                        try
                        {
                            if (null != command.Transaction)
                                command.Transaction.Rollback();
                        }
                        catch
                        {
                            //
                            // Rollback() can throw, nothing to do but ---- if this happens
                            // so that we don't lose the original exception
                        }

                        if (dbRetry.TryDoRetry(ref count))
                        {
                            WorkflowTrace.Host.TraceEvent(TraceEventType.Information, 0, "SqlTrackingService.ExecuteNonQueryWithTxRetried retrying.");
                            continue;
                        }

                        throw;
                    }
                }
            }
            finally
            {
                if ((null != command) && (null != command.Connection) && (ConnectionState.Closed != command.Connection.State))
                    command.Connection.Close();
            }
        }

        private void ResetConnectionForCommand(DbCommand command)
        {
            if (null == command)
                return;

            if (null != command.Connection)
            {
                if (ConnectionState.Open != command.Connection.State)
                {
                    if (ConnectionState.Closed != command.Connection.State)
                        command.Connection.Close();

                    command.Connection.Dispose();

                    command.Connection = _dbResourceAllocator.OpenNewConnectionNoEnlist();
                }
            }
        }

        internal static XmlWriter CreateXmlWriter(TextWriter output)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;
            settings.CloseOutput = true;

            return XmlWriter.Create(output as TextWriter, settings);
        }

        private TrackingProfile GetProfileByScheduleType(Type workflowType, Version profileVersionId, bool wantToCreateDefault)
        {
            DbCommand cmd = this._dbResourceAllocator.NewCommand();
            DbDataReader reader = null;
            TrackingProfile profile = null;

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.GetTrackingProfile";

            cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@TypeFullName", workflowType.FullName));
            cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@AssemblyFullName", workflowType.Assembly.FullName));

            if (profileVersionId != SqlTrackingService.UnknownProfileVersionId)
                cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@Version", profileVersionId.ToString()));

            cmd.Parameters.Add(this._dbResourceAllocator.NewDbParameter("@CreateDefault", wantToCreateDefault));
            try
            {
                reader = ExecuteReaderRetried(cmd, CommandBehavior.CloseConnection);

                if (reader.Read())
                {
                    string tmp = reader[0] as string;

                    if (null != tmp)
                    {
                        TrackingProfileSerializer serializer = new TrackingProfileSerializer();
                        StringReader pReader = null;

                        try
                        {
                            pReader = new StringReader(tmp);
                            profile = serializer.Deserialize(pReader);
                        }
                        finally
                        {
                            if (null != pReader)
                                pReader.Close();
                        }
                    }
                }
            }
            finally
            {
                if ((null != reader) && (!reader.IsClosed))
                    reader.Close();

                if ((null != cmd) && (null != cmd.Connection) && (ConnectionState.Closed != cmd.Connection.State))
                    cmd.Connection.Close();
            }

            return profile;

        }


        #endregion

        #region Private Classes

        private class TypeKeyedCollection : KeyedCollection<string, Type>
        {
            protected override string GetKeyForItem(Type item)
            {
                return item.AssemblyQualifiedName;
            }
        }

        private class SerializedDataItem : TrackingDataItem
        {
            public Type Type;
            public string StringData;
            public byte[] SerializedData;
            public bool NonSerializable;
        }

        private class SerializedEventArgs : EventArgs
        {
            public Type Type;
            public byte[] SerializedArgs;
        }

        private struct AddedActivity
        {
            public string ActivityTypeFullName;
            public string ActivityTypeAssemblyFullName;
            public string QualifiedName;
            public string ParentQualifiedName;
            public string AddedActivityActionXoml;
            public int Order;
        }

        private struct RemovedActivity
        {
            public string QualifiedName;
            public string ParentQualifiedName;
            public string RemovedActivityActionXoml;
            public int Order;
        }

        private class SerializedWorkflowChangedEventArgs : SerializedEventArgs
        {
            public IList<AddedActivity> AddedActivities = new List<AddedActivity>();
            public IList<RemovedActivity> RemovedActivities = new List<RemovedActivity>();
        }

        #endregion Private Classes

        internal class SqlTrackingChannel : TrackingChannel, IPendingWork
        {
            #region Private Members

            private SqlTrackingService _service = null;
            private string _callPathKey = null, _parentCallPathKey = null;
            private bool _isTrans = false;
            private long _internalId = -1;
            private long _tmpInternalId = -1;
            private Dictionary<string, long> _activityInstanceId = new Dictionary<string, long>(32);
            private Dictionary<string, long> _tmpActivityInstanceId = new Dictionary<string, long>(10);
            private TrackingParameters _parameters = null;
            private bool _pendingArchive = false;
            private bool _completedTerminated = false;

            private static int _activityEventBatchSize = 5;
            private static int _dataItemBatchSize = 5;
            private static int _dataItemAnnotationBatchSize = 5;
            private static int _eventAnnotationBatchSize = 5;


            #endregion

            #region Construction
            protected SqlTrackingChannel()
            {
            }

            public SqlTrackingChannel(TrackingParameters parameters, SqlTrackingService service)
            {
                if (null == service)
                    return;

                _service = service;
                _parameters = parameters;
                _isTrans = service.IsTransactional;

                GetCallPathKeys(parameters.CallPath);
                if (!_isTrans)
                {
                    //
                    // Look up instance id or insert if new instance
                    // If we're transactional we'll do this in the first IPendingWork.Commit()
                    _service.ExecuteRetried(ExecuteInsertWorkflowInstance, null);
                }
            }

            #endregion

            #region Public Properties

            private DbResourceAllocator DbResourceAllocator
            {
                get { return _service.DbResourceAllocator; }
            }

            private WorkflowCommitWorkBatchService WorkflowCommitWorkBatchService
            {
                get { return _service._transactionService; }
            }
            #endregion

            #region TrackingChannel

            protected internal override void InstanceCompletedOrTerminated()
            {
                if (_isTrans)
                {
                    //
                    // Indicate that at the next batch commit we should stamp the enddate
                    _completedTerminated = true;
                    //
                    // Indicate that when the next batch commit completes successfully we should partition this instance
                    if (_service.PartitionOnCompletion)
                        _pendingArchive = true;
                }
                else
                {
                    _service.ExecuteRetried(ExecuteSetEndDate, null);

                    if (_service.PartitionOnCompletion)
                        _service.ExecuteRetried(PartitionInstance, null);
                }
            }

            private void PartitionInstance(object param)
            {
                DbCommand command = null;
                try
                {
                    //
                    // Allow enlisting if there is an ambient tx
                    // This can only happen on a host initiated terminate in V1.
                    DbConnection connection = DbResourceAllocator.OpenNewConnection(false);
                    command = DbResourceAllocator.NewCommand(connection);
                    command.CommandText = "[dbo].[PartitionWorkflowInstance]";
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", _internalId));

                    command.ExecuteNonQuery();
                }
                finally
                {
                    if ((null != command) && (null != command.Connection) && (ConnectionState.Closed != command.Connection.State))
                        command.Connection.Close();
                }
            }

            private void ExecuteSetEndDate(object param)
            {
                DbCommand command = null;
                try
                {
                    //
                    // Allow enlisting if there is an ambient tx
                    // This can only happen on a host initiated terminate in V1.
                    DbConnection connection = DbResourceAllocator.OpenNewConnection(false);
                    command = DbResourceAllocator.NewCommand(connection);
                    ExecuteSetEndDate(_internalId, command);
                }
                finally
                {
                    if ((null != command) && (null != command.Connection) && (ConnectionState.Closed != command.Connection.State))
                        command.Connection.Close();
                }
            }

            private void ExecuteSetEndDate(long internalId, DbCommand command)
            {
                if (null == command)
                    throw new ArgumentNullException("command");

                command.Parameters.Clear();
                command.CommandText = "[dbo].[SetWorkflowInstanceEndDateTime]";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EndDateTime", DateTime.UtcNow));
                command.ExecuteNonQuery();
            }

            protected internal override void Send(TrackingRecord record)
            {
                if ((Guid.Empty == _parameters.InstanceId) || (null == record))
                    throw new ArgumentException(ExecutionStringManager.MissingParametersTrack);

                if (record is ActivityTrackingRecord)
                {
                    ActivityTrackingRecord act = record as ActivityTrackingRecord;
                    if (_isTrans)
                        WorkflowEnvironment.WorkBatch.Add(this, SerializeRecord(act));
                    else
                        _service.ExecuteRetried(ExecuteInsertActivityStatusInstance, SerializeRecord(act));
                }
                else if (record is WorkflowTrackingRecord)
                {
                    //
                    // Instance events cannot be batched - many occur when there isn't a batch
                    WorkflowTrackingRecord inst = (WorkflowTrackingRecord)record;
                    if (_isTrans)
                    {
                        WorkflowEnvironment.WorkBatch.Add(this, SerializeRecord(inst));
                    }
                    else
                    {
                        if (TrackingWorkflowEvent.Changed == inst.TrackingWorkflowEvent)
                        {
                            //
                            // Dynamic updates are inserted in the WorkflowInstanceEvent table
                            // and then the arg (workflowchanges) is normalized into xoml 
                            // and the added/removed activities tables
                            _service.ExecuteRetried(ExecuteInsertWorkflowChange, SerializeRecord(inst));
                        }
                        else
                        {
                            _service.ExecuteRetried(ExecuteInsertWorkflowInstanceEvent, SerializeRecord(inst));
                        }
                    }
                }
                else if (record is UserTrackingRecord)
                {
                    UserTrackingRecord user = (UserTrackingRecord)record;
                    if (_isTrans)
                        WorkflowEnvironment.WorkBatch.Add(this, SerializeRecord(user));
                    else
                        _service.ExecuteRetried(ExecuteInsertUserEvent, SerializeRecord(user));
                }
            }

            #endregion

            #region IPendingWork Members

            public bool MustCommit(ICollection items)
            {
                //
                // Never force a persist - this is a balancing act but the V1
                // decision is to err on the side of persisting only when the workflow
                // requires it based on its model.  If the workflow uses persistence points
                // wisely this is great.  If it goes a long time between persists with lots
                // of events the persists will take a long time as the batch can be huge.
                return false;
            }

            public void Commit(System.Transactions.Transaction transaction, ICollection items)
            {
                if ((null == items) || (0 == items.Count))
                    return;

                DbCommand command = null;
                DbConnection connection = null;
                bool needToCloseConnection = false;
                DbTransaction localTransaction = null;
                bool commitTx = false;

                try
                {
                    //
                    // Get the connection and transaction
                    // The connection might be shared or local
                    // The tx is shared and may be either a DTC or a local sql tx
                    connection = DbResourceAllocator.GetEnlistedConnection(
                        this.WorkflowCommitWorkBatchService, transaction, out needToCloseConnection);
                    localTransaction = DbResourceAllocator.GetLocalTransaction(
                        this.WorkflowCommitWorkBatchService, transaction);

                    if (null == localTransaction)
                    {
                        localTransaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                        commitTx = true;
                    }

                    command = DbResourceAllocator.NewCommand(connection);
                    command.Transaction = localTransaction;
                    //
                    // If we don't have the internal id for the instance this is the first batch
                    // for this channel instance.  If this is a new instance the following will insert
                    // a new instance record in the db and set _tmpInternalId.  If this is a reload of
                    // an existing instance it will just do a lookup and set _tmpInternalId
                    // In Completed we will assign _tmpInternalId to _internalId if the batch is successful.
                    long internalId = -1;
                    if (_internalId <= 0)
                    {
                        ExecuteInsertWorkflowInstance(command);
                        internalId = _tmpInternalId;
                    }
                    else
                        internalId = _internalId;

                    IList<ActivityTrackingRecord> activities = new List<ActivityTrackingRecord>(5);
                    WorkflowTrackingRecord workflow = null;
                    //
                    // Build the batch statement
                    foreach (object o in items)
                    {
                        if (!(o is TrackingRecord))
                            continue;

                        if (o is ActivityTrackingRecord)
                        {
                            //
                            // If we have a cached workflow tracking record send it
                            if (null != workflow)
                            {
                                ExecuteInsertWorkflowInstanceEvent(internalId, workflow, null, command);
                                workflow = null;
                            }

                            ActivityTrackingRecord activity = (ActivityTrackingRecord)o;
                            //
                            // Add this event to the list and send to the db if we've hit our limit
                            activities.Add(activity);

                            if (_activityEventBatchSize == activities.Count)
                            {
                                ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                activities = new List<ActivityTrackingRecord>(5);
                            }
                        }
                        else if (o is UserTrackingRecord)
                        {
                            //
                            // If we have cached activity or workflow tracking records send them
                            if (activities.Count > 0)
                            {
                                ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                activities.Clear();
                            }

                            if (null != workflow)
                            {
                                ExecuteInsertWorkflowInstanceEvent(internalId, workflow, null, command);
                                workflow = null;
                            }

                            ExecuteInsertUserEvent(internalId, (UserTrackingRecord)o, command);
                        }
                        else if (o is WorkflowTrackingRecord)
                        {
                            //
                            // If we have cached activity tracking records send them
                            if (activities.Count > 0)
                            {
                                ExecuteInsertActivityStatusInstance(internalId, activities, command);
                                activities.Clear();
                            }

                            WorkflowTrackingRecord record = (WorkflowTrackingRecord)o;

                            if (TrackingWorkflowEvent.Changed == record.TrackingWorkflowEvent)
                            {
                                //
                                // If we're already holding a workflow tracking record send both to the db
                                // else cache it and wait for the next workflow tracking record
                                if (null != workflow)
                                {
                                    ExecuteInsertWorkflowInstanceEvent(internalId, workflow, null, command);
                                    workflow = null;
                                }
                                ExecuteInsertWorkflowChange(internalId, record, command);
                            }
                            else
                            {
                                //
                                // If we're already holding a workflow tracking record send both to the db
                                // else cache it and wait for the next workflow tracking record
                                if (null != workflow)
                                {
                                    ExecuteInsertWorkflowInstanceEvent(internalId, workflow, record, command);
                                    workflow = null;
                                }
                                else
                                {
                                    workflow = record;
                                }
                            }
                        }
                    }

                    //
                    // If we ended up with any activities event send them.
                    if (activities.Count > 0)
                        ExecuteInsertActivityStatusInstance(internalId, activities, command);

                    if (null != workflow)
                    {
                        ExecuteInsertWorkflowInstanceEvent(internalId, workflow, null, command);
                        workflow = null;
                    }

                    if (_completedTerminated)
                        ExecuteSetEndDate(internalId, command);

                    if (commitTx)
                        localTransaction.Commit();
                }
                catch (DbException e)
                {
                    if (commitTx)
                        localTransaction.Rollback();

                    WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, "Error writing tracking data to database: " + e);
                    throw;
                }
                finally
                {
                    if (needToCloseConnection)
                    {
                        connection.Dispose();
                    }
                }

                return;
            }

            public void Complete(bool succeeded, ICollection items)
            {
                //
                // If we didn't succeed on commit reset all flags
                if (!succeeded)
                {
                    _completedTerminated = false;
                    _pendingArchive = false;
                    _tmpInternalId = -1;
                    _tmpActivityInstanceId.Clear();
                    return;
                }
                //
                // Commit succeeded - move the tmp internalId to the real internalId member
                if (-1 == _internalId && _tmpInternalId > 0)
                    _internalId = _tmpInternalId;

                //
                // Move the tmp activity instance ids to the real activity instance id member
                if (null != _tmpActivityInstanceId && _tmpActivityInstanceId.Count > 0)
                {
                    foreach (string key in _tmpActivityInstanceId.Keys)
                    {
                        if (!_activityInstanceId.ContainsKey(key))
                            _activityInstanceId.Add(key, _tmpActivityInstanceId[key]);
                    }
                    _tmpActivityInstanceId.Clear();
                }

                if (_pendingArchive)
                {
                    try
                    {
                        _service.ExecuteRetried(PartitionInstance, null);
                    }
                    catch (Exception e)
                    {
                        //
                        // ---- exceptions here, do not fail the instance.
                        // Partition logic can be re-run to clean up on failure
                        WorkflowTrace.Host.TraceEvent(TraceEventType.Error, 0, string.Format(System.Globalization.CultureInfo.InvariantCulture, "Error partitioning instance {0}: {1}", _parameters.InstanceId, e.ToString()));
                    }
                }
            }

            #endregion

            #region Sql Commands - InsertWorkflowInstance

            private void ExecuteInsertWorkflowInstance(object param)
            {

                DbConnection conn = DbResourceAllocator.OpenNewConnection();
                DbCommand command = DbResourceAllocator.NewCommand(conn);
                DbTransaction tx = null;

                try
                {
                    tx = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    command.Connection = conn;
                    command.Transaction = tx;

                    _internalId = ExecuteInsertWorkflowInstance(command);

                    tx.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (null != tx)
                            tx.Rollback();
                    }
                    catch (Exception)
                    {
                        //
                        // Rollback can throw - ignore these exceptions
                        // so we don't lose the original exception
                    }
                    //
                    // Re-throw original exception
                    throw;
                }
                finally
                {
                    if ((null != conn) && (ConnectionState.Closed != conn.State))
                        conn.Close();
                }

                return;
            }

            private long ExecuteInsertWorkflowInstance(DbCommand command)
            {
                if (null == command)
                    throw new ArgumentNullException("command");

                if ((null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentException(ExecutionStringManager.InvalidCommandBadConnection, "command");
                //
                // Write the type and the workflow definition
                string xaml = _parameters.RootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                if (null != xaml && xaml.Length > 0)
                    InsertWorkflow(command, _parameters.InstanceId, null, _parameters.RootActivity);
                else
                    InsertWorkflow(command, _parameters.InstanceId, _parameters.WorkflowType, _parameters.RootActivity);
                //
                // Write the instance record
                BuildInsertWorkflowInstanceParameters(command);

                DbDataReader reader = null;
                try
                {
                    reader = command.ExecuteReader();

                    if (reader.Read())
                        _tmpInternalId = reader.GetInt64(0);

                    return _tmpInternalId;
                }
                finally
                {
                    if (null != reader)
                        reader.Close();
                }
            }

            private void BuildInsertWorkflowInstanceParameters(DbCommand command)
            {
                Debug.Assert((command != null), "Null command");
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflowInstance]";

                command.Parameters.Clear();

                bool xamlInst = false;
                string xaml = _parameters.RootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
                if (null != xaml && xaml.Length > 0)
                    xamlInst = true;

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceId", _parameters.InstanceId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TypeFullName", (xamlInst ? _parameters.InstanceId.ToString() : _parameters.WorkflowType.FullName)));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@AssemblyFullName", (xamlInst ? _parameters.InstanceId.ToString() : _parameters.WorkflowType.Assembly.FullName)));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ContextGuid", _parameters.ContextGuid));
                if (Guid.Empty != _parameters.CallerInstanceId)
                {
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@CallerInstanceId", _parameters.CallerInstanceId));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@CallPath", _callPathKey));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@CallerContextGuid", _parameters.CallerContextGuid));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@CallerParentContextGuid", _parameters.CallerParentContextGuid));
                }
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventDateTime", this.GetSqlDateTimeString(DateTime.UtcNow)));
            }

            private void InsertWorkflow(DbCommand command, Guid workflowInstanceId, Type workflowType, Activity rootActivity)
            {
                string xoml = null;

                //
                // If we've already seen this type just return
                if (null != workflowType)
                {
                    lock (_service._typeCacheLock)
                    {
                        if (_service._types.Contains(workflowType.AssemblyQualifiedName))
                            return;
                        else
                            xoml = GetXomlDocument(rootActivity);
                    }
                }
                else
                {
                    // Don't forget to deal with XOML-only workflows
                    lock (_service._typeCacheLock)
                    {
                        xoml = GetXomlDocument(rootActivity);
                    }
                }
                //
                // It is possible to ---- here but the pk specifies ignore duplicate key
                // This is better than taking a lock around all of the logic in this method.
                command.Parameters.Clear();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflow]";

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TypeFullName", (null == workflowType ? workflowInstanceId.ToString() : workflowType.FullName)));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@AssemblyFullName", (null == workflowType ? workflowInstanceId.ToString() : workflowType.Assembly.FullName)));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@IsInstanceType", (null == workflowType ? true : false)));


                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowDefinition", xoml));

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowId", DbType.Int32, System.Data.ParameterDirection.Output));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Exists", DbType.Boolean, System.Data.ParameterDirection.Output));

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Activities", GetActivitiesXml((CompositeActivity)rootActivity)));

                command.ExecuteNonQuery();
                //
                // Add this to the list of types we've already seen so we don't go
                // through the serialization overhead again and hit the db only to learn we've already stored it
                // Use a lock here to avoid ---- on _types dictionary
                if (null != workflowType)
                {
                    lock (_service._typeCacheLock)
                    {
                        if (!_service._types.Contains(workflowType.AssemblyQualifiedName))
                        {
                            _service._types.Add(workflowType);
                        }
                    }
                }

                return;
            }

            #endregion

            #region Sql Commands - InsertWorkflowInstanceEvent

            private void ExecuteInsertWorkflowInstanceEvent(object param)
            {
                WorkflowTrackingRecord record = param as WorkflowTrackingRecord;

                if (null == record)
                    throw new ArgumentException(ExecutionStringManager.InvalidWorkflowTrackingRecordParameter, "param");

                DbConnection conn = DbResourceAllocator.OpenNewConnection();
                DbCommand command = DbResourceAllocator.NewCommand(conn);
                DbTransaction tx = null;

                try
                {
                    tx = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    command.Connection = conn;
                    command.Transaction = tx;

                    ExecuteInsertWorkflowInstanceEvent(_internalId, record, null, command);

                    tx.Commit();
                }
                catch (Exception)
                {
                    try
                    {
                        if (null != tx)
                            tx.Rollback();
                    }
                    catch (Exception)
                    {
                        //
                        // Rollback can throw - ignore these exceptions
                        // so we don't lose the original exception
                    }
                    //
                    // Re-throw original exception
                    throw;
                }
                finally
                {
                    if ((null != conn) && (ConnectionState.Closed != conn.State))
                        conn.Close();
                }

                return;
            }

            private void ExecuteInsertWorkflowInstanceEvent(long internalId, WorkflowTrackingRecord record1, WorkflowTrackingRecord record2, DbCommand command)
            {
                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentException();

                BuildInsertWorkflowInstanceEventParameters(internalId, record1, record2, command);

                command.ExecuteNonQuery();

                long eventId1 = (long)command.Parameters["@WorkflowInstanceEventId1"].Value;
                Debug.Assert(eventId1 > 0, "Invalid eventId1");

                long eventId2 = -1;
                if (null != record2)
                {
                    eventId2 = (long)command.Parameters["@WorkflowInstanceEventId2"].Value;
                    Debug.Assert(eventId2 > 0, "Invalid eventId2");
                }

                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(record1.Annotations.Count + (null == record2 ? 0 : record2.Annotations.Count));

                foreach (string s in record1.Annotations)
                    annotations.Add(new KeyValuePair<long, string>(eventId1, s));

                if (null != record2)
                {
                    foreach (string s in record2.Annotations)
                        annotations.Add(new KeyValuePair<long, string>(eventId2, s));
                }

                BatchExecuteInsertEventAnnotation(internalId, 'w', annotations, command);
            }

            private void BuildInsertWorkflowInstanceEventParameters(long internalId, WorkflowTrackingRecord record1, WorkflowTrackingRecord record2, DbCommand command)
            {
                if (null == record1)
                    throw new ArgumentNullException("record");

                if (null == command)
                    throw new ArgumentNullException("command");

                Debug.Assert(internalId != -1, "Invalid internalId");

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertWorkflowInstanceEvent]";

                command.Parameters.Clear();
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TrackingWorkflowEventId1", (int)record1.TrackingWorkflowEvent));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventDateTime1", record1.EventDateTime));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventOrder1", record1.EventOrder));

                if (null != record1.EventArgs)
                {
                    Type t = record1.EventArgs.GetType();
                    Byte[] data = null;

                    if (!(record1.EventArgs is SerializedEventArgs))
                        record1 = SerializeRecord(record1);

                    SerializedEventArgs sargs = record1.EventArgs as SerializedEventArgs;
                    data = sargs.SerializedArgs;
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArgTypeFullName1", t.FullName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArgAssemblyFullName1", t.Assembly.FullName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArg1", data));
                }
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId1", DbType.Int64, ParameterDirection.Output));

                if (null != record2)
                {
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TrackingWorkflowEventId2", (int)record2.TrackingWorkflowEvent));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventDateTime2", record2.EventDateTime));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventOrder2", record2.EventOrder));

                    if (null != record2.EventArgs)
                    {
                        Type t = record2.EventArgs.GetType();
                        Byte[] data = null;

                        if (!(record2.EventArgs is SerializedEventArgs))
                            record2 = SerializeRecord(record2);

                        SerializedEventArgs sargs = record2.EventArgs as SerializedEventArgs;
                        data = sargs.SerializedArgs;
                        command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArgTypeFullName2", t.FullName));
                        command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArgAssemblyFullName2", t.Assembly.FullName));
                        command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventArg2", data));
                    }
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId2", DbType.Int64, ParameterDirection.Output));
                }
            }

            #endregion

            #region Sql Commands - InsertActivityStatusInstance

            private void ExecuteInsertActivityStatusInstance(object param)
            {
                ActivityTrackingRecord record = param as ActivityTrackingRecord;

                if (null == record)
                    throw new ArgumentException(ExecutionStringManager.InvalidActivityTrackingRecordParameter, "param");

                DbConnection conn = DbResourceAllocator.OpenNewConnection();

                DbTransaction tx = null;

                try
                {
                    tx = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    DbCommand command = conn.CreateCommand();
                    command.Transaction = tx;

                    IList<ActivityTrackingRecord> activity = new List<ActivityTrackingRecord>(1);
                    activity.Add(record);

                    ExecuteInsertActivityStatusInstance(_internalId, activity, command);

                    tx.Commit();
                }
                catch (Exception)
                {
                    //
                    // Rollback can throw - ignore these exceptions
                    // so we don't lose the original exception
                    try
                    {
                        if (null != tx)
                            tx.Rollback();
                    }
                    catch (Exception)
                    {
                    }

                    //
                    // Re-throw original exception
                    throw;
                }
                finally
                {
                    if ((null != conn) && (ConnectionState.Closed != conn.State))
                        conn.Close();

                }

                return;
            }

            private void ExecuteInsertActivityStatusInstance(long internalId, IList<ActivityTrackingRecord> activities, DbCommand command)
            {
                if (null == activities || activities.Count <= 0)
                    return;

                if (activities.Count > _activityEventBatchSize)
                    throw new ArgumentOutOfRangeException("activities");

                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentException();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertActivityExecutionStatusEventMultiple]";
                //
                // Add the common parameters
                command.Parameters.Clear();
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceId", _parameters.InstanceId));
                //
                // If we have the workflow's internal id use it to avoid the look up in the db
                DbParameter param = DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", DbType.Int64, System.Data.ParameterDirection.InputOutput);
                command.Parameters.Add(param);
                if (internalId > 0)
                    param.Value = internalId;

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceContextGuid", _parameters.ContextGuid));
                //
                // Hashed ids of QName, context and pcontext used as key for storing activity record ids
                // Save these for each record in the list so we don't have to recompute them below when adding to the cache
                string[] ids = new string[] { null, null, null, null, null };

                for (int i = 0; i < activities.Count; i++)
                {
                    ActivityTrackingRecord record = activities[i];

                    long aid = -1;
                    ids[i] = BuildQualifiedNameVarName(record.QualifiedName, record.ContextGuid, record.ParentContextGuid);

                    TryGetActivityInstanceId(ids[i], out aid);

                    BuildInsertActivityStatusEventParameters(internalId, aid, i + 1, record, command);
                }

                command.ExecuteNonQuery();
                //
                // Get all the output ids
                long[] eventIds = new long[] { -1, -1, -1, -1, -1 };
                for (int i = 0; i < activities.Count; i++)
                {
                    string index = (i + 1).ToString(CultureInfo.InvariantCulture);
                    //
                    // ActivityInstanceId
                    long aId = (long)command.Parameters["@ActivityInstanceId" + index].Value;
                    Debug.Assert(aId > 0, "Invalid @ActivityInstanceId output parameter value");
                    //
                    // For all status changes that aren't "Closed" add the id to the instance cache
                    // Set... method checks and only adds if it does already exist.
                    // To keep the cache size under control remove entries for activities that have closed.
                    // The activity might fault and need to do a lookup in the db but this isn't the common
                    // path and the db lookup isn't very expensive.
                    if (ActivityExecutionStatus.Closed != activities[i].ExecutionStatus)
                        SetActivityInstanceId(ids[i], aId);
                    else
                        RemoveActivityInstanceId(ids[i]);
                    //
                    // ActivityExecutionStatusEventId
                    long aeseId = (long)command.Parameters["@ActivityExecutionStatusEventId" + index].Value;
                    Debug.Assert(aeseId > 0, "Invalid @ActivityExecutionStatusEventId output parameter value");
                    eventIds[i] = aeseId;
                }

                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(10);
                List<KeyValuePair<long, TrackingDataItem>> items = new List<KeyValuePair<long, TrackingDataItem>>(10);
                for (int i = 0; i < activities.Count; i++)
                {
                    ActivityTrackingRecord record = activities[i];
                    //
                    // Get the ActivityExecutionStatusEventId
                    long eventId = eventIds[i];
                    if (eventId <= 0)
                        throw new InvalidOperationException();

                    foreach (string s in record.Annotations)
                        annotations.Add(new KeyValuePair<long, string>(eventId, s));

                    foreach (TrackingDataItem item in record.Body)
                        items.Add(new KeyValuePair<long, TrackingDataItem>(eventId, item));
                }

                BatchExecuteInsertEventAnnotation(internalId, 'a', annotations, command);

                BatchExecuteInsertTrackingDataItems(internalId, 'a', items, command);
            }

            private void BuildInsertActivityStatusEventParameters(long internalId, long activityInstanceId, int parameterId, ActivityTrackingRecord record, DbCommand command)
            {
                string paramIdString = parameterId.ToString(CultureInfo.InvariantCulture);
                //
                // If we have the activity's instance id use it to avoid the look up in the db
                DbParameter param = DbResourceAllocator.NewDbParameter("@ActivityInstanceId" + paramIdString, DbType.Int64, System.Data.ParameterDirection.InputOutput);
                command.Parameters.Add(param);

                if (activityInstanceId > 0)
                    param.Value = activityInstanceId;

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@QualifiedName" + paramIdString, record.QualifiedName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ContextGuid" + paramIdString, record.ContextGuid));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ParentContextGuid" + paramIdString, record.ParentContextGuid));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ExecutionStatusId" + paramIdString, (int)record.ExecutionStatus));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventDateTime" + paramIdString, record.EventDateTime));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventOrder" + paramIdString, record.EventOrder));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ActivityExecutionStatusEventId" + paramIdString, DbType.Int64, ParameterDirection.Output));

            }

            #endregion

            #region Sql Commands - InsertUserEvent

            private void ExecuteInsertUserEvent(object param)
            {
                UserTrackingRecord record = param as UserTrackingRecord;

                if (null == record)
                    throw new ArgumentException(ExecutionStringManager.InvalidUserTrackingRecordParameter, "param");

                DbConnection conn = DbResourceAllocator.OpenNewConnection();

                DbTransaction tx = null;

                try
                {
                    tx = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                    DbCommand command = conn.CreateCommand();
                    command.Transaction = tx;

                    ExecuteInsertUserEvent(_internalId, record, command);

                    tx.Commit();
                }
                catch (Exception)
                {
                    //
                    // Rollback can throw - ignore these exceptions
                    // so we don't lose the original exception
                    try
                    {
                        if (null != tx)
                            tx.Rollback();
                    }
                    catch (Exception)
                    {
                    }

                    //
                    // Re-throw original exception
                    throw;
                }
                finally
                {
                    if ((null != conn) && (ConnectionState.Closed != conn.State))
                        conn.Close();

                }

                return;
            }

            private void ExecuteInsertUserEvent(long internalId, UserTrackingRecord record, DbCommand command)
            {
                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentException();

                long aid = -1;
                bool cached = false;
                //
                // Check if we have the activityInstanceId in the cache - we cache to avoid repeatedly searching this table.
                string id = BuildQualifiedNameVarName(record.QualifiedName, record.ContextGuid, record.ParentContextGuid);
                if (TryGetActivityInstanceId(id, out aid))
                    cached = true;

                BuildInsertUserEventParameters(internalId, aid, record, command);
                command.ExecuteNonQuery();
                //
                // If we didn't already have the activityInstanceId get it from the IN/OUT param and put it in the cache
                if (!cached)
                    SetActivityInstanceId(id, (long)command.Parameters["@ActivityInstanceId"].Value);

                long eventId = (long)command.Parameters["@UserEventId"].Value;

                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(10);
                List<KeyValuePair<long, TrackingDataItem>> items = new List<KeyValuePair<long, TrackingDataItem>>(10);

                foreach (string s in record.Annotations)
                    annotations.Add(new KeyValuePair<long, string>(eventId, s));

                foreach (TrackingDataItem item in record.Body)
                    items.Add(new KeyValuePair<long, TrackingDataItem>(eventId, item));

                BatchExecuteInsertEventAnnotation(internalId, 'u', annotations, command);

                BatchExecuteInsertTrackingDataItems(internalId, 'u', items, command);
            }

            private void BuildInsertUserEventParameters(long internalId, long activityInstanceId, UserTrackingRecord record, DbCommand command)
            {
                Debug.Assert(internalId != -1, "Invalid internalId");
                Debug.Assert((command != null), "Null command passed to BuildInsertActivityStatusEventParameters");

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertUserEvent]";

                command.Parameters.Clear();

                DbParameter param = DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", DbType.Int64);
                command.Parameters.Add(param);
                param.Value = internalId;

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventOrder", record.EventOrder));
                //
                // If we have the activity's instance id use it to avoid the look up in the db
                param = DbResourceAllocator.NewDbParameter("@ActivityInstanceId", DbType.Int64, System.Data.ParameterDirection.InputOutput);
                command.Parameters.Add(param);

                if (activityInstanceId > 0)
                {
                    param.Value = activityInstanceId;
                }
                else
                {
                    //
                    // Keep the network traffic down - only include the fields needed 
                    // to insert an ActivityInstance record if we don't have the activityInstanceId
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@QualifiedName", record.QualifiedName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ContextGuid", record.ContextGuid));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ParentContextGuid", record.ParentContextGuid));
                }

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventDateTime", record.EventDateTime));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserDataKey", record.UserDataKey));

                if (null != record.UserData)
                {
                    Type t = record.UserData.GetType();
                    Byte[] data = null;
                    bool nonSerializable = false;
                    string userDataString = null;

                    if (!(record.UserData is SerializedDataItem))
                        SerializeDataItem(record.UserData, out data, out nonSerializable);

                    SerializedDataItem sItem = record.UserData as SerializedDataItem;
                    data = sItem.SerializedData;
                    nonSerializable = sItem.NonSerializable;
                    userDataString = sItem.StringData;

                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserDataTypeFullName", t.FullName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserDataAssemblyFullName", t.Assembly.FullName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserData_Str", userDataString));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserData_Blob", data));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserDataNonSerializable", nonSerializable));
                }
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@UserEventId", DbType.Int64, ParameterDirection.Output));
            }

            #endregion

            #region Sql Commands - InsertTrackingDataItem

            private void BatchExecuteInsertTrackingDataItems(long internalId, char eventTypeId, IList<KeyValuePair<long, TrackingDataItem>> items, DbCommand command)
            {
                if (null == items || items.Count <= 0)
                    return;
                //
                // If the list is smaller than the batch size just push the whole thing
                if (items.Count <= _dataItemBatchSize)
                {
                    ExecuteInsertTrackingDataItems(internalId, eventTypeId, items, command);
                    return;
                }
                //
                // Need to split the list into max batch size chunks
                List<KeyValuePair<long, TrackingDataItem>> batch = new List<KeyValuePair<long, TrackingDataItem>>(_dataItemBatchSize);
                foreach (KeyValuePair<long, TrackingDataItem> kvp in items)
                {
                    batch.Add(kvp);
                    if (batch.Count == _dataItemBatchSize)
                    {
                        ExecuteInsertTrackingDataItems(internalId, eventTypeId, batch, command);
                        batch.Clear();
                    }
                }
                //
                // Send anything that hasn't been sent
                if (batch.Count > 0)
                    ExecuteInsertTrackingDataItems(internalId, eventTypeId, batch, command);
            }

            private void ExecuteInsertTrackingDataItems(long internalId, char eventTypeId, IList<KeyValuePair<long, TrackingDataItem>> items, DbCommand command)
            {
                Debug.Assert(internalId != -1, "Invalid internalId");
                if (null == items || items.Count <= 0)
                    return;

                if (items.Count > _dataItemAnnotationBatchSize)
                    throw new ArgumentOutOfRangeException("items");

                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentException();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertTrackingDataItemMultiple]";

                command.Parameters.Clear();
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventTypeId", eventTypeId));

                int i = 1; // base 1 to match parameter names
                foreach (KeyValuePair<long, TrackingDataItem> kvp in items)
                {
                    string index = (i++).ToString(CultureInfo.InvariantCulture);

                    SerializedDataItem sItem = kvp.Value as SerializedDataItem;
                    if (null == sItem)
                        sItem = SerializeDataItem(kvp.Value);

                    Type t = sItem.Type;

                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventId" + index, kvp.Key));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@FieldName" + index, sItem.FieldName));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TypeFullName" + index, ((null == t) ? null : t.FullName)));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@AssemblyFullName" + index, ((null == t) ? null : t.Assembly.FullName)));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Data_Str" + index, sItem.StringData));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Data_Blob" + index, sItem.SerializedData));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@DataNonSerializable" + index, sItem.NonSerializable));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TrackingDataItemId" + index, DbType.Int64, System.Data.ParameterDirection.Output));
                }

                command.ExecuteNonQuery();
                //
                // Get all the out parameters holding the data item record ids
                // This keeps us from repeatedly going into the parameters collection
                // below if a data item has more than one annotation
                List<long> ids = new List<long>(_dataItemAnnotationBatchSize);
                for (i = 0; i < items.Count; i++)
                {
                    string index = (i + 1).ToString(CultureInfo.InvariantCulture);
                    ids.Insert(i, (long)command.Parameters["@TrackingDataItemId" + index].Value);
                }

                //
                // Go through all the data items and send all the annotations in batches
                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(_dataItemAnnotationBatchSize);
                i = 0;

                foreach (KeyValuePair<long, TrackingDataItem> kvp in items)
                {
                    TrackingDataItem item = kvp.Value;
                    long dataItemId = ids[i++];

                    foreach (string s in item.Annotations)
                    {
                        annotations.Add(new KeyValuePair<long, string>(dataItemId, s));
                        if (annotations.Count == _dataItemAnnotationBatchSize)
                        {
                            ExecuteInsertAnnotation(internalId, annotations, command);
                            annotations.Clear();
                        }
                    }
                }
                //
                // If we have anything left send them.
                if (annotations.Count > 0)
                    ExecuteInsertAnnotation(internalId, annotations, command);
            }

            private void ExecuteInsertAnnotation(long internalId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                if (null == annotations || annotations.Count <= 0)
                    return;

                if (annotations.Count > _dataItemAnnotationBatchSize)
                    throw new ArgumentOutOfRangeException("annotations");

                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentNullException("command");

                command.Parameters.Clear();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertTrackingDataItemAnnotationMultiple]";

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));

                int i = 1; // base 1 to match parameter names
                foreach (KeyValuePair<long, string> kvp in annotations)
                {
                    string index = (i++).ToString(CultureInfo.InvariantCulture);
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@HasData" + index, true));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TrackingDataItemId" + index, kvp.Key));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Annotation" + index, kvp.Value));
                }

                command.ExecuteNonQuery();

                return;
            }

            private void BatchExecuteInsertEventAnnotation(long internalId, char eventTypeId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                if (null == annotations || annotations.Count <= 0)
                    return;

                //
                // If the list is smaller than the max batch size just send it directly
                if (annotations.Count <= _eventAnnotationBatchSize)
                {
                    ExecuteInsertEventAnnotation(internalId, eventTypeId, annotations, command);
                    return;
                }
                //
                // Need to split the list into max batch size chunks
                List<KeyValuePair<long, string>> batch = new List<KeyValuePair<long, string>>(_eventAnnotationBatchSize);
                foreach (KeyValuePair<long, string> kvp in annotations)
                {
                    batch.Add(kvp);
                    if (batch.Count == _eventAnnotationBatchSize)
                    {
                        ExecuteInsertEventAnnotation(internalId, eventTypeId, batch, command);
                        batch.Clear();
                    }
                }
                //
                // Send anything that hasn't been sent
                if (batch.Count > 0)
                    ExecuteInsertEventAnnotation(internalId, eventTypeId, batch, command);
            }

            private void ExecuteInsertEventAnnotation(long internalId, char eventTypeId, IList<KeyValuePair<long, string>> annotations, DbCommand command)
            {
                Debug.Assert(internalId != -1, "Invalid internalId");

                if (null == annotations || annotations.Count <= 0)
                    return;

                if (annotations.Count > _eventAnnotationBatchSize)
                    throw new ArgumentOutOfRangeException("annotations");

                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentNullException("command");

                command.Parameters.Clear();
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertEventAnnotationMultiple]";

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventTypeId", eventTypeId));

                int i = 1; //base 1 to match parameter names
                foreach (KeyValuePair<long, string> kvp in annotations)
                {
                    string index = (i++).ToString(CultureInfo.InvariantCulture);
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@HasData" + index, true));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@EventId" + index, kvp.Key));
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Annotation" + index, kvp.Value));
                }

                command.ExecuteNonQuery();

                return;
            }


            #endregion

            #region Workflow Change

            private void ExecuteInsertWorkflowChange(object param)
            {
                WorkflowTrackingRecord record = param as WorkflowTrackingRecord;

                if (null == record)
                    throw new ArgumentException(ExecutionStringManager.InvalidWorkflowTrackingRecordParameter, "param");

                DbCommand command = DbResourceAllocator.NewCommand();

                try
                {
                    if (ConnectionState.Open != command.Connection.State)
                        command.Connection.Open();
                    command.Transaction = command.Connection.BeginTransaction();

                    ExecuteInsertWorkflowChange(_internalId, record, command);

                    command.Transaction.Commit();
                }
                catch (Exception)
                {
                    //
                    // Rollback can throw - ignore these exceptions
                    // so we don't lose the original exception
                    try
                    {
                        if ((null != command) && (null != command.Transaction))
                            command.Transaction.Rollback();
                    }
                    catch (Exception)
                    {
                    }

                    //
                    // Re-throw original exception
                    throw;
                }
                finally
                {
                    if ((null != command) && (null != command.Connection) && (ConnectionState.Closed != command.Connection.State))
                        command.Connection.Close();
                }

                return;
            }

            private void ExecuteInsertWorkflowChange(long internalId, WorkflowTrackingRecord record, DbCommand command)
            {
                if (null == record)
                    throw new ArgumentNullException("record");

                if (null == record.EventArgs)
                    throw new InvalidOperationException(ExecutionStringManager.InvalidWorkflowChangeArgs);

                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentNullException("command");
                //
                // If we haven't already serialized do so now.
                // This is work we have to do to write to store in the db anyway.
                if (!(record.EventArgs is SerializedWorkflowChangedEventArgs))
                    record = SerializeRecord(record);
                //
                // Insert the workflow instance event
                BuildInsertWorkflowInstanceEventParameters(internalId, record, null, command);

                command.ExecuteNonQuery();
                //
                // Get the event id for added/removed activities and annotations
                long eventId = (long)command.Parameters["@WorkflowInstanceEventId1"].Value;

                SerializedWorkflowChangedEventArgs sargs = (SerializedWorkflowChangedEventArgs)record.EventArgs;
                //
                // Normalize the activities that have been added/removed if we're tracking definitions
                if ((null != sargs.AddedActivities) && (sargs.AddedActivities.Count > 0))
                {
                    foreach (AddedActivity added in sargs.AddedActivities)
                        ExecuteInsertAddedActivity(internalId, added.QualifiedName, added.ParentQualifiedName, added.ActivityTypeFullName, added.ActivityTypeAssemblyFullName, added.AddedActivityActionXoml, eventId, added.Order, command);
                }

                if ((null != sargs.RemovedActivities) && (sargs.RemovedActivities.Count > 0))
                {
                    foreach (RemovedActivity removed in sargs.RemovedActivities)
                        ExecuteInsertRemovedActivity(internalId, removed.QualifiedName, removed.ParentQualifiedName, removed.RemovedActivityActionXoml, eventId, removed.Order, command);
                }

                List<KeyValuePair<long, string>> annotations = new List<KeyValuePair<long, string>>(record.Annotations.Count);
                foreach (string s in record.Annotations)
                    annotations.Add(new KeyValuePair<long, string>(eventId, s));
                BatchExecuteInsertEventAnnotation(internalId, 'w', annotations, command);
            }

            private void ExecuteInsertAddedActivity(long internalId, string qualifiedName, string parentQualifiedName, string typeFullName, string assemblyFullName, string addedActivityActionXoml, long eventId, int order, DbCommand command)
            {
                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentNullException("command");

                command.Parameters.Clear();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertAddedActivity]";

                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId", eventId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@QualifiedName", qualifiedName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@TypeFullName", typeFullName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@AssemblyFullName", assemblyFullName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ParentQualifiedName", parentQualifiedName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@AddedActivityAction", addedActivityActionXoml));
                if (-1 == order)
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Order", DBNull.Value));
                else
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Order", order));

                command.ExecuteNonQuery();
            }

            private void ExecuteInsertRemovedActivity(long internalId, string qualifiedName, string parentQualifiedName, string removedActivityActionXoml, long eventId, int order, DbCommand command)
            {
                if ((null == command) || (null == command.Connection) || (ConnectionState.Open != command.Connection.State))
                    throw new ArgumentNullException("command");

                command.Parameters.Clear();

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "[dbo].[InsertRemovedActivity]";
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceInternalId", internalId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@WorkflowInstanceEventId", eventId));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@QualifiedName", qualifiedName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@ParentQualifiedName", parentQualifiedName));
                command.Parameters.Add(DbResourceAllocator.NewDbParameter("@RemovedActivityAction", removedActivityActionXoml));
                if (-1 == order)
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Order", DBNull.Value));
                else
                    command.Parameters.Add(DbResourceAllocator.NewDbParameter("@Order", order));

                command.ExecuteNonQuery();
            }

            #endregion

            #region Utility

            private bool TryGetActivityInstanceId(string key, out long id)
            {
                //
                // Check the cache of committed ids
                if (_activityInstanceId.TryGetValue(key, out id))
                    return true;
                //
                // If we're batched check the cache of temp ids generated during this batch commit
                if (_isTrans)
                    return _tmpActivityInstanceId.TryGetValue(key, out id);
                else
                    return false; // not batched so we didn't find the id
            }

            private void SetActivityInstanceId(string key, long id)
            {
                //
                // If we're batched put the ids in the temp member
                // If the commit is successful we'll move these to the real member
                // in IPendingWork.Complete
                if (_isTrans)
                {
                    if (!_tmpActivityInstanceId.ContainsKey(key))
                        _tmpActivityInstanceId.Add(key, id);
                }
                else
                {
                    if (!_activityInstanceId.ContainsKey(key))
                        _activityInstanceId.Add(key, id);
                }
            }

            private void RemoveActivityInstanceId(string key)
            {
                //
                // Remove from both the temp and real caches
                if (_isTrans)
                {
                    if (_tmpActivityInstanceId.ContainsKey(key))
                        _tmpActivityInstanceId.Remove(key);
                }

                if (_activityInstanceId.ContainsKey(key))
                    _activityInstanceId.Remove(key);
            }

            private string GetSqlDateTimeString(DateTime dateTime)
            {
                return dateTime.Year.ToString(System.Globalization.CultureInfo.InvariantCulture) + PadToDblDigit(dateTime.Month) + PadToDblDigit(dateTime.Day) + " " + dateTime.Hour.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + dateTime.Minute.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + dateTime.Second.ToString(System.Globalization.CultureInfo.InvariantCulture) + ":" + dateTime.Millisecond.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }


            private string PadToDblDigit(int num)
            {
                string s = num.ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (s.Length == 1)
                    return "0" + s;
                else
                    return s;
            }

            /// <summary>
            /// Build a string to uniquely identify each activity that should be recorded as a seperate instance.
            /// A separate instance is defined by the combination of QualifiedName, Context and ParentContext
            /// </summary>
            /// <param name="record"></param>
            /// <returns></returns>
            private string BuildQualifiedNameVarName(string qId, Guid context, Guid parentContext)
            {
                Guid hashed = HashHelper.HashServiceType(qId);
                return hashed.ToString().Replace('-', '_') + "_" + context.ToString().Replace('-', '_') + "_" + parentContext.ToString().Replace('-', '_');
            }

            private ActivityTrackingRecord SerializeRecord(ActivityTrackingRecord record)
            {
                if ((null == record.Body) || (0 == record.Body.Count))
                    return record;

                for (int i = 0; i < record.Body.Count; i++)
                    record.Body[i] = SerializeDataItem(record.Body[i]);

                return record;
            }

            private UserTrackingRecord SerializeRecord(UserTrackingRecord record)
            {
                if (((null == record.Body) || (0 == record.Body.Count)) && (null == record.EventArgs) && (null == record.UserData))
                    return record;

                if (null != record.UserData)
                {
                    SerializedDataItem item = new SerializedDataItem();

                    byte[] data = null;
                    bool nonSerializable;
                    SerializeDataItem(record.UserData, out data, out nonSerializable);

                    item.Type = record.UserData.GetType();
                    item.StringData = record.UserData.ToString();
                    item.SerializedData = data;
                    item.NonSerializable = nonSerializable;

                    record.UserData = item;
                }

                for (int i = 0; i < record.Body.Count; i++)
                    record.Body[i] = SerializeDataItem(record.Body[i]);

                return record;
            }

            private WorkflowTrackingRecord SerializeRecord(WorkflowTrackingRecord record)
            {
                if (null == record.EventArgs)
                    return record;

                SerializedEventArgs args;
                if (TrackingWorkflowEvent.Changed == record.TrackingWorkflowEvent)
                {
                    //
                    // Convert the WorkflowChanged items
                    SerializedWorkflowChangedEventArgs sargs = new SerializedWorkflowChangedEventArgs();
                    TrackingWorkflowChangedEventArgs wargs = (TrackingWorkflowChangedEventArgs)record.EventArgs;
                    if (null != wargs)
                    {
                        for (int i = 0; i < wargs.Changes.Count; i++)
                        {
                            WorkflowChangeAction action = wargs.Changes[i];
                            if (action is RemovedActivityAction)
                                AddRemovedActivity((RemovedActivityAction)action, i, sargs.RemovedActivities);
                            else if (action is AddedActivityAction)
                                AddAddedActivity((AddedActivityAction)action, i, sargs.AddedActivities);
                        }
                    }
                    args = sargs;
                }
                else
                {
                    args = new SerializedEventArgs();
                    byte[] data = null;
                    bool nonSerializable;

                    SerializeDataItem(record.EventArgs, out data, out nonSerializable);
                    args.SerializedArgs = data;
                    //
                    // nonSerializable will only be null for SerializationExceptions, all others bubble
                    if (nonSerializable)
                    {
                        //
                        // Something didn't serialize.
                        // If this is an exception or terminated event it is most likely the Exception member
                        // Save the exception message - better than losing all record of the exception
                        Exception e;
                        switch (record.TrackingWorkflowEvent)
                        {
                            case TrackingWorkflowEvent.Terminated:
                                e = ((TrackingWorkflowTerminatedEventArgs)record.EventArgs).Exception;
                                if (null != e)
                                {
                                    SerializeDataItem(e.ToString(), out data, out nonSerializable);
                                    args.SerializedArgs = data;
                                }
                                break;
                            case TrackingWorkflowEvent.Exception:
                                e = ((TrackingWorkflowExceptionEventArgs)record.EventArgs).Exception;
                                if (null != e)
                                {
                                    SerializeDataItem(e.ToString(), out data, out nonSerializable);
                                    args.SerializedArgs = data;
                                }
                                break;
                        }
                    }
                }
                //
                // Set the type of the EventArgs and then 
                // put the serialized item in the args member, 
                // we don't need the original Args object any longer
                args.Type = record.EventArgs.GetType();
                record.EventArgs = args;

                return record;
            }

            private void AddRemovedActivity(RemovedActivityAction removedAction, int order, IList<RemovedActivity> activities)
            {
                Activity removed = removedAction.OriginalRemovedActivity;
                RemovedActivity removedActivity = new RemovedActivity();
                removedActivity.Order = order;
                removedActivity.QualifiedName = removed.QualifiedName;
                if (null != removed.Parent)
                    removedActivity.ParentQualifiedName = removed.Parent.QualifiedName;
                //
                // Save the defintion of this change
                removedActivity.RemovedActivityActionXoml = GetXomlDocument(removedAction);
                activities.Add(removedActivity);
                //
                // Recursively add all contained activities to the removed list
                if (removed is CompositeActivity)
                {
                    foreach (Activity activity in ((CompositeActivity)removed).Activities)
                    {
                        AddRemovedActivity(activity, activities);
                    }
                }
            }

            private void AddRemovedActivity(Activity removed, IList<RemovedActivity> activities)
            {
                RemovedActivity removedActivity = new RemovedActivity();
                removedActivity.Order = -1;
                removedActivity.QualifiedName = removed.QualifiedName;
                if (null != removed.Parent)
                    removedActivity.ParentQualifiedName = removed.Parent.QualifiedName;
                activities.Add(removedActivity);
                //
                // Recursively add all contained activities to the removed list
                if (removed is CompositeActivity)
                {
                    foreach (Activity activity in ((CompositeActivity)removed).Activities)
                    {
                        AddRemovedActivity(activity, activities);
                    }
                }
            }

            private void AddAddedActivity(AddedActivityAction addedAction, int order, IList<AddedActivity> activities)
            {
                Activity added = addedAction.AddedActivity;
                AddedActivity addedActivity = new AddedActivity();
                addedActivity.Order = order;
                Type type = added.GetType();

                addedActivity.ActivityTypeFullName = type.FullName;
                addedActivity.ActivityTypeAssemblyFullName = type.Assembly.FullName;
                addedActivity.QualifiedName = added.QualifiedName;
                if (null != added.Parent)
                    addedActivity.ParentQualifiedName = added.Parent.QualifiedName;
                addedActivity.AddedActivityActionXoml = GetXomlDocument(addedAction);

                activities.Add(addedActivity);
                //
                // Recursively add all contained activities to the added list
                if (added is CompositeActivity)
                {
                    foreach (Activity activity in ((CompositeActivity)added).Activities)
                    {
                        AddAddedActivity(activity, activities);
                    }
                }
            }

            private void AddAddedActivity(Activity added, IList<AddedActivity> activities)
            {
                AddedActivity addedActivity = new AddedActivity();
                addedActivity.Order = -1;
                Type type = added.GetType();

                addedActivity.ActivityTypeFullName = type.FullName;
                addedActivity.ActivityTypeAssemblyFullName = type.Assembly.FullName;
                addedActivity.QualifiedName = added.QualifiedName;
                if (null != added.Parent)
                    addedActivity.ParentQualifiedName = added.Parent.QualifiedName;

                activities.Add(addedActivity);
                //
                // Recursively add all contained activities to the added list
                if (added is CompositeActivity)
                {
                    foreach (Activity activity in ((CompositeActivity)added).Activities)
                    {
                        AddAddedActivity(activity, activities);
                    }
                }
            }

            private SerializedDataItem SerializeDataItem(TrackingDataItem item)
            {
                if (null == item)
                    return null;

                SerializedDataItem s = new SerializedDataItem();
                s.Data = item.Data;
                s.Annotations.AddRange(item.Annotations);
                s.FieldName = item.FieldName;

                if (null != item.Data)
                {
                    byte[] state = null;
                    bool nonSerializable;
                    SerializeDataItem(item.Data, out state, out nonSerializable);
                    s.SerializedData = state;
                    s.StringData = item.Data.ToString();
                    s.Type = item.Data.GetType();
                    s.NonSerializable = nonSerializable;
                }

                return s;
            }

            /// <summary>
            /// Binary serialize an object.  Used to persist trackingDataItems.
            /// </summary>
            /// <param name="data"></param>
            /// <param name="state"></param>
            private void SerializeDataItem(object data, out byte[] state, out bool nonSerializable)
            {
                nonSerializable = false;
                state = null;
                if (null == data)
                    return;

                MemoryStream stream = new MemoryStream(1024);
                BinaryFormatter bf = new BinaryFormatter();

                try
                {
                    bf.Serialize(stream, data);

                    state = new byte[stream.Length];
                    stream.Position = 0;

                    if (stream.Length > Int32.MaxValue)
                        return;
                    else
                    {
                        int read = 0, totalRead = 0, cbToRead = 0;
                        do
                        {
                            totalRead += read;
                            cbToRead = (int)stream.Length - totalRead;
                            read = stream.Read(state, totalRead, cbToRead);
                        } while (read > 0);
                    }
                }
                catch (SerializationException)
                {
                    nonSerializable = true;
                    return;
                }
                finally
                {
                    stream.Close();
                }
            }
            /// <summary>
            /// Make string sql safe
            /// </summary>
            /// <param name="val"></param>
            /// <returns></returns>
            private string SqlEscape(string val)
            {
                if (null == val)
                    return null;

                return val.Replace("'", "''");
            }
            /*
            static char[] hexDigits = {
            '0', '1', '2', '3', '4', '5', '6', '7',
            '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};
            /// <summary>
            /// Convert a byte array to a string of hex chars for sql image type
            /// </summary>
            /// <param name="bytes"></param>
            /// <returns></returns>
            private static string ToHexString( byte[] bytes )
            {
                if ( null == bytes )
                    return null;

                if ( 0 == bytes.Length )
                    return null;

                char[] chars = new char[bytes.Length * 2];
                for ( int i = 0; i < bytes.Length; i++ )
                {
                    int b = bytes[i];
                    chars[i * 2] = hexDigits[b >> 4];
                    chars[i * 2 + 1] = hexDigits[b & 0xF];
                }
                return "0x" + new string( chars );
            }
            */
            private void GetCallPathKeys(IList<string> callPath)
            {
                if ((null == callPath) || (callPath.Count <= 0))
                    return;

                for (int i = 0; i < callPath.Count; i++)
                {
                    _callPathKey = _callPathKey + "." + callPath[i];
                    if (i < callPath.Count - 1)
                        _parentCallPathKey = _parentCallPathKey + "." + callPath[i];
                }

                if (null != _callPathKey)
                    _callPathKey = SqlEscape(_callPathKey.Substring(1));

                if (null != _parentCallPathKey)
                    _parentCallPathKey = SqlEscape(_parentCallPathKey.Substring(1));
            }

            private string GetActivitiesXml(CompositeActivity root)
            {
                if (null == root)
                    return null;

                StringBuilder sb = new StringBuilder();
                XmlWriter writer = XmlWriter.Create(sb);

                try
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("Activities");

                    WriteActivity(root, writer);

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }
                finally
                {
                    writer.Flush();
                    writer.Close();
                }

                return sb.ToString();
            }

            private void WriteActivity(Activity activity, XmlWriter writer)
            {
                if (null == activity)
                    return;
                if (null == writer)
                    throw new ArgumentNullException("writer");

                Type t = activity.GetType();

                writer.WriteStartElement("Activity");
                writer.WriteElementString("TypeFullName", t.FullName);
                writer.WriteElementString("AssemblyFullName", t.Assembly.FullName);
                writer.WriteElementString("QualifiedName", activity.QualifiedName);
                //
                // Don't write the element if the value is null, sql will see a missing element as a null value
                if (null != activity.Parent)
                    writer.WriteElementString("ParentQualifiedName", activity.Parent.QualifiedName);
                writer.WriteEndElement();

                if (activity is CompositeActivity)
                    foreach (Activity a in GetAllEnabledActivities((CompositeActivity)activity))
                        WriteActivity(a, writer);
            }


            // This function returns all the executable activities including secondary flow activities.
            private IList<Activity> GetAllEnabledActivities(CompositeActivity compositeActivity)
            {
                if (compositeActivity == null)
                    throw new ArgumentNullException("compositeActivity");

                List<Activity> allActivities = new List<Activity>(compositeActivity.EnabledActivities);

                foreach (Activity secondaryFlowActivity in ((ISupportAlternateFlow)compositeActivity).AlternateFlowActivities)
                {
                    if (!allActivities.Contains(secondaryFlowActivity))
                        allActivities.Add(secondaryFlowActivity);
                }

                return allActivities;
            }


            internal string GetXomlDocument(object obj)
            {
                string xomlText = null;
                using (StringWriter stringWriter = new StringWriter(System.Globalization.CultureInfo.InvariantCulture))
                {
                    using (XmlWriter xmlWriter = CreateXmlWriter(stringWriter))
                    {
                        WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                        serializer.Serialize(xmlWriter, obj);
                        xomlText = stringWriter.ToString();
                    }
                }
                return xomlText;
            }


            #endregion

        }
    }
}
