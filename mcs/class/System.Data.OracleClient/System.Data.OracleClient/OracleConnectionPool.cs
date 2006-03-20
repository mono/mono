//
// OracleConnectionPool.cs 
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Authors: 
//    Hubert FONGARNAND <informatique.internet@fiducial.fr>
//   
// (C) Copyright 2005 Hubert FONGARNAND
//
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OracleClient.Oci;
using System.Drawing.Design;
using System.EnterpriseServices;
using System.Text;
using System.Threading;

namespace System.Data.OracleClient 
{
	internal class OracleConnectionPool 
	{
		ArrayList list = new ArrayList (); // list of connections
		OracleConnectionInfo info;
		OracleConnectionPoolManager manager;
		bool initialized;
		int activeConnections = 0;
		int PoolMinSize;
		int PoolMaxSize;
		
		
		public OracleConnectionPool (OracleConnectionPoolManager manager, OracleConnectionInfo info, int minPoolSize, int maxPoolSize) 
		{
			this.info = info;
			this.manager = manager;
			initialized = false;
			PoolMinSize = minPoolSize;
			PoolMaxSize = maxPoolSize;
		}
		
		public OciGlue GetConnection () 
		{
			OciGlue connection = null;
			lock (list) {
				if (!initialized) {
					
					for (int n = 0; n < PoolMinSize; n++)
						list.Add (CreateConnection ());
					initialized = true;
				}
				do {
					if (list.Count > 0) {
						// There are available connections in the pool
						connection = (OciGlue)list [list.Count - 1];
						list.RemoveAt (list.Count -1);
						if (!connection.Connected){
							connection = null;
							continue;
						}
					}
					
					if (connection == null && activeConnections < PoolMaxSize) {
						connection = CreateConnection ();
					}
					// Pas de connection disponible on attends que quelqu'un en libere une
					if (connection == null) {
						if (Monitor.Wait (list, 6000) == false)
							throw new InvalidOperationException ("Timeout expired.  The timeout expired waiting for a connection in the pool probably due to max connections reached.");
					}
				} while (connection == null);
			}
			return connection;
		}
		
		public void ReleaseConnection (OciGlue connection) 
		{
			lock (list) {
				list.Add (connection);
				Monitor.Pulse (list);
			}
		}
		
		OciGlue CreateConnection () 
		{
			activeConnections++;
			return manager.CreateConnection (info);
		}

		public void Dispose () 
		{
			if (list != null) {
				if (list.Count > 0)
					foreach (OciGlue connection in list)
						if (connection.Connected)
							connection.Disconnect ();
				list.Clear ();
				list = null;
			}			
		}
	}
}

