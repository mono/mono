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
using System.Exception;

namespace System.Data.OleDb
{
	public sealed class OleDbConnection : Component, ICloneable, IDbConnection
	{
		private IntPtr m_gdaConnection = IntPtr.Zero;
		private string m_string = "";
		private int m_timeout = 15; // default is 15 seconds

		/*
		 * Constructors
		 */
		
		public OleDbConnection ()
		{
		}

		/*
		 * Properties
		 */
		
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
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
		}

		public string Provider
		{
			get {
				if (m_gdaConnection != IntPtr.Zero
				    && libgda.gda_connection_is_open (m_gdaConnection)) {
					return libgda.gda_connection_get_provider (m_gdaConnection);
				}

				return null;
			}
		}

		public string ServerVersion
		{
			[MonoTODO]
			get {
				throw new NotImplementedException ();
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

		internal IntPtr GdaConnection
		{
			get {
				return m_gdaConnection;
			}
		}
		
		/*
		 * Methods
		 */
		
		IDbTransaction IDbConnection.BeginTransaction ()
		{
			if (m_gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this);

			return null;
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel level)
		{
			if (m_gdaConnection != IntPtr.Zero)
				return new OleDbTransaction (this, level);

			return null;
		}

		void IDbConnection.ChangeDatabase (string name)
		{
			// FIXME: see http://bugzilla.gnome.org/show_bug.cgi?id=83315
		}

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException();
		}
		
		void IDbConnection.Close ()
		{
			if (m_gdaConnection != IntPtr.Zero) {
				libgda.gda_connection_close (m_gdaConnection);
				m_gdaConnection = IntPtr.Zero;
			}
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			if (m_gdaConnection != IntPtr.Zero
			    && libgda.gda_connection_is_open (m_gdaConnection)) {
				return new OleDbCommand ();
			}

			return null;
		}

		[MonoTODO]
		public DataTable GetOleDbSchemaTable (Guid schema,
						      object[] restrictions)
		{
			throw new NotImplementedException ();
		}

		void IDbConnection.Open ()
		{
			if (m_gdaConnection != IntPtr.Zero ||
			    libgda.gda_connection_is_open (m_gdaConnection))
				throw new InvalidOperationException ();

			m_gdaConnection = libgda.gda_client_open_connection (
				libgda.GdaClient,
				m_string,
				"", "");
		}

		/*
		 * Events
		 */
		
		public event OleDbInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;
	}
}
