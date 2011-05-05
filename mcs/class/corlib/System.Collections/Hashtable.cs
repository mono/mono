//
// System.Collections.Hashtable.cs
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//

//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Runtime.Serialization;

using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Collections {

	[Serializable]
#if INSIDE_CORLIB
	[ComVisible(true)]
	[DebuggerDisplay ("Count={Count}")]
	[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	public class Hashtable : IDictionary, ICollection, 
		IEnumerable, ICloneable, ISerializable, IDeserializationCallback {
#else
	internal class Hashtable : IDictionary, ICollection, IEnumerable {
#endif

		[Serializable]
		internal struct Slot {
			internal Object key;

			internal Object value;
		}

		[Serializable]
		internal class KeyMarker
#if !NET_2_1
			: IObjectReference
#endif
		{
			public readonly static KeyMarker Removed = new KeyMarker();
#if !NET_2_1
			public object GetRealObject (StreamingContext context)
			{
				return KeyMarker.Removed;
			}
#endif
		}

		//
		// Private data
		//

		const int CHAIN_MARKER  = ~Int32.MaxValue;


		private int inUse;
		private int modificationCount;
		private float loadFactor;
		private Slot [] table;
		// Hashcodes of the corresponding entries in the slot table. Kept separate to
		// help the GC
		private int [] hashes;

		private int threshold;
	
		private HashKeys hashKeys;
		private HashValues hashValues;

		private IHashCodeProvider hcpRef;
		private IComparer comparerRef;
		private SerializationInfo serializationInfo;

		private IEqualityComparer equalityComparer;

		private static readonly int [] primeTbl = {
			11,
			19,
			37,
			73,
			109,
			163,
			251,
			367,
			557,
			823,
			1237,
			1861,
			2777,
			4177,
			6247,
			9371,
			14057,
			21089,
			31627,
			47431,
			71143,
			106721,
			160073,
			240101,
			360163,
			540217,
			810343,
			1215497,
			1823231,
			2734867,
			4102283,
			6153409,
			9230113,
			13845163
		};

		//
		// Constructors
		//

		public Hashtable () : this (0, 1.0f) {}

		[Obsolete ("Please use Hashtable(int, float, IEqualityComparer) instead")]
		public Hashtable (int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer) {
			if (capacity<0)
				throw new ArgumentOutOfRangeException ("capacity", "negative capacity");

			if (loadFactor < 0.1f || loadFactor > 1.0f || Single.IsNaN (loadFactor))
				throw new ArgumentOutOfRangeException ("loadFactor", "load factor");

			if (capacity == 0) ++capacity;
			this.loadFactor = 0.75f*loadFactor;
			double tableSize = capacity / this.loadFactor;

                        if (tableSize > Int32.MaxValue)
                                throw new ArgumentException ("Size is too big");

                        int size = (int) tableSize;
			size = ToPrime (size);
			this.SetTable (new Slot [size], new int [size]);

			this.hcp = hcp;
			this.comparer = comparer;

			this.inUse = 0;
			this.modificationCount = 0;
		}

		public Hashtable (int capacity, float loadFactor) :
			this (capacity, loadFactor, null, null)
		{
		}

		public Hashtable (int capacity) : this (capacity, 1.0f)
		{
		}

		//
		// Used as a faster Clone
		//
		internal Hashtable (Hashtable source)
		{
			inUse = source.inUse;
			loadFactor = source.loadFactor;

			table = (Slot []) source.table.Clone ();
			hashes = (int []) source.hashes.Clone ();
			threshold = source.threshold;
			hcpRef = source.hcpRef;
			comparerRef = source.comparerRef;
			equalityComparer = source.equalityComparer;
		}
			
		[Obsolete ("Please use Hashtable(int, IEqualityComparer) instead")]
		public Hashtable (int capacity,
		                  IHashCodeProvider hcp,
		                  IComparer comparer)
			: this (capacity, 1.0f, hcp, comparer)
		{
		}

		[Obsolete ("Please use Hashtable(IDictionary, float, IEqualityComparer) instead")]
		public Hashtable (IDictionary d, float loadFactor,
		                  IHashCodeProvider hcp, IComparer comparer)
			: this (d!=null ? d.Count : 0,
		                loadFactor, hcp, comparer)
		{

			if (d  ==  null)
				throw new ArgumentNullException ("dictionary");

			IDictionaryEnumerator it = d.GetEnumerator ();
			while (it.MoveNext ()) {
				Add (it.Key, it.Value);
			}

		}

		public Hashtable (IDictionary d, float loadFactor)
		       : this (d, loadFactor, null, null)
		{
		}


		public Hashtable (IDictionary d) : this (d, 1.0f)
		{
		}

		[Obsolete ("Please use Hashtable(IDictionary, IEqualityComparer) instead")]
		public Hashtable (IDictionary d, IHashCodeProvider hcp, IComparer comparer)
		                 : this (d, 1.0f, hcp, comparer)
		{
		}

		[Obsolete ("Please use Hashtable(IEqualityComparer) instead")]
		public Hashtable (IHashCodeProvider hcp, IComparer comparer)
		                 : this (1, 1.0f, hcp, comparer)
		{
		}

		protected Hashtable (SerializationInfo info, StreamingContext context)
		{
			serializationInfo = info;
		}

		public Hashtable (IDictionary d, IEqualityComparer equalityComparer) : this (d, 1.0f, equalityComparer)
		{
		}

		public Hashtable (IDictionary d, float loadFactor, IEqualityComparer equalityComparer) : this (d, loadFactor)
		{
			this.equalityComparer = equalityComparer;
		}

		public Hashtable (IEqualityComparer equalityComparer) : this (1, 1.0f, equalityComparer)
		{
		}

		public Hashtable (int capacity, IEqualityComparer equalityComparer) : this (capacity, 1.0f, equalityComparer)
		{
		}

		public Hashtable (int capacity, float loadFactor, IEqualityComparer equalityComparer) : this (capacity, loadFactor)
		{
			this.equalityComparer = equalityComparer;
		}

		//
		// Properties
		//

		[Obsolete ("Please use EqualityComparer property.")]
		protected IComparer comparer {
			set {
				comparerRef = value;
			}
			get {
				return comparerRef;
			}
		}

		[Obsolete ("Please use EqualityComparer property.")]
		protected IHashCodeProvider hcp {
			set {
				hcpRef = value;
			}
			get {
				return hcpRef;
			}
		}

		protected IEqualityComparer EqualityComparer {
			get {
				return equalityComparer;
			}
		}

		// ICollection

		public virtual int Count {
			get {
				return inUse;
			}
		}

		public virtual bool IsSynchronized {
			get {
				return false;
			}
		}

		public virtual Object SyncRoot {
			get {
				return this;
			}
		}



		// IDictionary

		public virtual bool IsFixedSize {
			get {
				return false;
			}
		}


		public virtual bool IsReadOnly {
			get {
				return false;
			}
		}

		public virtual ICollection Keys {
			get {
				if (this.hashKeys == null)
					this.hashKeys = new HashKeys (this);
				return this.hashKeys;
			}
		}

		public virtual ICollection Values {
			get {
				if (this.hashValues == null)
					this.hashValues = new HashValues (this);
				return this.hashValues;
			}
		}



		public virtual Object this [Object key] {
			get {
				if (key == null)
					throw new ArgumentNullException ("key", "null key");
	
				Slot [] table = this.table;
				int [] hashes = this.hashes;
				uint size = (uint) table.Length;
				int h = this.GetHash (key) & Int32.MaxValue;
				uint indx = (uint)h;
				uint step = (uint) ((h >> 5)+1) % (size-1)+1;
				
	
				for (uint i = size; i > 0; i--) {
					indx %= size;
					Slot entry = table [indx];
					int hashMix = hashes [indx];
					Object k = entry.key;
					if (k == null)
						break;
					
					if (k == key || ((hashMix & Int32.MaxValue) == h
					    && this.KeyEquals (key, k))) {
						return entry.value;
					}
	
					if ((hashMix & CHAIN_MARKER) == 0)
						break;
	
					indx += step;
				}
				
				return null;
			}
			
			set {
				PutImpl (key, value, true);
			}
		}




		//
		// Interface methods
		//


		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.ENTRY_MODE);
		}


		// ICollection
		public virtual void CopyTo (Array array, int arrayIndex)
		{
			if (null == array)
				throw new ArgumentNullException ("array");

			if (arrayIndex < 0)
				throw new ArgumentOutOfRangeException ("arrayIndex");

			if (array.Rank > 1)
				throw new ArgumentException ("array is multidimensional");

			if ((array.Length > 0) && (arrayIndex >= array.Length))
				throw new ArgumentException ("arrayIndex is equal to or greater than array.Length");

			if (arrayIndex + this.inUse > array.Length)
				throw new ArgumentException ("Not enough room from arrayIndex to end of array for this Hashtable");

			IDictionaryEnumerator it = GetEnumerator ();
			int i = arrayIndex;

			while (it.MoveNext ()) {
				array.SetValue (it.Entry, i++);
			}
		}


		// IDictionary

		public virtual void Add (Object key, Object value)
		{
			PutImpl (key, value, false);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.Success)]
		public virtual void Clear ()
		{
			for (int i = 0;i<table.Length;i++) {
				table [i].key = null;
				table [i].value = null;
				hashes [i] = 0;
			}

			inUse = 0;
			modificationCount++;
		}

		public virtual bool Contains (Object key)
		{
			return (Find (key) >= 0);
		}

		public virtual IDictionaryEnumerator GetEnumerator ()
		{
			return new Enumerator (this, EnumeratorMode.ENTRY_MODE);
		}

		[ReliabilityContractAttribute (Consistency.WillNotCorruptState, Cer.MayFail)]
		public virtual void Remove (Object key)
		{
			int i = Find (key);
			if (i >= 0) {
				Slot [] table = this.table;
				int h = hashes [i];
				h &= CHAIN_MARKER;
				hashes [i] = h;
				table [i].key = (h != 0)
				              ? KeyMarker.Removed
				              : null;
				table [i].value = null;
				--inUse;
				++modificationCount;
			}
		}

		public virtual bool ContainsKey (object key)
		{
			return Contains (key);
		}

		public virtual bool ContainsValue (object value)
		{
			int size = this.table.Length;
			Slot [] table = this.table;
			if (value == null) {
				for (int i = 0; i < size; i++) {
					Slot entry = table [i];
					if (entry.key != null && entry.key!= KeyMarker.Removed
				    	&& entry.value == null) {
						return true;
					}
				}
			} else { 
				for (int i = 0; i < size; i++) {
					Slot entry = table [i];
					if (entry.key != null && entry.key!= KeyMarker.Removed
				    	&& value.Equals (entry.value)) {
						return true;
					}
				}
			}
			return false;
		}


		// ICloneable

		public virtual object Clone ()
		{
			return new Hashtable (this);
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			info.AddValue ("LoadFactor", loadFactor);
			info.AddValue ("Version", modificationCount);
			if (equalityComparer != null)
				info.AddValue ("KeyComparer", equalityComparer);
			else
				info.AddValue ("Comparer", comparerRef);
			if (hcpRef != null)
				info.AddValue ("HashCodeProvider", hcpRef);
			info.AddValue ("HashSize", this.table.Length);
// Create Keys
                        Object [] keys = new Object[inUse];
			CopyToArray(keys, 0, EnumeratorMode.KEY_MODE); 
  
// Create Values
                        Object [] values = new Object[inUse];
			CopyToArray(values, 0, EnumeratorMode.VALUE_MODE);

			info.AddValue ("Keys", keys);
			info.AddValue ("Values", values);

			info.AddValue ("equalityComparer", equalityComparer);
		}

		[MonoTODO ("Serialize equalityComparer")]
		public virtual void OnDeserialization (object sender)
		{
			if (serializationInfo == null) return;

			loadFactor = (float) serializationInfo.GetValue ("LoadFactor", typeof(float));
			modificationCount = (int) serializationInfo.GetValue ("Version", typeof(int));
			try {
				equalityComparer = (IEqualityComparer) serializationInfo.GetValue ("KeyComparer", typeof (object));
			} catch {
				// If not found, try to get "Comparer"
			}
			
			if (equalityComparer == null)
				comparerRef = (IComparer) serializationInfo.GetValue ("Comparer", typeof (object));
			try {
				hcpRef = (IHashCodeProvider) serializationInfo.GetValue ("HashCodeProvider", typeof (object));
			} catch {} // Missing value => null
			int size = (int) serializationInfo.GetValue ("HashSize", typeof(int));
			
			Object [] keys = (Object []) serializationInfo.GetValue("Keys", typeof(Object [] ));
			Object [] values = (Object []) serializationInfo.GetValue("Values", typeof(Object [] ));

			if (keys.Length != values.Length) 
			  throw new SerializationException("Keys and values of uneven size");
			 
			size = ToPrime (size);
			this.SetTable (new Slot [size], new int [size]);
			
			for(int i=0;i<keys.Length;i++)
				Add(keys[i], values[i]);
		
 			AdjustThreshold();
			
			serializationInfo = null;
		}

		/// <summary>
		///  Returns a synchronized (thread-safe)
		///  wrapper for the Hashtable.
		/// </summary>
		public static Hashtable Synchronized (Hashtable table)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			return new SyncHashtable (table);
		}



		//
		// Protected instance methods
		//

		/// <summary>Returns the hash code for the specified key.</summary>
		protected virtual int GetHash (Object key)
		{
			if (equalityComparer != null)
				return equalityComparer.GetHashCode (key);
			if (hcpRef == null)
				return key.GetHashCode ();
			
			return hcpRef.GetHashCode (key);
		}

		/// <summary>
		///  Compares a specific Object with a specific key
		///  in the Hashtable.
		/// </summary>
		protected virtual bool KeyEquals (Object item, Object key)
		{
			if (key == KeyMarker.Removed)
				return false;
			if (equalityComparer != null)
				return equalityComparer.Equals (item, key);
			if (comparerRef == null)
				return item.Equals (key);
			
			return comparerRef.Compare (item, key) == 0;
		}



		//
		// Private instance methods
		//

		private void AdjustThreshold ()
		{
			int size = table.Length;

			threshold = (int) (size*loadFactor);
			if (this.threshold >= size)
				threshold = size-1;
		}

		private void SetTable (Slot [] table, int[] hashes)
		{
			if (table == null)
				throw new ArgumentNullException ("table");

			this.table = table;
			this.hashes = hashes;
			AdjustThreshold ();
		}

		private int Find (Object key)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "null key");

			Slot [] table = this.table;
			int [] hashes = this.hashes;
			uint size = (uint) table.Length;
			int h = this.GetHash (key) & Int32.MaxValue;
			uint indx = (uint)h;
			uint step = (uint) ((h >> 5)+1) % (size-1)+1;
			

			for (uint i = size; i > 0; i--) {
				indx %= size;
				Slot entry = table [indx];
				int hashMix = hashes [indx];
				Object k = entry.key;
				if (k == null)
					break;
				
				if (k == key || ((hashMix & Int32.MaxValue) == h
				    && this.KeyEquals (key, k))) {
					return (int) indx;
				}

				if ((hashMix & CHAIN_MARKER) == 0)
					break;

				indx += step;
			}
			return -1;
		}


		private void Rehash ()
		{
			int oldSize = this.table.Length;

			// From the SDK docs:
			//   Hashtable is automatically increased
			//   to the smallest prime number that is larger
			//   than twice the current number of Hashtable buckets
			uint newSize = (uint)ToPrime ((oldSize<<1)|1);


			Slot [] newTable = new Slot [newSize];
			Slot [] table = this.table;
			int [] newHashes = new int [newSize];
			int [] hashes = this.hashes;

			for (int i = 0;i<oldSize;i++) {
				Slot s = table [i];
				if (s.key!= null) {
					int h = hashes [i] & Int32.MaxValue;
					uint spot = (uint)h;
					uint step = ((uint) (h>>5)+1)% (newSize-1)+1;
					for (uint j = spot%newSize;;spot+= step, j = spot%newSize) {
						// No check for KeyMarker.Removed here,
						// because the table is just allocated.
						if (newTable [j].key == null) {
							newTable [j].key = s.key;
							newTable [j].value = s.value;
							newHashes [j] |= h;
							break;
						} else {
							newHashes [j] |= CHAIN_MARKER;
						}
					}
				}
			}

			++this.modificationCount;

			this.SetTable (newTable, newHashes);
		}


		private void PutImpl (Object key, Object value, bool overwrite)
		{
			if (key == null)
				throw new ArgumentNullException ("key", "null key");

			if (this.inUse >= this.threshold) 
				this.Rehash ();

			uint size = (uint)this.table.Length;

			int h = this.GetHash (key) & Int32.MaxValue;
			uint spot = (uint)h;
			uint step = (uint) ((spot>>5)+1)% (size-1)+1;
			Slot [] table = this.table;
			int [] hashes = this.hashes;
			Slot entry;

			int freeIndx = -1;
			for (int i = 0; i < size; i++) {
				int indx = (int) (spot % size);
				entry = table [indx];
				int hashMix = hashes [indx];

				if (freeIndx == -1
				    && entry.key == KeyMarker.Removed
				    && (hashMix & CHAIN_MARKER) != 0)
					freeIndx = indx;

				if (entry.key == null ||
				    (entry.key == KeyMarker.Removed
				     && (hashMix & CHAIN_MARKER) == 0)) {

					if (freeIndx == -1)
						freeIndx = indx;
					break;
				}

				if ((hashMix & Int32.MaxValue) == h && KeyEquals (key, entry.key)) {
					if (overwrite) {
						table [indx].value = value;
						++this.modificationCount;
					} else {
						// Handle Add ():
						// An entry with the same key already exists in the Hashtable.
						throw new ArgumentException (
								"Key duplication when adding: " + key);
					}
					return;
				}

				if (freeIndx == -1) {
					hashes [indx] |= CHAIN_MARKER;
				}

				spot+= step;

			}

			if (freeIndx!= -1) {
				table [freeIndx].key = key;
				table [freeIndx].value = value;
				hashes [freeIndx] |= h;

				++this.inUse;
				++this.modificationCount;
			}

		}

		private void  CopyToArray (Array arr, int i,
					   EnumeratorMode mode)
		{
			IEnumerator it = new Enumerator (this, mode);

			while (it.MoveNext ()) {
				arr.SetValue (it.Current, i++);
			}
		}



		//
		// Private static methods
		//
		internal static bool TestPrime (int x)
		{
			if ((x & 1) != 0) {
				int top = (int)Math.Sqrt (x);
				
				for (int n = 3; n < top; n += 2) {
					if ((x % n) == 0)
						return false;
				}
				return true;
			}
			// There is only one even prime - 2.
			return (x == 2);
		}

		internal static int CalcPrime (int x)
		{
			for (int i = (x & (~1))-1; i< Int32.MaxValue; i += 2) {
				if (TestPrime (i)) return i;
			}
			return x;
		}

		internal static int ToPrime (int x)
		{
			for (int i = 0; i < primeTbl.Length; i++) {
				if (x <= primeTbl [i])
					return primeTbl [i];
			}
			return CalcPrime (x);
		}




		//
		// Inner classes
		//

		private enum EnumeratorMode : int {KEY_MODE = 0, VALUE_MODE, ENTRY_MODE};

		[Serializable]
		private sealed class Enumerator : IDictionaryEnumerator, IEnumerator {

			private Hashtable host;
			private int stamp;
			private int pos;
			private int size;
			private EnumeratorMode mode;

			private Object currentKey;
			private Object currentValue;

			private readonly static string xstr = "Hashtable.Enumerator: snapshot out of sync.";

			public Enumerator (Hashtable host, EnumeratorMode mode) {
				this.host = host;
				stamp = host.modificationCount;
				size = host.table.Length;
				this.mode = mode;
				Reset ();
			}

			public Enumerator (Hashtable host)
			           : this (host, EnumeratorMode.KEY_MODE) {}


			private void FailFast ()
			{
				if (host.modificationCount != stamp) {
					throw new InvalidOperationException (xstr);
				}
			}

			public void Reset ()
			{
				FailFast ();

				pos = -1;
				currentKey = null;
				currentValue = null;
			}

			public bool MoveNext ()
			{
				FailFast ();

				if (pos < size) {
					while (++pos < size) {
						Slot entry = host.table [pos];

						if (entry.key != null && entry.key != KeyMarker.Removed) {
							currentKey = entry.key;
							currentValue = entry.value;
							return true;
						}
					}
				}

				currentKey = null;
				currentValue = null;
				return false;
			}

			public DictionaryEntry Entry
			{
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					FailFast ();
					return new DictionaryEntry (currentKey, currentValue);
				}
			}

			public Object Key {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					FailFast ();
					return currentKey;
				}
			}

			public Object Value {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
					FailFast ();
					return currentValue;
				}
			}

			public Object Current {
				get {
					if (currentKey == null) throw new InvalidOperationException ();
						
					switch (mode) {
					case EnumeratorMode.KEY_MODE:
						return currentKey;
					case EnumeratorMode.VALUE_MODE:
						return currentValue;
					case EnumeratorMode.ENTRY_MODE:
						return new DictionaryEntry (currentKey, currentValue);
					}
					throw new Exception ("should never happen");
				}
			}
		}

		[Serializable]
