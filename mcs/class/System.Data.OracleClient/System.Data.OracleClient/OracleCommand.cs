// 
// OracleCommand.cs
//
// Part of the Mono class libraries at
// mcs/class/System.Data.OracleClient/System.Data.OracleClient
//
// Assembly: System.Data.OracleClient.dll
// Namespace: System.Data.OracleClient
//
// Author: Daniel Morgan <danmorg@sc.rr.com>
//
// Copyright (C) Daniel Morgan, 2002
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Data;
using System.Data.OracleClient.OCI;

namespace System.Data.OracleClient
{
	public class OracleCommand
	{
		string sql = "";
		OracleConnection conn = null;

		public OracleCommand ()
		{
			
		}

		public int ExecuteNonQuery () 
		{
			int rowsAffected = -1;

			if(conn == null)
				throw new Exception("Connection is null");
			if(conn.State != ConnectionState.Open)
				throw new Exception("ConnectionState not Open");
			if(sql.Equals(""))
				throw new Exception("CommandText is StringEmpty");

			Int32 status;
			status = conn.Oci.PrepareAndExecuteNonQuerySimple (sql);
			if(status != 0) {
				string statusText;
				statusText = conn.Oci.CheckError(status);
				string msg = 
					"SQL Error: [" + 
					sql + "] " +
					statusText;
				throw new Exception(msg);
			}

			return rowsAffected;
		}

		public string CommandText {
			get {
				return sql;
			}
			set {
				sql = value;
			}
		}

		public OracleConnection Connection {
			get {
				return conn;
			}
			set {
				conn = value;
			}
		}
	}
}
