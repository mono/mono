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
using FieldSelector = Mono.Lucene.Net.Documents.FieldSelector;
using CorruptIndexException = Mono.Lucene.Net.Index.CorruptIndexException;
using IndexReader = Mono.Lucene.Net.Index.IndexReader;
using Term = Mono.Lucene.Net.Index.Term;
using Directory = Mono.Lucene.Net.Store.Directory;
using ReaderUtil = Mono.Lucene.Net.Util.ReaderUtil;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary>Implements search over a single IndexReader.
	/// 
	/// <p/>Applications usually need only call the inherited {@link #Search(Query)}
	/// or {@link #Search(Query,Filter)} methods. For performance reasons it is 
	/// recommended to open only one IndexSearcher and use it for all of your searches.
	/// 
	/// <p/>Note that you can only access Hits from an IndexSearcher as long as it is
	/// not yet closed, otherwise an IOException will be thrown. 
	/// 
	/// <a name="thread-safety"></a><p/><b>NOTE</b>: {@link
	/// <code>IndexSearcher</code>} instances are completely
	/// thread safe, meaning multiple threads can call any of its
	/// methods, concurrently.  If your application requires
	/// external synchronization, you should <b>not</b>
	/// synchronize on the <code>IndexSearcher</code> instance;
	/// use your own (non-Lucene) objects instead.<p/>
	/// </summary>
    [Serializable]
	public class IndexSearcher:Searcher
	{
		internal IndexReader reader;
		private bool closeReader;
		private IndexReader[] subReaders;
		private int[] docStarts;
		
		/// <summary>Creates a searcher searching the index in the named directory.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #IndexSearcher(Directory, boolean)} instead
		/// </deprecated>
        [Obsolete("Use IndexSearcher(Directory, bool) instead")]
		public IndexSearcher(System.String path):this(IndexReader.Open(path), true)
		{
		}
		
		/// <summary>Creates a searcher searching the index in the named
		/// directory.  You should pass readOnly=true, since it
		/// gives much better concurrent performance, unless you
		/// intend to do write operations (delete documents or
		/// change norms) with the underlying IndexReader.
		/// </summary>
		/// <param name="path">directory where IndexReader will be opened
		/// </param>
		/// <param name="readOnly">if true, the underlying IndexReader
		/// will be opened readOnly
		/// </param>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #IndexSearcher(Directory, boolean)} instead
		/// </deprecated>
        [Obsolete("Use IndexSearcher(Directory, bool) instead")]
		public IndexSearcher(System.String path, bool readOnly):this(IndexReader.Open(path, readOnly), true)
		{
		}
		
		/// <summary>Creates a searcher searching the index in the provided directory.</summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <deprecated> Use {@link #IndexSearcher(Directory, boolean)} instead
		/// </deprecated>
        [Obsolete("Use IndexSearcher(Directory, bool) instead")]
		public IndexSearcher(Directory directory):this(IndexReader.Open(directory), true)
		{
		}
		
		/// <summary>Creates a searcher searching the index in the named
		/// directory.  You should pass readOnly=true, since it
		/// gives much better concurrent performance, unless you
		/// intend to do write operations (delete documents or
		/// change norms) with the underlying IndexReader.
		/// </summary>
		/// <throws>  CorruptIndexException if the index is corrupt </throws>
		/// <throws>  IOException if there is a low-level IO error </throws>
		/// <param name="path">directory where IndexReader will be opened
		/// </param>
		/// <param name="readOnly">if true, the underlying IndexReader
		/// will be opened readOnly
		/// </param>
		public IndexSearcher(Directory path, bool readOnly):this(IndexReader.Open(path, readOnly), true)
		{
		}
		
		/// <summary>Creates a searcher searching the provided index. </summary>
		public IndexSearcher(IndexReader r):this(r, false)
		{
		}
		
		private IndexSearcher(IndexReader r, bool closeReader)
		{
			reader = r;
			this.closeReader = closeReader;
			
			System.Collections.IList subReadersList = new System.Collections.ArrayList();
			GatherSubReaders(subReadersList, reader);
            subReaders = (IndexReader[])new System.Collections.ArrayList(subReadersList).ToArray(typeof(IndexReader));
			docStarts = new int[subReaders.Length];
			int maxDoc = 0;
			for (int i = 0; i < subReaders.Length; i++)
			{
				docStarts[i] = maxDoc;
				maxDoc += subReaders[i].MaxDoc();
			}
		}
		
		protected internal virtual void  GatherSubReaders(System.Collections.IList allSubReaders, IndexReader r)
		{
			ReaderUtil.GatherSubReaders(allSubReaders, r);
		}
		
		/// <summary>Return the {@link IndexReader} this searches. </summary>
		public virtual IndexReader GetIndexReader()
		{
			return reader;
		}
		
		/// <summary> Note that the underlying IndexReader is not closed, if
		/// IndexSearcher was constructed with IndexSearcher(IndexReader r).
		/// If the IndexReader was supplied implicitly by specifying a directory, then
		/// the IndexReader gets closed.
		/// </summary>
		public override void  Close()
		{
			if (closeReader)
				reader.Close();
		}

        /// <summary>
        /// .NET
        /// </summary>
        public override void Dispose()
        {
            Close();
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
		public override Document Doc(int i, FieldSelector fieldSelector)
		{
			return reader.Document(i, fieldSelector);
		}
		
		// inherit javadoc
		public override int MaxDoc()
		{
			return reader.MaxDoc();
		}
		
		// inherit javadoc
		public override TopDocs Search(Weight weight, Filter filter, int nDocs)
		{
			
			if (nDocs <= 0)
			{
				throw new System.ArgumentException("nDocs must be > 0");
			}
            nDocs = System.Math.Min(nDocs, reader.MaxDoc());

			TopScoreDocCollector collector = TopScoreDocCollector.create(nDocs, !weight.ScoresDocsOutOfOrder());
			Search(weight, filter, collector);
			return collector.TopDocs();
		}
		
		public override TopFieldDocs Search(Weight weight, Filter filter, int nDocs, Sort sort)
		{
			return Search(weight, filter, nDocs, sort, true);
		}
		
		/// <summary> Just like {@link #Search(Weight, Filter, int, Sort)}, but you choose
		/// whether or not the fields in the returned {@link FieldDoc} instances
		/// should be set by specifying fillFields.<br/>
		/// 
		/// <p/>
		/// NOTE: this does not compute scores by default. If you need scores, create
		/// a {@link TopFieldCollector} instance by calling
		/// {@link TopFieldCollector#create} and then pass that to
		/// {@link #Search(Weight, Filter, Collector)}.
		/// <p/>
		/// </summary>
		public virtual TopFieldDocs Search(Weight weight, Filter filter, int nDocs, Sort sort, bool fillFields)
		{
            nDocs = System.Math.Min(nDocs, reader.MaxDoc());

			SortField[] fields = sort.fields;
			bool legacy = false;
			for (int i = 0; i < fields.Length; i++)
			{
				SortField field = fields[i];
				System.String fieldname = field.GetField();
				int type = field.GetType();
				// Resolve AUTO into its true type
				if (type == SortField.AUTO)
				{
					int autotype = SortField.DetectFieldType(reader, fieldname);
					if (autotype == SortField.STRING)
					{
						fields[i] = new SortField(fieldname, field.GetLocale(), field.GetReverse());
					}
					else
					{
						fields[i] = new SortField(fieldname, autotype, field.GetReverse());
					}
				}
				
				if (field.GetUseLegacySearch())
				{
					legacy = true;
				}
			}
			
			if (legacy)
			{
				// Search the single top-level reader
				TopDocCollector collector = new TopFieldDocCollector(reader, sort, nDocs);
				HitCollectorWrapper hcw = new HitCollectorWrapper(collector);
				hcw.SetNextReader(reader, 0);
				if (filter == null)
				{
					Scorer scorer = weight.Scorer(reader, true, true);
					if (scorer != null)
					{
						scorer.Score(hcw);
					}
				}
				else
				{
					SearchWithFilter(reader, weight, filter, hcw);
				}
				return (TopFieldDocs) collector.TopDocs();
			}
			
			TopFieldCollector collector2 = TopFieldCollector.create(sort, nDocs, fillFields, fieldSortDoTrackScores, fieldSortDoMaxScore, !weight.ScoresDocsOutOfOrder());
			Search(weight, filter, collector2);
			return (TopFieldDocs) collector2.TopDocs();
		}
		
		public override void  Search(Weight weight, Filter filter, Collector collector)
		{
			
			if (filter == null)
			{
				for (int i = 0; i < subReaders.Length; i++)
				{
					// search each subreader
					collector.SetNextReader(subReaders[i], docStarts[i]);
					Scorer scorer = weight.Scorer(subReaders[i], !collector.AcceptsDocsOutOfOrder(), true);
					if (scorer != null)
					{
						scorer.Score(collector);
					}
				}
			}
			else
			{
				for (int i = 0; i < subReaders.Length; i++)
				{
					// search each subreader
					collector.SetNextReader(subReaders[i], docStarts[i]);
					SearchWithFilter(subReaders[i], weight, filter, collector);
				}
			}
		}
		
		private void  SearchWithFilter(IndexReader reader, Weight weight, Filter filter, Collector collector)
		{
			
			System.Diagnostics.Debug.Assert(filter != null);
			
			Scorer scorer = weight.Scorer(reader, true, false);
			if (scorer == null)
			{
				return ;
			}
			
			int docID = scorer.DocID();
			System.Diagnostics.Debug.Assert(docID == - 1 || docID == DocIdSetIterator.NO_MORE_DOCS);
			
			// CHECKME: use ConjunctionScorer here?
			DocIdSet filterDocIdSet = filter.GetDocIdSet(reader);
			if (filterDocIdSet == null)
			{
				// this means the filter does not accept any documents.
				return ;
			}
			
			DocIdSetIterator filterIter = filterDocIdSet.Iterator();
			if (filterIter == null)
			{
				// this means the filter does not accept any documents.
				return ;
			}
			int filterDoc = filterIter.NextDoc();
			int scorerDoc = scorer.Advance(filterDoc);
			
			collector.SetScorer(scorer);
			while (true)
			{
				if (scorerDoc == filterDoc)
				{
					// Check if scorer has exhausted, only before collecting.
					if (scorerDoc == DocIdSetIterator.NO_MORE_DOCS)
					{
						break;
					}
					collector.Collect(scorerDoc);
					filterDoc = filterIter.NextDoc();
					scorerDoc = scorer.Advance(filterDoc);
				}
				else if (scorerDoc > filterDoc)
				{
					filterDoc = filterIter.Advance(scorerDoc);
				}
				else
				{
					scorerDoc = scorer.Advance(filterDoc);
				}
			}
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
		
		public override Explanation Explain(Weight weight, int doc)
		{
			int n = ReaderUtil.SubIndex(doc, docStarts);
			int deBasedDoc = doc - docStarts[n];
			
			return weight.Explain(subReaders[n], deBasedDoc);
		}
		
		private bool fieldSortDoTrackScores;
		private bool fieldSortDoMaxScore;
		
		/// <summary> By default, no scores are computed when sorting by field (using
		/// {@link #Search(Query,Filter,int,Sort)}). You can change that, per
		/// IndexSearcher instance, by calling this method. Note that this will incur
		/// a CPU cost.
		/// 
		/// </summary>
		/// <param name="doTrackScores">If true, then scores are returned for every matching document
		/// in {@link TopFieldDocs}.
		/// 
		/// </param>
		/// <param name="doMaxScore">If true, then the max score for all matching docs is computed.
		/// </param>
		public virtual void  SetDefaultFieldSortScoring(bool doTrackScores, bool doMaxScore)
		{
			fieldSortDoTrackScores = doTrackScores;
			fieldSortDoMaxScore = doMaxScore;
		}

        public IndexReader reader_ForNUnit
        {
            get { return reader; }
        }
	}
}
