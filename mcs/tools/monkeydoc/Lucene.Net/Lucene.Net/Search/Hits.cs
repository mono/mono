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

using Document = Mono.Lucene.Net.Documents.Document;
using CorruptIndexException = Mono.Lucene.Net.Index.CorruptIndexException;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A ranked list of documents, used to hold search results.
	/// <p/>
	/// <b>Caution:</b> Iterate only over the hits needed. Iterating over all hits is
	/// generally not desirable and may be the source of performance issues. If you
	/// need to iterate over many or all hits, consider using the search method that
	/// takes a {@link HitCollector}.
	/// <p/>
	/// <p/>
	/// <b>Note:</b> Deleting matching documents concurrently with traversing the
	/// hits, might, when deleting hits that were not yet retrieved, decrease
	/// {@link #Length()}. In such case,
	/// {@link java.util.ConcurrentModificationException
	/// ConcurrentModificationException} is thrown when accessing hit <code>n</code>
	/// &gt; current_{@link #Length()} (but <code>n</code> &lt; {@link #Length()}
	/// _at_start).
	/// 
	/// </summary>
	/// <deprecated> see {@link Searcher#Search(Query, int)},
	/// {@link Searcher#Search(Query, Filter, int)} and
	/// {@link Searcher#Search(Query, Filter, int, Sort)}:<br/>
	/// 
	/// <pre>
	/// TopDocs topDocs = searcher.Search(query, numHits);
	/// ScoreDoc[] hits = topDocs.scoreDocs;
	/// for (int i = 0; i &lt; hits.Length; i++) {
	/// int docId = hits[i].doc;
	/// Document d = searcher.Doc(docId);
	/// // do something with current hit
	/// ...
	/// </pre>
	/// </deprecated>
    [Obsolete("see Searcher.Search(Query, int), Searcher.Search(Query, Filter, int) and Searcher.Search(Query, Filter, int, Sort)")]
	public sealed class Hits
	{
		private Weight weight;
		private Searcher searcher;
		private Filter filter = null;
		private Sort sort = null;
		
		private int length; // the total number of hits
		private System.Collections.ArrayList hitDocs = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // cache of hits retrieved
		
		private HitDoc first; // head of LRU cache
		private HitDoc last; // tail of LRU cache
		private int numDocs = 0; // number cached
		private int maxDocs = 200; // max to cache
		
		private int nDeletions; // # deleted docs in the index.    
		private int lengthAtStart; // this is the number apps usually count on (although deletions can bring it down). 
		private int nDeletedHits = 0; // # of already collected hits that were meanwhile deleted.
		
		public /*internal*/ bool debugCheckedForDeletions = false; // for test purposes.
		
		internal Hits(Searcher s, Query q, Filter f)
		{
			weight = q.Weight(s);
			searcher = s;
			filter = f;
			nDeletions = CountDeletions(s);
			GetMoreDocs(50); // retrieve 100 initially
			lengthAtStart = length;
		}
		
		internal Hits(Searcher s, Query q, Filter f, Sort o)
		{
			weight = q.Weight(s);
			searcher = s;
			filter = f;
			sort = o;
			nDeletions = CountDeletions(s);
			GetMoreDocs(50); // retrieve 100 initially
			lengthAtStart = length;
		}
		
		// count # deletions, return -1 if unknown.
		private int CountDeletions(Searcher s)
		{
			int cnt = - 1;
			if (s is IndexSearcher)
			{
				cnt = s.MaxDoc() - ((IndexSearcher) s).GetIndexReader().NumDocs();
			}
			return cnt;
		}
		
		/// <summary> Tries to add new documents to hitDocs.
		/// Ensures that the hit numbered <code>min</code> has been retrieved.
		/// </summary>
		private void  GetMoreDocs(int min)
		{
			if (hitDocs.Count > min)
			{
				min = hitDocs.Count;
			}
			
			int n = min * 2; // double # retrieved
			TopDocs topDocs = (sort == null)?searcher.Search(weight, filter, n):searcher.Search(weight, filter, n, sort);
			
			length = topDocs.TotalHits;
			ScoreDoc[] scoreDocs = topDocs.ScoreDocs;
			
			float scoreNorm = 1.0f;
			
			if (length > 0 && topDocs.GetMaxScore() > 1.0f)
			{
				scoreNorm = 1.0f / topDocs.GetMaxScore();
			}
			
			int start = hitDocs.Count - nDeletedHits;
			
			// any new deletions?
			int nDels2 = CountDeletions(searcher);
			debugCheckedForDeletions = false;
			if (nDeletions < 0 || nDels2 > nDeletions)
			{
				// either we cannot count deletions, or some "previously valid hits" might have been deleted, so find exact start point
				nDeletedHits = 0;
				debugCheckedForDeletions = true;
				int i2 = 0;
				for (int i1 = 0; i1 < hitDocs.Count && i2 < scoreDocs.Length; i1++)
				{
					int id1 = ((HitDoc) hitDocs[i1]).id;
					int id2 = scoreDocs[i2].doc;
					if (id1 == id2)
					{
						i2++;
					}
					else
					{
						nDeletedHits++;
					}
				}
				start = i2;
			}
			
			int end = scoreDocs.Length < length?scoreDocs.Length:length;
			length += nDeletedHits;
			for (int i = start; i < end; i++)
			{
				hitDocs.Add(new HitDoc(scoreDocs[i].score * scoreNorm, scoreDocs[i].doc));
			}
			
			nDeletions = nDels2;
		}
		
		/// <summary>Returns the total number of hits available in this set. </summary>
		public int Length()
		{
			return length;
		}
		
		/// <summary>Returns the stored fields of the n<sup>th</sup> document in this set.
		/// <p/>Documents are cached, so that repeated requests for the same element may
		/// return the same Document object.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		public Document Doc(int n)
		{
			HitDoc hitDoc = HitDoc(n);
			
			// Update LRU cache of documents
			Remove(hitDoc); // remove from list, if there
			AddToFront(hitDoc); // add to front of list
			if (numDocs > maxDocs)
			{
				// if cache is full
				HitDoc oldLast = last;
				Remove(last); // flush last
				oldLast.doc = null; // let doc get gc'd
			}
			
			if (hitDoc.doc == null)
			{
				hitDoc.doc = searcher.Doc(hitDoc.id); // cache miss: read document
			}
			
			return hitDoc.doc;
		}
		
		/// <summary>Returns the score for the n<sup>th</sup> document in this set. </summary>
		public float Score(int n)
		{
			return HitDoc(n).score;
		}
		
		/// <summary>Returns the id for the n<sup>th</sup> document in this set.
		/// Note that ids may change when the index changes, so you cannot
		/// rely on the id to be stable.
		/// </summary>
		public int Id(int n)
		{
			return HitDoc(n).id;
		}
		
		/// <summary> Returns a {@link HitIterator} to navigate the Hits.  Each item returned
		/// from {@link Iterator#next()} is a {@link Hit}.
		/// <p/>
		/// <b>Caution:</b> Iterate only over the hits needed.  Iterating over all
		/// hits is generally not desirable and may be the source of
		/// performance issues. If you need to iterate over many or all hits, consider
		/// using a search method that takes a {@link HitCollector}.
		/// <p/>
		/// </summary>
		public System.Collections.IEnumerator Iterator()
		{
			return new HitIterator(this);
		}
		
		private HitDoc HitDoc(int n)
		{
			if (n >= lengthAtStart)
			{
				throw new System.IndexOutOfRangeException("Not a valid hit number: " + n);
			}
			
			if (n >= hitDocs.Count)
			{
				GetMoreDocs(n);
			}
			
			if (n >= length)
			{
				throw new System.Exception("Not a valid hit number: " + n);
			}
			
			return (HitDoc) hitDocs[n];
		}
		
		private void  AddToFront(HitDoc hitDoc)
		{
			// insert at front of cache
			if (first == null)
			{
				last = hitDoc;
			}
			else
			{
				first.prev = hitDoc;
			}
			
			hitDoc.next = first;
			first = hitDoc;
			hitDoc.prev = null;
			
			numDocs++;
		}
		
		private void  Remove(HitDoc hitDoc)
		{
			// remove from cache
			if (hitDoc.doc == null)
			{
				// it's not in the list
				return ; // abort
			}
			
			if (hitDoc.next == null)
			{
				last = hitDoc.prev;
			}
			else
			{
				hitDoc.next.prev = hitDoc.prev;
			}
			
			if (hitDoc.prev == null)
			{
				first = hitDoc.next;
			}
			else
			{
				hitDoc.prev.next = hitDoc.next;
			}
			
			numDocs--;
		}
	}
	
	sealed class HitDoc
	{
		internal float score;
		internal int id;
		internal Document doc = null;
		
		internal HitDoc next; // in doubly-linked cache
		internal HitDoc prev; // in doubly-linked cache
		
		internal HitDoc(float s, int i)
		{
			score = s;
			id = i;
		}
	}
}
