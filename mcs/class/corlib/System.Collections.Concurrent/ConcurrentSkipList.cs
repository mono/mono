#if NET_4_0
// ConcurrentSkipList.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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

namespace System.Collections.Concurrent
{
	
	internal class ConcurrentSkipList<T> : IProducerConsumerCollection<T>
	{
		// Used for randomSeed
		[ThreadStatic]
		static Random r;
		// Used in FindNodes and thus most others methods
		// avoid heavy local array creation at each method call and use 
		// for thread locallity ThreadStatic attribute
		[ThreadStaticAttribute]
		static Node[] preds;
		[ThreadStaticAttribute]
		static Node[] succs;

		int count = 0;
		
		class Node
		{
			public readonly int      Key;
			public T                 Value;
			public readonly int      TopLayer;
			public readonly Node[]   Nexts;
			public volatile bool     Marked;
			public volatile bool     FullyLinked;
			public readonly SpinLock SpinLock;

			public Node (int key, T value, int heightValue)
			{
				Key = key;
				Value = value;
				TopLayer = heightValue;
				Nexts = new Node [heightValue + 1];
				SpinLock = new SpinLock (false);
				Marked = FullyLinked = false;
			}
		}

		Node leftSentinel;
		Node rightSentinel;

		const int MaxHeight = 200;
		uint randomSeed;

		Func<T, int> GetKey;

		public ConcurrentSkipList () : this ((value) => value.GetHashCode ())
		{
		}

		public ConcurrentSkipList (IEqualityComparer<T> comparer)
			: this ((value) => comparer.GetHashCode (value))
		{
		}
		
		public ConcurrentSkipList(Func<T, int> hasher)
		{
			GetKey = hasher;
			Init ();
		}

		void Init ()
		{
			if (succs == null)
				succs = new Node [MaxHeight];
			if (preds == null)
				preds = new Node [MaxHeight];
			
			leftSentinel = new Node (int.MinValue, default (T), MaxHeight);
			rightSentinel = new Node (int.MaxValue, default (T), MaxHeight);

			for (int i = 0; i < MaxHeight; i++) {
				leftSentinel.Nexts [i] = rightSentinel;
			}
			// The or ensures that randomSeed != 0
			randomSeed = ((uint)Math.Abs (Next())) | 0x0100;
		}
		
		public bool TryAdd (T value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			CleanArrays ();
			int topLayer = GetRandomLevel ();

			int v = GetKey (value);

			while (true) {
				int found = FindNode (v, preds, succs);
				if (found != -1) {
					// A node with the same key already exists
					Node nodeFound = succs [found];
					if (!nodeFound.Marked) {
						SpinWait sw = new SpinWait ();
						while (!nodeFound.FullyLinked) {
							sw.SpinOnce ();
						}
						return false;
					}
					continue;
				}
				int highestLocked = -1;
				try {
					bool valid = LockNodes (topLayer, ref highestLocked,
					                        (layer, pred, succ) => !pred.Marked && !succ.Marked && pred.Nexts [layer] == succ);
					if (!valid)
						continue;
						
					Node newNode = new Node (v, value, topLayer);
					for (int layer = 0; layer <= topLayer; layer++) {
						newNode.Nexts [layer] = succs [layer];
						preds [layer].Nexts [layer] = newNode;
					}
					newNode.FullyLinked = true;
				} finally {
					Unlock (preds, highestLocked);
				}
				Interlocked.Increment (ref count);
				return true;
			}
		}

		bool IProducerConsumerCollection<T>.TryTake (out T value)
		{
			throw new NotSupportedException ();
		}

		public T[] ToArray ()
		{
			int countSnapshot = count;
			T[] temp = new T [countSnapshot];
			
			CopyTo(temp, 0);

			return temp;
		}

		public void CopyTo (T[] array, int startIndex)
		{
			IEnumerator<T> e = GetInternalEnumerator ();
			for (int i = startIndex; i < array.Length; i++) {
				if (!e.MoveNext ())
					return;
				array [i] = e.Current;
			}
			e.Dispose ();
		}

		void ICollection.CopyTo (Array array, int startIndex)
		{
			T[] temp = array as T[];
			if (temp == null)
				return;

			CopyTo (temp, startIndex);
		}

		object ICollection.SyncRoot {
			get {
				return this;
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return true;
			}
		}

