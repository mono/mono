//
// OracleConnectionPoolManager.cs 
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
// (C) Copyright Hubert FONGARNAND, 2005
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
	internal class OracleConnectionPoolManager 
	{
		Hashtable pools = new Hashtable();
		
		public OracleConnectionPoolManager () 
		{
		}
		
		public OracleConnectionPool GetConnectionPool (OracleConnectionInfo info, int minPoolSize, int maxPoolSize) 
		{
			lock (pools) {
				
				OracleConnectionPool pool = (OracleConnectionPool) pools [info.ConnectionString];
				if (pool == null) {
					pool = new OracleConnectionPool (this, info, minPoolSize, maxPoolSize);
					pools [info.ConnectionString] = pool;
				}
				return pool;
			}
		}
		
		public virtual OciGlue CreateConnection (OracleConnectionInfo info) 
		{
			OciGlue oci;
			oci = new OciGlue ();
			oci.CreateConnection (info);
			return oci;
		}

		public void Dispose () 
		{
			if (pools != null) {
				foreach (OracleConnectionPool pool in pools)
					pool.Dispose ();
				pools.Clear ();
				pools = null;
			}
		}

		~OracleConnectionPoolManager () 
		{
			Dispose ();
		}
	}
}

