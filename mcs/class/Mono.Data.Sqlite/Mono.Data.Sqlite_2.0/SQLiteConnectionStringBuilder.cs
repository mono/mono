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
  using System.ComponentModel;
  using System.Collections;
  using System.Globalization;
  using System.Reflection;

#if !PLATFORM_COMPACTFRAMEWORK

#if!WINDOWS_STORE_APP
  using System.ComponentModel.Design;
#endif

  /// <summary>
  /// SQLite implementation of DbConnectionStringBuilder.
  /// </summary>
#if !WINDOWS_STORE_APP
  [DefaultProperty ("DataSource")]
  [DefaultMember ("Item")]
#endif
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
      _properties = new Hashtable (StringComparer.OrdinalIgnoreCase);
#if !WINDOWS_STORE_APP
      try {
        base.GetProperties (_properties);
      } catch (NotImplementedException) {
        FallbackGetProperties (_properties);
      }
#else
      FallbackGetProperties (_properties);
#endif

      if (String.IsNullOrEmpty(cnnString) == false)
        ConnectionString = cnnString;
    }

    /// <summary>
    /// Gets/Sets the default version of the SQLite engine to instantiate.  Currently the only valid value is 3, indicating version 3 of the sqlite library.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (3)]
    public int Version
    {
      get
      {
        object value;
        TryGetValue("version", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        if (value != 3)
          throw new NotSupportedException();

        this["version"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the synchronization mode (file flushing) of the connection string.  Default is "Normal".
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Synchronous")]
    [DefaultValue (SynchronizationModes.Normal)]
    public SynchronizationModes SyncMode
    {
      get
      {
        object value;
        TryGetValue("synchronous", out value);
        if (value is string)
          return (SynchronizationModes)Enum.Parse (typeof (SynchronizationModes), value as string, true);
        else return (SynchronizationModes)value;
      }
      set
      {
        this["synchronous"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the encoding for the connection string.  The default is "False" which indicates UTF-8 encoding.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (false)]
    public bool UseUTF16Encoding
    {
      get
      {
        object value;
        TryGetValue("useutf16encoding", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["useutf16encoding"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whether or not to use connection pooling.  The default is "False"
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (false)]
    public bool Pooling
    {
      get
      {
        object value;
        TryGetValue("pooling", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["pooling"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets whethor not to store GUID's in binary format.  The default is True
    /// which saves space in the database.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (true)]
    public bool BinaryGUID
    {
      get
      {
        object value;
        TryGetValue("binaryguid", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["binaryguid"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the filename to open on the connection string.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Data Source")]
    [DefaultValue ("")]
    public string DataSource
    {
      get
      {
        object value;
        TryGetValue("data source", out value);
        return value.ToString();
      }
      set
      {
        this["data source"] = value;
      }
    }

    /// <summary>
    /// An alternate to the data source property
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (false)]
#endif
    public string Uri
    {
      get
      {
        object value;
        TryGetValue("uri", out value);
        return value.ToString();
      }
      set
      {
        this["uri"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the default command timeout for newly-created commands.  This is especially useful for 
    /// commands used internally such as inside a SqliteTransaction, where setting the timeout is not possible.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Default Timeout")]
    [DefaultValue (30)]
    public int DefaultTimeout
    {
      get
      {
        object value;
        TryGetValue("default timeout", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["default timeout"] = value;
      }
    }

    /// <summary>
    /// Determines whether or not the connection will automatically participate
    /// in the current distributed transaction (if one exists)
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (true)]
    public bool Enlist
    {
      get
      {
        object value;
        TryGetValue("enlist", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["enlist"] = value;
      }
    }

    /// <summary>
    /// If set to true, will throw an exception if the database specified in the connection
    /// string does not exist.  If false, the database will be created automatically.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (false)]
    public bool FailIfMissing
    {
      get
      {
        object value;
        TryGetValue("failifmissing", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["failifmissing"] = value;
      }
    }

    /// <summary>
    /// If enabled, uses the legacy 3.xx format for maximum compatibility, but results in larger
    /// database sizes.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Legacy Format")]
    [DefaultValue (false)]
    public bool LegacyFormat
    {
      get
      {
        object value;
        TryGetValue("legacy format", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["legacy format"] = value;
      }
    }

    /// <summary>
    /// When enabled, the database will be opened for read-only access and writing will be disabled.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Read Only")]
    [DefaultValue (false)]
    public bool ReadOnly
    {
      get
      {
        object value;
        TryGetValue("read only", out value);
        return SqliteConvert.ToBoolean(value);
      }
      set
      {
        this["read only"] = value;
      }
    }

    /// <summary>
    /// Gets/sets the database encryption password
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
    [PasswordPropertyText (true)]
#endif
    [DefaultValue ("")]
    public string Password
    {
      get
      {
        object value;
        TryGetValue("password", out value);
        return value.ToString();
      }
      set
      {
        this["password"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the page size for the connection.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Page Size")]
    [DefaultValue (1024)]
    public int PageSize
    {
      get
      {
        object value;
        TryGetValue("page size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["page size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the maximum number of pages the database may hold
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Max Page Count")]
    [DefaultValue (0)]
    public int MaxPageCount
    {
      get
      {
        object value;
        TryGetValue("max page count", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["max page count"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the cache size for the connection.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DisplayName ("Cache Size")]
    [DefaultValue (2000)]
    public int CacheSize
    {
      get
      {
        object value;
        TryGetValue("cache size", out value);
        return Convert.ToInt32(value, CultureInfo.CurrentCulture);
      }
      set
      {
        this["cache size"] = value;
      }
    }

    /// <summary>
    /// Gets/Sets the datetime format for the connection.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (SQLiteDateFormats.ISO8601)]
    public SQLiteDateFormats DateTimeFormat
    {
      get
      {
        object value;
        TryGetValue("datetimeformat", out value);
        if (value is string)
          return (SQLiteDateFormats)Enum.Parse (typeof (SQLiteDateFormats), value as string, true);
        else return (SQLiteDateFormats)value;
      }
      set
      {
        this["datetimeformat"] = value;
      }
    }

    /// <summary>
    /// Determines how SQLite handles the transaction journal file.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (SQLiteJournalModeEnum.Delete)]
    [DisplayName ("Journal Mode")]
    public SQLiteJournalModeEnum JournalMode
    {
      get
      {
        object value;
        TryGetValue("journal mode", out value);
        if (value is string)
          return (SQLiteJournalModeEnum)Enum.Parse (typeof (SQLiteJournalModeEnum), value as string, true);
        else
          return (SQLiteJournalModeEnum)value;
      }
      set
      {
        this["journal mode"] = value;
      }
    }

    /// <summary>
    /// Sets the default isolation level for transactions on the connection.
    /// </summary>
#if !WINDOWS_STORE_APP
    [Browsable (true)]
#endif
    [DefaultValue (IsolationLevel.Serializable)]
    [DisplayName ("Default Isolation Level")]
    public IsolationLevel DefaultIsolationLevel
    {
      get
      {
        object value;
        TryGetValue("default isolationlevel", out value);
        if (value is string)
          return (IsolationLevel)Enum.Parse (typeof (IsolationLevel), value as string, true);
        else
          return (IsolationLevel)value;
      }
      set
      {
        this["default isolationlevel"] = value;
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

      PropertyInfo pd = _properties [keyword] as PropertyInfo;

      if (pd == null) return b;

      // Attempt to coerce the value into something more solid
      if (b)
      {
        if (pd.PropertyType == typeof(Boolean))
          value = SqliteConvert.ToBoolean(value);
        else
          value = Convert.ChangeType (value, pd.PropertyType);
      }
      else
      {
        DefaultValueAttribute[] defaultValueAttributes = (DefaultValueAttribute[]) pd.GetCustomAttributes (typeof (DefaultValueAttribute), false);
        DefaultValueAttribute att = defaultValueAttributes.Length > 0 ? defaultValueAttributes [0] : null;
        if (att != null)
        {
          value = att.Value;
          b = true;
        }
      }
      return b;
    }

    /// <summary>
    /// Fallback method for MONO, which doesn't implement DbConnectionStringBuilder.GetProperties()
    /// </summary>
    /// <param name="propertyList">The hashtable to fill with property descriptors</param>
    private void FallbackGetProperties(Hashtable propertyList)
    {
      foreach (PropertyInfo property in this.GetType ().GetProperties ()) {
        DisplayNameAttribute[] displayAttributes = (DisplayNameAttribute[]) property.GetCustomAttributes (typeof (DisplayNameAttribute), false);
        string displayName = displayAttributes.Length > 0 ? displayAttributes [0].DisplayName : property.Name;
        if (property.Name != "ConnectionString" && propertyList.ContainsKey (displayName) == false) {
          propertyList.Add (displayName, property);
        }
      }
    }
  }
#endif
}
