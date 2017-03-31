/********************************************************
 * ADO.NET 2.0 Data Provider for SQLite Version 3.X
 * Written by Robert Simpson (robert@blackcastlesoft.com)
 * 
 * Released to the public domain, use at your own risk!
 ********************************************************/

namespace Mono.Data.Sqlite
{
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Runtime.InteropServices;
  using System.Globalization;

  /// <summary>
  /// This abstract class is designed to handle user-defined functions easily.  An instance of the derived class is made for each
  /// connection to the database.
  /// </summary>
  /// <remarks>
  /// Although there is one instance of a class derived from SqliteFunction per database connection, the derived class has no access
  /// to the underlying connection.  This is necessary to deter implementers from thinking it would be a good idea to make database
  /// calls during processing.
  /// 
  /// It is important to distinguish between a per-connection instance, and a per-SQL statement context.  One instance of this class
  /// services all SQL statements being stepped through on that connection, and there can be many.  One should never store per-statement
  /// information in member variables of user-defined function classes.
  /// 
  /// For aggregate functions, always create and store your per-statement data in the contextData object on the 1st step.  This data will
  /// be automatically freed for you (and Dispose() called if the item supports IDisposable) when the statement completes.
  /// </remarks>
  public abstract class SqliteFunction : IDisposable
  {
    private class AggregateData
    {
      internal int _count = 1;
      internal object _data = null;
    }

    /// <summary>
    /// The base connection this function is attached to
    /// </summary>
    internal SQLiteBase              _base;

    /// <summary>
    /// Internal array used to keep track of aggregate function context data
    /// </summary>
    private Dictionary<long, AggregateData> _contextDataList;

    /// <summary>
    /// Holds a reference to the callback function for user functions
    /// </summary>
    private SQLiteCallback  _InvokeFunc;
    /// <summary>
    /// Holds a reference to the callbakc function for stepping in an aggregate function
    /// </summary>
    private SQLiteCallback  _StepFunc;
    /// <summary>
    /// Holds a reference to the callback function for finalizing an aggregate function
    /// </summary>
    private SQLiteFinalCallback  _FinalFunc;
    /// <summary>
    /// Holds a reference to the callback function for collation sequences
    /// </summary>
    private SQLiteCollation _CompareFunc;

    private SQLiteCollation _CompareFunc16;

    /// <summary>
    /// Current context of the current callback.  Only valid during a callback
    /// </summary>
    internal IntPtr _context;

    /// <summary>
    /// This static list contains all the user-defined functions declared using the proper attributes.
    /// </summary>
    private static List<SqliteFunctionAttribute> _registeredFunctions = new List<SqliteFunctionAttribute>();

    /// <summary>
    /// Internal constructor, initializes the function's internal variables.
    /// </summary>
    protected SqliteFunction()
    {
      _contextDataList = new Dictionary<long, AggregateData>();
    }

    /// <summary>
    /// Returns a reference to the underlying connection's SqliteConvert class, which can be used to convert
    /// strings and DateTime's into the current connection's encoding schema.
    /// </summary>
    public SqliteConvert SqliteConvert
    {
      get
      {
        return _base;
      }
    }

    /// <summary>
    /// Scalar functions override this method to do their magic.
    /// </summary>
    /// <remarks>
    /// Parameters passed to functions have only an affinity for a certain data type, there is no underlying schema available
    /// to force them into a certain type.  Therefore the only types you will ever see as parameters are
    /// DBNull.Value, Int64, Double, String or byte[] array.
    /// </remarks>
    /// <param name="args">The arguments for the command to process</param>
    /// <returns>You may return most simple types as a return value, null or DBNull.Value to return null, DateTime, or
    /// you may return an Exception-derived class if you wish to return an error to SQLite.  Do not actually throw the error,
    /// just return it!</returns>
    public virtual object Invoke(object[] args)
    {
      return null;
    }

    /// <summary>
    /// Aggregate functions override this method to do their magic.
    /// </summary>
    /// <remarks>
    /// Typically you'll be updating whatever you've placed in the contextData field and returning as quickly as possible.
    /// </remarks>
    /// <param name="args">The arguments for the command to process</param>
    /// <param name="stepNumber">The 1-based step number.  This is incrememted each time the step method is called.</param>
    /// <param name="contextData">A placeholder for implementers to store contextual data pertaining to the current context.</param>
    public virtual void Step(object[] args, int stepNumber, ref object contextData)
    {
    }

