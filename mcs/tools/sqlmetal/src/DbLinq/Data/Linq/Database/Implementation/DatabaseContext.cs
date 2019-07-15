#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Reflection;

namespace DbLinq.Data.Linq.Database.Implementation
{
    /// <summary>
    /// Default database context implementation
    /// </summary>
    internal class DatabaseContext : IDatabaseContext
    {
        private bool _connectionOwner;
        private IDbConnection _connection;
        public IDbConnection Connection
        {
            get { return _connection; }
            set { ChangeConnection(value, false); }
        }

        public IDbTransaction CurrentTransaction { get; set; }

        private readonly DbProviderFactory _providerFactory;
        /// <summary>
        /// Gets the provider factory.
        /// </summary>
        /// <value>The provider factory.</value>
        protected DbProviderFactory ProviderFactory
        {
            get
            {
                if (_providerFactory == null)
                    throw new Exception("In order to use this method, a DbProviderFactory must be provided");
                return _providerFactory;
            }
        }

        /// <summary>
        /// Connects with the specified connection string.
        /// Alters Connection property
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public void Connect(string connectionString)
        {
            IDbConnection connection = null;
            if (connectionString != null)
            {
                connection = ProviderFactory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            ChangeConnection(connection, true);
        }

        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        public void Disconnect()
        {
            if (Connection != null && _connectionOwner)
                Connection.Close();
        }

        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <returns></returns>
        public IDisposable OpenConnection()
        {
            // if we don't own the connection, then it is not our business
            if (_connectionOwner)
                return null;

            return new DatabaseConnection(_connection);
        }

        /// <summary>
        /// Creates a transaction.
        /// </summary>
        /// <returns></returns>
        public IDbTransaction CreateTransaction()
        {
            if (CurrentTransaction != null)
                throw new InvalidOperationException("Attempting to create a transaction while within a transaction.");
            return CurrentTransaction = Connection.BeginTransaction();
        }

        /// <summary>
        /// Creates a command.
        /// </summary>
        /// <returns></returns>
        public IDbCommand CreateCommand()
        {
            IDbCommand command = Connection.CreateCommand();
            if (command.Transaction == null)
                command.Transaction = CurrentTransaction;
            return command;
        }

        /// <summary>
        /// Creates a DataAdapter.
        /// </summary>
        /// <returns></returns>
        public IDbDataAdapter CreateDataAdapter()
        {
            return ProviderFactory.CreateDataAdapter();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            ClearConnection();
        }

        /// <summary>
        /// Sets the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="owner">if set to <c>true</c> [owner].</param>
        protected void SetConnection(IDbConnection connection, bool owner)
        {
            if (connection == null)
                return;

            _connectionOwner = owner;
            _connection = connection;
            if (owner)
                _connection.Open();
        }

        /// <summary>
        /// Clears the connection.
        /// </summary>
        protected void ClearConnection()
        {
            if (_connectionOwner && _connection != null)
                _connection.Dispose();
        }

        /// <summary>
        /// Changes the connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="owner">if set to <c>true</c> [owner].</param>
        protected void ChangeConnection(IDbConnection connection, bool owner)
        {
            ClearConnection();
            SetConnection(connection, owner);
        }

        /// <summary>
        /// Finds a DbProviderFactory, if possible, by AppDomain scan.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns></returns>
        protected static DbProviderFactory FindFactory(IDbConnection connection)
        {
            // we start from connection assembly
            var connectionAssembly = connection.GetType().Assembly;
            // then look for all types present in assembly
            foreach (var testType in connectionAssembly.GetExportedTypes())
            {
                if (typeof(DbProviderFactory).IsAssignableFrom(testType))
                {
                    var bindingFlags = BindingFlags.Static | BindingFlags.Public;
                    FieldInfo instanceFieldInfo = testType.GetField("Instance", bindingFlags);
                    if (instanceFieldInfo != null)
                    {
                        return (DbProviderFactory)instanceFieldInfo.GetValue(null);
                    }
                    PropertyInfo instancePropertyInfo = testType.GetProperty("Instance", bindingFlags);
                    if (instancePropertyInfo != null)
                    {
                        return (DbProviderFactory)instancePropertyInfo.GetGetMethod().Invoke(null, new object[0]);
                    }
                }
            }
            return null;
        }

        public DatabaseContext(DbProviderFactory providerFactory)
            : this(providerFactory, null)
        {
        }

        public DatabaseContext(DbProviderFactory providerFactory, string connectionString)
        {
            _providerFactory = providerFactory;
            Connect(connectionString);
        }

        public DatabaseContext(IDbConnection connection)
        {
            _providerFactory = FindFactory(connection);
            Connection = connection;
        }
    }
}
