//
// Mono.Data.Sqlite.SQLiteFactory.cs
//
// Author(s):
//   Robert Simpson (robert@blackcastlesoft.com)
//
// Adapted and modified for the Mono Project by
//   Marek Habersack (grendello@gmail.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2007 Marek Habersack
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

/********************************************************
 * ADO.NET 2.0 Data Provider for Sqlite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/
#if NET_2_0
namespace Mono.Data.Sqlite
{
  using System;
  using System.Data.Common;

#if !PLATFORM_COMPACTFRAMEWORK
  /// <summary>
  /// Sqlite implementation of DbProviderFactory.
  /// </summary>
  public sealed class SqliteFactory : DbProviderFactory
  {
    /// <summary>
    /// Static instance member which returns an instanced SqliteFactory class.
    /// </summary>
    public static readonly SqliteFactory Instance = new SqliteFactory();

    /// <summary>
    /// Returns a new SqliteCommand object.
    /// </summary>
    /// <returns>A SqliteCommand object.</returns>
    public override DbCommand CreateCommand()
    {
      return new SqliteCommand();
    }

    /// <summary>
    /// Returns a new SqliteCommandBuilder object.
    /// </summary>
    /// <returns>A SqliteCommandBuilder object.</returns>
    public override DbCommandBuilder CreateCommandBuilder()
    {
      return new SqliteCommandBuilder();
    }

    /// <summary>
    /// Creates a new SqliteConnection.
    /// </summary>
    /// <returns>A SqliteConnection object.</returns>
    public override DbConnection CreateConnection()
    {
      return new SqliteConnection();
    }

    /// <summary>
    /// Creates a new SqliteConnectionStringBuilder.
    /// </summary>
    /// <returns>A SqliteConnectionStringBuilder object.</returns>
    public override DbConnectionStringBuilder CreateConnectionStringBuilder()
    {
      return new SqliteConnectionStringBuilder();
    }

    /// <summary>
    /// Creates a new SqliteDataAdapter.
    /// </summary>
    /// <returns>A SqliteDataAdapter object.</returns>
    public override DbDataAdapter CreateDataAdapter()
    {
      return new SqliteDataAdapter();
    }

    /// <summary>
    /// Creates a new SqliteParameter.
    /// </summary>
    /// <returns>A SqliteParameter object.</returns>
    public override DbParameter CreateParameter()
    {
      return new SqliteParameter();
    }
  }
#endif
}
#endif
