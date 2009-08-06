/*
 Copyright (c) 2003-2008 Niels Kokholm and Peter Sestoft
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

// C5 example: EventPatterns.cs for pattern chapter

// Compile with 
//   csc /r:C5.dll EventPatterns.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace EventPatterns {
  class EventPatterns {
    public static void Main(String[] args) {
      UnindexedCollectionEvents();
      Console.WriteLine("--------------------");
      IndexedCollectionEvents();
      Console.WriteLine("--------------------");
      UpdateEvent();
    }

    public static void UnindexedCollectionEvents() {
      ICollection<int> coll = new ArrayList<int>();
      ICollection<int> bag1 = new HashBag<int>();
      bag1.AddAll(new int[] { 3, 2, 5, 5, 7, 7, 5, 3, 7, 7 });
      // Add change handler
      coll.CollectionChanged 
        += delegate(Object c)  { 
             Console.WriteLine("Collection changed"); 
           };
      // Add cleared handler
      coll.CollectionCleared 
        += delegate(Object c, ClearedEventArgs args) { 
             Console.WriteLine("Collection cleared"); 
           };
      // Add added handler
      coll.ItemsAdded 
        += delegate(Object c, ItemCountEventArgs<int> args) { 
             Console.WriteLine("Item {0} added", args.Item); 
           };
      // Add item count handler
      AddItemsAddedCounter(coll);
      AddItemsRemovedCounter(coll);
      coll.AddAll(bag1);
      coll.RemoveAll(new int[] { 2, 5, 6, 3, 7, 2 });
      coll.Clear();
      ICollection<int> bag2 = new HashBag<int>();
      // Add added handler with multiplicity
      bag2.ItemsAdded 
        += delegate(Object c, ItemCountEventArgs<int> args) { 
             Console.WriteLine("{0} copies of {1} added", 
                               args.Count, args.Item);
           };
      bag2.AddAll(bag1);
      // Add removed handler with multiplicity
      bag2.ItemsRemoved 
        += delegate(Object c, ItemCountEventArgs<int> args) { 
             Console.WriteLine("{0} copies of {1} removed", 
                               args.Count, args.Item);
           };
      bag2.RemoveAllCopies(7);
    }

    // This works for all kinds of collections, also those with bag
    // semantics and representing duplicates by counting:

    private static void AddItemsAddedCounter<T>(ICollection<T> coll) {
      int addedCount = 0;
      coll.ItemsAdded 
        += delegate(Object c, ItemCountEventArgs<T> args) { 
             addedCount += args.Count; 
           };
      coll.CollectionChanged 
        += delegate(Object c) { 
             if (addedCount > 0)
               Console.WriteLine("{0} items were added", addedCount);
             addedCount = 0;
           };
    }

    // This works for all kinds of collections, also those with bag
    // semantics and representing duplicates by counting:

    private static void AddItemsRemovedCounter<T>(ICollection<T> coll) {
      int removedCount = 0;
      coll.ItemsRemoved 
        += delegate(Object c, ItemCountEventArgs<T> args) { 
             removedCount += args.Count; 
           };
      coll.CollectionChanged 
        += delegate(Object c) { 
             if (removedCount > 0)
               Console.WriteLine("{0} items were removed", removedCount);
             removedCount = 0;
           };
    }

    // Event patterns on indexed collections

    public static void IndexedCollectionEvents() {
      IList<int> coll = new ArrayList<int>();
      ICollection<int> bag = new HashBag<int>();
      bag.AddAll(new int[] { 3, 2, 5, 5, 7, 7, 5, 3, 7, 7 });
      // Add item inserted handler
      coll.ItemInserted 
        += delegate(Object c, ItemAtEventArgs<int> args)  { 
             Console.WriteLine("Item {0} inserted at {1}", 
                               args.Item, args.Index); 
           };
      coll.InsertAll(0, bag);
      // Add item removed-at handler
      coll.ItemRemovedAt 
        += delegate(Object c, ItemAtEventArgs<int> args)  { 
             Console.WriteLine("Item {0} removed at {1}", 
                               args.Item, args.Index); 
           };
      coll.RemoveLast();
      coll.RemoveFirst();
      coll.RemoveAt(1);
    }

    // Recognizing Update event as a Removed-Added-Changed sequence

    private enum State { Before, Removed, Updated };

    private static void AddItemUpdatedHandler<T>(ICollection<T> coll) {
      State state = State.Before;
      T removed = default(T), added = default(T);
      coll.ItemsRemoved 
        += delegate(Object c, ItemCountEventArgs<T> args) { 
             if (state==State.Before) {
               state = State.Removed; 
               removed = args.Item;
             } else 
               state = State.Before;
           };
      coll.ItemsAdded 
        += delegate(Object c, ItemCountEventArgs<T> args) { 
             if (state==State.Removed) { 
               state = State.Updated; 
               added = args.Item; 
             } else 
               state = State.Before;
           };
      coll.CollectionChanged 
        += delegate(Object c)  { 
             if (state==State.Updated) 
               Console.WriteLine("Item {0} was updated to {1}", 
                                 removed, added);
             state = State.Before;
           };
    }

    public static void UpdateEvent() {
      ICollection<Teacher> coll = new HashSet<Teacher>();
      AddItemUpdatedHandler(coll);
      Teacher kristian = new Teacher("Kristian", "physics");
      coll.Add(kristian);
      coll.Add(new Teacher("Poul Einer", "mathematics"));
      // This should be caught by the update handler:
      coll.Update(new Teacher("Thomas", "mathematics"));
      // This should not be caught by the update handler:
      coll.Remove(kristian);
      coll.Add(new Teacher("Jens", "physics"));
      // The update handler is activated also by indexed updates
      IList<int> list = new ArrayList<int>();
      list.AddAll(new int[] { 7, 11, 13 });
      AddItemUpdatedHandler(list);
      list[1] = 9;
    }
  }

  // Example class where objects may be equal yet display differently

  class Teacher : IEquatable<Teacher> { 
    private readonly String name, subject;

    public Teacher(String name, String subject) { 
      this.name = name; this.subject = subject;
    }

    public bool Equals(Teacher that) {
      return this.subject.Equals(that.subject);
    }

    public override int GetHashCode() {
      return subject.GetHashCode();
    }

    public override String ToString() {
      return name + "[" + subject + "]";
    }
  }
}
