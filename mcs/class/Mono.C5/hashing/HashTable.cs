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

#define LINEARPROBING
#define REFBUCKETnot
#define SHRINKnot
#define INTERHASHERnot
#define RANDOMINTERHASHER
 
using System;
using System.Diagnostics;
using MSG = System.Collections.Generic;

namespace C5
{
	/// <summary>
	/// A set collection class based on linear hashing
	/// </summary>
	public class HashSet<T>: CollectionBase<T>, ICollection<T>
	{
		#region Feature
		/// <summary>
		/// Enum class to assist printing of compilation alternatives.
		/// </summary>
		[Flags]
		public enum Feature: short
		{
			/// <summary>
			/// Nothing
			/// </summary>
			Dummy = 0,
			/// <summary>
			/// Buckets are of reference type
			/// </summary>
			RefTypeBucket = 1,
			/// <summary>
			/// Primary buckets are of value type
			/// </summary>
			ValueTypeBucket = 2,
			/// <summary>
			/// Using linear probing to resolve index clashes
			/// </summary>
			LinearProbing = 4,
			/// <summary>
			/// Shrink table when very sparsely filled
			/// </summary>
			ShrinkTable = 8,
			/// <summary>
			/// Use chaining to resolve index clashes
			/// </summary>
			Chaining = 16,
			/// <summary>
			/// Use hash function on item hash code
			/// </summary>
			InterHasher = 32,
			/// <summary>
			/// Use a universal family of hash functions on item hash code
			/// </summary>
			RandomInterHasher = 64
		}



		static Feature features = Feature.Dummy
#if REFBUCKET
		| Feature.RefTypeBucket
#else
		| Feature.ValueTypeBucket
#endif
#if SHRINK
		| Feature.ShrinkTable
#endif
#if LINEARPROBING
		| Feature.LinearProbing
#else
		| Feature.Chaining
#endif
#if INTERHASHER
		| Feature.InterHasher
#elif RANDOMINTERHASHER
		| Feature.RandomInterHasher
#endif
		;


		/// <summary>
		/// Show which implementation features was chosen at compilation time
		/// </summary>
		public static Feature Features { get { return features; } }

		#endregion

		#region Fields

		int indexmask, bits, bitsc, origbits; //bitsc==32-bits; indexmask==(1<<bits)-1;

		Bucket[] table;

#if !REFBUCKET
		bool defaultvalid = false;

		T defaultitem;
#endif
		double fillfactor = 0.66;

		int resizethreshhold;

#if RANDOMINTERHASHER
#if DEBUG
		uint randomhasher = 1529784659;
#else
		uint randomhasher = (2 * (uint)(new Random()).Next() + 1) * 1529784659;
#endif
#endif

		#endregion

		#region Bucket nested class(es)
#if REFBUCKET
		class Bucket
		{
			internal T item;

			internal int hashval; //Cache!

#if LINEARPROBING
			internal Bucket(T item, int hashval)
			{
				this.item = item;
				this.hashval = hashval;
			}
#else
			internal Bucket overflow;

			internal Bucket(T item, int hashval, Bucket overflow)
			{
				this.item = item;
				this.hashval = hashval;
				this.overflow = overflow;
			}
#endif
		}
#else
		struct Bucket
		{
			internal T item;

			internal int hashval; //Cache!

#if LINEARPROBING
			internal Bucket(T item, int hashval)
			{
				this.item = item;
				this.hashval = hashval;
			}
#else
			internal OverflowBucket overflow;


			internal Bucket(T item, int hashval)
			{
				this.item = item;
				this.hashval = hashval;
				this.overflow = OverflowBuckedefault(T);
			}
#endif
		}


#if !LINEARPROBING
		class OverflowBucket
		{
			internal T item;

			internal int hashval; //Cache!

			internal OverflowBucket overflow;


			internal OverflowBucket(T item, int hashval, OverflowBucket overflow)
			{
				this.item = item;
				this.hashval = hashval;
				this.overflow = overflow;
			}
		}
#endif
#endif

		#endregion

		#region Basic Util

		bool equals(T i1, T i2) { return itemhasher.Equals(i1, i2); }

#if !REFBUCKET
		bool isnull(T item) { return itemhasher.Equals(item, default(T)); }
#endif

		int gethashcode(T item) { return itemhasher.GetHashCode(item); }


