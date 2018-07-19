using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel.Design.Serialization;

using System.Workflow.Runtime;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.Runtime.Hosting;

namespace System.Workflow.Runtime.Tracking
{
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SqlTrackingWorkflowInstance
    {
        #region Private Members

        private delegate void LoadFromReader(SqlDataReader reader, object parameter);

        private static int _deadlock = 1205;
        private static short _retries = 5;
        private string _connectionString = null;
        private bool _autoRefresh = false;

        DateTime _currDT = DateTime.UtcNow,
            _actMinDT = SqlDateTime.MinValue.Value,
            _userMinDT = SqlDateTime.MinValue.Value,
            _instMinDT = SqlDateTime.MinValue.Value,
            _childMinDT = SqlDateTime.MinValue.Value,
            _changesMinDT = SqlDateTime.MinValue.Value,
            _invMinDT = SqlDateTime.MinValue.Value;

        long _internalId = -1;

        Guid _id;
        DateTime _initialized;
        Guid _invoker = Guid.Empty;
        WorkflowStatus _status;
        Type _workflowType = null;
        bool _changed = false;

        List<ActivityTrackingRecord> _activityEvents = new List<ActivityTrackingRecord>();
        List<UserTrackingRecord> _userEvents = new List<UserTrackingRecord>();
        List<WorkflowTrackingRecord> _workflowEvents = new List<WorkflowTrackingRecord>();
        List<SqlTrackingWorkflowInstance> _invoked = new List<SqlTrackingWorkflowInstance>();

        Activity _def = null;

        #endregion Private Members

        #region Constructors

        private SqlTrackingWorkflowInstance() { }

        internal SqlTrackingWorkflowInstance(string connectionString)
        {
            if (null == connectionString)
                throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;
        }

        #endregion Constructors

        #region Properties

        public bool AutoRefresh
        {
            get { return _autoRefresh; }
            set { _autoRefresh = value; }
        }

        public Guid WorkflowInstanceId
        {
            get { return _id; }
            set { _id = value; }
        }

        public long WorkflowInstanceInternalId
        {
            get { return _internalId; }
            set { _internalId = value; }
        }

        public DateTime Initialized
        {
            get { return _initialized; }
            set { _initialized = value; }
        }

        public Guid InvokingWorkflowInstanceId
        {
            get { return _invoker; }
            set { _invoker = value; }
        }

        public WorkflowStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public Type WorkflowType
        {
            get { return _workflowType; }
            set { _workflowType = value; }
        }

        public bool WorkflowDefinitionUpdated
        {
            get
            {
                if (_autoRefresh)
                    Refresh();

                LoadDef();
                return _changed;
            }
        }

        public IList<ActivityTrackingRecord> ActivityEvents
        {
            get
            {
                if (_autoRefresh)
                    Refresh();

                LoadActivityEvents();
                return _activityEvents;
            }
        }

        public IList<UserTrackingRecord> UserEvents
        {
            get
            {
                if (_autoRefresh)
                    Refresh();

                LoadUserEvents();
                return _userEvents;
            }
        }

        public IList<WorkflowTrackingRecord> WorkflowEvents
        {
            get
            {
                if (_autoRefresh)
                    Refresh();

                LoadWorkflowEvents();
                return _workflowEvents;
            }
        }

        public Activity WorkflowDefinition
        {
            get
            {
                if (_autoRefresh)
                    Refresh();

                LoadDef();
                return _def;
            }
        }

        public IList<SqlTrackingWorkflowInstance> InvokedWorkflows
        {
            get
            {
                if (_autoRefresh)
                    Refresh();
                LoadInvokedWorkflows();
                return _invoked;
            }
        }
        #endregion Properties

        public void Refresh()
        {
            _currDT = DateTime.UtcNow;
        }

        private void LoadActivityEvents()
        {
            SqlCommand cmd = CreateInternalIdDateTimeCommand("[dbo].[GetActivityEventsWithDetails]", _actMinDT);

            ExecuteRetried(cmd, LoadActivityEventsFromReader);
        }

        private void LoadActivityEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            //
            // There should always be 4 recordsets in this reader!
            //

            Dictionary<long, ActivityTrackingRecord> activities = new Dictionary<long, ActivityTrackingRecord>();
            //
            // Build a dictionary of activity records so that we can match 
            // annotation and artifact records from subsequent recordsets
            DateTime tmpMin = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                string qId = reader.GetString(0);
                ActivityExecutionStatus status = (ActivityExecutionStatus)reader[1];
                DateTime dt = reader.GetDateTime(2);
                Guid context = reader.GetGuid(3), parentContext = reader.GetGuid(4);
                int order = reader.GetInt32(5);

                if (reader.IsDBNull(6) || reader.IsDBNull(7))
                    throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.CurrentCulture, ExecutionStringManager.SqlTrackingTypeNotFound, qId));

                Type type = Type.GetType(reader.GetString(6) + ", " + reader.GetString(7), true, false);
                long eventId = reader.GetInt64(8);

                DateTime dbDt = reader.GetDateTime(9);

                activities.Add(eventId, new ActivityTrackingRecord(type, qId, context, parentContext, status, dt, order, null));
                if (dbDt > tmpMin)
                    tmpMin = dbDt;
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);

            //
            // If we have annotations on the event itself, add them
            while (reader.Read())
            {
                long eventId = reader.GetInt64(0);
                string annotation = null;

                if (!reader.IsDBNull(1))
                    annotation = reader.GetString(1);

                ActivityTrackingRecord activity = null;
                if (activities.TryGetValue(eventId, out activity))
                {
                    if (null != activity)
                        activity.Annotations.Add(annotation);
                }
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);

            //
            // Build a dictionary of artifact records so that we can match 
            // annotation records from subsequent recordsets
            BinaryFormatter formatter = new BinaryFormatter();
            Dictionary<long, TrackingDataItem> artifacts = new Dictionary<long, TrackingDataItem>();
            while (reader.Read())
            {
                long eventId = reader.GetInt64(0);
                long artId = reader.GetInt64(1);
                string name = reader.GetString(2), strData = null;
                object data = null;
                //
                // These may both be null
                if (!reader.IsDBNull(3))
                    strData = reader.GetString(3);

                if (!reader.IsDBNull(4))
                    data = formatter.Deserialize(new MemoryStream((Byte[])reader[4]));

                TrackingDataItem item = new TrackingDataItem();
                item.FieldName = name;
                if (null != data)
                    item.Data = data;
                else
                    item.Data = strData;

                artifacts.Add(artId, item);
                //
                // Find the event to which this artifact belongs and add it to the record
                ActivityTrackingRecord activity = null;
                if (activities.TryGetValue(eventId, out activity))
                {
                    if (null != activity)
                        activity.Body.Add(item);
                }
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidActivityEventReader);

            //
            // If we have annotations add them to the appropriate artifact
            while (reader.Read())
            {
                long artId = reader.GetInt64(0);
                string annotation = null;

                if (!reader.IsDBNull(1))
                    annotation = reader.GetString(1);
                //
                // Find the right artifact and give it the annotation
                TrackingDataItem item = null;
                if (artifacts.TryGetValue(artId, out item))
                {
                    if (null != item)
                        item.Annotations.Add(annotation);
                }
            }

            _activityEvents.AddRange(activities.Values);

            //
            // Set the min value to the most recent event that we got with this query
            // Don't overwrite the previous min if nothing came back for this query
            if (tmpMin > SqlDateTime.MinValue.Value)
                _actMinDT = tmpMin;
            return;
        }

        private void LoadUserEvents()
        {
            SqlCommand cmd = CreateInternalIdDateTimeCommand("[dbo].[GetUserEventsWithDetails]", _userMinDT);

            ExecuteRetried(cmd, LoadUserEventsFromReader);
        }

        private void LoadUserEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            //
            // There should always be 4 recordsets in this reader!
            //

            BinaryFormatter formatter = new BinaryFormatter();
            Dictionary<long, UserTrackingRecord> userEvents = new Dictionary<long, UserTrackingRecord>();
            //
            // Build a dictionary of activity records so that we can match 
            // annotation and artifact records from subsequent recordsets
            DateTime tmpMin = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                string qId = reader.GetString(0);
                DateTime dt = reader.GetDateTime(1);
                Guid context = reader.GetGuid(2), parentContext = reader.GetGuid(3);
                int order = reader.GetInt32(4);
                string key = null;
                if (!reader.IsDBNull(5))
                    key = reader.GetString(5);
                //
                // Get the user data from the serialized column if we can
                // Try the string column if serialized column is null
                // If both are null the user data was null originally
                object userData = null;
                if (!reader.IsDBNull(7))
                    userData = formatter.Deserialize(new MemoryStream((Byte[])reader[7]));
                else if (!reader.IsDBNull(6))
                    userData = reader.GetString(6);

                if (reader.IsDBNull(8) || reader.IsDBNull(9))
                    throw new InvalidOperationException(String.Format(System.Globalization.CultureInfo.CurrentCulture, ExecutionStringManager.SqlTrackingTypeNotFound, qId));

                Type type = Type.GetType(reader.GetString(8) + ", " + reader.GetString(9), true, false);
                long eventId = reader.GetInt64(10);

                DateTime dbDt = reader.GetDateTime(11);

                userEvents.Add(eventId, new UserTrackingRecord(type, qId, context, parentContext, dt, order, key, userData));

                if (dbDt > tmpMin)
                    tmpMin = dbDt;
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);

            //
            // If we have annotations on the event itself, add them
            while (reader.Read())
            {
                long eventId = reader.GetInt64(0);
                string annotation = null;

                if (!reader.IsDBNull(1))
                    annotation = reader.GetString(1);

                UserTrackingRecord user = null;
                if (userEvents.TryGetValue(eventId, out user))
                {
                    if (null != user)
                        user.Annotations.Add(annotation);
                }
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);

            //
            // Build a dictionary of artifact records so that we can match 
            // annotation records from subsequent recordsets
            Dictionary<long, TrackingDataItem> artifacts = new Dictionary<long, TrackingDataItem>();
            while (reader.Read())
            {
                long eventId = reader.GetInt64(0);
                long artId = reader.GetInt64(1);
                string name = reader.GetString(2), strData = null;
                object data = null;
                //
                // These may both be null
                if (!reader.IsDBNull(3))
                    strData = reader.GetString(3);

                if (!reader.IsDBNull(4))
                    data = formatter.Deserialize(new MemoryStream((Byte[])reader[4]));

                TrackingDataItem item = new TrackingDataItem();
                item.FieldName = name;
                if (null != data)
                    item.Data = data;
                else
                    item.Data = strData;

                artifacts.Add(artId, item);
                //
                // Find the event to which this artifact belongs and add it to the record
                UserTrackingRecord user = null;
                if (userEvents.TryGetValue(eventId, out user))
                {
                    if (null != user)
                        user.Body.Add(item);
                }
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidUserEventReader);

            //
            // If we have annotations add them to the appropriate artifact
            while (reader.Read())
            {
                long artId = reader.GetInt64(0);
                string annotation = null;

                if (!reader.IsDBNull(1))
                    annotation = reader.GetString(1);
                //
                // Find the right artifact and give it the annotation
                TrackingDataItem item = null;
                if (artifacts.TryGetValue(artId, out item))
                {
                    if (null != item)
                        item.Annotations.Add(annotation);
                }
            }

            _userEvents.AddRange(userEvents.Values);
            //
            // Set the min dt to query for next time to the most recent event we got for this query.
            // Don't overwrite the previous min if nothing came back for this query
            if (tmpMin > SqlDateTime.MinValue.Value)
                _userMinDT = tmpMin;
            return;
        }

        private void LoadWorkflowEvents()
        {
            SqlCommand cmd = CreateInternalIdDateTimeCommand("[dbo].[GetWorkflowInstanceEventsWithDetails]", _instMinDT);

            ExecuteRetried(cmd, LoadWorkflowEventsFromReader);
        }

        private void LoadWorkflowEventsFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            //
            // There should always be 2 recordsets in this reader!
            //

            DateTime tmpMin = SqlDateTime.MinValue.Value;

            Dictionary<long, WorkflowTrackingRecord> inst = new Dictionary<long, WorkflowTrackingRecord>();
            while (reader.Read())
            {
                TrackingWorkflowEvent evt = (TrackingWorkflowEvent)reader[0];
                DateTime dt = reader.GetDateTime(1);
                int order = reader.GetInt32(2);

                object tmp = null;
                EventArgs args = null;
                if (!reader.IsDBNull(3))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    tmp = formatter.Deserialize(new MemoryStream((Byte[])reader[3]));
                    if (tmp is EventArgs)
                        args = (EventArgs)tmp;
                }
                long eventId = reader.GetInt64(4);

                DateTime dbDt = reader.GetDateTime(5);

                inst.Add(eventId, new WorkflowTrackingRecord(evt, dt, order, args));

                if (dbDt > tmpMin)
                    tmpMin = dbDt;
            }

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowInstanceEventReader);

            //
            // Add any annotations
            while (reader.Read())
            {
                long eventId = reader.GetInt64(0);
                string annotation = null;

                if (!reader.IsDBNull(1))
                    annotation = reader.GetString(1);

                WorkflowTrackingRecord rec = null;
                if (inst.TryGetValue(eventId, out rec))
                {
                    if (null != rec)
                        rec.Annotations.Add(annotation);
                }
            }

            if (!reader.IsClosed)
                reader.Close();
            //
            // Check if we have any WorkflowChange events in this list
            // If so pull back the change actions and reconstruct the args property
            foreach (KeyValuePair<long, WorkflowTrackingRecord> kvp in inst)
            {
                WorkflowTrackingRecord rec = kvp.Value;
                if (TrackingWorkflowEvent.Changed != rec.TrackingWorkflowEvent)
                    continue;

                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[GetWorkflowChangeEventArgs]";
                cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceInternalId", _internalId));
                cmd.Parameters.Add(new SqlParameter("@BeginDateTime", SqlDateTime.MinValue.Value));
                cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceEventId", kvp.Key));

                ExecuteRetried(cmd, LoadWorkflowChangeEventArgsFromReader, rec);
            }

            _workflowEvents.AddRange(inst.Values);
            //
            // set the min for the next query to the most recent event from this query
            // Don't overwrite the previous min if nothing came back for this query
            if (tmpMin > SqlDateTime.MinValue.Value)
                _instMinDT = tmpMin;
        }

        private void LoadWorkflowChangeEventArgsFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (null == parameter)
                throw new ArgumentNullException("parameter");

            WorkflowTrackingRecord record = parameter as WorkflowTrackingRecord;

            if (null == record)
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsParameter, "parameter");

            if (!reader.Read())
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);

            StringReader sr = new StringReader(reader.GetString(0));

            //Deserialize the xoml and set the root activity
            Activity def = null;
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            IList errors = null;
            try
            {
                using (serializationManager.CreateSession())
                {
                    using (XmlReader xmlReader = XmlReader.Create(sr))
                    {
                        def = serializer.Deserialize(serializationManager, xmlReader) as Activity;
                        errors = serializationManager.Errors;
                    }
                }
            }
            finally
            {
                sr.Close();
            }

            if ((null == def) || ((null != errors) && (errors.Count > 0)))
                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);

            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);
            //
            // There is a result set that we don't care about for this scenario, skip it
            if (!reader.NextResult())
                throw new ArgumentException(ExecutionStringManager.InvalidWorkflowChangeEventArgsReader);

            List<WorkflowChangeAction> actions = new List<WorkflowChangeAction>();
            DateTime currDT = DateTime.MinValue;
            int currEventOrder = -1;
            int currOrder = -1;

            while (reader.Read())
            {
                DateTime dt = reader.GetDateTime(1);
                int eventOrder = reader.GetInt32(2);
                int order = reader.GetInt32(3);
                //
                // Build temp lists as we read the results but
                // only save the last set of change actions
                if (dt > currDT && eventOrder > currEventOrder)
                {
                    currEventOrder = eventOrder;
                    currOrder = order;
                    currDT = dt;
                    actions = new List<WorkflowChangeAction>();
                }

                using (sr = new StringReader(reader.GetString(0)))
                {
                    using (serializationManager.CreateSession())
                    {
                        using (XmlReader xmlReader = XmlReader.Create(sr))
                        {
                            ActivityChangeAction aAction = serializer.Deserialize(serializationManager, xmlReader) as ActivityChangeAction;

                            errors = serializationManager.Errors;
                            if (null == aAction)
                                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);

                            actions.Add(aAction);
                            aAction.ApplyTo(def);
                        }
                    }
                }
            }

            record.EventArgs = new TrackingWorkflowChangedEventArgs(actions, def);
        }

        private void LoadDef()
        {
            SqlCommand cmd = null;
            //
            // If we don't have the definition load it
            if (null == _def)
            {
                cmd = new SqlCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[dbo].[GetWorkflowDefinition]";
                cmd.Parameters.Add(new SqlParameter("@WorkflowInstanceInternalId", _internalId));

                ExecuteRetried(cmd, LoadDefFromReader);
            }
            //
            // Now check for changes.  If we find changes apply them to the definition
            cmd = CreateInternalIdDateTimeCommand("[dbo].[GetWorkflowChanges]", _changesMinDT);

            ExecuteRetried(cmd, LoadChangesFromReader);
        }

        private void LoadDefFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            if (!reader.Read())
                throw new ArgumentException(ExecutionStringManager.InvalidDefinitionReader);

            StringReader sr = new StringReader(reader.GetString(0));

            //Deserialize the xoml and set the root activity
            WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            IList errors = null;
            try
            {
                using (serializationManager.CreateSession())
                {
                    using (XmlReader xmlReader = XmlReader.Create(sr))
                    {
                        _def = serializer.Deserialize(serializationManager, xmlReader) as Activity;
                        errors = serializationManager.Errors;
                    }
                }
            }
            finally
            {
                sr.Close();
            }

            if ((null == _def) || ((null != errors) && (errors.Count > 0)))
                throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
        }

        private void LoadChangesFromReader(SqlDataReader reader, object parameter)
        {
            if (!reader.Read())
                return;
            //
            // Reset the min to the most recent change event
            DateTime tmpDT = _changesMinDT;
            if (!reader.IsDBNull(0))
                tmpDT = reader.GetDateTime(0);

            if (reader.NextResult())
            {
                WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                DesignerSerializationManager serializationManager = new DesignerSerializationManager();
                while (reader.Read())
                {
                    IList errors = null;
                    using (StringReader sr = new StringReader(reader.GetString(0)))
                    {
                        using (serializationManager.CreateSession())
                        {
                            using (XmlReader xmlReader = XmlReader.Create(sr))
                            {
                                ActivityChangeAction aAction = serializer.Deserialize(serializationManager, xmlReader) as ActivityChangeAction;

                                errors = serializationManager.Errors;
                                if (null != aAction)
                                    aAction.ApplyTo(_def);
                                else
                                    throw new WorkflowMarkupSerializationException(ExecutionStringManager.WorkflowMarkupDeserializationError);
                            }
                        }
                    }
                }
            }

            if (tmpDT > _changesMinDT)
            {
                _changed = true;
                _changesMinDT = tmpDT;
            }
        }

        private void LoadInvokedWorkflows()
        {
            SqlCommand cmd = new SqlCommand();

            cmd.CommandText = "[dbo].[GetInvokedWorkflows]";
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter param = new SqlParameter("@WorkflowInstanceId", SqlDbType.UniqueIdentifier);
            param.Value = _id;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@BeginDateTime", SqlDbType.DateTime);
            param.Value = _invMinDT;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@EndDateTime", SqlDbType.DateTime);
            param.Value = _currDT;
            cmd.Parameters.Add(param);

            ExecuteRetried(cmd, LoadInvokedWorkflowsFromReader);
        }

        private void LoadInvokedWorkflowsFromReader(SqlDataReader reader, object parameter)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");

            DateTime tmpMin = SqlDateTime.MinValue.Value;
            while (reader.Read())
            {
                SqlTrackingWorkflowInstance inst = SqlTrackingQuery.BuildInstance(reader, _connectionString);

                if (inst.Initialized > tmpMin)
                    tmpMin = inst.Initialized;

                _invoked.Add(inst);
            }
            //
            // set the min for the next query to the most recently invoked instance from this query
            // Don't overwrite the previous min if nothing came back for this query
            if (tmpMin > SqlDateTime.MinValue.Value)
                _invMinDT = tmpMin;
        }

        private void ExecuteRetried(SqlCommand cmd, LoadFromReader loader)
        {
            ExecuteRetried(cmd, loader, null);
        }

        private void ExecuteRetried(SqlCommand cmd, LoadFromReader loader, object loadFromReaderParam)
        {
            SqlDataReader reader = null;
            short count = 0;
            while (true)
            {
                try
                {
                    using (SqlConnection conn = new SqlConnection(_connectionString))
                    {
                        cmd.Connection = conn;
                        cmd.Connection.Open();
                        reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        loader(reader, loadFromReaderParam);
                        break;
                    }
                }
                catch (SqlException se)
                {
                    //
                    // Retry if we deadlocked.
                    // All other exceptions bubble
                    if ((_deadlock == se.Number) && (++count < _retries))
                        continue;

                    throw;
                }
                finally
                {
                    if ((null != reader) && (!reader.IsClosed))
                        reader.Close();
                }
            }
        }

        private SqlCommand CreateInternalIdDateTimeCommand(string commandText, DateTime minDT)
        {
            return CreateInternalIdDateTimeCommand(commandText, minDT, _currDT);
        }

        private SqlCommand CreateInternalIdDateTimeCommand(string commandText, DateTime minDT, DateTime maxDT)
        {
            if (null == commandText)
                throw new ArgumentNullException("commandText");

            SqlCommand cmd = new SqlCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = commandText;
            //
            // Add parameter values for GetActivityEvents
            SqlParameter param = new SqlParameter("@WorkflowInstanceInternalId", SqlDbType.BigInt);
            param.Value = _internalId;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@BeginDateTime", SqlDbType.DateTime);
            param.Value = minDT;
            cmd.Parameters.Add(param);

            param = new SqlParameter("@EndDateTime", SqlDbType.DateTime);
            param.Value = maxDT;
            cmd.Parameters.Add(param);

            return cmd;
        }

    }
}
