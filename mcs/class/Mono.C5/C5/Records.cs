/*
 Copyright (c) 2003-2006 Niels Kokholm and Peter Sestoft
 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:
 
 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.
 
 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 SOFTWARE.
*/

using C5;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Diagnostics;

namespace C5
{
  struct RecConst
  {
    public const int HASHFACTOR = 387281;
  }
  /// <summary>
  /// A generic record type with two fields. 
  /// <para>
  /// Equality is defined field by field, using the <code>Equals</code> method 
  /// inherited from <code>System.Object</code> (i.e. using <see cref="T:C5.NaturalEqualityComparer`1"/>).
  /// </para>
  /// <para>
  /// This type is similar to <see cref="T:C5.KeyValuePair`2"/>, but the latter
  /// uses <see cref="P:C5.EqualityComparer`1.Default"/> to define field equality instead of <see cref="T:C5.NaturalEqualityComparer`1"/>.
  /// </para>
  /// </summary>
  /// <typeparam name="T1"></typeparam>
  /// <typeparam name="T2"></typeparam>
  public struct Rec<T1, T2> : IEquatable<Rec<T1, T2>>, IShowable
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly T1 X1;
    /// <summary>
    /// 
    /// </summary>
    public readonly T2 X2;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    [Tested]
    public Rec(T1 x1, T2 x2)
    {
      this.X1 = x1; this.X2 = x2;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Tested]
    public bool Equals(Rec<T1, T2> other)
    {
      return
        (X1 == null ? other.X1 == null : X1.Equals(other.X1)) &&
        (X2 == null ? other.X2 == null : X2.Equals(other.X2))
        ;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Tested]
    public override bool Equals(object obj)
    {
      return obj is Rec<T1, T2> ? Equals((Rec<T1, T2>)obj) : false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator ==(Rec<T1, T2> record1, Rec<T1, T2> record2)
    {
      return record1.Equals(record2);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator !=(Rec<T1, T2> record1, Rec<T1, T2> record2)
    {
      return !record1.Equals(record2);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Tested]
    public override int GetHashCode()
    {
      //TODO: don't use 0 as hashcode for null, but something else!
      int hashcode = X1 == null ? 0 : X1.GetHashCode();
      hashcode = hashcode * RecConst.HASHFACTOR + (X2 == null ? 0 : X2.GetHashCode());
      return hashcode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("({0}, {1})", X1, X2);
    }

    #region IShowable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public bool Show(System.Text.StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      bool incomplete = true;
      stringbuilder.Append("(");
      rest -= 2;
      try
      {
        if (incomplete = !Showing.Show(X1, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X2, stringbuilder, ref rest, formatProvider))
          return false;
      }
      finally
      {
        if (incomplete)
        {
          stringbuilder.Append("...");
          rest -= 3;
        }
        stringbuilder.Append(")");
      }
      return true;
    }
    #endregion

    #region IFormattable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      return Showing.ShowString(this, format, formatProvider);
    }

