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
    public sealed class SqlTrackingQuery
    {
        string _connectionString = null;

        public SqlTrackingQuery()
        {
        }

        public SqlTrackingQuery(string connectionString)
        {
            if (null == connectionString)
                throw new ArgumentNullException("connectionString");
            _connectionString = connectionString;
        }

        public string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                _connectionString = value;
            }
        }

        public bool TryGetWorkflow(Guid workflowInstanceId, out SqlTrackingWorkflowInstance workflowInstance)
        {
            SqlCommand cmd = BuildCommand(workflowInstanceId);
            SqlDataReader reader = null;
            workflowInstance = null;

            try
            {

                cmd.Connection = GetConnection();
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //
                // There will only be 1 row
                if (reader.Read())
                {
                    workflowInstance = BuildInstance(reader);
                    return true;
                }
                else
                    return false;
            }
            finally
            {
                if (null != reader)
                    reader.Close();

                if (null != cmd && null != cmd.Connection && ConnectionState.Closed != cmd.Connection.State)
                    cmd.Connection.Close();
            }
        }

        public IList<SqlTrackingWorkflowInstance> GetWorkflows(SqlTrackingQueryOptions options)
        {
            if (null == options)
                throw new ArgumentNullException("options");

            if (null != options.TrackingDataItems)
            {
                foreach (TrackingDataItemValue val in options.TrackingDataItems)
                {
                    if (null == val.QualifiedName)
                        throw new ArgumentNullException("options.TrackingDataItems.QualifiedName");
                    if (null == val.FieldName)
                        throw new ArgumentNullException("options.TrackingDataItems.FieldName");
                }
            }

            SqlCommand cmd = BuildCommand(options);
            SqlDataReader reader = null;
            List<SqlTrackingWorkflowInstance> inst = new List<SqlTrackingWorkflowInstance>();

            try
            {
                cmd.Connection = GetConnection();
                reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                //
                // There will only be 1 row
                while (reader.Read())
                {
                    inst.Add(BuildInstance(reader));
                }
            }
            finally
            {
                if (null != reader)
                    reader.Close();

                if (null != cmd && null != cmd.Connection && ConnectionState.Closed != cmd.Connection.State)
                    cmd.Connection.Close();
            }

            return inst;
        }

        private SqlTrackingWorkflowInstance BuildInstance(SqlDataReader reader)
        {
            return SqlTrackingQuery.BuildInstance(reader, _connectionString);
        }

        internal static SqlTrackingWorkflowInstance BuildInstance(SqlDataReader reader, string connectionString)
        {
            if (null == reader)
                throw new ArgumentNullException("reader");
            if (reader.IsClosed)
                throw new ArgumentException(ExecutionStringManager.InvalidSqlDataReader, "reader");

            SqlTrackingWorkflowInstance inst = new SqlTrackingWorkflowInstance(connectionString);
            inst.WorkflowInstanceId = reader.GetGuid(1);
            inst.WorkflowInstanceInternalId = reader.GetInt64(2);
            inst.Initialized = reader.GetDateTime(3);
            if (DBNull.Value == reader[4])
                inst.InvokingWorkflowInstanceId = Guid.Empty;
            else
                inst.InvokingWorkflowInstanceId = reader.GetGuid(4);
            inst.Status = (WorkflowStatus)reader.GetInt32(5);
            //
            // Xaml only workflows do not have types
            if (!reader.IsDBNull(6))
            {
                string fullName = reader.GetString(6), assemblyName = reader.GetString(7);
                inst.WorkflowType = Type.GetType(fullName + ", " + assemblyName, true, false);
            }

            return inst;
        }

        private SqlConnection GetConnection()
        {
            if (null == _connectionString)
            {
                throw new InvalidOperationException(ExecutionStringManager.MissingConnectionString);
            }

            SqlConnection conn = new SqlConnection(_connectionString);
            conn.Open();

            return conn;
        }

        private SqlCommand BuildCommand(Guid workflowInstanceId)
        {
            SqlCommand cmd = new SqlCommand("[dbo].[GetWorkflows]");
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter param = new SqlParameter();
            param.ParameterName = "@WorkflowInstanceId";
            param.SqlDbType = SqlDbType.UniqueIdentifier;
            param.Value = workflowInstanceId;
            cmd.Parameters.Add(param);

            return cmd;
        }

        private SqlCommand BuildCommand(SqlTrackingQueryOptions opt)
        {
            SqlCommand cmd = new SqlCommand("[dbo].[GetWorkflows]");
            cmd.CommandType = CommandType.StoredProcedure;

            SqlParameter param = new SqlParameter();
            if (opt.WorkflowStatus.HasValue)
            {
                param.ParameterName = "@WorkflowStatusId";
                param.SqlDbType = SqlDbType.TinyInt;
                param.Value = opt.WorkflowStatus.Value;
                cmd.Parameters.Add(param);
                //
                // If one of the range values is set we have a date range constraint
                if (DateTime.MinValue != opt.StatusMinDateTime || DateTime.MaxValue != opt.StatusMaxDateTime)
                {
                    param = new SqlParameter();
                    param.ParameterName = "@StatusMinDateTime";
                    param.SqlDbType = SqlDbType.DateTime;
                    if (opt.StatusMinDateTime < SqlDateTime.MinValue.Value)
                        param.Value = SqlDateTime.MinValue.Value;
                    else
                        param.Value = opt.StatusMinDateTime;

                    cmd.Parameters.Add(param);

                    param = new SqlParameter();
                    param.ParameterName = "@StatusMaxDateTime";
                    param.SqlDbType = SqlDbType.DateTime;
                    if (opt.StatusMaxDateTime > SqlDateTime.MaxValue.Value)
                        param.Value = SqlDateTime.MaxValue.Value;
                    else
                        param.Value = opt.StatusMaxDateTime;

                    cmd.Parameters.Add(param);
                }
            }

            if (null != opt.WorkflowType)
            {
                param = new SqlParameter("@TypeFullName", opt.WorkflowType.FullName);
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 128;
                cmd.Parameters.Add(param);

                param = new SqlParameter("@AssemblyFullName", opt.WorkflowType.Assembly.FullName);
                param.SqlDbType = SqlDbType.NVarChar;
                param.Size = 128;
                cmd.Parameters.Add(param);
            }

            if (null != opt.TrackingDataItems && opt.TrackingDataItems.Count > 0)
                BuildArtifactParameters(cmd, opt.TrackingDataItems);

            return cmd;
        }

        private void BuildArtifactParameters(SqlCommand cmd, IList<TrackingDataItemValue> artifacts)
        {
            if (null == artifacts || 0 == artifacts.Count)
                return;

            StringBuilder sb = new StringBuilder();
            XmlWriter writer = XmlWriter.Create(sb);

            try
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("TrackingDataItems");

                foreach (TrackingDataItemValue art in artifacts)
                {
                    writer.WriteStartElement("TrackingDataItem");
                    writer.WriteElementString("QualifiedName", art.QualifiedName);
                    writer.WriteElementString("FieldName", art.FieldName);
                    //
                    // If data value is null don't write the node as 
                    // the proc sees no DataValue node as null and matches null rows.
                    // This allows us to match null, "", and positive length strings
                    if (null != art.DataValue)
                        writer.WriteElementString("DataValue", art.DataValue);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
            finally
            {
                writer.Flush();
                writer.Close();
            }

            SqlParameter param = new SqlParameter();
            param.ParameterName = "@TrackingDataItems";
            param.SqlDbType = SqlDbType.NText;
            param.Value = sb.ToString();

            cmd.Parameters.Add(param);
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class SqlTrackingQueryOptions
    {
        private DateTime _min = DateTime.MinValue, _max = DateTime.MaxValue;
        private WorkflowStatus? _status = new Nullable<WorkflowStatus>();
        private Type _type = null;
        private List<TrackingDataItemValue> _dataItems = new List<TrackingDataItemValue>();

        public Type WorkflowType
        {
            get { return _type; }
            set { _type = value; }
        }
        public WorkflowStatus? WorkflowStatus
        {
            get { return _status; }
            set { _status = value; }
        }
        public DateTime StatusMinDateTime
        {
            get { return _min; }
            set { _min = value; }
        }
        public DateTime StatusMaxDateTime
        {
            get { return _max; }
            set { _max = value; }
        }
        public IList<TrackingDataItemValue> TrackingDataItems
        {
            get { return _dataItems; }
        }

        public void Clear()
        {
            _min = DateTime.MinValue;
            _max = DateTime.MaxValue;
            _status = new Nullable<WorkflowStatus>();
            _type = null;
            _dataItems = new List<TrackingDataItemValue>();
        }
    }
}