		public bool Remove (T value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			CleanArrays();
			Node toDelete = null;
			bool isMarked = false;
			int topLayer = -1;
			int v = GetKey (value);

			while (true) {
				int found = FindNode (v, preds, succs);
				
				if (isMarked || (found != -1 && OkToDelete (succs [found], found))) {
					// If not marked then logically delete the node
					if (!isMarked) {
						toDelete = succs [found];
						topLayer = toDelete.TopLayer;
						bool taken = false;
						do {
							toDelete.SpinLock.Enter (ref taken);
						} while (!taken);
						// Now that we have the lock, check if the node hasn't already been marked
						if (toDelete.Marked) {
							toDelete.SpinLock.Exit (true);
							return false;
						}
						toDelete.Marked = true;
						isMarked = true;
					}
					int highestLocked = -1;
					try {
						bool valid = LockNodes (topLayer, ref highestLocked,
						                        (layer, pred, succ) => !pred.Marked && pred.Nexts [layer] == succ);
						if (!valid)
							continue;

						for (int layer = topLayer; layer >= 0; layer--) {
							preds [layer].Nexts [layer] = toDelete.Nexts [layer];
						}
						toDelete.SpinLock.Exit (true);
					} finally {
						Unlock (preds, highestLocked);
					}
					Interlocked.Decrement (ref count);
					return true;
				} else {
					return false;
				}
			}
		}

		public bool Contains (T value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			
			return ContainsFromHash (GetKey (value));
		}
		
		internal bool ContainsFromHash (int hash)
		{
			CleanArrays ();
			int found = FindNode (hash, preds, succs);
			return found != -1 && succs [found].FullyLinked && !succs [found].Marked;
		}
		
		internal bool GetFromHash (int hash, out T value)
		{
			value = default (T);
			CleanArrays ();
			// We are blindly supposing that the hash is correct
			// i.e. I trust myself :-)
			int found = FindNode (hash, preds, succs);
			if (found == -1)
				return false;
			
			try {
				bool taken = false;
				do {
					succs [found].SpinLock.Enter (ref taken);
				} while (!taken);
				Node node = succs [found];
				if (node.FullyLinked && !node.Marked) {
					value = node.Value;
					return true;
				}
			} finally {
				succs [found].SpinLock.Exit (true);
			}
			
			return false;
		}

		public int Count {
			get {
				return count;
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator ()
		{
			return GetInternalEnumerator ();
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetInternalEnumerator ();
		}
		
		IEnumerator<T> GetInternalEnumerator ()
		{
			Node curr = leftSentinel;
			while ((curr = curr.Nexts [0]) != rightSentinel) {
				// If there is an Add operation ongoing we wait a little
				// Possible optimization : use a helping scheme
				SpinWait sw = new SpinWait ();
				while (!curr.FullyLinked) {
					sw.SpinOnce ();
				}
				yield return curr.Value;
			}
		}

		void Unlock(Node[] preds, int highestLocked)
		{
			for (int i = 0; i <= highestLocked; i++) {
				preds [i].SpinLock.Exit (true);
			}
		}

		bool LockNodes (int topLayer, ref int highestLocked, Func<int, Node, Node, bool> validityTest)
		{
			Node pred, succ, prevPred = null;
			bool valid = true;
			
			for (int layer = 0; valid && (layer <= topLayer); layer++) {
				pred = preds [layer];
				succ = succs [layer];
				if (pred != prevPred) {
					// Possible optimization : limit topLayer to the first refused lock
					bool taken = false;
					do {
						pred.SpinLock.Enter (ref taken);
					} while (!taken);
					highestLocked = layer;
					prevPred = pred;
				}
				valid = validityTest (layer, pred, succ);
			}

			return valid;
		}

		int FindNode (int v, Node[] preds, Node[] succs)
		{
			// With preds and succs we record the path we use for searching v
			if (preds.Length != MaxHeight || succs.Length != MaxHeight)
				throw new Exception ("precs or succs don't have the  good length");

			int found = -1;
			Node pred = leftSentinel;

			// We start at the higher layer
			for (int layer = MaxHeight - 1; layer >= 0; layer--) {
				Node curr = pred.Nexts [layer];
				// In the current layer we find the best position, then the operation will continue on the
				// layer just beneath
				while (v > curr.Key) {
					pred = curr;
					curr = curr.Nexts [layer];
				}
				if (found == -1 && v == curr.Key)
					found = layer;
				preds [layer] = pred;
				succs [layer] = curr;
			}
			
			return found;
		}

		bool OkToDelete (Node candidate, int found)
		{
			return candidate.FullyLinked && candidate.TopLayer == found && !candidate.Marked;
		}

		// Taken from Doug Lea's code released in the public domain
		int GetRandomLevel ()
		{
			uint x = randomSeed;
			x ^= x << 13;
			x ^= x >> 17;
			x ^= x << 5;
			randomSeed = x;
			if ((x & 0x80000001) != 0) // test highest and lowest bits
				return 0;
			int level = 1;
			while (((x >>= 1) & 1) != 0) ++level;
			return level;
		}
		
		void CleanArrays ()
		{
			if (succs == null)
				succs = new Node [MaxHeight];
			if (preds == null)
				preds = new Node [MaxHeight];
			
			// Hopefully these are more optimized than a bare for loop
			// (I suppose it uses memset internally)
			Array.Clear (preds, 0, preds.Length);
			Array.Clear (succs, 0, succs.Length);
		}

		int Next ()
		{
		  if (r == null)
			r = new Random ();

		  return r.Next ();
		}
	}
}
#endif
