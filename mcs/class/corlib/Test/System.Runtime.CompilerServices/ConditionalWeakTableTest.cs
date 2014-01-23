//
// ConditionalWeakTableTest.cs
//
// Author:
//	Rodrigo Kumpera   <rkumpera@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

#if NET_4_0

using NUnit.Framework;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Collections.Generic;
using System.Threading;


namespace MonoTests.System.Runtime.CompilerServices {

	[TestFixture]
	public class ConditionalWeakTableTest {

	public class Link {
		public object obj;
	
		public Link(object obj) { 
			this.obj = obj;
		}
	}

	class Key {
		public int Foo;
		public override string ToString () {
				return "key-" + Foo;
			}
	}

	class Val {
		public int Foo;
		public override string ToString () {
			return "value-" + Foo;
		}
	}
	

	[Test]
	public void GetValue () {
		var cwt = new ConditionalWeakTable <object,object> ();

		try {
			cwt.GetValue (null, k => null);
			Assert.Fail ("#0");
		} catch (ArgumentNullException) {}

		try {
			cwt.GetValue (20, null);
			Assert.Fail ("#1");
		} catch (ArgumentNullException) {}


		object key = "foo";
		object val = cwt.GetValue (key, k => new Link (k));
		Assert.IsTrue (val != null, "#2");
		Assert.AreEqual (typeof (Link), val.GetType () , "#3");

		Assert.AreEqual (val, cwt.GetValue (key, k => new object ()), "#4");
	}

	[Test]
	public void GetOrCreateValue () {
		var cwt = new ConditionalWeakTable <object,object> ();

		try {
			cwt.GetOrCreateValue (null);
			Assert.Fail ("#0");
		} catch (ArgumentNullException) {}


		object key = "foo";
		object val = cwt.GetOrCreateValue (key);
		Assert.IsTrue (val != null, "#2");
		Assert.AreEqual (typeof (object), val.GetType () , "#3");

		Assert.AreEqual (val, cwt.GetOrCreateValue (key), "#4");

		var cwt2 = new ConditionalWeakTable <object, string> ();
		try {
			cwt2.GetOrCreateValue (key);
			Assert.Fail ("#5");
		} catch (MissingMethodException) {}
	}

	[Test]
	public void Remove () {
		var cwt = new ConditionalWeakTable <object,object> ();
		object c = new Key ();

		cwt.Add (c, "x");

		try {
			cwt.Remove (null);
			Assert.Fail ("#0");
		} catch (ArgumentNullException) {}


		Assert.IsFalse (cwt.Remove ("x"), "#1");
		Assert.IsTrue (cwt.Remove (c), "#2");
		Assert.IsFalse (cwt.Remove (c), "#3");
	}

	[Test]
	public void Add () {
		var cwt = new ConditionalWeakTable <object,object> ();
		object c = new Key ();

		cwt.Add (c, new Link (c));

		try {
			cwt.Add (c, new Link (c));
			Assert.Fail ("#0");
		} catch (ArgumentException) {}

		cwt.Add ("zzz", null);//ok

		try {
			cwt.Add (null, new Link (c));
			Assert.Fail ("#1");
		} catch (ArgumentNullException) {}
	}

	[Test]
	public void TryGetValue () {
		var cwt = new ConditionalWeakTable <object,object> ();
		object res;
		object c = new Key ();

		cwt.Add (c, "foo");

		try {
			cwt.TryGetValue (null, out res);
			Assert.Fail ("#0");
		} catch (ArgumentNullException) {}

		Assert.IsFalse (cwt.TryGetValue ("foo", out res), "#1");
		Assert.IsTrue (cwt.TryGetValue (c, out res), "#2");
		Assert.AreEqual ("foo", res, "#3");
	}


	static void FillStuff (ConditionalWeakTable <object,object> cwt, 
			out List<object> keepAlive, out List<WeakReference> keys) {

		keepAlive = new List<object> ();
		keys = new List<WeakReference> ();

		object a = new Key ();
		object b = new Key ();
		object c = new Key ();
		
		cwt.Add (a, new string ("str0".ToCharArray ()));
		cwt.Add (b, "str1");
		cwt.Add (c, new Link (a));

		keepAlive.Add (c);
		keys.Add (new WeakReference(a));
		keys.Add (new WeakReference(b));
		keys.Add (new WeakReference(c));
	}