		int hv2i(int hashval)
		{
#if INTERHASHER
			//Note: *inverse  mod 2^32 is -1503427877
			return (int)(((uint)hashval * 1529784659) >>bitsc); 
#elif RANDOMINTERHASHER
			return (int)(((uint)hashval * randomhasher) >>bitsc); 
#else
			return indexmask & hashval;
#endif
		}


		void expand()
		{
			//Console.WriteLine(String.Format("Expand to {0} bits", bits+1));
			resize(bits + 1);
		}


		void shrink()
		{
			if (bits > 3)
			{
				//Console.WriteLine(String.Format("Shrink to {0} bits", bits - 1));
				resize(bits - 1);
			}
		}


		void resize(int bits)
		{
			//Console.WriteLine(String.Format("Resize to {0} bits", bits));
			this.bits = bits;
			bitsc = 32 - bits;
			indexmask = (1 << bits) - 1;

			Bucket[] newtable = new Bucket[indexmask + 1];

			for (int i = 0, s = table.Length; i < s; i++)
			{
				Bucket b = table[i];

#if LINEARPROBING
#if REFBUCKET
				if (b != null)
				{
					int j = hv2i(b.hashval);

					while (newtable[j] != null) { j = indexmask & (j + 1); }

					newtable[j] = b;
				}
#else
				if (!isnull(b.item))
				{
					int j = hv2i(b.hashval);

					while (!isnull(newtable[j].item)) { j = indexmask & (j + 1); }

					newtable[j] = b;
				}
#endif
#else
#if REFBUCKET
				while (b != null)
				{
					int j = hv2i(b.hashval);

					newtable[j] = new Bucket(b.item, b.hashval, newtable[j]);
					b = b.overflow;
				}
#else
				if (!isnull(b.item))
				{
					insert(b.item, b.hashval, newtable);

					OverflowBucket ob = b.overflow;

					while (ob != null)
					{
						insert(ob.item, ob.hashval, newtable);
						ob = ob.overflow;
					}
				}
#endif
#endif
			}

			table = newtable;
			resizethreshhold = (int)(table.Length * fillfactor);
			//Console.WriteLine(String.Format("Resize to {0} bits done", bits));
		}

#if REFBUCKET 
#else
#if LINEARPROBING
#else
		//Only for resize!!!
		private void insert(T item, int hashval, Bucket[] t)
		{
			int i = hv2i(hashval);
			Bucket b = t[i];

			if (!isnull(b.item))
			{
				t[i].overflow = new OverflowBucket(item, hashval, b.overflow);
			}
			else
				t[i] = new Bucket(item, hashval);
		}
#endif
#endif

		private bool searchoradd(ref T item, bool add, bool update)
		{
#if LINEARPROBING
#if REFBUCKET
			int hashval = gethashcode(item);
			int i = hv2i(hashval);
			Bucket b = table[i];

			while (b != null)
			{
				if (equals(b.item, item))
				{
					if (update)
						b.item = item;
					else
						item = b.item;

					return true;
				}

				b = table[i = indexmask & (i + 1)];
			}

			if (!add) goto notfound;

			table[i] = new Bucket(item, hashval);
			
#else
			if (isnull(item))
			{
				if (defaultvalid)
				{
					if (update)
						defaultitem = item;
					else
						item = defaultitem;

					return true;
				}

				if (!add) goto notfound;

				defaultvalid = true;
				defaultitem = item;
			}
			else
			{
				int hashval = gethashcode(item);
				int i = hv2i(hashval);
				T t = table[i].item;

				while (!isnull(t))
				{
					if (equals(t, item))
					{
						if (update)
							table[i].item = item;
						else
							item = t;

						return true;
					}

					t = table[i = indexmask & (i + 1)].item;
				}

				if (!add) goto notfound;

				table[i] = new Bucket(item, hashval);
			}
#endif
#else
#if REFBUCKET
			int hashval = gethashcode(item);
			int i = hv2i(hashval);
			Bucket b = table[i], bold = null;

			if (b != null)
			{
				while (b != null)
				{
					if (equals(b.item, item))
					{
						if (update)
							b.item = item;
						else
							item = b.item;

						return true;
					}

					bold = b;
					b = b.overflow;
				}

				if (!add) goto notfound;

				bold.overflow = new Bucket(item, hashval, null);
			}
			else
			{
				if (!add) goto notfound;

				table[i] = new Bucket(item, hashval, null);
			}
#else
			if (isnull(item))
			{
				if (defaultvalid)
				{
					if (update)
						defaultitem = item;
					else
						item = defaultitem;

					return true;
				}

				if (!add) goto notfound;

				defaultvalid = true;
				defaultitem = item;
			}
			else
			{
				int hashval = gethashcode(item);
				int i = hv2i(hashval);
				Bucket b = table[i];

				if (!isnull(b.item))
				{
					if (equals(b.item, item))
					{
						if (update)
							table[i].item = item;
						else
							item = b.item;

						return true;
					}

					OverflowBucket ob = table[i].overflow;

					if (ob == null)
					{
						if (!add) goto notfound;

						table[i].overflow = new OverflowBucket(item, hashval, null);
					}
					else
					{
						while (ob.overflow != null)
						{
							if (equals(item, ob.item))
							{
								if (update)
									ob.item = item;
								else
									item = ob.item;

								return true;
							}

							ob = ob.overflow;
						}

						if (equals(item, ob.item))
						{
							if (update)
								ob.item = item;
							else
								item = ob.item;

							return true;
						}

						if (!add) goto notfound;

						ob.overflow = new OverflowBucket(item, hashval, null);
					}
				}
				else
				{
					if (!add) goto notfound;

					table[i] = new Bucket(item, hashval);
				}
			}
#endif
#endif
			size++;
			if (size > resizethreshhold)
				expand();
		notfound :

			return false;
		}


