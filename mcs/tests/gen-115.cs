//-- ex-gen-class-linkedlist
//-- ex-anonymous-method-linkedlist
//-- ex-gen-printable
//-- ex-gen-interface-ilist
//-- ex-gen-linkedlist-map
//-- ex-gen-linkedlistenumerator
//-- ex-gen-delegate-fun

// A generic LinkedList class

using System;
using System.IO;                        // TextWriter
using System.Collections.Generic;       // IEnumerable<T>, IEnumerator<T>

public delegate R Mapper<A,R>(A x);

public interface IMyList<T> : IEnumerable<T> {
  int Count { get; }                    // Number of elements
  T this[int i] { get; set; }           // Get or set element at index i
  void Add(T item);                     // Add element at end
  void Insert(int i, T item);           // Insert element at index i
  void RemoveAt(int i);                 // Remove element at index i
  IMyList<U> Map<U>(Mapper<T,U> f);     // Map f over all elements
}

public class LinkedList<T> : IMyList<T> {
  protected int size;               // Number of elements in the list
  protected Node first, last;       // Invariant: first==null iff last==null

  protected class Node {
    public Node prev, next;
    public T item;

    public Node(T item) {
      this.item = item; 
    }

    public Node(T item, Node prev, Node next) {
      this.item = item; this.prev = prev; this.next = next; 
    }
  }

  public LinkedList() {
    first = last = null;
    size = 0;
  }

  public LinkedList(params T[] arr) : this() {
    foreach (T x in arr) 
      Add(x);
  }

  public int Count {
    get { return size; }
  }

  public T this[int i] {
    get { return get(i).item; }
    set { get(i).item = value; }
  }      

  private Node get(int n) {
    if (n < 0 || n >= size)
      throw new IndexOutOfRangeException();
    else if (n < size/2) {              // Closer to front
      Node node = first;
      for (int i=0; i<n; i++)
        node = node.next;
      return node;
    } else {                            // Closer to end
      Node node = last;
      for (int i=size-1; i>n; i--)
        node = node.prev;
      return node;
    }
  }

  public void Add(T item) { 
    Insert(size, item); 
  }

  public void Insert(int i, T item) { 
    if (i == 0) {
      if (first == null) // and thus last == null
        first = last = new Node(item);
      else {
        Node tmp = new Node(item, null, first);
        first.prev = tmp;
        first = tmp;
      }
      size++;
    } else if (i == size) {
      if (last == null) // and thus first = null
        first = last = new Node(item);
      else {
        Node tmp = new Node(item, last, null);
        last.next = tmp;
        last = tmp;
      }
      size++; 
    } else {
      Node node = get(i);
      // assert node.prev != null;
      Node newnode = new Node(item, node.prev, node);
      node.prev.next = newnode;
      node.prev = newnode;
      size++;
    }
  }

  public void RemoveAt(int i) {
    Node node = get(i);
    if (node.prev == null) 
      first = node.next;
    else
      node.prev.next = node.next;
    if (node.next == null) 
      last = node.prev;
    else
      node.next.prev = node.prev;       
    size--;
  }

  public override bool Equals(Object that) {
    if (that != null && GetType() == that.GetType() 
	&& this.size == ((IMyList<T>)that).Count) {
      Node thisnode = this.first;
      IEnumerator<T> thatenm = ((IMyList<T>)that).GetEnumerator();
      while (thisnode != null) {
        if (!thatenm.MoveNext())
          throw new ApplicationException("Impossible: LinkedList<T>.Equals");
        // assert MoveNext() was true (because of the above size test)
        if (!thisnode.item.Equals(thatenm.Current))
          return false;
        thisnode = thisnode.next; 
      }
      // assert !MoveNext(); // because of the size test
      return true;
    } else
      return false;
  }

  public override int GetHashCode() {
    int hash = 0;
    foreach (T x in this)
      hash ^= x.GetHashCode();
    return hash;
  }

