//
// Mono.Data.SqliteClient.SqliteTransaction.cs
//
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
	public sealed class SqliteTransaction : IDbTransaction
	{
	
		#region Fields
		
		private IsolationLevel _isolationLevel;
		private SqliteConnection _connection;
		private bool _open;
		
		#endregion

		#region Contructors and destructors
		
		internal SqliteTransaction() 
		{
			_open = true;
		}

		void System.IDisposable.Dispose() 
		{
		}
		
		#endregion

		#region Public Properties

		public IDbConnection Connection
		{
			get { return _connection; } 
			set { _connection = (SqliteConnection)value; }
		}

		public IsolationLevel IsolationLevel 
		{
			get { return _isolationLevel; }
			set { _isolationLevel = value; }
		}

		#endregion
		
		#region Public Methods
		
		public void Commit()
		{
			if (_connection == null || _connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been committed or is not pending");
			try 
			{
				SqliteCommand cmd = _connection.CreateCommand();
				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				_open = false;
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}

		public void Rollback()
		{
			if (_connection == null || _connection.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!_open)
				throw new InvalidOperationException("Transaction has already been rolled back or is not pending");
			try 
			{
				SqliteCommand cmd = _connection.CreateCommand();
				cmd.CommandText = "COMMIT";
				cmd.ExecuteNonQuery();
				_open = false;
			}
			catch (Exception ex) 
			{
				throw ex;
			}
		}
		
		#endregion
	}
}
