// SortedListTest.cs - NUnit Test Cases for the System.Collections.SortedList class
//
// Authors:
//      Jaak Simm
//      Duncan Mak (duncan@ximian.com)
//
// Thanks go to David Brandt (bucky@keystreams.com),
// because this file is based on his ArrayListTest.cs
//
// (C) Ximian, Inc.  http://www.ximian.com
// 
// main TODO: additional tests for functions affected by
//            fixedsize and read-only properties 


using System;
using System.Collections;

using NUnit.Framework;


namespace MonoTests.System.Collections {


/// <summary>SortedList test.</summary>
[TestFixture]
public class SortedListTest : Assertion {
	protected SortedList sl1;
	protected SortedList sl2;
	protected SortedList emptysl;
	protected const int icap=16;

        [SetUp]
	public  void SetUp() 
	{
	}

        [TearDown]
	public void TearDown() 
	{
	}

	public void TestConstructor1() {
		SortedList temp1 = new SortedList();
		AssertNotNull("sl.constructor-1: returns null", temp1);
		AssertEquals("sl.constructor-1: incorrect initial capacity", icap, temp1.Capacity);
	}

        [Test]
	public void TestConstructor2() {
		Comparer c = Comparer.Default;
		SortedList temp1 = new SortedList(c);
		AssertNotNull("sl.constructor-2: returns null", temp1);
		AssertEquals("sl.constructor-2: incorrect initial capacity", icap, temp1.Capacity);
	}

        [Test]
	public void TestConstructor3() {
		Hashtable d = new Hashtable();
		d.Add("one", "Mircosoft");
		d.Add("two", "will");
		d.Add("three", "rule");
		d.Add("four", "the world");
		
		SortedList temp1 = new SortedList(d);
		AssertNotNull("sl.constructor-3: returns null", temp1);
		AssertEquals("sl.constructor-3: incorrect initial capacity", 4, temp1.Capacity);
		AssertEquals("sl.constructor-3: incorrect count", 4, temp1.Count);

		try {
			d=null;
			temp1 = new SortedList(d);
			Fail ("sl.constructor-3: does not throw ArgumentNullException");
		} catch (ArgumentNullException) {}
		try {
			d = new Hashtable();
			d.Add("one", "Mircosoft");
			d.Add("two", "will");
			d.Add("three", "rule");
			d.Add("four", "the world");
			d.Add(7987,"lkj");
			temp1 = new SortedList(d);
			Fail ("sl.constructor-3: does not throw InvalidCastException");
		} catch (InvalidOperationException) {
		} catch (Exception e) {
			Fail ("Unexpected Exception throw: e=" + e);
		}
	}

        [Test]	
	public void TestConstructor4() {
		SortedList temp1 = new SortedList(17);
		AssertNotNull("sl.constructor-4: returns null", temp1);
		AssertEquals("sl.constructor-4: incorrect initial capacity", temp1.Capacity, 17);
		try {
			temp1 = new SortedList(-6);
			Fail ("sl.constructor-4: does not throw ArgumentOutOfRangeException, with negative values");
		} catch (ArgumentOutOfRangeException) {}
		try {
			temp1 = new SortedList(0);
		} catch (ArgumentOutOfRangeException) {
			Fail ("sl.constructor-4: throws ArgumentOutOfRangeException with 0");
		}
	}

        [Test]	
	public void TestConstructor5() {
		Comparer c = Comparer.Default;
		SortedList temp1 = new SortedList(c,27);
		AssertNotNull("sl.constructor-5: returns null", temp1);
		AssertEquals("sl.constructor-5: incorrect initial capacity", temp1.Capacity, 27);
		try {
			temp1 = new SortedList(-12);
			Fail ("sl.constructor-5: does not throw ArgumentOutOfRangeException, with negative values");
		} catch (ArgumentOutOfRangeException) {}
	}

        [Test]	
	public void TestIsSynchronized() {
		SortedList sl1 = new SortedList();
		Assert("sl: should not be synchronized by default", 
		       !sl1.IsSynchronized);
		SortedList sl2 = SortedList.Synchronized(sl1);
		Assert("sl: synchronized wrapper not working", sl2.IsSynchronized);
	}

        [Test]	
	public void TestCapacity() {
		for (int i = 0; i < 100; i++) {
			SortedList sl1 = new SortedList(i);
			AssertEquals("Bad capacity of " + i,
				     i, sl1.Capacity);
		}
	}

