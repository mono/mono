//
// System.Collections.Hashtable
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//



using System;
using System.Collections;


// TODO: 1. Interfaces to implement: ISerializable and IDeserializationCallback;
//          Synchronized wrapper (it's really easy but requires at least
//          System.Threading.Monitor to be present, maybe just a stub for now).
//       2. Meaningfull error messages for all exceptions.


namespace System.Collections {

	public class Hashtable : IDictionary, ICollection,
	                         IEnumerable, ICloneable {


		internal struct slot {
			internal Object key;

			internal Object value;

			// Hashcode. Chains are also marked through this.
			internal int hashMix;
		}



		//
		// Private data
		//

		private readonly static int CHAIN_MARKER=~Int32.MaxValue;
		private readonly static int ALLOC_GRAIN=0x2F;

		// Used as indicator for the removed parts of a chain.
		private readonly static Object REMOVED_MARKER=new Object();


		private int inUse;
		private int modificationCount;
		private float loadFactor;
		private slot[] table;
		private int threshold;

		private IHashCodeProvider m_hcp;
		private IComparer m_comparer;

		public static int[] primeTbl={};


		// Class constructor

		static Hashtable() {
			// NOTE: Precalculate primes table.
			// This precalculated table of primes is intended
			// to speed-up allocations/resize for relatively
			// small tables.
			// I'm not sure whether it's a good idea or not.
			// Also I am in doubt as for the quality of this
			// particular implementation, probably the increment
			// shouldn't be linear? Consider this as a hack
			// or as a placeholder for future improvements.
			int size=0x2000/ALLOC_GRAIN;
			primeTbl=new int[size];
			for (int x=53,i=0;i<size;x+=ALLOC_GRAIN,i++) {
				primeTbl[i]=CalcPrime(x);
			}
		}


		//
		// Constructors
		//

		public Hashtable() : this(0,1.0f) {}


		public Hashtable(int capacity, float loadFactor, IHashCodeProvider hcp, IComparer comparer) {
			if (capacity<0)
				throw new ArgumentOutOfRangeException("negative capacity");

			if (loadFactor<0.1 || loadFactor>1)
				throw new ArgumentOutOfRangeException("load factor");

			if (capacity==0) ++capacity;
			this.loadFactor=0.75f*loadFactor;
			int size=(int)(capacity/this.loadFactor);
			size=ToPrime(size);
			this.SetTable(new slot[size]);

			this.hcp=hcp;
			this.comparer=comparer;

			this.inUse=0;
			this.modificationCount=0;

		}

		public Hashtable(int capacity, float loadFactor) :
			this(capacity,loadFactor,null,null) {}

		public Hashtable(int capacity) : this(capacity,1.0f) {}

		public Hashtable(int capacity,
		                 IHashCodeProvider hcp,
		                 IComparer comparer
		                ) : this(capacity,1.0f,hcp,comparer) {}


		public Hashtable(IDictionary d, float loadFactor,
		                 IHashCodeProvider hcp,IComparer comparer)
		                 : this(d!=null?d.Count:0,
		                        loadFactor,hcp,comparer) {
			if (d==null)
				throw new ArgumentNullException("dictionary");

			IDictionaryEnumerator it=d.GetEnumerator();
			while (it.MoveNext()) {
				Add(it.Key,it.Value);
			}
			
		}

		public Hashtable(IDictionary d, float loadFactor)
		       : this(d,loadFactor,null,null) {}


		public Hashtable(IDictionary d) : this(d,1.0f) {}

		public Hashtable(IDictionary d, IHashCodeProvider hcp,IComparer comparer)
		                 : this(d,1.0f,hcp,comparer) {}

		public Hashtable(IHashCodeProvider hcp,IComparer comparer)
		                 : this(1,1.0f,hcp,comparer) {}



		//
		// Properties
		//

		protected IComparer comparer {
			set {
				m_comparer=value;
			}
			get {
				return m_comparer;
			}
		}

		protected IHashCodeProvider hcp {
			set {
				m_hcp=value;
			}
			get {
				return m_hcp;
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
				return new HashKeys(this);
			}
		}

		public virtual ICollection Values {
			get {
				return new HashValues(this);
			}
		}



		public virtual Object this[Object key] {
			get {
				return GetImpl(key);
			}
			set {
				PutImpl(key,value,true);
			}
		}




		//
		// Interface methods
		//


		// IEnumerable

		IEnumerator IEnumerable.GetEnumerator() {
			return new Enumerator(this,EnumeratorMode.KEY_MODE);
		}


		// ICollection
		public virtual void CopyTo(Array array, int arrayIndex) {
			IDictionaryEnumerator it=GetEnumerator();
			int i=arrayIndex;
			while (it.MoveNext()) {
				array.SetValue(it.Entry,i++);
			}
		}


		// IDictionary

		public virtual void Add(Object key, Object value) {
			PutImpl(key,value,false);
		}

		public virtual void Clear() {
			for (int i=0;i<table.Length;i++) {
				table[i].key=null;
				table[i].value=null;
				table[i].hashMix=0;
			}
		}

		public virtual bool Contains(Object key) {
			return (Find(key)>=0);
		}

		public virtual IDictionaryEnumerator GetEnumerator() {
			return new Enumerator(this,EnumeratorMode.KEY_MODE);
		}

		public virtual void Remove(Object key) {
			int i=Find(key);
			slot[] table=this.table;
			if (i>=0) {
				int h=table[i].hashMix;
				h&=CHAIN_MARKER;
				table[i].hashMix=h;
				table[i].key=(h!=0)
				              ? REMOVED_MARKER
				              : null;
				table[i].value=null;
				--inUse;
				++modificationCount;
			}
		}




		public virtual bool ContainsKey(object key) {
			return Contains(key);
		}

		public virtual bool ContainsValue(object value) {
			int size=this.table.Length;
			slot[] table=this.table;

			for (int i=0;i<size;i++) {
				slot entry=table[i];
				if (entry.key!=null
				    && entry.key!=REMOVED_MARKER
				    && value.Equals(entry.value)) {
					return true;
				}
			}
			return false;
		}




		// ICloneable

		public virtual object Clone() {
			Hashtable ht=new Hashtable(Count, hcp, comparer);
			ht.modificationCount=this.modificationCount;
			ht.inUse=this.inUse;
			ht.AdjustThreshold();

			// FIXME: maybe it's faster to simply
			//        copy the back-end array?
			IDictionaryEnumerator it=GetEnumerator();
			while (it.MoveNext()) {
				ht[it.Key]=it.Value;
			}

			return ht;
		}



		// TODO: public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {}
		// TODO: public virtual void OnDeserialization(object sender);


		public override string ToString() {
			// FIXME: What's it supposed to do?
			//        Maybe print out some internals here? Anyway.
			return "mono::System.Collections.Hashtable";
		}


		/// <summary>
		///  Returns a synchronized (thread-safe)
		///  wrapper for the Hashtable.
		/// </summary>
		public static Hashtable Synchronized(Hashtable table) {
			// TODO: implement
			return null;
		}



		//
		// Protected instance methods
		//

		/// <summary>Returns the hash code for the specified key.</summary>
		protected virtual int GetHash(Object key) {
			IHashCodeProvider hcp=this.hcp;
			return (hcp!=null)
			        ? hcp.GetHashCode()
			        : key.GetHashCode();
		}

		/// <summary>
		///  Compares a specific Object with a specific key
		///  in the Hashtable.
		/// </summary>
		protected virtual bool KeyEquals(Object item,Object key) {
			IComparer c=this.comparer;
			if (c!=null)
				return (c.Compare(item,key)==0);
			else
				return item.Equals(key);
		}



		//
		// Private instance methods
		//

		private void AdjustThreshold() {
			int size=table.Length;
			threshold=(int)(size*loadFactor);
			if (this.threshold>=size) threshold=size-1;
		}

		private void SetTable(slot[] table) {
			if (table==null)
				throw new ArgumentNullException("table");

			this.table=table;
			AdjustThreshold();
		}

		private Object GetImpl(Object key) {
			int i=Find(key);

			if (i>=0)
				return table[i].value;
			else
				return null;
		}


		private int Find(Object key) {
			if (key==null)
				throw new ArgumentNullException("null key");

			uint size=(uint)this.table.Length;
			int h=this.GetHash(key) & Int32.MaxValue;
			uint spot=(uint)h;
			uint step=(uint)((h>>5)+1)%(size-1)+1;
			slot[] table=this.table;

			for (int i=0;i<size;i++) {
				int indx=(int)(spot%size);
				slot entry=table[indx];
				Object k=entry.key;
				if (k==null) return -1;
				if ((entry.hashMix & Int32.MaxValue)==h
				   && this.KeyEquals(key,k)) {
					return indx;
				}

				if ((entry.hashMix & CHAIN_MARKER)==0)
					return -1;

				spot+=step;
			}
			return -1;
		}


		private void Rehash() {
			int oldSize=this.table.Length;

			// From the SDK docs:
			//   Hashtable is automatically increased
			//   to the smallest prime number that is larger
			//   than twice the current number of Hashtable buckets
			uint newSize=(uint)ToPrime((oldSize<<1)|1);


			slot[] newTable=new slot[newSize];
			slot[] table=this.table;

			for (int i=0;i<oldSize;i++) {
				slot s=table[i];
				if (s.key!=null) {
					int h=s.hashMix & Int32.MaxValue;
					uint spot=(uint)h;
					uint step=((uint)(h>>5)+1)%(newSize-1)+1;
					for (uint j=spot%newSize;;spot+=step,j=spot%newSize) {
						// No check for REMOVED_MARKER here,
						// because the table is just allocated.
						if (newTable[j].key==null) {
							newTable[j].key=s.key;
							newTable[j].value=s.value;
							newTable[j].hashMix|=h;
							break;
						} else {
							newTable[j].hashMix|=CHAIN_MARKER;
						}
					}
				}
			}

			++this.modificationCount;

			this.SetTable(newTable);
		}


		private void PutImpl(Object key,Object value,bool overwrite) {
			if (key==null)
				throw new ArgumentNullException("null key");

			uint size=(uint)this.table.Length;
			if (this.inUse>=this.threshold) {
				this.Rehash();
				size=(uint)this.table.Length;
			}

			int h=this.GetHash(key) & Int32.MaxValue;
			uint spot=(uint)h;
			uint step=(uint)((spot>>5)+1)%(size-1)+1;
			slot[] table=this.table;
			slot entry;

			int freeIndx=-1;
			for (int i=0;i<size;i++) {
				int indx=(int)(spot%size);
				entry=table[indx];

				if (freeIndx==-1
				    && entry.key==REMOVED_MARKER
				    && (entry.hashMix & CHAIN_MARKER)!=0) freeIndx=indx;

				if (entry.key==null ||
				    (entry.key==REMOVED_MARKER
				     && (entry.hashMix & CHAIN_MARKER)!=0)) {

					if (freeIndx==-1) freeIndx=indx;
					break;
				}

				if ((entry.hashMix & Int32.MaxValue)==h
				   && KeyEquals(key,entry.key)) {
					if (overwrite) {
						table[indx].value=value;
						++this.modificationCount;
					} else {
						// Handle Add():
						// An entry with the same key already exists in the Hashtable.
						throw new ArgumentException("Key duplication");
					}
					return;
				}

				if (freeIndx==-1) {
					table[indx].hashMix|=CHAIN_MARKER;
				}

				spot+=step;

			}

			if (freeIndx!=-1) {
				table[freeIndx].key=key;
				table[freeIndx].value=value;
				table[freeIndx].hashMix|=h;

				++this.inUse;
				++this.modificationCount;
			}

		}

		private void  CopyToArray(Array arr,int i,
		                          EnumeratorMode mode) {
			IEnumerator it=new Enumerator(this,mode);
			while (it.MoveNext()) {
				arr.SetValue(it.Current,i++);
			}
		}



		//
		// Private static methods
		//
		private static bool TestPrime(int x) {
			if ((x & 1)!=0) {
				for (int n=3;n<(int)Math.Sqrt(x);n+=2) {
					if (x%n==0) return false;
				}
				return true;
			}
			// There is only one even prime - 2.
			return (x==2);
		}

		private static int CalcPrime(int x) {
			for (int i=(x&(~1))-1;i<Int32.MaxValue;i+=2) {
				if (TestPrime(i)) return i;
			}
			return x;
		}

		private static int ToPrime(int x) {
			for (int i=x/ALLOC_GRAIN;i<primeTbl.Length;i++) {
				if (x<=primeTbl[i]) return primeTbl[i];
			}
			return CalcPrime(x);
		}




		//
		// Inner classes
		//

		public enum EnumeratorMode : int {KEY_MODE=0,VALUE_MODE};

		protected sealed class Enumerator : IDictionaryEnumerator, IEnumerator {

			private Hashtable host;
			private int stamp;
			private int pos;
			private int size;
			private EnumeratorMode mode;

			private Object currentKey;
			private Object currentValue;

			private readonly static string xstr="Hashtable.Enumerator: snapshot out of sync.";

			public Enumerator(Hashtable host,EnumeratorMode mode) {
				this.host=host;
				stamp=host.modificationCount;
				size=host.table.Length;
				this.mode=mode;
				Reset();
			}

			public Enumerator(Hashtable host)
			           : this(host,EnumeratorMode.KEY_MODE) {}


			private void FailFast() {
				if (host.modificationCount!=stamp) {
					throw new InvalidOperationException(xstr);
				}
			}

			public void Reset() {
				FailFast();

				pos=-1;
				currentKey=null;
				currentValue=null;
			}

			public bool MoveNext() {
				FailFast();

				if (pos<size) while (++pos<size) {
					slot entry=host.table[pos];
					if (entry.key!=null && entry.key!=REMOVED_MARKER) {
						currentKey=entry.key;
						currentValue=entry.value;
						return true;
					}
				}
				currentKey=null;
				currentValue=null;
				return false;
			}

			public DictionaryEntry Entry {
				get {
					FailFast();
					return new DictionaryEntry(currentKey,currentValue);
				}
			}

			public Object Key {
				get {
					FailFast();
					return currentKey;
				}
			}

			public Object Value {
				get {
					FailFast();
					return currentValue;
				}
			}

			public Object Current {
				get {
					FailFast();
					return (mode==EnumeratorMode.KEY_MODE)
					        ? currentKey
					        : currentValue;
				}
			}
		}



		protected class HashKeys : ICollection, IEnumerable {

			private Hashtable host;
			private int count;

			public HashKeys(Hashtable host) {
				if (host==null)
					throw new ArgumentNullException();

				this.host=host;
				this.count=host.Count;
			}

			// ICollection

			public virtual int Count {
				get {return count;}
			}

			public virtual bool IsSynchronized {
				get {return host.IsSynchronized;}
			}

			public virtual Object SyncRoot {
				get {return host.SyncRoot;}
			}

			public virtual void CopyTo(Array array, int arrayIndex) {
				host.CopyToArray(array,arrayIndex,EnumeratorMode.KEY_MODE);
			}

			// IEnumerable

			public virtual IEnumerator GetEnumerator() {
				return new Hashtable.Enumerator(host,EnumeratorMode.KEY_MODE);
			}
		}


		protected class HashValues : ICollection, IEnumerable {

			private Hashtable host;
			private int count;

			public HashValues(Hashtable host) {
				if (host==null)
					throw new ArgumentNullException();

				this.host=host;
				this.count=host.Count;
			}

			// ICollection

			public virtual int Count {
				get {return count;}
			}

			public virtual bool IsSynchronized {
				get {return host.IsSynchronized;}
			}

			public virtual Object SyncRoot {
				get {return host.SyncRoot;}
			}

			public virtual void CopyTo(Array array, int arrayIndex) {
				host.CopyToArray(array,arrayIndex,EnumeratorMode.VALUE_MODE);
			}

			// IEnumerable

			public virtual IEnumerator GetEnumerator() {
				return new Hashtable.Enumerator(host,EnumeratorMode.VALUE_MODE);
			}
		}


	}
}

