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

using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A base class for all collectors that return a {@link TopDocs} output. This
	/// collector allows easy extension by providing a single constructor which
	/// accepts a {@link PriorityQueue} as well as protected members for that
	/// priority queue and a counter of the number of total hits.<br/>
	/// Extending classes can override {@link #TopDocs(int, int)} and
	/// {@link #GetTotalHits()} in order to provide their own implementation.
	/// </summary>
	public abstract class TopDocsCollector:Collector
	{
		
		// This is used in case topDocs() is called with illegal parameters, or there
		// simply aren't (enough) results.
		protected internal static readonly TopDocs EMPTY_TOPDOCS = new TopDocs(0, new ScoreDoc[0], System.Single.NaN);
		
		/// <summary> The priority queue which holds the top documents. Note that different
		/// implementations of PriorityQueue give different meaning to 'top documents'.
		/// HitQueue for example aggregates the top scoring documents, while other PQ
		/// implementations may hold documents sorted by other criteria.
		/// </summary>
		protected internal PriorityQueue pq;
		
		/// <summary>The total number of documents that the collector encountered. </summary>
		protected internal int totalHits;
		
		protected internal TopDocsCollector(PriorityQueue pq)
		{
			this.pq = pq;
		}
		
		/// <summary> Populates the results array with the ScoreDoc instaces. This can be
		/// overridden in case a different ScoreDoc type should be returned.
		/// </summary>
		protected internal virtual void  PopulateResults(ScoreDoc[] results, int howMany)
		{
			for (int i = howMany - 1; i >= 0; i--)
			{
				results[i] = (ScoreDoc) pq.Pop();
			}
		}
		
		/// <summary> Returns a {@link TopDocs} instance containing the given results. If
		/// <code>results</code> is null it means there are no results to return,
		/// either because there were 0 calls to collect() or because the arguments to
		/// topDocs were invalid.
		/// </summary>
		public /*protected internal*/ virtual TopDocs NewTopDocs(ScoreDoc[] results, int start)
		{
			return results == null?EMPTY_TOPDOCS:new TopDocs(totalHits, results);
		}
		
		/// <summary>The total number of documents that matched this query. </summary>
		public virtual int GetTotalHits()
		{
			return totalHits;
		}
		
		/// <summary>Returns the top docs that were collected by this collector. </summary>
		public TopDocs TopDocs()
		{
			// In case pq was populated with sentinel values, there might be less
			// results than pq.size(). Therefore return all results until either
			// pq.size() or totalHits.
			return TopDocs(0, totalHits < pq.Size()?totalHits:pq.Size());
		}
		
		/// <summary> Returns the documents in the rage [start .. pq.size()) that were collected
		/// by this collector. Note that if start >= pq.size(), an empty TopDocs is
		/// returned.<br/>
		/// This method is convenient to call if the application allways asks for the
		/// last results, starting from the last 'page'.<br/>
		/// <b>NOTE:</b> you cannot call this method more than once for each search
		/// execution. If you need to call it more than once, passing each time a
		/// different <code>start</code>, you should call {@link #TopDocs()} and work
		/// with the returned {@link TopDocs} object, which will contain all the
		/// results this search execution collected.
		/// </summary>
		public TopDocs TopDocs(int start)
		{
			// In case pq was populated with sentinel values, there might be less
			// results than pq.size(). Therefore return all results until either
			// pq.size() or totalHits.
			return TopDocs(start, totalHits < pq.Size()?totalHits:pq.Size());
		}
		
		/// <summary> Returns the documents in the rage [start .. start+howMany) that were
		/// collected by this collector. Note that if start >= pq.size(), an empty
		/// TopDocs is returned, and if pq.size() - start &lt; howMany, then only the
		/// available documents in [start .. pq.size()) are returned.<br/>
		/// This method is useful to call in case pagination of search results is
		/// allowed by the search application, as well as it attempts to optimize the
		/// memory used by allocating only as much as requested by howMany.<br/>
		/// <b>NOTE:</b> you cannot call this method more than once for each search
		/// execution. If you need to call it more than once, passing each time a
		/// different range, you should call {@link #TopDocs()} and work with the
		/// returned {@link TopDocs} object, which will contain all the results this
		/// search execution collected.
		/// </summary>
		public TopDocs TopDocs(int start, int howMany)
		{
			
			// In case pq was populated with sentinel values, there might be less
			// results than pq.size(). Therefore return all results until either
			// pq.size() or totalHits.
			int size = totalHits < pq.Size()?totalHits:pq.Size();
			
			// Don't bother to throw an exception, just return an empty TopDocs in case
			// the parameters are invalid or out of range.
			if (start < 0 || start >= size || howMany <= 0)
			{
				return NewTopDocs(null, start);
			}
			
			// We know that start < pqsize, so just fix howMany. 
			howMany = System.Math.Min(size - start, howMany);
			ScoreDoc[] results = new ScoreDoc[howMany];
			
			// pq's pop() returns the 'least' element in the queue, therefore need
			// to discard the first ones, until we reach the requested range.
			// Note that this loop will usually not be executed, since the common usage
			// should be that the caller asks for the last howMany results. However it's
			// needed here for completeness.
			for (int i = pq.Size() - start - howMany; i > 0; i--)
			{
				pq.Pop();
			}
			
			// Get the requested results from pq.
			PopulateResults(results, howMany);
			
			return NewTopDocs(results, start);
		}
	}
}