        [Test]
        public void TestCapacity2 ()
        {
                SortedList list = new SortedList ();
                list.Capacity = 5;

                AssertEquals (5, list.Capacity);
        }

        [Test]
        public void TestCapacity3 ()
        {
                SortedList list = new SortedList (1000);
                list.Capacity = 5;

                AssertEquals (16, list.Capacity);
        }

        [Test]
        [ExpectedException (typeof (OutOfMemoryException))]
        [Ignore ("This is not implemented in the runtime yet")]
        public void TestCapacity4 ()
        {
                SortedList list = new SortedList ();
                list.Capacity = Int32.MaxValue;
        }

        [Test]	
	public void TestCount() {
		{
			SortedList sl1 = new SortedList();
			AssertEquals("Bad initial count",
				     0, sl1.Count);
			for (int i = 1; i <= 100; i++) {
				sl1.Add(""+i,""+i);
				AssertEquals("Bad count " + i,
					     i, sl1.Count);
			}
		}
	}

        [Test]	
	public void TestIsFixed() {
		SortedList sl1 = new SortedList();
		Assert("should not be fixed by default", !sl1.IsFixedSize);
	}


        [Test]	
	public void TestIsReadOnly() {
		SortedList sl1 = new SortedList();
		Assert("should not be ReadOnly by default", !sl1.IsReadOnly);
	}


        [Test]	
	public void TestItem() {
		SortedList sl1 = new SortedList();
		string key = null;
		{
			try {
				object o = sl1[-1];
			} catch (ArgumentNullException) {
				Fail ("sl.Item: throws ArgumentNullException with negative values");
			}
			try {
				object o = sl1[key];
				Fail ("sl.Item: does not throw ArgumentNullException with null key");
			} catch (ArgumentNullException) {}
		}

		for (int i = 0; i <= 100; i++) {
			sl1.Add("kala "+i,i);
		}
		for (int i = 0; i <= 100; i++) {
			AssertEquals("sl.Item: item not fetched for " + i,
				     i, sl1["kala "+i]);
		}
	}

        [Test]
        public void TestSyncRoot()
        {
                SortedList sl1 = new SortedList();
		AssertEquals("sl.SyncRoot: does not function",false, sl1.SyncRoot == null);
		/*
		lock( sl1.SyncRoot ) {
			foreach ( Object item in sl1 ) {
				item="asdf";
				Assert ("sl.SyncRoot: item not read-only",item.IsReadOnly);
 			}
		}
		*/
        }

        [Test]
	public void TestValues()
	{
  	SortedList sl1 = new SortedList();
		ICollection ic1 = sl1.Values;
		for (int i = 0; i <= 100; i++) {
			sl1.Add("kala "+i,i);
			AssertEquals("sl.Values: .Values has different count",ic1.Count,sl1.Count);
		}
	}
	
	
	// TODO: Add with IComparer
        [Test]
	public void TestAdd() {
		// seems SortedList cannot be set fixedsize or readonly
		SortedList sl1 = new SortedList();
		string key = null;
		{
			try {
				sl1.Add(key,"kala");
				Fail ("sl.Add: does not throw ArgumentNullException with null key");
			} catch (ArgumentNullException) {}
		}

		{
			for (int i = 1; i <= 100; i++) {
				sl1.Add("kala "+i,i);
				AssertEquals("sl.Add: incorrect count",i,sl1.Count);
				AssertEquals("sl.Add: incorrect value",i,sl1["kala "+i]);
			}
		}
		{
			try {
				sl1.Add("kala",10);
				sl1.Add("kala",11);
				Fail ("sl.Add: does not throw ArgumentException when adding existing key");
			} catch (ArgumentException) {}
		}
	}

        [Test]
	public void TestClear() {
		SortedList sl1 = new SortedList(10);
		sl1.Add("kala", 'c');
		sl1.Add("kala2", 'd');
		AssertEquals("sl.Clear: capacity is incorrect", 10, sl1.Capacity);
		AssertEquals("sl.Clear: should have one element", 2, sl1.Count);
		sl1.Clear();
		AssertEquals("sl.Clear: is not cleared", 0, sl1.Count);
		AssertEquals("sl.Clear: capacity is altered", 16, sl1.Capacity);
	}