    /// <summary>
    /// Aggregate functions override this method to finish their aggregate processing.
    /// </summary>
    /// <remarks>
    /// If you implemented your aggregate function properly,
    /// you've been recording and keeping track of your data in the contextData object provided, and now at this stage you should have
    /// all the information you need in there to figure out what to return.
    /// NOTE:  It is possible to arrive here without receiving a previous call to Step(), in which case the contextData will
    /// be null.  This can happen when no rows were returned.  You can either return null, or 0 or some other custom return value
    /// if that is the case.
    /// </remarks>
    /// <param name="contextData">Your own assigned contextData, provided for you so you can return your final results.</param>
    /// <returns>You may return most simple types as a return value, null or DBNull.Value to return null, DateTime, or
    /// you may return an Exception-derived class if you wish to return an error to SQLite.  Do not actually throw the error,
    /// just return it!
    /// </returns>
    public virtual object Final(object contextData)
    {
      return null;
    }

    /// <summary>
    /// User-defined collation sequences override this method to provide a custom string sorting algorithm.
    /// </summary>
    /// <param name="param1">The first string to compare</param>
    /// <param name="param2">The second strnig to compare</param>
    /// <returns>1 if param1 is greater than param2, 0 if they are equal, or -1 if param1 is less than param2</returns>
    public virtual int Compare(string param1, string param2)
    {
      return 0;
    }

    /// <summary>
    /// Converts an IntPtr array of context arguments to an object array containing the resolved parameters the pointers point to.
    /// </summary>
    /// <remarks>
    /// Parameters passed to functions have only an affinity for a certain data type, there is no underlying schema available
    /// to force them into a certain type.  Therefore the only types you will ever see as parameters are
    /// DBNull.Value, Int64, Double, String or byte[] array.
    /// </remarks>
    /// <param name="nArgs">The number of arguments</param>
    /// <param name="argsptr">A pointer to the array of arguments</param>
    /// <returns>An object array of the arguments once they've been converted to .NET values</returns>
    internal object[] ConvertParams(int nArgs, IntPtr argsptr)
    {
      object[] parms = new object[nArgs];
#if !PLATFORM_COMPACTFRAMEWORK
      IntPtr[] argint = new IntPtr[nArgs];
#else
      int[] argint = new int[nArgs];
#endif
      Marshal.Copy(argsptr, argint, 0, nArgs);

      for (int n = 0; n < nArgs; n++)
      {
        switch (_base.GetParamValueType((IntPtr)argint[n]))
        {
          case TypeAffinity.Null:
            parms[n] = DBNull.Value;
            break;
          case TypeAffinity.Int64:
            parms[n] = _base.GetParamValueInt64((IntPtr)argint[n]);
            break;
          case TypeAffinity.Double:
            parms[n] = _base.GetParamValueDouble((IntPtr)argint[n]);
            break;
          case TypeAffinity.Text:
            parms[n] = _base.GetParamValueText((IntPtr)argint[n]);
            break;
          case TypeAffinity.Blob:
            {
              int x;
              byte[] blob;

              x = (int)_base.GetParamValueBytes((IntPtr)argint[n], 0, null, 0, 0);
              blob = new byte[x];
              _base.GetParamValueBytes((IntPtr)argint[n], 0, blob, 0, x);
              parms[n] = blob;
            }
            break;
          case TypeAffinity.DateTime: // Never happens here but what the heck, maybe it will one day.
            parms[n] = _base.ToDateTime(_base.GetParamValueText((IntPtr)argint[n]));
            break;
        }
      }
      return parms;
    }

