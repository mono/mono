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
	
	/// <summary>A {@link HitCollector} implementation that collects the top-sorting
	/// documents, returning them as a {@link TopFieldDocs}.  This is used by {@link
	/// IndexSearcher} to implement {@link TopFieldDocs}-based search.
	/// 
	/// <p/>This may be extended, overriding the collect method to, e.g.,
	/// conditionally invoke <code>super()</code> in order to filter which
	/// documents are collected.
	/// 
	/// </summary>
	/// <deprecated> Please use {@link TopFieldCollector} instead.
	/// </deprecated>
    [Obsolete("Please use TopFieldCollector instead.")]
	public class TopFieldDocCollector:TopDocCollector
	{
		
		private FieldDoc reusableFD;
		
		/// <summary>Construct to collect a given number of hits.</summary>
		/// <param name="reader">the index to be searched
		/// </param>
		/// <param name="sort">the sort criteria
		/// </param>
		/// <param name="numHits">the maximum number of hits to collect
		/// </param>
		public TopFieldDocCollector(IndexReader reader, Sort sort, int numHits):base(new FieldSortedHitQueue(reader, sort.fields, numHits))
		{
		}
		
		// javadoc inherited
		public override void  Collect(int doc, float score)
		{
			if (score > 0.0f)
			{
				totalHits++;
				if (reusableFD == null)
					reusableFD = new FieldDoc(doc, score);
				else
				{
					// Whereas TopScoreDocCollector can skip this if the
					// score is not competitive, we cannot because the
					// comparators in the FieldSortedHitQueue.lessThan
					// aren't in general congruent with "higher score
					// wins"
					reusableFD.score = score;
					reusableFD.doc = doc;
				}
				reusableFD = (FieldDoc) hq.InsertWithOverflow(reusableFD);
			}
		}
		
		// javadoc inherited
		public override TopDocs TopDocs()
		{
			FieldSortedHitQueue fshq = (FieldSortedHitQueue) hq;
			ScoreDoc[] scoreDocs = new ScoreDoc[fshq.Size()];
			for (int i = fshq.Size() - 1; i >= 0; i--)
			// put docs in array
				scoreDocs[i] = fshq.FillFields((FieldDoc) fshq.Pop());
			
			return new TopFieldDocs(totalHits, scoreDocs, fshq.GetFields(), fshq.GetMaxScore());
		}
	}
}
