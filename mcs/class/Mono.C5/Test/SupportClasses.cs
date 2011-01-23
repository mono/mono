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
using C5;
using NUnit.Framework;
using SCG = System.Collections.Generic;


namespace C5UnitTests
{
  class SC : SCG.IComparer<string>
  {
    public int Compare(string a, string b)
    {
      return a.CompareTo(b);
    }


    public void appl(String s)
    {
      System.Console.WriteLine("--{0}", s);
    }
  }

  class TenEqualityComparer : SCG.IEqualityComparer<int>, SCG.IComparer<int>
  {
    TenEqualityComparer() { }
    public static TenEqualityComparer Default { get { return new TenEqualityComparer(); } }
    public int GetHashCode(int item) { return (item / 10).GetHashCode(); }
    public bool Equals(int item1, int item2) { return item1 / 10 == item2 / 10; }
    public int Compare(int a, int b) { return (a / 10).CompareTo(b / 10); }
  }

  class IC : SCG.IComparer<int>, IComparable<int>, SCG.IComparer<IC>, IComparable<IC>
  {
    public int Compare(int a, int b)
    {
      return a > b ? 1 : a < b ? -1 : 0;
    }


    public int Compare(IC a, IC b)
    {
      return a._i > b._i ? 1 : a._i < b._i ? -1 : 0;
    }


    private int _i;


    public int i
    {
      get { return _i; }
      set { _i = value; }
    }


    public IC() { }


    public IC(int i) { _i = i; }


    public int CompareTo(int that) { return _i > that ? 1 : _i < that ? -1 : 0; }

    public bool Equals(int that) { return _i == that; }


    public int CompareTo(IC that) { return _i > that._i ? 1 : _i < that._i ? -1 : 0; }
    public bool Equals(IC that) { return _i == that._i; }


    public static bool eq(SCG.IEnumerable<int> me, params int[] that)
    {
      int i = 0, maxind = that.Length - 1;

      foreach (int item in me)
        if (i > maxind || item != that[i++])
          return false;

      return i == maxind + 1;
    }
    public static bool seteq(ICollectionValue<int> me, params int[] that)
    {
      int[] me2 = me.ToArray();

      Array.Sort(me2);

      int i = 0, maxind = that.Length - 1;

      foreach (int item in me2)
        if (i > maxind || item != that[i++])
          return false;

      return i == maxind + 1;
    }
    public static bool seteq(ICollectionValue<KeyValuePair<int, int>> me, params int[] that)
    {
      ArrayList<KeyValuePair<int, int>> first = new ArrayList<KeyValuePair<int, int>>();
      first.AddAll(me);
      ArrayList<KeyValuePair<int, int>> other = new ArrayList<KeyValuePair<int, int>>();
      for (int i = 0; i < that.Length; i += 2)
      {
        other.Add(new KeyValuePair<int, int>(that[i], that[i + 1]));
      }
      return other.UnsequencedEquals(first);
    }
  }

  class RevIC : SCG.IComparer<int>
  {
    public int Compare(int a, int b)
    {
      return a > b ? -1 : a < b ? 1 : 0;
    }
  }

  public class FunEnumerable : SCG.IEnumerable<int>
  {
    int size;
    Fun<int, int> f;

    public FunEnumerable(int size, Fun<int, int> f)
    {
      this.size = size; this.f = f;
    }

    public SCG.IEnumerator<int> GetEnumerator()
    {
      for (int i = 0; i < size; i++)
        yield return f(i);
    }


    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      throw new Exception("The method or operation is not implemented.");
    }