		private bool remove(ref T item)
		{
			if (size == 0)
				return false;
#if LINEARPROBING
#if REFBUCKET
			int hashval = gethashcode(item);
			int index = hv2i(hashval);
			Bucket b = table[index];

			while (b != null)
			{
				if (equals(item, b.item))
				{
					//ref
					item = table[index];
					table[index] = null;

					//Algorithm R
					int j = (index + 1) & indexmask;

					b = table[j];
					while (b != null)
					{
						int k = hv2i(b.hashval);

						if ((k <= index && index < j) || (index < j && j < k) || (j < k && k <= index))
						//if (index > j ? (j < k && k <= index): (k <= index || j < k) )
						{
							table[index] = b;
							table[j] = null;
							index = j;
						}

						j = (j + 1) & indexmask;
						b = table[j];
					}

					goto found;
				}

				b = table[index = indexmask & (index + 1)];
			}
			return false;
#else
			if (isnull(item))
			{
				if (!defaultvalid)
					return false;

				//ref
				item = defaultitem;
				defaultvalid = false;
				defaultitem = default(T); //No spaceleaks!
			}
			else
			{
				int hashval = gethashcode(item);
				int index = hv2i(hashval);
				T t = table[index].item;

				while (!isnull(t))
				{
					if (equals(item, t))
					{
						//ref
						item = table[index].item;
						table[index].item = default(T);

						//algorithm R
						int j = (index + 1) & indexmask;
						Bucket b = table[j];

						while (!isnull(b.item))
						{
							int k = hv2i(b.hashval);

							if ((k <= index && index < j) || (index < j && j < k) || (j < k && k <= index))
							{
								table[index] = b;
								table[j].item = default(T);
								index = j;
							}

							j = (j + 1) & indexmask;
							b = table[j];
						}

						goto found;
					}

					t = table[index = indexmask & (index + 1)].item;
				}

				return false;
			}
#endif
			found :
#else
#if REFBUCKET
			int hashval = gethashcode(item);
			int index = hv2i(hashval);
			Bucket b = table[index], bold;

			if (b == null)
				return false;

			if (equals(item, b.item)) {
				//ref
				item = b.item;
				table[index] = b.overflow;
				}
			else
			{
				bold = b;
				b = b.overflow;
				while (b != null && !equals(item, b.item))
				{
					bold = b;
					b = b.overflow;
				}

				if (b == null)
					return false;

				//ref
				item = b.item;
				bold.overflow = b.overflow;
			}
			
#else
			if (isnull(item))
			{
				if (!defaultvalid)
					return false;

				//ref
				item = defaultitem;
				defaultvalid = false;
				defaultitem = default(T); //No spaceleaks!
			}
			else
			{
				int hashval = gethashcode(item);
				int index = hv2i(hashval);
				Bucket b = table[index];
				OverflowBucket ob = b.overflow;

				if (equals(item, b.item))
				{
					//ref
					item = b.item;
					if (ob == null)
					{
						table[index] = new Bucket();
					}
					else
					{
						b = new Bucket(ob.item, ob.hashval);
						b.overflow = ob.overflow;
						table[index] = b;
					}
				}
				else
				{
					if (ob == null)
						return false;

					if (equals(item, ob.item)) 
					{
						//ref
						item=ob.item;
						table[index].overflow = ob.overflow;
					}
					else
					{
						while (ob.overflow != null)
							if (equals(item, ob.overflow.item))
							{
								//ref
								item = ob.overflow.item;
								break;
							}
							else
								ob = ob.overflow;

						if (ob.overflow == null)
							return false;

						ob.overflow = ob.overflow.overflow;
					}
				}
			}
#endif
#endif
			size--;

			return true;
		}


