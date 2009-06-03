/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Data;
  using System.Data.Common;
  using System.Collections.Generic;
  using System.Globalization;
  using System.ComponentModel;

  public sealed partial class SqliteConnection
  {
    /// <summary>
    /// Returns a SQLiteProviderFactory object.
    /// </summary>
    protected override DbProviderFactory DbProviderFactory
    {
      get { return SqliteFactory.Instance; }
    }
  }
}

