//
// Mono.Data.Sqlite.SQLiteFunctionAttribute.cs
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
  using System.Runtime.InteropServices;

  /// <summary>
  /// A simple custom attribute to enable us to easily find user-defined functions in
  /// the loaded assemblies and initialize them in Sqlite as connections are made.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
  public class SqliteFunctionAttribute : Attribute
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
    /// The function's name as it will be used in Sqlite command text.
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
#endif
