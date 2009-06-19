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

namespace DbLinq.Data.Linq.Database
{
    /// <summary>
    /// IDatabaseContext contains all database related information:
    /// - connection
    /// - creates or manage a transaction
    /// </summary>
#if !MONO_STRICT
    public
#endif
    interface IDatabaseContext : IDisposable
    {
        // there are two ways to specify a connection:
        // 1. use a provided IDbConnection
        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <value>The connection.</value>
        IDbConnection Connection { set; get; }
        // 2. create our own
        /// <summary>
        /// Connects with the specified connection string.
        /// Alters Connection property
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        void Connect(string connectionString);
        /// <summary>
        /// Disconnects this instance.
        /// </summary>
        void Disconnect();

        // connection and transactions
        /// <summary>
        /// Creates a transaction.
        /// </summary>
        /// <returns></returns>
        IDatabaseTransaction Transaction();
        /// <summary>
        /// Opens a connection.
        /// </summary>
        /// <returns></returns>
        IDisposable OpenConnection();

        // factory
        /// <summary>
        /// Creates a command.
        /// </summary>
        /// <returns></returns>
        IDbCommand CreateCommand();
        /// <summary>
        /// Creates a DataAdapter.
        /// </summary>
        /// <returns></returns>
        IDbDataAdapter CreateDataAdapter();
    }
}