		private void clear()
		{
			bits = origbits;
			bitsc = 32 - bits;
			indexmask = (1 << bits) - 1;
			size = 0;
			table = new Bucket[indexmask + 1];
			resizethreshhold = (int)(table.Length * fillfactor);
#if !REFBUCKET
			defaultitem = default(T);
			defaultvalid = false;
#endif
		}

		#endregion

		#region Constructors
		/// <summary>
		/// Create a hash set with natural item hasher and default fill threshold (66%)
		/// and initial table size (16).
		/// </summary>
		public HashSet() 
			: this(HasherBuilder.ByPrototype<T>.Examine()) { }


		/// <summary>
		/// Create a hash set with external item hasher and default fill threshold (66%)
		/// and initial table size (16).
		/// </summary>
		/// <param name="itemhasher">The external item hasher</param>
		public HashSet(IHasher<T> itemhasher) 
			: this(16, itemhasher) { }


		/// <summary>
		/// Create a hash set with external item hasher and default fill threshold (66%)
		/// </summary>
		/// <param name="capacity">Initial table size (rounded to power of 2, at least 16)</param>
		/// <param name="itemhasher">The external item hasher</param>
		public HashSet(int capacity, IHasher<T> itemhasher) 
			: this(capacity, 0.66, itemhasher) { }


		/// <summary>
		/// Create a hash set with external item hasher.
		/// </summary>
		/// <param name="capacity">Initial table size (rounded to power of 2, at least 16)</param>
		/// <param name="fill">Fill threshold (in range 10% to 90%)</param>
		/// <param name="itemhasher">The external item hasher</param>
		public HashSet(int capacity, double fill, IHasher<T> itemhasher)
		{
			if (fill < 0.1 || fill > 0.9)
				throw new ArgumentException("Fill outside valid range [0.1, 0.9]");

			if (capacity <= 0)
				throw new ArgumentException("Non-negative capacity ");

			this.itemhasher = itemhasher;
			origbits = 4;
			while (capacity - 1 >>origbits > 0) origbits++;

			clear();
		}



		#endregion

		#region IEditableCollection<T> Members

		/// <summary>
		/// The complexity of the Contains operation
		/// </summary>
		/// <value>Always returns Speed.Constant</value>
		[Tested]
		public virtual Speed ContainsSpeed { [Tested]get { return Speed.Constant; } }


		[Tested]
		int ICollection<T>.GetHashCode()
		{ return unsequencedhashcode(); }


		[Tested]
		bool ICollection<T>.Equals(ICollection<T> that)
		{ return unsequencedequals(that); }


		/// <summary>
		/// Check if an item is in the set 
		/// </summary>
		/// <param name="item">The item to look for</param>
		/// <returns>True if set contains item</returns>
		[Tested]
		public virtual bool Contains(T item) { return searchoradd(ref item, false, false); }


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the set and
		/// if so report the actual item object found.
		/// </summary>
		/// <param name="item">On entry, the item to look for.
		/// On exit the item found, if any</param>
		/// <returns>True if set contains item</returns>
		[Tested]
		public virtual bool Find(ref T item) { return searchoradd(ref item, false, false); }


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the set and
		/// if so replace the item object in the set with the supplied one.
		/// </summary>
		/// <param name="item">The item object to update with</param>
		/// <returns>True if item was found (and updated)</returns>
		[Tested]
		public virtual bool Update(T item)
		{ updatecheck(); return searchoradd(ref item, false, true); }


