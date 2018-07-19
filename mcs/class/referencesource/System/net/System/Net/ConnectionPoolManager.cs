//------------------------------------------------------------------------------
// <copyright file="ConnectionPoolManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Net
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Globalization;

    internal class ConnectionPoolManager {
        private static Hashtable m_ConnectionPools = new Hashtable();   // Hashtable used for connection pools
        private static object s_InternalSyncObject;

        private ConnectionPoolManager() {
        }

        private static object InternalSyncObject {
            get {
                if (s_InternalSyncObject == null) {
                    object o = new Object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, o, null);
                }
                return s_InternalSyncObject;
            }
        } 

        /*internal static ConnectionPool[] ConnectionPools {
            get {
                lock(InternalSyncObject) {
                    ConnectionPool [] connectionPools = new ConnectionPool[m_ConnectionPools.Count];
                    m_ConnectionPools.CopyTo(connectionPools, 0);
                    return connectionPools;
                }
            }
        }
        */
        private static string GenerateKey(string hostName, int port, string groupName) {
            return hostName+"\r"+port.ToString(NumberFormatInfo.InvariantInfo)+"\r"+groupName;
        }

        internal static ConnectionPool GetConnectionPool(ServicePoint servicePoint, string groupName, CreateConnectionDelegate createConnectionCallback) {
            string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock(InternalSyncObject) {
                ConnectionPool connectionPool = (ConnectionPool) m_ConnectionPools[key];
                if (connectionPool == null) {                    
                    connectionPool = new ConnectionPool(servicePoint, servicePoint.ConnectionLimit, 0, servicePoint.MaxIdleTime, createConnectionCallback);
                    m_ConnectionPools[key] = connectionPool;
                }
                return connectionPool;
            }
        }

        /*
        internal static ConnectionPool GetConnectionPool(string hostName, int port, string groupName, CreateConnectionDelegate createConnectionCallback) {
            string key = hostName + "\r" + port.ToString(NumberFormatInfo.InvariantInfo) + "\r" + groupName;
            lock(InternalSyncObject) {
                ConnectionPool connectionPool = (ConnectionPool) m_ConnectionPools[key];
                if (connectionPool == null) {                    
                    ServicePoint servicePoint = ServicePointManager.FindServicePoint(new Uri("sockets://" + hostName + ":" + port.ToString(NumberFormatInfo.InvariantInfo)), null);
                    connectionPool = new ConnectionPool(servicePoint, m_DefaultMaxPool, 0,  servicePoint.MaxIdleTime, createConnectionCallback);
                    m_ConnectionPools[key] = connectionPool;
                }
                return connectionPool;
            }
        }
        */

        internal static bool RemoveConnectionPool(ServicePoint servicePoint, string groupName) {
            string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock(InternalSyncObject) {
                ConnectionPool connectionPool = (ConnectionPool)(m_ConnectionPools[key]);
                if(connectionPool != null)
                {
                    m_ConnectionPools[key] = null;
                    m_ConnectionPools.Remove(key);
                    return true;
                }
            }
            return false;
        }

        // Gets the connection pool for the specified service point (if it exists) and 
        // invokes ForceCleanup to gracefully clean up any connections in the pool.
        //
        // preconditions: none
        //
        // postconditions: if a connection pool for the specified servicepoint and group name
        // exists, any open connections that are not in use will be gracefully cleaned up.
        //
        // Note: this will not destroy the connection pool itself so any future objects that
        // need that connection pool will still be able to access it to create new connections
        // and any objects using connections in the pool will still be able to return them
        internal static void CleanupConnectionPool(ServicePoint servicePoint, string groupName) {
            string key = GenerateKey(servicePoint.Host, servicePoint.Port, groupName);
            lock (InternalSyncObject) {
                ConnectionPool connectionPool = (ConnectionPool)(m_ConnectionPools[key]);
                if (connectionPool != null) {
                    connectionPool.ForceCleanup();
                }
            }
        }
    }
}
