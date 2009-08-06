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

// C5 example: WrappedArray 2005-07-21

// Compile with 
//   csc /r:C5.dll WrappedArray.cs 

using System;
using C5;
using SCG = System.Collections.Generic;

namespace WrappedArray {
  class WrappedArray {
    public static void Main(String[] args) {
    }


    // System.Array.Exists

    public static bool Exists<T>(T[] arr, Fun<T,bool> p) {
      return new WrappedArray<T>(arr).Exists(p);
    }  

    // System.Array.TrueForAll

    public static bool TrueForAll<T>(T[] arr, Fun<T,bool> p) {
      return new WrappedArray<T>(arr).All(p);
    }  

    // System.Array.Find(T[], Predicate)
    // This loses the valuable bool returned by C5 Find.

    public static T Find<T>(T[] arr, Fun<T,bool> p) {
      T res; 
      new WrappedArray<T>(arr).Find(p, out res);
      return res;
    }  

    // System.Array.FindAll(T[], Predicate)

    public static T[] FindAll<T>(T[] arr, Fun<T,bool> p) {
      return new WrappedArray<T>(arr).FindAll(p).ToArray();
    }  

    // System.Array.FindIndex(T[], Predicate)

    public static int FindIndex<T>(T[] arr, Fun<T,bool> p) {
      return new WrappedArray<T>(arr).FindIndex(p);
    }  

    // System.Array.FindIndex(T[], int, Predicate)

    public static int FindIndex<T>(T[] arr, int i, Fun<T,bool> p) {
      int j = new WrappedArray<T>(arr).View(i,arr.Length-i).FindIndex(p);
      return j < 0 ? j : j+i;
    }  

    // System.Array.FindIndex(T[], int, int, Predicate)

    public static int FindIndex<T>(T[] arr, int i, int n, Fun<T,bool> p) {
      int j = new WrappedArray<T>(arr).View(i,n).FindIndex(p);
      return j < 0 ? j : j+i;
    }  

    // System.Array.FindLast(T[], Predicate)
    // This loses the valuable bool returned by C5 Find.

    public static T FindLast<T>(T[] arr, Fun<T,bool> p) {
      T res; 
      new WrappedArray<T>(arr).FindLast(p, out res);
      return res;
    }  

    // System.Array.FindLastIndex(T[], Predicate)

    public static int FindLastIndex<T>(T[] arr, Fun<T,bool> p) {
      return new WrappedArray<T>(arr).FindIndex(p);
    }  

    // System.Array.FindLastIndex(T[], int, Predicate)

    public static int FindLastIndex<T>(T[] arr, int i, Fun<T,bool> p) {
      int j = new WrappedArray<T>(arr).View(i,arr.Length-i).FindIndex(p);
      return j < 0 ? j : j+i;
    }  

    // System.Array.FindLastIndex(T[], int, int, Predicate)

    public static int FindLastIndex<T>(T[] arr, int i, int n, Fun<T,bool> p) {
      int j = new WrappedArray<T>(arr).View(i,n).FindIndex(p);
      return j < 0 ? j : j+i;
    }  
    
    // System.Array.ForEach(T[], Action)

    public static void ForEach<T>(T[] arr, Act<T> act) {
      new WrappedArray<T>(arr).Apply(act);
    }  

    // System.Array.IndexOf(T[], T)

    public static int IndexOf<T>(T[] arr, T x) {
      int j = new WrappedArray<T>(arr).IndexOf(x);
      return j < 0 ? -1 : j;
    }  
    
    // System.Array.IndexOf(T[], T, int)

    public static int IndexOf<T>(T[] arr, T x, int i) {
      int j = new WrappedArray<T>(arr).View(i, arr.Length-i).IndexOf(x);
      return j < 0 ? -1 : j+i;
    }  
    
    // System.Array.IndexOf(T[], T, int, int)

    public static int IndexOf<T>(T[] arr, T x, int i, int n) {
      int j = new WrappedArray<T>(arr).View(i, n).IndexOf(x);
      return j < 0 ? -1 : j+i;
    }  

    // System.Array.LastIndexOf(T[], T)

    public static int LastIndexOf<T>(T[] arr, T x) {
      int j = new WrappedArray<T>(arr).LastIndexOf(x);
      return j < 0 ? -1 : j;
    }  
    
    // System.Array.LastIndexOf(T[], T, int)

    public static int LastIndexOf<T>(T[] arr, T x, int i) {
      int j = new WrappedArray<T>(arr).View(i, arr.Length-i).LastIndexOf(x);
      return j < 0 ? -1 : j+i;
    }  
    
    // System.Array.LastIndexOf(T[], T, int, int)

    public static int LastIndexOf<T>(T[] arr, T x, int i, int n) {
      int j = new WrappedArray<T>(arr).View(i, n).LastIndexOf(x);
      return j < 0 ? -1 : j+i;
    }  

    // System.Array.Sort(T[])

    public static void Sort<T>(T[] arr) {
      new WrappedArray<T>(arr).Sort();
    }  

    // System.Array.Sort(T[], int, int)

    public static void Sort<T>(T[] arr, int i, int n) {
      new WrappedArray<T>(arr).View(i, n).Sort();
    }  

    // System.Array.Sort(T[], SCG.IComparer<T>)

    public static void Sort<T>(T[] arr, SCG.IComparer<T> cmp) {
      new WrappedArray<T>(arr).Sort(cmp);
    }  
    
    // System.Array.Sort(T[], int, int, SCG.IComparer<T>)

    public static void Sort<T>(T[] arr, int i, int n, SCG.IComparer<T> cmp) {
      new WrappedArray<T>(arr).View(i, n).Sort(cmp);
    }  
    
    // System.Array.Sort(T[], Comparison)

    public static void Sort<T>(T[] arr, Comparison<T> csn) {
      new WrappedArray<T>(arr).Sort(new DelegateComparer<T>(csn));
    }  
  }
}
