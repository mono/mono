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
using SCG = System.Collections.Generic;

namespace C5
{
  /// <summary>
  /// A default item comparer for an item type that is either generic (IComparable&lt;T&gt;)
  /// or ordinarily (System.IComparable) comparable.
  /// </summary>
  public static class Comparer<T>
  {
    readonly static Type naturalComparerO = typeof(NaturalComparerO<>);

    readonly static Type naturalComparer = typeof(NaturalComparer<>);

    static SCG.IComparer<T> cachedComparer = null;

    /// <summary>
    /// Create a default comparer. 
    /// <para>The IComparer[T] object is constructed when this class is initialised, i.e. 
    /// its static constructors called. Thus, the property will be the same object 
    /// for the duration of an invocation of the runtime, but a value serialized in 
    /// another invocation and deserialized here will not be the same object.</para>
    /// </summary>
    /// <exception cref="NotComparableException">If T is not comparable</exception>
    /// <value>The comparer</value>
    [Tested]
    public static SCG.IComparer<T> Default
    {
      get
      {
        if (cachedComparer != null)
          return cachedComparer;

        Type t = typeof(T);

        if (t.IsValueType)
        {
          if (t.Equals(typeof(char)))
            return cachedComparer = (SCG.IComparer<T>)(new CharComparer());

          if (t.Equals(typeof(sbyte)))
            return cachedComparer = (SCG.IComparer<T>)(new SByteComparer());

          if (t.Equals(typeof(byte)))
            return cachedComparer = (SCG.IComparer<T>)(new ByteComparer());

          if (t.Equals(typeof(short)))
            return cachedComparer = (SCG.IComparer<T>)(new ShortComparer());

          if (t.Equals(typeof(ushort)))
            return cachedComparer = (SCG.IComparer<T>)(new UShortComparer());

          if (t.Equals(typeof(int)))
            return cachedComparer = (SCG.IComparer<T>)(new IntComparer());

          if (t.Equals(typeof(uint)))
            return cachedComparer = (SCG.IComparer<T>)(new UIntComparer());

          if (t.Equals(typeof(long)))
            return cachedComparer = (SCG.IComparer<T>)(new LongComparer());

          if (t.Equals(typeof(ulong)))
            return cachedComparer = (SCG.IComparer<T>)(new ULongComparer());

          if (t.Equals(typeof(float)))
            return cachedComparer = (SCG.IComparer<T>)(new FloatComparer());

          if (t.Equals(typeof(double)))
            return cachedComparer = (SCG.IComparer<T>)(new DoubleComparer());

          if (t.Equals(typeof(decimal)))
            return cachedComparer = (SCG.IComparer<T>)(new DecimalComparer());
        }

        if (typeof(IComparable<T>).IsAssignableFrom(t))
        {
          Type c = naturalComparer.MakeGenericType(new Type[] { t });

          return cachedComparer = (SCG.IComparer<T>)(c.GetConstructor(System.Type.EmptyTypes).Invoke(null));
        }

        if (t.GetInterface("System.IComparable") != null)
        {
          Type c = naturalComparerO.MakeGenericType(new Type[] { t });

          return cachedComparer = (SCG.IComparer<T>)(c.GetConstructor(System.Type.EmptyTypes).Invoke(null));
        }

        throw new NotComparableException(String.Format("Cannot make comparer for type {0}", t));
      }
    }
  }

  /// <summary>
  /// A natural generic IComparer for an IComparable&lt;T&gt; item type
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public class NaturalComparer<T> : SCG.IComparer<T>
      where T : IComparable<T>
  {
    /// <summary>
    /// Compare two items
    /// </summary>
    /// <param name="item1">First item</param>
    /// <param name="item2">Second item</param>
    /// <returns>item1 &lt;=&gt; item2</returns>
    [Tested]
    public int Compare(T item1, T item2) { return item1 != null ? item1.CompareTo(item2) : item2 != null ? -1 : 0; }
  }

  /// <summary>
  /// A natural generic IComparer for a System.IComparable item type
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public class NaturalComparerO<T> : SCG.IComparer<T> where T : System.IComparable
  {
    /// <summary>
    /// Compare two items
    /// </summary>
    /// <param name="item1">First item</param>
    /// <param name="item2">Second item</param>
    /// <returns>item1 &lt;=&gt; item2</returns>
    [Tested]
    public int Compare(T item1, T item2) { return item1 != null ? item1.CompareTo(item2) : item2 != null ? -1 : 0; }
  }

  /// <summary>
  /// A generic comparer for type T based on a Comparison[T] delegate
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  public class DelegateComparer<T> : SCG.IComparer<T>
  {
    readonly Comparison<T> cmp;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="comparison"></param>
    public DelegateComparer(Comparison<T> comparison)
    {
      if (comparison == null)
        throw new NullReferenceException("Comparison cannot be null");
      this.cmp = comparison;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="item1">First item</param>
    /// <param name="item2">Second item</param>
    /// <returns>item1 &lt;=&gt; item2</returns>
    public int Compare(T item1, T item2) { return cmp(item1, item2); }
  }
}