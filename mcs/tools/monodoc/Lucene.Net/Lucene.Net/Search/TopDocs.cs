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

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Represents hits returned by {@link
    /// * Searcher#search(Query,Filter,int)} and {@link
    /// * Searcher#search(Query,int)
    /// </summary>
	[Serializable]
	public class TopDocs
	{
		/// <summary>The total number of hits for the query.</summary>
		/// <seealso cref="Hits.Length()">
		/// </seealso>
        [Obsolete("For backward compatibility. Use TotalHits instead")]
		public int totalHits;
		/// <summary>The top hits for the query. </summary>
        [Obsolete("For backward compatibility. Use ScoreDocs instead")]
		public ScoreDoc[] scoreDocs;
		/// <summary>Stores the maximum score value encountered, needed for normalizing. </summary>
        [Obsolete("For backward compatibility. Use MaxScore instead")]
		private float maxScore;

        public int TotalHits
        {
            get { return totalHits; }
            set { totalHits = value; }
        }

        public ScoreDoc[] ScoreDocs
        {
            get { return scoreDocs; }
            set { scoreDocs = value; }
        }

        public float MaxScore
        {
            get { return maxScore; }
            set { maxScore = value; }
        }


		/// <summary>Returns the maximum score value encountered. Note that in case
		/// scores are not tracked, this returns {@link Float#NaN}.
		/// </summary>
		public virtual float GetMaxScore()
		{
			return MaxScore;
		}
		
		/// <summary>Sets the maximum score value encountered. </summary>
		public virtual void  SetMaxScore(float maxScore)
		{
			this.MaxScore = maxScore;
		}
		
		/// <summary>Constructs a TopDocs with a default maxScore=Float.NaN. </summary>
		internal TopDocs(int totalHits, ScoreDoc[] scoreDocs):this(totalHits, scoreDocs, System.Single.NaN)
		{
		}
		
		/// <summary></summary>
		public TopDocs(int totalHits, ScoreDoc[] scoreDocs, float maxScore)
		{
			this.TotalHits = totalHits;
			this.ScoreDocs = scoreDocs;
			this.MaxScore = maxScore;
		}
	}
}
