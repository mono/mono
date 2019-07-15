// HashtableTest.cs - NUnit Test Cases for the System.Collections.Hashtable class
//
//
// (C) Ximian, Inc.  http://www.ximian.com
// 


using System;
using System.Collections;
using System.Reflection;

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;

using NUnit.Framework;



namespace MonoTests.System.Collections {


/// <summary>Hashtable test.</summary>
[TestFixture]
public class HashtableTest {

        [Test]
	public void TestCtor1() {
		Hashtable h = new Hashtable();
		Assert.IsNotNull (h, "No hash table");
	}

        [Test]
	public void TestCtor2() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable((IDictionary) null);
			} catch (ArgumentNullException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "null hashtable error not thrown");
		}
		{
			string[] keys = {"this", "is", "a", "test"};
			char[] values = {'a', 'b', 'c', 'd'};
			Hashtable h1 = new Hashtable();
			for (int i = 0; i < keys.Length; i++) {
				h1[keys[i]] = values[i];
			}
			Hashtable h2 = new Hashtable(h1);
			for (int i = 0; i < keys.Length; i++) {
				Assert.AreEqual (values[i], h2[keys[i]], "No match for key " + keys[i]);
			}
		}
	}

        [Test]
        [ExpectedException (typeof (ArgumentOutOfRangeException))]
        public void TestCtor3 ()
        {
                Hashtable h = new Hashtable ();
                Hashtable hh = new Hashtable (h, Single.NaN);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void TestCtor4 ()
        {
                Hashtable ht = new Hashtable (Int32.MaxValue, 0.1f, null, null);
        }

	[Test]
	public void TestCtor5 ()
	{
		// tests if negative capacity throws exception
		try {
			Hashtable ht = new Hashtable (-10, 0.1f, null, null);
			Assert.Fail ("must throw ArgumentOutOfRange exception, param: capacity");
		} catch (ArgumentOutOfRangeException e) {
			Assert.IsTrue (e.ParamName == "capacity", "ParamName is not capacity");
		}

		// tests if loadFactor out of range throws exception (low)
		try {
			Hashtable ht = new Hashtable (100, 0.01f, null, null);
			Assert.Fail ("must throw ArgumentOutOfRange exception, param: loadFactor, too low value");
		} 	catch (ArgumentOutOfRangeException e) 
		{
			Assert.IsTrue (e.ParamName == "loadFactor", "ParamName is not loadFactor");
		}

		// tests if loadFactor out of range throws exception (high)
		try 
		{
			Hashtable ht = new Hashtable (100, 2f, null, null);
			Assert.Fail ("must throw ArgumentOutOfRange exception, param: loadFactor, too high value");
		} 	
		catch (ArgumentOutOfRangeException e) 
		{
			Assert.IsTrue (e.ParamName == "loadFactor", "ParamName is not loadFactor");
		}

	}

	// TODO - Ctors for capacity and load (how to test? any access?)
        // TODO - Ctors with IComparer, IHashCodeProvider, Serialization

        [Test]	
	public void TestCount() {
		Hashtable h = new Hashtable();
		Assert.AreEqual (0, h.Count, "new table - count zero");
		int max = 100;
		for (int i = 1; i <= max; i++) {
			h[i] = i;
			Assert.AreEqual (i, h.Count, "Count wrong for " + i);
		}
		for (int i = 1; i <= max; i++) {
			h[i] = i * 2;
			Assert.AreEqual (max, h.Count, "Count shouldn't change at " + i);
		}
	}

        [Test]        
	public void TestIsFixedSize() {
		Hashtable h = new Hashtable();
		Assert.AreEqual (false, h.IsFixedSize, "hashtable not fixed by default");
		// TODO - any way to get a fixed-size hashtable?
	}

	public void TestIsReadOnly() {
		Hashtable h = new Hashtable();
		Assert.AreEqual (false, h.IsReadOnly, "hashtable not read-only by default");
		// TODO - any way to get a read-only hashtable?
	}

        [Test]        
	public void TestIsSynchronized ()
	{
		Hashtable h = new Hashtable ();
		Assert.IsTrue (!h.IsSynchronized, "hashtable not synched by default");

		Hashtable h2 = Hashtable.Synchronized (h);
		Assert.IsTrue (h2.IsSynchronized, "hashtable should by synched");

		Hashtable h3 = (Hashtable) h2.Clone ();
		Assert.IsTrue (h3.IsSynchronized, "Cloned Hashtable should by synched");
	}

        [Test]
	public void TestItem() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				Object o = h[null];
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("key", e.ParamName, "ParamName is not \"key\"");
			}
			Assert.IsTrue (errorThrown, "null hashtable error not thrown");
		}
		// TODO - if read-only and/or fixed-size is possible,
		//        test 'NotSupportedException' here

		{
			Hashtable h = new Hashtable();
			int max = 100;
			for (int i = 1; i <= max; i++) {
				h[i] = i;
				Assert.AreEqual (i, h[i], "value wrong for " + i);
			}
		}
	}

        [Test]
	public void TestKeys() {
		string[] keys = {"this", "is", "a", "test"};
		string[] keys2 = {"new", "keys"};
		char[] values1 = {'a', 'b', 'c', 'd'};
		char[] values2 = {'e', 'f', 'g', 'h'};
		ICollection keysReference, keysReference2;
		Hashtable h1 = new Hashtable();
		for (int i = 0; i < keys.Length; i++) {
			h1[keys[i]] = values1[i];
		}
		Assert.AreEqual (keys.Length, h1.Keys.Count, "keys wrong size");
		for (int i = 0; i < keys.Length; i++) {
			h1[keys[i]] = values2[i];
		}
		Assert.AreEqual (keys.Length, h1.Keys.Count, "keys wrong size 2");

		// MS .NET Always returns the same reference when calling Keys property
		keysReference = h1.Keys;
	    keysReference2 = h1.Keys;
		Assert.AreEqual (keysReference, keysReference2, "keys references differ");

		for (int i = 0; i < keys2.Length; i++) 
		{
			h1[keys2[i]] = values2[i];
		}
		Assert.AreEqual (keys.Length+keys2.Length, h1.Keys.Count, "keys wrong size 3");
		Assert.AreEqual (keys.Length+keys2.Length, keysReference.Count, "keys wrong size 4");
	}

	// TODO - SyncRoot
        [Test]        
	public void TestValues() {
		string[] keys = {"this", "is", "a", "test"};
		char[] values1 = {'a', 'b', 'c', 'd'};
		char[] values2 = {'e', 'f', 'g', 'h'};
		Hashtable h1 = new Hashtable();
		for (int i = 0; i < keys.Length; i++) {
			h1[keys[i]] = values1[i];
		}
		Assert.AreEqual (keys.Length, h1.Values.Count, "values wrong size");
		for (int i = 0; i < keys.Length; i++) {
			h1[keys[i]] = values2[i];
		}
		Assert.AreEqual (keys.Length, h1.Values.Count, "values wrong size 2");

		// MS .NET Always returns the same reference when calling Values property
		ICollection valuesReference1 = h1.Values;
		ICollection valuesReference2 = h1.Values;
		Assert.AreEqual (valuesReference1, valuesReference2, "values references differ");
	}

	[Test]
	public void TestAdd() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h.Add(null, "huh?");
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("key", e.ParamName, "ParamName is not 'key'");
			}
			Assert.IsTrue (errorThrown, "null add error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h.Add('a', 1);
				h.Add('a', 2);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "re-add error not thrown");
		}
		// TODO - hit NotSupportedException
		{
			Hashtable h = new Hashtable();
			int max = 100;
			for (int i = 1; i <= max; i++) {
				h.Add(i, i);
				Assert.AreEqual (i, h[i], "value wrong for " + i);
			}
		}
	}

        [Test]
	public void TestClear() {
		// TODO - hit NotSupportedException
		Hashtable h = new Hashtable();
		Assert.AreEqual (0, h.Count, "new table - count zero");
		int max = 100;
		for (int i = 1; i <= max; i++) {
			h[i] = i;
		}
		Assert.IsTrue (h.Count > 0, "table don't gots stuff");
		h.Clear();
		Assert.AreEqual (0, h.Count, "Table should be cleared");
	}

	public class MyEqualityComparer : IEqualityComparer {
		bool IEqualityComparer.Equals (object x, object y) { return x == y; }
		public int GetHashCode (object obj) { return 1; }
	}

	static IEqualityComparer GetEqualityComparer (Hashtable h)
	{
		return (IEqualityComparer) typeof (Hashtable).GetField ("_keycomparer",
			BindingFlags.NonPublic | BindingFlags.Instance).GetValue (h);
	}
	
        [Test]
	public void TestClone() {
		{
			char[] c1 = {'a', 'b', 'c'};
			char[] c2 = {'d', 'e', 'f'};
			Hashtable h1 = new Hashtable();
			for (int i = 0; i < c1.Length; i++) {
				h1[c1[i]] = c2[i];
			}
			Hashtable h2 = (Hashtable)h1.Clone();
			Assert.IsNotNull (h2, "got no clone!");
			Assert.IsNotNull (h2[c1[0]], "clone's got nothing!");
			for (int i = 0; i < c1.Length; i++) {
				Assert.AreEqual (h1[c1[i]], h2[c1[i]], "Hashtable match");
			}
		}
		{
			char[] c1 = {'a', 'b', 'c'};
			char[] c20 = {'1', '2'};
			char[] c21 = {'3', '4'};
			char[] c22 = {'5', '6'};
			char[][] c2 = {c20, c21, c22};
			Hashtable h1 = new Hashtable();
			for (int i = 0; i < c1.Length; i++) {
				h1[c1[i]] = c2[i];
			}
			Hashtable h2 = (Hashtable)h1.Clone();
			Assert.IsNotNull (h2, "got no clone!");
			Assert.IsNotNull (h2[c1[0]], "clone's got nothing!");
			for (int i = 0; i < c1.Length; i++) {
				Assert.AreEqual (h1[c1[i]], h2[c1[i]], "Hashtable match");
			}

			((char[])h1[c1[0]])[0] = 'z';
			Assert.AreEqual (h1[c1[0]], h2[c1[0]], "shallow copy");

			// NET 2.0 stuff
			MyEqualityComparer a = new MyEqualityComparer ();
			Hashtable mh1 = new Hashtable (a);
			Hashtable mh1clone = (Hashtable) mh1.Clone ();
			
			// warning, depends on the field name.
			Assert.AreEqual (GetEqualityComparer (mh1), GetEqualityComparer (mh1clone), "EqualityComparer");
		}
	}

        [Test]
	public void TestContains() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				bool result = h.Contains(null);
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("key", e.ParamName, "ParamName is not 'key'");
			}
			Assert.IsTrue (errorThrown, "null add error not thrown");
		}
		{
			Hashtable h = new Hashtable();
			for (int i = 0; i < 10000; i += 2) 
			{
				h[i] = i;
			}
			for (int i = 0; i < 10000; i += 2) 
			{
				Assert.IsTrue (h.Contains(i), "hashtable must contain"+i.ToString());
				Assert.IsTrue (!h.Contains(i+1), "hashtable does not contain "+((int)(i+1)).ToString());
			}
		}
	}

        [Test]
	public void TestContainsKey() {
	{
			bool errorThrown = false;
			try 
			{
				Hashtable h = new Hashtable();
				bool result = h.Contains(null);
			} 
			catch (ArgumentNullException e) 
			{
				errorThrown = true;
				Assert.AreEqual ("key", e.ParamName, "ParamName is not 'key'");
			}
			Assert.IsTrue (errorThrown, "null add error not thrown");
		}
		{
			Hashtable h = new Hashtable();
			for (int i = 0; i < 1000; i += 2) 
			{
				h[i] = i;
			}
			for (int i = 0; i < 1000; i += 2) 
			{
				Assert.IsTrue (h.Contains(i), "hashtable must contain"+i.ToString());
				Assert.IsTrue (!h.Contains(i+1), "hashtable does not contain "+((int)(i+1)).ToString());
			}
		}

	}

        [Test]        
	public void TestContainsValue() {
		{
			Hashtable h = new Hashtable();
			h['a'] = "blue";
			Assert.IsTrue (h.ContainsValue("blue"), "blue? it's in there!");
			Assert.IsTrue (!h.ContainsValue("green"), "green? no way!");
			Assert.IsTrue (!h.ContainsValue(null), "null? no way!");
			h['b'] = null;
			Assert.IsTrue (h.ContainsValue(null), "null? it's in there!");

		}
	}

        [Test]
	public void TestCopyTo() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h.CopyTo(null, 0);
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("array", e.ParamName, "ParamName is not \"array\""); 
			}
			Assert.IsTrue (errorThrown, "null hashtable error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				Object[] o = new Object[1];
				h.CopyTo(o, -1);
			} catch (ArgumentOutOfRangeException e) {
				errorThrown = true;
				Assert.AreEqual ("arrayIndex", e.ParamName, "ParamName is not \"arrayIndex\"");
			}
			Assert.IsTrue (errorThrown, "out of range error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				Object[,] o = new Object[1,1];
				h.CopyTo(o, 1);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "multi-dim array error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h['a'] = 1; // no error if table is empty
				Object[] o = new Object[5];
				h.CopyTo(o, 5);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "no room in array error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h['a'] = 1;
				h['b'] = 2;
				h['c'] = 2;
				Object[] o = new Object[2];
				h.CopyTo(o, 0);
			} catch (ArgumentException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "table too big error not thrown");
		}
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h["blue"] = 1;
				h["green"] = 2;
				h["red"] = 3;
				Char[] o = new Char[3];
				h.CopyTo(o, 0);
			} catch (InvalidCastException) {
				errorThrown = true;
			}
			Assert.IsTrue (errorThrown, "invalid cast error not thrown");
		}

		{
			Hashtable h = new Hashtable();
			h['a'] = 1;
			h['b'] = 2;
			DictionaryEntry[] o = new DictionaryEntry[2];
			h.CopyTo(o,0);
			Assert.AreEqual ('a', o[0].Key, "first copy fine.");
			Assert.AreEqual (1, o[0].Value, "first copy fine.");
			Assert.AreEqual ('b', o[1].Key, "second copy fine.");
			Assert.AreEqual (2, o[1].Value, "second copy fine.");
		}
	}

        [Test]        
	public void TestGetEnumerator() {
		String[] s1 = {"this", "is", "a", "test"};
		Char[] c1 = {'a', 'b', 'c', 'd'};
		Hashtable h1 = new Hashtable();
		for (int i = 0; i < s1.Length; i++) {
			h1[s1[i]] = c1[i];
		}
		IDictionaryEnumerator en = h1.GetEnumerator();
		Assert.IsNotNull (en, "No enumerator");
		
		for (int i = 0; i < s1.Length; i++) {
			en.MoveNext();
			Assert.IsTrue (Array.IndexOf(s1, en.Key) >= 0, "Not enumerating for " + en.Key);
			Assert.IsTrue (Array.IndexOf(c1, en.Value) >= 0, "Not enumerating for " + en.Value);
		}
	}

	[Test]
	public void TestSerialization () {
		Hashtable table1 = new Hashtable();
		Hashtable table2;
		Stream str = new MemoryStream ();
		BinaryFormatter formatter = new BinaryFormatter();

		for (int i = 0; i < 100; i++)
			table1[i] = "TestString Key: " + i.ToString();
		
		formatter.Serialize (str, table1);
		str.Position = 0;
		table2 = (Hashtable) formatter.Deserialize (str);
		
		bool result;
		foreach (DictionaryEntry de in table1)
			Assert.AreEqual (table2 [de.Key], de.Value);
	}
	
	[Test]
	[Category ("Remoting")]
	public void TestSerialization2 () {
		// Test from bug #70570
		MemoryStream stream = new MemoryStream();
		BinaryFormatter formatter = new BinaryFormatter();
	
		Hashtable table = new Hashtable();
		table.Add (new Bug(), "Hello");

		formatter.Serialize(stream, table);
		stream.Position = 0;
		table = (Hashtable) formatter.Deserialize(stream);
		Assert.AreEqual (1, table.Count, "#1");
	}

        [Test]        
	public void TestRemove() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = new Hashtable();
				h.Remove(null);
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("key", e.ParamName, "ParamName is not \"key\"");
			}
			Assert.IsTrue (errorThrown, "null hashtable error not thrown");
		}
		{
			string[] keys = {"this", "is", "a", "test"};
			char[] values = {'a', 'b', 'c', 'd'};
			Hashtable h = new Hashtable();
			for (int i = 0; i < keys.Length; i++) {
				h[keys[i]] = values[i];
			}
			Assert.AreEqual (4, h.Count, "not enough in table");
			h.Remove("huh?");
			Assert.AreEqual (4, h.Count, "not enough in table");
			h.Remove("this");
			Assert.AreEqual (3, h.Count, "Wrong count in table");
			h.Remove("this");
			Assert.AreEqual (3, h.Count, "Wrong count in table");
		}
	}

        [Test]        
	public void TestSynchronized() {
		{
			bool errorThrown = false;
			try {
				Hashtable h = Hashtable.Synchronized(null);
			} catch (ArgumentNullException e) {
				errorThrown = true;
				Assert.AreEqual ("table", e.ParamName, "ParamName is not \"table\"");
			}
			Assert.IsTrue (errorThrown, "null hashtable error not thrown");
		}
		{
			Hashtable h = new Hashtable();
			Assert.IsTrue (!h.IsSynchronized, "hashtable not synced by default");
			Hashtable h2 = Hashtable.Synchronized(h);
			Assert.IsTrue (h2.IsSynchronized, "hashtable should by synced");
		}
	}
	
	
	protected Hashtable ht;
	private static Random rnd;
	
	[SetUp]
	public void SetUp() {
		ht=new Hashtable();
		rnd=new Random();
	}
	
	private void SetDefaultData() {
		ht.Clear();
		ht.Add("k1","another");
		ht.Add("k2","yet");
		ht.Add("k3","hashtable");
	}
	
        [Test]
	public void TestAddRemoveClear() {
		ht.Clear();
		Assert.IsTrue (ht.Count==0);
		
		SetDefaultData();
		Assert.IsTrue (ht.Count==3);
		
		bool thrown=false;
		try {
			ht.Add("k2","cool");
		} catch (ArgumentException) {thrown=true;}
		Assert.IsTrue (thrown, "Must throw ArgumentException!");
		
		ht["k2"]="cool";
		Assert.IsTrue (ht.Count==3);
		Assert.IsTrue (ht["k2"].Equals("cool"));
		
	}

        [Test]	
	public void TestCopyTo2() {
		SetDefaultData();
		Object[] entries=new Object[ht.Count];
		ht.CopyTo(entries,0);
		Assert.IsTrue (entries[0] is DictionaryEntry, "Not an entry.");
	}

	[Test]
	public void CopyTo_Empty ()
	{
		Hashtable ht = new Hashtable ();
		Assert.AreEqual (0, ht.Count, "Count");
		object[] array = new object [ht.Count];
		ht.CopyTo (array, 0);
	}
	
        [Test]	
	public void TestUnderHeavyLoad() {
		ht.Clear();
		int max=100000;
		String[] cache=new String[max*2];
		int n=0;
		
		for (int i=0;i<max;i++) {
			int id=rnd.Next()&0xFFFF;
			String key=""+id+"-key-"+id;
			String val="value-"+id;
			if (ht[key]==null) {
				ht[key]=val;
				cache[n]=key;
				cache[n+max]=val;
				n++;
			}
		}
		
		Assert.IsTrue (ht.Count==n);
		
		for (int i=0;i<n;i++) {
			String key=cache[i];
			String val=ht[key] as String;
			String err="ht[\""+key+"\"]=\""+val+
				"\", expected \""+cache[i+max]+"\"";
			Assert.IsTrue (val!=null && val.Equals(cache[i+max]), err);
		}
		
		int r1=(n/3);
		int r2=r1+(n/5);
		
		for (int i=r1;i<r2;i++) {
			ht.Remove(cache[i]);
		}
		
		
		for (int i=0;i<n;i++) {
			if (i>=r1 && i<r2) {
				Assert.IsTrue (ht[cache[i]]==null);
			} else {
				String key=cache[i];
				String val=ht[key] as String;
				String err="ht[\""+key+"\"]=\""+val+
					"\", expected \""+cache[i+max]+"\"";
				Assert.IsTrue (val!=null && val.Equals(cache[i+max]), err);
			}
		}
		
		ICollection keys=ht.Keys;
		int nKeys=0;
		foreach (Object key in keys) {
			Assert.IsTrue ((key as String) != null);
			nKeys++;
		}
		Assert.IsTrue (nKeys==ht.Count);

		
		ICollection vals=ht.Values;
		int nVals=0;
		foreach (Object val in vals) {
			Assert.IsTrue ((val as String) != null);
			nVals++;
		}
		Assert.IsTrue (nVals==ht.Count);
		
	}

	
	/// <summary>
	///  Test hashtable with CaseInsensitiveHashCodeProvider
	///  and CaseInsensitive comparer.
	/// </summary>
	[Test]
	[Category ("ManagedCollator")]
	public void TestCaseInsensitive ()
	{
		// Not very meaningfull test, just to make
		// sure that hcp is set properly set.
		Hashtable ciHashtable = new Hashtable(11,1.0f,CaseInsensitiveHashCodeProvider.Default,CaseInsensitiveComparer.Default);
		ciHashtable ["key1"] = "value";
		ciHashtable ["key2"] = "VALUE";
		Assert.IsTrue (ciHashtable ["key1"].Equals ("value"));
		Assert.IsTrue (ciHashtable ["key2"].Equals ("VALUE"));

		ciHashtable ["KEY1"] = "new_value";
		Assert.IsTrue (ciHashtable ["key1"].Equals ("new_value"));

	}

        [Test]
	public void TestCopyConstructor ()
	{
		SetDefaultData ();

		Hashtable htCopy = new Hashtable (ht);

		Assert.IsTrue (ht.Count == htCopy.Count);
	}

        [Test]
	public void TestEnumerator ()
	{
		SetDefaultData ();

		IEnumerator e = ht.GetEnumerator ();

		while (e.MoveNext ()) {}

		Assert.IsTrue (!e.MoveNext ());

	}

	[Test]
	[ExpectedException (typeof (ArgumentNullException))]
	public void GetObjectData_NullSerializationInfo () 
	{
		SetDefaultData ();
		ht.GetObjectData (null, new StreamingContext ());
	}

	// bug #75790
	[Test]
	[Category ("NotDotNet")] // .NET raises InvalidOperationException.
	public void SyncHashtable_ICollectionsGetEnumerator ()
	{
		Hashtable hashtable = Hashtable.Synchronized (new Hashtable ());
		hashtable["a"] = 1;
		//IEnumerator e = (hashtable.Clone() as
		IEnumerator e = (hashtable as ICollection).GetEnumerator ();
		//e.Reset();
		e.MoveNext ();
		DictionaryEntry de = (DictionaryEntry) e.Current;
	}

	[Test]
	public void SerializableSubClasses ()
	{
		Hashtable ht = new Hashtable ();
		// see bug #76300
		Assert.IsTrue (ht.Keys.GetType ().IsSerializable, "Keys.IsSerializable");
		Assert.IsTrue (ht.Values.GetType ().IsSerializable, "Values.IsSerializable");
		Assert.IsTrue (ht.GetEnumerator ().GetType ().IsSerializable, "GetEnumerator.IsSerializable");
		Assert.IsTrue (Hashtable.Synchronized (ht).GetType ().IsSerializable, "Synchronized.IsSerializable");
	}

	[Test]
	public void TestHashtableWithCustomComparer ()
	{
		// see bug #324761
		IDHashtable dd = new IDHashtable ();
		Random r = new Random (1000);
		for (int n = 0; n < 10000; n++) {
			int v = r.Next (0, 1000);
			dd [v] = v;
			v = r.Next (0, 1000);
			dd.Remove (v);
		}
	}

	[Test]
	public void HashtableCopyWithCustomComparer ()
	{
		var ht = new Hashtable ();
		ht.Add ("a", "b");
		try {
			new Hashtable (ht, new IEqualityComparer_ApplicationException ());
			Assert.Fail ("custom comparer not used");
		} catch (ApplicationException) {

		}
	}
}

class IDHashtable : Hashtable {

	class IDComparer : IComparer {
		public int Compare (object x, object y)
		{
			if ((int) x == (int) y)
				return 0;
			else
				return 1;
		}
	}

	class IDHashCodeProvider : IHashCodeProvider {
		public int GetHashCode (object o)
		{
			return (int) o;
		}
	}

	public IDHashtable ()
		: base (new IDHashCodeProvider (),
				new IDComparer ())
	{
	}
}

[Serializable]
public class Bug :ISerializable {

	[Serializable]
	private sealed class InnerClassSerializationHelper : IObjectReference {
		public object GetRealObject( StreamingContext context )
		{
			return new Bug();
		}
	};

	void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context )
	{
		info.SetType( typeof(InnerClassSerializationHelper) );
	}
};

	class IEqualityComparer_ApplicationException : IEqualityComparer
	{
		public new bool Equals (object x, object y)
		{
			return false;
		}

		public int GetHashCode (object obj)
		{
			throw new ApplicationException ();
		}
	}

}
