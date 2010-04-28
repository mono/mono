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
  /// 
  /// </summary>
  [Flags]
  public enum EventTypeEnum
  {
    /// <summary>
    /// 
    /// </summary>
    None = 0x00000000,
    /// <summary>
    /// 
    /// </summary>
    Changed = 0x00000001,
    /// <summary>
    /// 
    /// </summary>
    Cleared = 0x00000002,
    /// <summary>
    /// 
    /// </summary>
    Added = 0x00000004,
    /// <summary>
    /// 
    /// </summary>
    Removed = 0x00000008,
    /// <summary>
    /// 
    /// </summary>
    Basic = 0x0000000f,
    /// <summary>
    /// 
    /// </summary>
    Inserted = 0x00000010,
    /// <summary>
    /// 
    /// </summary>
    RemovedAt = 0x00000020,
    /// <summary>
    /// 
    /// </summary>
    All = 0x0000003f
  }

  /// <summary>
  /// Holds the real events for a collection
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  internal sealed class EventBlock<T>
  {
    internal EventTypeEnum events;

    event CollectionChangedHandler<T> collectionChanged;
    internal event CollectionChangedHandler<T> CollectionChanged
    {
      add
      {
        collectionChanged += value;
        events |= EventTypeEnum.Changed;
      }
      remove
      {
        collectionChanged -= value;
        if (collectionChanged == null)
          events &= ~EventTypeEnum.Changed;
      }
    }
    internal void raiseCollectionChanged(object sender)
    { if (collectionChanged != null) collectionChanged(sender); }

    event CollectionClearedHandler<T> collectionCleared;
    internal event CollectionClearedHandler<T> CollectionCleared
    {
      add
      {
        collectionCleared += value;
        events |= EventTypeEnum.Cleared;
      }
      remove
      {
        collectionCleared -= value;
        if (collectionCleared == null)
          events &= ~EventTypeEnum.Cleared;
      }
    }
    internal void raiseCollectionCleared(object sender, bool full, int count)
    { if (collectionCleared != null) collectionCleared(sender, new ClearedEventArgs(full, count)); }
    internal void raiseCollectionCleared(object sender, bool full, int count, int? start)
    { if (collectionCleared != null) collectionCleared(sender, new ClearedRangeEventArgs(full, count, start)); }

    event ItemsAddedHandler<T> itemsAdded;
    internal event ItemsAddedHandler<T> ItemsAdded
    {
      add
      {
        itemsAdded += value;
        events |= EventTypeEnum.Added;
      }
      remove
      {
        itemsAdded -= value;
        if (itemsAdded == null)
          events &= ~EventTypeEnum.Added;
      }
    }
    internal void raiseItemsAdded(object sender, T item, int count)
    { if (itemsAdded != null) itemsAdded(sender, new ItemCountEventArgs<T>(item, count)); }

    event ItemsRemovedHandler<T> itemsRemoved;
    internal event ItemsRemovedHandler<T> ItemsRemoved
    {
      add
      {
        itemsRemoved += value;
        events |= EventTypeEnum.Removed;
      }
      remove
      {
        itemsRemoved -= value;
        if (itemsRemoved == null)
          events &= ~EventTypeEnum.Removed;
      }
    }
    internal void raiseItemsRemoved(object sender, T item, int count)
    { if (itemsRemoved != null) itemsRemoved(sender, new ItemCountEventArgs<T>(item, count)); }

    event ItemInsertedHandler<T> itemInserted;
    internal event ItemInsertedHandler<T> ItemInserted
    {
      add
      {
        itemInserted += value;
        events |= EventTypeEnum.Inserted;
      }
      remove
      {
        itemInserted -= value;
        if (itemInserted == null)
          events &= ~EventTypeEnum.Inserted;
      }
    }
    internal void raiseItemInserted(object sender, T item, int index)
    { if (itemInserted != null) itemInserted(sender, new ItemAtEventArgs<T>(item, index)); }

    event ItemRemovedAtHandler<T> itemRemovedAt;
    internal event ItemRemovedAtHandler<T> ItemRemovedAt
    {
      add
      {
        itemRemovedAt += value;
        events |= EventTypeEnum.RemovedAt;
      }
      remove
      {
        itemRemovedAt -= value;
        if (itemRemovedAt == null)
          events &= ~EventTypeEnum.RemovedAt;
      }
    }
    internal void raiseItemRemovedAt(object sender, T item, int index)
    { if (itemRemovedAt != null) itemRemovedAt(sender, new ItemAtEventArgs<T>(item, index)); }
  }

  /// <summary>
  /// Tentative, to conserve memory in GuardedCollectionValueBase
  /// This should really be nested in Guarded collection value, only have a guardereal field
  /// </summary>
  /// <typeparam name="T"></typeparam>
  [Serializable]
  internal sealed class ProxyEventBlock<T>
  {
    ICollectionValue<T> proxy, real;

    internal ProxyEventBlock(ICollectionValue<T> proxy, ICollectionValue<T> real)
    { this.proxy = proxy; this.real = real; }

    event CollectionChangedHandler<T> collectionChanged;
    CollectionChangedHandler<T> collectionChangedProxy;
    internal event CollectionChangedHandler<T> CollectionChanged
    {
      add
      {
        if (collectionChanged == null)
        {
          if (collectionChangedProxy == null)
            collectionChangedProxy = delegate(object sender) { collectionChanged(proxy); };
          real.CollectionChanged += collectionChangedProxy;
        }
        collectionChanged += value;
      }
      remove
      {
        collectionChanged -= value;
        if (collectionChanged == null)
          real.CollectionChanged -= collectionChangedProxy;
      }
    }

    event CollectionClearedHandler<T> collectionCleared;
    CollectionClearedHandler<T> collectionClearedProxy;
    internal event CollectionClearedHandler<T> CollectionCleared
    {
      add
      {
        if (collectionCleared == null)
        {
          if (collectionClearedProxy == null)
            collectionClearedProxy = delegate(object sender, ClearedEventArgs e) { collectionCleared(proxy, e); };
          real.CollectionCleared += collectionClearedProxy;
        }
        collectionCleared += value;
      }
      remove
      {
        collectionCleared -= value;
        if (collectionCleared == null)
          real.CollectionCleared -= collectionClearedProxy;
      }
    }

    event ItemsAddedHandler<T> itemsAdded;
    ItemsAddedHandler<T> itemsAddedProxy;
    internal event ItemsAddedHandler<T> ItemsAdded
    {
      add
      {
        if (itemsAdded == null)
        {
          if (itemsAddedProxy == null)
            itemsAddedProxy = delegate(object sender, ItemCountEventArgs<T> e) { itemsAdded(proxy, e); };
          real.ItemsAdded += itemsAddedProxy;
        }
        itemsAdded += value;
      }
      remove
      {
        itemsAdded -= value;
        if (itemsAdded == null)
          real.ItemsAdded -= itemsAddedProxy;
      }
    }

    event ItemInsertedHandler<T> itemInserted;
    ItemInsertedHandler<T> itemInsertedProxy;
    internal event ItemInsertedHandler<T> ItemInserted
    {
      add
      {
        if (itemInserted == null)
        {
          if (itemInsertedProxy == null)
            itemInsertedProxy = delegate(object sender, ItemAtEventArgs<T> e) { itemInserted(proxy, e); };
          real.ItemInserted += itemInsertedProxy;
        }
        itemInserted += value;
      }
      remove
      {
        itemInserted -= value;
        if (itemInserted == null)
          real.ItemInserted -= itemInsertedProxy;
      }
    }

    event ItemsRemovedHandler<T> itemsRemoved;
    ItemsRemovedHandler<T> itemsRemovedProxy;
    internal event ItemsRemovedHandler<T> ItemsRemoved
    {
      add
      {
        if (itemsRemoved == null)
        {
          if (itemsRemovedProxy == null)
            itemsRemovedProxy = delegate(object sender, ItemCountEventArgs<T> e) { itemsRemoved(proxy, e); };
          real.ItemsRemoved += itemsRemovedProxy;
        }
        itemsRemoved += value;
      }
      remove
      {
        itemsRemoved -= value;
        if (itemsRemoved == null)
          real.ItemsRemoved -= itemsRemovedProxy;
      }
    }

    event ItemRemovedAtHandler<T> itemRemovedAt;
    ItemRemovedAtHandler<T> itemRemovedAtProxy;
    internal event ItemRemovedAtHandler<T> ItemRemovedAt
    {
      add
      {
        if (itemRemovedAt == null)
        {
          if (itemRemovedAtProxy == null)
            itemRemovedAtProxy = delegate(object sender, ItemAtEventArgs<T> e) { itemRemovedAt(proxy, e); };
          real.ItemRemovedAt += itemRemovedAtProxy;
        }
        itemRemovedAt += value;
      }
      remove
      {
        itemRemovedAt -= value;
        if (itemRemovedAt == null)
          real.ItemRemovedAt -= itemRemovedAtProxy;
      }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ItemAtEventArgs<T> : EventArgs
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly T Item;
    /// <summary>
    /// 
    /// </summary>
    public readonly int Index;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="item"></param>
    /// <param name="index"></param>
    public ItemAtEventArgs(T item, int index) { Item = item; Index = index; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("(ItemAtEventArgs {0} '{1}')", Index, Item);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class ItemCountEventArgs<T> : EventArgs
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly T Item;
    /// <summary>
    /// 
    /// </summary>
    public readonly int Count;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    /// <param name="item"></param>
    public ItemCountEventArgs(T item, int count) { Item = item; Count = count; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("(ItemCountEventArgs {0} '{1}')", Count, Item);
    }
  }



  /// <summary>
  /// 
  /// </summary>
  public class ClearedEventArgs : EventArgs
  {
    /// <summary>
    /// 
    /// </summary>
    public readonly bool Full;
    /// <summary>
    /// 
    /// </summary>
    public readonly int Count;
    /// <summary>
    /// 
    /// </summary>
    /// 
    /// <param name="full">True if the operation cleared all of the collection</param>
    /// <param name="count">The number of items removed by the clear.</param>
    public ClearedEventArgs(bool full, int count) { Full = full; Count = count; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("(ClearedEventArgs {0} {1})", Count, Full);
    }
  }

  /// <summary>
  /// 
  /// </summary>
  public class ClearedRangeEventArgs : ClearedEventArgs
  {
    //WE could let this be of type int? to  allow 
    /// <summary>
    /// 
    /// </summary>
    public readonly int? Start;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="full"></param>
    /// <param name="count"></param>
    /// <param name="start"></param>
    public ClearedRangeEventArgs(bool full, int count, int? start) : base(full,count) { Start = start; }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Format("(ClearedRangeEventArgs {0} {1} {2})", Count, Full, Start);
    }
  }

  /// <summary>
  /// The type of event raised after an operation on a collection has changed its contents.
  /// Normally, a multioperation like AddAll, 
  /// <see cref="M:C5.IExtensible`1.AddAll(System.Collections.Generic.IEnumerable{`0})"/> 
  /// will only fire one CollectionChanged event. Any operation that changes the collection
  /// must fire CollectionChanged as its last event.
  /// </summary>
  public delegate void CollectionChangedHandler<T>(object sender);

  /// <summary>
  /// The type of event raised after the Clear() operation on a collection.
  /// <para/>
  /// Note: The Clear() operation will not fire ItemsRemoved events. 
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="eventArgs"></param>
  public delegate void CollectionClearedHandler<T>(object sender, ClearedEventArgs eventArgs);

  /// <summary>
  /// The type of event raised after an item has been added to a collection.
  /// The event will be raised at a point of time, where the collection object is 
  /// in an internally consistent state and before the corresponding CollectionChanged 
  /// event is raised.
  /// <para/>
  /// Note: an Update operation will fire an ItemsRemoved and an ItemsAdded event.
  /// <para/>
  /// Note: When an item is inserted into a list (<see cref="T:C5.IList`1"/>), both
  /// ItemInserted and ItemsAdded events will be fired.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="eventArgs">An object with the item that was added</param>
  public delegate void ItemsAddedHandler<T>(object sender, ItemCountEventArgs<T> eventArgs);

  /// <summary>
  /// The type of event raised after an item has been removed from a collection.
  /// The event will be raised at a point of time, where the collection object is 
  /// in an internally consistent state and before the corresponding CollectionChanged 
  /// event is raised.
  /// <para/>
  /// Note: The Clear() operation will not fire ItemsRemoved events. 
  /// <para/>
  /// Note: an Update operation will fire an ItemsRemoved and an ItemsAdded event.
  /// <para/>
  /// Note: When an item is removed from a list by the RemoveAt operation, both an 
  /// ItemsRemoved and an ItemRemovedAt event will be fired.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="eventArgs">An object with the item that was removed</param>
  public delegate void ItemsRemovedHandler<T>(object sender, ItemCountEventArgs<T> eventArgs);

  /// <summary>
  /// The type of event raised after an item has been inserted into a list by an Insert, 
  /// InsertFirst or InsertLast operation.
  /// The event will be raised at a point of time, where the collection object is 
  /// in an internally consistent state and before the corresponding CollectionChanged 
  /// event is raised.
  /// <para/>
  /// Note: an ItemsAdded event will also be fired.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="eventArgs"></param>
  public delegate void ItemInsertedHandler<T>(object sender, ItemAtEventArgs<T> eventArgs);

  /// <summary>
  /// The type of event raised after an item has been removed from a list by a RemoveAt(int i)
  /// operation (or RemoveFirst(), RemoveLast(), Remove() operation).
  /// The event will be raised at a point of time, where the collection object is 
  /// in an internally consistent state and before the corresponding CollectionChanged 
  /// event is raised.
  /// <para/>
  /// Note: an ItemRemoved event will also be fired.
  /// </summary>
  /// <param name="sender"></param>
  /// <param name="eventArgs"></param>
  public delegate void ItemRemovedAtHandler<T>(object sender, ItemAtEventArgs<T> eventArgs);
}