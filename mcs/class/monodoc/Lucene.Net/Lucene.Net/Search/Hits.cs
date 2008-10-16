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
using Document = Monodoc.Lucene.Net.Documents.Document;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>A ranked list of documents, used to hold search results. </summary>
	public sealed class Hits
	{
		private Query query;
		private Searcher searcher;
		private Filter filter = null;
		private Sort sort = null;
		
		private int length; // the total number of hits
		private System.Collections.ArrayList hitDocs = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10)); // cache of hits retrieved
		
		private HitDoc first; // head of LRU cache
		private HitDoc last; // tail of LRU cache
		private int numDocs = 0; // number cached
		private int maxDocs = 200; // max to cache
		
		internal Hits(Searcher s, Query q, Filter f)
		{
			query = q;
			searcher = s;
			filter = f;
			GetMoreDocs(50); // retrieve 100 initially
		}
		
		internal Hits(Searcher s, Query q, Filter f, Sort o)
		{
			query = q;
			searcher = s;
			filter = f;
			sort = o;
			GetMoreDocs(50); // retrieve 100 initially
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
			TopDocs topDocs = (sort == null) ? searcher.Search(query, filter, n) : searcher.Search(query, filter, n, sort);
			length = topDocs.totalHits;
			ScoreDoc[] scoreDocs = topDocs.scoreDocs;
			
			float scoreNorm = 1.0f;
			if (length > 0 && scoreDocs[0].score > 1.0f)
			{
				scoreNorm = 1.0f / scoreDocs[0].score;
			}
			
			int end = scoreDocs.Length < length?scoreDocs.Length:length;
			for (int i = hitDocs.Count; i < end; i++)
			{
				hitDocs.Add(new HitDoc(scoreDocs[i].score * scoreNorm, scoreDocs[i].doc));
			}
		}
		
		/// <summary>Returns the total number of hits available in this set. </summary>
		public int Length()
		{
			return length;
		}
		
		/// <summary>Returns the stored fields of the n<sup>th</sup> document in this set.
		/// <p>Documents are cached, so that repeated requests for the same element may
		/// return the same Document object. 
		/// </summary>
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
		
		/// <summary>Returns the score for the nth document in this set. </summary>
		public float Score(int n)
		{
			return HitDoc(n).score;
		}
		
		/// <summary>Returns the id for the nth document in this set. </summary>
		public int Id(int n)
		{
			return HitDoc(n).id;
		}
		
		
		private HitDoc HitDoc(int n)
		{
			if (n >= length)
			{
				throw new System.IndexOutOfRangeException("Not a valid hit number: " + n);
			}
			
			if (n >= hitDocs.Count)
			{
				GetMoreDocs(n);
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