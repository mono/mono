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

using System;
using System.Net.Sockets;
using System.IO;
using System.Text;

namespace Npgsql
{
    /// <summary>
    /// !!! Helper class, for compilation only.
    /// Connector implements the logic for the Connection Objects to
    /// access the physical connection to the database, and isolate
    /// the application developer from connection pooling internals.
    /// </summary>
    internal class Connector
    {
        // Used to obtain a current key for the non-shared pool.
        private NpgsqlConnection connection;

        private Stream _stream;

        private ProtocolVersion _backendProtocolVersion;
        private ServerVersion _serverVersion;

        private Encoding _encoding;

        private Boolean _isInitialized;

        private Boolean             _shared;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="Shared">Controls whether the connector can be shared.</param>
        public Connector(bool Shared)
        {
            _shared = Shared;
            _isInitialized = false;
        }

        /// <summary>
        /// The NpgsqlConnection using this connector.  This will always return
        /// null for shared connectors.
        /// </summary>
        internal NpgsqlConnection Connection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = value;
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


        /// <value>Reports whether this connector can be shared.</value>
        /// <remarks>Set true if this connector is shared among multiple
        /// connections.</remarks>
        internal bool Shared
        {
            get
            {
                return _shared;
            }
        }

        /// <value>Counts the numbers of Connections that share
        /// this Connector. Used in Release() to decide wether this
        /// connector is to be moved to the PooledConnectors list.</value>
        internal int mShareCount;

        /// <summary>
        /// Opens the physical connection to the server.
        /// </summary>
        /// <remarks>Usually called by the RequestConnector
        /// Method of the connection pool manager.</remarks>
        internal void Open()
        {
            //this.Socket = new Npgsql.Socket();
            //this.Socket.Open(); // !!! to be fixed
            //this.mOpen = true;
        }


        /// <summary>
        /// Closes the physical connection to the server.
        /// </summary>
        internal void Close()
        {
            // HACK HACK
            // There needs to be a cleaner way to close this thing...
            try {
                Stream.Close();
            } catch {}
        }

        /*
        /// <summary>
        /// Releases a connector back to the pool manager's garding. Or to the
        /// garbage collection.
        /// </summary>
        /// <remarks>The Shared and Pooled properties are no longer needed after
        /// evaluation inside this method, so they are left in their current state.
        ///	They get new meaning again when the connector is requested from the
        /// pool manager later. </remarks>
        public void Release()
        {
            if ( this.mShared )
            {
                // A shared connector is returned to the pooled connectors
                // list only if it is not used by any Connection object.
                // Otherwise the job is done by simply decrementing the
                // usage counter:
                if ( --this.mShareCount == 0 )
                {
                    Npgsql.ConnectorPool.ConnectorPoolMgr.CutOutConnector( this );
                    // Shared connectors are *always* pooled after usage.
                    // Depending on the Pooled property at this point
                    // might introduce a lot of trouble into an application...
                    Npgsql.ConnectorPool.ConnectorPoolMgr.InsertPooledConnector( this );
                }
            }
            else // it is a nonshared connector
            {
                if ( this.Pooled )
                {
                    // Pooled connectors are simply put in the
                    // PooledConnectors list for later recycling
                    Npgsql.ConnectorPool.ConnectorPoolMgr.InsertPooledConnector( this );
                }
                else
                {
                    // Unpooled private connectors get the physical
                    // connection closed, they are *not* recyled later.
                    // Instead they are (implicitly) handed over to the
                    // garbage collection.
                    // !!! to be fixed
                    //this.Socket.Close();
                }
            }
        }*/
    }
}
