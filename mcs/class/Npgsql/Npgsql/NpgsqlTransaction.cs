// created on 17/11/2002 at 19:04

// Npgsql.NpgsqlTransaction.cs
// 
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
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
	
	public sealed class NpgsqlTransaction : IDbTransaction
	{
		
		private static readonly String CLASSNAME = "NpgsqlTransaction";
		
		private NpgsqlConnection	_conn				= null;
		private IsolationLevel		_isolation	= IsolationLevel.ReadCommitted;
		
		
		
		internal NpgsqlTransaction(NpgsqlConnection conn) : this(conn, IsolationLevel.ReadCommitted)
		{
			
		}
		
		internal NpgsqlTransaction(NpgsqlConnection conn, IsolationLevel isolation)
		{
			
			if ((isolation != IsolationLevel.ReadCommitted) &&
				(isolation != IsolationLevel.Serializable))
				throw new ArgumentException("Must be Read Committed or Serializable", "isolation");
			
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
						
		}
		
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
		
		public IsolationLevel IsolationLevel
		{
			get
			{
				return _isolation;
			}
		}
		
		public void Dispose()
		{
			
		}
		
		public void Commit()
		{
			NpgsqlCommand command = new NpgsqlCommand("COMMIT", _conn);
			command.ExecuteNonQuery();
			_conn.InTransaction = false;
		}
		
		public void Rollback()
		{
			NpgsqlCommand command = new NpgsqlCommand("ROLLBACK", _conn);
			command.ExecuteNonQuery();
			_conn.InTransaction = false;
		}
		
		
	}
}
