// NpgsqlTransactionCallbacks.cs
//
// Author:
//  Josh Cooley <jbnpgsql@tuxinthebox.net>
//
// Copyright (C) 2007, The Npgsql Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.Data;

namespace Npgsql
{
	internal interface INpgsqlTransactionCallbacks : IDisposable
	{
		string GetName();
		void PrepareTransaction();
		void CommitTransaction();
		void RollbackTransaction();
	}

	internal class NpgsqlTransactionCallbacks : MarshalByRefObject, INpgsqlTransactionCallbacks
	{
		private NpgsqlConnection _connection;
		private readonly string _connectionString;
		private bool _closeConnectionRequired;
		private bool _prepared;
		private readonly string _txName = Guid.NewGuid().ToString();

		private static readonly String CLASSNAME = "NpgsqlTransactionCallbacks";

		public NpgsqlTransactionCallbacks(NpgsqlConnection connection)
		{
			_connection = connection;
			_connectionString = _connection.ConnectionString;
			_connection.Disposed += new EventHandler(_connection_Disposed);
		}

		private void _connection_Disposed(object sender, EventArgs e)
		{
			// TODO: what happens if this is called from another thread?
			// connections should not be shared across threads while in a transaction
			_connection.Disposed -= new EventHandler(_connection_Disposed);
			_connection = null;
		}

		private NpgsqlConnection GetConnection()
		{
			if (_connection == null || (_connection.FullState & ConnectionState.Open) != ConnectionState.Open)
			{
				_connection = new NpgsqlConnection(_connectionString);
				_connection.Open();
				_closeConnectionRequired = true;
				return _connection;
			}
			else
			{
				return _connection;
			}
		}

		#region INpgsqlTransactionCallbacks Members

		public string GetName()
		{
			return _txName;
		}

		public void CommitTransaction()
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "CommitTransaction");
			NpgsqlConnection connection = GetConnection();
			NpgsqlCommand command = null;
			if (_prepared)
			{
				command = new NpgsqlCommand(string.Format("COMMIT PREPARED '{0}'", _txName), connection);
			}
			else
			{
				command = new NpgsqlCommand("COMMIT", connection);
			}
			command.ExecuteBlind();
		}

		public void PrepareTransaction()
		{
            if (!_prepared)
            {
                NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "PrepareTransaction");
                NpgsqlConnection connection = GetConnection();
                NpgsqlCommand command = new NpgsqlCommand(string.Format("PREPARE TRANSACTION '{0}'", _txName), connection);
                command.ExecuteBlind();
                _prepared = true;
            }
		}

		public void RollbackTransaction()
		{
			NpgsqlEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "RollbackTransaction");
			NpgsqlConnection connection = GetConnection();
			NpgsqlCommand command = null;
			if (_prepared)
			{
				command = new NpgsqlCommand(string.Format("ROLLBACK PREPARED '{0}'", _txName), connection);
			}
			else
			{
				command = new NpgsqlCommand("ROLLBACK", connection);
			}
			command.ExecuteBlind();
		}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			if (_closeConnectionRequired)
			{
				_connection.Close();
			}
			_closeConnectionRequired = false;
		}

		#endregion
	}
}
