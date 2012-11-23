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
using Query = Mono.Lucene.Net.Search.Query;
using Searcher = Mono.Lucene.Net.Search.Searcher;
using Weight = Mono.Lucene.Net.Search.Weight;

namespace Mono.Lucene.Net.Search.Spans
{
	
	/// <summary>Base class for span-based queries. </summary>
	[Serializable]
	public abstract class SpanQuery:Query
	{
		/// <summary>Expert: Returns the matches for this query in an index.  Used internally
		/// to search for spans. 
		/// </summary>
		public abstract Spans GetSpans(IndexReader reader);
		
		/// <summary>Returns the name of the field matched by this query.</summary>
		public abstract System.String GetField();
		
		/// <summary>Returns a collection of all terms matched by this query.</summary>
		/// <deprecated> use extractTerms instead
		/// </deprecated>
		/// <seealso cref="Query.ExtractTerms(Set)">
		/// </seealso>
        [Obsolete("use ExtractTerms instead")]
		public abstract System.Collections.ICollection GetTerms();
		
		public override Weight CreateWeight(Searcher searcher)
		{
			return new SpanWeight(this, searcher);
		}
	}
}
