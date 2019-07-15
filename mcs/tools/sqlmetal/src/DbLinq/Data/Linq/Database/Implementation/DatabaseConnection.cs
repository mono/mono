﻿#region MIT license
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

namespace DbLinq.Data.Linq.Database.Implementation
{
    /// <summary>
    /// Database connection allows to open a connection if none available
    /// </summary>
    internal class DatabaseConnection: IDisposable
    {
        private readonly IDbConnection _connection;
        private readonly bool _mustClose;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnection"/> class.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public DatabaseConnection(IDbConnection connection)
        {
            _connection = connection;
            switch (_connection.State)
            {
            case ConnectionState.Open:
                _mustClose = false;
                break;
            case ConnectionState.Closed:
                _mustClose = true;
                break;
            default:
                throw new ApplicationException("L33: Can only handle Open or Closed connection states, not " + _connection.State);
            }
            if (_mustClose)
                _connection.Open();
        }

        /// <summary>
        /// Disposes current connection, if owned.
        /// </summary>
        public void Dispose()
        {
            if (_mustClose)
                _connection.Close();
        }
    }
}