    /// <summary>
    /// Takes the return value from Invoke() and Final() and figures out how to return it to SQLite's context.
    /// </summary>
    /// <param name="context">The context the return value applies to</param>
    /// <param name="returnValue">The parameter to return to SQLite</param>
    void SetReturnValue(IntPtr context, object returnValue)
    {
      if (returnValue == null || returnValue == DBNull.Value)
      {
        _base.ReturnNull(context);
        return;
      }

      Type t = returnValue.GetType();
      if (t == typeof(DateTime))
      {
        _base.ReturnText(context, _base.ToString((DateTime)returnValue));
        return;
      }
      else
      {
        Exception r = returnValue as Exception;

        if (r != null)
        {
          _base.ReturnError(context, r.Message);
          return;
        }
      }

      switch (SqliteConvert.TypeToAffinity(t))
      {
        case TypeAffinity.Null:
          _base.ReturnNull(context);
          return;
        case TypeAffinity.Int64:
          _base.ReturnInt64(context, Convert.ToInt64(returnValue, CultureInfo.CurrentCulture));
          return;
        case TypeAffinity.Double:
          _base.ReturnDouble(context, Convert.ToDouble(returnValue, CultureInfo.CurrentCulture));
          return;
        case TypeAffinity.Text:
          _base.ReturnText(context, returnValue.ToString());
          return;
        case TypeAffinity.Blob:
          _base.ReturnBlob(context, (byte[])returnValue);
          return;
      }
    }

    /// <summary>
    /// Internal scalar callback function, which wraps the raw context pointer and calls the virtual Invoke() method.
    /// </summary>
    /// <param name="context">A raw context pointer</param>
    /// <param name="nArgs">Number of arguments passed in</param>
    /// <param name="argsptr">A pointer to the array of arguments</param>
    internal void ScalarCallback(IntPtr context, int nArgs, IntPtr argsptr)
    {
      _context = context;
      SetReturnValue(context, Invoke(ConvertParams(nArgs, argsptr)));
    }

    /// <summary>
    /// Internal collation sequence function, which wraps up the raw string pointers and executes the Compare() virtual function.
    /// </summary>
    /// <param name="ptr">Not used</param>
    /// <param name="len1">Length of the string pv1</param>
    /// <param name="ptr1">Pointer to the first string to compare</param>
    /// <param name="len2">Length of the string pv2</param>
    /// <param name="ptr2">Pointer to the second string to compare</param>
    /// <returns>Returns -1 if the first string is less than the second.  0 if they are equal, or 1 if the first string is greater
    /// than the second.</returns>
    internal int CompareCallback(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
    {
      return Compare(SqliteConvert.UTF8ToString(ptr1, len1), SqliteConvert.UTF8ToString(ptr2, len2));
    }

    internal int CompareCallback16(IntPtr ptr, int len1, IntPtr ptr1, int len2, IntPtr ptr2)
    {
      return Compare(SQLite3_UTF16.UTF16ToString(ptr1, len1), SQLite3_UTF16.UTF16ToString(ptr2, len2));
    }

    /// <summary>
    /// The internal aggregate Step function callback, which wraps the raw context pointer and calls the virtual Step() method.
    /// </summary>
    /// <remarks>
    /// This function takes care of doing the lookups and getting the important information put together to call the Step() function.
    /// That includes pulling out the user's contextData and updating it after the call is made.  We use a sorted list for this so
    /// binary searches can be done to find the data.
    /// </remarks>
    /// <param name="context">A raw context pointer</param>
    /// <param name="nArgs">Number of arguments passed in</param>
    /// <param name="argsptr">A pointer to the array of arguments</param>
    internal void StepCallback(IntPtr context, int nArgs, IntPtr argsptr)
    {
      long nAux;
      AggregateData data;

      nAux = (long)_base.AggregateContext(context);
      if (_contextDataList.TryGetValue(nAux, out data) == false)
      {
        data = new AggregateData();
        _contextDataList[nAux] = data;
      }

      try
      {
        _context = context;
        Step(ConvertParams(nArgs, argsptr), data._count, ref data._data);
      }
      finally
      {
        data._count++;
      }
    }

    /// <summary>
    /// An internal aggregate Final function callback, which wraps the context pointer and calls the virtual Final() method.
    /// </summary>
    /// <param name="context">A raw context pointer</param>
    internal void FinalCallback(IntPtr context)
    {
      long n = (long)_base.AggregateContext(context);
      object obj = null;

      if (_contextDataList.ContainsKey(n))
      {
        obj = _contextDataList[n]._data;
        _contextDataList.Remove(n);
      }

      _context = context;
      SetReturnValue(context, Final(obj));

      IDisposable disp = obj as IDisposable;
      if (disp != null) disp.Dispose();
    }

    /// <summary>
    /// Placeholder for a user-defined disposal routine
    /// </summary>
    /// <param name="disposing">True if the object is being disposed explicitly</param>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        IDisposable disp;

        foreach (KeyValuePair<long, AggregateData> kv in _contextDataList)
        {
          disp = kv.Value._data as IDisposable;
          if (disp != null)
            disp.Dispose();
        }
        _contextDataList.Clear();

        _InvokeFunc = null;
        _StepFunc = null;
        _FinalFunc = null;
        _CompareFunc = null;
        _base = null;
        _contextDataList = null;
      }
    }

