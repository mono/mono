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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Search.ExtendedFieldCache_old
{
	
	/// <summary> This interface is obsolete, use {@link FieldCache} instead.
	/// 
	/// </summary>
	/// <deprecated> Use {@link FieldCache}, this will be removed in Lucene 3.0
	/// 
	/// </deprecated>
    [Obsolete("Use FieldCache, this will be removed in Lucene 3.0")]
	public struct ExtendedFieldCache_Fields{
		/// <deprecated> Use {@link FieldCache#DEFAULT}; this will be removed in Lucene 3.0 
		/// </deprecated>
        [Obsolete("Use FieldCache.DEFAULT; this will be removed in Lucene 3.0 ")]
		public readonly static ExtendedFieldCache EXT_DEFAULT;
		static ExtendedFieldCache_Fields()
		{
			EXT_DEFAULT = (ExtendedFieldCache) Mono.Lucene.Net.Search.FieldCache_Fields.DEFAULT;
		}
	}
	public interface ExtendedFieldCache:FieldCache
	{
		
		/// <deprecated> Will be removed in 3.0, this is for binary compatibility only 
		/// </deprecated>
        [Obsolete("Will be removed in 3.0, this is for binary compatibility only ")]
		new long[] GetLongs(IndexReader reader, System.String field, Mono.Lucene.Net.Search.LongParser parser);
		
		/// <deprecated> Will be removed in 3.0, this is for binary compatibility only 
		/// </deprecated>
        [Obsolete("Will be removed in 3.0, this is for binary compatibility only ")]
		new double[] GetDoubles(IndexReader reader, System.String field, Mono.Lucene.Net.Search.DoubleParser parser);
	}

	/// <deprecated> Use {@link FieldCache.LongParser}, this will be removed in Lucene 3.0 
	/// </deprecated>
    [Obsolete("Use FieldCache.LongParser, this will be removed in Lucene 3.0 ")]
	public interface LongParser:Mono.Lucene.Net.Search.LongParser
	{
	}

	/// <deprecated> Use {@link FieldCache.DoubleParser}, this will be removed in Lucene 3.0 
	/// </deprecated>
    [Obsolete("Use FieldCache.DoubleParser, this will be removed in Lucene 3.0 ")]
	public interface DoubleParser:Mono.Lucene.Net.Search.DoubleParser
	{
	}
}
