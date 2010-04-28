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
  /// An exception to throw from library code when an internal inconsistency is encountered.
  /// </summary>
  public class InternalException : Exception
  {
    internal InternalException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by an update operation on a Read-Only collection or dictionary.
  /// <para>This exception will be thrown unconditionally when an update operation 
  /// (method or set property) is called. No check is made to see if the update operation, 
  /// if allowed, would actually change the collection. </para>
  /// </summary>
  [Serializable]
  public class ReadOnlyCollectionException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public ReadOnlyCollectionException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public ReadOnlyCollectionException(string message) : base(message) { }
  }

  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class FixedSizeCollectionException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public FixedSizeCollectionException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public FixedSizeCollectionException(string message) : base(message) { }
  }

  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class UnlistenableEventException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public UnlistenableEventException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public UnlistenableEventException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by enumerators, range views etc. when accessed after 
  /// the underlying collection has been modified.
  /// </summary>
  [Serializable]
  public class CollectionModifiedException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public CollectionModifiedException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public CollectionModifiedException(string message) : base(message) { }
  }

  /// <summary>
  /// An excption thrown when trying to access a view (a list view on a <see cref="T:C5.IList`1"/> or 
  /// a snapshot on a <see cref="T:C5.IPersistentSorted`1"/>)
  /// that has been invalidated by some earlier operation.
  /// <para>
  /// The typical scenario is a view on a list that hash been invalidated by a call to 
  /// Sort, Reverse or Shuffle on some other, overlapping view or the whole list.
  /// </para>
  /// </summary>
  [Serializable]
  public class ViewDisposedException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public ViewDisposedException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public ViewDisposedException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by a lookup or lookup with update operation that does not 
  /// find the lookup item and has no other means to communicate failure.
  /// <para>The typical scenario is a lookup by key in a dictionary with an indexer,
  /// see e.g. <see cref="P:C5.IDictionary`2.Item(`0)"/></para>
  /// </summary>
  [Serializable]
  public class NoSuchItemException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public NoSuchItemException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public NoSuchItemException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by an operation on a list (<see cref="T:C5.IList`1"/>)
  /// that only makes sense for a view, not for an underlying list.
  /// </summary>
  [Serializable]
  public class NotAViewException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public NotAViewException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public NotAViewException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown when an operation attempts to create a duplicate in a collection with set semantics 
  /// (<see cref="P:C5.IExtensible`1.AllowsDuplicates"/> is false) or attempts to create a duplicate key in a dictionary.
  /// <para>With collections this can only happen with Insert operations on lists, since the Add operations will
  /// not try to create duplictes and either ignore the failure or report it in a bool return value.
  /// </para>
  /// <para>With dictionaries this can happen with the <see cref="M:C5.IDictionary`2.Add(`0,`1)"/> metod.</para>
  /// </summary>
  [Serializable]
  public class DuplicateNotAllowedException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public DuplicateNotAllowedException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public DuplicateNotAllowedException(string message) : base(message) { }
  }

  /// <summary>
  /// 
  /// </summary>
  [Serializable]
  public class InvalidPriorityQueueHandleException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public InvalidPriorityQueueHandleException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public InvalidPriorityQueueHandleException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by an operation that need to construct a natural
  /// comparer for a type.
  /// </summary>
  [Serializable]
  public class NotComparableException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public NotComparableException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public NotComparableException(string message) : base(message) { }
  }

  /// <summary>
  /// An exception thrown by operations on a list that expects an argument
  /// that is a view on the same underlying list.
  /// </summary>
  [Serializable]
  public class IncompatibleViewException : Exception
  {
    /// <summary>
    /// Create a simple exception with no further explanation.
    /// </summary>
    public IncompatibleViewException() : base() { }
    /// <summary>
    /// Create the exception with an explanation of the reason.
    /// </summary>
    /// <param name="message"></param>
    public IncompatibleViewException(string message) : base(message) { }
  }

}