        [Test]
	public void TestClone() {
		{
			SortedList sl1 = new SortedList(10);
			for (int i = 0; i <= 50; i++) {sl1.Add("kala "+i,i);}
			SortedList sl2 = (SortedList)sl1.Clone();
			for (int i = 0; i <= 50; i++) {
				AssertEquals("sl.Clone: copying failed @"+i, sl1["kala "+i], sl2["kala "+i]);
			}
		}
		{
			char[] d10 = {'a', 'b'};
			char[] d11 = {'a', 'c'};
			char[] d12 = {'b', 'c'};
			//char[][] d1 = {d10, d11, d12};
			SortedList sl1 = new SortedList();
			sl1.Add("d1",d10);
			sl1.Add("d2",d11);
			sl1.Add("d3",d12);
			SortedList sl2 = (SortedList)sl1.Clone();
			AssertEquals("sl.Clone: Array not matching", sl1["d1"], sl2["d1"]);
			AssertEquals("sl.Clone: Array not matching", sl1["d2"], sl2["d2"]);
			AssertEquals("sl.Clone: Array not matching", sl1["d3"], sl2["d3"]);
			
			((char[])sl1["d1"])[0] = 'z';
			AssertEquals("s1.Clone: shallow copy", sl1["d1"], sl2["d1"]);
		}
	}

        [Test]
	public void TestContains() {
		SortedList sl1 = new SortedList(55);
		for (int i = 0; i <= 50; i++) {sl1.Add("kala "+i,i);}

		try {
			if (sl1.Contains(null)){}
			Fail ("sl.Contains: does not throw ArgumentNullException with null key");
		} catch (ArgumentNullException) {}
		
		Assert("sl.Contains: can't find existing key", sl1.Contains("kala 17"));
		Assert("sl.Contains: finds non-existing key", !sl1.Contains("ohoo"));
	}

        [Test]
	public void TestContainsKey() {
		SortedList sl1 = new SortedList(55);
		for (int i = 0; i <= 50; i++) {sl1.Add("kala "+i,i);}

		try {
			if (sl1.ContainsKey(null)){}
			Fail ("sl.ContainsKey: does not throw ArgumentNullException with null key");
		} catch (ArgumentNullException) {}
		
		Assert("sl.ContainsKey: can't find existing key", sl1.ContainsKey("kala 17"));
		Assert("sl.ContainsKey: finds non-existing key", !sl1.ContainsKey("ohoo"));
	}

        [Test]
	public void TestContainsValue() {
		SortedList sl1 = new SortedList(55);
                sl1.Add(0, "zero");
                sl1.Add(1, "one");
                sl1.Add(2, "two");
                sl1.Add(3, "three");
                sl1.Add(4, "four");

		Assert("sl.ContainsValue: can't find existing value", sl1.ContainsValue("zero"));
		Assert("sl.ContainsValue: finds non-existing value", !sl1.ContainsValue("ohoo"));
		Assert("sl.ContainsValue: finds non-existing value", !sl1.ContainsValue(null));
	}

