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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> A {@link Collector} implementation that collects the top-scoring hits,
	/// returning them as a {@link TopDocs}. This is used by {@link IndexSearcher} to
	/// implement {@link TopDocs}-based search. Hits are sorted by score descending
	/// and then (when the scores are tied) docID ascending. When you create an
	/// instance of this collector you should know in advance whether documents are
	/// going to be collected in doc Id order or not.
	/// 
	/// <p/><b>NOTE</b>: The values {@link Float#NaN} and
    /// {Float#NEGATIVE_INFINITY} are not valid scores.  This
    /// collector will not properly collect hits with such
    /// scores.
	/// </summary>
	public abstract class TopScoreDocCollector:TopDocsCollector
	{
		
		// Assumes docs are scored in order.
		private class InOrderTopScoreDocCollector:TopScoreDocCollector
		{
			internal InOrderTopScoreDocCollector(int numHits):base(numHits)
			{
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();
                
                // This collector cannot handle these scores:
                System.Diagnostics.Debug.Assert(score != float.NegativeInfinity);
                System.Diagnostics.Debug.Assert(!float.IsNaN(score));

				totalHits++;
				if (score <= pqTop.score)
				{
					// Since docs are returned in-order (i.e., increasing doc Id), a document
					// with equal score to pqTop.score cannot compete since HitQueue favors
					// documents with lower doc Ids. Therefore reject those docs too.
					return ;
				}
				pqTop.doc = doc + docBase;
				pqTop.score = score;
				pqTop = (ScoreDoc) pq.UpdateTop();
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return false;
			}
		}
		
		// Assumes docs are scored out of order.
		private class OutOfOrderTopScoreDocCollector:TopScoreDocCollector
		{
			internal OutOfOrderTopScoreDocCollector(int numHits):base(numHits)
			{
			}
			
			public override void  Collect(int doc)
			{
				float score = scorer.Score();

                // This collector cannot handle NaN
                System.Diagnostics.Debug.Assert(!float.IsNaN(score));

				totalHits++;
				doc += docBase;
				if (score < pqTop.score || (score == pqTop.score && doc > pqTop.doc))
				{
					return ;
				}
				pqTop.doc = doc;
				pqTop.score = score;
				pqTop = (ScoreDoc) pq.UpdateTop();
			}
			
			public override bool AcceptsDocsOutOfOrder()
			{
				return true;
			}
		}
		
		/// <summary> Creates a new {@link TopScoreDocCollector} given the number of hits to
		/// collect and whether documents are scored in order by the input
		/// {@link Scorer} to {@link #SetScorer(Scorer)}.
		/// 
		/// <p/><b>NOTE</b>: The instances returned by this method
		/// pre-allocate a full array of length
		/// <code>numHits</code>, and fill the array with sentinel
		/// objects.
		/// </summary>
		public static TopScoreDocCollector create(int numHits, bool docsScoredInOrder)
		{
			
			if (docsScoredInOrder)
			{
				return new InOrderTopScoreDocCollector(numHits);
			}
			else
			{
				return new OutOfOrderTopScoreDocCollector(numHits);
			}
		}
		
		internal ScoreDoc pqTop;
		internal int docBase = 0;
		internal Scorer scorer;
		
		// prevents instantiation
		private TopScoreDocCollector(int numHits):base(new HitQueue(numHits, true))
		{
			// HitQueue implements getSentinelObject to return a ScoreDoc, so we know
			// that at this point top() is already initialized.
			pqTop = (ScoreDoc) pq.Top();
		}
		
		public /*protected internal*/ override TopDocs NewTopDocs(ScoreDoc[] results, int start)
		{
			if (results == null)
			{
				return EMPTY_TOPDOCS;
			}
			
			// We need to compute maxScore in order to set it in TopDocs. If start == 0,
			// it means the largest element is already in results, use its score as
			// maxScore. Otherwise pop everything else, until the largest element is
			// extracted and use its score as maxScore.
			float maxScore = System.Single.NaN;
			if (start == 0)
			{
				maxScore = results[0].score;
			}
			else
			{
				for (int i = pq.Size(); i > 1; i--)
				{
					pq.Pop();
				}
				maxScore = ((ScoreDoc) pq.Pop()).score;
			}
			
			return new TopDocs(totalHits, results, maxScore);
		}
		
		public override void  SetNextReader(IndexReader reader, int base_Renamed)
		{
			docBase = base_Renamed;
		}
		
		public override void  SetScorer(Scorer scorer)
		{
			this.scorer = scorer;
		}
	}
}
