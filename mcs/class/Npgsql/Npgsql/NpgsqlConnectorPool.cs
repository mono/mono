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
//	ConnectorPool.cs
// ------------------------------------------------------------------
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - creation
//		          - 05/??/2004 - Glen Parker<glenebob@nwlink.com> rewritten using
//                               System.Queue.

using System;
using System.Collections;
using Npgsql;
using System.Threading;

namespace Npgsql
{
    internal class ConnectorPool
    {
        /// <summary>
        /// A queue with an extra Int32 for keeping track of busy connections.
        /// </summary>
        private class ConnectorQueue : System.Collections.Queue
        {
            /// <summary>
            /// The number of pooled Connectors that belong to this queue but
            /// are currently in use.
            /// </summary>
            public Int32            UseCount = 0;
        }

        /// <value>Unique static instance of the connector pool
        /// mamager.</value>
        internal static ConnectorPool ConnectorPoolMgr = new Npgsql.ConnectorPool();

        public ConnectorPool()
        {
            PooledConnectors = new Hashtable();
        }


        /// <value>Map of index to unused pooled connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of queues of pooled connectors available to be used.</remarks>
        private Hashtable PooledConnectors;

        /// <value>Map of shared connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of shared connectors available to be used.</remarks>
        // To be implemented
        //private Hashtable SharedConnectors;

        /// <summary>
        /// Searches the shared and pooled connector lists for a
        /// matching connector object or creates a new one.
        /// </summary>
        /// <param name="Connection">The NpgsqlConnection that is requesting
        /// the connector. Its ConnectionString will be used to search the
        /// pool for available connectors.</param>
        /// <returns>A connector object.</returns>
        internal Npgsql.Connector RequestConnector (NpgsqlConnection Connection)
        {
            if (Connection.Pooling) {
                return RequestPooledConnector(Connection);
            } else {
                return GetNonPooledConnector(Connection);
            }
        }

        /// <summary>
        /// Find a pooled connector.  Handle locking and timeout here.
        /// </summary>
        private Npgsql.Connector RequestPooledConnector (NpgsqlConnection Connection)
        {
            Connector       Connector;
            Int32           timeoutMilliseconds = Connection.Timeout * 1000;

            lock(this)
            {
                Connector = RequestPooledConnectorInternal(Connection);
            }

            while (Connector == null && timeoutMilliseconds > 0)
            {
                Int32 ST = timeoutMilliseconds > 1000 ? 1000 : timeoutMilliseconds;

                Thread.Sleep(ST);
                timeoutMilliseconds -= ST;

                lock(this)
                {
                    Connector = RequestPooledConnectorInternal(Connection);
                }
            }

            if (Connector == null) {
                throw new Exception("Timeout while getting a connection from pool.");
            }

            return Connector;
        }

        /// <summary>
        /// Find a pooled connector.  Handle shared/non-shared here.
        /// </summary>
        private Npgsql.Connector RequestPooledConnectorInternal (NpgsqlConnection Connection)
        {
            Connector       Connector = null;
            Boolean         Shared = false;

            // If sharing were implemented, I suppose Shared would be set based
            // on some property on the Connection.

            if (! Shared) {
                Connector = GetPooledConnector(Connection);
            } else {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }

            return Connector;
        }

        /// <summary>
        /// Releases a connector, possibly back to the pool for future use.
        /// </summary>
        /// <remarks>
        /// Pooled connectors will be put back into the pool if there is room.
        /// Shared connectors should just have their use count decremented
        /// since they always stay in the shared pool.
        /// </remarks>
        /// <param name="Connector">The connector to release.</param>
        /// <param name="ForceClose">Force the connector to close, even if it is pooled.</param>
        public void ReleaseConnector (Connector Connector, bool ForceClose)
        {
            if (Connector.Connection.Pooling) {
                ReleasePooledConnector(Connector, ForceClose);
            } else {
                UngetNonPooledConnector(Connector);
            }
        }

        /// <summary>
        /// Release a pooled connector.  Handle locking here.
        /// </summary>
        private void ReleasePooledConnector (Connector Connector, bool ForceClose)
        {
            lock(this)
            {
                ReleasePooledConnectorInternal(Connector, ForceClose);
            }
        }

        /// <summary>
        /// Release a pooled connector.  Handle shared/non-shared here.
        /// </summary>
        private void ReleasePooledConnectorInternal (Connector Connector, bool ForceClose)
        {
            if (! Connector.Shared) {
                UngetPooledConnector(Connector, ForceClose);
            } else {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }
        }

        /// <summary>
        /// Create a connector without any pooling functionality.
        /// </summary>
        private Npgsql.Connector GetNonPooledConnector(NpgsqlConnection Connection)
        {
            Connector       Connector;

            Connector = new Connector(false);
            Connector.Connection = Connection;

            return Connector;
        }

        /// <summary>
        /// Find an available pooled connector in the non-shared pool, or create
        /// a new one if none found.
        /// </summary>
        private Npgsql.Connector GetPooledConnector(NpgsqlConnection Connection)
        {
            ConnectorQueue  Queue;
            Connector       Connector = null;

            // Try to find a queue.
            Queue = (ConnectorQueue)PooledConnectors[Connection.ConnectionString];

            if (Queue == null) {
                Queue = new ConnectorQueue();
                PooledConnectors[Connection.ConnectionString] = Queue;
            }

			if (Queue.Count > 0) {
                // Found a queue with connectors.  Grab the top one.
                Connector = (Connector)Queue.Dequeue();
                Queue.UseCount++;
                Connector.Connection = Connection;
            } else if (Queue.Count + Queue.UseCount < Connection.MaxPoolSize) {
                Connector = new Connector(false);
                Queue.UseCount++;
                Connector.Connection = Connection;
            }
            
            
            return Connector;
        }

        /// <summary>
        /// Find an available shared connector in the shared pool, or create
        /// a new one if none found.
        /// </summary>
        private Npgsql.Connector GetSharedConnector(NpgsqlConnection Connection)
        {
            // To be implemented

            return null;
        }

        /// <summary>
        /// Close the connector.
        /// </summary>
        /// <param name="Connector">Connector to release</param>
        private void UngetNonPooledConnector(Connector Connector)
        {
            Connector.Close();
        }

        /// <summary>
        /// Put a pooled connector into the pool queue.  Create the queue if needed.
        /// </summary>
        /// <param name="Connector">Connector to pool</param>
        private void UngetPooledConnector(Connector Connector, bool ForceClose)
        {
            ConnectorQueue           Queue;

            // Find the queue.
            Queue = (ConnectorQueue)PooledConnectors[Connector.Connection.ConnectionString];

            if (Queue == null) {
                throw new InvalidOperationException("Internal: No connector queue found for existing connector.");
            }

            if (ForceClose) {
                Connector.Close();
            } else {
                Connector.Connection = null;
                Queue.Enqueue(Connector);
            }

            Queue.UseCount--;
        }

        /// <summary>
        /// Stop sharing a shared connector.
        /// </summary>
        /// <param name="Connector">Connector to unshare</param>
        private void UngetSharedConnector(Connector Connector)
        {
            // To be implemented
        }
    }
}
