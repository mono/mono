//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DurableInstancing
{
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.Text;
    using System.Xml.Linq;

    static class StoreUtilities
    {
        public static readonly Version Version40 = new Version(4, 0, 0, 0);
        public static readonly Version Version45 = new Version(4, 5, 0, 0);

        public static Exception CheckRemainingResultSetForErrors(XName commandName, SqlDataReader reader)
        {
            Exception returnException = null;

            do
            {
                returnException = StoreUtilities.GetNextResultSet(commandName, reader);
            }
            while (returnException == null && reader.NextResult());

            return returnException;
        }

        public static Exception CheckResult(XName commandName, SqlDataReader reader)
        {
            Exception returnValue = null;

            CommandResult result = (CommandResult) reader.GetInt32(0);
            if (result != CommandResult.Success)
            {
                returnValue = StoreUtilities.GetError(commandName, result, reader);
            }

            return returnValue;
        }

        public static SqlConnection CreateConnection(string connectionString)
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Open();
            return connection;
        }

        public static Exception GetError(XName commandName, CommandResult result, SqlDataReader reader)
        {
            Exception returnValue = null;

            if (result != CommandResult.Success)
            {
                switch (result)
                {
                    case CommandResult.InstanceAlreadyExists:
                        returnValue = new InstanceCollisionException(commandName, reader.GetGuid(1));
                        break;
                    case CommandResult.InstanceLockNotAcquired:
                        returnValue = new InstanceLockedException(commandName, reader.GetGuid(1), reader.GetGuid(2), ReadLockOwnerMetadata(reader));
                        break;
                    case CommandResult.InstanceNotFound:
                        returnValue = new InstanceNotReadyException(commandName, reader.GetGuid(1));
                        break;
                    case CommandResult.KeyAlreadyExists:
                        returnValue = new InstanceKeyCollisionException(commandName, Guid.Empty,
                            new InstanceKey(reader.GetGuid(1)), Guid.Empty);
                        break;
                    case CommandResult.KeyNotFound:
                        returnValue = new InstanceKeyNotReadyException(commandName, new InstanceKey(reader.GetGuid(1)));
                        break;
                    case CommandResult.InstanceLockLost:
                        returnValue = new InstanceLockLostException(commandName, reader.GetGuid(1));
                        break;
                    case CommandResult.InstanceCompleted:
                        returnValue = new InstanceCompleteException(commandName, reader.GetGuid(1));
                        break;
                    case CommandResult.KeyDisassociated:
                        returnValue = new InstanceKeyCompleteException(commandName, new InstanceKey(reader.GetGuid(1)));
                        break;
                    case CommandResult.StaleInstanceVersion:
                        returnValue = new InstanceLockLostException(commandName, reader.GetGuid(1));
                        break;
                    case CommandResult.HostLockExpired:
                        returnValue = new InstancePersistenceException(SR.HostLockExpired);
                        break;
                    case CommandResult.HostLockNotFound:
                        returnValue = new InstancePersistenceException(SR.HostLockNotFound);
                        break;
                    case CommandResult.CleanupInProgress:
                        returnValue = new InstancePersistenceCommandException(SR.CleanupInProgress);
                        break;
                    case CommandResult.InstanceAlreadyLockedToOwner:
                        returnValue = new InstanceAlreadyLockedToOwnerException(commandName, reader.GetGuid(1), reader.GetInt64(2));
                        break;
                    default:
                        returnValue = new InstancePersistenceCommandException(SR.UnknownSprocResult(result));
                        break;
                }
            }

            return returnValue;
        }

        public static Exception GetNextResultSet(XName commandName, SqlDataReader reader)
        {
            do
            {
                if (reader.Read())
                {
                    do
                    {
                        if (reader.FieldCount == 0)
                        {
                            continue;
                        }

                        string columnName = reader.GetName(0);

                        if (string.Compare("Result", columnName, StringComparison.Ordinal) == 0)
                        {
                            return StoreUtilities.CheckResult(commandName, reader);
                        }
                    }
                    while (reader.Read());
                }
            }
            while (reader.NextResult());

            return null;
        }

        public static void TraceSqlCommand(SqlCommand command, bool isStarting)
        {
            if (((isStarting && TD.StartSqlCommandExecuteIsEnabled()) ||
                (!isStarting && TD.EndSqlCommandExecuteIsEnabled())) && command != null)
            {
                StringBuilder traceString = new StringBuilder(SqlWorkflowInstanceStoreConstants.DefaultStringBuilderCapacity);
                bool firstItem = false;

                foreach (SqlParameter sqlParameter in command.Parameters)
                {
                    string value;
                    if ((sqlParameter.Value == DBNull.Value) || (sqlParameter.Value == null))
                    {
                        value = "Null";
                    }
                    else if (sqlParameter.DbType == System.Data.DbType.Binary)
                    {
                        value = "Binary";
                    }
                    else
                    {
                        value = sqlParameter.Value.ToString();
                    }

                    if (firstItem)
                    {
                        traceString.AppendFormat(CultureInfo.InvariantCulture, "{0}='{1}'", sqlParameter.ParameterName, value);
                        firstItem = false;
                    }
                    else
                    {
                        traceString.AppendFormat(CultureInfo.InvariantCulture, ", {0}='{1}'", sqlParameter.ParameterName, value);
                    }

                    traceString.AppendLine(command.CommandText);
                }

                if (isStarting)
                {
                    TD.StartSqlCommandExecute(traceString.ToString());
                }
                else
                {
                    TD.EndSqlCommandExecute(traceString.ToString());
                }
            }
        }

        static Dictionary<XName, object> ReadLockOwnerMetadata(SqlDataReader reader)
        {
            Dictionary<XName, object> lockOwnerProperties = new Dictionary<XName, object>();
            InstanceEncodingOption encodingOption = (InstanceEncodingOption)(reader.GetByte(3));
            byte[] serializedPrimitiveLockOwnerData = reader.IsDBNull(4) ? null : (byte[]) reader.GetValue(4);
            byte[] serializedComplexLockOwnerData = reader.IsDBNull(5) ? null : (byte[]) reader.GetValue(5);
            IObjectSerializer serializer = ObjectSerializerFactory.GetObjectSerializer(encodingOption);
            Dictionary<XName, object>[] lockOwnerPropertyBags = new Dictionary<XName, object>[2];

            if (serializedPrimitiveLockOwnerData != null)
            {
                lockOwnerPropertyBags[0] = (Dictionary<XName, object>)serializer.DeserializeValue(serializedPrimitiveLockOwnerData);
            }

            if (serializedComplexLockOwnerData != null)
            {
                lockOwnerPropertyBags[1] = serializer.DeserializePropertyBag(serializedComplexLockOwnerData);
            }

            foreach (Dictionary<XName, object> propertyBag in lockOwnerPropertyBags)
            {
                if (propertyBag != null)
                {
                    foreach (KeyValuePair<XName, object> property in propertyBag)
                    {
                        lockOwnerProperties.Add(property.Key, property.Value);
                    }
                }
            }

            return lockOwnerProperties;
        }
    }
}
