//
// Mono.Data.Sqlite.SQLiteConnectionStringBuilder.cs
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
  using System.ComponentModel;
  using System.Collections;
  using System.Globalization;
  using System.Reflection;

#if !PLATFORM_COMPACTFRAMEWORK
  using System.ComponentModel.Design;

  /// <summary>
  /// Sqlite implementation of DbConnectionStringBuilder.
  /// </summary>
  [DefaultProperty("DataSource")]
  [DefaultMember("Item")]
  public sealed class SqliteConnectionStringBuilder : DbConnectionStringBuilder
  {
    /// <summary>
    /// Properties of this class
    /// </summary>
    private Hashtable _properties;

    /// <overloads>
    /// Constructs a new instance of the class
    /// </overloads>
    /// <summary>
    /// Default constructor
    /// </summary>
    public SqliteConnectionStringBuilder()
    {
      Initialize(null);
    }

    /// <summary>
    /// Constructs a new instance of the class using the specified connection string.
    /// </summary>
    /// <param name="connectionString">The connection string to parse</param>
    public SqliteConnectionStringBuilder(string connectionString)
    {
      Initialize(connectionString);
    }

    /// <summary>
    /// Private initializer, which assigns the connection string and resets the builder
    /// </summary>
    /// <param name="cnnString">The connection string to assign</param>
    private void Initialize(string cnnString)
    {
      _properties = new Hashtable();
      base.GetProperties(_properties);

      if (String.IsNullOrEmpty(cnnString) == false)
        ConnectionString = cnnString;
    }

    /// <summary>
    /// Gets/Sets the default version of the Sqlite engine to instantiate.  Currently the only valid value is 3, indicating version 3 of the sqlite library.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(3)]
    public int Version
    {
      get
      {
        if (ContainsKey("Version") == false) return 3;

        return Convert.ToInt32(this["Version"], CultureInfo.CurrentCulture);
      }
      set
      {
        if (value != 3)
          throw new NotSupportedException();

        this["Version"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the synchronous mode of the connection string.  Default is "Normal".
    /// </summary>
    [DisplayName("Synchronous")]
    [Browsable(true)]
    [DefaultValue(SynchronizationModes.Normal)]
    public SynchronizationModes SyncMode
    {
      get
      {
        return (SynchronizationModes)TypeDescriptor.GetConverter(typeof(SynchronizationModes)).ConvertFrom(this["Synchronous"]);
      }
      set
      {
        this["Synchronous"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the encoding for the connection string.  The default is "False" which indicates UTF-8 encoding.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(false)]
    public bool UseUTF16Encoding
    {
      get
      {
        return Convert.ToBoolean(this["UseUTF16Encoding"], CultureInfo.CurrentCulture);
      }
      set
      {
        this["UseUTF16Encoding"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the filename to open on the connection string.
    /// </summary>
    [DisplayName("Data Source")]
    [Browsable(true)]
    public string DataSource
    {
      get
      {
        if (ContainsKey("Data Source") == false) return "";

        return this["Data Source"].ToString();
      }
      set
      {
        this["Data Source"] = value;
      }
    }

#region Mono-specific
    /// <summary>
    /// Gets/Sets the filename to open on the connection string (Mono-specific, uses DataSource).
    /// </summary>
    [DisplayName("Data Source")]
    [Browsable(true)]
    public string Uri
    {
      get
      {
	return DataSource;
      }
      set
      {
        DataSource = value;
      }
    }
#endregion
    
    /// <summary>
    /// Determines whether or not the connection will automatically participate
    /// in the current distributed transaction (if one exists)
    /// </summary>
    [DisplayName("Automatic Enlistment")]
    [Browsable(true)]
    [DefaultValue(true)]
    public bool Enlist
    {
      get
      {
        if (ContainsKey("Enlist") == false) return true;

        return (this["Enlist"].ToString() == "Y");
      }
      set
      {
        this["Enlist"] = (value == true) ? "Y" : "N";
      }
    }
    /// <summary>
    /// Gets/sets the database encryption password
    /// </summary>
    [Browsable(true)]
    [PasswordPropertyText(true)]
    public string Password
    {
      get
      {
        if (ContainsKey("Password") == false) return "";

        return this["Password"].ToString();
      }
      set
      {
        this["Password"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the page size for the connection.
    /// </summary>
    [DisplayName("Page Size")]
    [Browsable(true)]
    [DefaultValue(1024)]
    public int PageSize
    {
      get
      {
        if (ContainsKey("Page Size") == false) return 1024;
        return Convert.ToInt32(this["Page Size"], CultureInfo.InvariantCulture);
      }
      set
      {
        this["Page Size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the cache size for the connection.
    /// </summary>
    [DisplayName("Cache Size")]
    [Browsable(true)]
    [DefaultValue(2000)]
    public int CacheSize
    {
      get
      {
        if (ContainsKey("Cache Size") == false) return 2000;
        return Convert.ToInt32(this["Cache Size"], CultureInfo.InvariantCulture);
      }
      set
      {
        this["Cache Size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the datetime format for the connection.
    /// </summary>
    [Browsable(true)]
    [DefaultValue(SqliteDateFormats.ISO8601)]
    public SqliteDateFormats DateTimeFormat
    {
      get
      {
        if (ContainsKey("DateTimeFormat") == false) return SqliteDateFormats.ISO8601;

        return (SqliteDateFormats)TypeDescriptor.GetConverter(typeof(SqliteDateFormats)).ConvertFrom(this["DateTimeFormat"]);
      }
      set
      {
        this["DateTimeFormat"] = value;
      }
    }

    /// <summary>
    /// Helper function for retrieving values from the connectionstring
    /// </summary>
    /// <param name="keyword">The keyword to retrieve settings for</param>
    /// <param name="value">The resulting parameter value</param>
    /// <returns>Returns true if the value was found and returned</returns>
    public override bool TryGetValue(string keyword, out object value)
    {
      bool b = base.TryGetValue(keyword, out value);

      if (!_properties.ContainsKey(keyword)) return b;

      PropertyDescriptor pd = _properties[keyword] as PropertyDescriptor;

      if (pd == null) return b;

      if (b)
      {
        value = TypeDescriptor.GetConverter(pd.PropertyType).ConvertFrom(value);
      }
      else
      {
        DefaultValueAttribute att = pd.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
        if (att != null)
        {
          value = att.Value;
          b = true;
        }
      }
      return b;
    }
  }
#endif
}
#endif