    /// <summary>
    /// Disposes of any active contextData variables that were not automatically cleaned up.  Sometimes this can happen if
    /// someone closes the connection while a DataReader is open.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
    }

    /// <summary>
    /// Using reflection, enumerate all assemblies in the current appdomain looking for classes that
    /// have a SqliteFunctionAttribute attribute, and registering them accordingly.
    /// </summary>
#if !PLATFORM_COMPACTFRAMEWORK
    [global::System.Security.Permissions.FileIOPermission(global::System.Security.Permissions.SecurityAction.Assert, AllFiles = global::System.Security.Permissions.FileIOPermissionAccess.PathDiscovery)]
#endif
    static SqliteFunction()
    {
      try
      {
#if !PLATFORM_COMPACTFRAMEWORK
        SqliteFunctionAttribute at;
        System.Reflection.Assembly[] arAssemblies = System.AppDomain.CurrentDomain.GetAssemblies();
        int w = arAssemblies.Length;
        System.Reflection.AssemblyName sqlite = System.Reflection.Assembly.GetCallingAssembly().GetName();

        for (int n = 0; n < w; n++)
        {
          Type[] arTypes;
          bool found = false;
          System.Reflection.AssemblyName[] references;
          try
          {
            // Inspect only assemblies that reference SQLite
            references = arAssemblies[n].GetReferencedAssemblies();
            int t = references.Length;
            for (int z = 0; z < t; z++)
            {
              if (references[z].Name == sqlite.Name)
              {
                found = true;
                break;
              }
            }

            if (found == false)
              continue;

            arTypes = arAssemblies[n].GetTypes();
          }
          catch (global::System.Reflection.ReflectionTypeLoadException e)
          {
            arTypes = e.Types;
          }

          int v = arTypes.Length;
          for (int x = 0; x < v; x++)
          {
            if (arTypes[x] == null) continue;

            object[] arAtt = arTypes[x].GetCustomAttributes(typeof(SqliteFunctionAttribute), false);
            int u = arAtt.Length;
            for (int y = 0; y < u; y++)
            {
              at = arAtt[y] as SqliteFunctionAttribute;
              if (at != null)
              {
                at._instanceType = arTypes[x];
                _registeredFunctions.Add(at);
              }
            }
          }
        }
#endif
      }
      catch // SQLite provider can continue without being able to find built-in functions
      {
      }
    }
    /// <summary>
    /// Manual method of registering a function.  The type must still have the SqliteFunctionAttributes in order to work
    /// properly, but this is a workaround for the Compact Framework where enumerating assemblies is not currently supported.
    /// </summary>
    /// <param name="typ">The type of the function to register</param>
    public static void RegisterFunction(Type typ)
    {
      object[] arAtt = typ.GetCustomAttributes(typeof(SqliteFunctionAttribute), false);
      int u = arAtt.Length;
      SqliteFunctionAttribute at;

      for (int y = 0; y < u; y++)
      {
        at = arAtt[y] as SqliteFunctionAttribute;
        if (at != null)
        {
          at._instanceType = typ;
          _registeredFunctions.Add(at);
        }
      }
    }

    /// <summary>
    /// Called by SQLiteBase derived classes, this function binds all user-defined functions to a connection.
    /// It is done this way so that all user-defined functions will access the database using the same encoding scheme
    /// as the connection (UTF-8 or UTF-16).
    /// </summary>
    /// <remarks>
    /// The wrapper functions that interop with SQLite will create a unique cookie value, which internally is a pointer to
    /// all the wrapped callback functions.  The interop function uses it to map CDecl callbacks to StdCall callbacks.
    /// </remarks>
    /// <param name="sqlbase">The base object on which the functions are to bind</param>
    /// <returns>Returns an array of functions which the connection object should retain until the connection is closed.</returns>
    internal static SqliteFunction[] BindFunctions(SQLiteBase sqlbase)
    {
      SqliteFunction f;
      List<SqliteFunction> lFunctions = new List<SqliteFunction>();

      foreach (SqliteFunctionAttribute pr in _registeredFunctions)
      {
        f = (SqliteFunction)Activator.CreateInstance(pr._instanceType);
        f._base = sqlbase;
        f._InvokeFunc = (pr.FuncType == FunctionType.Scalar) ? new SQLiteCallback(f.ScalarCallback) : null;
        f._StepFunc = (pr.FuncType == FunctionType.Aggregate) ? new SQLiteCallback(f.StepCallback) : null;
        f._FinalFunc = (pr.FuncType == FunctionType.Aggregate) ? new SQLiteFinalCallback(f.FinalCallback) : null;
        f._CompareFunc = (pr.FuncType == FunctionType.Collation) ? new SQLiteCollation(f.CompareCallback) : null;
        f._CompareFunc16 = (pr.FuncType == FunctionType.Collation) ? new SQLiteCollation(f.CompareCallback16) : null;

        if (pr.FuncType != FunctionType.Collation)
          sqlbase.CreateFunction(pr.Name, pr.Arguments, (f is SqliteFunctionEx), f._InvokeFunc, f._StepFunc, f._FinalFunc);
        else {
#if MONOTOUCH
          GCHandle handle = GCHandle.Alloc (f);
          sqlbase.CreateCollation(pr.Name, collation_callback, collation_callback16, GCHandle.ToIntPtr (handle));
#else
          sqlbase.CreateCollation(pr.Name, f._CompareFunc, f._CompareFunc16, IntPtr.Zero);
#endif
        }


        lFunctions.Add(f);
      }

      SqliteFunction[] arFunctions = new SqliteFunction[lFunctions.Count];
      lFunctions.CopyTo(arFunctions, 0);

      return arFunctions;
    }

