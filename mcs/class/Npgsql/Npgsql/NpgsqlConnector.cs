//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
//	Connector.cs
// ------------------------------------------------------------------
//	Project
//		Npgsql
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - created
//		          - 06/??/2004 - Glen Parker<glenebob@nwlink.com> rewritten

using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Data;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Mono.Security.Protocol.Tls;

using NpgsqlTypes;

namespace Npgsql
{
    /// <summary>
    /// !!! Helper class, for compilation only.
    /// Connector implements the logic for the Connection Objects to
    /// access the physical connection to the database, and isolate
    /// the application developer from connection pooling internals.
    /// </summary>
    internal class NpgsqlConnector
    {
        // Immutable.
        internal NpgsqlConnectionString                ConnectionString;

        /// <summary>
        /// Occurs on NoticeResponses from the PostgreSQL backend.
        /// </summary>
        internal event NoticeEventHandler			         Notice;

        /// <summary>
        /// Occurs on NotificationResponses from the PostgreSQL backend.
        /// </summary>
        internal event NotificationEventHandler        Notification;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateSelectionCallback delegate.
        /// </summary>
        internal event CertificateSelectionCallback    CertificateSelectionCallback;

        /// <summary>
        /// Mono.Security.Protocol.Tls.CertificateValidationCallback delegate.
        /// </summary>
        internal event CertificateValidationCallback   CertificateValidationCallback;

        /// <summary>
        /// Mono.Security.Protocol.Tls.PrivateKeySelectionCallback delegate.
        /// </summary>
        internal event PrivateKeySelectionCallback     PrivateKeySelectionCallback;

        private ConnectionState                  _connection_state;

        // The physical network connection to the backend.
        private Stream                           _stream;

        // Mediator which will hold data generated from backend.
        private NpgsqlMediator                   _mediator;

        private ProtocolVersion                  _backendProtocolVersion;
        private ServerVersion                    _serverVersion;

        // Values for possible CancelRequest messages.
        private NpgsqlBackEndKeyData             _backend_keydata;

        // Flag for transaction status.
        //        private Boolean                         _inTransaction = false;
        private NpgsqlTransaction                _transaction = null;

        private Boolean                          _supportsPrepare = false;

        private NpgsqlBackendTypeMapping         _oidToNameMapping = null;

        private Encoding                         _encoding;

        private Boolean                          _isInitialized;

        private Boolean                          _pooled;
        private Boolean                          _shared;

        private NpgsqlState                      _state;


        private Int32                            _planIndex;
        private Int32                            _portalIndex;

        private const String                     _planNamePrefix = "NpgsqlPlan";
        private const String                     _portalNamePrefix = "NpgsqlPortal";



        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shared">Controls whether the connector can be shared.</param>
        public NpgsqlConnector(NpgsqlConnectionString ConnectionString, bool Pooled, bool Shared)
        {
            this.ConnectionString = ConnectionString;
            _connection_state = ConnectionState.Closed;
            _pooled = Pooled;
            _shared = Shared;
            _isInitialized = false;
            _state = NpgsqlClosedState.Instance;
            _mediator = new NpgsqlMediator();
            _oidToNameMapping = new NpgsqlBackendTypeMapping();
            _planIndex = 0;
            _portalIndex = 0;

        }


