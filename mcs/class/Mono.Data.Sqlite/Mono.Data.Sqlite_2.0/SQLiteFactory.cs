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
  public sealed partial class SqliteFactory : DbProviderFactory
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
