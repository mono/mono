#if NET_4_0
// ConcurrentSkipList.cs
//
// Copyright (c) 2009 Jérémie "Garuma" Laval
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace System.Threading.Collections
{
	public class ConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		class Pair
		{
			public readonly TKey Key;
			public TValue Value;
			
			public Pair(TKey key, TValue value)
			{
				Key = key;
				Value = value;
			}
			
			public override bool Equals (object obj)
			{
				Pair rhs = obj as Pair;
				return rhs == null ? false : Key.Equals(rhs.Key) && Value.Equals(rhs.Value);
			}
			
			public override int GetHashCode ()
			{
				return Key.GetHashCode();
			}
		}
		
		class Basket: List<Pair>
		{
		}
		
		// Assumption: a List<T> is never empty
		ConcurrentSkipList<Basket> container
			= new ConcurrentSkipList<Basket>((value) => value[0].GetHashCode());
		int count;
		int stamp = int.MinValue;
		
		public ConcurrentDictionary()
		{
		}
		
		public void Add(TKey key, TValue value)
		{
			Basket basket;
			// Add a value to an existing basket
			if (TryGetBasket(key, out basket)) {
				// Find a maybe more sexy locking scheme later
				lock (basket) {
					foreach (var p in basket) {
						if (p.Key.Equals(key))
							throw new ArgumentException("An element with the same key already exists");
					}
					basket.Add(new Pair(key, value));
				}
			} else {
				// Add a new basket
				basket = new Basket();
				basket.Add(new Pair(key, value));
				container.Add(basket);
			}
			Interlocked.Increment(ref count);
			Interlocked.Increment(ref stamp);
		}
		
		void ICollection<KeyValuePair<TKey,TValue>>.Add(KeyValuePair<TKey, TValue> pair)
		{
			Add(pair.Key, pair.Value);
		}
		
		public TValue GetValue(TKey key)
		{
			TValue temp;
			if (!TryGetValue(key, out temp))
				// TODO: find a correct Exception
				throw new ArgumentOutOfRangeException("key");
			return temp;
		}
		
		public bool TryGetValue(TKey key, out TValue value)
		{
			Basket basket;
			value = default(TValue);
			
			if (!TryGetBasket(key, out basket))
				return false;
			
			lock (basket) {
				Pair pair = basket.Find((p) => p.Key.Equals(key));
				if (pair == null)
					return false;
				value = pair.Value;
			}
			
			return true;
		}
		
		public TValue this[TKey key] {
			get {
				return GetValue(key);
			}
			set {
				Basket basket;
				if (!TryGetBasket(key, out basket)) {
					Add(key, value);
					return;
				}
				lock (basket) {
					Pair pair = basket.Find((p) => p.Key.Equals(key));
					if (pair == null)
						throw new InvalidOperationException("pair is null, shouldn't be");
					pair.Value = value;
					Interlocked.Increment(ref stamp);
				}
			}
		}
		
		public bool Remove(TKey key)
		{
			Basket b;
			if (!TryGetBasket(key, out b))
				return false;
			
			lock (b) {
				// Should always be == 1 but who know
				return b.RemoveAll((p) => p.Key.Equals(key)) >= 1;
			}
		}
		
		bool ICollection<KeyValuePair<TKey,TValue>>.Remove(KeyValuePair<TKey,TValue> pair)
		{
			return Remove(pair.Key);
		}
		
		public bool ContainsKey(TKey key)
		{
			return container.ContainsFromHash(key.GetHashCode());
		}
		
		bool ICollection<KeyValuePair<TKey,TValue>>.Contains(KeyValuePair<TKey, TValue> pair)
		{
			return ContainsKey(pair.Key);
		}
	
		public void Clear()
		{
			// Pronk
			container = new ConcurrentSkipList<Basket>((value) => value[0].GetHashCode());
		}
		
		public int Count {
			get {
				return count;
			}
		}
		
		public bool IsReadOnly {
			get {
				return false;
			}
		}
		
		public ICollection<TKey> Keys {
			get {
				return null;
			}
		}
		
		public ICollection<TValue> Values {
			get {
				return null;
			}
		}
		
		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return GetEnumeratorInternal();
		}
		
		IEnumerator IEnumerable.GetEnumerator()
		{
			return (IEnumerator)GetEnumeratorInternal();
		}
		
		IEnumerator<KeyValuePair<TKey, TValue>> GetEnumeratorInternal()
		{	
			foreach (Basket b in container) {
				lock (b) {
					foreach (Pair p in b)
						yield return new KeyValuePair<TKey, TValue>(p.Key, p.Value);
				}
			}
		}
		
		public void CopyTo(KeyValuePair<TKey,TValue>[] array, int startIndex)
		{
			
		}
		
		/*void UpdateKeysValuesColl()
		{
			foreach (Pair p in this) {
				// Read from time to time to see if we became useless
				if (currStamp != tempStamp)
					break;
				keys.Add(p.Key);
				values.Add(p.Value);
			}
		}*/
		
		bool TryGetBasket(TKey key, out Basket basket)
		{
			basket = null;
			if (!container.GetFromHash(key.GetHashCode(), out basket))
				return false;
			
			return true;
		}
	}
}
#endif
