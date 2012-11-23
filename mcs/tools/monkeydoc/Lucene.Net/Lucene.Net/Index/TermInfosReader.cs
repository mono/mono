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

using Directory = Mono.Lucene.Net.Store.Directory;
using CloseableThreadLocal = Mono.Lucene.Net.Util.CloseableThreadLocal;
using SimpleLRUCache = Mono.Lucene.Net.Util.Cache.SimpleLRUCache;

namespace Mono.Lucene.Net.Index
{

    /// <summary>This stores a monotonically increasing set of &lt;Term, TermInfo&gt; pairs in a
	/// Directory.  Pairs are accessed either by Term or by ordinal position the
	/// set.  
	/// </summary>
	
	sealed class TermInfosReader
	{
		private Directory directory;
		private System.String segment;
		private FieldInfos fieldInfos;
		
		private CloseableThreadLocal threadResources = new CloseableThreadLocal();
		private SegmentTermEnum origEnum;
		private long size;
		
		private Term[] indexTerms;
		private TermInfo[] indexInfos;
		private long[] indexPointers;
		
		private int totalIndexInterval;
		
		private const int DEFAULT_CACHE_SIZE = 1024;
		
		/// <summary> Per-thread resources managed by ThreadLocal</summary>
		private sealed class ThreadResources
		{
			internal SegmentTermEnum termEnum;
			
			// Used for caching the least recently looked-up Terms
			internal Mono.Lucene.Net.Util.Cache.Cache termInfoCache;
		}
		
		internal TermInfosReader(Directory dir, System.String seg, FieldInfos fis, int readBufferSize, int indexDivisor)
		{
			bool success = false;
			
			if (indexDivisor < 1 && indexDivisor != - 1)
			{
				throw new System.ArgumentException("indexDivisor must be -1 (don't load terms index) or greater than 0: got " + indexDivisor);
			}
			
			try
			{
				directory = dir;
				segment = seg;
				fieldInfos = fis;
				
				origEnum = new SegmentTermEnum(directory.OpenInput(segment + "." + IndexFileNames.TERMS_EXTENSION, readBufferSize), fieldInfos, false);
				size = origEnum.size;
				
				
				if (indexDivisor != - 1)
				{
					// Load terms index
					totalIndexInterval = origEnum.indexInterval * indexDivisor;
					SegmentTermEnum indexEnum = new SegmentTermEnum(directory.OpenInput(segment + "." + IndexFileNames.TERMS_INDEX_EXTENSION, readBufferSize), fieldInfos, true);
					
					try
					{
						int indexSize = 1 + ((int) indexEnum.size - 1) / indexDivisor; // otherwise read index
						
						indexTerms = new Term[indexSize];
						indexInfos = new TermInfo[indexSize];
						indexPointers = new long[indexSize];
						
						for (int i = 0; indexEnum.Next(); i++)
						{
							indexTerms[i] = indexEnum.Term();
							indexInfos[i] = indexEnum.TermInfo();
							indexPointers[i] = indexEnum.indexPointer;
							
							for (int j = 1; j < indexDivisor; j++)
								if (!indexEnum.Next())
									break;
						}
					}
					finally
					{
						indexEnum.Close();
					}
				}
				else
				{
					// Do not load terms index:
					totalIndexInterval = - 1;
					indexTerms = null;
					indexInfos = null;
					indexPointers = null;
				}
				success = true;
			}
			finally
			{
				// With lock-less commits, it's entirely possible (and
				// fine) to hit a FileNotFound exception above. In
				// this case, we want to explicitly close any subset
				// of things that were opened so that we don't have to
				// wait for a GC to do so.
				if (!success)
				{
					Close();
				}
			}
		}
		
		public int GetSkipInterval()
		{
			return origEnum.skipInterval;
		}
		
		public int GetMaxSkipLevels()
		{
			return origEnum.maxSkipLevels;
		}
		
		internal void  Close()
		{
			if (origEnum != null)
				origEnum.Close();
			threadResources.Close();
		}
		
		/// <summary>Returns the number of term/value pairs in the set. </summary>
		internal long Size()
		{
			return size;
		}
		
