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

namespace ByteFX.Data.MySQLClient
{
	public class MySQLTransaction : IDbTransaction
	{
		private IsolationLevel	_level;
		private MySQLConnection	_conn;
		private bool			_open;

		internal MySQLTransaction() 
		{
			_open = true;
		}

		public IsolationLevel IsolationLevel 
		{
			get { return _level; }
			set { _level = value; }
		}

		public IDbConnection Connection
		{
			get { return _conn;	} 
			set { _conn = (MySQLConnection)value; }
		}

		public void Dispose() 
		{
		}

		public void Commit()
		{
			if (_conn == null || _conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been committed or is not pending");
			Driver d = _conn.Driver;
			try 
			{
				d.SendCommand(DBCmd.QUERY, "COMMIT");
				_open = false;
			}
			catch (MySQLException ex) 
			{
				throw ex;
			}
		}

		public void Rollback()
		{
			if (_conn == null || _conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been rolled back or is not pending");
			Driver d = _conn.Driver;
			try 
			{
				d.SendCommand(DBCmd.QUERY, "ROLLBACK");
				_open = false;
			}
			catch (MySQLException ex) 
			{
				throw ex;
			}
		}
	}
}