	[Test]
	public void Reachability () {
		if (GC.MaxGeneration == 0) /*Boehm doesn't handle ephemerons */
			return;
		var cwt = new ConditionalWeakTable <object,object> ();
		List<object> keepAlive;
		List<WeakReference> keys;
		FillStuff (cwt, out keepAlive, out keys);

		GC.Collect ();

		Assert.IsTrue (keys [0].IsAlive, "r0");
		Assert.IsFalse (keys [1].IsAlive, "r1");
		Assert.IsTrue (keys [2].IsAlive, "r2");

		object res;
		Assert.IsTrue (cwt.TryGetValue (keepAlive [0], out res), "ka0");
		Assert.IsTrue (res is Link, "ka1");

		Link link = res as Link;
		Assert.IsTrue (cwt.TryGetValue (link.obj, out res), "ka2");
		Assert.AreEqual ("str0", res, "ka3");
	}


	static List<WeakReference> FillWithNetwork (ConditionalWeakTable <object,object> cwt)
	{
		const int K = 500;
		object[] keys = new object [K];
		for (int i = 0; i < K; ++i)
			keys[i] = new object ();

		Random rand = new Random ();

		/*produce a complex enough network of links*/
		for (int i = 0; i < K; ++i)
			cwt.Add (keys [i], new Link (keys [rand.Next (K)]));

		var res = new List<WeakReference> ();

		for (int i = 0; i < 10; ++i)
			res.Add (new WeakReference (keys [rand.Next (K)]));
		Array.Clear (keys, 0, keys.Length);

		return res;
	}

	[Test]
	public void InsertStress () {
		if (GC.MaxGeneration == 0) /*Boehm doesn't handle ephemerons */
			return;
		var cwt = new ConditionalWeakTable <object,object> ();

		var a = new object ();
		var b = new object ();

		cwt.Add (a, new object ());
		cwt.Add (b, new object ());

		List<WeakReference> res = null;
		ThreadStart dele = () => { res = FillWithNetwork (cwt); };
		var th = new Thread(dele);
		th.Start ();
		th.Join ();

		GC.Collect ();
		GC.Collect ();

		for (int i = 0; i < res.Count; ++i)
			Assert.IsFalse (res [i].IsAlive, "#r" + i);
	}

	static List<WeakReference> FillWithNetwork2 (ConditionalWeakTable <object,object>[] cwt) {
		if (cwt [0] == null)
			cwt[0] = new ConditionalWeakTable <object,object> ();
		var res = FillWithNetwork (cwt[0]);

		return res;
	}

	static void ForcePromotion () {
		var o = new object[64000];

		for (int i = 0; i < 64000; ++i)
			o[i] = new int[10];
	}

	static List<object> FillReachable (ConditionalWeakTable <object,object>[] cwt)
	{
		var res = new List<object> ();
		for (int i = 0; i < 10; ++i) {
			res.Add (new object ());
			cwt[0].Add (res [i], i);
		}

		return res;
	}

	[Test]
	public void OldGenStress () {
		if (GC.MaxGeneration == 0) /*Boehm doesn't handle ephemerons */
			return;
		var cwt = new ConditionalWeakTable <object,object>[1];
		List<object> k = null;
		List<WeakReference> res, res2;
		res = res2 = null;

		ThreadStart dele = () => {
			res = FillWithNetwork2 (cwt);
			ForcePromotion ();
			k = FillReachable (cwt);
			res2 = FillWithNetwork2 (cwt);
		};

		var th = new Thread(dele);
		th.Start ();
		th.Join ();

		GC.Collect ();

		for (int i = 0; i < res.Count; ++i)
			Assert.IsFalse (res [i].IsAlive, "#r0-" + i);
		for (int i = 0; i < res2.Count; ++i)
			Assert.IsFalse (res2 [i].IsAlive, "#r1-" + i);

		for (int i = 0; i < k.Count; ++i) {
			object val;
			Assert.IsTrue (cwt[0].TryGetValue (k [i], out val), "k0-" + i);
			Assert.AreEqual (i, val, "k1-" + i);
		}
	}


	static List<GCHandle> FillTable3 (ConditionalWeakTable <object,object> cwt) {
		var handles = new List<GCHandle> ();

		var a = (object)10;
		var b = (object)20;
		var k1 = (object)30;
		var k2 = (object)40;

		handles.Add (GCHandle.Alloc (a, GCHandleType.Pinned));
		handles.Add (GCHandle.Alloc (b, GCHandleType.Pinned));
		handles.Add (GCHandle.Alloc (k1, GCHandleType.Pinned));
		handles.Add (GCHandle.Alloc (k2, GCHandleType.Pinned));
		
		cwt.Add (a, k1);
		cwt.Add (b, k2);

	
		return handles;
	}

