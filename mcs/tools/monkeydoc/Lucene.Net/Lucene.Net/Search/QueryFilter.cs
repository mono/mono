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

namespace Mono.Lucene.Net.Search
{
	
	
	/// <summary>Constrains search results to only match those which also match a provided
	/// query.  Results are cached, so that searches after the first on the same
	/// index using this filter are much faster.
	/// 
	/// </summary>
	/// <version>  $Id: QueryFilter.java 528298 2007-04-13 00:59:28Z hossman $
	/// </version>
	/// <deprecated> use a CachingWrapperFilter with QueryWrapperFilter
	/// </deprecated>
    [Obsolete("use a CachingWrapperFilter with QueryWrapperFilter")]
	[Serializable]
	public class QueryFilter:CachingWrapperFilter
	{
		
		/// <summary>Constructs a filter which only matches documents matching
		/// <code>query</code>.
		/// </summary>
		public QueryFilter(Query query):base(new QueryWrapperFilter(query))
		{
		}
		
		public  override bool Equals(System.Object o)
		{
			return base.Equals((QueryFilter) o);
		}
		
		public override int GetHashCode()
		{
			return base.GetHashCode() ^ unchecked((int) 0x923F64B9);
		}
	}
}