    #endregion
  }
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T1"></typeparam>
  /// <typeparam name="T2"></typeparam>
  /// <typeparam name="T3"></typeparam>
  public struct Rec<T1, T2, T3> : IEquatable<Rec<T1, T2, T3>>, IShowable
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly T1 X1;
    /// <summary>
    /// 
    /// </summary>
    public readonly T2 X2;
    /// <summary>
    /// 
    /// </summary>
    public readonly T3 X3;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="x3"></param>
    [Tested]
    public Rec(T1 x1, T2 x2, T3 x3)
    {
      this.X1 = x1; this.X2 = x2; this.X3 = x3;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Tested]
    public bool Equals(Rec<T1, T2, T3> other)
    {
      return
        (X1 == null ? other.X1 == null : X1.Equals(other.X1)) &&
        (X2 == null ? other.X2 == null : X2.Equals(other.X2)) &&
        (X3 == null ? other.X3 == null : X3.Equals(other.X3))
        ;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Tested]
    public override bool Equals(object obj)
    {
      return obj is Rec<T1, T2, T3> ? Equals((Rec<T1, T2, T3>)obj) : false;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator ==(Rec<T1, T2, T3> record1, Rec<T1, T2, T3> record2)
    {
      return record1.Equals(record2);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator !=(Rec<T1, T2, T3> record1, Rec<T1, T2, T3> record2)
    {
      return !record1.Equals(record2);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Tested]
    public override int GetHashCode()
    {
      //TODO: don't use 0 as hashcode for null, but something else!
      int hashcode = X1 == null ? 0 : X1.GetHashCode();
      hashcode = hashcode * RecConst.HASHFACTOR + (X2 == null ? 0 : X2.GetHashCode());
      hashcode = hashcode * RecConst.HASHFACTOR + (X3 == null ? 0 : X3.GetHashCode());
      return hashcode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("({0}, {1}, {2})", X1, X2, X3);
    }
    #region IShowable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public bool Show(System.Text.StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      bool incomplete = true;
      stringbuilder.Append("(");
      rest -= 2;
      try
      {
        if (incomplete = !Showing.Show(X1, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X2, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X3, stringbuilder, ref rest, formatProvider))
          return false;
      }
      finally
      {
        if (incomplete)
        {
          stringbuilder.Append("...");
          rest -= 3;
        }
        stringbuilder.Append(")");
      }
      return true;
    }
    #endregion

    #region IFormattable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      return Showing.ShowString(this, format, formatProvider);
    }

    #endregion
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T1"></typeparam>
  /// <typeparam name="T2"></typeparam>
  /// <typeparam name="T3"></typeparam>
  /// <typeparam name="T4"></typeparam>
  public struct Rec<T1, T2, T3, T4> : IEquatable<Rec<T1, T2, T3, T4>>, IShowable
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly T1 X1;
    /// <summary>
    /// 
    /// </summary>
    public readonly T2 X2;
    /// <summary>
    /// 
    /// </summary>
    public readonly T3 X3;
    /// <summary>
    /// 
    /// </summary>
    public readonly T4 X4;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="x1"></param>
    /// <param name="x2"></param>
    /// <param name="x3"></param>
    /// <param name="x4"></param>
    [Tested]
    public Rec(T1 x1, T2 x2, T3 x3, T4 x4)
    {
      this.X1 = x1; this.X2 = x2; this.X3 = x3; this.X4 = x4;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    [Tested]
    public bool Equals(Rec<T1, T2, T3, T4> other)
    {
      return
        (X1 == null ? other.X1 == null : X1.Equals(other.X1)) &&
        (X2 == null ? other.X2 == null : X2.Equals(other.X2)) &&
        (X3 == null ? other.X3 == null : X3.Equals(other.X3)) &&
        (X4 == null ? other.X4 == null : X4.Equals(other.X4))
        ;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    [Tested]
    public override bool Equals(object obj)
    {
      return obj is Rec<T1, T2, T3, T4> ? Equals((Rec<T1, T2, T3, T4>)obj) : false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator ==(Rec<T1, T2, T3, T4> record1, Rec<T1, T2, T3, T4> record2)
    {
      return record1.Equals(record2);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="record1"></param>
    /// <param name="record2"></param>
    /// <returns></returns>
    [Tested]
    public static bool operator !=(Rec<T1, T2, T3, T4> record1, Rec<T1, T2, T3, T4> record2)
    {
      return !record1.Equals(record2);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    [Tested]
    public override int GetHashCode()
    {
      //TODO: don't use 0 as hashcode for null, but something else!
      int hashcode = X1 == null ? 0 : X1.GetHashCode();
      hashcode = hashcode * RecConst.HASHFACTOR + (X2 == null ? 0 : X2.GetHashCode());
      hashcode = hashcode * RecConst.HASHFACTOR + (X3 == null ? 0 : X3.GetHashCode());
      hashcode = hashcode * RecConst.HASHFACTOR + (X4 == null ? 0 : X4.GetHashCode());
      return hashcode;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("({0}, {1}, {2}, {3})", X1, X2, X3, X4);
    }
    #region IShowable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public bool Show(System.Text.StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    {
      bool incomplete = true;
      stringbuilder.Append("(");
      rest -= 2;
      try
      {
        if (incomplete = !Showing.Show(X1, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X2, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X3, stringbuilder, ref rest, formatProvider))
          return false;
        stringbuilder.Append(", ");
        rest -= 2;
        if (incomplete = !Showing.Show(X4, stringbuilder, ref rest, formatProvider))
          return false;
      }
      finally
      {
        if (incomplete)
        {
          stringbuilder.Append("...");
          rest -= 3;
        }
        stringbuilder.Append(")");
      }
      return true;
    }
    #endregion

    #region IFormattable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public string ToString(string format, IFormatProvider formatProvider)
    {
      return Showing.ShowString(this, format, formatProvider);
    }

    #endregion
  }
}
