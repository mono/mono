#if NET_2_0
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
using System;
using System.Diagnostics;
using SCG = System.Collections.Generic;
namespace C5
{
  #region int stuff
  [Serializable]
  class IntComparer : SCG.IComparer<int>
  {
    [Tested]
    public int Compare(int a, int b) { return a > b ? 1 : a < b ? -1 : 0; }
  }

  /// <summary>
  /// An equalityComparer for System.Int32 integers. 
  /// <para>This class is a singleton and the instance can be accessed
  /// via the static <see cref="P:C5.IntEqualityComparer.Default"/> property</para>
  /// </summary>
  [Serializable]
  public class IntEqualityComparer : SCG.IEqualityComparer<int>
  {
    static IntEqualityComparer cached;
    IntEqualityComparer() { }
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    [Tested]
    public static IntEqualityComparer Default { get { return cached ?? (cached = new IntEqualityComparer()); } }
    /// <summary>
    /// Get the hash code of this integer, that is, itself
    /// </summary>
    /// <param name="item">The integer</param>
    /// <returns>The same</returns>
    [Tested]
    public int GetHashCode(int item) { return item; }


    /// <summary>
    /// Determine whether two integers are equal
    /// </summary>
    /// <param name="int1">first integer</param>
    /// <param name="int2">second integer</param>
    /// <returns>True if equal</returns>
    [Tested]
    public bool Equals(int int1, int int2) { return int1 == int2; }
  }

  #endregion

  #region double stuff
  class DoubleComparer : SCG.IComparer<double>
  {
    public int Compare(double a, double b) { return a > b ? 1 : a < b ? -1 : 0; }
  }

  /// <summary>
  /// An equalityComparer for double. 
  /// <para>This class is a singleton and the instance can be accessed
  /// via the static <see cref="P:C5.DoubleEqualityComparer.Default"/> property</para>
  /// </summary>
  public class DoubleEqualityComparer : SCG.IEqualityComparer<double>
  {
    static DoubleEqualityComparer cached;
    DoubleEqualityComparer() { }
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    [Tested]
    public static DoubleEqualityComparer Default { get { return cached ?? (cached = new DoubleEqualityComparer()); } }
    /// <summary>
    /// Get the hash code of this double,
    /// </summary>
    /// <param name="item">The double</param>
    /// <returns>The same</returns>
    [Tested]
    public int GetHashCode(double item) { return item.GetHashCode(); }


    /// <summary>
    /// Check if two doubles are equal
    /// </summary>
    /// <param name="item1">first double</param>
    /// <param name="item2">second double</param>
    /// <returns>True if equal</returns>
    [Tested]
    public bool Equals(double item1, double item2) { return item1 == item2; }
  }
  #endregion

  #region byte stuff
  class ByteComparer : SCG.IComparer<byte>
  {
    public int Compare(byte a, byte b) { return a > b ? 1 : a < b ? -1 : 0; }
  }

  /// <summary>
  /// An equalityComparer for byte
  /// <para>This class is a singleton and the instance can be accessed
  /// via the <see cref="P:C5.ByteEqualityComparer.Default"/> property</para>
  /// </summary>
  public class ByteEqualityComparer : SCG.IEqualityComparer<byte>
  {
    static ByteEqualityComparer cached = new ByteEqualityComparer();
    ByteEqualityComparer() { }
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static ByteEqualityComparer Default { get { return cached ?? (cached = new ByteEqualityComparer()); } }
    /// <summary>
    /// Get the hash code of this byte, i.e. itself
    /// </summary>
    /// <param name="item">The byte</param>
    /// <returns>The same</returns>
    public int GetHashCode(byte item) { return item; }

    /// <summary>
    /// Check if two bytes are equal
    /// </summary>
    /// <param name="item1">first byte</param>
    /// <param name="item2">second byte</param>
    /// <returns>True if equal</returns>
    public bool Equals(byte item1, byte item2) { return item1 == item2; }
  }
  #endregion

  #region char stuff
  class CharComparer : SCG.IComparer<char>
  {
    public int Compare(char a, char b) { return a > b ? 1 : a < b ? -1 : 0; }
  }

  /// <summary>
  /// An equalityComparer for char.
  /// </summary>
  public class CharEqualityComparer : SCG.IEqualityComparer<char>
  {
    static CharEqualityComparer cached = new CharEqualityComparer();
    CharEqualityComparer() { }
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static CharEqualityComparer Default { get { return cached ?? (cached = new CharEqualityComparer()); } }

    /// <summary>
    /// Get the hash code of this char
    /// </summary>
    /// <param name="item">The char</param>
    /// <returns>The same</returns>
    public int GetHashCode(char item) { return item.GetHashCode(); }


    /// <summary>
    /// Check if two chars are equal
    /// </summary>
    /// <param name="item1">first char</param>
    /// <param name="item2">second char</param>
    /// <returns>True if equal</returns>
    public bool Equals(char item1, char item2) { return item1 == item2; }
  }
  #endregion

}
#endif
