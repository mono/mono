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
  /// A utility class with functions for sorting arrays with respect to an IComparer&lt;T&gt;
  /// </summary>
  public class Sorting
  {
    Sorting() { }

    /// <summary>
    /// Sort part of array in place using IntroSort
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If the <code>start</code>
    /// and <code>count</code> arguments does not describe a valid range.</exception>
    /// <param name="array">Array to sort</param>
    /// <param name="start">Index of first position to sort</param>
    /// <param name="count">Number of elements to sort</param>
    /// <param name="comparer">IComparer&lt;T&gt; to sort by</param>
    [Tested]
    public static void IntroSort<T>(T[] array, int start, int count, SCG.IComparer<T> comparer)
    {
      if (start < 0 || count < 0 || start + count > array.Length)
        throw new ArgumentOutOfRangeException();
      new Sorter<T>(array, comparer).IntroSort(start, start + count);
    }

    /// <summary>
    /// Sort an array in place using IntroSort and default comparer
    /// </summary>
    /// <exception cref="NotComparableException">If T is not comparable</exception>
    /// <param name="array">Array to sort</param>
    [Tested]
    public static void IntroSort<T>(T[] array)
    {
      new Sorter<T>(array, Comparer<T>.Default).IntroSort(0, array.Length);
    }


    /// <summary>
    /// Sort part of array in place using Insertion Sort
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If the <code>start</code>
    /// and <code>count</code> arguments does not describe a valid range.</exception>
    /// <param name="array">Array to sort</param>
    /// <param name="start">Index of first position to sort</param>
    /// <param name="count">Number of elements to sort</param>
    /// <param name="comparer">IComparer&lt;T&gt; to sort by</param>
    [Tested]
    public static void InsertionSort<T>(T[] array, int start, int count, SCG.IComparer<T> comparer)
    {
      if (start < 0 || count < 0 || start + count > array.Length)
        throw new ArgumentOutOfRangeException();
      new Sorter<T>(array, comparer).InsertionSort(start, start + count);
    }


    /// <summary>
    /// Sort part of array in place using Heap Sort
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">If the <code>start</code>
    /// and <code>count</code> arguments does not describe a valid range.</exception>
    /// <param name="array">Array to sort</param>
    /// <param name="start">Index of first position to sort</param>
    /// <param name="count">Number of elements to sort</param>
    /// <param name="comparer">IComparer&lt;T&gt; to sort by</param>
    [Tested]
    public static void HeapSort<T>(T[] array, int start, int count, SCG.IComparer<T> comparer)
    {
      if (start < 0 || count < 0 || start + count > array.Length)
        throw new ArgumentOutOfRangeException();
      new Sorter<T>(array, comparer).HeapSort(start, start + count);
    }


    class Sorter<T>
    {
      T[] a;

      SCG.IComparer<T> c;


      internal Sorter(T[] a, SCG.IComparer<T> c) { this.a = a; this.c = c; }


      internal void IntroSort(int f, int b)
      {
        if (b - f > 31)
        {
          int depth_limit = (int)Math.Floor(2.5 * Math.Log(b - f, 2));

          introSort(f, b, depth_limit);
        }
        else
          InsertionSort(f, b);
      }


      private void introSort(int f, int b, int depth_limit)
      {
        const int size_threshold = 14;//24;

        if (depth_limit-- == 0)
          HeapSort(f, b);
        else if (b - f <= size_threshold)
          InsertionSort(f, b);
        else
        {
          int p = partition(f, b);

          introSort(f, p, depth_limit);
          introSort(p, b, depth_limit);
        }
      }


      private int compare(T i1, T i2) { return c.Compare(i1, i2); }


      private int partition(int f, int b)
      {
        int bot = f, mid = (b + f) / 2, top = b - 1;
        T abot = a[bot], amid = a[mid], atop = a[top];

        if (compare(abot, amid) < 0)
        {
          if (compare(atop, abot) < 0)//atop<abot<amid
          { a[top] = amid; amid = a[mid] = abot; a[bot] = atop; }
          else if (compare(atop, amid) < 0) //abot<=atop<amid
          { a[top] = amid; amid = a[mid] = atop; }
          //else abot<amid<=atop
        }
        else
        {
          if (compare(amid, atop) > 0) //atop<amid<=abot
          { a[bot] = atop; a[top] = abot; }
          else if (compare(abot, atop) > 0) //amid<=atop<abot
          { a[bot] = amid; amid = a[mid] = atop; a[top] = abot; }
          else //amid<=abot<=atop
          { a[bot] = amid; amid = a[mid] = abot; }
        }

        int i = bot, j = top;

        while (true)
        {
          while (compare(a[++i], amid) < 0) ;

          while (compare(amid, a[--j]) < 0) ;

          if (i < j)
          {
            T tmp = a[i]; a[i] = a[j]; a[j] = tmp;
          }
          else
            return i;
        }
      }


      internal void InsertionSort(int f, int b)
      {
        for (int j = f + 1; j < b; j++)
        {
          T key = a[j], other;
          int i = j - 1;

          if (c.Compare(other = a[i], key) > 0)
          {
            a[j] = other;
            while (i > f && c.Compare(other = a[i - 1], key) > 0) { a[i--] = other; }

            a[i] = key;
          }
        }
      }


      internal void HeapSort(int f, int b)
      {
        for (int i = (b + f) / 2; i >= f; i--) heapify(f, b, i);

        for (int i = b - 1; i > f; i--)
        {
          T tmp = a[f]; a[f] = a[i]; a[i] = tmp;
          heapify(f, i, f);
        }
      }


      private void heapify(int f, int b, int i)
      {
        T pv = a[i], lv, rv, max = pv;
        int j = i, maxpt = j;

        while (true)
        {
          int l = 2 * j - f + 1, r = l + 1;

          if (l < b && compare(lv = a[l], max) > 0) { maxpt = l; max = lv; }

          if (r < b && compare(rv = a[r], max) > 0) { maxpt = r; max = rv; }

          if (maxpt == j)
            break;

          a[j] = max;
          max = pv;
          j = maxpt;
        }

        if (j > i)
          a[j] = pv;
      }
    }
  }
}