    #endregion
  }

  public class BadEnumerableException : Exception { }

  public class BadEnumerable<T> : CollectionValueBase<T>, ICollectionValue<T>
  {
    T[] contents;
    Exception exception;

    public BadEnumerable(Exception exception, params T[] contents)
    {
      this.contents = (T[])contents.Clone();
      this.exception = exception;
    }

    public override SCG.IEnumerator<T> GetEnumerator()
    {
      for (int i = 0; i < contents.Length; i++)
        yield return contents[i];
      throw exception;
    }

    public override bool IsEmpty { get { return false; } }

    public override int Count { get { return contents.Length + 1; } }

    public override Speed CountSpeed { get { return Speed.Constant; } }

    public override T Choose() { throw exception; }
  }

  public class CollectionEventList<T>
  {
    ArrayList<CollectionEvent<T>> happened;
    EventTypeEnum listenTo;
    SCG.IEqualityComparer<T> itemequalityComparer;
    public CollectionEventList(SCG.IEqualityComparer<T> itemequalityComparer)
    {
      happened = new ArrayList<CollectionEvent<T>>();
      this.itemequalityComparer = itemequalityComparer;
    }
    public void Listen(ICollectionValue<T> list, EventTypeEnum listenTo)
    {
      this.listenTo = listenTo;
      if ((listenTo & EventTypeEnum.Changed) != 0)
        list.CollectionChanged += new CollectionChangedHandler<T>(changed);
      if ((listenTo & EventTypeEnum.Cleared) != 0)
        list.CollectionCleared += new CollectionClearedHandler<T>(cleared);
      if ((listenTo & EventTypeEnum.Removed) != 0)
        list.ItemsRemoved += new ItemsRemovedHandler<T>(removed);
      if ((listenTo & EventTypeEnum.Added) != 0)
        list.ItemsAdded += new ItemsAddedHandler<T>(added);
      if ((listenTo & EventTypeEnum.Inserted) != 0)
        list.ItemInserted += new ItemInsertedHandler<T>(inserted);
      if ((listenTo & EventTypeEnum.RemovedAt) != 0)
        list.ItemRemovedAt += new ItemRemovedAtHandler<T>(removedAt);
    }
    public void Add(CollectionEvent<T> e) { happened.Add(e); }
    /// <summary>
    /// Check that we have seen exactly the events in expected that match listenTo.
    /// </summary>
    /// <param name="expected"></param>
    public void Check(SCG.IEnumerable<CollectionEvent<T>> expected)
    {
      int i = 0;
      foreach (CollectionEvent<T> expectedEvent in expected)
      {
        if ((expectedEvent.Act & listenTo) == 0)
          continue;
        if (i >= happened.Count)
          Assert.Fail(string.Format("Event number {0} did not happen:\n expected {1}", i, expectedEvent));
        if (!expectedEvent.Equals(happened[i], itemequalityComparer))
          Assert.Fail(string.Format("Event number {0}:\n expected {1}\n but saw {2}", i, expectedEvent, happened[i]));
        i++;
      }
      if (i < happened.Count)
        Assert.Fail(string.Format("Event number {0} seen but no event expected:\n {1}", i, happened[i]));
      happened.Clear();
    }
    public void Clear() { happened.Clear(); }
    public void Print(System.IO.TextWriter writer)
    {
      happened.Apply(delegate(CollectionEvent<T> e) { writer.WriteLine(e); });
    }
    void changed(object sender)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.Changed, new EventArgs(), sender));
    }
    void cleared(object sender, ClearedEventArgs eventArgs)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.Cleared, eventArgs, sender));
    }
    void added(object sender, ItemCountEventArgs<T> eventArgs)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.Added, eventArgs, sender));
    }
    void removed(object sender, ItemCountEventArgs<T> eventArgs)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.Removed, eventArgs, sender));
    }
    void inserted(object sender, ItemAtEventArgs<T> eventArgs)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.Inserted, eventArgs, sender));
    }
    void removedAt(object sender, ItemAtEventArgs<T> eventArgs)
    {
      happened.Add(new CollectionEvent<T>(EventTypeEnum.RemovedAt, eventArgs, sender));
    }
  }

  public sealed class CollectionEvent<T>
  {
    public readonly EventTypeEnum Act;
    public readonly EventArgs Args;
    public readonly object Sender;

    public CollectionEvent(EventTypeEnum act, EventArgs args, object sender)
    {
      this.Act = act;
      this.Args = args;
      this.Sender = sender;
    }

    public bool Equals(CollectionEvent<T> otherEvent, SCG.IEqualityComparer<T> itemequalityComparer)
    {
      if (otherEvent == null || Act != otherEvent.Act || !object.ReferenceEquals(Sender, otherEvent.Sender))
        return false;
      switch (Act)
      {
        case EventTypeEnum.None:
          break;
        case EventTypeEnum.Changed:
          return true;
        case EventTypeEnum.Cleared:
          if (Args is ClearedRangeEventArgs)
          {
            ClearedRangeEventArgs a = Args as ClearedRangeEventArgs, o = otherEvent.Args as ClearedRangeEventArgs;
            if (o == null)
              return false;
            return a.Full == o.Full && a.Start == o.Start && a.Count == o.Count;
          }
          else
          {
            if (otherEvent.Args is ClearedRangeEventArgs)
              return false;
            ClearedEventArgs a = Args as ClearedEventArgs, o = otherEvent.Args as ClearedEventArgs;
            return a.Full == o.Full && a.Count == o.Count;
          }
        case EventTypeEnum.Added:
          {
            ItemCountEventArgs<T> a = Args as ItemCountEventArgs<T>, o = otherEvent.Args as ItemCountEventArgs<T>;
            return itemequalityComparer.Equals(a.Item, o.Item) && a.Count == o.Count;
          }
        case EventTypeEnum.Removed:
          {
            ItemCountEventArgs<T> a = Args as ItemCountEventArgs<T>, o = otherEvent.Args as ItemCountEventArgs<T>;
            return itemequalityComparer.Equals(a.Item, o.Item) && a.Count == o.Count;
          }
        case EventTypeEnum.Inserted:
          {
            ItemAtEventArgs<T> a = Args as ItemAtEventArgs<T>, o = otherEvent.Args as ItemAtEventArgs<T>;
            return a.Index == o.Index && itemequalityComparer.Equals(a.Item, o.Item);
          }
        case EventTypeEnum.RemovedAt:
          {
            ItemAtEventArgs<T> a = Args as ItemAtEventArgs<T>, o = otherEvent.Args as ItemAtEventArgs<T>;
            return a.Index == o.Index && itemequalityComparer.Equals(a.Item, o.Item);
          }
      }
      throw new ApplicationException("Illegal Act: " + Act);
    }

    public override string ToString()
    {
      return string.Format("Act: {0}, Args : {1}, Source : {2}", Act, Args, Sender);
    }

  }

  public class CHC
  {
    static public int unsequencedhashcode(params int[] a)
    {
      int h = 0;
      foreach (int i in a)
      {
        h += (int)(((uint)i * 1529784657 + 1) ^ ((uint)i * 2912831877) ^ ((uint)i * 1118771817 + 2));
      }
      return h;
    }
    static public int sequencedhashcode(params int[] a)
    {
      int h = 0;
      foreach (int i in a) { h = h * 31 + i; }
      return h;
    }
  }

  //This class is a modified sample from VS2005 beta1 documentation
  public class RadixFormatProvider : IFormatProvider
  {
    RadixFormatter _radixformatter;
    public RadixFormatProvider(int radix)
    {
      if (radix < 2 || radix > 36)
        throw new ArgumentException(String.Format(
            "The radix \"{0}\" is not in the range 2..36.",
            radix));
      _radixformatter = new RadixFormatter(radix);
    }
    public object GetFormat(Type argType)
    {
      if (argType == typeof(ICustomFormatter))
        return _radixformatter;
      else
        return null;
    }
  }

  //This class is a modified sample from VS2005 beta1 documentation
  public class RadixFormatter : ICustomFormatter
  {
    int radix;
    public RadixFormatter(int radix)
    {
      if (radix < 2 || radix > 36)
        throw new ArgumentException(String.Format(
            "The radix \"{0}\" is not in the range 2..36.",
            radix));
      this.radix = radix;
    }

    // The value to be formatted is returned as a signed string 
    // of digits from the rDigits array. 
    private static char[] rDigits = {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 
        'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 
        'U', 'V', 'W', 'X', 'Y', 'Z' };

    public string Format(string formatString,
        object argToBeFormatted, IFormatProvider provider)
    {
      /*switch (Type.GetTypeCode(argToBeFormatted.GetType()))
      {
        case TypeCode.Boolean:
          break;
        case TypeCode.Byte:
          break;
        case TypeCode.Char:
          break;
        case TypeCode.DBNull:
          break;
        case TypeCode.DateTime:
          break;
        case TypeCode.Decimal:
          break;
        case TypeCode.Double:
          break;
        case TypeCode.Empty:
          break;
        case TypeCode.Int16:
          break;
        case TypeCode.Int32:
          break;
        case TypeCode.Int64:
          break;
        case TypeCode.Object:
          break;
        case TypeCode.SByte:
          break;
        case TypeCode.Single:
          break;
        case TypeCode.String:
          break;
        case TypeCode.UInt16:
          break;
        case TypeCode.UInt32:
          break;
        case TypeCode.UInt64:
          break;
      }*/
      int intToBeFormatted;
      try
      {
        intToBeFormatted = (int)argToBeFormatted;
      }
      catch (Exception)
      {
        if (argToBeFormatted is IFormattable)
          return ((IFormattable)argToBeFormatted).
              ToString(formatString, provider);
        else
          return argToBeFormatted.ToString();
      }
      return formatInt(intToBeFormatted);
    }

    private string formatInt(int intToBeFormatted)
    {
      // The formatting is handled here.
      if (intToBeFormatted == 0)
        return "0";
      int digitIndex = 0;
      int intPositive;
      char[] outDigits = new char[31];

      // Verify that the argument can be converted to a int integer.
      // Extract the magnitude for conversion.
      intPositive = Math.Abs(intToBeFormatted);

      // Convert the magnitude to a digit string.
      for (digitIndex = 0; digitIndex <= 32; digitIndex++)
      {
        if (intPositive == 0) break;

        outDigits[outDigits.Length - digitIndex - 1] =
            rDigits[intPositive % radix];
        intPositive /= radix;
      }

      // Add a minus sign if the argument is negative.
      if (intToBeFormatted < 0)
        outDigits[outDigits.Length - digitIndex++ - 1] =
            '-';

      return new string(outDigits,
          outDigits.Length - digitIndex, digitIndex);
    }
  }

}