  public static explicit operator LinkedList<T>(T[] arr) {
    return new LinkedList<T>(arr);
  }

  public static LinkedList<T> operator +(LinkedList<T> xs1, LinkedList<T> xs2) {
    LinkedList<T> res = new LinkedList<T>();
    foreach (T x in xs1) 
      res.Add(x);
    foreach (T x in xs2) 
      res.Add(x);
    return res;
  }

  public IMyList<U> Map<U>(Mapper<T,U> f) {
    LinkedList<U> res = new LinkedList<U>();
    foreach (T x in this) 
      res.Add(f(x));
    return res;
  }

  public IEnumerator<T> GetEnumerator() {
    return new LinkedListEnumerator(this);
  }

  private class LinkedListEnumerator : IEnumerator<T> {
    T curr;                     // The enumerator's current element
    bool valid;                 // Is the current element valid?
    Node next;                  // Node holding the next element, or null

    public LinkedListEnumerator(LinkedList<T> lst) {
      next = lst.first; valid = false;
    }
    
    public T Current {
      get { 
        if (valid) 
          return curr; 
        else
          throw new InvalidOperationException();
      }
    }
    
    public bool MoveNext() {
      if (next != null)  {
        curr = next.item; next = next.next; valid = true;
      } else 
        valid = false; 
      return valid;
    }

    public void Dispose() {
      curr = default(T); next = null; valid = false;
    }
  }
}

class SortedList<T> : LinkedList<T> where T : IComparable<T> {
  // Sorted insertion
  public void Insert(T x) { 
    Node node = first;
    while (node != null && x.CompareTo(node.item) > 0) 
      node = node.next;
    if (node == null)           // x > all elements; insert at end
      Add(x);
    else {                      // x <= node.item; insert before node
      Node newnode = new Node(x);
      if (node.prev == null)    // insert as first element
        first = newnode;
      else 
        node.prev.next = newnode;
      newnode.next = node;
      newnode.prev = node.prev;
      node.prev = newnode;
    }
  }
}

interface IPrintable {
  void Print(TextWriter fs);
}
class PrintableLinkedList<T> : LinkedList<T>, IPrintable where T : IPrintable {
  public void Print(TextWriter fs) {
    bool firstElement = true;
    foreach (T x in this) {
      x.Print(fs);
      if (firstElement) 
        firstElement = false;
      else
        fs.Write(", ");
    }
  }
}

class MyString : IComparable<MyString> {
  private readonly String s;
  public MyString(String s) {
    this.s = s;
  }
  public int CompareTo(MyString that) {
    return String.Compare(that.Value, s);       // Reverse ordering
  }
  public bool Equals(MyString that) {
    return that.Value == s;
  }
  public String Value {
    get { return s; }
  }
}

class MyTest {
  public static void Main(String[] args) {
    LinkedList<double> dLst = new LinkedList<double>(7.0, 9.0, 13.0, 0.0);
    foreach (double d in dLst)
      Console.Write("{0} ", d);
    Console.WriteLine();
    IMyList<int> iLst = 
      dLst.Map<int>(new Mapper<double, int>(Math.Sign));
    foreach (int i in iLst)
      Console.Write("{0} ", i);
    Console.WriteLine();
    IMyList<String> sLst = 
      dLst.Map<String>(delegate(double d) { return "s" + d; });
    foreach (String s in sLst)
      Console.Write("{0} ", s);
    Console.WriteLine();
    // Testing SortedList<MyString>
    SortedList<MyString> sortedLst = new SortedList<MyString>();
    sortedLst.Insert(new MyString("New York"));
    sortedLst.Insert(new MyString("Rome"));
    sortedLst.Insert(new MyString("Dublin"));
    sortedLst.Insert(new MyString("Riyadh"));
    sortedLst.Insert(new MyString("Tokyo"));
    foreach (MyString s in sortedLst)
      Console.Write("{0}   ", s.Value);
    Console.WriteLine();
  }
}
