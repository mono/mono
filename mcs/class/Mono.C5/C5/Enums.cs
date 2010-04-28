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

  /// <summary>
  /// The symbolic characterization of the speed of lookups for a collection.
  /// The values may refer to worst-case, amortized and/or expected asymtotic 
  /// complexity wrt. the collection size.
  /// </summary>
  public enum Speed : short
  {
    /// <summary>
    /// Counting the collection with the <code>Count property</code> may not return
    /// (for a synthetic and potentially infinite collection).
    /// </summary>
    PotentiallyInfinite = 1,
    /// <summary>
    /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
    /// property may take time O(n),
    /// where n is the size of the collection.
    /// </summary>
    Linear = 2,
    /// <summary>
    /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
    /// property  takes time O(log n),
    /// where n is the size of the collection.
    /// </summary>
    Log = 3,
    /// <summary>
    /// Lookup operations like <code>Contains(T item)</code> or the <code>Count</code>
    /// property  takes time O(1),
    /// where n is the size of the collection.
    /// </summary>
    Constant = 4
  }
  /*
  /// <summary>
  /// 
  /// </summary>
  public enum ItemEqualityTypeEnum
  {
    /// <summary>
    /// Only an Equals(T,T)
    /// </summary>
    Equator, 
    /// <summary>
    /// Equals(T,T) and GetHashCode(T)
    /// </summary>
    HashingEqualityComparer, 
    /// <summary>
    /// Compare(T,T)
    /// </summary>
    Comparer, 
    /// <summary>
    /// Compatible Compare(T,T) and GetHashCode(T)
    /// </summary>
    Both
  }
*/

  /// <summary>
  /// Direction of enumeration order relative to original collection.
  /// </summary>
  public enum EnumerationDirection
  {
    /// <summary>
    /// Same direction
    /// </summary>
    Forwards,
    /// <summary>
    /// Opposite direction
    /// </summary>
    Backwards
  }
}