		private ThreadResources GetThreadResources()
		{
			ThreadResources resources = (ThreadResources) threadResources.Get();
			if (resources == null)
			{
				resources = new ThreadResources();
				resources.termEnum = Terms();
				// Cache does not have to be thread-safe, it is only used by one thread at the same time
				resources.termInfoCache = new SimpleLRUCache(DEFAULT_CACHE_SIZE);
				threadResources.Set(resources);
			}
			return resources;
		}
		
		
		/// <summary>Returns the offset of the greatest index entry which is less than or equal to term.</summary>
		private int GetIndexOffset(Term term)
		{
			int lo = 0; // binary search indexTerms[]
			int hi = indexTerms.Length - 1;
			
			while (hi >= lo)
			{
				int mid = SupportClass.Number.URShift((lo + hi), 1);
				int delta = term.CompareTo(indexTerms[mid]);
				if (delta < 0)
					hi = mid - 1;
				else if (delta > 0)
					lo = mid + 1;
				else
					return mid;
			}
			return hi;
		}
		
		private void  SeekEnum(SegmentTermEnum enumerator, int indexOffset)
		{
			enumerator.Seek(indexPointers[indexOffset], ((long)indexOffset * totalIndexInterval) - 1, indexTerms[indexOffset], indexInfos[indexOffset]);
		}
		
		/// <summary>Returns the TermInfo for a Term in the set, or null. </summary>
		internal TermInfo Get(Term term)
		{
			return Get(term, true);
		}
		
		/// <summary>Returns the TermInfo for a Term in the set, or null. </summary>
		private TermInfo Get(Term term, bool useCache)
		{
			if (size == 0)
				return null;
			
			EnsureIndexIsRead();
			
			TermInfo ti;
			ThreadResources resources = GetThreadResources();
			Mono.Lucene.Net.Util.Cache.Cache cache = null;
			
			if (useCache)
			{
				cache = resources.termInfoCache;
				// check the cache first if the term was recently looked up
				ti = (TermInfo) cache.Get(term);
				if (ti != null)
				{
					return ti;
				}
			}
			
			// optimize sequential access: first try scanning cached enum w/o seeking
			SegmentTermEnum enumerator = resources.termEnum;
			if (enumerator.Term() != null && ((enumerator.Prev() != null && term.CompareTo(enumerator.Prev()) > 0) || term.CompareTo(enumerator.Term()) >= 0))
			{
				int enumOffset = (int) (enumerator.position / totalIndexInterval) + 1;
				if (indexTerms.Length == enumOffset || term.CompareTo(indexTerms[enumOffset]) < 0)
				{
					// no need to seek
					
					int numScans = enumerator.ScanTo(term);
					if (enumerator.Term() != null && term.CompareTo(enumerator.Term()) == 0)
					{
						ti = enumerator.TermInfo();
						if (cache != null && numScans > 1)
						{
							// we only  want to put this TermInfo into the cache if
							// scanEnum skipped more than one dictionary entry.
							// This prevents RangeQueries or WildcardQueries to 
							// wipe out the cache when they iterate over a large numbers
							// of terms in order
							cache.Put(term, ti);
						}
					}
					else
					{
						ti = null;
					}
					
					return ti;
				}
			}
			
			// random-access: must seek
			SeekEnum(enumerator, GetIndexOffset(term));
			enumerator.ScanTo(term);
			if (enumerator.Term() != null && term.CompareTo(enumerator.Term()) == 0)
			{
				ti = enumerator.TermInfo();
				if (cache != null)
				{
					cache.Put(term, ti);
				}
			}
			else
			{
				ti = null;
			}
			return ti;
		}
						
		private void  EnsureIndexIsRead()
		{
			if (indexTerms == null)
			{
				throw new System.SystemException("terms index was not loaded when this reader was created");
			}
		}
		
		/// <summary>Returns the position of a Term in the set or -1. </summary>
		internal long GetPosition(Term term)
		{
			if (size == 0)
				return - 1;
			
			EnsureIndexIsRead();
			int indexOffset = GetIndexOffset(term);
			
			SegmentTermEnum enumerator = GetThreadResources().termEnum;
			SeekEnum(enumerator, indexOffset);
			
			while (term.CompareTo(enumerator.Term()) > 0 && enumerator.Next())
			{
			}
			
			if (term.CompareTo(enumerator.Term()) == 0)
				return enumerator.position;
			else
				return - 1;
		}
		
		/// <summary>Returns an enumeration of all the Terms and TermInfos in the set. </summary>
		public SegmentTermEnum Terms()
		{
			return (SegmentTermEnum) origEnum.Clone();
		}
		
		/// <summary>Returns an enumeration of terms starting at or after the named term. </summary>
		public SegmentTermEnum Terms(Term term)
		{
			// don't use the cache in this call because we want to reposition the
			// enumeration
			Get(term, false);
			return (SegmentTermEnum) GetThreadResources().termEnum.Clone();
		}
	}
}
