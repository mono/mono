// created on 17/11/2002 at 19:04

// Npgsql.NpgsqlTransaction.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
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


using System;
using System.Text;
using System.Data;


namespace Npgsql
{
	/// <summary>

	/// Represents a transaction to be made in a PostgreSQL database. This class cannot be inherited.

	/// </summary>
	public sealed class NpgsqlTransaction : MarshalByRefObject, IDbTransaction
	{
		
		private static readonly String CLASSNAME = "NpgsqlTransaction";
		
		private NpgsqlConnection	_conn				= null;
		private IsolationLevel		_isolation	= IsolationLevel.ReadCommitted;
		private bool _disposing = false;
		private System.Resources.ResourceManager resman;


		
		
		internal NpgsqlTransaction(NpgsqlConnection conn) : this(conn, IsolationLevel.ReadCommitted)
		{
			
		}
		
		internal NpgsqlTransaction(NpgsqlConnection conn, IsolationLevel isolation)
		{
			resman = new System.Resources.ResourceManager(this.GetType());

			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
			if ((isolation != IsolationLevel.ReadCommitted) &&
				(isolation != IsolationLevel.Serializable))
				throw new ArgumentException(resman.GetString("Exception_UnsopportedIsolationLevel"), "isolation");
			
			_conn = conn;
			_isolation = isolation;
			
			StringBuilder commandText = new StringBuilder("SET TRANSACTION ISOLATION LEVEL ");
			
			if (isolation == IsolationLevel.ReadCommitted)
				commandText.Append("READ COMMITTED");
			else
				commandText.Append("SERIALIZABLE");
			
			commandText.Append("; BEGIN");
			
						
			NpgsqlCommand command = new NpgsqlCommand(commandText.ToString(), conn);
			command.ExecuteNonQuery();
		  _conn.InTransaction = true;
						
		}

		/// <summary>
		/// Gets the <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> 
		/// object associated with the transaction, or a null reference if the 
		/// transaction is no longer valid.
		/// </summary>
		/// <value>The <see cref="Npgsql.NpgsqlConnection">NpgsqlConnection</see> 
		/// object associated with the transaction.</value>
		public NpgsqlConnection Connection
		{
			get
			{
				return _conn;
			}
			
		}
		
		
		IDbConnection IDbTransaction.Connection
		{
			get
			{
				return Connection;
			}
		}

		/// <summary>
		/// Specifies the <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction.
		/// </summary>
		/// <value>The <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction. 
		/// The default is <b>ReadCommitted</b>.</value>
		public IsolationLevel IsolationLevel
		{
			get
			{
				return _isolation;
			}
		}

		/// <summary>
		/// Releases the unmanaged resources used by the 
		/// <see cref="Npgsql.NpgsqlTransaction">NpgsqlTransaction</see> 
		/// and optionally releases the managed resources.
		/// </summary>
		public void Dispose()
		{
            this.Dispose(true);			
		}

		private void Dispose(bool disposing){
			if(disposing == true && this._conn != null){
				this._disposing = true;
				this.Rollback();
			}
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		public void Commit()
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Commit");
			NpgsqlCommand command = new NpgsqlCommand("COMMIT", _conn);
			command.ExecuteNonQuery();
			_conn.InTransaction = false;
		}

		/// <summary>
		/// Rolls back a transaction from a pending state.
		/// </summary>
		public void Rollback()
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Rollback");
			NpgsqlCommand command = new NpgsqlCommand("ROLLBACK", _conn);
			command.ExecuteNonQuery();
			_conn.InTransaction = false;
		}
		
		internal bool Disposing{
			get{
				return _disposing;
			}
		}
	}
}
