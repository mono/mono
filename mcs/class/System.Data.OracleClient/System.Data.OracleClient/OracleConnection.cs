//
// OracleConnection.cs 
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
// Original source code for setting ConnectionString 
// by Tim Coleman <tim@timcoleman.com>
//
// Copyright (C) Tim Coleman, 2002
//
// Licensed under the MIT/X11 License.
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Data.OracleClient.OCI;
using System.Text;

namespace System.Data.OracleClient 
{
	internal struct OracleConnectionInfo 
	{
		public string Username;
		public string Password;
		public string Database;
	}

	public class OracleConnection 
	{
		private	OciGlue oci;
		private ConnectionState state;
		private OracleConnectionInfo conInfo;
		private string connectionString = "";

		public OracleConnection () 
		{
			state = ConnectionState.Closed;
			oci = new OciGlue ();
		}

		public OracleConnection (string connectionString) : this() 
		{
			this.connectionString = connectionString;
		}

		public void Open () 
		{
			Int32 status;
			
			status = oci.Connect(conInfo);
			if(status != 0)
				throw new Exception("Error: Unable to connect: " + 
					status.ToString() + 
					": " +
					oci.CheckError(status));
			else
				state = ConnectionState.Open;
		}

		public void Close () 
		{
			Int32 status = oci.Disconnect();
			state = ConnectionState.Closed;
			if(status != 0)
				throw new Exception("Error: Unable to disconnect: " + 
					status.ToString() + 
					": " +
					oci.CheckError(status));
		}

		// only for DEBUG purposes - not part of MS.NET 1.1 OracleClient
		public static uint ConnectionCount 
		{
			get {
				uint count = OciGlue.OciGlue_ConnectionCount();
				return count;
			}
		}

		public ConnectionState State 
		{
			get {
				return state;
			}
		}

		public string ConnectionString 
		{
			get {
				return connectionString;
			}
			set {
				SetConnectionString(value);
			}
		}

		internal OciGlue Oci 
		{
			get {
				return oci;
			}
		}

		void SetConnectionString (string connectionString) 
		{
			this.connectionString = connectionString;
			conInfo.Username = "";
			conInfo.Database = "";
			conInfo.Password = "";

			if (connectionString == String.Empty)
				return;
			
			connectionString += ";";
			NameValueCollection parameters = new NameValueCollection ();

			bool inQuote = false;
			bool inDQuote = false;

			string name = String.Empty;
			string value = String.Empty;
			StringBuilder sb = new StringBuilder ();

			foreach (char c in connectionString) {
				switch (c) {
				case '\'':
					inQuote = !inQuote;
					break;
				case '"' :
					inDQuote = !inDQuote;
					break;
				case ';' :
					if (!inDQuote && !inQuote) {
						if (name != String.Empty && name != null) {
							value = sb.ToString ();
							parameters [name.ToUpper ().Trim ()] = value.Trim ();
						}
						name = String.Empty;
						value = String.Empty;
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				case '=' :
					if (!inDQuote && !inQuote) {
						name = sb.ToString ();
						sb = new StringBuilder ();
					}
					else
						sb.Append (c);
					break;
				default:
					sb.Append (c);
					break;
				}
			}

			SetProperties (parameters);
		}

		private void SetProperties (NameValueCollection parameters) 
		{	
			string value;
			foreach (string name in parameters) {
				value = parameters[name];

				switch (name) {
				case "DATA SOURCE" :
				case "DATABASE" :
					// set Database property
					conInfo.Database = value;
					break;
				case "PASSWORD" :
				case "PWD" :
					conInfo.Password = value;
					break;
				case "UID" :
				case "USER ID" :
					conInfo.Username = value;
					break;
				default:
					throw new Exception("Connection parameter not supported.");
				}
			}
		}
	}
}