		/// <summary>
		/// Check if an item (collection equal to a given one) is in the set.
		/// If found, report the actual item object in the set,
		/// else add the supplied one.
		/// </summary>
		/// <param name="item">On entry, the item to look for or add.
		/// On exit the actual object found, if any.</param>
		/// <returns>True if item was found</returns>
		[Tested]
		public virtual bool FindOrAdd(ref T item)
		{ updatecheck(); return searchoradd(ref item, true, false); }


		/// <summary>
		/// Check if an item (collection equal to a supplied one) is in the set and
		/// if so replace the item object in the set with the supplied one; else
		/// add the supplied one.
		/// </summary>
		/// <param name="item">The item to look for and update or add</param>
		/// <returns>True if item was updated</returns>
		[Tested]
		public virtual bool UpdateOrAdd(T item)
		{ updatecheck(); return searchoradd(ref item, true, true); }


		/// <summary>
		/// Remove an item from the set
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>True if item was (found and) removed </returns>
		[Tested]
		public virtual bool Remove(T item)
		{
			updatecheck();
			if (remove(ref item))
			{
#if SHRINK
				if (size<resizethreshhold/2 && resizethreshhold>8)
					shrink();
#endif
				return true;
			}
			else
				return false;
		}


		/// <summary>
		/// Remove an item from the set, reporting the actual matching item object.
		/// </summary>
		/// <param name="item">On entry the item to remove.
		/// On exit, the actual removed item object.</param>
		/// <returns>True if item was found.</returns>
		[Tested]
		public virtual bool RemoveWithReturn(ref T item)
		{
			updatecheck();
			if (remove(ref item))
			{
#if SHRINK
				if (size<resizethreshhold/2 && resizethreshhold>8)
					shrink();
#endif
				return true;
			}
			else
				return false;
		}


		/// <summary>
		/// Remove all items in a supplied collection from this set.
		/// </summary>
		/// <param name="items">The items to remove.</param>
		[Tested]
		public virtual void RemoveAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			T jtem;

			foreach (T item in items)
			{ jtem = item; remove(ref jtem); }
#if SHRINK
			if (size < resizethreshhold / 2 && resizethreshhold > 16)
			{
				int newlength = table.Length;

				while (newlength >= 32 && newlength * fillfactor / 2 > size)
					newlength /= 2;

				resize(newlength - 1);
			}
#endif
		}


		/// <summary>
		/// Remove all items from the set, resetting internal table to initial size.
		/// </summary>
		[Tested]
		public virtual void Clear()
		{
			updatecheck();
			clear();
		}


		/// <summary>
		/// Remove all items *not* in a supplied collection from this set.
		/// </summary>
		/// <param name="items">The items to retain</param>
		[Tested]
		public virtual void RetainAll(MSG.IEnumerable<T> items)
		{
			updatecheck();

			HashSet<T> t = (HashSet<T>)MemberwiseClone();

			t.Clear();

			//This only works for sets:
			foreach (T item in items)
				if (Contains(item))
				{
					T jtem = item;

					t.searchoradd(ref jtem, true, false);
				}

			table = t.table;
			size = t.size;
#if !REFBUCKET
			defaultvalid = t.defaultvalid;
			defaultitem = t.defaultitem;
#endif
			indexmask = t.indexmask;
			resizethreshhold = t.resizethreshhold;
		}


		/// <summary>
		/// Check if all items in a supplied collection is in this set
		/// (ignoring multiplicities). 
		/// </summary>
		/// <param name="items">The items to look for.</param>
		/// <returns>True if all items are found.</returns>
		[Tested]
		public virtual bool ContainsAll(MSG.IEnumerable<T> items)
		{
			foreach (T item in items)
				if (!Contains(item))
					return false;

			return true;
		}


		/// <summary>
		/// Create an array containing all items in this set (in enumeration order).
		/// </summary>
		/// <returns>The array</returns>
		[Tested]
		public override T[] ToArray()
		{
			T[] res = new T[size];
			int index = 0;

#if !REFBUCKET
			if (defaultvalid)
				res[index++] = defaultitem;
#endif
			for (int i = 0; i < table.Length; i++)
			{
				Bucket b = table[i];
#if LINEARPROBING
#if REFBUCKET
				if (b != null)
					res[index++] = b.item;
#else
				if (!isnull(b.item))
					res[index++] = b.item;
#endif
#else
#if REFBUCKET
				while (b != null)
				{
					res[index++] = b.item;
					b = b.overflow;
				}
#else
				if (!isnull(b.item))
				{
					res[index++] = b.item;

					OverflowBucket ob = b.overflow;

					while (ob != null)
					{
						res[index++] = ob.item;
						ob = ob.overflow;
					}
				}
#endif
#endif
			}

			Debug.Assert(size == index);
			return res;
		}