        [Test]	
	public void TestCopyTo() {
		SortedList sl1 = new SortedList();
		for (int i = 0; i <= 10; i++) {sl1.Add("kala "+i,i);}
		{
			try {
				sl1.CopyTo(null, 2);
				Fail("sl.CopyTo: does not throw ArgumentNullException when target null");
			} catch (ArgumentNullException) {}
		}
		{
			try {
				Char[,] c2 = new Char[2,2];
				sl1.CopyTo(c2, 2);
				Fail("sl.CopyTo: does not throw ArgumentException when target is multiarray");
			} catch (ArgumentException) {}
		}
		{
			try {
				Char[] c1 = new Char[2];
				sl1.CopyTo(c1, -2);
				Fail("sl.CopyTo: does not throw ArgumentOutOfRangeException when index is negative");
			} catch (ArgumentOutOfRangeException) {}
		}
		{
			try {
				Char[] c1 = new Char[2];
				sl1.CopyTo(c1, 3);
				Fail("sl.CopyTo: does not throw ArgumentException when index is too large");
			} catch (ArgumentException) {}
		}
		{
			try {
				Char[] c1 = new Char[2];
				sl1.CopyTo(c1, 1);
				Fail("sl.CopyTo: does not throw ArgumentException when SortedList too big for the array");
			} catch (ArgumentException) {}
		}
		{
			try {
				Char[] c2 = new Char[15];
				sl1.CopyTo(c2, 0);
				Fail("sl.CopyTo: does not throw InvalidCastException when incompatible data types");
			} catch (InvalidCastException) {}
		}

		// CopyTo function does not work well with SortedList
		// even example at MSDN gave InvalidCastException
		// thus, it is NOT tested here
		/*
				sl1.Clear();
				for (int i = 0; i <= 5; i++) {sl1.Add(i,""+i);}
		    Char[] copy = new Char[15];
		    Array.Clear(copy,0,copy.Length);
		    copy.SetValue( "The", 0 );
		    copy.SetValue( "quick", 1 );
		    copy.SetValue( "brown", 2 );
		    copy.SetValue( "fox", 3 );
		    copy.SetValue( "jumped", 4 );
		    copy.SetValue( "over", 5 );
		    copy.SetValue( "the", 6 );
		    copy.SetValue( "lazy", 7 );
		    copy.SetValue( "dog", 8 );
				sl1.CopyTo(copy,1);
				AssertEquals("sl.CopyTo: incorrect copy(1).","The", copy.GetValue(0));
				AssertEquals("sl.CopyTo: incorrect copy(1).","quick", copy.GetValue(1));
				for (int i=2; i<8; i++) AssertEquals("sl.CopyTo: incorrect copy(2).",sl1["kala "+(i-2)], copy.GetValue(i));
				AssertEquals("sl.CopyTo: incorrect copy(3).","dog", copy.GetValue(8));
		*/
	}

	public SortedList DefaultSL() {
		SortedList sl1 = new SortedList();
		sl1.Add( 1.0, "The" );
		sl1.Add( 1.1, "quick" );
		sl1.Add( 34.0, "brown" );
		sl1.Add( -100.75, "fox" );
		sl1.Add( 1.4, "jumped" );
		sl1.Add( 1.5, "over" );
		sl1.Add( 1.6, "the" );
		sl1.Add( 1.7, "lazy" );
		sl1.Add( 1.8, "dog" );
		return sl1;
	}
	
	public IList DefaultValues() {
		IList il = new ArrayList();
		il.Add( "fox" );
		il.Add( "The" );
		il.Add( "quick" );
		il.Add( "jumped" );
		il.Add( "over" );
		il.Add( "the" );
		il.Add( "lazy" );
		il.Add( "dog" );
		il.Add( "brown" );
		return il;
	}

        [Test]	
	public void TestGetByIndex() {
		SortedList sl1 = DefaultSL();
		AssertEquals("cl.GetByIndex: failed(1)",sl1.GetByIndex(4),"over");
		AssertEquals("cl.GetByIndex: failed(2)",sl1.GetByIndex(8),"brown");
		try {
			sl1.GetByIndex(-1);
			Fail("sl.GetByIndex: does not throw ArgumentOutOfRangeException with negative index");
		} catch (ArgumentOutOfRangeException) {}
		try {
			sl1.GetByIndex(100);
			Fail("sl.GetByIndex: does not throw ArgumentOutOfRangeException with too large index");
		} catch (ArgumentOutOfRangeException) {}
	}

        [Test]
	public void TestGetEnumerator() {
		SortedList sl1 = DefaultSL();
		IDictionaryEnumerator e = sl1.GetEnumerator();
		AssertNotNull("sl.GetEnumerator: does not return enumerator", e);
		AssertEquals("sl.GetEnumerator: enumerator not working(1)",e.MoveNext(),true);
		AssertNotNull("sl.GetEnumerator: enumerator not working(2)",e.Current);
	}

        [Test]
	public void TestGetKey() {
		SortedList sl1 = DefaultSL();
		AssertEquals("sl.GetKey: failed(1)",sl1.GetKey(4),1.5);
		AssertEquals("sl.GetKey: failed(2)",sl1.GetKey(8),34.0);
		try {
			sl1.GetKey(-1);
			Fail("sl.GetKey: does not throw ArgumentOutOfRangeException with negative index");
		} catch (ArgumentOutOfRangeException) {}
		try {
			sl1.GetKey(100);
			Fail("sl.GetKey: does not throw ArgumentOutOfRangeException with too large index");
		} catch (ArgumentOutOfRangeException) {}
	}
        
