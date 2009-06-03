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
  using System.Reflection;

  /// <summary>
  /// SQLite implementation of DbParameterCollection.
  /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
  [Editor("Microsoft.VSDesigner.Data.Design.DBParametersEditor, Microsoft.VSDesigner, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ListBindable(false)]
#endif
  public sealed class SqliteParameterCollection : DbParameterCollection
  {
    /// <summary>
    /// The underlying command to which this collection belongs
    /// </summary>
    private SqliteCommand         _command;
    /// <summary>
    /// The internal array of parameters in this collection
    /// </summary>
    private List<SqliteParameter> _parameterList;
    /// <summary>
    /// Determines whether or not all parameters have been bound to their statement(s)
    /// </summary>
    private bool                  _unboundFlag;

    /// <summary>
    /// Initializes the collection
    /// </summary>
    /// <param name="cmd">The command to which the collection belongs</param>
    internal SqliteParameterCollection(SqliteCommand cmd)
    {
      _command = cmd;
      _parameterList = new List<SqliteParameter>();
      _unboundFlag = true;
    }

    /// <summary>
    /// Returns true
    /// </summary>
    public override bool IsSynchronized
    {
      get { return true; }
    }

    /// <summary>
    /// Returns false
    /// </summary>
    public override bool IsFixedSize
    {
      get { return false; }
    }

    /// <summary>
    /// Returns false
    /// </summary>
    public override bool IsReadOnly
    {
      get { return false; }
    }

    /// <summary>
    /// Returns null
    /// </summary>
    public override object SyncRoot
    {
      get { return null; }
    }

    /// <summary>
    /// Retrieves an enumerator for the collection
    /// </summary>
    /// <returns>An enumerator for the underlying array</returns>
    public override System.Collections.IEnumerator GetEnumerator()
    {
      return _parameterList.GetEnumerator();
    }

    /// <summary>
    /// Adds a parameter to the collection
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the value</param>
    /// <param name="sourceColumn">The source column</param>
    /// <returns>A SqliteParameter object</returns>
    public SqliteParameter Add(string parameterName, DbType parameterType, int parameterSize, string sourceColumn)
    {
      SqliteParameter param = new SqliteParameter(parameterName, parameterType, parameterSize, sourceColumn);
      Add(param);

      return param;
    }

    /// <summary>
    /// Adds a parameter to the collection
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="parameterType">The data type</param>
    /// <param name="parameterSize">The size of the value</param>
    /// <returns>A SqliteParameter object</returns>
    public SqliteParameter Add(string parameterName, DbType parameterType, int parameterSize)
    {
      SqliteParameter param = new SqliteParameter(parameterName, parameterType, parameterSize);
      Add(param);

      return param;
    }

    /// <summary>
    /// Adds a parameter to the collection
    /// </summary>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="parameterType">The data type</param>
    /// <returns>A SqliteParameter object</returns>
    public SqliteParameter Add(string parameterName, DbType parameterType)
    {
      SqliteParameter param = new SqliteParameter(parameterName, parameterType);
      Add(param);

      return param;
    }

    /// <summary>
    /// Adds a parameter to the collection
    /// </summary>
    /// <param name="parameter">The parameter to add</param>
    /// <returns>A zero-based index of where the parameter is located in the array</returns>
    public int Add(SqliteParameter parameter)
    {
      int n = -1;

      if (String.IsNullOrEmpty(parameter.ParameterName) == false)
      {
        n = IndexOf(parameter.ParameterName);
      }

      if (n == -1)
      {
        n = _parameterList.Count;
        _parameterList.Add((SqliteParameter)parameter);
      }

      SetParameter(n, parameter);

      return n;
    }

    /// <summary>
    /// Adds a parameter to the collection
    /// </summary>
    /// <param name="value">The parameter to add</param>
    /// <returns>A zero-based index of where the parameter is located in the array</returns>
#if !PLATFORM_COMPACTFRAMEWORK
    [EditorBrowsable(EditorBrowsableState.Never)]
#endif
    public override int Add(object value)
    {
      return Add((SqliteParameter)value);
    }

    /// <summary>
    /// Adds a named/unnamed parameter and its value to the parameter collection.
    /// </summary>
    /// <param name="parameterName">Name of the parameter, or null to indicate an unnamed parameter</param>
    /// <param name="value">The initial value of the parameter</param>
    /// <returns>Returns the SqliteParameter object created during the call.</returns>
    public SqliteParameter AddWithValue(string parameterName, object value)
    {
      SqliteParameter param = new SqliteParameter(parameterName, value);
      Add(param);

      return param;
    }

    /// <summary>
    /// Adds an array of parameters to the collection
    /// </summary>
    /// <param name="values">The array of parameters to add</param>
    public void AddRange(SqliteParameter[] values)
    {
      int x = values.Length;
      for (int n = 0; n < x; n++)
        Add(values[n]);
    }

    /// <summary>
    /// Adds an array of parameters to the collection
    /// </summary>
    /// <param name="values">The array of parameters to add</param>
    public override void AddRange(Array values)
    {
      int x = values.Length;
      for (int n = 0; n < x; n++)
        Add((SqliteParameter)(values.GetValue(n)));
    }

    /// <summary>
    /// Clears the array and resets the collection
    /// </summary>
    public override void Clear()
    {
      _unboundFlag = true;
      _parameterList.Clear();
    }

    /// <summary>
    /// Determines if the named parameter exists in the collection
    /// </summary>
    /// <param name="parameterName">The name of the parameter to check</param>
    /// <returns>True if the parameter is in the collection</returns>
    public override bool Contains(string parameterName)
    {
      return (IndexOf(parameterName) != -1);
    }

    /// <summary>
    /// Determines if the parameter exists in the collection
    /// </summary>
    /// <param name="value">The SqliteParameter to check</param>
    /// <returns>True if the parameter is in the collection</returns>
    public override bool Contains(object value)
    {
      return _parameterList.Contains((SqliteParameter)value);
    }

    /// <summary>
    /// Not implemented
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    public override void CopyTo(Array array, int index)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Returns a count of parameters in the collection
    /// </summary>
    public override int Count
    {
      get { return _parameterList.Count; }
    }

    /// <summary>
    /// Overloaded to specialize the return value of the default indexer
    /// </summary>
    /// <param name="parameterName">Name of the parameter to get/set</param>
    /// <returns>The specified named SQLite parameter</returns>
    public new SqliteParameter this[string parameterName]
    {
      get
      {
        return (SqliteParameter)GetParameter(parameterName);
      }
      set
      {
        SetParameter(parameterName, value);
      }
    }

    /// <summary>
    /// Overloaded to specialize the return value of the default indexer
    /// </summary>
    /// <param name="index">The index of the parameter to get/set</param>
    /// <returns>The specified SQLite parameter</returns>
    public new SqliteParameter this[int index]
    {
      get
      {
        return (SqliteParameter)GetParameter(index);
      }
      set
      {
        SetParameter(index, value);
      }
    }
    /// <summary>
    /// Retrieve a parameter by name from the collection
    /// </summary>
    /// <param name="parameterName">The name of the parameter to fetch</param>
    /// <returns>A DbParameter object</returns>
    protected override DbParameter GetParameter(string parameterName)
    {
      return GetParameter(IndexOf(parameterName));
    }

    /// <summary>
    /// Retrieves a parameter by its index in the collection
    /// </summary>
    /// <param name="index">The index of the parameter to retrieve</param>
    /// <returns>A DbParameter object</returns>
    protected override DbParameter GetParameter(int index)
    {
      return _parameterList[index];
    }

    /// <summary>
    /// Returns the index of a parameter given its name
    /// </summary>
    /// <param name="parameterName">The name of the parameter to find</param>
    /// <returns>-1 if not found, otherwise a zero-based index of the parameter</returns>
    public override int IndexOf(string parameterName)
    {
      int x = _parameterList.Count;
      for (int n = 0; n < x; n++)
      {
        if (String.Compare(parameterName, _parameterList[n].ParameterName, true, CultureInfo.InvariantCulture) == 0)
          return n;
      }
      return -1;
    }

    /// <summary>
    /// Returns the index of a parameter
    /// </summary>
    /// <param name="value">The parameter to find</param>
    /// <returns>-1 if not found, otherwise a zero-based index of the parameter</returns>
    public override int IndexOf(object value)
    {
      return _parameterList.IndexOf((SqliteParameter)value);
    }

    /// <summary>
    /// Inserts a parameter into the array at the specified location
    /// </summary>
    /// <param name="index">The zero-based index to insert the parameter at</param>
    /// <param name="value">The parameter to insert</param>
    public override void Insert(int index, object value)
    {
      _unboundFlag = true;
      _parameterList.Insert(index, (SqliteParameter)value);
    }

    /// <summary>
    /// Removes a parameter from the collection
    /// </summary>
    /// <param name="value">The parameter to remove</param>
    public override void Remove(object value)
    {
      _unboundFlag = true;
      _parameterList.Remove((SqliteParameter)value);
    }

    /// <summary>
    /// Removes a parameter from the collection given its name
    /// </summary>
    /// <param name="parameterName">The name of the parameter to remove</param>
    public override void RemoveAt(string parameterName)
    {
      RemoveAt(IndexOf(parameterName));
    }

    /// <summary>
    /// Removes a parameter from the collection given its index
    /// </summary>
    /// <param name="index">The zero-based parameter index to remove</param>
    public override void RemoveAt(int index)
    {
      _unboundFlag = true;
      _parameterList.RemoveAt(index);
    }

    /// <summary>
    /// Re-assign the named parameter to a new parameter object
    /// </summary>
    /// <param name="parameterName">The name of the parameter to replace</param>
    /// <param name="value">The new parameter</param>
    protected override void SetParameter(string parameterName, DbParameter value)
    {
      SetParameter(IndexOf(parameterName), value);
    }

    /// <summary>
    /// Re-assign a parameter at the specified index
    /// </summary>
    /// <param name="index">The zero-based index of the parameter to replace</param>
    /// <param name="value">The new parameter</param>
    protected override void SetParameter(int index, DbParameter value)
    {
      _unboundFlag = true;
      _parameterList[index] = (SqliteParameter)value;
    }

    /// <summary>
    /// Un-binds all parameters from their statements
    /// </summary>
    internal void Unbind()
    {
      _unboundFlag = true;
    }

    /// <summary>
    /// This function attempts to map all parameters in the collection to all statements in a Command.
    /// Since named parameters may span multiple statements, this function makes sure all statements are bound
    /// to the same named parameter.  Unnamed parameters are bound in sequence.
    /// </summary>
    internal void MapParameters(SqliteStatement activeStatement)
    {
      if (_unboundFlag == false || _parameterList.Count == 0 || _command._statementList == null) return;

      int nUnnamed = 0;
      string s;
      int n;
      int y = -1;
      SqliteStatement stmt;

      foreach(SqliteParameter p in _parameterList)
      {
        y ++;
        s = p.ParameterName;
        if (s == null)
        {
          s = String.Format(CultureInfo.InvariantCulture, ";{0}", nUnnamed);
          nUnnamed++;
        }

        int x;
        bool isMapped = false;

        if (activeStatement == null)
          x = _command._statementList.Count;
        else
          x = 1;

        stmt = activeStatement;
        for (n = 0; n < x; n++)
        {
          isMapped = false;
          if (stmt == null) stmt = _command._statementList[n];
          if (stmt._paramNames != null)
          {
            if (stmt.MapParameter(s, p) == true)
              isMapped = true;
          }
          stmt = null;
        }

        // If the parameter has a name, but the SQL statement uses unnamed references, this can happen -- attempt to map
        // the parameter by its index in the collection
        if (isMapped == false)
        {
          s = String.Format(CultureInfo.InvariantCulture, ";{0}", y);

          stmt = activeStatement;
          for (n = 0; n < x; n++)
          {
            if (stmt == null) stmt = _command._statementList[n];
            if (stmt._paramNames != null)
            {
              if (stmt.MapParameter(s, p) == true)
                isMapped = true;
            }
            stmt = null;
          }
        }
      }
      if (activeStatement == null) _unboundFlag = false;
    }
  }
}