		/// <summary>
		/// Count the number of times an item is in this set (either 0 or 1).
		/// </summary>
		/// <param name="item">The item to look for.</param>
		/// <returns>1 if item is in set, 0 else</returns>
		[Tested]
		public virtual int ContainsCount(T item) { return Contains(item) ? 1 : 0; }


		/// <summary>
		/// Remove all (at most 1) copies of item from this set.
		/// </summary>
		/// <param name="item">The item to remove</param>
		[Tested]
		public virtual void RemoveAllCopies(T item) { Remove(item); }

		#endregion

		#region IEnumerable<T> Members

		/// <summary>
		/// Create an enumerator for this set.
		/// </summary>
		/// <returns>The enumerator</returns>
		[Tested]
		public override MSG.IEnumerator<T> GetEnumerator()
		{
			int index = -1;
			int mystamp = stamp;
			int len = table.Length;

#if LINEARPROBING
#if REFBUCKET
			while (++index < len)
			{
				if (mystamp != stamp) throw new InvalidOperationException();

				if (table[index] != null) yield table[index].item;
			}
#else
			if (defaultvalid)
				yield return defaultitem;

			while (++index < len)
			{
				if (mystamp != stamp) throw new InvalidOperationException();

				T item = table[index].item;

				if (!isnull(item)) yield return item;
			}
#endif
#else
#if REFBUCKET
			Bucket b = null;
#else
			OverflowBucket ob = null;

			if (defaultvalid)
				yield defaultitem;
#endif
			while (true)
			{
				if (mystamp != stamp)
					throw new InvalidOperationException();

#if REFBUCKET
				if (b == null || b.overflow == null)
				{
					do
					{
						if (++index >= len) yield break;
					} while (table[index] == null);

					b = table[index];
					yield b.item;
				}
				else
				{
					b = b.overflow;
					yield b.item;
				}
#else
				if (ob != null && ob.overflow != null)
				{
					ob = ob.overflow;
					yield ob.item;
				}
				else if (index >= 0 && ob == null && (ob = table[index].overflow) != null)
				{
					yield ob.item;
				}
				else
				{
					do
					{
						if (++index >= len) yield break;
					} while (isnull(table[index].item));

					yield table[index].item;
					ob = null;
				}
#endif
			}
#endif
		}

		#endregion

		#region ISink<T> Members
		/// <summary>
		/// Report if this is a set collection.
		/// </summary>
		/// <value>Always false</value>
		[Tested]
		public virtual bool AllowsDuplicates { [Tested]get { return false; } }


		/// <summary>
		/// Add an item to this set.
		/// </summary>
		/// <param name="item">The item to add.</param>
		/// <returns>True if item was added (i.e. not found)</returns>
		[Tested]
		public virtual bool Add(T item)
		{
			updatecheck();
			return !searchoradd(ref item, true, false);
		}


		/// <summary>
		/// Add all items of a collection to this set.
		/// </summary>
		/// <param name="items">The items to add</param>
		[Tested]
		public virtual void AddAll(MSG.IEnumerable<T> items)
		{
			updatecheck();
			foreach (T item in items)
			{
				T jtem = item;

				searchoradd(ref jtem, true, false);
			}
		}

        /// <summary>
        /// Add the elements from another collection with a more specialized item type 
        /// to this collection. Since this
        /// collection has set semantics, only items not already in the collection
        /// will be added.
        /// </summary>
        /// <typeparam name="U">The type of items to add</typeparam>
        /// <param name="items">The items to add</param>
        public virtual void AddAll<U>(MSG.IEnumerable<U> items) where U : T
        {
            updatecheck();
            foreach (T item in items)
            {
                T jtem = item;

                searchoradd(ref jtem, true, false);
            }
        }


		#endregion

		#region Diagnostics

