//
// System.Data.OleDb.OleDbTransaction
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//
// Copyright (C) Rodrigo Moya, 2002
//

using System.Data;
using System.Data.Common;
using System.Exception;

namespace System.Data.OleDb
{
	public sealed class OleDbTransaction : MarshalByRefObject,
		IDbTransaction, IDisposable
	{
		private OleDbConnection m_connection = null;
		private IsolationLevel m_level = IsolationLevel.ReadCommitted;

		/*
		 * Constructors
		 */
		
		protected OleDbTransaction (OleDbConnection cnc)
		{
			m_connection = cnc;
			libgda.gda_connection_begin_transaction (m_connection.GdaConnection,
								 null);
		}

		protected OleDbTransaction (OleDbConnection cnc,
					    IsolationLevel level) : this (cnc)
		{
			m_level = level;
		}

		/*
		 * Properties
		 */

		IDbConnection IDbTransaction.Connection
		{
			get {
				return m_connection;
			}
		}

		IsolationLevel IDbTransaction.IsolationLevel
		{
			get {
				return m_level;
			}
		}

		/*
		 * Methods
		 */

		public OleDbTransaction Begin ()
		{
			return new OleDbTransaction (m_connection);
		}

		public OleDbTransaction Begin (IsolationLevel level)
		{
			return new OleDbTransaction (m_connection, level);
		}

		void IDbTransaction.Commit ()
		{
			if (!libgda.gda_connection_commit_transaction (
				    m_connection.GdaConnection,
				    null))
				throw new InvalidOperationException ();
		}

		void IDbTransaction.Rollback ()
	        {
			if (!libgda.gda_connection_rollback_transaction (
				    m_connection.GdaConnection,
				    null))
				throw new InvalidOperationException ();
		}

		[MonoTODO]
		void IDisposable.Dispose ()
		{
			throw new NotImplementedException ();
		}
	}
}
