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

using System.Runtime.InteropServices;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Wraps another SpanFilter's result and caches it.  The purpose is to allow
	/// filters to simply filter, and then wrap with this class to add caching.
	/// </summary>
	[Serializable]
	public class CachingSpanFilter:SpanFilter
	{
		protected internal SpanFilter filter;
		
		/// <summary> A transient Filter cache.</summary>
		[NonSerialized]
        internal CachingWrapperFilter.FilterCache cache;

        /// <summary>
        /// New deletions always result in a cache miss, by default
        /// ({@link CachingWrapperFilter.DeletesMode#RECACHE}.
        /// <param name="filter">Filter to cache results of
		/// </param>
        /// </summary>
        public CachingSpanFilter(SpanFilter filter): this(filter, CachingWrapperFilter.DeletesMode.RECACHE)
		{
			
		}

        /**
        * @param filter Filter to cache results of
        * @param deletesMode See {@link CachingWrapperFilter.DeletesMode}
        */
        public CachingSpanFilter(SpanFilter filter, CachingWrapperFilter.DeletesMode deletesMode)
        {
            this.filter = filter;
            if (deletesMode == CachingWrapperFilter.DeletesMode.DYNAMIC)
            {
                throw new System.ArgumentException("DeletesMode.DYNAMIC is not supported");
            }
            this.cache = new AnonymousFilterCache(deletesMode);
        }

        class AnonymousFilterCache : CachingWrapperFilter.FilterCache
        {
            public AnonymousFilterCache(CachingWrapperFilter.DeletesMode deletesMode) : base(deletesMode)
            {
            }

            protected override object MergeDeletes(IndexReader reader, object docIdSet)
            {
                throw new System.ArgumentException("DeletesMode.DYNAMIC is not supported");
            }
        }

		/// <deprecated> Use {@link #GetDocIdSet(IndexReader)} instead.
		/// </deprecated>
        [Obsolete("Use GetDocIdSet(IndexReader) instead.")]
		public override System.Collections.BitArray Bits(IndexReader reader)
		{
			SpanFilterResult result = GetCachedResult(reader);
			return result != null?result.GetBits():null;
		}
		
		public override DocIdSet GetDocIdSet(IndexReader reader)
		{
			SpanFilterResult result = GetCachedResult(reader);
			return result != null?result.GetDocIdSet():null;
		}

        // for testing
        public int hitCount, missCount;

		private SpanFilterResult GetCachedResult(IndexReader reader)
		{
            object coreKey = reader.GetFieldCacheKey();
            object delCoreKey = reader.HasDeletions() ? reader.GetDeletesCacheKey() : coreKey;

            SpanFilterResult result = (SpanFilterResult) cache.Get(reader, coreKey, delCoreKey);
            if (result != null) {
                hitCount++;
                return result;
            }

            missCount++;
            result = filter.BitSpans(reader);

            cache.Put(coreKey, delCoreKey, result);
            return result;
		}
		
		
		public override SpanFilterResult BitSpans(IndexReader reader)
		{
			return GetCachedResult(reader);
		}
		
		public override System.String ToString()
		{
			return "CachingSpanFilter(" + filter + ")";
		}
		
		public  override bool Equals(System.Object o)
		{
			if (!(o is CachingSpanFilter))
				return false;
			return this.filter.Equals(((CachingSpanFilter) o).filter);
		}
		
		public override int GetHashCode()
		{
			return filter.GetHashCode() ^ 0x1117BF25;
		}
	}
}