		/// <summary>
		/// Test internal structure of data (invariants)
		/// </summary>
		/// <returns>True if pass</returns>
		[Tested]
		public virtual bool Check()
		{
			int count = 0;
#if LINEARPROBING
			int lasthole = table.Length - 1;

#if REFBUCKET
			while (lasthole >= 0 && table[lasthole] != null)
#else
			while (lasthole >= 0 && !isnull(table[lasthole].item))
#endif
			{
				lasthole--;
				count++;
			}

			if (lasthole < 0)
			{
				Console.WriteLine("Table is completely filled!");
				return false;
			}

			for (int cellindex = lasthole + 1, s = table.Length; cellindex < s; cellindex++)
			{
				Bucket b = table[cellindex];
				int hashindex = hv2i(b.hashval);

				if (hashindex <= lasthole || hashindex > cellindex)
				{
					Console.WriteLine("Bad cell item={0}, hashval={1}, hashindex={2}, cellindex={3}, lasthole={4}", b.item, b.hashval, hashindex, cellindex, lasthole);
					return false;
				}
			}

			int latesthole = -1;

			for (int cellindex = 0; cellindex < lasthole; cellindex++)
			{
				Bucket b = table[cellindex];

#if REFBUCKET
				if (b != null)
#else
				if (!isnull(b.item))
#endif
				{
					count++;

					int hashindex = hv2i(b.hashval);

					if (cellindex < hashindex && hashindex <= lasthole)
					{
						Console.WriteLine("Bad cell item={0}, hashval={1}, hashindex={2}, cellindex={3}, latesthole={4}", b.item, b.hashval, hashindex, cellindex, latesthole);
						return false;
					}
				}
				else
				{
					latesthole = cellindex;
					break;
				}
			}

			for (int cellindex = latesthole + 1; cellindex < lasthole; cellindex++)
			{
				Bucket b = table[cellindex];

#if REFBUCKET
				if (b != null)
#else
				if (!isnull(b.item))
#endif
				{
					count++;

					int hashindex = hv2i(b.hashval);

					if (hashindex <= latesthole || cellindex < hashindex)
					{
						Console.WriteLine("Bad cell item={0}, hashval={1}, hashindex={2}, cellindex={3}, latesthole={4}", b.item, b.hashval, hashindex, cellindex, latesthole);
						return false;
					}
				}
				else
				{
					latesthole = cellindex;
				}
			}

			return true;
#else
			bool retval = true;
			for (int i = 0, s = table.Length; i < s; i++)
			{
				int level = 0;
				Bucket b = table[i];
#if REFBUCKET
				while (b != null)
				{
					if (i != hv2i(b.hashval))
					{
						Console.WriteLine("Bad cell item={0}, hashval={1}, index={2}, level={3}", b.item, b.hashval, i, level);
						retval = false;
					}

					count++;
					level++;
					b = b.overflow;
				}
#else
				if (!isnull(b.item))
				{
					count++;
					if (i != hv2i(b.hashval))
					{
						Console.WriteLine("Bad cell item={0}, hashval={1}, index={2}, level={3}", b.item, b.hashval, i, level);
						retval = false;
					}

					OverflowBucket ob = b.overflow;

					while (ob != null)
					{
						level++;
						count++;
						if (i != hv2i(ob.hashval))
						{
							Console.WriteLine("Bad cell item={0}, hashval={1}, index={2}, level={3}", b.item, b.hashval, i, level);
							retval = false;
						}

						ob = ob.overflow;
					}
				}
#endif
			}

			if (count != size)
			{
				Console.WriteLine("size({0}) != count({1})", size, count);
				retval = false;
			}

			return retval;
#endif
		}


		/// <summary>
		/// Produce statistics on distribution of bucket sizes. Current implementation is incomplete.
		/// </summary>
		/// <returns>Histogram data.</returns>
		[Tested(via = "Manually")]
		public ISortedDictionary<int,int> BucketSizeDistribution()
		{
			TreeDictionary<int,int> res = new TreeDictionary<int,int>(new IC());
#if LINEARPROBING
			return res;
#else
			for (int i = 0, s = table.Length; i < s; i++)
			{
				int count = 0;
#if REFBUCKET
				Bucket b = table[i];

				while (b != null)
				{
					count++;
					b = b.overflow;
				}
#else
				Bucket b = table[i];

				if (!isnull(b.item))
				{
					count = 1;

					OverflowBucket ob = b.overflow;

					while (ob != null)
					{
						count++;
						ob = ob.overflow;
					}
				}
#endif
				if (res.Contains(count))
					res[count]++;
				else
					res[count] = 1;
			}

			return res;
#endif
		}

		#endregion
	}
}
#endif
