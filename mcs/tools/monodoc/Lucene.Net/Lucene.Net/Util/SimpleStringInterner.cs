/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Util
{
	
	
	/// <summary> Simple lockless and memory barrier free String intern cache that is guaranteed
	/// to return the same String instance as String.intern() does.
	/// </summary>
	public class SimpleStringInterner:StringInterner
	{
		
		internal /*private*/ class Entry
		{
			internal /*private*/ System.String str;
			internal /*private*/ int hash;
			internal /*private*/ Entry next;
			internal Entry(System.String str, int hash, Entry next)
			{
				this.str = str;
				this.hash = hash;
				this.next = next;
			}
		}
		
		private Entry[] cache;
		private int maxChainLength;
		
		/// <param name="tableSize"> Size of the hash table, should be a power of two.
		/// </param>
		/// <param name="maxChainLength"> Maximum length of each bucket, after which the oldest item inserted is dropped.
		/// </param>
		public SimpleStringInterner(int tableSize, int maxChainLength)
		{
			cache = new Entry[System.Math.Max(1, BitUtil.NextHighestPowerOfTwo(tableSize))];
			this.maxChainLength = System.Math.Max(2, maxChainLength);
		}
		
		// @Override
		public override System.String Intern(System.String s)
		{
			int h = s.GetHashCode();
			// In the future, it may be worth augmenting the string hash
			// if the lower bits need better distribution.
			int slot = h & (cache.Length - 1);
			
			Entry first = this.cache[slot];
			Entry nextToLast = null;
			
			int chainLength = 0;
			
			for (Entry e = first; e != null; e = e.next)
			{
				if (e.hash == h && ((System.Object) e.str == (System.Object) s || String.CompareOrdinal(e.str, s) == 0))
				{
					// if (e.str == s || (e.hash == h && e.str.compareTo(s)==0)) {
					return e.str;
				}
				
				chainLength++;
				if (e.next != null)
				{
					nextToLast = e;
				}
			}
			
			// insertion-order cache: add new entry at head
			s = String.Intern(s);
			this.cache[slot] = new Entry(s, h, first);
			if (chainLength >= maxChainLength)
			{
				// prune last entry
				nextToLast.next = null;
			}
			return s;
		}
	}
}
