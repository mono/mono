// Mono.Data.SqliteClient data access components for .Net
// Derived from ByteFX.Data
// With permission from Reggie Burnett to relicense under MIT/X11

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;

namespace Mono.Data.SqliteClient
{
	/// <summary>
	/// Represents a SQL transaction to be made in a Sqlite database. This class cannot be inherited.
	/// </summary>
	public sealed class SqliteTransaction : IDbTransaction
	{
		private IsolationLevel	_level;
		private SqliteConnection	_conn;
		private bool			_open;

		internal SqliteTransaction() 
		{
			_open = true;
		}

		#region Properties

		/// <summary>
		/// Gets the <see cref="SqliteConnection"/> object associated with the transaction, or a null reference (Nothing in Visual Basic) if the transaction is no longer valid.
		/// </summary>
		public IDbConnection Connection
		{
			get { return _conn;	} 
			set { _conn = (SqliteConnection)value; }
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
			try 
			{
				SqliteCommand cmd = _conn.CreateCommand();
				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				_open = false;
			}
			catch (Exception ex) 
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
			try 
			{
				SqliteCommand cmd = _conn.CreateCommand();
				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				_open = false;
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}
	}
}
