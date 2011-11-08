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
using Term = Mono.Lucene.Net.Index.Term;
using PriorityQueue = Mono.Lucene.Net.Util.PriorityQueue;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Implements parallel search over a set of <code>Searchables</code>.
	/// 
	/// <p/>Applications usually need only call the inherited {@link #Search(Query)}
	/// or {@link #Search(Query,Filter)} methods.
	/// </summary>
	public class ParallelMultiSearcher:MultiSearcher
	{
		private class AnonymousClassCollector1:Collector
		{
			public AnonymousClassCollector1(Mono.Lucene.Net.Search.Collector collector, int start, ParallelMultiSearcher enclosingInstance)
			{
				InitBlock(collector, start, enclosingInstance);
			}
			private void  InitBlock(Mono.Lucene.Net.Search.Collector collector, int start, ParallelMultiSearcher enclosingInstance)
			{
				this.collector = collector;
				this.start = start;
				this.enclosingInstance = enclosingInstance;
			}
			private Mono.Lucene.Net.Search.Collector collector;
			private int start;
			private ParallelMultiSearcher enclosingInstance;
			public ParallelMultiSearcher Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  SetScorer(Scorer scorer)
			{
				collector.SetScorer(scorer);
			}
			public override void  Collect(int doc)
			{
				collector.Collect(doc);
			}
			public override void  SetNextReader(IndexReader reader, int docBase)
			{
				collector.SetNextReader(reader, start + docBase);
			}
			public override bool AcceptsDocsOutOfOrder()
			{
				return collector.AcceptsDocsOutOfOrder();
			}
		}
		
		private Searchable[] searchables;
		private int[] starts;
		
		/// <summary>Creates a searchable which searches <i>searchables</i>. </summary>
		public ParallelMultiSearcher(params Searchable[] searchables):base(searchables)
		{
			this.searchables = searchables;
			this.starts = GetStarts();
		}
		
		/// <summary> TODO: parallelize this one too</summary>
		public override int DocFreq(Term term)
		{
			return base.DocFreq(term);
		}
		
		/// <summary> A search implementation which spans a new thread for each
		/// Searchable, waits for each search to complete and merge
		/// the results back together.
		/// </summary>
		public override TopDocs Search(Weight weight, Filter filter, int nDocs)
		{
			HitQueue hq = new HitQueue(nDocs, false);
			int totalHits = 0;
			MultiSearcherThread[] msta = new MultiSearcherThread[searchables.Length];
			for (int i = 0; i < searchables.Length; i++)
			{
				// search each searchable
				// Assume not too many searchables and cost of creating a thread is by far inferior to a search
				msta[i] = new MultiSearcherThread(searchables[i], weight, filter, nDocs, hq, i, starts, "MultiSearcher thread #" + (i + 1));
				msta[i].Start();
			}
			
			for (int i = 0; i < searchables.Length; i++)
			{
				try
				{
					msta[i].Join();
				}
				catch (System.Threading.ThreadInterruptedException ie)
				{
					// In 3.0 we will change this to throw
					// InterruptedException instead
					SupportClass.ThreadClass.Current().Interrupt();
					throw new System.SystemException(ie.Message, ie);
				}
				System.IO.IOException ioe = msta[i].GetIOException();
				if (ioe == null)
				{
					totalHits += msta[i].Hits();
				}
				else
				{
					// if one search produced an IOException, rethrow it
					throw ioe;
				}
			}
			
			ScoreDoc[] scoreDocs = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs[i] = (ScoreDoc) hq.Pop();
			
			float maxScore = (totalHits == 0)?System.Single.NegativeInfinity:scoreDocs[0].score;
			
			return new TopDocs(totalHits, scoreDocs, maxScore);
		}
		
		/// <summary> A search implementation allowing sorting which spans a new thread for each
		/// Searchable, waits for each search to complete and merges
		/// the results back together.
		/// </summary>
		public override TopFieldDocs Search(Weight weight, Filter filter, int nDocs, Sort sort)
		{
			// don't specify the fields - we'll wait to do this until we get results
			FieldDocSortedHitQueue hq = new FieldDocSortedHitQueue(null, nDocs);
			int totalHits = 0;
			MultiSearcherThread[] msta = new MultiSearcherThread[searchables.Length];
			for (int i = 0; i < searchables.Length; i++)
			{
				// search each searchable
				// Assume not too many searchables and cost of creating a thread is by far inferior to a search
				msta[i] = new MultiSearcherThread(searchables[i], weight, filter, nDocs, hq, sort, i, starts, "MultiSearcher thread #" + (i + 1));
				msta[i].Start();
			}
			
			float maxScore = System.Single.NegativeInfinity;
			
			for (int i = 0; i < searchables.Length; i++)
			{
				try
				{
					msta[i].Join();
				}
				catch (System.Threading.ThreadInterruptedException ie)
				{
					// In 3.0 we will change this to throw
					// InterruptedException instead
					SupportClass.ThreadClass.Current().Interrupt();
					throw new System.SystemException(ie.Message, ie);
				}
				System.IO.IOException ioe = msta[i].GetIOException();
				if (ioe == null)
				{
					totalHits += msta[i].Hits();
					maxScore = System.Math.Max(maxScore, msta[i].GetMaxScore());
				}
				else
				{
					// if one search produced an IOException, rethrow it
					throw ioe;
				}
			}
			
			ScoreDoc[] scoreDocs = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs[i] = (ScoreDoc) hq.Pop();
			
			return new TopFieldDocs(totalHits, scoreDocs, hq.GetFields(), maxScore);
		}
		
		/// <summary>Lower-level search API.
		/// 
		/// <p/>{@link Collector#Collect(int)} is called for every matching document.
		/// 
		/// <p/>Applications should only use this if they need <i>all</i> of the
		/// matching documents.  The high-level search API ({@link
		/// Searcher#Search(Query)}) is usually more efficient, as it skips
		/// non-high-scoring hits.
		/// 
		/// </summary>
		/// <param name="weight">to match documents
		/// </param>
		/// <param name="filter">if non-null, a bitset used to eliminate some documents
		/// </param>
		/// <param name="collector">to receive hits
		/// 
		/// TODO: parallelize this one too
		/// </param>
		public override void  Search(Weight weight, Filter filter, Collector collector)
		{
			for (int i = 0; i < searchables.Length; i++)
			{
				
				int start = starts[i];
				
				Collector hc = new AnonymousClassCollector1(collector, start, this);
				
				searchables[i].Search(weight, filter, hc);
			}
		}
		
		/*
		* TODO: this one could be parallelized too
		* @see Mono.Lucene.Net.Search.Searchable#rewrite(Mono.Lucene.Net.Search.Query)
		*/
		public override Query Rewrite(Query original)
		{
			return base.Rewrite(original);
		}
	}
	
	/// <summary> A thread subclass for searching a single searchable </summary>
	class MultiSearcherThread:SupportClass.ThreadClass
	{
		
		private Searchable searchable;
		private Weight weight;
		private Filter filter;
		private int nDocs;
		private TopDocs docs;
		private int i;
		private PriorityQueue hq;
		private int[] starts;
		private System.Exception ioe;
		private Sort sort;
		
		public MultiSearcherThread(Searchable searchable, Weight weight, Filter filter, int nDocs, HitQueue hq, int i, int[] starts, System.String name):base(name)
		{
			this.searchable = searchable;
			this.weight = weight;
			this.filter = filter;
			this.nDocs = nDocs;
			this.hq = hq;
			this.i = i;
			this.starts = starts;
		}
		
		public MultiSearcherThread(Searchable searchable, Weight weight, Filter filter, int nDocs, FieldDocSortedHitQueue hq, Sort sort, int i, int[] starts, System.String name):base(name)
		{
			this.searchable = searchable;
			this.weight = weight;
			this.filter = filter;
			this.nDocs = nDocs;
			this.hq = hq;
			this.i = i;
			this.starts = starts;
			this.sort = sort;
		}
		
		override public void  Run()
		{
			try
			{
				docs = (sort == null)?searchable.Search(weight, filter, nDocs):searchable.Search(weight, filter, nDocs, sort);
			}
			// Store the IOException for later use by the caller of this thread
			catch (System.Exception e)
			{
				this.ioe = e;
			}
			if (this.ioe == null)
			{
				// if we are sorting by fields, we need to tell the field sorted hit queue
				// the actual type of fields, in case the original list contained AUTO.
				// if the searchable returns null for fields, we'll have problems.
				if (sort != null)
				{
					TopFieldDocs docsFields = (TopFieldDocs) docs;
					// If one of the Sort fields is FIELD_DOC, need to fix its values, so that
					// it will break ties by doc Id properly. Otherwise, it will compare to
					// 'relative' doc Ids, that belong to two different searchables.
					for (int j = 0; j < docsFields.fields.Length; j++)
					{
						if (docsFields.fields[j].GetType() == SortField.DOC)
						{
							// iterate over the score docs and change their fields value
							for (int j2 = 0; j2 < docs.ScoreDocs.Length; j2++)
							{
								FieldDoc fd = (FieldDoc) docs.ScoreDocs[j2];
								fd.fields[j] = (System.Int32) (((System.Int32) fd.fields[j]) + starts[i]);
							}
							break;
						}
					}
					
					((FieldDocSortedHitQueue) hq).SetFields(docsFields.fields);
				}
				ScoreDoc[] scoreDocs = docs.ScoreDocs;
				for (int j = 0; j < scoreDocs.Length; j++)
				{
					// merge scoreDocs into hq
					ScoreDoc scoreDoc = scoreDocs[j];
					scoreDoc.doc += starts[i]; // convert doc 
					//it would be so nice if we had a thread-safe insert 
					lock (hq)
					{
						if (!hq.Insert(scoreDoc))
							break;
					} // no more scores > minScore
				}
			}
		}
		
		public virtual int Hits()
		{
			return docs.TotalHits;
		}
		
		public virtual float GetMaxScore()
		{
			return docs.GetMaxScore();
		}
		
		public virtual System.IO.IOException GetIOException()
		{
            if (ioe == null) return null;
            return new System.IO.IOException(ioe.Message);
		}
	}
}
