// ByteFX.Data data access components for .Net
// Copyright (C) 2002-2003  ByteFX, Inc.
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
using System.Data;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Represents a SQL transaction to be made in a MySQL database. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlTransaction.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	public sealed class MySqlTransaction : IDbTransaction
	{
		private IsolationLevel	_level;
		private MySqlConnection	_conn;
		private bool			_open;

		internal MySqlTransaction() 
		{
			_open = true;
		}

		#region Properties

		/// <summary>
		/// Gets the <see cref="MySqlConnection"/> object associated with the transaction, or a null reference (Nothing in Visual Basic) if the transaction is no longer valid.
		/// </summary>
		public IDbConnection Connection
		{
			get { return _conn;	} 
			set { _conn = (MySqlConnection)value; }
		}

		/// <summary>
		/// Specifies the <see cref="IsolationLevel"/> for this transaction.
		/// </summary>
		public IsolationLevel IsolationLevel 
		{
			get { return _level; }
			set { _level = value; }
		}

		#endregion

		void System.IDisposable.Dispose() 
		{
		}

		/// <summary>
		/// Commits the database transaction.
		/// </summary>
		public void Commit()
		{
			if (_conn == null || _conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been committed or is not pending");
			Driver d = _conn.InternalConnection.Driver;
			try 
			{
				d.Send(DBCmd.QUERY, "COMMIT");
				_open = false;
			}
			catch (MySqlException ex) 
			{
				throw ex;
			}
		}

		/// <summary>
		/// Overloaded. Rolls back a transaction from a pending state.
		/// </summary>
		public void Rollback()
		{
			if (_conn == null || _conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been rolled back or is not pending");
			Driver d = _conn.InternalConnection.Driver;
			try 
			{
				d.Send(DBCmd.QUERY, "ROLLBACK");
				_open = false;
			}
			catch (MySqlException ex) 
			{
				throw ex;
			}
		}
	}
}
