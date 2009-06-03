/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Data.Common;

#if !PLATFORM_COMPACTFRAMEWORK
  /// <summary>
  /// SQLite implementation of DbProviderFactory.
  /// </summary>
  public sealed partial class SQLiteFactory : DbProviderFactory
  {
    /// <summary>
    /// Static instance member which returns an instanced SQLiteFactory class.
    /// </summary>
    public static readonly SQLiteFactory Instance = new SQLiteFactory();

    /// <summary>
    /// Returns a new SQLiteCommand object.
    /// </summary>
    /// <returns>A SQLiteCommand object.</returns>
    public override DbCommand CreateCommand()
    {
      return new SQLiteCommand();
    }

    /// <summary>
    /// Returns a new SQLiteCommandBuilder object.
    /// </summary>
    /// <returns>A SQLiteCommandBuilder object.</returns>
    public override DbCommandBuilder CreateCommandBuilder()
    {
      return new SQLiteCommandBuilder();
    }

    /// <summary>
    /// Creates a new SQLiteConnection.
    /// </summary>
    /// <returns>A SQLiteConnection object.</returns>
    public override DbConnection CreateConnection()
    {
      return new SQLiteConnection();
    }

    /// <summary>
    /// Creates a new SQLiteConnectionStringBuilder.
    /// </summary>
    /// <returns>A SQLiteConnectionStringBuilder object.</returns>
    public override DbConnectionStringBuilder CreateConnectionStringBuilder()
    {
      return new SQLiteConnectionStringBuilder();
    }

    /// <summary>
    /// Creates a new SQLiteDataAdapter.
    /// </summary>
    /// <returns>A SQLiteDataAdapter object.</returns>
    public override DbDataAdapter CreateDataAdapter()
    {
      return new SQLiteDataAdapter();
    }

    /// <summary>
    /// Creates a new SQLiteParameter.
    /// </summary>
    /// <returns>A SQLiteParameter object.</returns>
    public override DbParameter CreateParameter()
    {
      return new SQLiteParameter();
    }
  }
#endif
}