        [Test]
	public void TestGetKeyList() {
		SortedList sl1 = DefaultSL();
		IList keys = sl1.GetKeyList();
		AssertNotNull("sl.GetKeyList: does not return keylist", keys);
		Assert("sl.GetKeyList: keylist is not readonly", keys.IsReadOnly);
		AssertEquals("sl.GetKeyList: incorrect keylist size",keys.Count,9);
		AssertEquals("sl.GetKeyList: incorrect key(1)",keys[3],1.4);
		sl1.Add(33.9,"ehhe");
		AssertEquals("sl.GetKeyList: incorrect keylist size",keys.Count,10);
		AssertEquals("sl.GetKeyList: incorrect key(2)",keys[8],33.9);
	}

        [Test]
	public void TestGetValueList() {
		SortedList sl1 = DefaultSL();
		IList originalvals = DefaultValues();
		IList vals = sl1.GetValueList();
		AssertNotNull("sl.GetValueList: does not return valuelist", vals);
		Assert("sl.GetValueList: valuelist is not readonly", vals.IsReadOnly);
		AssertEquals("sl.GetValueList: incorrect valuelist size",vals.Count,sl1.Count);
		for (int i=0; i<sl1.Count; i++) {
			AssertEquals("sl.GetValueList: incorrect key(1)",vals[i],originalvals[i]);
		}

		sl1.Add(0.01,"ehhe");
		AssertEquals("sl.GetValueList: incorrect valuelist size",vals.Count,10);
		AssertEquals("sl.GetValueList: incorrect value(2)",vals[8],"dog");
	}

	// TODO: IEnumerable.GetEnumerator [Explicit Interface Implementation]
	/*
	public void TestIEnumerable_GetEnumerator() {
		SortedList sl1 = DefaultSL();
		IEnumerator e = sl1.IEnumerable.GetEnumerator();
		AssertNotNull("sl.GetEnumerator: does not return enumerator", e);
		AssertEquals("sl.GetEnumerator: enumerator not working(1)",e.MoveNext(),true);
		AssertNotNull("sl.GetEnumerator: enumerator not working(2)",e.Current);
	}
	*/

        [Test]
	public void TestIndexOfKey() {
		SortedList sl1 = new SortedList(24);
		string s=null;
		int t;
		for (int i = 0; i <= 50; i++) {
			s=string.Format("{0:D2}", i); 
			sl1.Add("kala "+s,i);
		}
		AssertEquals("sl.IndexOfKey: does not return -1 for non-existing key", -1, sl1.IndexOfKey("kala "));
		s=null;
		try {
			t=sl1.IndexOfKey(s);
			Fail("sl.IndexOfKey: ArgumentNullException not caught, when key is null");
		}
		catch (ArgumentNullException) {}
		try {
			t=sl1.IndexOfKey(10);
			Fail("sl.IndexOfKey: InvalidOperationException not caught, when key invalid");
		}
		catch (InvalidOperationException) {}
		for (int i=0; i<=50; i++) {
			s=string.Format("{0:D2}", i); 
			AssertEquals("sl.IndexOfKey: incorrect index key", i, sl1.IndexOfKey("kala "+s));
		}
	}

        [Test]
	public void TestIndexOfValue() {
		SortedList sl1 = new SortedList(24);
		string s=null;
		for (int i = 0; i < 50; i++) {
			s=string.Format("{0:D2}", i); 
			sl1.Add("kala "+s,100+i*i);
		}
		for (int i = 0; i < 50; i++) {
			s=string.Format("{0:D2}", i+50); 
			sl1.Add("kala "+s,100+i*i);
		}
		AssertEquals("sl.IndexOfValue: does not return -1 for non-existing value(1)", -1, sl1.IndexOfValue(102));
		AssertEquals("sl.IndexOfValue: does not return -1 for non-existing value(2)", -1, sl1.IndexOfValue(null));
		for (int i=0; i<50; i++) {
			AssertEquals("sl.IndexOfValue: incorrect index key", i, sl1.IndexOfValue(100+i*i));
		}
	}

        [Test]
        public void TestIndexOfValue2 ()
        {
                SortedList list = new SortedList ();
                list.Add ("key0", "la la");
                list.Add ("key1", "value");
                list.Add ("key2", "value");

                int i = list.IndexOfValue ("value");

                AssertEquals (1, i);
        }

