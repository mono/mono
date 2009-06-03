/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Runtime.InteropServices;

  /// <summary>
  /// A simple custom attribute to enable us to easily find user-defined functions in
  /// the loaded assemblies and initialize them in SQLite as connections are made.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public sealed class SqliteFunctionAttribute : Attribute
  {
    private string       _name;
    private int          _arguments;
    private FunctionType _functionType;
    internal Type        _instanceType;

    /// <summary>
    /// Default constructor, initializes the internal variables for the function.
    /// </summary>
    public SqliteFunctionAttribute()
    {
      Name = "";
      Arguments = -1;
      FuncType = FunctionType.Scalar;
    }

    /// <summary>
    /// The function's name as it will be used in SQLite command text.
    /// </summary>
    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    /// <summary>
    /// The number of arguments this function expects.  -1 if the number of arguments is variable.
    /// </summary>
    public int Arguments
    {
      get { return _arguments; }
      set { _arguments = value; }
    }

    /// <summary>
    /// The type of function this implementation will be.
    /// </summary>
    public FunctionType FuncType
    {
      get { return _functionType; }
      set { _functionType = value; }
    }
  }
}
