/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
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
using Monodoc.Lucene.Net.Index;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Wraps another filters result and caches it.  The caching
	/// behavior is like {@link QueryFilter}.  The purpose is to allow
	/// filters to simply filter, and then wrap with this class to add
	/// caching, keeping the two concerns decoupled yet composable.
	/// </summary>
	[Serializable]
	public class CachingWrapperFilter:Filter
	{
		private Filter filter;
		
		/// <todo>  What about serialization in RemoteSearchable?  Caching won't work. </todo>
		/// <summary>       Should transient be removed?
		/// </summary>
		[NonSerialized]
		private System.Collections.IDictionary cache;
		
		/// <param name="filter">Filter to cache results of
		/// </param>
		public CachingWrapperFilter(Filter filter)
		{
			this.filter = filter;
		}
		
		public override System.Collections.BitArray Bits(Monodoc.Lucene.Net.Index.IndexReader reader)
		{
			if (cache == null)
			{
				cache = new System.Collections.Hashtable();
			}
			
			lock (cache.SyncRoot)
			{
				// check cache
				System.Collections.BitArray cached = (System.Collections.BitArray) cache[reader];
				if (cached != null)
				{
					return cached;
				}
			}
			
			System.Collections.BitArray bits = filter.Bits(reader);
			
			lock (cache.SyncRoot)
			{
				// update cache
				cache[reader] = bits;
			}
			
			return bits;
		}
		
		public override System.String ToString()
		{
			return "CachingWrapperFilter(" + filter + ")";
		}
	}
}