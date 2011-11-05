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

using NumericField = Mono.Lucene.Net.Documents.NumericField;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using TermDocs = Mono.Lucene.Net.Index.TermDocs;
using TermEnum = Mono.Lucene.Net.Index.TermEnum;
using FieldCacheSanityChecker = Mono.Lucene.Net.Util.FieldCacheSanityChecker;
using StringHelper = Mono.Lucene.Net.Util.StringHelper;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Expert: The default cache implementation, storing all values in memory.
	/// A WeakHashMap is used for storage.
	/// 
	/// <p/>Created: May 19, 2004 4:40:36 PM
	/// 
	/// </summary>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: FieldCacheImpl.java 807572 2009-08-25 11:44:45Z mikemccand $
	/// </version>
	// TODO: change interface to FieldCache in 3.0 when removed
	class FieldCacheImpl : ExtendedFieldCache_old.ExtendedFieldCache
	{
		
		private System.Collections.IDictionary caches;
		internal FieldCacheImpl()
		{
			Init();
		}
		private void  Init()
		{
			lock (this)
			{
                System.Collections.Hashtable caches2 = new System.Collections.Hashtable(7);
                caches2[System.Type.GetType("System.SByte")] = new ByteCache(this);
                caches2[System.Type.GetType("System.Int16")] = new ShortCache(this);
                caches2[System.Type.GetType("System.Int32")] = new IntCache(this);
                caches2[System.Type.GetType("System.Single")] = new FloatCache(this);
                caches2[System.Type.GetType("System.Int64")] = new LongCache(this);
                caches2[System.Type.GetType("System.Double")] = new DoubleCache(this);
                caches2[typeof(System.String)] = new StringCache(this);
                caches2[typeof(StringIndex)] = new StringIndexCache(this);
                caches2[typeof(System.IComparable)] = new CustomCache(this);
                caches2[typeof(System.Object)] = new AutoCache(this);
                caches = caches2;
			}
		}
		
		public virtual void  PurgeAllCaches()
		{
			Init();
		}

        public void Purge(IndexReader r)
        {
            foreach (Cache c in caches.Values)
            {
                c.Purge(r);
            }
        }
		
		public virtual CacheEntry[] GetCacheEntries()
		{
			System.Collections.IList result = new System.Collections.ArrayList(17);
			System.Collections.IEnumerator outerKeys = caches.Keys.GetEnumerator();
			while (outerKeys.MoveNext())
			{
				System.Type cacheType = (System.Type) outerKeys.Current;
				Cache cache = (Cache) caches[cacheType];
				System.Collections.IEnumerator innerKeys = cache.readerCache.Keys.GetEnumerator();
				while (innerKeys.MoveNext())
				{
					// we've now materialized a hard ref
					System.Object readerKey = innerKeys.Current;
					// innerKeys was backed by WeakHashMap, sanity check
					// that it wasn't GCed before we made hard ref
					if (null != readerKey && cache.readerCache.Contains(readerKey))
					{
						System.Collections.IDictionary innerCache = ((System.Collections.IDictionary) cache.readerCache[readerKey]);
						System.Collections.IEnumerator entrySetIterator = new System.Collections.Hashtable(innerCache).GetEnumerator();
						while (entrySetIterator.MoveNext())
						{
							System.Collections.DictionaryEntry mapEntry = (System.Collections.DictionaryEntry) entrySetIterator.Current;
							Entry entry = (Entry) mapEntry.Key;
							result.Add(new CacheEntryImpl(readerKey, entry.field, cacheType, entry.type, entry.custom, entry.locale, mapEntry.Value));
						}
					}
				}
			}
			return (CacheEntry[]) new System.Collections.ArrayList(result).ToArray(typeof(CacheEntry));
		}
		
		private sealed class CacheEntryImpl:CacheEntry
		{
			/// <deprecated> Only needed because of Entry (ab)use by 
			/// FieldSortedHitQueue, remove when FieldSortedHitQueue 
			/// is removed
			/// </deprecated>
            [Obsolete("Only needed because of Entry (ab)use by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			private int sortFieldType;
			/// <deprecated> Only needed because of Entry (ab)use by 
			/// FieldSortedHitQueue, remove when FieldSortedHitQueue 
			/// is removed
			/// </deprecated>
            [Obsolete("Only needed because of Entry (ab)use by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			private System.Globalization.CultureInfo locale;
			
			private System.Object readerKey;
			private System.String fieldName;
			private System.Type cacheType;
			private System.Object custom;
			private System.Object value_Renamed;
			internal CacheEntryImpl(System.Object readerKey, System.String fieldName, System.Type cacheType, int sortFieldType, System.Object custom, System.Globalization.CultureInfo locale, System.Object value_Renamed)
			{
				this.readerKey = readerKey;
				this.fieldName = fieldName;
				this.cacheType = cacheType;
				this.sortFieldType = sortFieldType;
				this.custom = custom;
				this.locale = locale;
				this.value_Renamed = value_Renamed;
				
				// :HACK: for testing.
				//         if (null != locale || SortField.CUSTOM != sortFieldType) {
				//           throw new RuntimeException("Locale/sortFieldType: " + this);
				//         }
			}
			public override System.Object GetReaderKey()
			{
				return readerKey;
			}
			public override System.String GetFieldName()
			{
				return fieldName;
			}
			public override System.Type GetCacheType()
			{
				return cacheType;
			}
			public override System.Object GetCustom()
			{
				return custom;
			}
			public override System.Object GetValue()
			{
				return value_Renamed;
			}
			/// <summary> Adds warning to super.toString if Local or sortFieldType were specified</summary>
			/// <deprecated> Only needed because of Entry (ab)use by 
			/// FieldSortedHitQueue, remove when FieldSortedHitQueue 
			/// is removed
			/// </deprecated>
            [Obsolete("Only needed because of Entry (ab)use by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			public override System.String ToString()
			{
				System.String r = base.ToString();
				if (null != locale)
				{
					r = r + "...!!!Locale:" + locale + "???";
				}
				if (SortField.CUSTOM != sortFieldType)
				{
					r = r + "...!!!SortType:" + sortFieldType + "???";
				}
				return r;
			}
		}
		
		/// <summary> Hack: When thrown from a Parser (NUMERIC_UTILS_* ones), this stops
		/// processing terms and returns the current FieldCache
		/// array.
		/// </summary>
		[Serializable]
		internal sealed class StopFillCacheException:System.SystemException
		{
		}
		
		/// <summary>Expert: Internal cache. </summary>
		internal abstract class Cache
		{
			internal Cache()
			{
				this.wrapper = null;
			}
			
			internal Cache(FieldCache wrapper)
			{
				this.wrapper = wrapper;
			}
			
			internal FieldCache wrapper;

            internal System.Collections.IDictionary readerCache = new SupportClass.WeakHashTable();
			
			protected internal abstract System.Object CreateValue(IndexReader reader, Entry key);

            /** Remove this reader from the cache, if present. */
            public void Purge(IndexReader r)
            {
                object readerKey = r.GetFieldCacheKey();
                lock (readerCache)
                {
                    readerCache.Remove(readerKey);
                }
            }
			
			public virtual System.Object Get(IndexReader reader, Entry key)
			{
				System.Collections.IDictionary innerCache;
				System.Object value_Renamed;
				System.Object readerKey = reader.GetFieldCacheKey();
				lock (readerCache.SyncRoot)
				{
					innerCache = (System.Collections.IDictionary) readerCache[readerKey];
					if (innerCache == null)
					{
						innerCache = new System.Collections.Hashtable();
						readerCache[readerKey] = innerCache;
						value_Renamed = null;
					}
					else
					{
						value_Renamed = innerCache[key];
					}
					if (value_Renamed == null)
					{
						value_Renamed = new CreationPlaceholder();
						innerCache[key] = value_Renamed;
					}
				}
				if (value_Renamed is CreationPlaceholder)
				{
					lock (value_Renamed)
					{
						CreationPlaceholder progress = (CreationPlaceholder) value_Renamed;
						if (progress.value_Renamed == null)
						{
							progress.value_Renamed = CreateValue(reader, key);
							lock (readerCache.SyncRoot)
							{
								innerCache[key] = progress.value_Renamed;
							}
							
							// Only check if key.custom (the parser) is
							// non-null; else, we check twice for a single
							// call to FieldCache.getXXX
							if (key.custom != null && wrapper != null)
							{
								System.IO.StreamWriter infoStream = wrapper.GetInfoStream();
								if (infoStream != null)
								{
									PrintNewInsanity(infoStream, progress.value_Renamed);
								}
							}
						}
						return progress.value_Renamed;
					}
				}
				return value_Renamed;
			}
			
			private void  PrintNewInsanity(System.IO.StreamWriter infoStream, System.Object value_Renamed)
			{
				FieldCacheSanityChecker.Insanity[] insanities = FieldCacheSanityChecker.CheckSanity(wrapper);
				for (int i = 0; i < insanities.Length; i++)
				{
					FieldCacheSanityChecker.Insanity insanity = insanities[i];
					CacheEntry[] entries = insanity.GetCacheEntries();
					for (int j = 0; j < entries.Length; j++)
					{
						if (entries[j].GetValue() == value_Renamed)
						{
							// OK this insanity involves our entry
							infoStream.WriteLine("WARNING: new FieldCache insanity created\nDetails: " + insanity.ToString());
							infoStream.WriteLine("\nStack:\n");
                            infoStream.WriteLine(new System.Exception());
							break;
						}
					}
				}
			}
		}
		
		/// <summary>Expert: Every composite-key in the internal cache is of this type. </summary>
		protected internal class Entry
		{
			internal System.String field; // which Fieldable
			/// <deprecated> Only (ab)used by FieldSortedHitQueue, 
			/// remove when FieldSortedHitQueue is removed
			/// </deprecated>
            [Obsolete("Only (ab)used by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			internal int type; // which SortField type
			internal System.Object custom; // which custom comparator or parser
			/// <deprecated> Only (ab)used by FieldSortedHitQueue, 
			/// remove when FieldSortedHitQueue is removed
			/// </deprecated>
            [Obsolete("Only (ab)used by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			internal System.Globalization.CultureInfo locale; // the locale we're sorting (if string)
			
			/// <deprecated> Only (ab)used by FieldSortedHitQueue, 
			/// remove when FieldSortedHitQueue is removed
			/// </deprecated>
            [Obsolete("Only (ab)used by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			internal Entry(System.String field, int type, System.Globalization.CultureInfo locale)
			{
				this.field = StringHelper.Intern(field);
				this.type = type;
				this.custom = null;
				this.locale = locale;
			}
			
			/// <summary>Creates one of these objects for a custom comparator/parser. </summary>
			internal Entry(System.String field, System.Object custom)
			{
				this.field = StringHelper.Intern(field);
				this.type = SortField.CUSTOM;
				this.custom = custom;
				this.locale = null;
			}
			
			/// <deprecated> Only (ab)used by FieldSortedHitQueue, 
			/// remove when FieldSortedHitQueue is removed
			/// </deprecated>
            [Obsolete("Only (ab)used by FieldSortedHitQueue, remove when FieldSortedHitQueue is removed")]
			internal Entry(System.String field, int type, Parser parser)
			{
				this.field = StringHelper.Intern(field);
				this.type = type;
				this.custom = parser;
				this.locale = null;
			}
			
			/// <summary>Two of these are equal iff they reference the same field and type. </summary>
			public  override bool Equals(System.Object o)
			{
				if (o is Entry)
				{
					Entry other = (Entry) o;
					if ((System.Object) other.field == (System.Object) field && other.type == type)
					{
						if (other.locale == null?locale == null:other.locale.Equals(locale))
						{
							if (other.custom == null)
							{
								if (custom == null)
									return true;
							}
							else if (other.custom.Equals(custom))
							{
								return true;
							}
						}
					}
				}
				return false;
			}
			
			/// <summary>Composes a hashcode based on the field and type. </summary>
			public override int GetHashCode()
			{
				return field.GetHashCode() ^ type ^ (custom == null?0:custom.GetHashCode()) ^ (locale == null?0:locale.GetHashCode());
			}
		}
		
		// inherit javadocs
		public virtual sbyte[] GetBytes(IndexReader reader, System.String field)
		{
			return GetBytes(reader, field, null);
		}
		
		// inherit javadocs
		public virtual sbyte[] GetBytes(IndexReader reader, System.String field, ByteParser parser)
		{
			return (sbyte[]) ((Cache) caches[System.Type.GetType("System.SByte")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class ByteCache:Cache
		{
			internal ByteCache(FieldCache wrapper):base(wrapper)
			{
			}
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				ByteParser parser = (ByteParser) entry.custom;
				if (parser == null)
				{
					return wrapper.GetBytes(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_BYTE_PARSER);
				}
				sbyte[] retArray = new sbyte[reader.MaxDoc()];
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						sbyte termval = parser.ParseByte(term.Text());
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				return retArray;
			}
		}
		
		
		// inherit javadocs
		public virtual short[] GetShorts(IndexReader reader, System.String field)
		{
			return GetShorts(reader, field, null);
		}
		
		// inherit javadocs
		public virtual short[] GetShorts(IndexReader reader, System.String field, ShortParser parser)
		{
			return (short[]) ((Cache) caches[System.Type.GetType("System.Int16")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class ShortCache:Cache
		{
			internal ShortCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				ShortParser parser = (ShortParser) entry.custom;
				if (parser == null)
				{
					return wrapper.GetShorts(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_SHORT_PARSER);
				}
				short[] retArray = new short[reader.MaxDoc()];
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						short termval = parser.ParseShort(term.Text());
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				return retArray;
			}
		}
		
		
		// inherit javadocs
		public virtual int[] GetInts(IndexReader reader, System.String field)
		{
			return GetInts(reader, field, null);
		}
		
		// inherit javadocs
		public virtual int[] GetInts(IndexReader reader, System.String field, IntParser parser)
		{
			return (int[]) ((Cache) caches[System.Type.GetType("System.Int32")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class IntCache:Cache
		{
			internal IntCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				IntParser parser = (IntParser) entry.custom;
				if (parser == null)
				{
					try
					{
						return wrapper.GetInts(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_INT_PARSER);
					}
					catch (System.FormatException ne)
					{
						return wrapper.GetInts(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_INT_PARSER);
					}
				}
				int[] retArray = null;
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						int termval = parser.ParseInt(term.Text());
						if (retArray == null)
						// late init
							retArray = new int[reader.MaxDoc()];
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				if (retArray == null)
				// no values
					retArray = new int[reader.MaxDoc()];
				return retArray;
			}
		}
		
		
		
		// inherit javadocs
		public virtual float[] GetFloats(IndexReader reader, System.String field)
		{
			return GetFloats(reader, field, null);
		}
		
		// inherit javadocs
		public virtual float[] GetFloats(IndexReader reader, System.String field, FloatParser parser)
		{
			
			return (float[]) ((Cache) caches[System.Type.GetType("System.Single")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class FloatCache:Cache
		{
			internal FloatCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				FloatParser parser = (FloatParser) entry.custom;
				if (parser == null)
				{
					try
					{
						return wrapper.GetFloats(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_FLOAT_PARSER);
					}
					catch (System.FormatException ne)
					{
						return wrapper.GetFloats(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_FLOAT_PARSER);
					}
				}
				float[] retArray = null;
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						float termval = parser.ParseFloat(term.Text());
						if (retArray == null)
						// late init
							retArray = new float[reader.MaxDoc()];
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				if (retArray == null)
				// no values
					retArray = new float[reader.MaxDoc()];
				return retArray;
			}
		}
		
		
		
		public virtual long[] GetLongs(IndexReader reader, System.String field)
		{
			return GetLongs(reader, field, null);
		}
		
		// inherit javadocs
		public virtual long[] GetLongs(IndexReader reader, System.String field, Mono.Lucene.Net.Search.LongParser parser)
		{
			return (long[]) ((Cache) caches[System.Type.GetType("System.Int64")]).Get(reader, new Entry(field, parser));
		}
		
		/// <deprecated> Will be removed in 3.0, this is for binary compatibility only 
		/// </deprecated>
        [Obsolete("Will be removed in 3.0, this is for binary compatibility only ")]
		public virtual long[] GetLongs(IndexReader reader, System.String field, Mono.Lucene.Net.Search.ExtendedFieldCache_old.LongParser parser)
		{
			return (long[]) ((Cache) caches[System.Type.GetType("System.Int64")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class LongCache:Cache
		{
			internal LongCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				Mono.Lucene.Net.Search.LongParser parser = (Mono.Lucene.Net.Search.LongParser) entry.custom;
				if (parser == null)
				{
					try
					{
						return wrapper.GetLongs(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_LONG_PARSER);
					}
					catch (System.FormatException ne)
					{
						return wrapper.GetLongs(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_LONG_PARSER);
					}
				}
				long[] retArray = null;
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						long termval = parser.ParseLong(term.Text());
						if (retArray == null)
						// late init
							retArray = new long[reader.MaxDoc()];
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				if (retArray == null)
				// no values
					retArray = new long[reader.MaxDoc()];
				return retArray;
			}
		}
		
		
		// inherit javadocs
		public virtual double[] GetDoubles(IndexReader reader, System.String field)
		{
			return GetDoubles(reader, field, null);
		}
		
		// inherit javadocs
		public virtual double[] GetDoubles(IndexReader reader, System.String field, Mono.Lucene.Net.Search.DoubleParser parser)
		{
			return (double[]) ((Cache) caches[System.Type.GetType("System.Double")]).Get(reader, new Entry(field, parser));
		}
		
		/// <deprecated> Will be removed in 3.0, this is for binary compatibility only 
		/// </deprecated>
        [Obsolete("Will be removed in 3.0, this is for binary compatibility only ")]
		public virtual double[] GetDoubles(IndexReader reader, System.String field, Mono.Lucene.Net.Search.ExtendedFieldCache_old.DoubleParser parser)
		{
			return (double[]) ((Cache) caches[System.Type.GetType("System.Double")]).Get(reader, new Entry(field, parser));
		}
		
		internal sealed class DoubleCache:Cache
		{
			internal DoubleCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				Mono.Lucene.Net.Search.DoubleParser parser = (Mono.Lucene.Net.Search.DoubleParser) entry.custom;
				if (parser == null)
				{
					try
					{
						return wrapper.GetDoubles(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT_DOUBLE_PARSER);
					}
					catch (System.FormatException ne)
					{
						return wrapper.GetDoubles(reader, field, Mono.Lucene.Net.Search.FieldCache_Fields.NUMERIC_UTILS_DOUBLE_PARSER);
					}
				}
				double[] retArray = null;
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						double termval = parser.ParseDouble(term.Text());
						if (retArray == null)
						// late init
							retArray = new double[reader.MaxDoc()];
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				catch (StopFillCacheException stop)
				{
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				if (retArray == null)
				// no values
					retArray = new double[reader.MaxDoc()];
				return retArray;
			}
		}
		
		
		// inherit javadocs
		public virtual System.String[] GetStrings(IndexReader reader, System.String field)
		{
			return (System.String[]) ((Cache) caches[typeof(System.String)]).Get(reader, new Entry(field, (Parser) null));
		}
		
		internal sealed class StringCache:Cache
		{
			internal StringCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				System.String field = StringHelper.Intern((System.String) entryKey.field);
				System.String[] retArray = new System.String[reader.MaxDoc()];
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						System.String termval = term.Text();
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				return retArray;
			}
		}
		
		
		// inherit javadocs
		public virtual StringIndex GetStringIndex(IndexReader reader, System.String field)
		{
			return (StringIndex) ((Cache) caches[typeof(StringIndex)]).Get(reader, new Entry(field, (Parser) null));
		}
		
		internal sealed class StringIndexCache:Cache
		{
			internal StringIndexCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				System.String field = StringHelper.Intern((System.String) entryKey.field);
				int[] retArray = new int[reader.MaxDoc()];
				System.String[] mterms = new System.String[reader.MaxDoc() + 1];
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				int t = 0; // current term number
				
				// an entry for documents that have no terms in this field
				// should a document with no terms be at top or bottom?
				// this puts them at the top - if it is changed, FieldDocSortedHitQueue
				// needs to change as well.
				mterms[t++] = null;
				
				try
				{
					do 
					{
						Term term = termEnum.Term();
                        if (term == null || term.Field() != field || t >= mterms.Length) break;
						
						// store term text
						mterms[t] = term.Text();
						
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = t;
						}
						
						t++;
					}
					while (termEnum.Next());
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				
				if (t == 0)
				{
					// if there are no terms, make the term array
					// have a single null entry
					mterms = new System.String[1];
				}
				else if (t < mterms.Length)
				{
					// if there are less terms than documents,
					// trim off the dead array space
					System.String[] terms = new System.String[t];
					Array.Copy(mterms, 0, terms, 0, t);
					mterms = terms;
				}
				
				StringIndex value_Renamed = new StringIndex(retArray, mterms);
				return value_Renamed;
			}
		}
		
		
		/// <summary>The pattern used to detect integer values in a field </summary>
		/// <summary>removed for java 1.3 compatibility
		/// protected static final Pattern pIntegers = Pattern.compile ("[0-9\\-]+");
		/// 
		/// </summary>
		
		/// <summary>The pattern used to detect float values in a field </summary>
		/// <summary> removed for java 1.3 compatibility
		/// protected static final Object pFloats = Pattern.compile ("[0-9+\\-\\.eEfFdD]+");
		/// </summary>
		
		// inherit javadocs
		public virtual System.Object GetAuto(IndexReader reader, System.String field)
		{
			return ((Cache) caches[typeof(System.Object)]).Get(reader, new Entry(field, (Parser) null));
		}
		
		/// <deprecated> Please specify the exact type, instead.
		/// Especially, guessing does <b>not</b> work with the new
		/// {@link NumericField} type.
		/// </deprecated>
        [Obsolete("Please specify the exact type, instead. Especially, guessing does not work with the new NumericField type.")]
		internal sealed class AutoCache:Cache
		{
			internal AutoCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				System.String field = StringHelper.Intern((System.String) entryKey.field);
				TermEnum enumerator = reader.Terms(new Term(field));
				try
				{
					Term term = enumerator.Term();
					if (term == null)
					{
						throw new System.SystemException("no terms in field " + field + " - cannot determine type");
					}
					System.Object ret = null;
					if ((System.Object) term.Field() == (System.Object) field)
					{
						System.String termtext = term.Text().Trim();
						
						try
						{
							System.Int32.Parse(termtext);
							ret = wrapper.GetInts(reader, field);
						}
						catch (System.FormatException nfe1)
						{
							try
							{
								System.Int64.Parse(termtext);
								ret = wrapper.GetLongs(reader, field);
							}
							catch (System.FormatException nfe2)
							{
								try
								{
                                    SupportClass.Single.Parse(termtext);
									ret = wrapper.GetFloats(reader, field);
								}
								catch (System.FormatException nfe3)
								{
									ret = wrapper.GetStringIndex(reader, field);
								}
							}
						}
					}
					else
					{
						throw new System.SystemException("field \"" + field + "\" does not appear to be indexed");
					}
					return ret;
				}
				finally
				{
					enumerator.Close();
				}
			}
		}
		
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		public virtual System.IComparable[] GetCustom(IndexReader reader, System.String field, SortComparator comparator)
		{
			return (System.IComparable[]) ((Cache) caches[typeof(System.IComparable)]).Get(reader, new Entry(field, comparator));
		}
		
		/// <deprecated> 
		/// </deprecated>
        [Obsolete]
		internal sealed class CustomCache:Cache
		{
			internal CustomCache(FieldCache wrapper):base(wrapper)
			{
			}
			
			protected internal override System.Object CreateValue(IndexReader reader, Entry entryKey)
			{
				Entry entry = (Entry) entryKey;
				System.String field = entry.field;
				SortComparator comparator = (SortComparator) entry.custom;
				System.IComparable[] retArray = new System.IComparable[reader.MaxDoc()];
				TermDocs termDocs = reader.TermDocs();
				TermEnum termEnum = reader.Terms(new Term(field));
				try
				{
					do 
					{
						Term term = termEnum.Term();
						if (term == null || (System.Object) term.Field() != (System.Object) field)
							break;
						System.IComparable termval = comparator.GetComparable(term.Text());
						termDocs.Seek(termEnum);
						while (termDocs.Next())
						{
							retArray[termDocs.Doc()] = termval;
						}
					}
					while (termEnum.Next());
				}
				finally
				{
					termDocs.Close();
					termEnum.Close();
				}
				return retArray;
			}
		}
		
		
		private volatile System.IO.StreamWriter infoStream;
		
		public virtual void  SetInfoStream(System.IO.StreamWriter stream)
		{
			infoStream = stream;
		}
		
		public virtual System.IO.StreamWriter GetInfoStream()
		{
			return infoStream;
		}
	}
}
