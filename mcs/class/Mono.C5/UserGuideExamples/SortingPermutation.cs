// C5 example
// 2004-11

using System;
using C5;
using SCG = System.Collections.Generic;

namespace SortingPermutation
{
  class MyTest
  {
    public static void Main(String[] args)
    {
      String[] cities = 
      { "Tokyo", "Beijing", "Hangzhou", "Kyoto", "Beijing", "Copenhagen", "Seattle" };
      IList<String> alst = new ArrayList<String>();
      alst.AddAll<String>(cities);
      foreach (int i in MySort.GetPermutation1(alst))
        Console.Write("{0} ", i);
      Console.WriteLine();
      IList<String> llst = new LinkedList<String>();
      llst.AddAll<String>(cities);
      foreach (int i in MySort.GetPermutation2(llst))
        Console.Write("{0} ", i);
      Console.WriteLine();
      Console.WriteLine("The rank of the cities:");
      ArrayList<int> res = MySort.GetPermutation1(MySort.GetPermutation2(llst));
      foreach (int i in res)
        Console.Write("{0} ", i);
      Console.WriteLine();
    }
  }

  class MySort
  {
    // Fast for array lists and similar, but not stable; slow for linked lists

    public static ArrayList<int> GetPermutation1<T>(IList<T> lst)
      where T : IComparable<T>
    {
      ArrayList<int> res = new ArrayList<int>(lst.Count);
      for (int i = 0; i < lst.Count; i++)
        res.Add(i);
      res.Sort(new DelegateComparer<int>
               (delegate(int i, int j) { return lst[i].CompareTo(lst[j]); }));
      return res;
    }

    // Stable and fairly fast both for array lists and linked lists, 
    // but does copy the collection's items. 

    public static ArrayList<int> GetPermutation2<T>(IList<T> lst)
      where T : IComparable<T>
    {
      int i = 0;
      IList<KeyValuePair<T, int>> zipList =
        lst.Map<KeyValuePair<T, int>>
            (delegate(T x) { return new KeyValuePair<T, int>(x, i++); });
      zipList.Sort(new KeyValueComparer<T>(lst));
      ArrayList<int> res = new ArrayList<int>(lst.Count);
      foreach (KeyValuePair<T, int> p in zipList)
        res.Add(p.Value);
      return res;
    }

    private class KeyValueComparer<T> : SCG.IComparer<KeyValuePair<T, int>>
      where T : IComparable<T>
    {
      private readonly IList<T> lst;
      public KeyValueComparer(IList<T> lst)
      {
        this.lst = lst;
      }
      public int Compare(KeyValuePair<T, int> p1, KeyValuePair<T, int> p2)
      {
        return p1.Key.CompareTo(p2.Key);
      }
    }
  }
}
