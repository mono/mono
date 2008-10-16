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
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Implements search over a set of <code>Searchables</code>.
	/// 
	/// <p>Applications usually need only call the inherited {@link #Search(Query)}
	/// or {@link #Search(Query,Filter)} methods.
	/// </summary>
	public class MultiSearcher : Searcher
	{
		private class AnonymousClassHitCollector : HitCollector
		{
			public AnonymousClassHitCollector(Monodoc.Lucene.Net.Search.HitCollector results, int start, MultiSearcher enclosingInstance)
			{
				InitBlock(results, start, enclosingInstance);
			}
			private void  InitBlock(Monodoc.Lucene.Net.Search.HitCollector results, int start, MultiSearcher enclosingInstance)
			{
				this.results = results;
				this.start = start;
				this.enclosingInstance = enclosingInstance;
			}
			private Monodoc.Lucene.Net.Search.HitCollector results;
			private int start;
			private MultiSearcher enclosingInstance;
			public MultiSearcher Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  Collect(int doc, float score)
			{
				results.Collect(doc + start, score);
			}
		}
		private Monodoc.Lucene.Net.Search.Searchable[] searchables;
		private int[] starts;
		private int maxDoc = 0;
		
		/// <summary>Creates a searcher which searches <i>searchables</i>. </summary>
		public MultiSearcher(Monodoc.Lucene.Net.Search.Searchable[] searchables)
		{
			this.searchables = searchables;
			
			starts = new int[searchables.Length + 1]; // build starts array
			for (int i = 0; i < searchables.Length; i++)
			{
				starts[i] = maxDoc;
				maxDoc += searchables[i].MaxDoc(); // compute maxDocs
			}
			starts[searchables.Length] = maxDoc;
		}
		
		protected internal virtual int[] GetStarts()
		{
			return starts;
		}
		
		// inherit javadoc
		public override void  Close()
		{
			for (int i = 0; i < searchables.Length; i++)
				searchables[i].Close();
		}
		
		public override int DocFreq(Term term)
		{
			int docFreq = 0;
			for (int i = 0; i < searchables.Length; i++)
				docFreq += searchables[i].DocFreq(term);
			return docFreq;
		}
		
		// inherit javadoc
		public override Document Doc(int n)
		{
			int i = SubSearcher(n); // find searcher index
			return searchables[i].Doc(n - starts[i]); // dispatch to searcher
		}
		
		/// <summary>Call {@link #subSearcher} instead.</summary>
		/// <deprecated>
		/// </deprecated>
		public virtual int SearcherIndex(int n)
		{
			return SubSearcher(n);
		}
		
		/// <summary>Returns index of the searcher for document <code>n</code> in the array
		/// used to construct this searcher. 
		/// </summary>
		public virtual int SubSearcher(int n)
		{
			// find searcher for doc n:
			// replace w/ call to Arrays.binarySearch in Java 1.2
			int lo = 0; // search starts array
			int hi = searchables.Length - 1; // for first element less
			// than n, return its index
			while (hi >= lo)
			{
				int mid = (lo + hi) >> 1;
				int midValue = starts[mid];
				if (n < midValue)
					hi = mid - 1;
				else if (n > midValue)
					lo = mid + 1;
				else
				{
					// found a match
					while (mid + 1 < searchables.Length && starts[mid + 1] == midValue)
					{
						mid++; // scan to last match
					}
					return mid;
				}
			}
			return hi;
		}
		
		/// <summary>Returns the document number of document <code>n</code> within its
		/// sub-index. 
		/// </summary>
		public virtual int SubDoc(int n)
		{
			return n - starts[SubSearcher(n)];
		}
		
		public override int MaxDoc()
		{
			return maxDoc;
		}
		
		public override TopDocs Search(Query query, Filter filter, int nDocs)
		{
			HitQueue hq = new HitQueue(nDocs);
			int totalHits = 0;
			
			for (int i = 0; i < searchables.Length; i++)
			{
				// search each searcher
				TopDocs docs = searchables[i].Search(query, filter, nDocs);
				totalHits += docs.totalHits; // update totalHits
				ScoreDoc[] scoreDocs = docs.scoreDocs;
				for (int j = 0; j < scoreDocs.Length; j++)
				{
					// merge scoreDocs into hq
					ScoreDoc scoreDoc = scoreDocs[j];
					scoreDoc.doc += starts[i]; // convert doc
					if (!hq.Insert(scoreDoc))
						break; // no more scores > minScore
				}
			}
			
			ScoreDoc[] scoreDocs2 = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs2[i] = (ScoreDoc) hq.Pop();
			
			return new TopDocs(totalHits, scoreDocs2);
		}
		
		
		public override TopFieldDocs Search(Query query, Filter filter, int n, Sort sort)
		{
			FieldDocSortedHitQueue hq = null;
			int totalHits = 0;
			
			for (int i = 0; i < searchables.Length; i++)
			{
				// search each searcher
				TopFieldDocs docs = searchables[i].Search(query, filter, n, sort);
				if (hq == null)
					hq = new FieldDocSortedHitQueue(docs.fields, n);
				totalHits += docs.totalHits; // update totalHits
				ScoreDoc[] scoreDocs = docs.scoreDocs;
				for (int j = 0; j < scoreDocs.Length; j++)
				{
					// merge scoreDocs into hq
					ScoreDoc scoreDoc = scoreDocs[j];
					scoreDoc.doc += starts[i]; // convert doc
					if (!hq.Insert(scoreDoc))
						break; // no more scores > minScore
				}
			}
			
			ScoreDoc[] scoreDocs2 = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs2[i] = (ScoreDoc) hq.Pop();
			
			return new TopFieldDocs(totalHits, scoreDocs2, hq.GetFields());
		}
		
		
		// inherit javadoc
		public override void  Search(Query query, Filter filter, HitCollector results)
		{
			for (int i = 0; i < searchables.Length; i++)
			{
				
				int start = starts[i];
				
				searchables[i].Search(query, filter, new AnonymousClassHitCollector(results, start, this));
			}
		}
		
		public override Query Rewrite(Query original)
		{
			Query[] queries = new Query[searchables.Length];
			for (int i = 0; i < searchables.Length; i++)
			{
				queries[i] = searchables[i].Rewrite(original);
			}
			return original.Combine(queries);
		}
		
		public override Explanation Explain(Query query, int doc)
		{
			int i = SubSearcher(doc); // find searcher index
			return searchables[i].Explain(query, doc - starts[i]); // dispatch to searcher
		}
	}
}