#if NET_2_0
/*
 Copyright (c) 2003-2004 Niels Kokholm <kokholm@itu.dk> and Peter Sestoft <sestoft@dina.kvl.dk>
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
using MSG = System.Collections.Generic;
namespace C5
{
	/// <summary>
	/// An entry in a dictionary from K to V.
	/// </summary>
	public struct KeyValuePair<K,V>
	{
		/// <summary>
		/// The key field of the entry
		/// </summary>
		public K key;

		/// <summary>
		/// The value field of the entry
		/// </summary>
		public V value;


		/// <summary>
		/// Create an entry with specified key and value
		/// </summary>
		/// <param name="k">The key</param>
		/// <param name="v">The value</param>
		public KeyValuePair(K k, V v) { key = k; value = v; }


		/// <summary>
		/// Create an entry with a specified key. The value is undefined.
		/// </summary>
		/// <param name="k">The key</param>
		public KeyValuePair(K k) { key = k; value = default(V); }


		/// <summary>
		/// Pretty print an entry
		/// </summary>
		/// <returns>(key, value)</returns>
		[Tested]
		public override string ToString() { return "(" + key + ", " + value + ")"; }


		/// <summary>
		/// Check equality of entries
		/// </summary>
		/// <param name="obj">The other object</param>
		/// <returns>True if obj is an entry of the same type and has the same key</returns>
		[Tested]
		public override bool Equals(object obj)
		{ return obj is KeyValuePair<K,V> && key.Equals(((KeyValuePair<K,V>)obj).key); }


		/// <summary>
		/// Get the hash code of the key.
		/// </summary>
		/// <returns>The hash code</returns>
		[Tested]
		public override int GetHashCode() { return key.GetHashCode(); }
	}



	/// <summary>
	/// Default comparer for dictionary entries in a sorted dictionary.
	/// Entry comparisons only look at keys.
	/// </summary>
	public class KeyValuePairComparer<K,V>: IComparer<KeyValuePair<K,V>>
	{
		IComparer<K> myc;


		/// <summary>
		/// Create an entry comparer for a item comparer of the keys
		/// </summary>
		/// <param name="c">Comparer of keys</param>
		public KeyValuePairComparer(IComparer<K> c) { myc = c; }


		/// <summary>
		/// Compare two entries
		/// </summary>
		/// <param name="a">First entry</param>
		/// <param name="b">Second entry</param>
		/// <returns>The result of comparing the keys</returns>
		[Tested]
		public int Compare(KeyValuePair<K,V> a, KeyValuePair<K,V> b)
		{ return myc.Compare(a.key, b.key); }
	}



	/// <summary>
	/// Default hasher for dictionary entries.
	/// Operations only look at keys.
	/// </summary>
	public sealed class KeyValuePairHasher<K,V>: IHasher<KeyValuePair<K,V>>
	{
		IHasher<K> myh;


		/// <summary>
		/// Create an entry hasher using the default hasher for keys
		/// </summary>
		public KeyValuePairHasher() { myh = HasherBuilder.ByPrototype<K>.Examine(); }


		/// <summary>
		/// Create an entry hasher from a specified item hasher for the keys
		/// </summary>
		/// <param name="c">The key hasher</param>
		public KeyValuePairHasher(IHasher<K> c) { myh = c; }


		/// <summary>
		/// Get the hash code of the entry
		/// </summary>
		/// <param name="item">The entry</param>
		/// <returns>The hash code of the key</returns>
		[Tested]
		public int GetHashCode(KeyValuePair<K,V> item) { return myh.GetHashCode(item.key); }


		/// <summary>
		/// Test two entries for equality
		/// </summary>
		/// <param name="i1">First entry</param>
		/// <param name="i2">Second entry</param>
		/// <returns>True if keys are equal</returns>
		[Tested]
		public bool Equals(KeyValuePair<K,V> i1, KeyValuePair<K,V> i2)
		{ return myh.Equals(i1.key, i2.key); }
	}



	/// <summary>
	/// A base class for implementing a dictionary based on a set collection implementation.
	/// <p>See the source code for <see cref="T:C5.HashDictionary!2"/> for an example</p>
	/// 
	/// </summary>
	public abstract class DictionaryBase<K,V>: EnumerableBase<KeyValuePair<K,V>>, IDictionary<K,V>
	{
		/// <summary>
		/// The set collection of entries underlying this dictionary implementation
		/// </summary>
		protected ICollection<KeyValuePair<K,V>> pairs;


		#region IDictionary<K,V> Members

		/// <summary>
		/// 
		/// </summary>
		/// <value>The number of entrues in the dictionary</value>
		[Tested]
		public int Count { [Tested]get { return pairs.Count; } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>A distinguished object to use for locking to synchronize multithreaded access</value>
		[Tested]
		public object SyncRoot { [Tested]get { return pairs.SyncRoot; } }


		/// <summary>
		/// Add a new (key, value) pair (a mapping) to the dictionary.
		/// <exception cref="InvalidOperationException"/> if there already is an entry with the same key. 
		/// </summary>
		/// <param name="key">Key to add</param>
		/// <param name="val">Value to add</param>
		[Tested]
		public void Add(K key, V val)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key, val);

			if (!pairs.Add(p))
				throw new System.ArgumentException("Item has already been added.  Key in dictionary: '" + key + "'  Key being added: '" + key + "'");
		}


		/// <summary>
		/// Remove an entry with a given key from the dictionary
		/// </summary>
		/// <param name="key">The key of the entry to remove</param>
		/// <returns>True if an entry was found (and removed)</returns>
		[Tested]
		public bool Remove(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return pairs.Remove(p);
		}


		/// <summary>
		/// Remove an entry with a given key from the dictionary and report its value.
		/// </summary>
		/// <param name="key">The key of the entry to remove</param>
		/// <param name="val">On exit, the value of the removed entry</param>
		/// <returns>True if an entry was found (and removed)</returns>
		[Tested]
		public bool Remove(K key, out V val)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			if (pairs.RemoveWithReturn(ref p))
			{
				val = p.value;
				return true;
			}
			else
			{
				val = default(V);
				return false;
			}
		}


		/// <summary>
		/// Remove all entries from the dictionary
		/// </summary>
		[Tested]
		public void Clear() { pairs.Clear(); }


		/// <summary>
		/// Check if there is an entry with a specified key
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <returns>True if key was found</returns>
		[Tested]
		public bool Contains(K key)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			return pairs.Contains(p);
		}


		/// <summary>
		/// Check if there is an entry with a specified key and report the corresponding
		/// value if found. This can be seen as a safe form of "val = this[key]".
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <param name="val">On exit, the value of the entry</param>
		/// <returns>True if key was found</returns>
		[Tested]
		public bool Find(K key, out V val)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

			if (pairs.Find(ref p))
			{
				val = p.value;
				return true;
			}
			else
			{
				val = default(V);
				return false;
			}
		}


		/// <summary>
		/// Look for a specific key in the dictionary and if found replace the value with a new one.
		/// This can be seen as a non-adding version of "this[key] = val".
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <param name="val">The new value</param>
		/// <returns>True if key was found</returns>
		[Tested]
		public bool Update(K key, V val)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key, val);

			return pairs.Update(p);
		}


		/// <summary>
		/// Look for a specific key in the dictionary. If found, report the corresponding value,
		/// else add an entry with the key and the supplied value.
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <param name="val">On entry the value to add if the key is not found.
		/// On exit the value found if any.</param>
		/// <returns>True if key was found</returns>
		[Tested]
		public bool FindOrAdd(K key, ref V val)
		{
			KeyValuePair<K,V> p = new KeyValuePair<K,V>(key, val);

			if (!pairs.FindOrAdd(ref p))
				return false;
			else
			{
				val = p.value;
				return true;
			}
		}


		/// <summary>
		/// Update value in dictionary corresponding to key if found, else add new entry.
		/// More general than "this[key] = val;" by reporting if key was found.
		/// </summary>
		/// <param name="key">The key to look for</param>
		/// <param name="val">The value to add or replace with.</param>
		/// <returns>True if entry was updated.</returns>
		[Tested]
		public bool UpdateOrAdd(K key, V val)
		{
			return pairs.UpdateOrAdd(new KeyValuePair<K,V>(key, val));
		}



		#region Keys,Values support classes
			
		internal class ValuesCollection: CollectionValueBase<V>, ICollectionValue<V>
		{
			ICollection<KeyValuePair<K,V>> pairs;


			internal ValuesCollection(ICollection<KeyValuePair<K,V>> pairs)
			{ this.pairs = pairs; }


			[Tested]
			public override MSG.IEnumerator<V> GetEnumerator()
			{
				//Updatecheck is performed by the pairs enumerator
				foreach (KeyValuePair<K,V> p in pairs)
					yield return p.value;
			}


			[Tested]
            public override int Count { [Tested]get { return pairs.Count; } }

            public override Speed CountSpeed { get { return Speed.Constant; } }
        }



        internal class KeysCollection: CollectionValueBase<K>, ICollectionValue<K>
		{
			ICollection<KeyValuePair<K,V>> pairs;


			internal KeysCollection(ICollection<KeyValuePair<K,V>> pairs)
			{ this.pairs = pairs; }


			[Tested]
			public override MSG.IEnumerator<K> GetEnumerator()
			{
				foreach (KeyValuePair<K,V> p in pairs)
					yield return p.key;
			}


			[Tested]
			public override int Count { [Tested]get { return pairs.Count; } }

            public override Speed CountSpeed { get { return pairs.CountSpeed; } }
        }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <value>A collection containg the all the keys of the dictionary</value>
		[Tested]
		public ICollectionValue<K> Keys { [Tested]get { return new KeysCollection(pairs); } }


		/// <summary>
		/// 
		/// </summary>
		/// <value>A collection containing all the values of the dictionary</value>
		[Tested]
		public ICollectionValue<V> Values { [Tested]get { return new ValuesCollection(pairs); } }


		/// <summary>
		/// Indexer for dictionary.
		/// <exception cref="InvalidOperationException"/> if no entry is found. 
		/// </summary>
		/// <value>The value corresponding to the key</value>
		[Tested]
		public V this[K key]
		{
			[Tested]
			get
			{
				KeyValuePair<K,V> p = new KeyValuePair<K,V>(key);

				if (pairs.Find(ref p))
					return p.value;
				else
					throw new System.ArgumentException("Key not present in Dictionary");
			}
			[Tested]
			set
			{ pairs.UpdateOrAdd(new KeyValuePair<K,V>(key, value)); }
		}


		/// <summary>
		/// 
		/// </summary>
		/// <value>True if dictionary is read  only</value>
		[Tested]
		public bool IsReadOnly { [Tested]get { return pairs.IsReadOnly; } }


		/// <summary>
		/// Check the integrity of the internal data structures of this dictionary.
		/// Only avaliable in DEBUG builds???
		/// </summary>
		/// <returns>True if check does not fail.</returns>
		[Tested]
		public bool Check() { return pairs.Check(); }

		#endregion

		#region IEnumerable<KeyValuePair<K,V>> Members


		/// <summary>
		/// Create an enumerator for the collection of entries of the dictionary
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<KeyValuePair<K,V>> GetEnumerator()
		{
			return pairs.GetEnumerator();;
		}

		#endregion
	}
}
#endif
