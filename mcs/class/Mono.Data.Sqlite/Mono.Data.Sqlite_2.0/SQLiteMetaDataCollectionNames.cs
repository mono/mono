/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;

  /// <summary>
  /// MetaDataCollections specific to SQLite
  /// </summary>
  public static class SqliteMetaDataCollectionNames
  {
    /// <summary>
    /// Returns a list of databases attached to the connection
    /// </summary>
    public static readonly string Catalogs = "Catalogs";
    /// <summary>
    /// Returns column information for the specified table
    /// </summary>
    public static readonly string Columns = "Columns";
    /// <summary>
    /// Returns index information for the optionally-specified table
    /// </summary>
    public static readonly string Indexes = "Indexes";
    /// <summary>
    /// Returns base columns for the given index
    /// </summary>
    public static readonly string IndexColumns = "IndexColumns";
    /// <summary>
    /// Returns the tables in the given catalog
    /// </summary>
    public static readonly string Tables = "Tables";
    /// <summary>
    /// Returns user-defined views in the given catalog
    /// </summary>
    public static readonly string Views = "Views";
    /// <summary>
    /// Returns underlying column information on the given view
    /// </summary>
    public static readonly string ViewColumns = "ViewColumns";
    /// <summary>
    /// Returns foreign key information for the given catalog
    /// </summary>
    public static readonly string ForeignKeys = "ForeignKeys";
    /// <summary>
    /// Returns the triggers on the database
    /// </summary>
    public static readonly string Triggers = "Triggers";
  }
}
