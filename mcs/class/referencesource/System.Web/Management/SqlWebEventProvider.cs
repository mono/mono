//------------------------------------------------------------------------------
// <copyright file="events.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration.Provider;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Data;
    using System.Data.SqlClient;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Threading;
    using System.Web.DataAccess;
    using System.Web.Util;

    ////////////
    // Events
    ////////////

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
    public class SqlWebEventProvider : BufferedWebEventProvider, IInternalWebEventProvider {
        const int       SQL_MAX_NTEXT_SIZE = 1073741823;
        const int       NO_LIMIT = -1;
        const string    SP_LOG_EVENT = "dbo.aspnet_WebEvent_LogEvent";

        string          _sqlConnectionString;
        int             _maxEventDetailsLength = NO_LIMIT;
        int             _commandTimeout = -1;
        int             _SchemaVersionCheck;
        int             _connectionCount = 0;

        DateTime        _retryDate = DateTime.MinValue; // Won't try sending unless DateTime.UtcNow is > _retryDate

        protected internal SqlWebEventProvider() { }

        public override void Initialize(string name, NameValueCollection config) {

            Debug.Trace("SqlWebEventProvider", "Initializing: name=" + name);
            _SchemaVersionCheck = 0;
            string  temp = null;

            ProviderUtil.GetAndRemoveStringAttribute(config, "connectionStringName", name, ref temp);
            ProviderUtil.GetAndRemoveStringAttribute(config, "connectionString", name, ref _sqlConnectionString);
            if (!String.IsNullOrEmpty(temp)) {
                if (!String.IsNullOrEmpty(_sqlConnectionString)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Only_one_connection_string_allowed));
                }

                _sqlConnectionString = SqlConnectionHelper.GetConnectionString(temp, true, true);
                if (_sqlConnectionString == null || _sqlConnectionString.Length < 1) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Connection_string_not_found, temp));
                }
            }
            else {
                // If a connection string is specified explicitly, verify that its not using integrated security
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(_sqlConnectionString);
                if (builder.IntegratedSecurity) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Cannot_use_integrated_security));
                }
            }

            if (String.IsNullOrEmpty(_sqlConnectionString)) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Must_specify_connection_string_or_name, temp));
            }


            ProviderUtil.GetAndRemovePositiveOrInfiniteAttribute(config, "maxEventDetailsLength", name, ref _maxEventDetailsLength);
            if (_maxEventDetailsLength == ProviderUtil.Infinite) {
                _maxEventDetailsLength = NO_LIMIT;
            }
            else if (_maxEventDetailsLength > SQL_MAX_NTEXT_SIZE) {
                throw new ConfigurationErrorsException(SR.GetString(SR.Invalid_max_event_details_length, name, _maxEventDetailsLength.ToString(CultureInfo.CurrentCulture)));
            }

            ProviderUtil.GetAndRemovePositiveAttribute(config, "commandTimeout", name, ref _commandTimeout);
            
            base.Initialize(name, config);
        }

        private void CheckSchemaVersion(SqlConnection connection) {
            string[] features = { "Health Monitoring" };
            string   version  = "1";
            SecUtility.CheckSchemaVersion( this, connection, features, version, ref _SchemaVersionCheck );
        }
        
        public override void ProcessEventFlush(WebEventBufferFlushInfo flushInfo) {
            Debug.Trace("SqlWebEventProvider", "EventBufferFlush called: " + 
                "NotificationType=" + flushInfo.NotificationType +
                ", NotificationSequence=" + flushInfo.NotificationSequence + 
                ", Events.Count=" + flushInfo.Events.Count);
            
            WriteToSQL(flushInfo.Events, flushInfo.EventsDiscardedSinceLastNotification,
                flushInfo.LastNotificationUtc);
        }

        void PrepareParams(SqlCommand sqlCommand) {
            sqlCommand.Parameters.Add(new SqlParameter("@EventId", SqlDbType.Char, 32));
            sqlCommand.Parameters.Add(new SqlParameter("@EventTimeUtc", SqlDbType.DateTime));
            sqlCommand.Parameters.Add(new SqlParameter("@EventTime", SqlDbType.DateTime));
            sqlCommand.Parameters.Add(new SqlParameter("@EventType", SqlDbType.NVarChar, 256));
            sqlCommand.Parameters.Add(new SqlParameter("@EventSequence", SqlDbType.Decimal));
            sqlCommand.Parameters.Add(new SqlParameter("@EventOccurrence", SqlDbType.Decimal));
            sqlCommand.Parameters.Add(new SqlParameter("@EventCode", SqlDbType.Int));
            sqlCommand.Parameters.Add(new SqlParameter("@EventDetailCode", SqlDbType.Int));
            sqlCommand.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, 1024));
            sqlCommand.Parameters.Add(new SqlParameter("@ApplicationPath", SqlDbType.NVarChar, 256));
            sqlCommand.Parameters.Add(new SqlParameter("@ApplicationVirtualPath", SqlDbType.NVarChar, 256));
            sqlCommand.Parameters.Add(new SqlParameter("@MachineName", SqlDbType.NVarChar, 256));
            sqlCommand.Parameters.Add(new SqlParameter("@RequestUrl", SqlDbType.NVarChar, 1024));
            sqlCommand.Parameters.Add(new SqlParameter("@ExceptionType", SqlDbType.NVarChar, 256));
            sqlCommand.Parameters.Add(new SqlParameter("@Details", SqlDbType.NText));
        }

        void FillParams(SqlCommand sqlCommand, WebBaseEvent eventRaised) {
            Exception               exception = null;
            WebRequestInformation   reqInfo = null;
            string                  details = null;
            WebApplicationInformation   appInfo = WebBaseEvent.ApplicationInformation;
            int                     n = 0;

            sqlCommand.Parameters[n++].Value = eventRaised.EventID.ToString("N", CultureInfo.InstalledUICulture);   // @EventId
            sqlCommand.Parameters[n++].Value = eventRaised.EventTimeUtc;      // @EventTimeUtc
            sqlCommand.Parameters[n++].Value = eventRaised.EventTime;         // @EventTime
            sqlCommand.Parameters[n++].Value = eventRaised.GetType().ToString();  // @EventType
            sqlCommand.Parameters[n++].Value = eventRaised.EventSequence;     // @EventSequence
            sqlCommand.Parameters[n++].Value = eventRaised.EventOccurrence;     // @EventOccurrence
            sqlCommand.Parameters[n++].Value = eventRaised.EventCode;         // @EventCode
            sqlCommand.Parameters[n++].Value = eventRaised.EventDetailCode;   // @EventDetailCode
            sqlCommand.Parameters[n++].Value = eventRaised.Message;           // @Message
            sqlCommand.Parameters[n++].Value = appInfo.ApplicationPath;       // @ApplicationPath
            sqlCommand.Parameters[n++].Value = appInfo.ApplicationVirtualPath; // @ApplicationVirtualPath
            sqlCommand.Parameters[n++].Value = appInfo.MachineName; // @MachineName

            // 
            
            // @RequestUrl
            if (eventRaised is WebRequestEvent) {
                reqInfo = ((WebRequestEvent)eventRaised).RequestInformation;
            }
            else if (eventRaised is WebRequestErrorEvent) {
                reqInfo = ((WebRequestErrorEvent)eventRaised).RequestInformation;
            }
            else if (eventRaised is WebErrorEvent) {
                reqInfo = ((WebErrorEvent)eventRaised).RequestInformation;
            }
            else if (eventRaised is WebAuditEvent) {
                reqInfo = ((WebAuditEvent)eventRaised).RequestInformation;
            }
            sqlCommand.Parameters[n++].Value = (reqInfo != null) ? reqInfo.RequestUrl : Convert.DBNull;

            // @ExceptionType
            if (eventRaised is WebBaseErrorEvent) {
                exception = ((WebBaseErrorEvent)eventRaised).ErrorException;
            }
            sqlCommand.Parameters[n++].Value = (exception != null) ? exception.GetType().ToString() : Convert.DBNull;

            // @Details
            details = eventRaised.ToString();
            if (_maxEventDetailsLength != NO_LIMIT &&
                details.Length > _maxEventDetailsLength) {
                details = details.Substring(0, _maxEventDetailsLength);
            }
            sqlCommand.Parameters[n++].Value = details;
        }

        [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted = true)]
        [SqlClientPermission(SecurityAction.Assert, Unrestricted = true)]
        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        void WriteToSQL(WebBaseEventCollection events, int eventsDiscardedByBuffer, DateTime lastNotificationUtc) {
            // We don't want to send any more events until we've waited until the _retryDate (which defaults to minValue)
            if (_retryDate > DateTime.UtcNow) {
                return;
            }

            try {
                SqlConnectionHolder sqlConnHolder = SqlConnectionHelper.GetConnection(_sqlConnectionString, true);

                SqlCommand sqlCommand = new SqlCommand(SP_LOG_EVENT);

                CheckSchemaVersion(sqlConnHolder.Connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Connection = sqlConnHolder.Connection;

                if (_commandTimeout > -1) {
                    sqlCommand.CommandTimeout = _commandTimeout;
                }

                PrepareParams(sqlCommand);

                try {
                    sqlConnHolder.Open(null, true);
                    Interlocked.Increment(ref _connectionCount);

                    if (eventsDiscardedByBuffer != 0) {
                        WebBaseEvent infoEvent = new WebBaseEvent(
                            SR.GetString(SR.Sql_webevent_provider_events_dropped,
                                eventsDiscardedByBuffer.ToString(CultureInfo.InstalledUICulture),
                                lastNotificationUtc.ToString("r", CultureInfo.InstalledUICulture)),
                                null,
                                WebEventCodes.WebEventProviderInformation,
                                WebEventCodes.SqlProviderEventsDropped);

                        FillParams(sqlCommand, infoEvent);
                        sqlCommand.ExecuteNonQuery();
                    }

                    foreach (WebBaseEvent eventRaised in events) {
                        FillParams(sqlCommand, eventRaised);
                        sqlCommand.ExecuteNonQuery();
                    }
                }
#if DBG
                catch (Exception e) {
                    Debug.Trace("SqlWebEventProvider", "ExecuteNonQuery failed: " + e);
                    throw;
                }
#endif
                finally {
                    sqlConnHolder.Close();
                    Interlocked.Decrement(ref _connectionCount);
                }

#if (!DBG)
                try {
#endif
                    EventProcessingComplete(events);
#if (!DBG)
                }
                catch {
                    // Ignore all errors.
                }
#endif
            }
            catch {
                // For any failure, we will wait at least 30 seconds or _commandTimeout before trying again
                double timeout = 30;
                if (_commandTimeout > -1) {
                    timeout = (double)_commandTimeout;
                }
                _retryDate = DateTime.UtcNow.AddSeconds(timeout);
                throw;
            }
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (UseBuffering) {
                base.ProcessEvent(eventRaised);
            }
            else {
                Debug.Trace("SqlWebEventProvider", "Writing event to SQL: event=" + eventRaised.GetType().Name);
                WriteToSQL(new WebBaseEventCollection(eventRaised), 0, new DateTime(0));
            }
        }

        protected virtual void EventProcessingComplete(WebBaseEventCollection raisedEvents) {
        }

        public override void Shutdown() {
            try {
                Flush();
            }
            finally {
                base.Shutdown();
            }

            // VSWhidbey 531556: Need to wait until all connections are gone before returning here
            // Sleep for 2x the command timeout in 1 sec intervals then give up, default timeout is 30 sec
            if (_connectionCount > 0) {
                int sleepAttempts = _commandTimeout*2;
                if (sleepAttempts <= 0) {
                    sleepAttempts = 60;
                }
                // Check every second
                while (_connectionCount > 0 && sleepAttempts > 0) {
                    --sleepAttempts;
                    Thread.Sleep(1000);
                }
            }
        }
    }
}