#if MONOTOUCH
    [Mono.Util.MonoPInvokeCallback (typeof (SQLiteCollation))]
    internal static int collation_callback (IntPtr puser, int len1, IntPtr pv1, int len2, IntPtr pv2)
    {
      var handle = GCHandle.FromIntPtr (puser);
      var func = (SqliteFunction) handle.Target;
      return func._CompareFunc (IntPtr.Zero, len1, pv1, len2, pv2);
    }

    [Mono.Util.MonoPInvokeCallback (typeof (SQLiteCollation))]
    internal static int collation_callback16 (IntPtr puser, int len1, IntPtr pv1, int len2, IntPtr pv2)
    {
      var handle = GCHandle.FromIntPtr (puser);
      var func = (SqliteFunction) handle.Target;
      return func._CompareFunc16 (IntPtr.Zero, len1, pv1, len2, pv2);
    }
#endif
  }

  /// <summary>
  /// Extends SqliteFunction and allows an inherited class to obtain the collating sequence associated with a function call.
  /// </summary>
  /// <remarks>
  /// User-defined functions can call the GetCollationSequence() method in this class and use it to compare strings and char arrays.
  /// </remarks>
  public class SqliteFunctionEx : SqliteFunction
  {
    /// <summary>
    /// Obtains the collating sequence in effect for the given function.
    /// </summary>
    /// <returns></returns>
    protected CollationSequence GetCollationSequence()
    {
      return _base.GetCollationSequence(this, _context);
    }
  }

  /// <summary>
  /// The type of user-defined function to declare
  /// </summary>
  public enum FunctionType
  {
    /// <summary>
    /// Scalar functions are designed to be called and return a result immediately.  Examples include ABS(), Upper(), Lower(), etc.
    /// </summary>
    Scalar = 0,
    /// <summary>
    /// Aggregate functions are designed to accumulate data until the end of a call and then return a result gleaned from the accumulated data.
    /// Examples include SUM(), COUNT(), AVG(), etc.
    /// </summary>
    Aggregate = 1,
    /// <summary>
    /// Collation sequences are used to sort textual data in a custom manner, and appear in an ORDER BY clause.  Typically text in an ORDER BY is
    /// sorted using a straight case-insensitive comparison function.  Custom collating sequences can be used to alter the behavior of text sorting
    /// in a user-defined manner.
    /// </summary>
    Collation = 2,
  }

  /// <summary>
  /// An internal callback delegate declaration.
  /// </summary>
  /// <param name="context">Raw context pointer for the user function</param>
  /// <param name="nArgs">Count of arguments to the function</param>
  /// <param name="argsptr">A pointer to the array of argument pointers</param>
