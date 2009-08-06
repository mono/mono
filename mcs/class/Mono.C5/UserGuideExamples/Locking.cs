// C5 example: locking 2005-11-07

// Compile with 
//   csc /r:C5.dll Locking.cs 

using System;
using System.Threading;
using C5;
using SCG = System.Collections.Generic;

namespace Locking {
  class Locking {
    static ArrayList<int> coll = new ArrayList<int>();
    // static SCG.List<int> coll = new SCG.List<int>();
    static readonly int count = 1000;

    public static void Main(String[] args) {
      Console.WriteLine("Adding and removing without locking:");
      RunTwoThreads(delegate { AddAndRemove(15000); });
      Console.WriteLine("coll has {0} items, should be 0", coll.Count);

      coll = new ArrayList<int>();
      Console.WriteLine("Adding and removing with locking:");
      RunTwoThreads(delegate { SafeAddAndRemove(15000); });
      Console.WriteLine("coll has {0} items, should be 0", coll.Count);

      Console.WriteLine("Moving items without locking:");
      ArrayList<int> from, to;
      from = new ArrayList<int>();
      to = new ArrayList<int>();
      for (int i=0; i<count; i++) 
        from.Add(i);
      RunTwoThreads(delegate { while (!from.IsEmpty) Move(from, to); });
      Console.WriteLine("coll has {0} items, should be {1}", to.Count, count);

      Console.WriteLine("Moving items with locking:");
      from = new ArrayList<int>();
      to = new ArrayList<int>();
      for (int i=0; i<count; i++) 
        from.Add(i);
      RunTwoThreads(delegate { while (!from.IsEmpty) SafeMove(from, to); });
      Console.WriteLine("coll has {0} items, should be {1}", to.Count, count);
    }

    public static void RunTwoThreads(Act run) {
      Thread t1 = new Thread(new ThreadStart(run)),
             t2 = new Thread(new ThreadStart(run));
      t1.Start(); t2.Start();
      t1.Join(); t2.Join();
    }

    // Concurrently adding to and removing from an arraylist

    public static void AddAndRemove(int count) {
      for (int i=0; i<count; i++) 
        coll.Add(i);
      for (int i=0; i<count; i++)
        coll.Remove(i);
    }

    private static readonly Object sync = new Object();

    public static void SafeAddAndRemove(int count) {
      for (int i=0; i<count; i++) 
        lock (sync)
          coll.Add(i);
      for (int i=0; i<count; i++)
        lock (sync)
          coll.Remove(i);
    }

    public static void SafeAdd<T>(IExtensible<T> coll, T x) { 
      lock (sync) {
        coll.Add(x);
      }
    }

    public static void Move<T>(ICollection<T> from, ICollection<T> to) { 
      if (!from.IsEmpty) {  
        T x = from.Choose();
        Thread.Sleep(0);        // yield processor to other threads
        from.Remove(x);
        to.Add(x);
      }
    }
    
    public static void SafeMove<T>(ICollection<T> from, ICollection<T> to) { 
      lock (sync) 
        if (!from.IsEmpty) {  
          T x = from.Choose();
          Thread.Sleep(0);      // yield processor to other threads
          from.Remove(x);
          to.Add(x);
        }
    }
  }
}
