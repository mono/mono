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
using Monodoc.Lucene.Net.Index;
using Term = Monodoc.Lucene.Net.Index.Term;
using Directory = Monodoc.Lucene.Net.Store.Directory;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary>Implements search over a single Monodoc.Lucene.Net.Index.IndexReader.
	/// 
	/// <p>Applications usually need only call the inherited {@link #Search(Query)}
	/// or {@link #Search(Query,Filter)} methods.
	/// </summary>
	public class IndexSearcher : Searcher
	{
		private class AnonymousClassHitCollector : HitCollector
		{
			public AnonymousClassHitCollector(System.Collections.BitArray bits, int[] totalHits, Monodoc.Lucene.Net.Search.HitQueue hq, int nDocs, IndexSearcher enclosingInstance)
			{
				InitBlock(bits, totalHits, hq, nDocs, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bits, int[] totalHits, Monodoc.Lucene.Net.Search.HitQueue hq, int nDocs, IndexSearcher enclosingInstance)
			{
				this.bits = bits;
				this.totalHits = totalHits;
				this.hq = hq;
                this.nDocs = nDocs;
                this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bits;
			private int[] totalHits;
			private Monodoc.Lucene.Net.Search.HitQueue hq;
            private int nDocs;
			private IndexSearcher enclosingInstance;
			public IndexSearcher Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
            private float minScore = 0.0f;
			public override void  Collect(int doc, float score)
			{
				if (score > 0.0f && (bits == null || bits.Get(doc)))
				{
					// skip docs not in bits
					totalHits[0]++;
                    if (hq.Size() < nDocs || score >= minScore)
                    {
                        hq.Insert(new ScoreDoc(doc, score));
                        minScore = ((ScoreDoc) hq.Top()).score; // maintain minScore
                    }
                }
			}
		}
		private class AnonymousClassHitCollector1 : HitCollector
		{
			public AnonymousClassHitCollector1(System.Collections.BitArray bits, int[] totalHits, Monodoc.Lucene.Net.Search.FieldSortedHitQueue hq, IndexSearcher enclosingInstance)
			{
				InitBlock(bits, totalHits, hq, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bits, int[] totalHits, Monodoc.Lucene.Net.Search.FieldSortedHitQueue hq, IndexSearcher enclosingInstance)
			{
				this.bits = bits;
				this.totalHits = totalHits;
				this.hq = hq;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bits;
			private int[] totalHits;
			private Monodoc.Lucene.Net.Search.FieldSortedHitQueue hq;
			private IndexSearcher enclosingInstance;
			public IndexSearcher Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  Collect(int doc, float score)
			{
				if (score > 0.0f && (bits == null || bits.Get(doc)))
				{
					// skip docs not in bits
					totalHits[0]++;
					hq.Insert(new FieldDoc(doc, score));
				}
			}
		}
		private class AnonymousClassHitCollector2 : HitCollector
		{
			public AnonymousClassHitCollector2(System.Collections.BitArray bits, Monodoc.Lucene.Net.Search.HitCollector results, IndexSearcher enclosingInstance)
			{
				InitBlock(bits, results, enclosingInstance);
			}
			private void  InitBlock(System.Collections.BitArray bits, Monodoc.Lucene.Net.Search.HitCollector results, IndexSearcher enclosingInstance)
			{
				this.bits = bits;
				this.results = results;
				this.enclosingInstance = enclosingInstance;
			}
			private System.Collections.BitArray bits;
			private Monodoc.Lucene.Net.Search.HitCollector results;
			private IndexSearcher enclosingInstance;
			public IndexSearcher Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			public override void  Collect(int doc, float score)
			{
				if (bits.Get(doc))
				{
					// skip docs not in bits
					results.Collect(doc, score);
				}
			}
		}
		public /*internal*/ Monodoc.Lucene.Net.Index.IndexReader reader;
		private bool closeReader;
		
		/// <summary>Creates a searcher searching the index in the named directory. </summary>
		public IndexSearcher(System.String path) : this(Monodoc.Lucene.Net.Index.IndexReader.Open(path), true)
		{
		}
		
		/// <summary>Creates a searcher searching the index in the provided directory. </summary>
		public IndexSearcher(Directory directory) : this(Monodoc.Lucene.Net.Index.IndexReader.Open(directory), true)
		{
		}
		
		/// <summary>Creates a searcher searching the provided index. </summary>
		public IndexSearcher(Monodoc.Lucene.Net.Index.IndexReader r) : this(r, false)
		{
		}
		
		private IndexSearcher(Monodoc.Lucene.Net.Index.IndexReader r, bool closeReader)
		{
			reader = r;
			this.closeReader = closeReader;
		}
		
		/// <summary> Note that the underlying Monodoc.Lucene.Net.Index.IndexReader is not closed, if
		/// IndexSearcher was constructed with IndexSearcher(Monodoc.Lucene.Net.Index.IndexReader r).
		/// If the Monodoc.Lucene.Net.Index.IndexReader was supplied implicitly by specifying a directory, then
		/// the Monodoc.Lucene.Net.Index.IndexReader gets closed.
		/// </summary>
		public override void Close()
		{
			if (closeReader)
				reader.Close();
		}
		
		// inherit javadoc
		public override int DocFreq(Term term)
		{
			return reader.DocFreq(term);
		}
		
		// inherit javadoc
		public override Document Doc(int i)
		{
			return reader.Document(i);
		}
		
		// inherit javadoc
		public override int MaxDoc()
		{
			return reader.MaxDoc();
		}
		
		// inherit javadoc
		public override TopDocs Search(Query query, Filter filter, int nDocs)
		{
			Scorer scorer = query.Weight(this).Scorer(reader);
			if (scorer == null)
				return new TopDocs(0, new ScoreDoc[0]);
			
			System.Collections.BitArray bits = filter != null ? filter.Bits(reader) : null;
			HitQueue hq = new HitQueue(nDocs);
			int[] totalHits = new int[1];
			scorer.Score(new AnonymousClassHitCollector(bits, totalHits, hq, nDocs, this));
			
			ScoreDoc[] scoreDocs = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs[i] = (ScoreDoc) hq.Pop();
			
			return new TopDocs(totalHits[0], scoreDocs);
		}
		
		// inherit javadoc
		public override TopFieldDocs Search(Query query, Filter filter, int nDocs, Sort sort)
		{
			Scorer scorer = query.Weight(this).Scorer(reader);
			if (scorer == null)
				return new TopFieldDocs(0, new ScoreDoc[0], sort.fields);
			
			System.Collections.BitArray bits = filter != null ? filter.Bits(reader) : null;
			FieldSortedHitQueue hq = new FieldSortedHitQueue(reader, sort.fields, nDocs);
			int[] totalHits = new int[1];
			scorer.Score(new AnonymousClassHitCollector1(bits, totalHits, hq, this));
			
			ScoreDoc[] scoreDocs = new ScoreDoc[hq.Size()];
			for (int i = hq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs[i] = hq.FillFields((FieldDoc) hq.Pop());
			
			return new TopFieldDocs(totalHits[0], scoreDocs, hq.GetFields());
		}
		
		
		// inherit javadoc
		public override void  Search(Query query, Filter filter, HitCollector results)
		{
			HitCollector collector = results;
			if (filter != null)
			{
				System.Collections.BitArray bits = filter.Bits(reader);
				collector = new AnonymousClassHitCollector2(bits, results, this);
			}
			
			Scorer scorer = query.Weight(this).Scorer(reader);
			if (scorer == null)
				return ;
			scorer.Score(collector);
		}
		
		public override Query Rewrite(Query original)
		{
			Query query = original;
			for (Query rewrittenQuery = query.Rewrite(reader); rewrittenQuery != query; rewrittenQuery = query.Rewrite(reader))
			{
				query = rewrittenQuery;
			}
			return query;
		}
		
		public override Explanation Explain(Query query, int doc)
		{
			return query.Weight(this).Explain(reader, doc);
		}
	}
}