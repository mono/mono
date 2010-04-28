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
using System.Text;
using System.Diagnostics;
using SCG = System.Collections.Generic;
namespace C5
{
  /// <summary>
  /// An advanced interface to operations on an array. The array is viewed as an 
  /// <see cref="T:C5.IList`1"/> of fixed size, and so all operations that would change the
  /// size of the array will be invalid (and throw <see cref="T:C5.FixedSizeCollectionException"/>
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class WrappedArray<T> : IList<T>, SCG.IList<T>
  {
    class InnerList : ArrayList<T>
    {
      internal InnerList(T[] array) { this.array = array; size = array.Length; }
    }
    ArrayList<T> innerlist;
    //TODO: remember a ref to the wrapped array in WrappedArray to save a little on indexing?
    WrappedArray<T> underlying;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="wrappedarray"></param>
    public WrappedArray(T[] wrappedarray) { innerlist = new InnerList(wrappedarray); }

    //for views
    WrappedArray(ArrayList<T> arraylist, WrappedArray<T> underlying) { innerlist = arraylist; this.underlying = underlying; }

    #region IList<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public T First { get { return innerlist.First; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public T Last { get { return innerlist.Last; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public T this[int index]
    {
      get { return innerlist[index]; }
      set { innerlist[index] = value; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public IList<T> FindAll(Fun<T, bool> filter) { return innerlist.FindAll(filter); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <param name="mapper"></param>
    /// <returns></returns>
    public IList<V> Map<V>(Fun<T, V> mapper) { return innerlist.Map<V>(mapper); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="V"></typeparam>
    /// <param name="mapper"></param>
    /// <param name="equalityComparer"></param>
    /// <returns></returns>
    public IList<V> Map<V>(Fun<T, V> mapper, SCG.IEqualityComparer<V> equalityComparer) { return innerlist.Map<V>(mapper, equalityComparer); }

    /// <summary>
    /// ???? should we throw NotRelevantException
    /// </summary>
    /// <value></value>
    public bool FIFO
    {
      get { throw new FixedSizeCollectionException(); }
      set { throw new FixedSizeCollectionException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual bool IsFixedSize
    {
      get { return true; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <param name="item"></param>
    public void Insert(int index, T item)
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointer"></param>
    /// <param name="item"></param>
    public void Insert(IList<T> pointer, T item)
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void InsertFirst(T item)
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void InsertLast(T item)
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="i"></param>
    /// <param name="items"></param>
    public void InsertAll<U>(int i, System.Collections.Generic.IEnumerable<U> items) where U : T
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T Remove()
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T RemoveFirst()
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T RemoveLast()
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public IList<T> View(int start, int count)
    {
      return new WrappedArray<T>((ArrayList<T>)innerlist.View(start, count), underlying ?? this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public IList<T> ViewOf(T item)
    {
      return new WrappedArray<T>((ArrayList<T>)innerlist.ViewOf(item), underlying ?? this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public IList<T> LastViewOf(T item)
    {
      return new WrappedArray<T>((ArrayList<T>)innerlist.LastViewOf(item), underlying ?? this);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public IList<T> Underlying { get { return underlying; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int Offset { get { return innerlist.Offset; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool IsValid { get { return innerlist.IsValid; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public IList<T> Slide(int offset) { return innerlist.Slide(offset); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public IList<T> Slide(int offset, int size) { return innerlist.Slide(offset, size); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <returns></returns>
    public bool TrySlide(int offset) { return innerlist.TrySlide(offset); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    /// <returns></returns>
    public bool TrySlide(int offset, int size) { return innerlist.TrySlide(offset, size); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="otherView"></param>
    /// <returns></returns>
    public IList<T> Span(IList<T> otherView) { return innerlist.Span(((WrappedArray<T>)otherView).innerlist); }

    /// <summary>
    /// 
    /// </summary>
    public void Reverse() { innerlist.Reverse(); }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsSorted() { return innerlist.IsSorted(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comparer"></param>
    /// <returns></returns>
    public bool IsSorted(SCG.IComparer<T> comparer) { return innerlist.IsSorted(comparer); }

    /// <summary>
    /// 
    /// </summary>
    public void Sort() { innerlist.Sort(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="comparer"></param>
    public void Sort(SCG.IComparer<T> comparer) { innerlist.Sort(comparer); }

    /// <summary>
    /// 
    /// </summary>
    public void Shuffle() { innerlist.Shuffle(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="rnd"></param>
    public void Shuffle(Random rnd) { innerlist.Shuffle(rnd); }

    #endregion

    #region IIndexed<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public Speed IndexingSpeed { get { return Speed.Constant; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public IDirectedCollectionValue<T> this[int start, int count] { get { return innerlist[start, count]; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int IndexOf(T item) { return innerlist.IndexOf(item); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int LastIndexOf(T item) { return innerlist.LastIndexOf(item); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public int FindIndex(Fun<T, bool> predicate) { return innerlist.FindIndex(predicate); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public int FindLastIndex(Fun<T, bool> predicate) { return innerlist.FindLastIndex(predicate); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="i"></param>
    /// <returns></returns>
    public T RemoveAt(int i) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    public void RemoveInterval(int start, int count) { throw new FixedSizeCollectionException(); }

    #endregion

    #region ISequenced<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetSequencedHashCode() { return innerlist.GetSequencedHashCode(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public bool SequencedEquals(ISequenced<T> that) { return innerlist.SequencedEquals(that); }

    #endregion

    #region ICollection<T> Members
    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public Speed ContainsSpeed { get { return Speed.Linear; } }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetUnsequencedHashCode() { return innerlist.GetUnsequencedHashCode(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="that"></param>
    /// <returns></returns>
    public bool UnsequencedEquals(ICollection<T> that) { return innerlist.UnsequencedEquals(that); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Contains(T item) { return innerlist.Contains(item); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public int ContainsCount(T item) { return innerlist.ContainsCount(item); }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ICollectionValue<T> UniqueItems() { return innerlist.UniqueItems(); }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public ICollectionValue<KeyValuePair<T, int>> ItemMultiplicities() { return innerlist.ItemMultiplicities(); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public bool ContainsAll<U>(System.Collections.Generic.IEnumerable<U> items) where U : T
    { return innerlist.ContainsAll(items); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Find(ref T item) { return innerlist.Find(ref item); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool FindOrAdd(ref T item) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Update(T item) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public bool Update(T item, out T olditem) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool UpdateOrAdd(T item) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="olditem"></param>
    /// <returns></returns>
    public bool UpdateOrAdd(T item, out T olditem) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Remove(T item) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="removeditem"></param>
    /// <returns></returns>
    public bool Remove(T item, out T removeditem) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    public void RemoveAllCopies(T item) { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items"></param>
    public void RemoveAll<U>(System.Collections.Generic.IEnumerable<U> items) where U : T { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    public void Clear() { throw new FixedSizeCollectionException(); }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items"></param>
    public void RetainAll<U>(System.Collections.Generic.IEnumerable<U> items) where U : T { throw new FixedSizeCollectionException(); }

    #endregion

    #region IExtensible<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool IsReadOnly { get { return true; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool AllowsDuplicates
    {
      get { return true; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public SCG.IEqualityComparer<T> EqualityComparer { get { return innerlist.EqualityComparer; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool DuplicatesByCounting
    {
      get { return false; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Add(T item)
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="U"></typeparam>
    /// <param name="items"></param>
    public void AddAll<U>(System.Collections.Generic.IEnumerable<U> items) where U : T
    {
      throw new FixedSizeCollectionException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool Check()
    {
      return innerlist.Check() && (underlying == null || underlying.innerlist == innerlist.Underlying);
    }

    #endregion

    #region ICollectionValue<T> Members
    /// <summary>
    /// No listeners may be installed
    /// </summary>
    /// <value>0</value>
    public virtual EventTypeEnum ListenableEvents { get { return 0; } }

    /// <summary>
    /// No listeners ever installed
    /// </summary>
    /// <value>0</value>
    public virtual EventTypeEnum ActiveEvents { get { return 0; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event CollectionChangedHandler<T> CollectionChanged
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event CollectionClearedHandler<T> CollectionCleared
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event ItemsAddedHandler<T> ItemsAdded
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event ItemInsertedHandler<T> ItemInserted
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event ItemsRemovedHandler<T> ItemsRemoved
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public event ItemRemovedAtHandler<T> ItemRemovedAt
    {
      add { throw new UnlistenableEventException(); }
      remove { throw new UnlistenableEventException(); }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public bool IsEmpty { get { return innerlist.IsEmpty; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int Count { get { return innerlist.Count; } }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public Speed CountSpeed { get { return innerlist.CountSpeed; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    public void CopyTo(T[] array, int index) { innerlist.CopyTo(array, index); }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T[] ToArray() { return innerlist.ToArray(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    public void Apply(Act<T> action) { innerlist.Apply(action); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool Exists(Fun<T, bool> predicate) { return innerlist.Exists(predicate); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool Find(Fun<T, bool> predicate, out T item) { return innerlist.Find(predicate, out item); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public bool All(Fun<T, bool> predicate) { return innerlist.All(predicate); }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public T Choose() { return innerlist.Choose(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public SCG.IEnumerable<T> Filter(Fun<T, bool> filter) { return innerlist.Filter(filter); }

    #endregion

    #region IEnumerable<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public SCG.IEnumerator<T> GetEnumerator() { return innerlist.GetEnumerator(); }
    #endregion

    #region IShowable Members

    /// <summary>
    /// 
    /// </summary>
    /// <param name="stringbuilder"></param>
    /// <param name="rest"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public bool Show(StringBuilder stringbuilder, ref int rest, IFormatProvider formatProvider)
    { return innerlist.Show(stringbuilder, ref  rest, formatProvider); }

    #endregion

    #region IFormattable Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() { return innerlist.ToString(); }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="format"></param>
    /// <param name="formatProvider"></param>
    /// <returns></returns>
    public virtual string ToString(string format, IFormatProvider formatProvider) { return innerlist.ToString(format, formatProvider); }

    #endregion

    #region IDirectedCollectionValue<T> Members

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IDirectedCollectionValue<T> Backwards() { return innerlist.Backwards(); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="predicate"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool FindLast(Fun<T, bool> predicate, out T item) { return innerlist.FindLast(predicate, out item); }

    #endregion

    #region IDirectedEnumerable<T> Members

    IDirectedEnumerable<T> IDirectedEnumerable<T>.Backwards() { return Backwards(); }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public EnumerationDirection Direction { get { return EnumerationDirection.Forwards; } }

    #endregion

    #region IDisposable Members

    /// <summary>
    /// Dispose this if a view else operation is illegal 
    /// </summary>
    /// <exception cref="FixedSizeCollectionException">If not a view</exception>
    public void Dispose()
    {
      if (underlying == null)
        throw new FixedSizeCollectionException();
      else
        innerlist.Dispose();
    }

    #endregion

    #region ICloneable Members

    /// <summary>
    /// Make a shallow copy of this WrappedArray.
    /// 
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual object Clone()
    {
      return new WrappedArray<T>(innerlist.ToArray());
    }

    #endregion

    #region System.Collections.Generic.IList<T> Members

    void System.Collections.Generic.IList<T>.RemoveAt(int index)
    {
      throw new FixedSizeCollectionException();
    }

    void System.Collections.Generic.ICollection<T>.Add(T item)
    {
      throw new FixedSizeCollectionException();
    }

    #endregion

    #region System.Collections.ICollection Members

    bool System.Collections.ICollection.IsSynchronized
    {
      get { return false; }
    }

    [Obsolete]
    Object System.Collections.ICollection.SyncRoot
    {
      get { return ((System.Collections.IList)innerlist).SyncRoot; }
    }

    void System.Collections.ICollection.CopyTo(Array arr, int index)
    {
      if (index < 0 || index + Count > arr.Length)
        throw new ArgumentOutOfRangeException();

      foreach (T item in this)
        arr.SetValue(item, index++);
    }
    
    #endregion

    #region System.Collections.IList Members

    Object System.Collections.IList.this[int index]
    {
      get { return this[index]; }
      set { this[index] = (T)value; }
    }

    int System.Collections.IList.Add(Object o)
    {
      bool added = Add((T)o);
      // What position to report if item not added? SC.IList.Add doesn't say
      return added ? Count - 1 : -1;
    }

    bool System.Collections.IList.Contains(Object o)
    {
      return Contains((T)o);
    }

    int System.Collections.IList.IndexOf(Object o)
    {
      return Math.Max(-1, IndexOf((T)o));
    }

    void System.Collections.IList.Insert(int index, Object o)
    {
      Insert(index, (T)o);
    }

    void System.Collections.IList.Remove(Object o)
    {
      Remove((T)o);
    }

    void System.Collections.IList.RemoveAt(int index)
    {
      RemoveAt(index);
    }

    #endregion
    
    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }
}