#if !PLATFORM_COMPACTFRAMEWORK
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
  internal delegate void SQLiteCallback(IntPtr context, int nArgs, IntPtr argsptr);
  /// <summary>
  /// An internal final callback delegate declaration.
  /// </summary>
  /// <param name="context">Raw context pointer for the user function</param>
#if !PLATFORM_COMPACTFRAMEWORK
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
  internal delegate void SQLiteFinalCallback(IntPtr context);
  /// <summary>
  /// Internal callback delegate for implementing collation sequences
  /// </summary>
  /// <param name="puser">Not used</param>
  /// <param name="len1">Length of the string pv1</param>
  /// <param name="pv1">Pointer to the first string to compare</param>
  /// <param name="len2">Length of the string pv2</param>
  /// <param name="pv2">Pointer to the second string to compare</param>
  /// <returns>Returns -1 if the first string is less than the second.  0 if they are equal, or 1 if the first string is greater
  /// than the second.</returns>
#if !PLATFORM_COMPACTFRAMEWORK
  [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
#endif
  internal delegate int SQLiteCollation(IntPtr puser, int len1, IntPtr pv1, int len2, IntPtr pv2);

  /// <summary>
  /// The type of collating sequence
  /// </summary>
  public enum CollationTypeEnum
  {
    /// <summary>
    /// The built-in BINARY collating sequence
    /// </summary>
    Binary = 1,
    /// <summary>
    /// The built-in NOCASE collating sequence
    /// </summary>
    NoCase = 2,
    /// <summary>
    /// The built-in REVERSE collating sequence
    /// </summary>
    Reverse = 3,
    /// <summary>
    /// A custom user-defined collating sequence
    /// </summary>
    Custom = 0,
  }

  /// <summary>
  /// The encoding type the collation sequence uses
  /// </summary>
  public enum CollationEncodingEnum
  {
    /// <summary>
    /// The collation sequence is UTF8
    /// </summary>
    UTF8 = 1,
    /// <summary>
    /// The collation sequence is UTF16 little-endian
    /// </summary>
    UTF16LE = 2,
    /// <summary>
    /// The collation sequence is UTF16 big-endian
    /// </summary>
    UTF16BE = 3,
  }

  /// <summary>
  /// A struct describing the collating sequence a function is executing in
  /// </summary>
  public struct CollationSequence
  {
    /// <summary>
    /// The name of the collating sequence
    /// </summary>
    public string Name;
    /// <summary>
    /// The type of collating sequence
    /// </summary>
    public CollationTypeEnum Type;

    /// <summary>
    /// The text encoding of the collation sequence
    /// </summary>
    public CollationEncodingEnum Encoding;

    /// <summary>
    /// Context of the function that requested the collating sequence
    /// </summary>
    internal SqliteFunction _func;

    /// <summary>
    /// Calls the base collating sequence to compare two strings
    /// </summary>
    /// <param name="s1">The first string to compare</param>
    /// <param name="s2">The second string to compare</param>
    /// <returns>-1 if s1 is less than s2, 0 if s1 is equal to s2, and 1 if s1 is greater than s2</returns>
    public int Compare(string s1, string s2)
    {
      return _func._base.ContextCollateCompare(Encoding, _func._context, s1, s2);
    }

    /// <summary>
    /// Calls the base collating sequence to compare two character arrays
    /// </summary>
    /// <param name="c1">The first array to compare</param>
    /// <param name="c2">The second array to compare</param>
    /// <returns>-1 if c1 is less than c2, 0 if c1 is equal to c2, and 1 if c1 is greater than c2</returns>
    public int Compare(char[] c1, char[] c2)
    {
      return _func._base.ContextCollateCompare(Encoding, _func._context, c1, c2);
    }
  }
}
