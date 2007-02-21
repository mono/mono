//
// Mono.Data.Sqlite.SQLiteMetaDataCollectionNames.cs
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

  /// <summary>
  /// MetaDataCollections specific to Sqlite
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
  }
}
#endif
