//
// System.Data.OleDb.OleDbConnection
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace System.Data.OleDb
{
	public sealed class OleDbConnection : Component, ICloneable, IDbConnection
	{
		private IntPtr m_gdaConnection = IntPtr.Zero;
		private string m_string = "";
		private int m_timeout = 15; // default is 15 seconds
		
		public OleDbConnection ()
		{
		}

		public OleDbConnection (string cnc_string) : this ()
		{
			m_string = cnc_string;
		}

		string IDbConnection.ConnectionString
		{
			get {
				return m_string;
			}
			set {
				m_string = value;
			}
		}

		int IDbConnection.ConnectionTimeout
		{
			get {
				return m_timeout;
			}
		}

		string IDbConnection.Database
		{
			get {
				if (m_gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (m_gdaConnection)) {
					return libgda.gda_connection_get_database (m_gdaConnection);
				}

				return null;
			}
		}

		public string DataSource
		{
			get {
			}
		}

		public string Provider
		{
			get {
			}
		}

		public string ServerVersion
		{
			get {
			}
		}

		ConnectionState IDbConnection.State
		{
			get {
				if (m_gdaConnection != IntPtr.Zero) {
					if (libgda.gda_connection_is_open (m_gdaConnection))
						return ConnectionState.Open;
				}

				return ConnectionState.Closed;
			}
		}

		public OleDbTransaction BeginTransaction ()
		{
			if (m_gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this);

			return null;
		}

		 public OleDbTransaction BeginTransaction (IsolationLevel level)
		 {
			 if (m_gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this, level);

			return null;
		 }
	}
}