	static void MakeObjMovable (List<GCHandle> handles)
	{
		for (int i = 0; i < handles.Count; ++i) {
			object o = handles[i].Target;
			handles[i].Free ();
			handles[i] = GCHandle.Alloc (o, GCHandleType.Normal);
		}
	}

	static void ForceMinor () {
		for (int i = 0; i < 64000; ++i)
			new object ();
	}

	public void PromotedCwtPointingToYoungStuff () {
		var cwt = new ConditionalWeakTable <object,object> ();

		var handles = FillTable3 (cwt);

		GC.Collect (0);

		/*Be 100% sure it will be on the young gen*/

		/*cwt array now will be on old gen*/
		ForceMinor ();
		ForceMinor ();
		ForceMinor ();

		//Make them non pinned
		MakeObjMovable (handles);

		GC.Collect (0);
		
		//Force a minor GC - this should cause
		ForceMinor ();
		ForceMinor ();
		ForceMinor ();
		ForceMinor ();

		GC.Collect (0);

		object r1, r2;
		Assert.IsTrue (cwt.TryGetValue (handles[0].Target, out r1), "#1");
		Assert.IsTrue (cwt.TryGetValue (handles[1].Target, out r2), "#2");

		GC.Collect ();
		cwt.GetHashCode ();
	}

	static object _lock1 = new object ();
	static object _lock2 = new object ();
	static int reachable = 0;
 
	public class FinalizableLink {
		object obj;
		ConditionalWeakTable <object,object> cwt;
		int id;

		public FinalizableLink (int id, object obj, ConditionalWeakTable <object,object> cwt) {
			this.id = id;
			this.obj = obj;
			this.cwt = cwt;
		}

		~FinalizableLink () {
			lock (_lock1) { ; }
			object obj;
			bool res = cwt.TryGetValue (this, out obj);
			if (res)
				++reachable;
			if (reachable == 20)
				lock (_lock2) { Monitor.Pulse (_lock2); }
		}
	}

	static void FillWithFinalizable (ConditionalWeakTable <object,object> cwt)
	{
		object a = new object ();
		object b = new FinalizableLink (0, a, cwt);
		cwt.Add (a, "foo");
		cwt.Add (b, "bar");

		for (int i = 1; i < 20; ++i) {
			b = new FinalizableLink (i, b, cwt);
			cwt.Add (b, i);
		}
	}

	[Test]
	public void FinalizableObjectsThatRetainDeadKeys ()
	{
		if (GC.MaxGeneration == 0) /*Boehm doesn't handle ephemerons */
			return;
		lock (_lock1) { 
			var cwt = new ConditionalWeakTable <object,object> ();
			ThreadStart dele = () => { FillWithFinalizable (cwt); };
			var th = new Thread(dele);
			th.Start ();
			th.Join ();
			GC.Collect ();
			GC.Collect ();

			Assert.AreEqual (0, reachable, "#1");
		}

		GC.Collect ();
		GC.Collect ();
		lock (_lock2) { Monitor.Wait (_lock2, 1000); }

		Assert.AreEqual (20, reachable, "#1");
	}

	[Test]
	public void OldGenKeysMakeNewGenObjectsReachable ()
	{
		if (GC.MaxGeneration == 0) /*Boehm doesn't handle ephemerons */
			return;
		ConditionalWeakTable<object, Val> table = new ConditionalWeakTable<object, Val>();
		List<Key> keys = new List<Key>();

		//
		// This list references all keys for the duration of the program, so none 
		// should be collected ever.
		//
		for (int x = 0; x < 1000; x++) 
			keys.Add (new Key () { Foo = x });

		for (int i = 0; i < 1000; ++i) {
			// Insert all keys into the ConditionalWeakTable
			foreach (var key in keys)
				table.Add (key, new Val () { Foo = key.Foo });

			// Look up all keys to verify that they are still there
			Val val;
			foreach (var key in keys)
				Assert.IsTrue (table.TryGetValue (key, out val), "#1-" + i + "-k-" + key);

			// Remove all keys from the ConditionalWeakTable
			foreach (var key in keys)
				Assert.IsTrue (table.Remove (key), "#2-" + i + "-k-" + key);
		}
	}
	}
}

#endif