        [Test]
        public void TestIndexOfValue3 ()
        {
                SortedList list = new SortedList ();
                int i = list.IndexOfValue ((string) null);

                AssertEquals (1, -i);
        }

        [Test]
	public void TestRemove() {
		SortedList sl1 = new SortedList(24);
		string s=null;
		int k;
		for (int i = 0; i < 50; i++) sl1.Add("kala "+i,i);
		
		try {
			sl1.Remove(s);
			Fail("sl.Remove: ArgumentNullException not caught, when key is null");
		} catch (ArgumentNullException) {}
		k=sl1.Count;
		sl1.Remove("kala ");
		AssertEquals("sl.Remove: removes an item, when non-existing key given",sl1.Count,k);
		try {
			sl1.Remove(15);
			Fail("sl.Remove: IComparer exception is not thrown");
		} catch (Exception) {}

		for (int i=15; i<20; i++) sl1.Remove("kala "+i);
		for (int i=45; i<55; i++) sl1.Remove("kala "+i);
		
		AssertEquals("sl.Remove: removing failed",sl1.Count,40);
		for (int i=45; i<55; i++)
			AssertEquals("sl.Remove: removing failed(2)",sl1["kala "+i],null);
	}

        [Test]
	public void TestRemoveAt() {
		SortedList sl1 = new SortedList(24);
		int k;
		string s=null;
		for (int i = 0; i < 50; i++) {
			s=string.Format("{0:D2}", i); 
			sl1.Add("kala "+s,i);
		}
		
		try {
			sl1.RemoveAt(-1);
			Fail("sl.RemoveAt: ArgumentOutOfRangeException not caught, when key is out of range");
		} catch (ArgumentOutOfRangeException) {}
		try {
			sl1.RemoveAt(100);
			Fail("sl.RemoveAt: ArgumentOutOfRangeException not caught, when key is out of range");
		} catch (ArgumentOutOfRangeException) {}
		k=sl1.Count;

		for (int i=0; i<20; i++) sl1.RemoveAt(9);
		
		AssertEquals("sl.RemoveAt: removing failed",sl1.Count,30);
		for (int i=0; i<9; i++)
			AssertEquals("sl.RemoveAt: removing failed(2)",sl1["kala "+string.Format("{0:D2}", i)],i);
		for (int i=9; i<29; i++)
			AssertEquals("sl.RemoveAt: removing failed(3)",sl1["kala "+string.Format("{0:D2}", i)],null);
		for (int i=29; i<50; i++)
			AssertEquals("sl.RemoveAt: removing failed(4)",sl1["kala "+string.Format("{0:D2}", i)],i);
	}

        [Test]
	public void TestSetByIndex() {
		SortedList sl1 = new SortedList(24);
		for (int i = 49; i>=0; i--) sl1.Add(100+i,i);
		
		try {
			sl1.SetByIndex(-1,77);
			Fail("sl.SetByIndex: ArgumentOutOfRangeException not caught, when key is out of range");
		} catch (ArgumentOutOfRangeException) {}
		try {
			sl1.SetByIndex(100,88);
			Fail("sl.SetByIndex: ArgumentOutOfRangeException not caught, when key is out of range");
		} catch (ArgumentOutOfRangeException) {}

		for(int i=5; i<25; i++) sl1.SetByIndex(i,-1);
		for(int i=0; i<5; i++)
			AssertEquals("sl.SetByIndex: set failed(1)",sl1[100+i],i);
		for(int i=5; i<25; i++)
			AssertEquals("sl.SetByIndex: set failed(2)",sl1[100+i],-1);
		for(int i=25; i<50; i++)
			AssertEquals("sl.SetByIndex: set failed(3)",sl1[100+i],i);

	}

        [Test]
	public void TestTrimToSize() {
		SortedList sl1 = new SortedList(24);
		
		sl1.TrimToSize();
		AssertEquals("sl.TrimToSize: incorrect capacity after trimming empty list",icap,sl1.Capacity);
		
		for (int i = 72; i>=0; i--) sl1.Add(100+i,i);
		sl1.TrimToSize();
		AssertEquals("sl.TrimToSize: incorrect capacity after trimming a list",73,sl1.Capacity);
	}
}

}
