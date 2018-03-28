//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel.Diagnostics;
    using System.Text;
    using System.Xml;

    [Obsolete("The WF3 types are deprecated.  Instead, please use the new WF4 types from System.Activities.*")]
    public class SqlPersistenceProviderFactory : PersistenceProviderFactory
    {
        static readonly TimeSpan maxSecondsTimeSpan = TimeSpan.FromSeconds(int.MaxValue);
        const string connectionStringNameParameter = "connectionStringName";
        const string lockTimeoutParameter = "lockTimeout";
        const string serializeAsTextParameter = "serializeAsText";
        List<SqlCommand> activeCommands;
        string canonicalConnectionString;

        string connectionString;
        CreateHandler createHandler;
        DeleteHandler deleteHandler;
        Guid hostId;
        LoadHandler loadHandler;
        TimeSpan lockTimeout;
        bool serializeAsText;
        UnlockHandler unlockHandler;
        UpdateHandler updateHandler;

        public SqlPersistenceProviderFactory(string connectionString)
            : this(connectionString, false, TimeSpan.Zero)
        {
        }

        public SqlPersistenceProviderFactory(string connectionString, bool serializeAsText)
            : this(connectionString, serializeAsText, TimeSpan.Zero)
        {
        }

        public SqlPersistenceProviderFactory(string connectionString, bool serializeAsText, TimeSpan lockTimeout)
        {
            this.ConnectionString = connectionString;
            this.LockTimeout = lockTimeout;
            this.SerializeAsText = serializeAsText;
            this.loadHandler = new LoadHandler(this);
            this.createHandler = new CreateHandler(this);
            this.updateHandler = new UpdateHandler(this);
            this.unlockHandler = new UnlockHandler(this);
            this.deleteHandler = new DeleteHandler(this);
        }

        public SqlPersistenceProviderFactory(NameValueCollection parameters)
        {
            if (parameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("parameters");
            }

            this.connectionString = null;
            this.LockTimeout = TimeSpan.Zero;
            this.SerializeAsText = false;

            foreach (string key in parameters.Keys)
            {
                switch (key)
                {
                    case connectionStringNameParameter:
                        ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[parameters[key]];

                        if (settings == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                                SR2.GetString(SR2.ConnectionStringNameIncorrect, parameters[key]));
                        }

                        this.connectionString = settings.ConnectionString;
                        break;
                    case serializeAsTextParameter:
                        this.SerializeAsText = bool.Parse(parameters[key]);
                        break;
                    case lockTimeoutParameter:
                        this.LockTimeout = TimeSpan.Parse(parameters[key], CultureInfo.InvariantCulture);
                        break;
                    default:
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                            key,
                            SR2.GetString(SR2.UnknownSqlPersistenceConfigurationParameter, key, connectionStringNameParameter, serializeAsTextParameter, lockTimeoutParameter));
                }
            }

            if (this.connectionString == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    SR2.GetString(SR2.ConnectionStringNameParameterRequired, connectionStringNameParameter));
            }

            this.loadHandler = new LoadHandler(this);
            this.createHandler = new CreateHandler(this);
            this.updateHandler = new UpdateHandler(this);
            this.unlockHandler = new UnlockHandler(this);
            this.deleteHandler = new DeleteHandler(this);
        }

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.connectionString = value;
            }
        }

        public TimeSpan LockTimeout
        {
            get
            {
                return this.lockTimeout;
            }
            set
            {
                // Allowed values are TimeSpan.Zero (no locking), TimeSpan.MaxValue (infinite locking),
                // and any values between 1 and int.MaxValue seconds
                if (value < TimeSpan.Zero ||
                    (value > TimeSpan.FromSeconds(int.MaxValue) && value != TimeSpan.MaxValue))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new ArgumentOutOfRangeException(
                        "value",
                        SR2.GetString(SR2.LockTimeoutOutOfRange)));
                }
                this.lockTimeout = value;
            }
        }

        public bool SerializeAsText
        {
            get
            {
                return this.serializeAsText;
            }
            set
            {
                this.serializeAsText = value;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return PersistenceProvider.DefaultOpenClosePersistenceTimout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return PersistenceProvider.DefaultOpenClosePersistenceTimout; }
        }

        bool IsLockingTurnedOn
        {
            get { return this.lockTimeout != TimeSpan.Zero; }
        }

        int LockTimeoutAsInt
        {
            get
            {
                // Consider storing lockTimeout as int32 TotalSeconds instead
                if (this.lockTimeout == TimeSpan.Zero)
                {
                    return -1;
                }
                else if (this.lockTimeout == TimeSpan.MaxValue)
                {
                    return 0;
                }
                else
                {
                    Fx.Assert(this.lockTimeout <= TimeSpan.FromSeconds(int.MaxValue),
                        "The lockTimeout should have been checked in the constructor.");

                    return Convert.ToInt32(this.lockTimeout.TotalSeconds);
                }
            }
        }

        public override PersistenceProvider CreateProvider(Guid id)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (Guid.Empty == id)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("id", SR2.GetString(SR2.SqlPersistenceProviderRequiresNonEmptyGuid));
            }

            return new SqlPersistenceProvider(id, this);
        }

        protected override void OnAbort()
        {
            if (this.activeCommands != null)
            {
                lock (this.activeCommands)
                {
                    foreach (SqlCommand command in this.activeCommands)
                    {
                        command.Cancel();
                    }
                }
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ValidateCommandTimeout(timeout);

            return new CloseAsyncResult(this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ValidateCommandTimeout(timeout);

            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            ValidateCommandTimeout(timeout);

            try
            {
                PerformOpen(timeout);
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new PersistenceException(
                    SR2.GetString(SR2.ErrorOpeningSqlPersistenceProvider),
                    e));
            }
        }

        static int ConvertTimeSpanToSqlTimeout(TimeSpan timeout)
        {
            if (timeout == TimeSpan.MaxValue)
            {
                return 0;
            }
            else
            {
                Fx.Assert(timeout <= TimeSpan.FromSeconds(int.MaxValue),
                    "Timeout should have been validated before entering this method.");

                return Convert.ToInt32(timeout.TotalSeconds);
            }
        }

        IAsyncResult BeginCreate(Guid id, object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ValidateCommandTimeout(timeout);

            return new OperationAsyncResult(this.createHandler, this, id, timeout, callback, state, instance, unlockInstance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801")]
        IAsyncResult BeginDelete(Guid id, object instance, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();

            ValidateCommandTimeout(timeout);

            return new OperationAsyncResult(this.deleteHandler, this, id, timeout, callback, state);
        }

        IAsyncResult BeginLoad(Guid id, TimeSpan timeout, bool lockInstance, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();

            ValidateCommandTimeout(timeout);

            return new OperationAsyncResult(this.loadHandler, this, id, timeout, callback, state, lockInstance);
        }

        IAsyncResult BeginUnlock(Guid id, TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();

            ValidateCommandTimeout(timeout);

            return new OperationAsyncResult(this.unlockHandler, this, id, timeout, callback, state);
        }

        IAsyncResult BeginUpdate(Guid id, object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ValidateCommandTimeout(timeout);

            return new OperationAsyncResult(this.updateHandler, this, id, timeout, callback, state, instance, unlockInstance);
        }

        void CleanupCommand(SqlCommand command)
        {
            lock (this.activeCommands)
            {
                this.activeCommands.Remove(command);
            }

            command.Dispose();
        }

        object Create(Guid id, object instance, TimeSpan timeout, bool unlockInstance)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ValidateCommandTimeout(timeout);

            PerformOperation(this.createHandler, id, timeout, instance, unlockInstance);

            return null;
        }

        SqlCommand CreateCommand(SqlConnection connection, TimeSpan timeout)
        {
            SqlCommand command = connection.CreateCommand();
            command.CommandTimeout = ConvertTimeSpanToSqlTimeout(timeout);

            lock (this.activeCommands)
            {
                this.activeCommands.Add(command);
            }

            return command;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801")]
        void Delete(Guid id, object instance, TimeSpan timeout)
        {
            base.ThrowIfDisposedOrNotOpen();

            ValidateCommandTimeout(timeout);

            PerformOperation(this.deleteHandler, id, timeout);
        }

        object DeserializeInstance(object serializedInstance, bool isText)
        {
            object instance;

            NetDataContractSerializer serializer = new NetDataContractSerializer();
            if (isText)
            {
                StringReader stringReader = new StringReader((string)serializedInstance);
                XmlReader xmlReader = XmlReader.Create(stringReader);

                instance = serializer.ReadObject(xmlReader);

                xmlReader.Close();
                stringReader.Close();
            }
            else
            {
                XmlDictionaryReader dictionaryReader = XmlDictionaryReader.CreateBinaryReader((byte[])serializedInstance, XmlDictionaryReaderQuotas.Max);

                instance = serializer.ReadObject(dictionaryReader);

                dictionaryReader.Close();
            }

            return instance;
        }

        object EndCreate(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            OperationAsyncResult.End(result);

            return null;
        }

        void EndDelete(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            OperationAsyncResult.End(result);
        }

        object EndLoad(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            return OperationAsyncResult.End(result);
        }

        void EndUnlock(IAsyncResult result)
        {
            OperationAsyncResult.End(result);
        }

        object EndUpdate(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }

            OperationAsyncResult.End(result);

            return null;
        }

        byte[] GetBinarySerializedForm(object instance)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream memStr = new MemoryStream();
            XmlDictionaryWriter dictionaryWriter = XmlDictionaryWriter.CreateBinaryWriter(memStr);

            serializer.WriteObject(dictionaryWriter, instance);
            dictionaryWriter.Flush();

            byte[] bytes = memStr.ToArray();

            dictionaryWriter.Close();
            memStr.Close();

            return bytes;
        }

        string GetConnectionString(TimeSpan timeout)
        {
            if (this.canonicalConnectionString != null)
            {
                StringBuilder sb = new StringBuilder(this.canonicalConnectionString);
                sb.Append(ConvertTimeSpanToSqlTimeout(timeout));
                return sb.ToString();
            }

            return this.connectionString;
        }

        string GetXmlSerializedForm(object instance)
        {
            NetDataContractSerializer serializer = new NetDataContractSerializer();
            MemoryStream memStr = new MemoryStream();

            serializer.WriteObject(memStr, instance);

            string xml = UnicodeEncoding.UTF8.GetString(memStr.ToArray());

            memStr.Close();

            return xml;
        }

        object Load(Guid id, TimeSpan timeout, bool lockInstance)
        {
            base.ThrowIfDisposedOrNotOpen();

            ValidateCommandTimeout(timeout);

            return PerformOperation(this.loadHandler, id, timeout, lockInstance);
        }

        SqlConnection OpenConnection(TimeSpan timeout)
        {
            // Do I need to do timeout decrementing?
            SqlConnection connection = new SqlConnection(GetConnectionString(timeout));
            connection.Open();

            return connection;
        }

        void PerformOpen(TimeSpan timeout)
        {
            string lowerCaseConnectionString = this.connectionString.ToUpper(CultureInfo.InvariantCulture);

            if (!lowerCaseConnectionString.Contains("CONNECTION TIMEOUT") &&
                !lowerCaseConnectionString.Contains("CONNECTIONTIMEOUT"))
            {
                this.canonicalConnectionString = this.connectionString.Trim();

                if (this.canonicalConnectionString.EndsWith(";", StringComparison.Ordinal))
                {
                    this.canonicalConnectionString += "Connection Timeout=";
                }
                else
                {
                    this.canonicalConnectionString += ";Connection Timeout=";
                }
            }

            // Check that the connection string is valid
            using (SqlConnection connection = new SqlConnection(GetConnectionString(timeout)))
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    Dictionary<string, string> openParameters = new Dictionary<string, string>(2)
                    {
                        { "IsLocking", this.IsLockingTurnedOn ? "True" : "False" },
                        { "LockTimeout", this.lockTimeout.ToString() }
                    };

                    TraceRecord record = new DictionaryTraceRecord(openParameters);

                    TraceUtility.TraceEvent(TraceEventType.Information,
                        TraceCode.SqlPersistenceProviderOpenParameters, SR.GetString(SR.TraceCodeSqlPersistenceProviderOpenParameters),
                        record, this, null);
                }

                connection.Open();
            }

            this.activeCommands = new List<SqlCommand>();
            this.hostId = Guid.NewGuid();
        }

        object PerformOperation(OperationHandler handler, Guid id, TimeSpan timeout, params object[] additionalParameters)
        {
            int resultCode;
            object returnValue = null;

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR2.GetString(SR2.SqlPrsistenceProviderOperationAndInstanceId, handler.OperationName, id.ToString());
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.SqlPersistenceProviderSQLCallStart, SR.GetString(SR.TraceCodeSqlPersistenceProviderSQLCallStart),
                    new StringTraceRecord("OperationDetail", traceText),
                    this, null);
            }

            try
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);

                using (SqlConnection connection = OpenConnection(timeoutHelper.RemainingTime()))
                {
                    SqlCommand command = CreateCommand(connection, timeoutHelper.RemainingTime());

                    try
                    {
                        handler.SetupCommand(command, id, additionalParameters);

                        if (handler.ExecuteReader)
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                returnValue = handler.ProcessReader(reader);
                            }
                        }
                        else
                        {
                            command.ExecuteNonQuery();
                        }

                        resultCode = (int)command.Parameters["@result"].Value;
                    }
                    finally
                    {
                        CleanupCommand(command);
                    }
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                {
                    throw;
                }

                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new PersistenceException(
                    SR2.GetString(SR2.PersistenceOperationError, handler.OperationName),
                    e));
            }

            Exception toThrow = handler.ProcessResult(resultCode, id, returnValue);

            if (DiagnosticUtility.ShouldTraceInformation)
            {
                string traceText = SR2.GetString(SR2.SqlPrsistenceProviderOperationAndInstanceId, handler.OperationName, id.ToString());
                TraceUtility.TraceEvent(TraceEventType.Information,
                    TraceCode.SqlPersistenceProviderSQLCallEnd, SR.GetString(SR.TraceCodeSqlPersistenceProviderSQLCallEnd),
                    new StringTraceRecord("OperationDetail", traceText),
                    this, null);
            }

            if (toThrow != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(toThrow);
            }

            return returnValue;
        }

        void Unlock(Guid id, TimeSpan timeout)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (this.unlockHandler.ShortcutExecution)
            {
                return;
            }

            ValidateCommandTimeout(timeout);

            PerformOperation(this.unlockHandler, id, timeout);
        }

        object Update(Guid id, object instance, TimeSpan timeout, bool unlockInstance)
        {
            base.ThrowIfDisposedOrNotOpen();

            if (instance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("instance");
            }

            ValidateCommandTimeout(timeout);

            PerformOperation(this.updateHandler, id, timeout, instance, unlockInstance);

            return null;
        }

        void ValidateCommandTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero ||
                (timeout > SqlPersistenceProviderFactory.maxSecondsTimeSpan && timeout != TimeSpan.MaxValue))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(
                    "timeout",
                    SR2.GetString(SR2.CommandTimeoutOutOfRange));
            }
        }

        class CloseAsyncResult : AsyncResult
        {
            public CloseAsyncResult(SqlPersistenceProviderFactory provider, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                // There is no point in even pretending SqlConnection.Close needs async
                provider.OnClose(timeout);

                Complete(true);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<CloseAsyncResult>(result);
            }
        }

        class CreateHandler : OperationHandler
        {
            public CreateHandler(SqlPersistenceProviderFactory provider)
                : base(provider)
            {
            }

            public override string OperationName
            {
                get { return "Create"; }
            }

            public override Exception ProcessResult(int resultCode, Guid id, object loadedInstance)
            {
                switch (resultCode)
                {
                    case 0: // Success
                        return null;
                    case 1: // Already exists
                        return new PersistenceException(SR2.GetString(SR2.InstanceAlreadyExists, id));
                    case 2: // Some other error
                        return new PersistenceException(SR2.GetString(SR2.InsertFailed, id));
                    default:
                        return
                            new PersistenceException(SR2.GetString(SR2.UnknownStoredProcResult));
                }
            }

            public override void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters)
            {
                Fx.Assert(additionalParameters != null && additionalParameters.Length == 2,
                    "Should have had 2 additional parameters.");

                Fx.Assert(additionalParameters[1].GetType() == typeof(bool),
                    "Parameter at index 1 should have been a boolean.");

                object instance = additionalParameters[0];

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "InsertInstance";

                SqlParameter idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                SqlParameter instanceParameter = new SqlParameter("@instance", SqlDbType.Image);
                SqlParameter instanceXmlParameter = new SqlParameter("@instanceXml", SqlDbType.Xml);

                if (this.provider.serializeAsText)
                {
                    instanceXmlParameter.Value = this.provider.GetXmlSerializedForm(instance);
                    instanceParameter.Value = null;
                }
                else
                {
                    instanceParameter.Value = this.provider.GetBinarySerializedForm(instance);
                    instanceXmlParameter.Value = null;
                }

                command.Parameters.Add(instanceParameter);
                command.Parameters.Add(instanceXmlParameter);

                SqlParameter unlockInstanceParameter = new SqlParameter("@unlockInstance", SqlDbType.Bit);
                unlockInstanceParameter.Value = (bool)additionalParameters[1];
                command.Parameters.Add(unlockInstanceParameter);

                SqlParameter lockOwnerParameter = new SqlParameter("@hostId", SqlDbType.UniqueIdentifier);
                lockOwnerParameter.Value = this.provider.hostId;
                command.Parameters.Add(lockOwnerParameter);

                SqlParameter lockTimeoutParameter = new SqlParameter("@lockTimeout", SqlDbType.Int);
                lockTimeoutParameter.Value = this.provider.LockTimeoutAsInt;
                command.Parameters.Add(lockTimeoutParameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Int);
                resultParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(resultParameter);
            }
        }

        class DeleteHandler : OperationHandler
        {
            public DeleteHandler(SqlPersistenceProviderFactory provider)
                : base(provider)
            {
            }

            public override string OperationName
            {
                get { return "Delete"; }
            }

            public override Exception ProcessResult(int resultCode, Guid id, object loadedInstance)
            {
                switch (resultCode)
                {
                    case 0: // Success
                        return null;
                    case 1: // Instance not found
                        return
                            new InstanceNotFoundException(id);
                    case 2: // Could not acquire lock
                        return new InstanceLockException(id, SR2.GetString(SR2.DidNotOwnLock, id, OperationName));
                    default:
                        return
                            new PersistenceException(
                            SR2.GetString(SR2.UnknownStoredProcResult));
                }
            }

            public override void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters)
            {
                Fx.Assert(additionalParameters == null || additionalParameters.Length == 0,
                    "Should not have gotten any additional parameters.");

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "DeleteInstance";

                SqlParameter idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                SqlParameter hostIdParameter = new SqlParameter("@hostId", SqlDbType.UniqueIdentifier);
                hostIdParameter.Value = this.provider.hostId;
                command.Parameters.Add(hostIdParameter);

                SqlParameter lockTimeoutParameter = new SqlParameter("@lockTimeout", SqlDbType.Int);
                lockTimeoutParameter.Value = this.provider.LockTimeoutAsInt;
                command.Parameters.Add(lockTimeoutParameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Int);
                resultParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(resultParameter);
            }
        }

        class LoadHandler : OperationHandler
        {
            public LoadHandler(SqlPersistenceProviderFactory provider)
                : base(provider)
            {
            }

            public override bool ExecuteReader
            {
                get { return true; }
            }

            public override string OperationName
            {
                get { return "Load"; }
            }

            public override object ProcessReader(SqlDataReader reader)
            {
                if (reader.Read())
                {
                    bool isXml = ((int)reader["isXml"] == 0 ? false : true);
                    object serializedInstance;

                    if (isXml)
                    {
                        serializedInstance = reader["instanceXml"];
                    }
                    else
                    {
                        serializedInstance = reader["instance"];
                    }

                    if (serializedInstance != null)
                    {
                        return this.provider.DeserializeInstance(serializedInstance, isXml);
                    }
                }

                return null;
            }

            public override Exception ProcessResult(int resultCode, Guid id, object loadedInstance)
            {
                Exception toReturn = null;

                switch (resultCode)
                {
                    case 0: // Success
                        break;
                    case 1: // Instance not found
                        toReturn = new InstanceNotFoundException(id);
                        break;
                    case 2: // Could not acquire lock
                        toReturn = new InstanceLockException(id);
                        break;
                    default:
                        toReturn =
                            new PersistenceException(SR2.GetString(SR2.UnknownStoredProcResult));
                        break;
                }

                if (toReturn == null)
                {
                    if (loadedInstance == null)
                    {
                        toReturn = new PersistenceException(SR2.GetString(SR2.SerializationFormatMismatch));
                    }
                }

                return toReturn;
            }

            public override void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters)
            {
                Fx.Assert(additionalParameters != null && additionalParameters.Length == 1,
                    "Should have had 1 additional parameter.");

                Fx.Assert(additionalParameters[0].GetType() == typeof(bool),
                    "Parameter 0 should have been a boolean.");

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "LoadInstance";

                SqlParameter idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                SqlParameter lockInstanceParameter = new SqlParameter("@lockInstance", SqlDbType.Bit);
                lockInstanceParameter.Value = (bool)additionalParameters[0];
                command.Parameters.Add(lockInstanceParameter);

                SqlParameter hostIdParameter = new SqlParameter("@hostId", SqlDbType.UniqueIdentifier);
                hostIdParameter.Value = this.provider.hostId;
                command.Parameters.Add(hostIdParameter);

                SqlParameter lockTimeoutParameter = new SqlParameter("@lockTimeout", SqlDbType.Int);
                lockTimeoutParameter.Value = this.provider.LockTimeoutAsInt;
                command.Parameters.Add(lockTimeoutParameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Int);
                resultParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(resultParameter);
            }
        }

        class OpenAsyncResult : AsyncResult
        {
            SqlPersistenceProviderFactory provider;
            TimeSpan timeout;

            public OpenAsyncResult(SqlPersistenceProviderFactory provider, TimeSpan timeout, AsyncCallback callback, object state)
                : base(callback, state)
            {
                Fx.Assert(provider != null,
                    "Provider should never be null.");

                this.provider = provider;
                this.timeout = timeout;

                ActionItem.Schedule(ScheduledCallback, null);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<OpenAsyncResult>(result);
            }

            void ScheduledCallback(object state)
            {
                Exception completionException = null;

                try
                {
                    this.provider.PerformOpen(this.timeout);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException =
                        new PersistenceException(
                        SR2.GetString(SR2.ErrorOpeningSqlPersistenceProvider),
                        e);
                }

                Complete(false, completionException);
            }
        }

        class OperationAsyncResult : AsyncResult
        {
            protected SqlPersistenceProviderFactory provider;
            static AsyncCallback commandCallback = Fx.ThunkCallback(new AsyncCallback(CommandExecutionComplete));

            SqlCommand command;
            OperationHandler handler;
            Guid id;

            object instance;

            // We are using virtual methods from the constructor on purpose
            [SuppressMessage("Microsoft.Usage", "CA2214")]
            public OperationAsyncResult(OperationHandler handler, SqlPersistenceProviderFactory provider, Guid id, TimeSpan timeout, AsyncCallback callback, object state, params object[] additionalParameters)
                : base(callback, state)
            {
                Fx.Assert(provider != null,
                    "Provider should never be null.");

                this.handler = handler;
                this.provider = provider;
                this.id = id;

                if (this.handler.ShortcutExecution)
                {
                    Complete(true);
                    return;
                }

                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                SqlConnection connection = this.provider.OpenConnection(timeoutHelper.RemainingTime());

                bool completeSelf = false;
                Exception delayedException = null;
                try
                {
                    this.command = this.provider.CreateCommand(connection, timeoutHelper.RemainingTime());

                    this.handler.SetupCommand(this.command, this.id, additionalParameters);

                    IAsyncResult result = null;

                    if (this.handler.ExecuteReader)
                    {
                        result = this.command.BeginExecuteReader(commandCallback, this);
                    }
                    else
                    {
                        result = this.command.BeginExecuteNonQuery(commandCallback, this);
                    }

                    if (result.CompletedSynchronously)
                    {
                        delayedException = CompleteOperation(result);

                        completeSelf = true;
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    try
                    {
                        connection.Close();
                        this.provider.CleanupCommand(this.command);
                    }
                    catch (Exception e1)
                    {
                        if (Fx.IsFatal(e1))
                        {
                            throw;
                        }
                        // do not rethrow non-fatal exceptions thrown from cleanup code
                    }
                    finally
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new PersistenceException(
                            SR2.GetString(SR2.PersistenceOperationError, this.handler.OperationName), e));
                    }
                }

                if (completeSelf)
                {
                    connection.Close();
                    this.provider.CleanupCommand(this.command);

                    if (delayedException != null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(delayedException);
                    }

                    Complete(true);
                }
            }

            public object Instance
            {
                get { return this.instance; }
            }

            public static object End(IAsyncResult result)
            {
                OperationAsyncResult operationResult = AsyncResult.End<OperationAsyncResult>(result);

                return operationResult.Instance;
            }

            static void CommandExecutionComplete(IAsyncResult result)
            {
                if (result.CompletedSynchronously)
                {
                    return;
                }

                OperationAsyncResult operationResult = (OperationAsyncResult)result.AsyncState;

                Exception completionException = null;
                try
                {
                    completionException = operationResult.CompleteOperation(result);
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }

                    completionException =
                        new PersistenceException(
                        SR2.GetString(SR2.PersistenceOperationError, operationResult.handler.OperationName), e);
                }
                finally
                {
                    try
                    {
                        operationResult.command.Connection.Close();
                        operationResult.provider.CleanupCommand(operationResult.command);
                    }
                    catch (Exception e1)
                    {
                        if (Fx.IsFatal(e1))
                        {
                            throw;
                        }
                        // do not rethrow non-fatal exceptions thrown from cleanup code
                    }
                }

                operationResult.Complete(false, completionException);
            }

            Exception CompleteOperation(IAsyncResult result)
            {
                Exception delayedException = null;

                if (this.handler.ExecuteReader)
                {
                    using (SqlDataReader reader = this.command.EndExecuteReader(result))
                    {
                        this.instance = this.handler.ProcessReader(reader);
                    }
                }
                else
                {
                    this.command.EndExecuteNonQuery(result);
                }

                int resultCode = (int)this.command.Parameters["@result"].Value;

                delayedException = this.handler.ProcessResult(resultCode, this.id, this.instance);

                return delayedException;
            }
        }

        abstract class OperationHandler
        {
            protected SqlPersistenceProviderFactory provider;

            public OperationHandler(SqlPersistenceProviderFactory provider)
            {
                this.provider = provider;
            }

            public virtual bool ExecuteReader
            {
                get { return false; }
            }

            public abstract string OperationName
            { get; }

            public virtual bool ShortcutExecution
            {
                get { return false; }
            }


            public virtual object ProcessReader(SqlDataReader reader)
            {
                return null;
            }

            public abstract Exception ProcessResult(int resultCode, Guid id, object loadedInstance);

            public abstract void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters);
        }

        class SqlPersistenceProvider : LockingPersistenceProvider
        {
            SqlPersistenceProviderFactory factory;

            public SqlPersistenceProvider(Guid id, SqlPersistenceProviderFactory factory)
                : base(id)
            {
                this.factory = factory;
            }

            protected override TimeSpan DefaultCloseTimeout
            {
                get { return TimeSpan.FromSeconds(15); }
            }

            protected override TimeSpan DefaultOpenTimeout
            {
                get { return TimeSpan.FromSeconds(15); }
            }

            public override IAsyncResult BeginCreate(object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.BeginCreate(this.Id, instance, timeout, unlockInstance, callback, state);
            }

            public override IAsyncResult BeginDelete(object instance, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.BeginDelete(this.Id, instance, timeout, callback, state);
            }

            public override IAsyncResult BeginLoad(TimeSpan timeout, bool lockInstance, AsyncCallback callback, object state)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.BeginLoad(this.Id, timeout, lockInstance, callback, state);
            }

            public override IAsyncResult BeginUnlock(TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.BeginUnlock(this.Id, timeout, callback, state);
            }

            public override IAsyncResult BeginUpdate(object instance, TimeSpan timeout, bool unlockInstance, AsyncCallback callback, object state)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.BeginUpdate(this.Id, instance, timeout, unlockInstance, callback, state);
            }

            public override object Create(object instance, TimeSpan timeout, bool unlockInstance)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.Create(this.Id, instance, timeout, unlockInstance);
            }

            public override void Delete(object instance, TimeSpan timeout)
            {
                base.ThrowIfDisposedOrNotOpen();
                this.factory.Delete(this.Id, instance, timeout);
            }

            public override object EndCreate(IAsyncResult result)
            {
                return this.factory.EndCreate(result);
            }

            public override void EndDelete(IAsyncResult result)
            {
                this.factory.EndDelete(result);
            }

            public override object EndLoad(IAsyncResult result)
            {
                return this.factory.EndLoad(result);
            }

            public override void EndUnlock(IAsyncResult result)
            {
                this.factory.EndUnlock(result);
            }

            public override object EndUpdate(IAsyncResult result)
            {
                return this.factory.EndUpdate(result);
            }

            public override object Load(TimeSpan timeout, bool lockInstance)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.Load(this.Id, timeout, lockInstance);
            }

            public override void Unlock(TimeSpan timeout)
            {
                base.ThrowIfDisposedOrNotOpen();
                this.factory.Unlock(this.Id, timeout);
            }

            public override object Update(object instance, TimeSpan timeout, bool unlockInstance)
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.factory.Update(this.Id, instance, timeout, unlockInstance);
            }

            protected override void OnAbort()
            {
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }
        }

        class UnlockHandler : OperationHandler
        {
            public UnlockHandler(SqlPersistenceProviderFactory provider)
                : base(provider)
            {
            }

            public override string OperationName
            {
                get { return "Unlock"; }
            }

            public override bool ShortcutExecution
            {
                get { return !this.provider.IsLockingTurnedOn; }
            }

            public override Exception ProcessResult(int resultCode, Guid id, object loadedInstance)
            {
                switch (resultCode)
                {
                    case 0: // Success
                        return null;
                    case 1: // Instance not found
                        return
                            new InstanceNotFoundException(id);
                    case 2: // Could not acquire lock
                        return new InstanceLockException(id, SR2.GetString(SR2.DidNotOwnLock, id, OperationName));
                    default:
                        return
                            new PersistenceException(SR2.GetString(SR2.UnknownStoredProcResult));
                }
            }

            public override void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters)
            {
                Fx.Assert(additionalParameters == null || additionalParameters.Length == 0,
                    "There should not be any additional parameters.");

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "UnlockInstance";

                SqlParameter idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                SqlParameter hostIdParameter = new SqlParameter("@hostId", SqlDbType.UniqueIdentifier);
                hostIdParameter.Value = this.provider.hostId;
                command.Parameters.Add(hostIdParameter);

                SqlParameter lockTimeoutParameter = new SqlParameter("@lockTimeout", SqlDbType.Int);
                lockTimeoutParameter.Value = this.provider.LockTimeoutAsInt;
                command.Parameters.Add(lockTimeoutParameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Int);
                resultParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(resultParameter);
            }
        }

        class UpdateHandler : OperationHandler
        {
            public UpdateHandler(SqlPersistenceProviderFactory provider)
                : base(provider)
            {
            }

            public override string OperationName
            {
                get { return "Update"; }
            }

            public override Exception ProcessResult(int resultCode, Guid id, object loadedInstance)
            {
                switch (resultCode)
                {
                    case 0: // Success
                        return null;
                    case 1: // Instance did not exist
                        return new InstanceNotFoundException(id, SR2.GetString(SR2.InstanceNotFoundForUpdate, id));
                    case 2: // Did not have lock
                        return new InstanceLockException(id, SR2.GetString(SR2.DidNotOwnLock, id, OperationName));
                    default:
                        return
                            new PersistenceException(SR2.GetString(SR2.UnknownStoredProcResult));
                }
            }

            public override void SetupCommand(SqlCommand command, Guid id, params object[] additionalParameters)
            {
                Fx.Assert(additionalParameters != null && additionalParameters.Length == 2,
                    "Should have had 2 additional parameters.");

                Fx.Assert(additionalParameters[1].GetType() == typeof(bool),
                    "Parameter at index 1 should have been a boolean.");

                object instance = additionalParameters[0];

                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "UpdateInstance";

                SqlParameter idParameter = new SqlParameter("@id", SqlDbType.UniqueIdentifier);
                idParameter.Value = id;
                command.Parameters.Add(idParameter);

                SqlParameter instanceParameter = new SqlParameter("@instance", SqlDbType.Image);
                SqlParameter instanceXmlParameter = new SqlParameter("@instanceXml", SqlDbType.Xml);

                if (this.provider.serializeAsText)
                {
                    instanceXmlParameter.Value = this.provider.GetXmlSerializedForm(instance);
                    instanceParameter.Value = null;
                }
                else
                {
                    instanceParameter.Value = this.provider.GetBinarySerializedForm(instance);
                    instanceXmlParameter.Value = null;
                }

                command.Parameters.Add(instanceParameter);
                command.Parameters.Add(instanceXmlParameter);

                SqlParameter unlockInstanceParameter = new SqlParameter("@unlockInstance", SqlDbType.Bit);
                unlockInstanceParameter.Value = (bool)additionalParameters[1];
                command.Parameters.Add(unlockInstanceParameter);

                SqlParameter lockOwnerParameter = new SqlParameter("@hostId", SqlDbType.UniqueIdentifier);
                lockOwnerParameter.Value = this.provider.hostId;
                command.Parameters.Add(lockOwnerParameter);

                SqlParameter lockTimeoutParameter = new SqlParameter("@lockTimeout", SqlDbType.Int);
                lockTimeoutParameter.Value = this.provider.LockTimeoutAsInt;
                command.Parameters.Add(lockTimeoutParameter);

                SqlParameter resultParameter = new SqlParameter("@result", SqlDbType.Int);
                resultParameter.Direction = ParameterDirection.Output;
                command.Parameters.Add(resultParameter);
            }
        }
    }
}