        internal String Host
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Host);
            }
        }

        internal Int32 Port
        {
            get
            {
                return ConnectionString.ToInt32(ConnectionStringKeys.Port, ConnectionStringDefaults.Port);
            }
        }

        internal String Database
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Database, UserName);
            }
        }

        internal String UserName
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.UserName);
            }
        }

        internal String Password
        {
            get
            {
                return ConnectionString.ToString(ConnectionStringKeys.Password);
            }
        }

        internal Boolean SSL
        {
            get
            {
                return ConnectionString.ToBool(ConnectionStringKeys.SSL);
            }
        }

        /// <summary>
        /// Gets the current state of the connection.
        /// </summary>
        internal ConnectionState State {
            get
            {
                return _connection_state;
            }
        }


        // State
        internal void Query (NpgsqlCommand queryCommand)
        {
            CurrentState.Query(this, queryCommand );
        }

        internal void Authenticate (string password)
        {
            CurrentState.Authenticate(this, password );
        }

        internal void Parse (NpgsqlParse parse)
        {
            CurrentState.Parse(this, parse);
        }

        internal void Flush ()
        {
            CurrentState.Flush(this);
        }

        internal void Sync ()
        {
            CurrentState.Sync(this);
        }

        internal void Bind (NpgsqlBind bind)
        {
            CurrentState.Bind(this, bind);
        }

        internal void Execute (NpgsqlExecute execute)
        {
            CurrentState.Execute(this, execute);
        }



        /// <summary>
        /// This method checks if the connector is still ok.
        /// We try to send a simple query text, select 1 as ConnectionTest;
        /// </summary>
	
        internal Boolean IsValid()
        {
            try
            {
                // Here we use a fake NpgsqlCommand, just to send the test query string.
                Query(new NpgsqlCommand("select 1 as ConnectionTest"));
            }
            catch
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// This method is responsible to release all portals used by this Connector.
        /// </summary>
        internal void ReleasePlansPortals()
        {
            Int32 i = 0;

            if (_planIndex > 0)
            {
                for(i = 1; i <= _planIndex; i++)
                    Query(new NpgsqlCommand(String.Format("deallocate \"{0}\";", _planNamePrefix + i)));
            }

            _portalIndex = 0;
            _planIndex = 0;


        }



        /// <summary>
        /// Check for mediator errors (sent by backend) and throw the appropriate
        /// exception if errors found.  This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckErrors()
        {
            if (_mediator.Errors.Count > 0)
            {
                throw new NpgsqlException(_mediator.Errors);
            }
        }

        /// <summary>
        /// Check for notices and fire the appropiate events.
        /// This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckNotices()
        {
            if (Notice != null)
            {
                foreach (NpgsqlError E in _mediator.Notices)
                {
                    Notice(this, new NpgsqlNoticeEventArgs(E));
                }
            }
        }

        /// <summary>
        /// Check for notifications and fire the appropiate events.
        /// This needs to be called after every interaction
        /// with the backend.
        /// </summary>
        internal void CheckNotifications()
        {
            if (Notification != null)
            {
                foreach (NpgsqlNotificationEventArgs E in _mediator.Notifications)
                {
                    Notification(this, E);
                }
            }
        }

        /// <summary>
        /// Check for errors AND notifications in one call.
        /// </summary>
        internal void CheckErrorsAndNotifications()
        {
            CheckNotices();
            CheckNotifications();
            CheckErrors();
        }

        /// <summary>
        /// Default SSL CertificateSelectionCallback implementation.
        /// </summary>
        internal X509Certificate DefaultCertificateSelectionCallback(
            X509CertificateCollection      clientCertificates,
            X509Certificate                serverCertificate,
            string                         targetHost,
            X509CertificateCollection      serverRequestedCertificates)
        {
            if (CertificateSelectionCallback != null)
            {
                return CertificateSelectionCallback(clientCertificates, serverCertificate, targetHost, serverRequestedCertificates);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Default SSL CertificateValidationCallback implementation.
        /// </summary>
        internal bool DefaultCertificateValidationCallback(
            X509Certificate       certificate,
            int[]                 certificateErrors)
        {
            if (CertificateValidationCallback != null)
            {
                return CertificateValidationCallback(certificate, certificateErrors);
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Default SSL PrivateKeySelectionCallback implementation.
        /// </summary>
        internal AsymmetricAlgorithm DefaultPrivateKeySelectionCallback(
            X509Certificate                certificate,
            string                         targetHost)
        {
            if (PrivateKeySelectionCallback != null)
            {
                return PrivateKeySelectionCallback(certificate, targetHost);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Version of backend server this connector is connected to.
        /// </summary>
        internal ServerVersion ServerVersion
        {
            get
            {
                return _serverVersion;
            }
            set
            {
                _serverVersion = value;
            }
        }

        internal Encoding Encoding
        {
            get
            {
                return _encoding;
            }
            set
            {
                _encoding = value;
            }
        }

        /// <summary>
        /// Backend protocol version in use by this connector.
        /// </summary>
        internal ProtocolVersion BackendProtocolVersion
        {
            get
            {
                return _backendProtocolVersion;
            }
            set
            {
                _backendProtocolVersion = value;
            }
        }

        /// <summary>
        /// The physical connection stream to the backend.
        /// </summary>
        internal Stream Stream {
            get
            {
                return _stream;
            }
            set
            {
                _stream = value;
            }
        }

        /// <summary>
        /// Reports if this connector is fully connected.
        /// </summary>
        internal Boolean IsInitialized
        {
            get
            {
                return _isInitialized;
            }
            set
            {
                _isInitialized = value;
            }
        }

        internal NpgsqlState CurrentState {
            get
            {
                return _state;
            }
            set
            {
                _state = value;
            }
        }


        internal bool Pooled
        {
            get
            {
                return _pooled;
            }
        }

        internal bool Shared
        {
            get
            {
                return _shared;
            }
        }

        internal NpgsqlBackEndKeyData BackEndKeyData {
            get
            {
                return _backend_keydata;
            }
        }

        internal NpgsqlBackendTypeMapping OidToNameMapping {
            get
            {
                return _oidToNameMapping;
            }
        }

        /// <summary>
        /// The connection mediator.
        /// </summary>
        internal NpgsqlMediator	Mediator {
            get
            {
                return _mediator;
            }
        }

        /// <summary>
        /// Report if the connection is in a transaction.
        /// </summary>
        internal NpgsqlTransaction Transaction {
            get
            {
                return _transaction;
            }
            set
            {
                _transaction = value;
            }
        }

        /// <summary>
        /// Report whether the current connection can support prepare functionality.
        /// </summary>
        internal Boolean SupportsPrepare {
            get
            {
                return _supportsPrepare;
            }
            set
            {
                _supportsPrepare = value;
            }
        }

        /// <summary>
        /// This method is required to set all the version dependent features flags.
        /// SupportsPrepare means the server can use prepared query plans (7.3+)
        /// </summary>
        // FIXME - should be private
        internal void ProcessServerVersion ()
        {
            this._supportsPrepare = (ServerVersion >= new ServerVersion(7, 3, 0));
        }

        /// <value>Counts the numbers of Connections that share
        /// this Connector. Used in Release() to decide wether this
        /// connector is to be moved to the PooledConnectors list.</value>
        // internal int mShareCount;

        /// <summary>
        /// Opens the physical connection to the server.
        /// </summary>
        /// <remarks>Usually called by the RequestConnector
        /// Method of the connection pool manager.</remarks>
        internal void Open()
        {
            ProtocolVersion      PV;

            // If Connection.ConnectionString specifies a protocol version, we will
            // not try to fall back to version 2 on failure.
            if (ConnectionString.Contains(ConnectionStringKeys.Protocol))
            {
                PV = ConnectionString.ToProtocolVersion(ConnectionStringKeys.Protocol);
            }
            else
            {
                PV = ProtocolVersion.Unknown;
            }

            _backendProtocolVersion = (PV == ProtocolVersion.Unknown) ? ProtocolVersion.Version3 : PV;

            // Reset state to initialize new connector in pool.
            Encoding = Encoding.Default;
            CurrentState = NpgsqlClosedState.Instance;

            // Get a raw connection, possibly SSL...
            CurrentState.Open(this);
            // Establish protocol communication and handle authentication...
            CurrentState.Startup(this);

            // Check for protocol not supported.  If we have been told what protocol to use,
            // we will not try this step.
            if (_mediator.Errors.Count > 0 && PV == ProtocolVersion.Unknown)
            {
                // If we attempted protocol version 3, it may be possible to drop back to version 2.
                if (BackendProtocolVersion == ProtocolVersion.Version3)
                {
                    NpgsqlError       Error0 = (NpgsqlError)_mediator.Errors[0];

                    // If NpgsqlError.ReadFromStream_Ver_3() encounters a version 2 error,
                    // it will set its own protocol version to version 2.  That way, we can tell
                    // easily if the error was a FATAL: protocol error.
                    if (Error0.BackendProtocolVersion == ProtocolVersion.Version2)
                    {
                        // Try using the 2.0 protocol.
                        _mediator.ResetResponses();
                        BackendProtocolVersion = ProtocolVersion.Version2;
                        CurrentState = NpgsqlClosedState.Instance;

                        // Get a raw connection, possibly SSL...
                        CurrentState.Open(this);
                        // Establish protocol communication and handle authentication...
                        CurrentState.Startup(this);
                    }
                }
            }

            // Check for errors and do the Right Thing.
            // FIXME - CheckErrors needs to be moved to Connector
            CheckErrors();

            _backend_keydata = _mediator.BackendKeyData;

            // Change the state of connection to open and ready.
            _connection_state = ConnectionState.Open;
            CurrentState = NpgsqlReadyState.Instance;

            String       ServerVersionString = String.Empty;

            // First try to determine backend server version using the newest method.
            if (((NpgsqlParameterStatus)_mediator.Parameters["__npgsql_server_version"]) != null)
                ServerVersionString = ((NpgsqlParameterStatus)_mediator.Parameters["__npgsql_server_version"]).ParameterValue;


            // Fall back to the old way, SELECT VERSION().
            // This should not happen for protocol version 3+.
            if (ServerVersionString.Length == 0)
            {
                NpgsqlCommand command = new NpgsqlCommand("select version();set DATESTYLE TO ISO;", this);
                ServerVersionString = PGUtil.ExtractServerVersion( (String)command.ExecuteScalar() );
            }

            // Cook version string so we can use it for enabling/disabling things based on
            // backend version.
            ServerVersion = PGUtil.ParseServerVersion(ServerVersionString);

            // Adjust client encoding.

            //NpgsqlCommand commandEncoding1 = new NpgsqlCommand("show client_encoding", _connector);
            //String clientEncoding1 = (String)commandEncoding1.ExecuteScalar();

            if (ConnectionString.ToString(ConnectionStringKeys.Encoding, ConnectionStringDefaults.Encoding).ToUpper() == "UNICODE")
            {
                Encoding = Encoding.UTF8;
                NpgsqlCommand commandEncoding = new NpgsqlCommand("SET CLIENT_ENCODING TO UNICODE", this);
                commandEncoding.ExecuteNonQuery();
            }

            // Make a shallow copy of the type mapping that the connector will own.
            // It is possible that the connector may add types to its private
            // mapping that will not be valid to another connector, even
            // if connected to the same backend version.
            _oidToNameMapping = NpgsqlTypesHelper.CreateAndLoadInitialTypesMapping(this).Clone();

            ProcessServerVersion();

            // The connector is now fully initialized. Beyond this point, it is
            // safe to release it back to the pool rather than closing it.
            IsInitialized = true;
        }


        /// <summary>
        /// Closes the physical connection to the server.
        /// </summary>
        internal void Close()
        {
            try
            {
                this.CurrentState.Close(this);
            }
            catch {}
        }


    ///<summary>
    /// Returns next portal index.
    ///</summary>
    internal String NextPortalName()
        {
            return _portalNamePrefix + System.Threading.Interlocked.Increment(ref _portalIndex);
        }


        ///<summary>
        /// Returns next plan index.
        ///</summary>
        internal String NextPlanName()
        {

            return _planNamePrefix + System.Threading.Interlocked.Increment(ref _planIndex);
        }



    }
}
