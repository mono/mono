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
using System.Collections.Generic;

using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using FieldCache = Mono.Lucene.Net.Search.FieldCache;
using CacheEntry = Mono.Lucene.Net.Search.CacheEntry;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Provides methods for sanity checking that entries in the FieldCache 
	/// are not wasteful or inconsistent.
	/// <p/>
	/// <p/>
	/// Lucene 2.9 Introduced numerous enhancements into how the FieldCache 
	/// is used by the low levels of Lucene searching (for Sorting and 
	/// ValueSourceQueries) to improve both the speed for Sorting, as well 
	/// as reopening of IndexReaders.  But these changes have shifted the 
	/// usage of FieldCache from "top level" IndexReaders (frequently a 
	/// MultiReader or DirectoryReader) down to the leaf level SegmentReaders.  
	/// As a result, existing applications that directly access the FieldCache 
	/// may find RAM usage increase significantly when upgrading to 2.9 or 
	/// Later.  This class provides an API for these applications (or their 
	/// Unit tests) to check at run time if the FieldCache contains "insane" 
	/// usages of the FieldCache.
	/// <p/>
	/// <p/>
	/// <b>EXPERIMENTAL API:</b> This API is considered extremely advanced and 
	/// experimental.  It may be removed or altered w/o warning in future releases 
	/// of Lucene.
	/// <p/>
	/// </summary>
	/// <seealso cref="FieldCache">
	/// </seealso>
	/// <seealso cref="FieldCacheSanityChecker.Insanity">
	/// </seealso>
	/// <seealso cref="FieldCacheSanityChecker.InsanityType">
	/// </seealso>
	public sealed class FieldCacheSanityChecker
	{
		
		private RamUsageEstimator ramCalc = null;
		public FieldCacheSanityChecker()
		{
			/* NOOP */
		}
		/// <summary> If set, will be used to estimate size for all CacheEntry objects 
		/// dealt with.
		/// </summary>
		public void  SetRamUsageEstimator(RamUsageEstimator r)
		{
			ramCalc = r;
		}
		
		
		/// <summary> Quick and dirty convenience method</summary>
		/// <seealso cref="check">
		/// </seealso>
		public static Insanity[] CheckSanity(FieldCache cache)
		{
			return CheckSanity(cache.GetCacheEntries());
		}
		
		/// <summary> Quick and dirty convenience method that instantiates an instance with 
		/// "good defaults" and uses it to test the CacheEntry[]
		/// </summary>
		/// <seealso cref="check">
		/// </seealso>
		public static Insanity[] CheckSanity(CacheEntry[] cacheEntries)
		{
			FieldCacheSanityChecker sanityChecker = new FieldCacheSanityChecker();
			// doesn't check for interned
			sanityChecker.SetRamUsageEstimator(new RamUsageEstimator(false));
			return sanityChecker.Check(cacheEntries);
		}
		
		
		/// <summary> Tests a CacheEntry[] for indication of "insane" cache usage.
		/// <p/>
		/// NOTE:FieldCache CreationPlaceholder objects are ignored.
		/// (:TODO: is this a bad idea? are we masking a real problem?)
		/// <p/>
		/// </summary>
		public Insanity[] Check(CacheEntry[] cacheEntries)
		{
			if (null == cacheEntries || 0 == cacheEntries.Length)
				return new Insanity[0];
			
			if (null != ramCalc)
			{
				for (int i = 0; i < cacheEntries.Length; i++)
				{
					cacheEntries[i].EstimateSize(ramCalc);
				}
			}
			
			// the indirect mapping lets MapOfSet dedup identical valIds for us
			//
			// maps the (valId) identityhashCode of cache values to 
			// sets of CacheEntry instances
			MapOfSets<int,CacheEntry> valIdToItems = new MapOfSets<int,CacheEntry>(new Dictionary<int,Dictionary<CacheEntry,CacheEntry>>(17));
			// maps ReaderField keys to Sets of ValueIds
			MapOfSets<ReaderField,int> readerFieldToValIds = new MapOfSets<ReaderField,int>(new Dictionary<ReaderField,Dictionary<int,int>>(17));
			//
			
			// any keys that we know result in more then one valId
            // TODO: This will be a HashSet<T> when we start using .NET Framework 3.5
            Dictionary<ReaderField, ReaderField> valMismatchKeys = new Dictionary<ReaderField, ReaderField>();
			
			// iterate over all the cacheEntries to get the mappings we'll need
			for (int i = 0; i < cacheEntries.Length; i++)
			{
				CacheEntry item = cacheEntries[i];
				System.Object val = item.GetValue();
				
				if (val is Mono.Lucene.Net.Search.CreationPlaceholder)
					continue;
				
				ReaderField rf = new ReaderField(item.GetReaderKey(), item.GetFieldName());
				
				System.Int32 valId = val.GetHashCode();
				
				// indirect mapping, so the MapOfSet will dedup identical valIds for us
				valIdToItems.Put(valId, item);
				if (1 < readerFieldToValIds.Put(rf, valId))
				{
                    if (!valMismatchKeys.ContainsKey(rf))
                    {
                        valMismatchKeys.Add(rf, rf);
                    }
				}
			}
			
			List<Insanity> insanity = new List<Insanity>(valMismatchKeys.Count * 3);
			
			insanity.AddRange(CheckValueMismatch(valIdToItems, readerFieldToValIds, valMismatchKeys));
			insanity.AddRange(CheckSubreaders(valIdToItems, readerFieldToValIds));
			
			return insanity.ToArray();
		}
		
		/// <summary> Internal helper method used by check that iterates over 
		/// valMismatchKeys and generates a Collection of Insanity 
		/// instances accordingly.  The MapOfSets are used to populate 
		/// the Insantiy objects. 
		/// </summary>
		/// <seealso cref="InsanityType.VALUEMISMATCH">
		/// </seealso>
		private List<Insanity> CheckValueMismatch(MapOfSets<int,CacheEntry> valIdToItems, MapOfSets<ReaderField,int> readerFieldToValIds, Dictionary<ReaderField,ReaderField> valMismatchKeys)
		{
			
			List<Insanity> insanity = new List<Insanity>(valMismatchKeys.Count * 3);
			
			if (!(valMismatchKeys.Count == 0))
			{
				// we have multiple values for some ReaderFields
				
                IDictionary<ReaderField,Dictionary<int,int>> rfMap = readerFieldToValIds.GetMap();
                IDictionary<int,Dictionary<CacheEntry,CacheEntry>> valMap = valIdToItems.GetMap();
                foreach (ReaderField rf in valMismatchKeys.Keys)
                {
                    List<CacheEntry> badEntries = new List<CacheEntry>(valMismatchKeys.Count * 2);
                    foreach (int val in rfMap[rf].Keys)
                    {
                        foreach (CacheEntry entry in valMap[val].Keys)
                        {
                            badEntries.Add(entry);
                        }
                    }

                    insanity.Add(new Insanity(InsanityType.VALUEMISMATCH, "Multiple distinct value objects for " + rf.ToString(), badEntries.ToArray()));
                }
            }
			return insanity;
		}
		
		/// <summary> Internal helper method used by check that iterates over 
		/// the keys of readerFieldToValIds and generates a Collection 
		/// of Insanity instances whenever two (or more) ReaderField instances are 
		/// found that have an ancestery relationships.  
		/// 
		/// </summary>
		/// <seealso cref="InsanityType.SUBREADER">
		/// </seealso>
		private List<Insanity> CheckSubreaders(MapOfSets<int,CacheEntry> valIdToItems, MapOfSets<ReaderField,int> readerFieldToValIds)
		{
			
            List<Insanity> insanity = new List<Insanity>(23);

            Dictionary<ReaderField, Dictionary<ReaderField, ReaderField>> badChildren = new Dictionary<ReaderField, Dictionary<ReaderField, ReaderField>>(17);
			MapOfSets<ReaderField, ReaderField> badKids = new MapOfSets<ReaderField, ReaderField>(badChildren); // wrapper

            IDictionary<int, Dictionary<CacheEntry, CacheEntry>> viToItemSets = valIdToItems.GetMap();
            IDictionary<ReaderField, Dictionary<int, int>> rfToValIdSets = readerFieldToValIds.GetMap();

            Dictionary<ReaderField, ReaderField> seen = new Dictionary<ReaderField, ReaderField>(17);

            foreach (ReaderField rf in rfToValIdSets.Keys)
            {
                if (seen.ContainsKey(rf))
                    continue;

                System.Collections.IList kids = GetAllDecendentReaderKeys(rf.readerKey);
				for (int i = 0; i < kids.Count; i++)
				{
					ReaderField kid = new ReaderField(kids[i], rf.fieldName);

					if (badChildren.ContainsKey(kid))
					{
						// we've already process this kid as RF and found other problems
						// track those problems as our own
						badKids.Put(rf, kid);
						badKids.PutAll(rf, badChildren[kid]);
						badChildren.Remove(kid);
					}
					else if (rfToValIdSets.ContainsKey(kid))
					{
						// we have cache entries for the kid
						badKids.Put(rf, kid);
					}
                    if (!seen.ContainsKey(kid))
                    {
                        seen.Add(kid, kid);
                    }
				}
                if (!seen.ContainsKey(rf))
                {
                    seen.Add(rf, rf);
                }
			}
			
			// every mapping in badKids represents an Insanity
			foreach (ReaderField parent in badChildren.Keys)
			{
				Dictionary<ReaderField,ReaderField> kids = badChildren[parent];
				
				List<CacheEntry> badEntries = new List<CacheEntry>(kids.Count * 2);
				
				// put parent entr(ies) in first
				{
					foreach (int val in rfToValIdSets[parent].Keys)
					{
						badEntries.AddRange(viToItemSets[val].Keys);
					}
				}
				
				// now the entries for the descendants
				foreach (ReaderField kid in kids.Keys)
				{
					foreach (int val in rfToValIdSets[kid].Keys)
					{
						badEntries.AddRange(viToItemSets[val].Keys);
					}
				}
				
				insanity.Add(new Insanity(InsanityType.SUBREADER, "Found caches for decendents of " + parent.ToString(), badEntries.ToArray()));
			}
			
			return insanity;
		}
		
		/// <summary> Checks if the seed is an IndexReader, and if so will walk
		/// the hierarchy of subReaders building up a list of the objects 
		/// returned by obj.getFieldCacheKey()
		/// </summary>
		private System.Collections.IList GetAllDecendentReaderKeys(System.Object seed)
		{
			System.Collections.IList all = new System.Collections.ArrayList(17); // will grow as we iter
			all.Add(seed);
			for (int i = 0; i < all.Count; i++)
			{
				System.Object obj = all[i];
				if (obj is IndexReader)
				{
					IndexReader[] subs = ((IndexReader) obj).GetSequentialSubReaders();
					for (int j = 0; (null != subs) && (j < subs.Length); j++)
					{
						all.Add(subs[j].GetFieldCacheKey());
					}
				}
			}
			// need to skip the first, because it was the seed
			return (System.Collections.IList) ((System.Collections.ArrayList) all).GetRange(1, all.Count - 1);
		}
		
		/// <summary> Simple pair object for using "readerKey + fieldName" a Map key</summary>
		private sealed class ReaderField
		{
			public System.Object readerKey;
			public System.String fieldName;
			public ReaderField(System.Object readerKey, System.String fieldName)
			{
				this.readerKey = readerKey;
				this.fieldName = fieldName;
			}
			public override int GetHashCode()
			{
				return readerKey.GetHashCode() * fieldName.GetHashCode();
			}
			public  override bool Equals(System.Object that)
			{
				if (!(that is ReaderField))
					return false;
				
				ReaderField other = (ReaderField) that;
				return (this.readerKey == other.readerKey && this.fieldName.Equals(other.fieldName));
			}
			public override System.String ToString()
			{
				return readerKey.ToString() + "+" + fieldName;
			}
		}
		
		/// <summary> Simple container for a collection of related CacheEntry objects that 
		/// in conjunction with eachother represent some "insane" usage of the 
		/// FieldCache.
		/// </summary>
		public sealed class Insanity
		{
			private InsanityType type;
			private System.String msg;
			private CacheEntry[] entries;
			public Insanity(InsanityType type, System.String msg, CacheEntry[] entries)
			{
				if (null == type)
				{
					throw new System.ArgumentException("Insanity requires non-null InsanityType");
				}
				if (null == entries || 0 == entries.Length)
				{
					throw new System.ArgumentException("Insanity requires non-null/non-empty CacheEntry[]");
				}
				this.type = type;
				this.msg = msg;
				this.entries = entries;
			}
			/// <summary> Type of insane behavior this object represents</summary>
			public new InsanityType GetType()
			{
				return type;
			}
			/// <summary> Description of hte insane behavior</summary>
			public System.String GetMsg()
			{
				return msg;
			}
			/// <summary> CacheEntry objects which suggest a problem</summary>
			public CacheEntry[] GetCacheEntries()
			{
				return entries;
			}
			/// <summary> Multi-Line representation of this Insanity object, starting with 
			/// the Type and Msg, followed by each CacheEntry.toString() on it's 
			/// own line prefaced by a tab character
			/// </summary>
			public override System.String ToString()
			{
				System.Text.StringBuilder buf = new System.Text.StringBuilder();
				buf.Append(GetType()).Append(": ");
				
				System.String m = GetMsg();
				if (null != m)
					buf.Append(m);
				
				buf.Append('\n');
				
				CacheEntry[] ce = GetCacheEntries();
				for (int i = 0; i < ce.Length; i++)
				{
					buf.Append('\t').Append(ce[i].ToString()).Append('\n');
				}
				
				return buf.ToString();
			}
		}
		
		/// <summary> An Enumaration of the differnet types of "insane" behavior that 
		/// may be detected in a FieldCache.
		/// 
		/// </summary>
		/// <seealso cref="InsanityType.SUBREADER">
		/// </seealso>
		/// <seealso cref="InsanityType.VALUEMISMATCH">
		/// </seealso>
		/// <seealso cref="InsanityType.EXPECTED">
		/// </seealso>
		public sealed class InsanityType
		{
			private System.String label;
			internal InsanityType(System.String label)
			{
				this.label = label;
			}
			public override System.String ToString()
			{
				return label;
			}
			
			/// <summary> Indicates an overlap in cache usage on a given field 
			/// in sub/super readers.
			/// </summary>
			public static readonly InsanityType SUBREADER = new InsanityType("SUBREADER");
			
			/// <summary> <p/>
			/// Indicates entries have the same reader+fieldname but 
			/// different cached values.  This can happen if different datatypes, 
			/// or parsers are used -- and while it's not necessarily a bug 
			/// it's typically an indication of a possible problem.
			/// <p/>
			/// <p/>
			/// PNOTE: Only the reader, fieldname, and cached value are actually 
			/// tested -- if two cache entries have different parsers or datatypes but 
			/// the cached values are the same Object (== not just equal()) this method 
			/// does not consider that a red flag.  This allows for subtle variations 
			/// in the way a Parser is specified (null vs DEFAULT_LONG_PARSER, etc...)
			/// <p/>
			/// </summary>
			public static readonly InsanityType VALUEMISMATCH = new InsanityType("VALUEMISMATCH");
			
			/// <summary> Indicates an expected bit of "insanity".  This may be useful for 
			/// clients that wish to preserve/log information about insane usage 
			/// but indicate that it was expected. 
			/// </summary>
			public static readonly InsanityType EXPECTED = new InsanityType("EXPECTED");
		}
	}
}