#if INSIDE_CORLIB
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
#endif
		private class HashKeys : ICollection, IEnumerable {

			private Hashtable host;

			public HashKeys (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			// ICollection

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return host.IsSynchronized;
				}
			}

			public virtual Object SyncRoot {
				get {return host.SyncRoot;}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (array.Rank != 1)
					throw new ArgumentException ("array");
				if (arrayIndex < 0) 
					throw new ArgumentOutOfRangeException ("arrayIndex");
				if (array.Length - arrayIndex < Count)
					throw new ArgumentException ("not enough space");
				
				host.CopyToArray (array, arrayIndex, EnumeratorMode.KEY_MODE);
			}

			// IEnumerable

			public virtual IEnumerator GetEnumerator ()
			{
				return new Hashtable.Enumerator (host, EnumeratorMode.KEY_MODE);
			}
		}

		[Serializable]
#if INSIDE_CORLIB
		[DebuggerDisplay ("Count={Count}")]
		[DebuggerTypeProxy (typeof (CollectionDebuggerView))]
#endif
		private class HashValues : ICollection, IEnumerable {

			private Hashtable host;

			public HashValues (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			// ICollection

			public virtual int Count {
				get {
					return host.Count;
				}
			}

			public virtual bool IsSynchronized {
				get {
					return host.IsSynchronized;
				}
			}

			public virtual Object SyncRoot {
				get {
					return host.SyncRoot;
				}
			}

			public virtual void CopyTo (Array array, int arrayIndex)
			{
				if (array == null)
					throw new ArgumentNullException ("array");
				if (array.Rank != 1)
					throw new ArgumentException ("array");
				if (arrayIndex < 0) 
					throw new ArgumentOutOfRangeException ("arrayIndex");
				if (array.Length - arrayIndex < Count)
					throw new ArgumentException ("not enough space");
				
				host.CopyToArray (array, arrayIndex, EnumeratorMode.VALUE_MODE);
			}

			// IEnumerable

			public virtual IEnumerator GetEnumerator ()
			{
				return new Hashtable.Enumerator (host, EnumeratorMode.VALUE_MODE);
			}
		}


		[Serializable]
		private class SyncHashtable : Hashtable, IEnumerable {

			private Hashtable host;

			public SyncHashtable (Hashtable host) {
				if (host == null)
					throw new ArgumentNullException ();

				this.host = host;
			}

			internal SyncHashtable (SerializationInfo info, StreamingContext context)
			{
				host = (Hashtable) info.GetValue("ParentTable", typeof(Hashtable));
			}
			
			public override void GetObjectData (SerializationInfo info, StreamingContext context)
			{
				info.AddValue ("ParentTable", host);
			}
			
			// ICollection

			public override int Count {
				get {
					return host.Count;
				}
			}

			public override bool IsSynchronized {
				get {
					return true;
				}
			}

			public override Object SyncRoot {
				get {
					return host.SyncRoot;
				}
			}



			// IDictionary

			public override bool IsFixedSize {
				get {
					return host.IsFixedSize;
				}     
			}


			public override bool IsReadOnly {
				get {
					return host.IsReadOnly;
				}
			}

			public override ICollection Keys {
				get {
					ICollection keys = null;
					lock (host.SyncRoot) {
						keys = host.Keys;
					}
					return keys;
				}
			}

			public override ICollection Values {
				get {
					ICollection vals = null;
					lock (host.SyncRoot) {
						vals = host.Values;
					}
					return vals;
				}
			}



			public override Object this [Object key] {
				get {
					return host [key];
				}
				set {
					lock (host.SyncRoot) {
						host [key] = value;
					}
				}
			}

			// IEnumerable

			IEnumerator IEnumerable.GetEnumerator ()
			{
				return new Enumerator (host, EnumeratorMode.ENTRY_MODE);
			}




			// ICollection

			public override void CopyTo (Array array, int arrayIndex)
			{
				host.CopyTo (array, arrayIndex);
			}


			// IDictionary

			public override void Add (Object key, Object value)
			{
				lock (host.SyncRoot) {
					host.Add (key, value);
				}
			}

			public override void Clear () 
			{
				lock (host.SyncRoot) {
					host.Clear ();
				}
			}

			public override bool Contains (Object key)
			{
				return (host.Find (key) >= 0);
			}

			public override IDictionaryEnumerator GetEnumerator ()
			{
				return new Enumerator (host, EnumeratorMode.ENTRY_MODE);
			}

			public override void Remove (Object key)
			{
				lock (host.SyncRoot) {
					host.Remove (key);
				}
			}



			public override bool ContainsKey (object key)
			{
				return host.Contains (key);
			}

			public override bool ContainsValue (object value)
			{
				return host.ContainsValue (value);
			}


			// ICloneable

			public override object Clone ()
			{
				lock(host.SyncRoot) {
					return new SyncHashtable( (Hashtable) host.Clone () );
				}
			}

		} // SyncHashtable


	} // Hashtable

}
